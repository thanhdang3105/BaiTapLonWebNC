using Newtonsoft.Json;
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
        StoredProcedure procedure;
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";
            string response = "{\"msg\": \"Request not Support!\" }";
            try
            {
                con.Open();
                procedure = new StoredProcedure(con);
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
                        response = "{\"data\": { \"hot\": " + HOT + ", \"new\": " + NEW + ", \"view\": " + VIEW + " }}";
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
                SqlCommand cmd = procedure.selectCategoryWithFilter();

                if (cmd == null) throw new Exception("System Error!");

                SqlDataReader reader = cmd.ExecuteReader();

                List<CategoryClass> lists = new List<CategoryClass>();

                while (reader.Read())
                {
                    CategoryClass category = new CategoryClass(reader["name"].ToString(), reader["key"].ToString());
                    lists.Add(category);
                }

                string data = JsonConvert.SerializeObject(lists);

                Response.Write("{\"data\": " + data + "}");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write("{\"msg\": \"" + ex.Message + "\"}");
            }
            Response.End();
        }

        protected string getSachHOT(int limit = 10, int skip = 0)
        {

            SqlCommand cmd = procedure.selectBooksWithFilter(null, "like DESC", limit, skip);

            if (cmd == null) throw new Exception("System Error!");

            SqlDataReader reader = cmd.ExecuteReader();

            List<ClassSach> lists = new List<ClassSach>();

            while (reader.Read())
            {
                ClassSach item = new ClassSach((int)reader["ID"], reader["name"].ToString(), reader["category"].ToString(), reader["description"].ToString(), reader["imgSrc"].ToString(), (int)reader["like"], (int)reader["view"]);
                lists.Add(item);
            }
            string data = JsonConvert.SerializeObject(lists);

            reader.Close();
            cmd.Cancel();

            return data;
        }

        protected string getSachNEW(int limit = 10, int skip = 0)
        {
            SqlCommand cmd = procedure.selectBooksWithFilter(null, "ID DESC", limit, skip);
            
            if (cmd == null) throw new Exception("System Error!");
            
            SqlDataReader reader = cmd.ExecuteReader();
            

            List<ClassSach> lists = new List<ClassSach>();

            while (reader.Read())
            {
                ClassSach item = new ClassSach((int)reader["ID"], reader["name"].ToString(), reader["category"].ToString(), reader["description"].ToString(), reader["imgSrc"].ToString(), (int)reader["like"], (int)reader["view"]);
                lists.Add(item);
            }

            string data = JsonConvert.SerializeObject(lists);

            reader.Close();
            cmd.Cancel();

            return data;
        }

        protected string getSachViewest(int limit = 10, int skip = 0)
        {
            SqlCommand cmd = procedure.selectBooksWithFilter(null, "view DESC", limit, skip);
            
            if (cmd == null) throw new Exception("System Error!");
            
            SqlDataReader reader = cmd.ExecuteReader();


            List<ClassSach> lists = new List<ClassSach>();

            while (reader.Read())
            {
                ClassSach item = new ClassSach((int)reader["ID"], reader["name"].ToString(), reader["category"].ToString(), reader["description"].ToString(), reader["imgSrc"].ToString(), (int)reader["like"], (int)reader["view"]);
                lists.Add(item);
            }

            string data = JsonConvert.SerializeObject(lists);

            reader.Close();
            cmd.Cancel();

            return data;
        }
    }

}