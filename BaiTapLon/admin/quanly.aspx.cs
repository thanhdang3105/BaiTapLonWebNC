using BaiTapLon.server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BaiTapLon.admin
{
    public partial class quanly : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            con.Open();
            if(Request.HttpMethod == "GET")
            {
                string pathInfo = Request.PathInfo;
                if(pathInfo == "/getData")
                {
                    getData();
                }
            }
            else
            {
                switch (Request.Form["action"])
                {
                    case "create":
                        createSach(Request.Form);
                        break;
                    case "createCate":
                        createCategory(Request.Form);
                        break;
                }
            }
            con.Close();
        }

        protected void getData(string filter = null ,string sort = null ,int limit=15,int skip=0)
        {

            string where = "";
            string orderBy = "ORDER BY ID DESC";
            if(!String.IsNullOrEmpty(filter))
            {
                where = "where " + filter;
            }
            if (!String.IsNullOrEmpty(sort))
            {
                orderBy = "order by " + sort;
            }

            SqlCommand cmd = new SqlCommand("select * from tblSach "+ where + orderBy, con);
            SqlDataReader reader = cmd.ExecuteReader();

            List<ClassSach> lists = new List<ClassSach>();

            while (reader.Read())
            {
                ClassSach sach = new ClassSach(Convert.ToInt32(reader["ID"]), reader["name"].ToString(), reader["category"].ToString(), reader["description"].ToString(), reader["imgSrc"].ToString(), Convert.ToInt32(reader["numberLike"]), Convert.ToInt32(reader["numberView"]));
                lists.Add(sach);
            }

            string[] data = new string[lists.Count];

            if (lists.Count > limit)
            {
                data = new string[limit];
            }

            for (int i = 0; i < data.Length; i++)
            {
                if (lists[skip + i] != null)
                {
                    ClassSach item = lists[skip + i];
                    data[i] = item.converString();
                }
            }
            Response.ContentType = "application/json";
            Response.Write("{\"data\": ["+ String.Join(",", data) + "], \"count\": "+lists.Count+"}");
            Response.End();
        }

        protected void createCategory(NameValueCollection form)
        {
            Response.ContentType = "application/json";
            string key = form["keyCate"];
            string value = form["valueCate"];
            if(!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(value))
            {
                Response.StatusCode = 200;
                Response.Write("{\"data\": \"Create Succes!\"}");
            }
            else
            {
                Response.StatusCode = 400;
                Response.Write("{\"msg\":\"Invalid Params!\"}");
            }
            Response.End();
        }

        protected void createSach(NameValueCollection form)
        {
            Response.ContentType = "application/json";
            string name = form["name"];
            string category = form["category"];
            string desc = form["desc"];
            string imgSach = form["imgSach"];
            if(String.IsNullOrEmpty(name) || String.IsNullOrEmpty(category)){
                Response.StatusCode = 400;
                Response.Write("{\"msg\":\"Invalid Params!\"}");
            }else
            {
                SqlCommand cmd = new SqlCommand("insert into tblSach (name,category,description,imgSrc)" +
                    " output INSERTED.ID " +
                    "values ('"+name+"','"+category+"','"+desc+"','"+imgSach+"')",con);
                try
                {
                    object output = cmd.ExecuteScalar();
                   
                    //tableSach.DataBind();
                    Response.Write("{\"data\": \""+ output.ToString()+ "\"}");
                    Response.StatusCode = 200;
                }catch (ThreadAbortException ex)
                {
                    Response.Write("{\"msg\":\""+ex.Message+"\"}");
                    Response.StatusCode = 500;
                }
            }
            Response.End();
        }
    }
}