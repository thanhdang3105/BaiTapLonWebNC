using Newtonsoft.Json;
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
        StoredProcedure procedure;
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";
            string response = "{\"msg\": \"Request not Support!\" }";
            try
            {
                con.Open();
                procedure = new StoredProcedure(con);
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
                    response = "{\"data\": { \"data\": " + dataSearch[0] + ", \"count\": " + dataSearch[1] + " } }";
                }
                else if (Request.RequestType == "POST" && category != null)
                {
                    if (limit <= 0)
                    {
                        limit = 20;
                    }

                    int count = CountData("category", category);
                    string dataWithCate = getSachWithCategory(category, limit, skip, sort);
                    response = "{\"data\": { \"data\": " + dataWithCate + ", \"count\": " + count + " } }";
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
            string where = column + " like '%" + value + "%'";
            if (column == null || value == null || value == "")
            {
                where = "";
            }
            SqlCommand cmd = procedure.countBooksWithFilter(where);

            object reader = cmd.ExecuteScalar();

            int count = (int)reader;

            cmd.Cancel();

            return count;
        }

        protected string[] SearchData(string search, int limit = 10, int skip = 0, string sort = null)
        {

            string order = "ID DESC";
            if (!String.IsNullOrEmpty(sort))
            {
                order = sort;
            }

            SqlCommand cmd = procedure.selectBooksWithSearch(search, order,limit,skip);

            SqlDataReader reader = cmd.ExecuteReader();

            List<ClassSach> lists = new List<ClassSach>();

            while (reader.Read())
            {
                ClassSach item = new ClassSach((int)reader["ID"], reader["name"].ToString(), reader["author"].ToString(), reader["categoryName"].ToString(), reader["categoryKey"].ToString(), reader["content"].ToString(), reader["imgSrc"].ToString(), (int)reader["like"], (int)reader["view"]);
                lists.Add(item);
            }

            string data = JsonConvert.SerializeObject(lists);

            cmd.Cancel();

            reader.Close();

            cmd = new SqlCommand("SELECT COUNT(ID) AS count FROM Books WHERE [name] LIKE N'%"+ search + "%' OR author LIKE N'%" + search + "%' OR [category] LIKE N'%"+ search + "%'",con);

            object count = cmd.ExecuteScalar();

            return new string[2] { data, count.ToString() };
        }

        protected string getSachWithCategory(string category, int limit = 20, int skip = 0, string sort = null)
        {
            string where = "{\"category\": \"" + category + "\"}";
            string order = "ID DESC";
            if (category == "")
            {
                where = "";
            }
            if (!String.IsNullOrEmpty(sort))
            {
                order = sort;
            }

            SqlCommand cmd = procedure.selectBooksWithFilter(where, sort, limit, skip);

            SqlDataReader reader = cmd.ExecuteReader();

            List<ClassSach> lists = new List<ClassSach>();

            while (reader.Read())
            {
                ClassSach item = new ClassSach((int)reader["ID"], reader["name"].ToString(), reader["author"].ToString(), reader["categoryName"].ToString(), reader["categoryKey"].ToString(), reader["content"].ToString(), reader["imgSrc"].ToString(), (int)reader["like"], (int)reader["view"]);
                lists.Add(item);
            }

            string data = JsonConvert.SerializeObject(lists);

            reader.Close();
            cmd.Cancel();

            return data;
        }
    }
}