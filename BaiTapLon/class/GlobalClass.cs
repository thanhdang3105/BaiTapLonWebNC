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
        public string birthday { get; set; }
        public string sex { get; set; }
        public User(int id, string Name, string Birthday, string sex)
        {
            this.ID = id;
            this.name = Name;
            this.birthday = Birthday;
            this.sex = sex;
        }
        public string converString()
        {
            return "{\"id\":\"" + this.ID + "\", " +
                "\"name\":\"" + this.name + "\"," +
                "\"birthDay\":\"" + this.birthday + "\"" +
                ",\"sex\":\"" + this.sex + "\"}";
        }
    }

    class Auth
    {
        public int ID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int userID { get; set; }
        public Auth(int id, string UserName, string Password, int Userid)
        {
            this.ID = id;
            this.username = UserName;
            this.password = (string)encodePassword(Password).Normalize();
            this.userID = Userid;
        }
        public string converString()
        {
            return "{\"id\":\"" + this.ID + "\", " +
                "\"username\":\"" + this.username + "\"," +
                "\"password\":\"" + this.password + "\"" +
                ",\"userID\":\"" + this.userID + "\"}";
        }
        public bool checkPassword(string password)
        {
            if (decodePassword() == password)
            {
                return true;
            } else
            {
                return false;
            }
        }
        private string encodePassword(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);

            string encoded = Convert.ToBase64String(bytes);
            return encoded.ToString();
        }
        private string decodePassword()
        {
            byte[] bytes = Convert.FromBase64String(this.password);

            string decoded = Encoding.UTF8.GetString(bytes);
            return decoded;
        }

    }

}