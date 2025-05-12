using System;
using System.Configuration;
using System.Data.SqlClient;
using Isopoh.Cryptography.Argon2;

namespace RRCManagementSystem
{
    public partial class Login : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            lblMessage.Text = "";

            // ✅ Already logged-in User (Inspector, Admin, SuperAdmin)
            if (Session["UserID"] != null)
            {
                string role = Session["Role"]?.ToString();
                if (role == "SuperAdmin")
                    Response.Redirect("~/SuperAdminDashboard.aspx");
                else if (role == "Inspector")
                    Response.Redirect("~/InspectorDashboard.aspx");
                else
                    Response.Redirect("~/Dashboard.aspx");
            }

            // ✅ Already logged-in Client
            if (Session["ClientID"] != null)
                Response.Redirect("Home.aspx");
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "⚠ Please enter both email and password.";
                return;
            }

            try
            {
                // ✅ USERS (SuperAdmin, Admin, Inspector)
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT UserID, Name, Role, PasswordHash, TOTPSecret, TwoFactorEnabled, Status FROM Users WHERE Email = @Email";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string status = reader["Status"].ToString();
                        string role = reader["Role"].ToString();
                        string hash = reader["PasswordHash"].ToString();
                        bool is2FAEnabled = Convert.ToBoolean(reader["TwoFactorEnabled"]);

                        if (status != "Active")
                        {
                            lblMessage.Text = "⚠ Account is not active.";
                            reader.Close();
                            return;
                        }

                        if (PasswordHelper.VerifyPassword(hash, password))
                        {
                            int userID = Convert.ToInt32(reader["UserID"]);
                            string userName = reader["Name"].ToString();

                            // ✅ Set unified session variables
                            Session["UserID"] = userID;
                            Session["Role"] = role;
                            Session["Name"] = userName;
                            Session["Email"] = email;
                            Session["Password"] = password;

                            AddAuditLog(userID, $"{role} {userName} logged in.");
                            reader.Close();

                            if (!is2FAEnabled)
                            {
                                switch (role)
                                {
                                    case "SuperAdmin":
                                        Response.Redirect("~/SuperAdminDashboard.aspx", false);
                                        break;
                                    case "Inspector":
                                        Response.Redirect("~/InspectorDashboard.aspx", false);
                                        break;
                                    default:
                                        Response.Redirect("~/Dashboard.aspx", false);
                                        break;
                                }
                            }
                            else
                            {
                                // 2FA flow
                                Session["Pending2FA_UserID"] = userID;
                                Session["Pending2FA_Email"] = email;
                                Session["Pending2FA_Name"] = userName;
                                Session["Pending2FA_Role"] = role;

                                switch (role)
                                {
                                    case "SuperAdmin":
                                        Session["Pending2FA_Redirect"] = "~/SuperAdminDashboard.aspx";
                                        break;
                                    case "Inspector":
                                        Session["Pending2FA_Redirect"] = "~/InspectorDashboard.aspx";
                                        break;
                                    default:
                                        Session["Pending2FA_Redirect"] = "~/Dashboard.aspx";
                                        break;
                                }

                                Response.Redirect("VerifyTOTP.aspx", false);
                            }

                            Context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                        else
                        {
                            lblMessage.Text = "⚠ Invalid email or password.";
                            reader.Close();
                            return;
                        }
                    }
                    reader.Close();
                }

                // ✅ CLIENTS
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT ClientID, Name, PasswordHash, Status FROM Clients WHERE Email = @Email";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string status = reader["Status"].ToString();
                        string hash = reader["PasswordHash"].ToString();

                        if (status != "Approved")
                        {
                            lblMessage.Text = "⚠ Account is not active.";
                            reader.Close();
                            return;
                        }

                        if (PasswordHelper.VerifyPassword(hash, password))
                        {
                            int clientID = Convert.ToInt32(reader["ClientID"]);
                            string name = reader["Name"].ToString();

                            Session["ClientID"] = clientID;
                            Session["Name"] = name;
                            Session["Email"] = email;
                            Session["Role"] = "Client";

                            AddAuditLog(null, $"Client {name} logged in.");
                            reader.Close();

                            Response.Redirect("Home.aspx", false);
                            Context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                        else
                        {
                            lblMessage.Text = "⚠ Invalid email or password.";
                            reader.Close();
                            return;
                        }
                    }
                    else
                    {
                        lblMessage.Text = "⚠ Email not found.";
                        reader.Close();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error during login: " + ex.Message;
            }
        }

        private void AddAuditLog(int? userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (UserID, Action, Timestamp) VALUES (@UserID, @Action, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", (object)userID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Optional: log somewhere else if needed
                    }
                }
            }
        }
    }
}