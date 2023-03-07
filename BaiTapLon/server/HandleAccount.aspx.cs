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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace BaiTapLon.server
{
    public partial class HandleAccount : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            con.Open();
            Response.ContentType = "application/json";
            String action = Request.QueryString["action"];
            switch (action) {
                case "register":
                    Register(Request.Form);
                    break;
                case "login":
                    String userName = Request.Form["username"];
                    String password = Request.Form["password"];
                    Login(userName, password);
                    break;
                default:
                    Response.StatusCode = 400;
                    Response.Write("{\"msg\":\"Action not supported with route!\"}");
                    Response.End();
                    break;
            }

            con.Close();
        }

        public void Login(string username, string password)
        {
            string errorMsg = "Login Failed!";
            if (username != null && password != null)
            {

                SqlCommand cmd = new SqlCommand("select * from Auth where username='"+username+"'", con);

                SqlDataReader reader = cmd.ExecuteReader();

                string userID = null;

                while (reader.Read())
                {
                    Auth auth = new Auth((int)reader["ID"], reader["username"].ToString(), reader["password"].ToString(), (int)reader["userID"]);
                    if (auth.username == username && auth.checkPassword(password))
                    {
                        userID = auth.userID.ToString();
                    }
                }

                reader.Close();

                User userInfo = null;
                if (userID != null)
                {
                    cmd = new SqlCommand("select * from Users where ID=" + userID, con);

                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        userInfo = new User((int)reader["ID"], reader["name"].ToString(), reader["birthday"].ToString(), reader["sex"].ToString());
                    }

                }
                if (userInfo != null)
                {
                    Response.Write("{\"data\":" + userInfo.converString() + "}");
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

        public void Register (NameValueCollection formData)
        {
            string username = formData["username"];
            string errorMsg = null;

            SqlCommand cmd = new SqlCommand("select * from Auth where username='" + username + "'", con);

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

            cmd = new SqlCommand("INSERT INTO Users (name,birthday,sex)" +
                " OUTPUT Inserted.ID " +
                "VALUES ('" + formData["fullname"] + "','" + formData["birthday"] + "','" + formData["sex"] + "')", con);
            int newUserId = (int)cmd.ExecuteScalar();

            cmd.Cancel();

            Auth auth = new Auth(1, username, formData["password"],newUserId);

            SqlCommand cmd1 = new SqlCommand("INSERT INTO Auth (username,password,userID)" +
                " OUTPUT Inserted.ID " +
                "VALUES ('" + username + "','" + auth.password + "', " + newUserId + ")", con);

            int newAccount = (int)cmd1.ExecuteScalar();

            User userInfo = new User(newUserId, formData["fullname"], formData["birthday"], formData["sex"]);


            Response.Write("{\"data\":" + userInfo.converString().ToString() + "}");
            Response.End();
        }
    }
}