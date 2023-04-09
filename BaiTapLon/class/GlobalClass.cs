using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;

namespace BaiTapLon.server {

    class ErrorResponse
    {
        public string msg { get; set; }
        public ErrorResponse(string errorMsg)
        {
            this.msg = errorMsg;
        }
    }
    class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string birthday { get; set; }
        public string phone { get; set; }
        public string sex { get; set; }

        public User(int id, string Name, string Email, string Birthday, string sex, string Phone)
        {
            this.id = id;
            this.name = Name;
            this.email = Email;
            this.birthday = Birthday;
            this.sex = sex;
            this.phone = Phone;
        }
        public User(int id, string Name, string Email, string Birthday, bool sex, string Phone)
        {
            this.id = id;
            this.name = Name;
            this.email = Email;
            this.birthday = Birthday;
            if (sex)
            {
                this.sex = "male";
            } else
            {
                this.sex = "female";
            }
            this.phone = Phone;
        }
        public string converString()
        {
            string data = "{\"id\":\"" + this.id + "\", " +
                "\"name\":\"" + this.name + "\"," +
                "\"email\":\"" + this.email + "\"," +
                "\"birthday\":\"" + this.birthday + "\"," +
                "\"phone\":\"" + this.phone + "\"," +
                "\"sex\":\"" + this.sex + "\"}";
            return data.ToString();
        }

    }

    class Auth
    {
        public int id { get; set; }
        public string username { get; set; }
        [JsonIgnore]
        public string password { get; set; }
        public int userID { get; set; }

        public string role { get; set; }

        public bool locked { get; set; }
        public SqlDateTime createdAt { get; set; }

        public SqlDateTime updatedAt { get; set; }

        public Auth(int id, string UserName, int Userid, string role = "user", bool locked = false)
        {
            this.id = id;
            this.username = UserName;
            this.userID = Userid;
            this.role = role;
            this.locked = locked;
        }

        public Auth(int id, string UserName, string Password, int Userid, string role = "user", bool locked = false)
        {
            this.id = id;
            this.username = UserName;
            this.password = Password;
            this.userID = Userid;
            this.role = role;
            this.locked = locked;
        }
        public Auth(int id, string UserName, string Password, int Userid, SqlDateTime createdAt, string role = "user", bool locked = false)
        {
            this.id = id;
            this.username = UserName;
            this.password = Password;
            this.userID = Userid;
            this.createdAt = createdAt;
            this.role = role;
            this.locked = locked;
        }
        public Auth(int id, string UserName, string Password, int Userid, SqlDateTime createdAt, SqlDateTime updatedAt, string role = "user", bool locked = false)
        {
            this.id = id;
            this.username = UserName;
            this.password = Password;
            this.userID = Userid;
            this.createdAt = createdAt;
            this.updatedAt = updatedAt;
            this.role = role;
            this.locked = locked;
        }
        public string converString()
        {
            return "{\"id\":\"" + this.id + "\", " +
                "\"username\":\"" + this.username + "\"," +
                "\"userID\":\"" + this.userID + "\"" +
                ",\"createdAt\":\"" + this.createdAt + "\"}";
        }
    }

    class AuthofUser
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string birthday { get; set; }
        public string phone { get; set; }
        public string sex { get; set; }
        public string role { get; set; }
        public bool locked { get; set; }
        public AuthofUser (int id, string Name, string Email, string Birthday, string Phone, bool sex, string role)
        {
            this.id = id;
            this.name = Name;
            this.email = Email;
            this.birthday = Birthday;
            this.phone = Phone;
            if (sex)
            {
                this.sex = "Male";
            }
            else
            {
                this.sex = "Female";
            }
            this.role = role;
        }
        public AuthofUser(int id, string Name, string Email, string Birthday, string Phone, bool sex, string role, bool locked)
        {
            this.id = id;
            this.name = Name;
            this.email = Email;
            this.birthday = Birthday;
            this.phone = Phone;
            if (sex)
            {
                this.sex = "Male";
            }
            else
            {
                this.sex = "Female";
            }
            this.role = role;
            this.locked = locked;
        }
    }

    class StoredProcedure 
    {
        SqlConnection conn;
        public StoredProcedure(SqlConnection con) {
            this.conn = con;
        }
        public SqlCommand selectUserWithID(int id)
        {
            if(id < 1) return null;

            SqlCommand cmd = new SqlCommand("SelectUserWithID",conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            return cmd;
        }

        public SqlCommand insertAuthInfo(string username, string password, int userID)
        {
            if(String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password) || userID < 1) return null;

            SqlCommand cmd = new SqlCommand("InsertAuthInfo", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
            cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = password;
            cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

            return cmd;
        }

        public SqlCommand selectAuthWithID(int ID)
        {
            if(ID < 1) return null;

            SqlCommand cmd = new SqlCommand("SelectAuthWithID", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ID", ID));

            return cmd;
        }

        public SqlCommand selectAuthWithEmail(string email, string password = null)
        {
            if(email == null) return null;

            string procedure = "SelectAuthWithEmail";

            List<SqlParameter> parameters = new List<SqlParameter>() { new SqlParameter("@Username", email) };


            if (!String.IsNullOrEmpty(password))
            {
                procedure = "SelectAuthWithEmailAndPWD";
                parameters.Add(new SqlParameter("@Password", password));
            }

            SqlCommand cmd = new SqlCommand(procedure,conn);
            cmd.CommandType = CommandType.StoredProcedure;
            parameters.ForEach(param => cmd.Parameters.Add(param));

            return cmd;
        }

        public SqlCommand selectAuthAndUser(string filter, string sort, int limit, int skip)
        {
            SqlCommand cmd = new SqlCommand("SelectAuthAndUser", conn);
            cmd.CommandType = CommandType.StoredProcedure;


            if (String.IsNullOrEmpty(filter))
            {
                filter = "";
            }
            else if (!filter.Contains('=') && !filter.Contains("like"))
            {
                return null;
            }

            if (String.IsNullOrEmpty(sort))
            {
                sort = "";
            }
            else
            {
                string[] arr = sort.Split(' ');
                if (arr.Length != 2)
                {
                    sort = "";
                }
                else
                {
                    if (arr[0] == "ID")
                    {
                        sort = "[Auth].[" + arr[0] + "] " + arr[1];
                    }
                    else
                    {
                        sort = "[" + arr[0] + "] " + arr[1];
                    }
                }
            }

            cmd.Parameters.Add(new SqlParameter("@Filter", filter));
            cmd.Parameters.Add(new SqlParameter("@Sort", sort));
            cmd.Parameters.Add(new SqlParameter("@Limit", limit == null ? 10 : limit));
            cmd.Parameters.Add(new SqlParameter("@Skip", skip == null ? 0 : skip));

            return cmd;
        }

        public SqlCommand countAuth(string filter)
        {
            SqlCommand cmd = new SqlCommand("CountAuth", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            if (String.IsNullOrEmpty(filter))
            {
                filter = "";
            }
            else if (!filter.Contains('=') && !filter.Contains("like"))
            {
                return null;
            }
            cmd.Parameters.Add(new SqlParameter("@Filter", filter));

            return cmd;
        }

        public SqlCommand insertUserInfo(string name, string email, string birthday, string phone, string sex)
        {
            if(String.IsNullOrEmpty(name) || String.IsNullOrEmpty(email) || String.IsNullOrEmpty(birthday) || String.IsNullOrEmpty(phone)) return null;

            SqlCommand cmd = new SqlCommand("InsertUserInfo",conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = email;
            cmd.Parameters.Add("@Birthday", SqlDbType.NVarChar).Value = birthday;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = phone;
            cmd.Parameters.Add("@Sex", SqlDbType.Bit).Value = sex == "male" ? true : false;

            return cmd;
        }

        public SqlCommand updateUserInfo(int id, string name, string birthday, string phone, string sex)
        {
            if(id < 1 || String.IsNullOrEmpty(name) || String.IsNullOrEmpty(birthday) || String.IsNullOrEmpty(phone) || String.IsNullOrEmpty(sex)) return null;

            SqlCommand cmd = new SqlCommand("UpdateUserInfo", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ID", id));
            cmd.Parameters.Add(new SqlParameter("@Name", name));
            cmd.Parameters.Add(new SqlParameter("@Birthday", birthday));
            cmd.Parameters.Add(new SqlParameter("@Phone", phone));
            cmd.Parameters.Add(new SqlParameter("@Sex", sex == "male" ? true : false));

            return cmd;
        }

        public SqlCommand updateAuthLocked(int id, bool locked)
        {
            if (id < 1) return null;

            SqlCommand cmd = new SqlCommand("UpdateAuthLocked", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@IsLocked", SqlDbType.Bit).Value = locked;

            return cmd;
        }

        public SqlCommand updateAuthPassword(int id, string password)
        {
            if(id < 1 || String.IsNullOrEmpty (password)) return null;

            SqlCommand cmd = new SqlCommand("UpdateAuthPassword",conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = password;

            return cmd;
        }

        public SqlCommand updateAuthRole(int id, string role)
        {
            if (id < 1 || String.IsNullOrEmpty(role)) return null;

            SqlCommand cmd = new SqlCommand("UpdateAuthRole", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@Role", SqlDbType.NVarChar,10).Value = role;

            return cmd;
        }

        public SqlCommand insertCategoryInfo(string name, string key)
        {
            if(String.IsNullOrEmpty(name) || String.IsNullOrEmpty(key)) return null;

            SqlCommand cmd = new SqlCommand("InsertCategoryInfo", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Name", name));
            cmd.Parameters.Add(new SqlParameter("@Key", key));

            return cmd;
        }

        public SqlCommand selectCategoryWithFilter(string filter = null, string sort = null)
        {
            if (String.IsNullOrEmpty(filter))
            {
                filter = "";
            }
            else if (!filter.Contains('=') && !filter.Contains("like"))
            {
                return null;
            }

            if (String.IsNullOrEmpty(sort))
            {
                sort = "";
            }
            else
            {
                string[] arr = sort.Split(' ');
                if (arr.Length != 2)
                {
                    sort = "";
                }
                else
                {
                    sort = "[" + arr[0] + "] " + arr[1];
                }
            }

            SqlCommand cmd = new SqlCommand("SelectCategoryWithFilter", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Filter", filter));
            cmd.Parameters.Add(new SqlParameter("@Sort", sort));

            return cmd;
        }

        public SqlCommand insertBookInfo(string name, string category, string description, string imgSrc)
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(category)) return null;

            SqlCommand cmd = new SqlCommand("InsertBookInfo", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Name", name));
            cmd.Parameters.Add(new SqlParameter("@Category", category));
            cmd.Parameters.Add(new SqlParameter("@Description", description));
            cmd.Parameters.Add(new SqlParameter("@ImgSrc", imgSrc));

            return cmd;
        }

        public SqlCommand updateBookInfo(int ID, string name, string category, string description, string imgSrc)
        {
            if (ID < 1 || String.IsNullOrEmpty(name) || String.IsNullOrEmpty(category)) return null;

            SqlCommand cmd = new SqlCommand("UpdateBookInfo", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ID", ID));
            cmd.Parameters.Add(new SqlParameter("@Name", name));
            cmd.Parameters.Add(new SqlParameter("@Category", category));
            cmd.Parameters.Add(new SqlParameter("@Description", description));
            cmd.Parameters.Add(new SqlParameter("@ImgSrc", imgSrc));

            return cmd;
        }

        public SqlCommand deleteRowWithID(int ID, string tableName)
        {
            if (ID < 1 || String.IsNullOrEmpty(tableName)) return null;

            SqlCommand cmd = new SqlCommand("DeleteRowWithID", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ID", ID));
            cmd.Parameters.Add(new SqlParameter("@TableName", tableName));

            return cmd;
        }

        public SqlCommand selectBooksWithFilter(string filter,string sort, int limit, int skip)
        {
            SqlCommand cmd = new SqlCommand("SelectBooksWithFilter", conn);
            cmd.CommandType = CommandType.StoredProcedure;


            if (String.IsNullOrEmpty(filter))
            {
                filter = "";
            }else if (!filter.Contains('=') && !filter.Contains("like"))
            {
                return null;
            }

            if (String.IsNullOrEmpty(sort))
            {
                sort = "";
            }else
            {
                string[] arr = sort.Split(' ');
                if(arr.Length != 2)
                {
                    sort = "";
                }else
                {
                    sort = "["+ arr[0]+"] " + arr[1];
                }
            }

            cmd.Parameters.Add(new SqlParameter("@Filter", filter));
            cmd.Parameters.Add(new SqlParameter("@Sort", sort));
            cmd.Parameters.Add(new SqlParameter("@Limit",limit == null ? 10 : limit));
            cmd.Parameters.Add(new SqlParameter("@Skip", skip == null ? 0 : skip));

            return cmd;
        }
        public SqlCommand selectBookwithId(int id)
        {
            if (id < 1 )return null;

            SqlCommand cmd = new SqlCommand("SelectBooksWithID", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            return cmd;
        }

        public SqlCommand countBooksWithFilter(string filter)
        {
            SqlCommand cmd = new SqlCommand("CountBooksWithFilter", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            if (String.IsNullOrEmpty(filter))
            {
                filter = "";
            }
            else if (!filter.Contains('=') && !filter.Contains("like"))
            {
                return null;
            }
            cmd.Parameters.Add(new SqlParameter("@Filter", filter));

            return cmd;
        }
    }

}