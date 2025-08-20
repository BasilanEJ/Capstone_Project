    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Net;
    using System.Web;
    using System.Web.UI;

    namespace RRCManagementSystem
    {
        public partial class Login : Page
        {
            private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
            private const int IpWindowMinutes = 10;  // rate-limit window for IP failures

            protected void Page_Load(object sender, EventArgs e)
            {
                // Prevent cached pages (back/forward bypass after logout)
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

                if (!IsPostBack)
                {
                    pnlCaptcha.Visible = false;
                    lblMessage.Text = "";
                }

                // Auto-redirect only if fully authenticated (after 2FA)
                if (Session["IsAuthenticated"] as bool? == true &&
                    Session["UserID"] != null && Session["Role"] != null)
                {
                    string role = Session["Role"].ToString();
                    Response.Redirect(
                        role == "SuperAdmin" ? "~/SuperAdminDashboard.aspx"
                      : role == "Inspector" ? "~/InspectorDashboard.aspx"
                                             : "~/Dashboard.aspx",
                        false
                    );
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Client already logged in
                if (Session["ClientID"] != null)
                {
                    Response.Redirect("Home.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }

            protected void btnLogin_Click(object sender, EventArgs e)
            {
                string ip = Request.UserHostAddress ?? "";

                if (GetFailedIPAttempts(ip, IpWindowMinutes) >= 5)
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
                    // ========== 1) Try Users (admins/staff) via SP ==========
                    using (var conn = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand("dbo.spAuth_GetUserByEmail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string status = reader["Status"]?.ToString() ?? "";
                                string role = reader["Role"]?.ToString() ?? "";
                                string hash = reader["PasswordHash"]?.ToString();
                                bool is2FAEnabled = reader["TwoFactorEnabled"] != DBNull.Value && Convert.ToBoolean(reader["TwoFactorEnabled"]);
                                int failedAttempts = reader["FailedAttempts"] != DBNull.Value ? Convert.ToInt32(reader["FailedAttempts"]) : 0;
                                object lockoutObj = reader["LockoutUntil"];

                                int userID = Convert.ToInt32(reader["UserID"]);
                                string userName = reader["Name"]?.ToString() ?? "";

                                // Status check
                                if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                                    !status.Equals("Available", StringComparison.OrdinalIgnoreCase))
                                {
                                    lblMessage.Text = "⚠ Your account is not active.";
                                    return;
                                }

                                // Lockout check
                                if (lockoutObj != DBNull.Value && Convert.ToDateTime(lockoutObj) > DateTime.Now)
                                {
                                    pnlCaptcha.Visible = true;
                                    lblMessage.Text = $"⏳ Account locked. Try again after {Convert.ToDateTime(lockoutObj):hh:mm tt}.";
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
                                if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                                {
                                    ResetFailedLogin(userID);   // SP
                                    LogIPAttempt(ip, true);     // SP
                                    AddAuditLog(userID, $"{role} {userName} passed password; 2FA pending."); // SP

                                    if (!role.Equals("Inspector", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // 2FA required -> store pending identity only
                                        Session["Pending2FA_UserID"] = userID;
                                        Session["Pending2FA_Email"] = email;
                                        Session["Pending2FA_Name"] = userName;
                                        Session["Pending2FA_Role"] = role;

                                        // ensure not authenticated yet
                                        Session.Remove("IsAuthenticated");
                                        Session.Remove("UserID");
                                        Session.Remove("Role");
                                        Session.Remove("Name");
                                        Session.Remove("Email");

                                        if (!is2FAEnabled)
                                            Response.Redirect("Enable2FA.aspx", false);
                                        else
                                            Response.Redirect("VerifyTOTP.aspx", false);

                                        Context.ApplicationInstance.CompleteRequest();
                                        return;
                                    }
                                    else
                                    {
                                        // Inspectors skip 2FA per your policy
                                        Session["UserID"] = userID;
                                        Session["Role"] = role;
                                        Session["Name"] = userName;
                                        Session["Email"] = email;
                                        Session["IsAuthenticated"] = true;

                                        Response.Redirect("~/InspectorDashboard.aspx", false);
                                        Context.ApplicationInstance.CompleteRequest();
                                        return;
                                    }
                                }
                                else
                                {
                                    HandleFailedLogin(userID); // SP (increments counter, sets lockout if threshold reached)
                                    LogIPAttempt(ip, false);   // SP

                                    int remaining = Math.Max(0, 4 - failedAttempts);
                                    lblMessage.Text = $"⚠ Invalid credentials. {remaining} attempt(s) left.";
                                    if (failedAttempts + 1 >= 5) pnlCaptcha.Visible = true;
                                    return;
                                }
                            }
                        }
                    }

                    // ========== 2) Try Clients via SP ==========
                    using (var conn = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand("dbo.spAuth_GetClientByEmail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        // Clients.Email is NVARCHAR(255)
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email;
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string status = reader["Status"]?.ToString() ?? "";
                                string hash = reader["PasswordHash"]?.ToString();
                                int clientId = Convert.ToInt32(reader["ClientID"]);
                                string name = reader["Name"]?.ToString() ?? "";

                                if (!status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                                {
                                    lblMessage.Text = "⚠ Your account is not approved yet.";
                                    return;
                                }

                                if (!string.IsNullOrEmpty(hash) && PasswordHelper.VerifyPassword(hash, password))
                                {
                                    Session["ClientID"] = clientId;
                                    Session["ClientName"] = name;
                                    Session["Email"] = email;

                                    AddAuditLog(null, $"Client {name} logged in."); // SP
                                    Response.Redirect("Home.aspx", false);
                                    Context.ApplicationInstance.CompleteRequest();
                                    return;
                                }
                                else
                                {
                                    lblMessage.Text = "⚠ Invalid credentials for client account.";
                                    return;
                                }
                            }
                            else
                            {
                                lblMessage.Text = "⚠ Account not found.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error during login: " + ex.Message;
                }
            }

            /* =========================
               CAPTCHA
               ========================= */
            private bool IsCaptchaValid()
            {
                string response = Request.Form["g-recaptcha-response"];
                if (string.IsNullOrEmpty(response)) return false;

                using (var client = new WebClient())
                {
                    string secret = "6LfmLqwrAAAAALDQW46-uZss3CZStl0xmMyj_GWw"; // TODO: move to config
                    string result = client.DownloadString($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}");
                    return result.Contains("\"success\": true");
                }
            }

            /* =========================
               Stored-proc helpers
               ========================= */

            private int GetFailedIPAttempts(string ip, int windowMinutes)
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spLoginAttempt_CountRecentFailures", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";
                    cmd.Parameters.Add("@WindowMinutes", SqlDbType.Int).Value = windowMinutes;
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            private void LogIPAttempt(string ip, bool success)
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spLoginAttempt_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = ip ?? "";
                    cmd.Parameters.Add("@IsSuccess", SqlDbType.Bit).Value = success;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            private void HandleFailedLogin(int userId)
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAuth_FailAndMaybeLock", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    // Optionally override defaults:
                    // cmd.Parameters.Add("@Threshold", SqlDbType.Int).Value = 5;
                    // cmd.Parameters.Add("@LockoutMinutes", SqlDbType.Int).Value = 5;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            private void ResetFailedLogin(int userId)
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAuth_ResetFailures", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            private void AddAuditLog(int? userID, string action)
            {
                if (userID == null) return;

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = userID.Value;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 255).Value = action ?? "";
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
