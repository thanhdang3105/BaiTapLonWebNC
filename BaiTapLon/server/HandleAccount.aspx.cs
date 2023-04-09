using BaiTapLon.server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Security;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace BaiTapLon.server
{
    public partial class HandleAccount : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString);
        StoredProcedure procedure = null;
        protected void Page_Load(object sender, EventArgs e)
        {

            Response.ContentType = "application/json";
            
            try
            {
                con.Open();
                procedure = new StoredProcedure(con);
            }catch(Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write("{\"msg\": \""+ex.Message.ToString()+"\" }");
                Response.End();
            }
                object test = ViewState["thanh@gmail.com"];


                string action = Request.QueryString["action"];
                switch (action)
                {
                    case "register":
                        Register(Request.Form);
                        Response.End();
                        break;
                    case "login":
                        String userName = Request.Form["username"];
                        String password = Request.Form["password"];
                        Login(userName, password);
                        Response.End();
                        break;
                    case "checkToken":
                        ReLogin(Request.InputStream);
                        Response.End();
                        break;
                    case "forgotPassword":
                        forgotPassword(Request);
                        Response.End();
                        break;
                    case "updateUser":
                        updateUser(Request.Form);
                        Response.End();
                        break;
                    default:
                        Response.StatusCode = 400;
                        Response.Write("{\"msg\":\"Action not supported with route!\"}");
                        Response.End();
                        break;
                }

            con.Close();
        }

        protected void ReLogin(Stream body)
        {
            string errorMsg = "Token expired!";
            try
            {
                if(procedure == null) throw new Exception("System Error!");
                Encoding encoding = Request.ContentEncoding;
                StreamReader readerStream = new StreamReader(body, encoding);
                string token = readerStream.ReadToEnd();
                Authorization auth = new Authorization(token);
                string decode = auth.decode(token);
                string[] values = decode.Split('-');
                if (DateTime.UtcNow.CompareTo(DateTime.Parse(values[1])) <= 0)
                {
                    string username = values[0];
                    string password = values[2];
                    SqlCommand cmd = procedure.selectAuthWithEmail(username, password);

                    if (cmd == null) throw new Exception("System Error!");

                    SqlDataReader reader = cmd.ExecuteReader();

                    string userID = null;

                    int locked = 0;

                    bool loop = reader.Read();

                    while (loop)
                    {
                        if (String.IsNullOrEmpty(userID) && !String.IsNullOrEmpty(reader["userID"].ToString()))
                        {
                            userID = reader["userID"].ToString();
                            loop = false;
                            locked = Convert.ToInt32(reader["locked"]);
                        }
                        else
                        {
                            loop = reader.Read();
                        }
                    }

                    if(locked != 0)
                    {
                        Response.StatusCode = 401;
                        Response.Write("{\"msg\":\"Account has been locked!\nplease contact admin to reopen it\"}");
                        return;
                    }

                    reader.Close();

                    User userInfo = null;
                    if (userID != null)
                    {
                        cmd = procedure.selectUserWithID(Convert.ToInt32(userID));

                        if (cmd == null) throw new Exception("System Error!");


                        reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            userInfo = new User((int)reader["ID"], reader["name"].ToString(), reader["email"].ToString(), reader["birthday"].ToString(), (bool)reader["sex"], reader["phone"].ToString());
                        }
                    }
                    else
                    {
                        errorMsg = "User name not Found!";
                    }


                    reader.Close();

                    if (userInfo != null)
                    {
                        Session["Authorization"] = token;
                        Response.Write("{\"data\":" + JsonConvert.SerializeObject(userInfo) + "}");
                        return;
                    }

                }
                Response.StatusCode = 400;
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                errorMsg = ex.Message;
            }
            Response.Write("{\"msg\":\"" + errorMsg + "\"}");
            Response.End();
        }

        protected void Login(string username, string password)
        {
            string errorMsg = "Login Failed!";

            try
            {
                DateTime dt = Convert.ToDateTime(Session["lockExpriedAt"]);
                int time = DateTime.Now.CompareTo(dt);
                if (time < 0)
                {
                    errorMsg = "Bạn nhập sai quá nhiều vui lòng quay lại sau: " + dt.ToLongTimeString() + "!";
                }                           
                else if (username != null && password != null)
                {

                    Authorization BasicAuth = new Authorization(password);

                    SqlCommand cmd = procedure.selectAuthWithEmail(username, BasicAuth.textEncrypted);

                    if (cmd == null) throw new Exception("System Error!");

                    //SqlCommand cmd = new SqlCommand("select * from Auth where username='" + username + "' and password='"+ BasicAuth.textEncrypted + "' ORDER BY ID DESC", con);

                    SqlDataReader reader = cmd.ExecuteReader();

                    string userID = null;

                    int locked = 0;


                    bool loop = reader.Read();

                    while (loop)
                    {
                        if (String.IsNullOrEmpty(userID) && !String.IsNullOrEmpty(reader["userID"].ToString()))
                        {
                            userID = reader["userID"].ToString();
                            loop = false;
                            locked = Convert.ToInt32(reader["locked"]);
                        }
                        else
                        {
                            loop = reader.Read();
                        }
                    }

                    if (locked != 0)
                    {
                        Response.StatusCode = 400;
                        Response.Write("Account has been locked!\\nplease contact admin to reopen it.");
                        return;
                    }

                    reader.Close();

                    User userInfo = null;
                    if (userID != null)
                    {
                        cmd = procedure.selectUserWithID(Convert.ToInt32(userID));
                        //cmd = new SqlCommand("select * from Users where ID=" + userID + " ORDER BY ID DESC", con);

                        reader = cmd.ExecuteReader();

                        loop = reader.Read();

                        while (loop)
                        {
                            if (userInfo == null && !String.IsNullOrEmpty(reader["ID"].ToString()))
                            {
                                userInfo = new User((int)reader["ID"], reader["name"].ToString(), reader["email"].ToString(), reader["birthday"].ToString(), (bool)reader["sex"], reader["phone"].ToString());
                                loop = false;
                            }
                            else
                            {
                                loop = reader.Read();
                            }
                        }

                    }
                    if (userInfo != null)
                    {

                        string token = BasicAuth.generateToken(username, BasicAuth.textEncrypted);
                        Session["Authorization"] = token;
                        Response.Write("{\"data\":" + JsonConvert.SerializeObject(userInfo) + ", \"token\": \"" + token + "\"}");
                        return;
                    }
                    else
                    {
                        int count = Convert.ToInt32(Session["CountLoginFail"]) + 1;
                        int maxCountLogin = Convert.ToInt32(ConfigurationManager.AppSettings.Get("maxCountLogin"));
                        int timeLocked = Convert.ToInt32(ConfigurationManager.AppSettings.Get("timeLocked"));
                        if (count > maxCountLogin)
                        {
                            Session["lockExpriedAt"] = DateTime.Now.AddMinutes(timeLocked);
                        }
                        Session["CountLoginFail"] = count + 1;
                        errorMsg = "Sai tên đăng nhập hoặc mật khẩu!";
                    }
                }
                Response.StatusCode = 400;
            }catch (Exception ex)
            {
                Response.StatusCode = 500;
                errorMsg = ex.Message;
            }
            Response.Write(errorMsg);
            Response.End();
        }

        protected void Register (NameValueCollection formData)
        {
            string errorMsg = null;
            try
            {
                string username = formData["username"];
                string fullname = formData["fullname"];
                string birthday = formData["birthday"];
                string phone = formData["phone"];
                string sex = formData["sex"];


                SqlCommand cmd = procedure.selectAuthWithEmail(username);

                if (cmd == null) throw new Exception("System Error!");

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {

                    if (reader["username"].ToString() == username)
                    {
                        errorMsg = username + " Account is existed!";
                    }
                }

                if (errorMsg != null)
                {
                    Response.StatusCode = 400;
                    Response.Write("{\"msg\":\"" + errorMsg + "\"}");
                    return;
                }

                reader.Close();

                cmd.Cancel();


                cmd = procedure.insertUserInfo(fullname, username, birthday, phone, sex);

                if (cmd == null) throw new Exception("System Error!");

                int newUserId = (int)cmd.ExecuteScalar();

                cmd.Cancel();

                Authorization password = new Authorization(formData["password"]);


                SqlCommand cmd1 = procedure.insertAuthInfo(username, password.textEncrypted, newUserId);

                if (cmd1 == null) throw new Exception("System Error!");

                cmd1.ExecuteNonQuery();

                User userInfo = new User(newUserId, fullname, username, birthday, sex, phone);

                string token = password.generateToken(username, password.textEncrypted);
                Session["Authorization"] = token;
                Response.Write("{\"data\":" + JsonConvert.SerializeObject(userInfo) + ", \"token\": \"" + token + "\"}");
            }catch (Exception ex)
            {
                Response.StatusCode = 500;
                errorMsg = ex.Message;
                Response.Write("{\"msg\":\"" + errorMsg + "\"}");
            }
            Response.End();
        }

        protected void forgotPassword(HttpRequest body)
        {
            string sendTo = body.Form["username"];
            string password = body.Form["password"];
            if (sendTo != null && body.Form["OTP"] == null && password == null)
            {
                SqlCommand cmd = procedure.selectAuthWithEmail(sendTo);

                if(cmd == null)
                {
                    Response.StatusCode = 500;
                    Response.Write("System Error!");
                    Response.End();
                }

                object scalar = cmd.ExecuteScalar();

                if(scalar != null)
                {
                    string message = sendEmail(sendTo);
                    if (message == "Send OTP to Mail")
                    {
                        Response.Write("Send OTP to Mail");
                    }else
                    {
                        Response.StatusCode = 500;
                        Response.Write(message);
                    }
                }
                else
                {
                    Response.StatusCode = 400;
                    Response.Write("Email not found!");
                }
                Response.End();
            } else if (sendTo != null && body.Form["OTP"] != null && password == null)
            {
                string OTP = Session[sendTo]?.ToString();
                if (OTP == body.Form["OTP"])
                {
                    Session.Remove(sendTo);
                    Response.Write("OTP isCorrect!");
                    Response.End();
                }
            } else if (sendTo != null && password != null && body.Form["password-check"] != null)
            {
                Authorization auth = new Authorization(password);
                SqlCommand cmd = procedure.selectAuthWithEmail(sendTo);

                if (cmd == null)
                {
                    Response.StatusCode = 500;
                    Response.Write("System Error!");
                }

                object ID = cmd.ExecuteScalar();

                if (ID != null)
                {
                    cmd.Cancel();
                    cmd = procedure.updateAuthPassword((int)ID, auth.textEncrypted);
                    try
                    {
                        cmd.ExecuteNonQuery();
                        Response.Write("Password is reset Successfuly!");
                    }
                    catch (Exception ex)
                    {
                        Response.Write(ex.Message);
                    }
                }
                else
                {
                    Response.StatusCode = 400;
                    Response.Write("Email not found!");
                }
                Response.End();
            }
            
        }

        protected void updateUser(NameValueCollection form)
        {
            ErrorResponse errMsg;
            try
            {
                string id = form["id"];
                string name = form["name"];
                string email = form["email"];
                string birthday = form["birthday"];
                string phone = form["phone"];
                string sex = form["sex"];
                string oldPassword = form["oldPassword"];
                string newPassword = form["newPassword"];

                SqlCommand cmd;
                if (!String.IsNullOrEmpty(oldPassword) && !String.IsNullOrEmpty(newPassword) && !String.IsNullOrEmpty(email))
                {
                    Authorization auth = new Authorization(oldPassword);
                    cmd = procedure.selectAuthWithEmail(email, auth.textEncrypted);
                    if (cmd == null)
                    {
                        throw new Exception("System Error!");
                    }
                    object result = cmd.ExecuteScalar();
                    cmd.Cancel();

                    if (result != null)
                    {
                        auth = new Authorization(newPassword);
                        cmd = procedure.updateAuthPassword((int)result, auth.textEncrypted);

                        cmd.ExecuteNonQuery();
                        string token = auth.generateToken(email, auth.textEncrypted);
                        Session["Authorization"] = token;
                        Response.Write("{\"data\": {\"token\": \""+ token + "\", \"msg\": \"Password is update success!\"}}");
                        cmd.Cancel();
                    }
                    else
                    {
                        Response.StatusCode = 400;
                        errMsg = new ErrorResponse("Password is update failed!");
                        Response.Write(JsonConvert.SerializeObject(errMsg));
                    }
                }else
                {
                    if(String.IsNullOrEmpty(id) || Convert.ToInt32(id) == 0 || String.IsNullOrEmpty(name) || String.IsNullOrEmpty(birthday) || String.IsNullOrEmpty(phone) || String.IsNullOrEmpty(sex))
                    {
                        Response.StatusCode = 400;
                        errMsg = new ErrorResponse("Invalid Params!");
                        Response.Write(JsonConvert.SerializeObject(errMsg));
                    }else
                    {
                        cmd = procedure.updateUserInfo(Convert.ToInt32(id),name,birthday,phone,sex);

                        if (cmd == null) throw new Exception("System Error!");

                        object result = cmd.ExecuteScalar();
                        cmd.Cancel();
                        if(result != null)
                        {
                            Response.Write("Update succes!");
                        }else
                        {
                            Response.StatusCode = 400;
                            errMsg = new ErrorResponse("Update failed!");
                            Response.Write(JsonConvert.SerializeObject(errMsg));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = new ErrorResponse(ex.Message);
                Response.StatusCode = 500;
                Response.Write(JsonConvert.SerializeObject(errMsg));
            }
            Response.End();
        }

        protected string sendEmail(string sendTo, string otp = null)
        {
            string text = DateTime.Now.ToFileTimeUtc().ToString();
            if(otp == null)
            {
                otp = text.Substring(text.Length - 6, 6);
            }

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);

            smtpClient.Credentials = new System.Net.NetworkCredential("myshopstore31@gmail.com", "jfdoifyqymizmzbj");
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;

            MailMessage mail = new MailMessage();

            //Setting From , To and CC
            mail.From = new MailAddress("myshopstore31@gmail.com", "MyWeb Site");
            mail.To.Add(new MailAddress(sendTo));
            mail.Subject = "Reset Password";
            mail.IsBodyHtml = true;
            mail.Body = "<h1>Mã OTP của bạn là:</h1> \n" + otp +
                "\nCảm ơn bạn đá quan tâm đến chúng tôi";
            try
            {
                smtpClient.Send(mail);

                Session[sendTo] = otp;


                return "Send OTP to Mail";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}