using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
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

            string NEW = getSachNEW();
            string HOT = getSachHOT();
            string VIEW = getSachViewest();
            Response.Write("{\"data\": { \"hot\": [" + HOT + "], \"new\": ["+NEW+ "], \"view\": ["+ VIEW + "] }}");
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