using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
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
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if(Page.IsValid)
            {
                //string Name = txtbName.Text;
                //string Category = txtbCategory.Text;
                //string Description = txtbDescription.Text;
                //string imgSrc = txtbimgSrc.Text;
                //int numberLike = Convert.ToInt32(txtbLike.Text);
                //int numberView = Convert.ToInt32(txtbView.Text);
                //SqlCommand cmd = new SqlCommand ("insert into tblSach(name,category,description,imgSrc,numberLike,numberView) " +
                //    "values('"+Name+"', '"+Category+"', '"+ Description + "', '"+imgSrc+"',"+numberLike+", "+numberView+")",con);
                //cmd.ExecuteNonQuery();
                //GridView1.DataBind();
            }
        }
    }
}