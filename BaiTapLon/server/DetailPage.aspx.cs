using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BaiTapLon.server
{
    public partial class DetailPage : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";
            string response = "{\"msg\": \"Request not Support!\" }";
            try
            {
                con.Open();
                string category = Request.Form["category"];
                string sort = Request.Form["sort"];
                int limit = Convert.ToInt32(Request.Form["limit"]);
                int skip = Convert.ToInt32(Request.Form["skip"]);
                if (Request.RequestType == "POST" && Request.Form["search"] != null)
                {
                    if (limit <= 0)
                    {
                        limit = 10;
                    }
                    string[] dataSearch = SearchData(Request.Form["search"], limit, skip, sort);
                    response = "{\"data\": { \"data\": [" + dataSearch[0] + "], \"count\": " + dataSearch[1] + " } }";
                }
                else if (Request.RequestType == "POST" && category != null)
                {
                    if (limit <= 0)
                    {
                        limit = 20;
                    }

                    int count = CountData("category", category);
                    string dataWithCate = getSachWithCategory(category, limit, skip, sort);
                    response = "{\"data\": { \"data\": [" + dataWithCate + "], \"count\": " + count + " } }";
                }
                else
                {
                    Response.StatusCode = 404;
                }
            }catch(Exception ex)
            {
                Response.StatusCode = 404;
                response = "{\"msg\": \""+ex.Message.ToString()+"\" }";
            }
            Response.Write(response);
            con.Close();
            Response.End();
        }

        protected int CountData(string column, string value)
        {
            string where = "where " + column + " like '%" + value + "%'";
            if (column == null || value == null || value == "")
            {
                where = "";
            }
            SqlCommand cmd = new SqlCommand("select COUNT(ID) from tblSach " + where, con);
            object reader = cmd.ExecuteScalar();

            int count = (int)reader;

            return count;
        }

        protected string[] SearchData(string search, int limit = 10, int skip = 0, string sort = null)
        {

            string order = "ID DESC";
            if (!String.IsNullOrEmpty(sort))
            {
                order = sort;
            }

            SqlCommand cmd = new SqlCommand("select * from tblSach WHERE name like '%" + search + "%' OR category like '%" + search + "%' ORDER BY "+ order +" OFFSET " + skip + " ROWS", con);
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

            return new string[2] { String.Join(",", data), lists.Count.ToString() };
        }

        protected string getSachWithCategory(string category, int limit = 20, int skip = 0, string sort = null)
        {
            string where = "where category = '" + category + "'";
            string order = "ID DESC";
            if (category == "")
            {
                where = "";
            }
            if (!String.IsNullOrEmpty(sort))
            {
                order = sort;
            }

            SqlCommand cmd = new SqlCommand("select * from tblSach " + where + " ORDER BY "+ order + " OFFSET " + (skip * limit) + " ROWS FETCH NEXT " + limit + " ROWS ONLY", con);
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