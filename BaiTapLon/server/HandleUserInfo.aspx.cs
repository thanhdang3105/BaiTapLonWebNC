using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BaiTapLon.server
{
    public partial class HandleUserInfo : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString);
        StoredProcedure procedure;
        ErrorResponse errMsg = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                con.Open();
                procedure = new StoredProcedure(con);
            }catch (Exception ex)
            {
                errMsg = new ErrorResponse("System Error!");
                Response.Write(JsonConvert.SerializeObject(errMsg));
                Response.End();
            }

            string action = Request.QueryString["action"];
            string sort = Request.QueryString["sort"];
            int limit = Convert.ToInt32(Request.QueryString["limit"]);
            int skip = Convert.ToInt32(Request.QueryString["skip"]);

            int userID = checkToken();
            switch (action)
            {
                case "updateUser":
                    updateUser(Request.Form, userID);
                    break;
                case "getUserReadBook":
                    getHistoryUserWithBook(userID,"view",sort,limit,skip);
                    break;
                case "getUserLikeBook":
                    getHistoryUserWithBook(userID,"like",sort,limit,skip);
                    break;
                default:
                    Response.StatusCode = 400;
                    Response.Write("{\"msg\":\"Action not supported with route!\"}");
                    Response.End();
                    break;
            }

        }

        protected void getHistoryUserWithBook(int userID, string action, string sort, int limit, int skip)
        {
            try
            {
                SqlCommand cmd = procedure.selectHistoryActionWithBook(0, userID, 0, action, sort, limit, skip);

                SqlDataReader reader = cmd.ExecuteReader();

                List<ClassSach> lists = new List<ClassSach>();

                while (reader.Read())
                {
                    ClassSach sach = new ClassSach(Convert.ToInt32(reader["bookID"]), reader["name"].ToString(), reader["author"].ToString(), reader["categoryName"].ToString(), reader["categoryKey"].ToString(), reader["content"].ToString(), reader["imgSrc"].ToString(), Convert.ToInt32(reader["like"]), Convert.ToInt32(reader["view"]), Convert.ToDateTime(reader["createdAt"]), Convert.ToDateTime(reader["updatedAt"]));
                    lists.Add(sach);
                }

                reader.Close();
                cmd.Cancel();

                cmd = new SqlCommand("SELECT COUNT(ID) AS count FROM HistoryUserBook WHERE [userID] = " + userID + " AND [action] = '" + action + "'",con);

                object count = cmd.ExecuteScalar();

                cmd.Cancel();

                string data = JsonConvert.SerializeObject(lists);

                Response.Write("{\"data\": " + data + ", \"count\": " + count.ToString() + "}");


            }
            catch(Exception ex)
            {
                Response.StatusCode = 500;
                errMsg = new ErrorResponse(ex.Message);
                Response.Write(JsonConvert.SerializeObject(errMsg));
            }
            Response.End();
        }

        protected void updateUser(NameValueCollection form, int userID)
        {
            ErrorResponse errMsg;
            try
            {
                string id = form["id"];
                string name = form["name"];
                string email = form["email"];
                string birthday = form["birthday"];
                string phone = form["phone"];
                string sex = form["sex"];
                string oldPassword = form["oldPassword"];
                string newPassword = form["newPassword"];
                SqlCommand cmd;

                if(Convert.ToInt32(id) != userID)
                {
                    Response.StatusCode = 401;
                    Response.Write("User not access!\nPlease login again.");
                }
                else if (!String.IsNullOrEmpty(oldPassword) && !String.IsNullOrEmpty(newPassword) && !String.IsNullOrEmpty(email))
                {
                    Authorization auth = new Authorization(oldPassword);
                    cmd = procedure.selectAuthWithEmail(email, auth.textEncrypted);
                    if (cmd == null)
                    {
                        throw new Exception("System Error!");
                    }
                    object result = cmd.ExecuteScalar();
                    cmd.Cancel();

                    if (result != null)
                    {
                        auth = new Authorization(newPassword);
                        cmd = procedure.updateAuthPassword((int)result, auth.textEncrypted);

                        cmd.ExecuteNonQuery();
                        string token = auth.generateToken(email, auth.textEncrypted);
                        Session["Authorization"] = token;
                        Response.Write("{\"data\": {\"token\": \"" + token + "\", \"msg\": \"Password is update success!\"}}");
                        cmd.Cancel();
                    }
                    else
                    {
                        Response.StatusCode = 400;
                        errMsg = new ErrorResponse("Password is update failed!");
                        Response.Write(JsonConvert.SerializeObject(errMsg));
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(id) || Convert.ToInt32(id) == 0 || String.IsNullOrEmpty(name) || String.IsNullOrEmpty(birthday) || String.IsNullOrEmpty(phone) || String.IsNullOrEmpty(sex))
                    {
                        Response.StatusCode = 400;
                        errMsg = new ErrorResponse("Invalid Params!");
                        Response.Write(JsonConvert.SerializeObject(errMsg));
                    }
                    else
                    {
                        cmd = procedure.updateUserInfo(Convert.ToInt32(id), name, birthday, phone, sex);

                        if (cmd == null) throw new Exception("System Error!");

                        object result = cmd.ExecuteScalar();
                        cmd.Cancel();
                        if (result != null)
                        {
                            Response.Write("Update succes!");
                        }
                        else
                        {
                            Response.StatusCode = 400;
                            errMsg = new ErrorResponse("Update failed!");
                            Response.Write(JsonConvert.SerializeObject(errMsg));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = new ErrorResponse(ex.Message);
                Response.StatusCode = 500;
                Response.Write(JsonConvert.SerializeObject(errMsg));
            }
            Response.End();
        }

        protected int checkToken()
        {
            string token = Session["Authorization"]?.ToString();
            ErrorResponse errMsg;
            if (!string.IsNullOrEmpty(token))
            {
                Authorization authClient = new Authorization(token);
                string tokenInfo = authClient.decode(token);
                string[] userInfo = tokenInfo.Split('-');
                string username = userInfo[0];
                string expriedAt = userInfo[1];
                string password = userInfo[2];
                if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password) || DateTime.UtcNow.CompareTo(DateTime.Parse(expriedAt)) >= 0)
                {
                    Response.StatusCode = 401;
                    errMsg = new ErrorResponse("Token is wrong or Token Expried!\nPlease ReLogin.");
                    Response.Write(JsonConvert.SerializeObject(errMsg));
                    Response.End();
                }
                else
                {
                    SqlCommand cmd = procedure.selectAuthWithEmail(username, password);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {

                        bool locked = (bool)reader["locked"];

                        int userID = (int)reader["ID"];

                        reader.Close();

                        if (locked)
                        {
                            Response.StatusCode = 401;
                            errMsg = new ErrorResponse("Account has been locked!\nplease contact admin to reopen it.");
                            Response.Write(JsonConvert.SerializeObject(errMsg));
                            Response.End();
                        }
                        return userID;
                    }
                }
            }
            return 0;
        }
    }
}