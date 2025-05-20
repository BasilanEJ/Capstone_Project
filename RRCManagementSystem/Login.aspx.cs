using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class Login : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlCaptcha.Visible = false;
                lblMessage.Text = "";
            }

            // Already logged-in user
            if (Session["UserID"] != null && Session["Role"] != null)
            {
                string role = Session["Role"].ToString();
                if (role == "SuperAdmin")
                    Response.Redirect("~/SuperAdminDashboard.aspx", false);
                else if (role == "Inspector")
                    Response.Redirect("~/InspectorDashboard.aspx", false);
                else
                    Response.Redirect("~/Dashboard.aspx", false);

                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Logged-in client
            if (Session["ClientID"] != null)
            {
                Response.Redirect("Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string ip = Request.UserHostAddress;

            if (GetFailedIPAttempts(ip) >= 5)
            {
                lblMessage.Text = "⏳ Too many failed attempts from this IP. Try again later.";
                return;
            }

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "⚠ Please enter both email and password.";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT UserID, Name, Role, PasswordHash, TwoFactorEnabled, Status,
                               FailedAttempts, LockoutUntil
                        FROM Users
                        WHERE Email = @Email";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string status = reader["Status"].ToString();
                        string role = reader["Role"].ToString();
                        string hash = reader["PasswordHash"].ToString();
                        bool is2FAEnabled = reader["TwoFactorEnabled"] != DBNull.Value && Convert.ToBoolean(reader["TwoFactorEnabled"]);
                        int failedAttempts = reader["FailedAttempts"] != DBNull.Value ? Convert.ToInt32(reader["FailedAttempts"]) : 0;
                        object lockoutObj = reader["LockoutUntil"];

                        int userID = Convert.ToInt32(reader["UserID"]);
                        string userName = reader["Name"].ToString();

                        // Lockout check
                        if (lockoutObj != DBNull.Value && Convert.ToDateTime(lockoutObj) > DateTime.Now)
                        {
                            pnlCaptcha.Visible = true;
                            lblMessage.Text = $"⏳ Account locked. Try again after {Convert.ToDateTime(lockoutObj):hh:mm tt}.";
                            reader.Close();
                            return;
                        }

                        // CAPTCHA after 5 failed attempts
                        if (failedAttempts >= 5)
                        {
                            pnlCaptcha.Visible = true;
                            if (!IsCaptchaValid())
                            {
                                lblMessage.Text = "⚠ CAPTCHA verification failed.";
                                return;
                            }
                        }

                        // Password check
                        if (PasswordHelper.VerifyPassword(hash, password))
                        {
                            ResetFailedLogin(userID);
                            LogIPAttempt(ip, true);

                            Session["UserID"] = userID;
                            Session["Role"] = role;
                            Session["Name"] = userName;
                            Session["Email"] = email;

                            AddAuditLog(userID, $"{role} {userName} logged in.");
                            reader.Close();

                            // 2FA Logic (non-Inspector only)
                            if (!is2FAEnabled && role != "Inspector")
                            {
                                Session["Pending2FA_UserID"] = userID;
                                Session["Pending2FA_Email"] = email;
                                Session["Pending2FA_Name"] = userName;
                                Session["Pending2FA_Role"] = role;
                                Response.Redirect("Enable2FA.aspx", false);
                            }
                            else if (is2FAEnabled && role != "Inspector")
                            {
                                Session["Pending2FA_UserID"] = userID;
                                Session["Pending2FA_Email"] = email;
                                Session["Pending2FA_Name"] = userName;
                                Session["Pending2FA_Role"] = role;
                                Response.Redirect("VerifyTOTP.aspx", false);
                            }
                            else
                            {
                                Response.Redirect(role == "SuperAdmin" ? "~/SuperAdminDashboard.aspx" :
                                                  role == "Inspector" ? "~/InspectorDashboard.aspx" :
                                                  "~/Dashboard.aspx", false);
                            }

                            Context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                        else
                        {
                            reader.Close();
                            HandleFailedLogin(userID, failedAttempts);
                            LogIPAttempt(ip, false);

                            int remaining = Math.Max(0, 4 - failedAttempts);
                            lblMessage.Text = $"⚠ Invalid credentials. {remaining} attempt(s) left.";
                            if (failedAttempts + 1 >= 5)
                                pnlCaptcha.Visible = true;
                        }
                    }
                    else
                    {
                        reader.Close();

                        // Check in Clients table
                        string clientQuery = @"SELECT ClientID, Name, PasswordHash, Status FROM Clients WHERE Email = @Email";
                        SqlCommand clientCmd = new SqlCommand(clientQuery, conn);
                        clientCmd.Parameters.AddWithValue("@Email", email);

                        SqlDataReader clientReader = clientCmd.ExecuteReader();

                        if (clientReader.Read())
                        {
                            string status = clientReader["Status"].ToString();
                            string hash = clientReader["PasswordHash"].ToString();
                            int clientId = Convert.ToInt32(clientReader["ClientID"]);
                            string name = clientReader["Name"].ToString();

                            if (status != "Approved")
                            {
                                lblMessage.Text = "⚠ Your account is not approved yet.";
                                clientReader.Close();
                                return;
                            }

                            if (PasswordHelper.VerifyPassword(hash, password))
                            {
                                Session["ClientID"] = clientId;
                                Session["ClientName"] = name;
                                Session["Email"] = email;

                                AddAuditLog(null, $"Client {name} logged in.");
                                clientReader.Close();
                                Response.Redirect("Home.aspx", false);
                                Context.ApplicationInstance.CompleteRequest();
                                return;
                            }
                            else
                            {
                                lblMessage.Text = "⚠ Invalid credentials for client account.";
                            }

                            clientReader.Close();
                        }
                        else
                        {
                            lblMessage.Text = "⚠ Account not found.";
                        }
                    }


                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error during login: " + ex.Message;
            }
        }

        private bool IsCaptchaValid()
        {
            string response = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(response)) return false;

            using (var client = new WebClient())
            {
                string secret = "6LdFpz4rAAAAAF33FYq5f39pW0uUe6QNI4XgcWAv"; // Replace with your actual secret key
                string result = client.DownloadString($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}");
                return result.Contains("\"success\": true");
            }
        }

        private int GetFailedIPAttempts(string ip)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT COUNT(*) FROM LoginAttempts WHERE IPAddress = @IP AND IsSuccess = 0 AND AttemptTime > DATEADD(MINUTE, -10, GETDATE())";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IP", ip);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        private void LogIPAttempt(string ip, bool success)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO LoginAttempts (IPAddress, IsSuccess) VALUES (@IP, @Success)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IP", ip);
                cmd.Parameters.AddWithValue("@Success", success);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void HandleFailedLogin(int userId, int currentAttempts)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    UPDATE Users
                    SET FailedAttempts = FailedAttempts + 1,
                        LockoutUntil = CASE
                            WHEN FailedAttempts + 1 >= 5 THEN DATEADD(MINUTE, 5, GETDATE())
                            ELSE NULL
                        END
                    WHERE UserID = @UserID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void ResetFailedLogin(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Users SET FailedAttempts = 0, LockoutUntil = NULL WHERE UserID = @UserID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void AddAuditLog(int? userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@UserID, @Action, GETDATE())";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", (object)userID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Action", action);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
