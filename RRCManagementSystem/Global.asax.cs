using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace RRCManagementSystem
{
    public class Global : System.Web.HttpApplication
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        /// <summary>
        /// Runs once when the application starts
        /// </summary>
        protected void Application_Start(object sender, EventArgs e)
        {
            // Cache initial maintenance mode value
            Application["MaintenanceMode"] = GetMaintenanceModeFromDB();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // Optional: run logic on session start
        }

        /// <summary>
        /// Runs when Session is available
        /// </summary>
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx == null || ctx.Session == null) return;

                string path = ctx.Request.Url.AbsolutePath.ToLower();

                // ======= STEP 1: Maintenance Mode Enforcement =======
                string[] allowedPages = new string[]
                {
                    "/systemsettings.aspx",
                    "/maintenance.aspx",
                    "/login.aspx",
                    "/verifytotp.aspx",
                    "/rootdashboard.aspx",
                    "/webresource.axd",
                    "/scriptresource.axd"
                };

                bool isAllowedPage = allowedPages.Any(page => path.IndexOf(page, StringComparison.OrdinalIgnoreCase) >= 0);

                bool isRootAdmin = ctx.Session["Role"] != null &&
                                   string.Equals(ctx.Session["Role"].ToString(), "RootAdmin", StringComparison.OrdinalIgnoreCase);

                if (!isAllowedPage && !isRootAdmin)
                {
                    string maintenanceMode = Application["MaintenanceMode"]?.ToString() ?? "false";
                    if (string.Equals(maintenanceMode, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Response.Redirect("~/Maintenance.aspx", false);
                        ctx.ApplicationInstance.CompleteRequest();
                        return; // Stop here if in maintenance
                    }
                }

                // ======= STEP 2: Single-Session Enforcement =======
                // Skip login/logout pages
                if (path.Contains("login.aspx") || path.Contains("logout.aspx") || path.Contains("error")) return;

                if (ctx.Session["SessionID"] != null)
                {
                    Guid currentSessionID;
                    if (!Guid.TryParse(ctx.Session["SessionID"].ToString(), out currentSessionID))
                        return;

                    int id;
                    string sql;

                    // ✅ Determine which table to check
                    if (ctx.Session["ClientID"] != null)
                    {
                        // Logged in as Client
                        id = Convert.ToInt32(ctx.Session["ClientID"]);
                        sql = "SELECT CurrentSessionID FROM dbo.Clients WHERE ClientID = @ID";
                    }
                    else if (ctx.Session["UserID"] != null)
                    {
                        // Logged in as Admin/SuperAdmin/Inspector
                        id = Convert.ToInt32(ctx.Session["UserID"]);
                        sql = "SELECT CurrentSessionID FROM dbo.Users WHERE UserID = @ID";
                    }
                    else
                    {
                        return; // No valid session
                    }

                    // ✅ Now check the correct table
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", id);
                            var dbSession = cmd.ExecuteScalar();

                            if (dbSession == null || (Guid)dbSession != currentSessionID)
                            {
                                ctx.Session.Clear();
                                ctx.Session.Abandon();
                                ctx.Response.Redirect("~/Login.aspx?msg=loggedOutByAnotherDevice", true);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Application_AcquireRequestState Error: " + ex.Message);
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // Optional authentication logic
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Optional global error handling
        }

        protected void Session_End(object sender, EventArgs e)
        {
            // Optional logic when session ends
        }

        protected void Application_End(object sender, EventArgs e)
        {
            // Optional logic when application stops
        }

        /// <summary>
        /// Reads maintenance mode value from the database
        /// </summary>
        private string GetMaintenanceModeFromDB()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT SettingValue FROM SettingSystem WHERE SettingKey='MaintenanceMode'", conn))
                    {
                        var result = cmd.ExecuteScalar()?.ToString();
                        return string.Equals(result, "true", StringComparison.OrdinalIgnoreCase) ? "true" : "false";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetMaintenanceModeFromDB Error: " + ex.Message);
                return "false";
            }
        }


        /// <summary>
        /// Refresh the maintenance mode cache immediately
        /// Call this after toggling the checkbox in SystemSettings.aspx
        /// </summary>
        public static void RefreshMaintenanceModeCache()
        {
            try
            {
                // Use Application instead of ApplicationInstance
                var app = HttpContext.Current?.Application;
                if (app != null)
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT SettingValue FROM SettingSystem WHERE SettingKey = 'MaintenanceMode'", conn))
                        {
                            var result = cmd.ExecuteScalar()?.ToString();
                            string newState = string.Equals(result, "true", StringComparison.OrdinalIgnoreCase) ? "true" : "false";

                            // ✅ Update application-wide cache
                            app["MaintenanceMode"] = newState;

                            // Debug logging for testing
                            System.Diagnostics.Debug.WriteLine("MaintenanceMode updated to: " + newState);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log errors but don't break application flow
                System.Diagnostics.Debug.WriteLine("RefreshMaintenanceModeCache Error: " + ex.Message);
            }
        }

    }
}
