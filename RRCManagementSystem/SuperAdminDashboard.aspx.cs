using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class SuperAdminDashboard : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "SuperAdmin")
            {
                Response.Redirect("~/Login.aspx", false);  // ✅ Add false parameter
                Context.ApplicationInstance.CompleteRequest();  // ✅ Add this line
                return;
            }

            if (!IsPostBack)
            {
                LoadDashboardStats();
                LoadRecentAuditLogs(10);
                CheckFailedLoginThreshold();
                LoadSummaryCards();
            }
            else  // ✅ Use else instead of separate if(IsPostBack)
            {
                // Handle unlock action from JavaScript BEFORE loading data
                HandleUnlockFromHiddenFields();
            }

            // ✅ Always load lockout data (both initial load and postback)
            LoadLockoutStatistics();
            LoadLockedUsers();
            LoadLockedDevices();
            LoadRecentFailedAttempts();
        }

        private void LoadSummaryCards()
        {
            // Example logic — replace with your existing count queries
            lblLockedUsers.Text = GetLockedUsersCount().ToString();
            lblLockedDevices.Text = GetLockedDevicesCount().ToString();
            lblLockedIPs.Text = GetLockedIPsCount().ToString();
            lblFailedAttemptsHour.Text = GetRecentFailedAttemptsCount().ToString();
        }


        protected void timerRefreshSummary_Tick(object sender, EventArgs e)
        {
            LoadSummaryCards(); // Just reload the counts
            LoadDashboardStats();
            upDashboardSummary.Update();
        }
        private void LoadDashboardStats()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spDashboard_GetStats", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            lblTotalAdmins.Text = Convert.ToString(rdr["TotalUsers"] ?? "0");
                        }
                        else
                        {
                            lblTotalAdmins.Text = "0";
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblTotalAdmins.Text = "0";
                    LogError("LoadDashboardStats", ex);
                }
            }
        }

        private void LoadRecentAuditLogs(int topN)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAuditLogs_GetRecent", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = topN;

                var dt = new DataTable();
                try
                {
                    da.Fill(dt);
                    gvAuditLogs.DataSource = dt;
                    gvAuditLogs.DataBind();
                }
                catch (Exception ex)
                {
                    LogError("LoadRecentAuditLogs", ex);
                }
            }
        }

        private void CheckFailedLoginThreshold()
        {
            const int windowMinutes = 10;
            const int threshold = 50;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spLoginAttempt_CountRecentFailures", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = DBNull.Value;
                cmd.Parameters.Add("@WindowMinutes", SqlDbType.Int).Value = windowMinutes;

                try
                {
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    int failedCount = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);

                    hfShowModal.Value = failedCount >= threshold ? "1" : "0";
                }
                catch (Exception ex)
                {
                    LogError("CheckFailedLoginThreshold", ex);
                    hfShowModal.Value = "0";
                }
            }
        }


        private void LoadLockoutStatistics()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Count locked users (Users table)
                    using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM dbo.Users 
                WHERE LockoutUntil > GETDATE()", conn))
                    {
                        lblLockedUsers.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Count locked users (Clients table)
                    int lockedClients = 0;
                    using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM dbo.Clients 
                WHERE LockoutUntil > GETDATE()", conn))
                    {
                        lockedClients = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    }

                    // Add clients to total
                    lblLockedUsers.Text = (Convert.ToInt32(lblLockedUsers.Text) + lockedClients).ToString();

                    // ✅ Count locked devices - using IsLocked flag only
                    using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM dbo.DeviceLockout 
                WHERE IsLocked = 1", conn))
                    {
                        var count = cmd.ExecuteScalar();
                        lblLockedDevices.Text = count?.ToString() ?? "0";
                        System.Diagnostics.Debug.WriteLine($"[LoadLockoutStatistics] Locked devices count: {lblLockedDevices.Text}");
                    }

                    // Count locked IPs from IPAddressLockout table
                    using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM dbo.IPAddressLockout 
                WHERE IsLocked = 1 AND LockedUntil > GETDATE()", conn))
                    {
                        lblLockedIPs.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Count failed attempts in last hour from LoginAttemptsByUserIP
                    using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM dbo.LoginAttemptsByUserIP 
                WHERE IsSuccess = 0 
                AND AttemptTime > DATEADD(HOUR, -1, GETDATE())", conn))
                    {
                        lblFailedAttemptsHour.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("LoadLockoutStatistics", ex);
                lblLockedUsers.Text = "0";
                lblLockedDevices.Text = "0";
                lblLockedIPs.Text = "0";
                lblFailedAttemptsHour.Text = "0";
            }
        }

        private void LoadLockedUsers()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var dt = new DataTable();
                    dt.Columns.Add("UserID", typeof(int));
                    dt.Columns.Add("Email", typeof(string));
                    dt.Columns.Add("EmailHash", typeof(string));
                    dt.Columns.Add("AccountType", typeof(string));
                    dt.Columns.Add("FailedAttempts", typeof(int));
                    dt.Columns.Add("LockedUntil", typeof(DateTime));
                    dt.Columns.Add("MinutesRemaining", typeof(string));
                    dt.Columns.Add("LastIPAddress", typeof(string));

                    // Load from Users table
                    using (var cmd = new SqlCommand(@"
        SELECT 
            u.UserID,
            u.Email,
            u.FailedAttempts,
            u.LockoutUntil,
            CASE 
                WHEN u.LockoutUntil > GETDATE() 
                THEN DATEDIFF(MINUTE, GETDATE(), u.LockoutUntil)
                ELSE 0 
            END AS MinutesRemaining
        FROM dbo.Users u
        WHERE u.LockoutUntil > GETDATE()
        ORDER BY u.LockoutUntil DESC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = dt.NewRow();
                            row["UserID"] = reader["UserID"];
                            row["Email"] = reader["Email"]?.ToString() ?? "N/A";
                            row["EmailHash"] = "N/A"; // Will be generated after decryption
                            row["AccountType"] = "Admin/User";
                            row["FailedAttempts"] = reader["FailedAttempts"];
                            row["LockedUntil"] = reader["LockoutUntil"];

                            int minutes = Convert.ToInt32(reader["MinutesRemaining"]);
                            row["MinutesRemaining"] = minutes > 0 ? $"{minutes} min" : "Expired";

                            row["LastIPAddress"] = "N/A";

                            dt.Rows.Add(row);
                        }
                    }

                    // Load from Clients table
                    using (var cmd = new SqlCommand(@"
        SELECT 
            c.ClientID,
            c.EmailEnc,
            c.EmailHash,
            c.FailedAttempts,
            c.LockoutUntil,
            CASE 
                WHEN c.LockoutUntil > GETDATE() 
                THEN DATEDIFF(MINUTE, GETDATE(), c.LockoutUntil)
                ELSE 0 
            END AS MinutesRemaining
        FROM dbo.Clients c
        WHERE c.LockoutUntil > GETDATE()
        ORDER BY c.LockoutUntil DESC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = dt.NewRow();
                            row["UserID"] = reader["ClientID"];
                            row["Email"] = reader["EmailEnc"]?.ToString() ?? "N/A";
                            row["EmailHash"] = reader["EmailHash"]?.ToString() ?? "N/A";
                            row["AccountType"] = "Client";
                            row["FailedAttempts"] = reader["FailedAttempts"];
                            row["LockedUntil"] = reader["LockoutUntil"];

                            int minutes = Convert.ToInt32(reader["MinutesRemaining"]);
                            row["MinutesRemaining"] = minutes > 0 ? $"{minutes} min" : "Expired";

                            row["LastIPAddress"] = "N/A";

                            dt.Rows.Add(row);
                        }
                    }

                    // Decrypt AES-256 encrypted emails (SAME AS RECENT FAILED ATTEMPTS)
                    foreach (DataRow row in dt.Rows)
                    {
                        string encEmail = row["Email"]?.ToString();
                        if (!string.IsNullOrEmpty(encEmail))
                        {
                            try
                            {
                                string decryptedEmail = RRCManagementSystem.Helpers.AESHelper.DecryptEmail(encEmail);
                                row["Email"] = decryptedEmail;

                                // Generate EmailHash for Users (Clients already have it)
                                if (row["EmailHash"].ToString() == "N/A")
                                {
                                    using (var sha256 = System.Security.Cryptography.SHA256.Create())
                                    {
                                        byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(decryptedEmail.ToLower()));
                                        row["EmailHash"] = BitConverter.ToString(hashBytes).Replace("-", "");
                                    }
                                }
                            }
                            catch
                            {
                                row["Email"] = "[Decryption Error]";
                            }
                        }
                        else
                        {
                            row["Email"] = "[Unknown]";
                        }
                    }

                    // Get Last IP Address for each user using their EmailHash
                    foreach (DataRow row in dt.Rows)
                    {
                        string emailHash = row["EmailHash"]?.ToString();
                        if (!string.IsNullOrEmpty(emailHash) && emailHash != "N/A")
                        {
                            using (var cmd = new SqlCommand(@"
                        SELECT TOP 1 IPAddress 
                        FROM dbo.LoginAttemptsByUserIP 
                        WHERE EmailHash = @EmailHash 
                        ORDER BY AttemptTime DESC", conn))
                            {
                                cmd.Parameters.AddWithValue("@EmailHash", emailHash);
                                var lastIP = cmd.ExecuteScalar()?.ToString();
                                if (!string.IsNullOrEmpty(lastIP))
                                {
                                    row["LastIPAddress"] = lastIP;
                                }
                            }
                        }
                    }

                    gvLockedUsers.DataSource = dt;
                    gvLockedUsers.DataBind();

                    System.Diagnostics.Debug.WriteLine($"[LoadLockedUsers] Total locked users displayed: {dt.Rows.Count}");
                }
            }
            catch (Exception ex)
            {
                LogError("LoadLockedUsers", ex);
            }
        }

        private void LoadLockedDevices()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var dt = new DataTable();

                    // Define columns that match the GridView
                    dt.Columns.Add("DeviceLockoutID", typeof(int));
                    dt.Columns.Add("BrowserName", typeof(string));
                    dt.Columns.Add("Platform", typeof(string));
                    dt.Columns.Add("FailedAttempts", typeof(int));
                    dt.Columns.Add("LockedUntil", typeof(string));  // Pre-formatted string
                    dt.Columns.Add("MinutesRemaining", typeof(string));  // Pre-formatted string
                    dt.Columns.Add("LastIPAddress", typeof(string));

                    // Query locked devices
                    using (var cmd = new SqlCommand(@"
                SELECT 
                    DeviceLockoutID,
                    ISNULL(BrowserName, 'Unknown') AS BrowserName,
                    ISNULL(Platform, 'Unknown') AS Platform,
                    ISNULL(FailedAttempts, 0) AS FailedAttempts,
                    LockedUntil,
                    CASE 
                        WHEN LockedUntil IS NOT NULL AND LockedUntil > GETDATE() 
                        THEN DATEDIFF(MINUTE, GETDATE(), LockedUntil)
                        ELSE 0 
                    END AS MinutesRemaining,
                    ISNULL(LastIPAddress, 'N/A') AS LastIPAddress
                FROM dbo.DeviceLockout
                WHERE IsLocked = 1
                ORDER BY UpdatedAt DESC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = dt.NewRow();

                            row["DeviceLockoutID"] = reader["DeviceLockoutID"];
                            row["BrowserName"] = reader["BrowserName"];
                            row["Platform"] = reader["Platform"];
                            row["FailedAttempts"] = reader["FailedAttempts"];

                            // Format LockedUntil
                            if (reader["LockedUntil"] != DBNull.Value)
                            {
                                DateTime lockedUntil = Convert.ToDateTime(reader["LockedUntil"]);
                                row["LockedUntil"] = lockedUntil.ToString("MMM dd, hh:mm tt");
                            }
                            else
                            {
                                row["LockedUntil"] = "Indefinite";
                            }

                            // Format MinutesRemaining
                            int minutes = Convert.ToInt32(reader["MinutesRemaining"]);
                            if (minutes > 60)
                            {
                                row["MinutesRemaining"] = $"{minutes / 60}h {minutes % 60}m";
                            }
                            else if (minutes > 0)
                            {
                                row["MinutesRemaining"] = $"{minutes}m";
                            }
                            else
                            {
                                row["MinutesRemaining"] = "Permanent";
                            }

                            row["LastIPAddress"] = reader["LastIPAddress"];

                            dt.Rows.Add(row);
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"[LoadLockedDevices] Found {dt.Rows.Count} locked devices");

                    gvLockedDevices.DataSource = dt;
                    gvLockedDevices.DataBind();

                    System.Diagnostics.Debug.WriteLine($"[LoadLockedDevices] GridView bound successfully. Rows: {gvLockedDevices.Rows.Count}");
                }
            }
            catch (Exception ex)
            {
                LogError("LoadLockedDevices", ex);
                System.Diagnostics.Debug.WriteLine($"[LoadLockedDevices] ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LoadLockedDevices] Stack Trace: {ex.StackTrace}");
            }
        }

        // ===== FIXED: LoadRecentFailedAttempts - Just show what we have =====

        private void LoadRecentFailedAttempts()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
            SELECT TOP 20
                a.AttemptTime,
                a.IPAddress,
                a.EmailHash,
                a.UserAgent,
                ISNULL(u.Email, c.EmailEnc) AS EncryptedEmail,
                CASE 
                    WHEN u.Email IS NOT NULL THEN 'Admin/User'
                    WHEN c.EmailEnc IS NOT NULL THEN 'Client'
                    ELSE 'Unknown'
                END AS AccountType
            FROM dbo.LoginAttemptsByUserIP a
            LEFT JOIN dbo.Users u ON a.EmailHash = u.EmailHash
            LEFT JOIN dbo.Clients c ON a.EmailHash = c.EmailHash
            WHERE a.IsSuccess = 0
              AND a.AttemptTime >= DATEADD(HOUR, -1, GETDATE()) -- ✅ Limit to last 1 hour
            ORDER BY a.AttemptTime DESC", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);

                    // Decrypt AES-256 encrypted emails
                    foreach (DataRow row in dt.Rows)
                    {
                        string encEmail = row["EncryptedEmail"]?.ToString();
                        if (!string.IsNullOrEmpty(encEmail))
                        {
                            try
                            {
                                row["EncryptedEmail"] = RRCManagementSystem.Helpers.AESHelper.DecryptEmail(encEmail);
                            }
                            catch
                            {
                                row["EncryptedEmail"] = "[Decryption Error]";
                            }
                        }
                        else
                        {
                            row["EncryptedEmail"] = "[Unknown]";
                        }
                    }

                    gvRecentFailures.DataSource = dt;
                    gvRecentFailures.DataBind();
                }
            }
            catch (Exception ex)
            {
                LogError("LoadRecentFailedAttempts", ex);
            }
        }



        private void HandleUnlockFromHiddenFields()
        {
            string unlockAction = hfUnlockAction.Value;
            string unlockID = hfUnlockID.Value;

            if (!string.IsNullOrEmpty(unlockAction) && !string.IsNullOrEmpty(unlockID))
            {
                if (unlockAction == "user")
                {
                    UnlockUser(Convert.ToInt32(unlockID));
                    hfUnlockAction.Value = string.Empty;
                    hfUnlockID.Value = string.Empty;
                    ScriptManager.RegisterStartupScript(this, GetType(), "unlockSuccess",
                        "Swal.fire('Success', 'User unlocked successfully!', 'success');", true);
                }
                else if (unlockAction == "device")
                {
                    UnlockDevice(unlockID);
                    hfUnlockAction.Value = string.Empty;
                    hfUnlockID.Value = string.Empty;
                    ScriptManager.RegisterStartupScript(this, GetType(), "unlockSuccess",
                        "Swal.fire('Success', 'Device unlocked successfully!', 'success');", true);
                }
            }
        }



        protected void gvLockedUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Not used - handled by JavaScript + hidden fields
        }

        protected void gvLockedDevices_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Not used - handled by JavaScript + hidden fields
        }

        protected void btnUnlockAllUsers_Click(object sender, EventArgs e)
        {
            try
            {
                int totalCount = 0;

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Unlock staff/admin users
                    using (var cmd = new SqlCommand(@"
                        UPDATE dbo.Users 
                        SET FailedAttempts = 0, LockoutUntil = NULL 
                        WHERE LockoutUntil > GETDATE()", conn))
                    {
                        totalCount += cmd.ExecuteNonQuery();
                    }

                    // Unlock clients
                    using (var cmd = new SqlCommand(@"
                        UPDATE dbo.Clients 
                        SET FailedAttempts = 0, LockoutUntil = NULL 
                        WHERE LockoutUntil > GETDATE()", conn))
                    {
                        totalCount += cmd.ExecuteNonQuery();
                    }
                }

                ShowSuccessMessage($"Successfully unlocked {totalCount} user(s)!");

                // ✅ FIX: Data already reloaded at the beginning of Page_Load on postback
            }
            catch (Exception ex)
            {
                LogError("UnlockAllUsers", ex);
                ShowErrorMessage("Error unlocking users. Please try again.");
            }
        }

        protected void btnUnlockAllDevices_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
            UPDATE dbo.DeviceLockout 
            SET IsLocked = 0, FailedAttempts = 0, LockedUntil = NULL 
            WHERE IsLocked = 1", conn))
                {
                    conn.Open();
                    int count = cmd.ExecuteNonQuery();

                    ShowSuccessMessage($"Successfully unlocked {count} device(s)!");

                    // ✅ FIX: Data already reloaded at the beginning of Page_Load on postback
                }
            }
            catch (Exception ex)
            {
                LogError("UnlockAllDevices", ex);
                ShowErrorMessage("Error unlocking devices. Please try again.");
            }
        }

        private void UnlockUser(int userId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    int totalRowsAffected = 0;

                    // Try to unlock from Users table
                    using (var cmd = new SqlCommand(@"
                UPDATE dbo.Users 
                SET LockoutUntil = NULL, 
                    FailedAttempts = 0 
                WHERE UserID = @UserID 
                  AND LockoutUntil IS NOT NULL", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        totalRowsAffected += rowsAffected;
                        System.Diagnostics.Debug.WriteLine($"[UnlockUser] Users table - UserID: {userId}, Rows affected: {rowsAffected}");
                    }

                    // Try to unlock from Clients table (in case it's a client)
                    using (var cmd = new SqlCommand(@"
                UPDATE dbo.Clients 
                SET LockoutUntil = NULL, 
                    FailedAttempts = 0 
                WHERE ClientID = @ClientID 
                  AND LockoutUntil IS NOT NULL", conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        totalRowsAffected += rowsAffected;
                        System.Diagnostics.Debug.WriteLine($"[UnlockUser] Clients table - ClientID: {userId}, Rows affected: {rowsAffected}");
                    }

                    System.Diagnostics.Debug.WriteLine($"[UnlockUser] Total rows unlocked: {totalRowsAffected}");
                }
            }
            catch (Exception ex)
            {
                LogError("UnlockUser", ex);
                ScriptManager.RegisterStartupScript(this, GetType(), "unlockError",
                    "Swal.fire('Error', 'Failed to unlock user. Please try again.', 'error');", true);
            }
        }

        private void UnlockDevice(string deviceLockoutID)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
            UPDATE dbo.DeviceLockout 
            SET IsLocked = 0, 
                FailedAttempts = 0, 
                LockedUntil = NULL 
            WHERE DeviceLockoutID = @DeviceLockoutID", conn))
                {
                    cmd.Parameters.AddWithValue("@DeviceLockoutID", Convert.ToInt32(deviceLockoutID));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LogError("UnlockDevice", ex);
            }
        }

        private void ShowSuccessMessage(string message)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'success',
                    title: 'Success',
                    text: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#198754',
                    timer: 2000,
                    showConfirmButton: false
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "successMessage", script, true);
        }

        protected void TimerLockedUsers_Tick(object sender, EventArgs e)
        {
            LoadLockedUsers(); // same method that binds gvLockedUsers
        }

        protected void TimerLockedDevices_Tick(object sender, EventArgs e)
        {
            LoadLockedDevices(); // your existing method that binds gvLockedDevices
        }

        protected void TimerRecentFailures_Tick(object sender, EventArgs e)
        {
            LoadRecentFailedAttempts(); // your existing method that binds gvRecentFailures
        }

        protected void TimerAuditLogs_Tick(object sender, EventArgs e)
        {
            LoadRecentAuditLogs(10); // Reload the audit logs
        }

        private void ShowErrorMessage(string message)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'error',
                    title: 'Error',
                    text: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#212529'
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "errorMessage", script, true);
        }


      

        // ===== Summary Count Helper Methods =====

        private int GetLockedUsersCount()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
        SELECT 
            (SELECT COUNT(*) FROM dbo.Users WHERE LockoutUntil > GETDATE())
          + (SELECT COUNT(*) FROM dbo.Clients WHERE LockoutUntil > GETDATE())", conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }
        }

        private int GetLockedDevicesCount()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.DeviceLockout WHERE IsLocked = 1", conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }
        }

        private int GetLockedIPsCount()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.IPAddressLockout WHERE IsLocked = 1 AND LockedUntil > GETDATE()", conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }
        }

        private int GetRecentFailedAttemptsCount()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
        SELECT COUNT(*) 
        FROM dbo.LoginAttemptsByUserIP 
        WHERE IsSuccess = 0 
          AND AttemptTime > DATEADD(HOUR, -1, GETDATE())", conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }
        }


        private void LogError(string context, Exception ex)
        {
            try
            {
                string logPath = Server.MapPath("~/Logs/ErrorLog.txt");
                string message = $"{DateTime.Now:u}: [{context}] {ex}{Environment.NewLine}";
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));
                System.IO.File.AppendAllText(logPath, message);
            }
            catch
            {
                // swallow logging failures
            }
        }
    }
}