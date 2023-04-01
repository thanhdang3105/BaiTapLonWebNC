using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BaiTapLon.server
{
    public partial class homePage : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";
            string response = "{\"msg\": \"Request not Support!\" }";
            try
            {
                con.Open();
                if (Request.RequestType == "GET")
                {
                    string pathInfo = Request.PathInfo;
                    if (pathInfo == "/getCate")
                    {
                        getCategory();
                    }
                    else
                    {
                        string NEW = getSachNEW();
                        string HOT = getSachHOT();
                        string VIEW = getSachViewest();
                        response = "{\"data\": { \"hot\": [" + HOT + "], \"new\": [" + NEW + "], \"view\": [" + VIEW + "] }}";
                    }
                }
                else
                {
                    Response.StatusCode = 404;
                }
            } catch(Exception ex)
            {
                response = "{\"msg\": \""+ex.Message.ToString()+"\" }";
            }
            Response.Write(response);
            con.Close();
            Response.End();
        }

        protected void getCategory()
        {
            Response.ContentType = "application/json";
            try
            {
                SqlCommand cmd = new SqlCommand("select * from tblCategory order by ID DESC", con);
                SqlDataReader reader = cmd.ExecuteReader();

                List<string> lists = new List<string>();

                while (reader.Read())
                {

                    string category = "{\"key\": \"" + reader["name"] + "\", \"value\":\"" + reader["category"] + "\"}";
                    lists.Add(category);

                }

                string[] data = new string[lists.Count];

                for (int i = 0; i < data.Length; i++)
                {
                    if (lists[i] != null)
                    {
                        data[i] = lists[i];
                    }
                }
                Response.Write("{\"data\": [" + String.Join(",", data) + "]}");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write("{\"msg\":'" + ex.Message + "'}");
            }
            Response.End();
        }

        protected string getSachHOT(int limit = 10, int skip = 0)
        {
            SqlCommand cmd = new SqlCommand("select * from tblSach ORDER BY numberLike DESC OFFSET "+skip+" ROWS FETCH NEXT "+limit+" ROWS ONLY", con);
            SqlDataReader reader = cmd.ExecuteReader();

            List<ClassSach> lists = new List<ClassSach>();

            while (reader.Read())
            {
                ClassSach item = new ClassSach((int)reader["ID"], reader["name"].ToString(), reader["category"].ToString(), reader["description"].ToString(), reader["imgSrc"].ToString(), (int)reader["numberLike"], (int)reader["numberView"]);
                lists.Add(item);
            }
            string[] data = new string[lists.Count];

            for (int i = 0; i < lists.Count; i++)
            {
                ClassSach item = lists[i];
                data[i] = item.converString();
            }

            reader.Close();
            cmd.Cancel();

            return String.Join(",", data);
        }

        protected string getSachNEW(int limit = 10, int skip = 0)
        {
            SqlCommand cmd = new SqlCommand("select * from tblSach ORDER BY ID DESC OFFSET " + skip + " ROWS FETCH NEXT " + limit + " ROWS ONLY", con);
            SqlDataReader reader = cmd.ExecuteReader();

            List<ClassSach> lists = new List<ClassSach>();

            while (reader.Read())
            {
                ClassSach item = new ClassSach((int)reader["ID"], reader["name"].ToString(), reader["category"].ToString(), reader["description"].ToString(), reader["imgSrc"].ToString(), (int)reader["numberLike"], (int)reader["numberView"]);
                lists.Add(item);
            }
            string[] data = new string[lists.Count];

            for (int i = 0; i < lists.Count; i++)
            {
                ClassSach item = lists[i];
                data[i] = item.converString();
            }

            reader.Close();
            cmd.Cancel();

            return String.Join(",", data);
        }

        protected string getSachViewest(int limit = 10, int skip = 0)
        {
            SqlCommand cmd = new SqlCommand("select * from tblSach ORDER BY numberView DESC OFFSET " + skip + " ROWS FETCH NEXT " + limit + " ROWS ONLY", con);
            SqlDataReader reader = cmd.ExecuteReader();

            List<ClassSach> lists = new List<ClassSach>();

            while (reader.Read())
            {
                ClassSach item = new ClassSach((int)reader["ID"], reader["name"].ToString(), reader["category"].ToString(), reader["description"].ToString(), reader["imgSrc"].ToString(), (int)reader["numberLike"], (int)reader["numberView"]);
                lists.Add(item);
            }
            string[] data = new string[lists.Count];

            for (int i = 0; i < lists.Count; i++)
            {
                ClassSach item = lists[i];
                data[i] = item.converString();
            }

            reader.Close();
            cmd.Cancel();

            return String.Join(",", data);
        }
    }

}