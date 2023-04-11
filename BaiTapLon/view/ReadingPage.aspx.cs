using BaiTapLon.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace BaiTapLon
{
    public partial class ReadingPage : System.Web.UI.Page
    {
        SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ConnectionString);
        StoredProcedure procedure;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                sqlConnection.Open();
                procedure = new StoredProcedure(sqlConnection);
            }catch(Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write("System Error!");
                Response.End();
            }

            int userID = checkToken();

            if (Request.HttpMethod == "POST")
            {
                ClassSach bookReading = (ClassSach)Session["BookReading"];

                string action = Request.Form["action"];

                if(action != "view" && userID < 1)
                {
                    ErrorResponse errMsg = new ErrorResponse("Vui lòng đăng nhập!");
                    Response.StatusCode = 401;
                    Response.Write(JsonConvert.SerializeObject(errMsg));
                    Response.End();
                }

                setHistoryAction(bookReading, action, userID);
            }else
            {
                Select_data(userID);
            }
        }

        protected void Select_data(int userID)
        {
            string id = Request.QueryString["id"];
            int ID = Convert.ToInt32(id);

            if (ID > 0)
            {
                SqlCommand cmd = procedure.selectHistoryActionWithBook(0, userID, ID);

                SqlDataReader reader = cmd.ExecuteReader();

                List<ClassSach> lists = new List<ClassSach>();

                bool like = false;

                while (reader.Read())
                {
                    ClassSach sach = new ClassSach(Convert.ToInt32(reader["bookID"]), reader["name"].ToString(), reader["author"].ToString(), reader["categoryName"].ToString(), reader["categoryKey"].ToString(), reader["content"].ToString(), reader["imgSrc"].ToString(), Convert.ToInt32(reader["like"]), Convert.ToInt32(reader["view"]));
                    lists.Add(sach);
                    if (reader["action"].ToString() == "like")
                    {
                        like = true;
                    }
                }

                reader.Close();

                ClassSach bookReading = null;

                if (lists.Count > 0)
                {
                    bookReading = lists[0];
                }
                else
                {
                    cmd = procedure.selectBooksWithFilter("{\"ID\":\""+ ID + "\"}", "", 1, 0);
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        ClassSach sach = new ClassSach(Convert.ToInt32(reader["ID"]), reader["name"].ToString(), reader["author"].ToString(), reader["categoryName"].ToString(), reader["categoryKey"].ToString(), reader["content"].ToString(), reader["imgSrc"].ToString(), Convert.ToInt32(reader["like"]), Convert.ToInt32(reader["view"]));
                        bookReading = sach;
                    }
                    cmd.Cancel();
                    reader.Close();
                }

                if(bookReading != null)
                {
                    Render_data(bookReading, like);
                }
            }
        }

        protected void Render_data(ClassSach book,bool like)
        {
            StringBuilder btn_like = new StringBuilder();
            StringBuilder categoryLink = new StringBuilder("<a href='/view/DetailPage.html?category=" + book.category.value + "' class='text_link'>" + book.category.key + "</a>");
            storyContent.Text = book.content;
            category.Text = categoryLink.ToString();
            breadBookName.Text = book.name;
            bookName.Text = book.name;
            author.Text = "Tác giả: " + book.author;
            if (like)
            {
                btn_like.Append("<button class='btn btn_unlike' type='button' onclick='handleAction(event,\"unlike\")'><i class='fa fa-heart-crack'></i> Bỏ thích</button>");
            }
            else
            {
                btn_like.Append("<button class='btn btn_like' type='button' onclick='handleAction(event,\"like\")'><i class='fa fa-heart'></i> Thích</button>");
            }
            btnLike.Text = btn_like.ToString();
            Session["BookReading"] = book;
        }

        protected void setHistoryAction(ClassSach book, string action = "view", int userID = 0)
        {
            ErrorResponse errMsg;
            try
            {
                if (book.id > 0)
                {
                    string queryAction = action;
                    string setUpdate = "[view] = " + (book.view + 1);
                    if (action == "like")
                    {
                        setUpdate = "[like] = " + (book.like + 1);
                        queryAction = "unlike";
                    }
                    else if (action == "unlike")
                    {
                        if(book.like - 1 < 0)
                        {
                            throw new Exception("Like < 0");
                        }else { 
                            setUpdate = "[like] = " + (book.like - 1);
                            queryAction = "like";
                        }
                    }

                    if (userID > 0)
                    {

                        SqlCommand cmd = procedure.selectHistoryActionWithBook(0, userID, book.id, queryAction);

                        if (cmd == null) throw new Exception("System Error!");

                        SqlDataReader reader = cmd.ExecuteReader();
                        
                        cmd.Cancel();

                        if (reader.Read())
                        {
                            int ID = Convert.ToInt32(reader["ID"]);

                            cmd = procedure.updateHistoryActionWithBook(ID, action);

                        }else
                        {
                            cmd = procedure.insertHistoryActionWithBook(userID, book.id, action);
                        }

                        reader.Close();
                        cmd.ExecuteNonQuery();
                        cmd.Cancel();
                    }
                    

                    SqlCommand updateBook = new SqlCommand("update Books set " + setUpdate + ", [updatedAt] = (GETDATE()) where ID = " + book.id, sqlConnection);
                    updateBook.ExecuteNonQuery();

                    updateBook.Cancel();
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;    
            }
            Response.End();
        }

        protected int checkToken()
        {
            string token = Session["Authorization"]?.ToString();

            if (string.IsNullOrEmpty(token))
            {
                token = Request.Headers["Authorization"];
            }

            if (!string.IsNullOrEmpty(token))
            {
                Authorization authClient = new Authorization(token);
                string tokenInfo = authClient.decode(token);
                string[] userInfo = tokenInfo.Split('-');
                string username = userInfo[0];
                string expriedAt = userInfo[1];
                string password = userInfo[2];
                if (!String.IsNullOrEmpty(username) || !String.IsNullOrEmpty(password) && DateTime.UtcNow.CompareTo(DateTime.Parse(expriedAt)) < 0)
                {
                    SqlCommand cmd = procedure.selectAuthWithEmail(username, password);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        bool locked = (bool)reader["locked"];
                        int ID = Convert.ToInt32(reader["ID"]);
                        reader.Close();

                        if (!locked)
                        {
                            try
                            {
                                return ID;
                            }catch (Exception) {
                                return 0;
                            }
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
            return 0;
        }
    }
}