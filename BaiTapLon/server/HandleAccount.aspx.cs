using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
            String action = Request.QueryString["action"];
            switch (action) {
                case "register":

                    break;
                case "login":
                    String userName = Request.Form["username"];
                    String password = Request.Form["password"];
                    Login(userName, password);
                    break;
                default:
                    Response.ContentType = "application/json";
                    Response.StatusCode = 400;
                    Response.Write("{\"msg\":\"Action not supported with route!\"}");
                    Response.End();
                    break;
            }

            con.Close();
        }

        public class user
        {
            public int ID { get; set; }
            public string username { get; set; }
            public string birthday { get; set; }
            public string sex { get; set; }
            public user(int id, string userName, string birthDay, string Sex)
            {
                this.ID = id;
                this.username = userName;
                this.birthday = birthDay;
                this.sex = Sex;
            }
            public string converString ()
            {
                return "{\"id\":\"" + this.ID + "\", \"username\":\"" +this.username +"\",\"birthDay\":\""+this.birthday+"\"" +
                    ",\"sex\":\""+this.sex+"\"}";
            }
        }

        private void Login(string username, string password)
        {
            Response.ContentType = "application/json";
            string errorMsg = "Login Failed!";
            if (username != null && password != null)
            {
                string encode = encodePassword(password);

                string decode = dencodePassword(encode);

                SqlCommand cmd = new SqlCommand("select * from Auth where username='"+username+"'", con);

                SqlDataReader reader = cmd.ExecuteReader();

                string userID = null;



                while (reader.Read())
                {
                    if (reader[1].ToString() == username && reader[2].ToString() == password)
                    {
                        userID = reader[3].ToString();
                    }
                }

                reader.Close();

                user userInfo = null;
                if (userID != null)
                {
                    cmd = new SqlCommand("select * from Users where ID=" + userID, con);

                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        userInfo = new user((int)reader["ID"], reader["name"].ToString(), reader["birthday"].ToString(), reader["sex"].ToString());
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

        private string encodePassword(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);

            string encoded = Convert.ToBase64String(bytes);
            return encoded;
        }
        private string dencodePassword(string passwordEncoded)
        {
            byte[] bytes = Convert.FromBase64String(passwordEncoded);

            string decoded = Encoding.UTF8.GetString(bytes);
            return decoded;
        }
    }
}