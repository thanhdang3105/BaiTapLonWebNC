using BaiTapLon.server;
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
using System.Web;
using System.Web.Http.Controllers;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace BaiTapLon.server
{
    public partial class HandleAccount : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            con.Open();

            Response.ContentType = "application/json";

            object test = ViewState["thanh@gmail.com"];


            string action = Request.QueryString["action"];
            switch (action) {
                case "register":
                    Register(Request.Form);
                    break;
                case "login":
                    String userName = Request.Form["username"];
                    String password = Request.Form["password"];
                    Login(userName, password);
                    break;
                case "checkToken":
                    ReLogin(Request.InputStream);
                    break;
                case "forgotPassword":
                    sendEmail(Request);
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
            Encoding encoding = Request.ContentEncoding;
            StreamReader readerStream = new StreamReader(body, encoding);
            string token = readerStream.ReadToEnd();
            Authorization auth = new Authorization(token);
            string decode = auth.decode(token);
            string[] values = decode.Split('-');
            int check = DateTime.UtcNow.CompareTo(DateTime.Parse(values[1]));
            if (DateTime.UtcNow.CompareTo(DateTime.Parse(values[1])) <= 0)
            {
                string username = values[0];
                SqlCommand cmd = new SqlCommand("select * from newAuth where username='" + username + "'", con);

                SqlDataReader reader = cmd.ExecuteReader();

                string userID = null;

                while (reader.Read())
                {
                    userID = reader["userID"].ToString();
                }

                reader.Close();
                
                User userInfo = null;
                if (userID != null)
                {
                    cmd = new SqlCommand("select * from newUsers where ID=" + userID, con);

                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        userInfo = new User((int)reader["ID"], reader["name"].ToString(), reader["email"].ToString(), reader["birthday"].ToString(), reader["sex"].ToString(), reader["phone"].ToString());
                    }
                }
                else
                {
                    errorMsg = "User name not Found!";
                }
                
                reader.Close();

                if (userInfo != null)
                {
                    Response.Write("{\"data\":" + userInfo.converString() + "}");
                    Response.End();
                }

            }
            Response.StatusCode = 400;
            Response.Write("{\"msg\":\"" + errorMsg + "\"}");
            Response.End();
        }

        protected void Login(string username, string password)
        {
            string errorMsg = "Login Failed!";
            if (username != null && password != null)
            {

                SqlCommand cmd = new SqlCommand("select * from newAuth where username='" + username + "'", con);

                SqlDataReader reader = cmd.ExecuteReader();

                string userID = null;

                Authorization BasicAuth = new Authorization(password);

                while (reader.Read())
                {
                    Auth auth = new Auth((int)reader["ID"], reader["username"].ToString(), reader["password"].ToString(), (int)reader["userID"], (DateTime)reader["createdAt"]);
                    if (auth.username == username && auth.password == BasicAuth.textEncrypted)
                    {
                        userID = auth.userID.ToString();
                    }
                }

                reader.Close();

                User userInfo = null;
                if (userID != null)
                {
                    cmd = new SqlCommand("select * from newUsers where ID=" + userID, con);

                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        userInfo = new User((int)reader["ID"], reader["name"].ToString(), reader["email"].ToString(), reader["birthday"].ToString(), reader["sex"].ToString(), reader["phone"].ToString());
                    }

                }
                if (userInfo != null)
                {

                    string token = BasicAuth.generateToken(username);
                    
                    Response.Write("{\"data\":" + userInfo.converString() + ", \"token\": \""+token+"\"}");
                    Response.End();
                }
                else
                {
                    errorMsg = "Sai tên đăng nhập hoặc mật khẩu!";
                }
            }
            Response.StatusCode = 400;
            Response.Write("{\"msg\":\""+ errorMsg +"\"}");
            Response.End();
        }

        protected void Register (NameValueCollection formData)
        {
            string username = formData["username"];
            string errorMsg = null;


            SqlCommand cmd = new SqlCommand("select * from newAuth where username='" + username + "'", con);

            SqlDataReader reader = cmd.ExecuteReader();

            while(reader.Read())
            {

                if (reader["username"].ToString() == username)
                {
                    errorMsg = username + " Account is existed!";
                }
            }

            if(errorMsg != null)
            {
                Response.StatusCode = 400;
                Response.Write("{\"msg\":\"" + errorMsg + "\"}");
                Response.End();
            }

            reader.Close(); 

            cmd.Cancel();

            cmd = new SqlCommand("INSERT INTO newUsers (name,email,birthday,phone,sex)" +
                " OUTPUT Inserted.ID " +
                "VALUES ('" + formData["fullname"] + "','" + username + "','" + formData["birthday"] + "','" + formData["phone"] + "','" + formData["sex"] + "')", con);
            int newUserId = (int)cmd.ExecuteScalar();

            cmd.Cancel();

            Authorization password = new Authorization(formData["password"]);
            

            Auth auth = new Auth(1, username, password.textEncrypted, newUserId, DateTime.Now);

            SqlCommand cmd1 = new SqlCommand("INSERT INTO newAuth (username,password,userID)" +
                "VALUES ('" + username + "','" + password.textEncrypted + "', " + newUserId + ")", con);

            cmd1.ExecuteNonQuery();

            User userInfo = new User(newUserId, formData["fullname"], username, formData["birthday"], formData["sex"], formData["phone"]);

            string token = password.generateToken(username);

            Response.Write("{\"data\":" + userInfo.converString() + ", \"token\": \"" + token + "\"}");
            Response.End();
        }

        protected void sendEmail(HttpRequest body)
        {
            string sendTo = body.Form["username"];
            string password = body.Form["password"];
            if (sendTo != null && body.Form["OTP"] == null && password == null)
            {
                string text = DateTime.Now.ToFileTimeUtc().ToString();
                string otp = text.Substring(text.Length - 6, 6);

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);

                smtpClient.Credentials = new System.Net.NetworkCredential("myshopstore31@gmail.com", "jfdoifyqymizmzbj");
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.EnableSsl = true;

                MailMessage mail = new MailMessage();

                //Setting From , To and CC
                mail.From = new MailAddress("myshopstore31@gmail.com", "MyWeb Site");
                mail.To.Add(new MailAddress("thanhls1235ls12@gmail.com"));
                mail.Subject = "Reset Password";
                mail.Body = "<h1>Mã OTP của bạn là:</h1> \n" + otp +
                    "\nCảm ơn bạn đá quan tâm đến chúng tôi";
                //mail.CC.Add(new MailAddress("MyEmailID@gmail.com"));
                try
                {
                    smtpClient.Send(mail);

                    Session[sendTo] = otp;

                    Response.Write("Semd OTP to Mail");
                    Response.End();

                }
                catch (Exception ex)
                {
                    throw ex;
                }
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
                SqlCommand cmd = new SqlCommand("update newAuth set password = '"+auth.textEncrypted+"' where username='"+sendTo+"'",con);
                try
                {
                    cmd.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Response.Write(ex.Message);
                    Response.End();
                }
                Response.Write("Password is reset Successfuly!");
                Response.End();
            }
            
        }
    }
}