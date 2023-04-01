using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BaiTapLon.server
{

    public class Authorization
    {
        public string textEncrypted { get; set; }
        public Authorization(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                this.textEncrypted = "";
            }
            else
            {
                this.textEncrypted = encode(text);
            }
        }
        private string encode(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return "";
            }
            byte[] bytes = Encoding.UTF8.GetBytes(value);

            string encoded = Convert.ToBase64String(bytes);
            return encoded;
        }
        public string decode(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                return "";
            }
            try
            {
                byte[] bytes = Convert.FromBase64String(value);

                string decoded = Encoding.UTF8.GetString(bytes);
                return decoded;
            }
            catch (Exception err)
            {

                return err.Message.ToString();
            }
        }

        public string generateToken(string text, string role = "admin")
        {
            string token = text + "-" + DateTime.UtcNow.AddDays(7).ToString() + "-" + role;
            string encryptToken = encode(token);
            return encryptToken;
        }
    }
}