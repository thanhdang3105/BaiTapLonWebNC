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
using System.Xml.Linq;

namespace BaiTapLon.admin
{
    public partial class quanly : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            con.Open();

            if (Request.HttpMethod == "GET")
            {
                string pathInfo = Request.PathInfo;
                if(pathInfo == "/getData")
                {
                    CheckPermission();
                    string sort = Request.QueryString["sort"];
                    int skip = Convert.ToInt32(Request.QueryString["skip"]);
                    getData(null,sort,10,skip);
                }
            }
            else
            {
                CheckPermission();
                switch (Request.Form["action"])
                {
                    case "create":
                        createSach(Request.Form);
                        break;
                    case "createCate":
                        createCategory(Request.Form);
                        break;
                    case "update":
                        updateSach(Request.Form);
                        break;
                    case "delete":
                        deleteSach(Request.Form);
                        break;
                    default:
                        Response.ContentType = "application/json";
                        Response.Write("{\"msg\": \"Request not Support!\" }");
                        Response.End();
                        break;
                }
            }
            con.Close();
        }

        protected void CheckPermission()
        {
            string token = Session["Authorization"]?.ToString();

            if (String.IsNullOrEmpty(token))
            {
                token = Request.Headers["Authorization"];
            }

            Authorization authClient = new Authorization(token);
            string tokenInfo = authClient.decode(token);
            if (String.IsNullOrEmpty(tokenInfo))
            {
                Response.StatusCode = 401;
                Response.Write("No Permission!");
                Response.End();
            }
            else
            {
                string[] userInfo = tokenInfo.Split('-');
                if (userInfo[2] != "admin" || DateTime.UtcNow.CompareTo(DateTime.Parse(userInfo[1])) >= 0)
                {
                    Response.StatusCode = 401;
                    Response.Write("No Permission!");
                    Response.End();
                }
            }
        }

        protected void getData(string filter = null ,string sort = null ,int limit=10,int skip=0)
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

            Response.ContentType = "application/json";
            try
            {
                SqlCommand cmd = new SqlCommand("select * from tblSach " + where + orderBy, con);
                SqlDataReader reader = cmd.ExecuteReader();

                List<ClassSach> lists = new List<ClassSach>();

                while (reader.Read())
                {
                    ClassSach sach = new ClassSach(Convert.ToInt32(reader["ID"]), reader["name"].ToString(), reader["category"].ToString(), reader["description"].ToString(), reader["imgSrc"].ToString(), Convert.ToInt32(reader["numberLike"]), Convert.ToInt32(reader["numberView"]));
                    lists.Add(sach);
                }

                string[] data = new string[lists.Count];
                
                skip = skip * limit;

                int length = limit;

                if (skip > lists.Count)
                {
                    skip = 0;
                }else
                {
                    length = lists.Count - skip;
                }

                

                if (length > limit)
                {
                    data = new string[limit];
                }else
                {
                    data = new string[length];
                }

                

                for (int i = 0; i < data.Length; i++)
                {
                    if (lists[skip + i] != null)
                    {
                        ClassSach item = lists[skip + i];
                        data[i] = item.converString();
                    }
                }
                Response.Write("{\"data\": [" + String.Join(",", data) + "], \"count\": " + lists.Count + "}");
            }
            catch(Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write("{\"msg\": \""+ ex.Message + "\"}");
            }
            Response.End();
        }

        protected void createCategory(NameValueCollection form)
        {
            Response.ContentType = "application/json";
            string name = form["nameCate"];
            string value = form["valueCate"];
            if(!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(value))
            {
                SqlCommand cmd = new SqlCommand("insert into tblCategory(name,category) values (N'"+name+"',N'"+value+"')",con);
                try
                {
                    cmd.ExecuteNonQuery();
                    Response.StatusCode = 200;
                    Response.Write("{\"data\": \"Create Succes!\"}");
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 400;
                    Response.Write("{\"msg\": \"Create Fail!\"}");
                }
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
            string imgSrc = form["imgSrc"];
            if(String.IsNullOrEmpty(name) || String.IsNullOrEmpty(category)){
                Response.StatusCode = 400;
                Response.Write("{\"msg\":\"Invalid Params!\"}");
            }else
            {
                SqlCommand cmd = new SqlCommand("insert into tblSach (name,category,description,imgSrc)" +
                    " output INSERTED.ID " +
                    "values (N'"+name+"',N'"+category+"',N'"+desc+"',N'"+ imgSrc + "')",con);
                try
                {
                    object output = cmd.ExecuteScalar();
                   
                    //tableSach.DataBind();
                    Response.Write("{\"data\": \""+ output.ToString()+ "\"}");
                    Response.StatusCode = 200;
                }catch (ThreadAbortException ex)
                {
                    Response.Write("{\"msg\":'"+ex.Message+"'}");
                    Response.StatusCode = 500;
                }
            }
            Response.End();
        }

        protected void updateSach(NameValueCollection form)
        {
            Response.ContentType = "application/json";
            string id = form["id"];
            string name = form["name"];
            string category = form["category"];
            string desc = form["desc"];
            string imgSrc = form["imgSrc"];
            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(name) || String.IsNullOrEmpty(category))
            {
                Response.StatusCode = 400;
                Response.Write("{\"msg\":\"Invalid Params!\"}");
            }
            else
            {
                string newData = "name = N'" + name + "' , category = N'" + category + "' , description = N'" + desc + "' , imgSrc = N'" + imgSrc + "'";
                SqlCommand cmd = new SqlCommand("update tblSach set " + newData + " where ID = " + Convert.ToInt32(id), con);
                try
                {
                    cmd.ExecuteNonQuery();

                    Response.Write("{\"data\": \"Update Succes!\"}");
                    Response.StatusCode = 200;
                }
                catch (ThreadAbortException ex)
                {
                    Response.Write("{\"msg\":\"" + ex.Message + "\"}");
                    Response.StatusCode = 500;
                }
            }
            Response.End();
        }

        protected void deleteSach(NameValueCollection form)
        {
            Response.ContentType = "application/json";
            string id = form["id"];
            if (String.IsNullOrEmpty(id))
            {
                Response.StatusCode = 400;
                Response.Write("{\"msg\":\"Invalid Params!\"}");
            }
            else
            {
                SqlCommand cmd = new SqlCommand("delete from tblSach where ID = " + Convert.ToInt32(id), con);
                try
                {
                    cmd.ExecuteNonQuery();

                    Response.Write("{\"data\": \"Delete Succes!\"}");
                    Response.StatusCode = 200;
                }
                catch (ThreadAbortException ex)
                {
                    Response.Write("{\"msg\":'" + ex.Message + "'}");
                    Response.StatusCode = 500;
                }
            }
            Response.End();
        }
    }
}