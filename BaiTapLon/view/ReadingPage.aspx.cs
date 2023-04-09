using BaiTapLon.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;

namespace BaiTapLon
{
    public partial class ReadingPage : System.Web.UI.Page
    {
        SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ConnectionString);
        StoredProcedure procedure;
        protected void Page_Load(object sender, EventArgs e)
        {
            string id = Request.QueryString["id"];
            int ID = Convert.ToInt32(id);
            sqlConnection.Open();
            procedure = new StoredProcedure(sqlConnection);
            if(ID > 0)
            {
                SqlCommand cmd = procedure.selectBookwithId(ID);

                SqlDataReader reader = cmd.ExecuteReader();

                List<ClassSach> lists = new List<ClassSach>();

                while (reader.Read())
                {
                    ClassSach sach = new ClassSach(Convert.ToInt32(reader["ID"]), reader["name"].ToString(), reader["category"].ToString(), reader["description"].ToString(), reader["imgSrc"].ToString(), Convert.ToInt32(reader["like"]), Convert.ToInt32(reader["view"]));
                    lists.Add(sach);
                }
                storyContent.Text = lists[0].desc;
                Literal1.Text = lists[0].category;
                Literal2.Text = lists[0].name;
                Literal3.Text = lists[0].name;
            }
            
        }
    }
}