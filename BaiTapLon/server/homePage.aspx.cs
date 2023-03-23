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
            con.Open();
            Response.ContentType = "application/json";
            string response = "{\"msg\": Request not Support! }";
            int limit = Convert.ToInt32(Request.Form["limit"]);
            int skip = Convert.ToInt32(Request.Form["skip"]);
            if (limit <= 0)
            {
                limit = 10;
            }
            if (Request.RequestType == "GET")
            {
                string NEW = getSachNEW();
                string HOT = getSachHOT();
                string VIEW = getSachViewest();
                response = "{\"data\": { \"hot\": [" + HOT + "], \"new\": [" + NEW + "], \"view\": [" + VIEW + "] }}";
            } else if (Request.RequestType == "POST" && Request.Form["search"] != null)
            {
                string[] dataSearch = SearchData(Request.Form["search"],limit,skip);
                response = "{\"data\": { \"data\": [" + dataSearch[0] + "], \"count\": " + dataSearch[1] +" } }";
            } else if (Request.RequestType == "POST" && Request.Form["category"] != null) { 
                if(limit <= 0) {
                    limit = 20;
                }
                int count = CountData("category", Request.Form["category"]);
                string dataWithCate = getSachWithCategory(Request.Form["category"], limit, skip);
                response = "{\"data\": { \"data\": [" + dataWithCate + "], \"count\": " + count + " } }";
            } else
            {
                Response.StatusCode = 404;
            }
            Response.Write(response);
            Response.End();
        }

        protected int CountData(string column, string value)
        {
            string where = "where " + column + " like '%" + value + "%'";
            if(column == null || value == null || value == "")
            {
                where = "";
            }
            SqlCommand cmd = new SqlCommand("select COUNT(ID) from tblSach "+where, con);
            object reader = cmd.ExecuteScalar();

            int count = (int)reader;

            return count;
        }

        protected string[] SearchData(string search, int limit = 10, int skip = 0)
        {
            SqlCommand cmd = new SqlCommand("select * from tblSach WHERE name like '%"+search+ "%' OR category like '%"+search+"%' ORDER BY numberLike DESC OFFSET " + skip + " ROWS", con);
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

            return new string[2] { String.Join(",", data) , lists.Count.ToString() };
        }

        protected string getSachWithCategory(string category, int limit = 20, int skip = 0)
        {
            string where = "where category = '" + category + "'";
            if(category == "") {
                where = "";
            }

            SqlCommand cmd = new SqlCommand("select * from tblSach " + where + " ORDER BY ID DESC OFFSET " + skip + " ROWS FETCH NEXT " + limit + " ROWS ONLY", con);
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