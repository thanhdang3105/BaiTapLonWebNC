using BaiTapLon.server;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace BaiTapLon.admin
{
    public partial class quanly : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString);
        StoredProcedure procedure;
        ErrorResponse errMsg;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                con.Open();
                procedure = new StoredProcedure(con);
            }catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write(ex.Message);
                Response.End();
            }
            if (Request.HttpMethod == "GET")
            {
                string pathInfo = Request.PathInfo;
                string sort = Request.QueryString["sort"];
                int limit = Convert.ToInt32(Request.QueryString["limit"]);
                int skip = Convert.ToInt32(Request.QueryString["skip"]);

                if (!String.IsNullOrEmpty(pathInfo)) { 
                    CheckPermission();
                }

                if (pathInfo == "/getData")
                {
                    getData(null,sort, limit, skip);
                }else if (pathInfo == "/getAccounts")
                {
                    getAccounts(null, sort, limit, skip);
                }
            }
            else
            {
                CheckPermission();
                switch (Request.Form["action"])
                {
                    case "create":
                        createSach(Request.Form);
                        break;
                    case "createCate":
                        createCategory(Request.Form);
                        break;
                    case "update":
                        updateSach(Request.Form);
                        break;
                    case "delete":
                        deleteSach(Request.Form);
                        break;
                    case "resetPassword":
                        resetPassword(Request.Form);
                        break;
                    case "lockAccount":
                        lockOrUnlockAccount(Request.Form,true);
                        break;
                    case "unlockAccount":
                        lockOrUnlockAccount(Request.Form, false);
                        break;
                    case "changeRoleAccount":
                        changeRoleAccount(Request.Form);
                        break;
                    default:
                        Response.ContentType = "application/json";
                        errMsg = new ErrorResponse("Request not Support!");
                        Response.Write(JsonConvert.SerializeObject(errMsg));
                        Response.End();
                        break;
                }
            }
            con.Close();
        }

        protected void CheckPermission()
        {
            string token = Session["Authorization"]?.ToString();

            if (String.IsNullOrEmpty(token))
            {
                token = Request.Headers["Authorization"];
            }

            Authorization authClient = new Authorization(token);
            string tokenInfo = authClient.decode(token);
            if (String.IsNullOrEmpty(tokenInfo))
            {
                Response.StatusCode = 401;
                Response.Write("No Permission!");
                Response.End();
            }
            else
            {
                string[] userInfo = tokenInfo.Split('-');
                string username = userInfo[0];
                string expriedAt = userInfo[1];
                string password = userInfo[2];
                if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password) || DateTime.UtcNow.CompareTo(DateTime.Parse(expriedAt)) >= 0)
                {
                    Response.StatusCode = 401;
                    Response.Write("Token is wrong or Token Expired!\n\nPlease Login again.");
                    Response.End();
                }
                else
                {
                    SqlCommand cmd = procedure.selectAuthWithEmail(username, password);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string role = reader["role"].ToString();

                        bool locked = (bool)reader["locked"];


                        if (locked == true)
                        {
                            Response.StatusCode = 401;
                            Response.Write("Account has been locked!\nplease contact admin to reopen it.");
                            Response.End();
                        }

                        if (role != "admin" && role != "system")
                        {
                            Response.StatusCode = 401;
                            Response.Write("Account is not permission!\n\nPlease change account and retry.");
                            Response.End();
                        }
                    }else
                    {
                        Response.StatusCode = 401;
                        Response.Write("Token is not access!\n\nPlease Login again.");
                        Response.End();
                    }

                    reader.Close();

                    cmd.Cancel();

                }
            }
        }

        protected void getData(string filter = null ,string sort = null ,int limit=10,int skip=0)
        {

            Response.ContentType = "application/json";
            try
            {
                if (limit == 0)
                {
                    limit = 10;
                }
                if (skip < 0)
                {
                    skip = 0;
                }

                SqlCommand cmd1 = procedure.countBooksWithFilter(filter);

                SqlCommand cmd = procedure.selectBooksWithFilter(filter, sort, limit, skip * limit);

                if (cmd == null || cmd1 == null) throw new Exception("System Error!");

                object count = cmd1.ExecuteScalar();
                SqlDataReader reader = cmd.ExecuteReader();


                List<ClassSach> lists = new List<ClassSach>();

                while (reader.Read())
                {
                    ClassSach sach = new ClassSach(Convert.ToInt32(reader["ID"]), reader["name"].ToString(), reader["author"].ToString(), reader["categoryName"].ToString(), reader["categoryKey"].ToString(), reader["content"].ToString(), reader["imgSrc"].ToString(), Convert.ToInt32(reader["like"]), Convert.ToInt32(reader["view"]));
                    lists.Add(sach);
                }

                reader.Close();
                cmd1.Cancel();
                cmd.Cancel();

                string data = JsonConvert.SerializeObject(lists);
                
                Response.Write("{\"data\": " + data + ", \"count\": " + count.ToString() + "}");
            }
            catch(Exception ex)
            {
                Response.StatusCode = 500;
                errMsg = new ErrorResponse(ex.Message);
                Response.Write(JsonConvert.SerializeObject(errMsg));
            }
            Response.End();
        }

        protected void getAccounts(string filter = null, string sort = null, int limit = 10, int skip = 0)
        {
            try
            {
                SqlCommand cmd1 = procedure.countAuth(filter);

                SqlCommand cmd = procedure.selectAuthAndUser(filter, sort, limit, skip * limit);

                if (cmd == null || cmd1 == null) throw new Exception("System Error!");

                object count = cmd1.ExecuteScalar();
                SqlDataReader reader = cmd.ExecuteReader();

                List<AuthofUser> lists = new List<AuthofUser>();

                while (reader.Read())
                {
                    AuthofUser account = new AuthofUser(Convert.ToInt32(reader["ID"]), reader["name"].ToString(), reader["email"].ToString(), reader["birthday"].ToString(), reader["phone"].ToString(), (bool)reader["sex"], reader["role"].ToString(), (bool)reader["locked"]);
                    lists.Add(account);
                }

                reader.Close();
                cmd1.Cancel();
                cmd.Cancel();


                string data = JsonConvert.SerializeObject(lists);

                Response.Write("{\"data\": " + data + ", \"count\": " + count.ToString() + "}");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;

                ErrorResponse errMsg = new ErrorResponse(ex.Message);

                Response.Write(JsonConvert.SerializeObject(errMsg));
            }
            Response.End();
        }

        protected void createCategory(NameValueCollection form)
        {
            Response.ContentType = "application/json";
            string name = form["nameCate"];
            string value = form["valueCate"];
            if(!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(value))
            {
                SqlCommand cmd = procedure.insertCategoryInfo(name, value);
                try
                {
                    if (cmd == null) throw new Exception("System Error!");
                    cmd.ExecuteNonQuery();
                    Response.StatusCode = 200;
                    Response.Write("{\"data\": \"Create Succes!\"}");
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 400;
                    Response.Write("{\"msg\": \"Create Fail!\"}");
                }
            }
            else
            {
                Response.StatusCode = 400;
                Response.Write("{\"msg\":\"Invalid Params!\"}");
            }
            Response.End();
        }

        protected void createSach(NameValueCollection form)
        {
            Response.ContentType = "application/json";
            string name = form["name"];
            string author = form["author"];
            string category = form["category"];
            string content = form["content"];
            string imgSrc = form["imgSrc"];
            if(String.IsNullOrEmpty(name) || String.IsNullOrEmpty(category)){
                Response.StatusCode = 400;
                Response.Write("{\"msg\":\"Invalid Params!\"}");
            }else
            {
                SqlCommand cmd = procedure.insertBookInfo(name, author, category, content, imgSrc); 
                try
                {
                    if (cmd == null) throw new Exception("System Error!");

                    object output = cmd.ExecuteScalar();
                   
                    //tableSach.DataBind();
                    Response.Write("{\"data\": \""+ output?.ToString()+ "\"}");
                    Response.StatusCode = 200;
                }catch (ThreadAbortException ex)
                {
                    Response.Write("{\"msg\":'"+ex.Message+"'}");
                    Response.StatusCode = 500;
                }
            }
            Response.End();
        }

        protected void updateSach(NameValueCollection form)
        {
            Response.ContentType = "application/json";
            string id = form["id"];
            string name = form["name"];
            string author = form["author"];
            string category = form["category"];
            string content = form["content"];
            string imgSrc = form["imgSrc"];
            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(name) || String.IsNullOrEmpty(category))
            {
                Response.StatusCode = 400;
                Response.Write("{\"msg\":\"Invalid Params!\"}");
            }
            else
            {
                SqlCommand cmd = procedure.updateBookInfo(Convert.ToInt32(id),name, author,category, content, imgSrc);
                try
                {

                    if (cmd == null) throw new Exception("System Error!");

                    cmd.ExecuteNonQuery();

                    Response.Write("{\"data\": \"Update Succes!\"}");
                    Response.StatusCode = 200;
                }
                catch (ThreadAbortException ex)
                {
                    Response.Write("{\"msg\":\"" + ex.Message + "\"}");
                    Response.StatusCode = 500;
                }
            }
            Response.End();
        }

        protected void deleteSach(NameValueCollection form)
        {
            Response.ContentType = "application/json";
            string id = form["id"];
            if (String.IsNullOrEmpty(id))
            {
                Response.StatusCode = 400;
                Response.Write("{\"msg\":\"Invalid Params!\"}");
            }
            else
            {
                SqlCommand cmd = procedure.deleteRowWithID(Convert.ToInt32(id), "Books"); 
                    
                try
                {
                    if (cmd == null) throw new Exception("System Error!");
                    cmd.ExecuteNonQuery();

                    Response.Write("{\"data\": \"Delete Succes!\"}");
                    Response.StatusCode = 200;
                }
                catch (ThreadAbortException ex)
                {
                    Response.Write("{\"msg\":'" + ex.Message + "'}");
                    Response.StatusCode = 500;
                }
            }
            Response.End();
        }

        protected void resetPassword(NameValueCollection form)
        {
            ErrorResponse errMsg;
            try
            {
                string id = form["ID"];

                if (id == null) throw new Exception("Invalid ID!");

                int ID = Convert.ToInt32(id);

                SqlCommand cmd = procedure.selectAuthWithID(ID);

                SqlDataReader reader = cmd.ExecuteReader();

                List<Auth> list = new List<Auth>();

                while (reader.Read())
                {
                    Auth account = new Auth((int)reader["ID"], reader["username"].ToString(), (int)reader["userId"], reader["role"].ToString(), Convert.ToBoolean(reader["locked"])); 
                    list.Add(account);
                }

                reader.Close();
                cmd.Cancel();

                if(list.Count < 0) {
                    errMsg = new ErrorResponse("Account (ID="+ ID + ") not found!");
                    Response.Write(JsonConvert.SerializeObject(errMsg));
                }else if(list.Count > 1) {
                    errMsg = new ErrorResponse("Account (ID=" + ID + ") is duplicated, please contact admin check!!");
                    Response.Write(JsonConvert.SerializeObject(errMsg));
                }else
                {
                    Auth account = (Auth)list[0];
                    string random = DateTime.Now.ToFileTimeUtc().ToString();
                    string newPwd = random.Substring(random.Length - 6, 6);
                    Authorization authAccount = new Authorization(newPwd);
                    cmd = procedure.updateAuthPassword(ID, authAccount.textEncrypted);

                    try
                    {
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {

                            string bodyMail = "<h1>Tài khoản của bạn tại websach đã được thay đổi</h1></br></br>" +
                                "<h3>Mật khẩu mới của bạn là: <strong>"+newPwd+"</strong></h3></br></br>" +
                                "Đến trang của chúng tôi -> <a href='https://localhost:44398/view/HomePage.html'>Web sách</a>" +
                                "<h2>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi</h1>";

                            string status = sendEmail(account.username, bodyMail);

                            if(status == "Success")
                            {
                                Response.Write("Password is reset Successfuly!\nNew password is send your mail!");
                            }else
                            {
                                Response.Write("Password is reset Successfuly!\nBut send password to mail is faulty, please check or try again!");
                            }
                        }
                        else
                        {
                            Response.Write("Reset password failed!!");
                        }
                    }
                    catch (Exception ex)
                    {
                        errMsg = new ErrorResponse(ex.Message);
                        Response.Write(JsonConvert.SerializeObject(errMsg));
                    }
                }

            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                errMsg = new ErrorResponse(ex.Message);
                Response.Write(JsonConvert.SerializeObject(errMsg));
            }

            Response.End();
        }

        protected void lockOrUnlockAccount(NameValueCollection form, bool isLock)
        {
            ErrorResponse errMsg;
            Response.ContentType = "application/json";
            try
            {
                string id = form["ID"];

                if (id == null) throw new Exception("Invalid ID!");

                int ID = Convert.ToInt32(id);

                SqlCommand cmd = procedure.selectAuthWithID(ID);

                SqlDataReader reader = cmd.ExecuteReader();

                List<Auth> list = new List<Auth>();

                while (reader.Read())
                {
                    Auth account = new Auth((int)reader["ID"], reader["username"].ToString(), (int)reader["userId"], reader["role"].ToString(), Convert.ToBoolean(reader["locked"]));
                    list.Add(account);
                }

                reader.Close();
                cmd.Cancel();

                if (list.Count < 0)
                {
                    errMsg = new ErrorResponse("Account (ID=" + ID + ") not found!");
                    Response.Write(JsonConvert.SerializeObject(errMsg));
                }
                else if (list.Count > 1)
                {
                    errMsg = new ErrorResponse("Account (ID=" + ID + ") is duplicated, please contact admin check!!");
                    Response.Write(JsonConvert.SerializeObject(errMsg));
                }
                else
                {
                    Auth account = (Auth)list[0];
                    
                    if(account.role == "system")
                    {
                        Response.StatusCode = 400;
                        Response.Write("Action failed!\nNot Permission!");
                    }else
                    {
                        try
                        {
                            cmd = procedure.updateAuthLocked(ID, isLock);

                            if (cmd == null) throw new Exception("System Error!");

                            object result = cmd.ExecuteScalar();

                            if (result != null)
                            {
                                string bodyMail;
                                string subject;
                                if (isLock)
                                {
                                    subject = "Account is locked";
                                    bodyMail = "<h1>Tài khoản của bạn tại websach đã bị khóa do vi phạm</h1></br></br>" +
                                    "<h3>Vui lòng liên hệ với quản trị viên để được hỗ trợ</strong></h3></br></br>" +
                                    "Đến trang của chúng tôi -> <a href='https://localhost:44398/view/HomePage.html'>Web sách</a>" +
                                    "<h1>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi</h1>";
                                }
                                else
                                {
                                    subject = "Account is unlocked";
                                    bodyMail = "<h1>Tài khoản của bạn tại websach đã được mở khóa</h1></br></br>" +
                                    "<h3>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi</h3></br></br>" +
                                    "Đến trang của chúng tôi -> <a href='https://localhost:44398/view/HomePage.html'>Web sách</a>";
                                }

                                string status = sendEmail(account.username, bodyMail, subject);

                                if (status == "Success")
                                {
                                    Response.Write("Mail is sended to user!");
                                }
                                else
                                {
                                    Response.Write("Acction done!\nBut send notification to mail is faulty, please check or try again!");
                                }
                            }
                            else
                            {
                                Response.StatusCode = 400;
                                Response.Write("Action failed!!");
                            }
                        }
                        catch (Exception ex)
                        {
                            Response.StatusCode = 500;
                            errMsg = new ErrorResponse(ex.Message);
                            Response.Write(JsonConvert.SerializeObject(errMsg));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                errMsg = new ErrorResponse(ex.Message);
                Response.Write(JsonConvert.SerializeObject(errMsg));
            }

            Response.End();
        }

        protected void changeRoleAccount(NameValueCollection form)
        {
            ErrorResponse errMsg;
            try
            {
                string id = form["ID"];
                string role = form["value"];

                if (id == null) throw new Exception("Invalid ID!");

                int ID = Convert.ToInt32(id);

                SqlCommand cmd = procedure.selectAuthWithID(ID);

                SqlDataReader reader = cmd.ExecuteReader();

                List<Auth> list = new List<Auth>();

                while (reader.Read())
                {
                    Auth account = new Auth((int)reader["ID"], reader["username"].ToString(), (int)reader["userId"], reader["role"].ToString(), Convert.ToBoolean(reader["locked"]));
                    list.Add(account);
                }

                reader.Close();
                cmd.Cancel();

                if (list.Count < 0)
                {
                    errMsg = new ErrorResponse("Account (ID=" + ID + ") not found!");
                    Response.Write(JsonConvert.SerializeObject(errMsg));
                }
                else if (list.Count > 1)
                {
                    errMsg = new ErrorResponse("Account (ID=" + ID + ") is duplicated, please contact admin check!!");
                    Response.Write(JsonConvert.SerializeObject(errMsg));
                }
                else
                {
                    Auth account = (Auth)list[0];

                    if (account.role == "system")
                    {
                        Response.StatusCode = 400;
                        Response.Write("Action failed!\nNot Permission!");
                    }else
                    {
                        try
                        {
                            cmd = procedure.updateAuthRole(ID, role);

                            if (cmd == null) throw new Exception("System Error");

                            object result = cmd.ExecuteScalar();

                            if (result != null)
                            {
                                string bodyMail;

                                if (role == "admin")
                                {
                                    bodyMail = "<h1>Tài khoản của bạn tại websach đã được cấp quyền quản trị viên</h1></br></br>" +
                                    "<h3>Hãy đăng nhập và sử dụng quyền lợi của quản trị viên.</strong></h3></br></br>" +
                                    "Đến trang quản trị -> <a href='https://localhost:44398/admin/quanly.aspx'>Web sách</a>" +
                                    "<h1>Cảm ơn bạn đã đồng hành cùng chúng tôi</h1>";
                                }
                                else
                                {
                                    bodyMail = "<h1>Tài khoản của bạn tại websach đã bị tước quyền quản trị viên</h1></br></br>" +
                                    "<h3>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi</h3></br></br>" +
                                    "Đến trang của chúng tôi -> <a href='https://localhost:44398/view/HomePage.html'>Web sách</a>";
                                }

                                string status = sendEmail(account.username, bodyMail, "Change role Account");

                                if (status == "Success")
                                {
                                    Response.Write("Mail is sended to user!");
                                }
                                else
                                {
                                    Response.Write("Acction done!\nBut send notification to mail is faulty, please check or try again!");
                                }
                            }
                            else
                            {
                                Response.Write("Action failed!!");
                            }
                        }
                        catch (Exception ex)
                        {
                            errMsg = new ErrorResponse(ex.Message);
                            Response.Write(JsonConvert.SerializeObject(errMsg));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                errMsg = new ErrorResponse(ex.Message);
                Response.Write(JsonConvert.SerializeObject(errMsg));
            }

            Response.End();
        }

        protected string sendEmail(string sendTo, string body, string subject = "Reset Password")
        {

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);

            smtpClient.Credentials = new System.Net.NetworkCredential("myshopstore31@gmail.com", "jfdoifyqymizmzbj");
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;

            MailMessage mail = new MailMessage();

            //Setting From , To and CC
            mail.From = new MailAddress("myshopstore31@gmail.com", "MyWeb Site");
            mail.To.Add(new MailAddress(sendTo));
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = body;
            try
            {
                smtpClient.Send(mail);

                return "Success";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}