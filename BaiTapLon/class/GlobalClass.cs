using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;

namespace BaiTapLon.server {
    class User
    {
        public int ID { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string birthday { get; set; }
        public string phone { get; set; }
        public string sex { get; set; }
        public User(int id, string Name, string Email, string Birthday, string sex, string Phone)
        {
            this.ID = id;
            this.name = Name;
            this.email = Email;
            this.birthday = Birthday;
            this.sex = sex;
            this.phone = Phone;
        }
        public string converString()
        {
            string data = "{\"id\":\"" + this.ID + "\", " +
                "\"name\":\"" + this.name + "\"," +
                "\"email\":\"" + this.email + "\"," +
                "\"birthDay\":\"" + this.birthday + "\"," +
                "\"phone\":\"" + this.phone + "\"," +
                "\"sex\":\"" + this.sex + "\"}";
            return data.ToString();
        }
    }

    class Auth
    {
        public int ID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int userID { get; set; }
        public DateTime createdAt { get; set; }
        public Auth(int id, string UserName, string Password, int Userid, DateTime createdAt)
        {
            this.ID = id;
            this.username = UserName;
            this.password = Password;
            this.userID = Userid;
            this.createdAt = createdAt;
        }
        public string converString()
        {
            return "{\"id\":\"" + this.ID + "\", " +
                "\"username\":\"" + this.username + "\"," +
                "\"userID\":\"" + this.userID + "\"" +
                ",\"createdAt\":\"" + this.createdAt.ToString() + "\"}";
        }
    }

}