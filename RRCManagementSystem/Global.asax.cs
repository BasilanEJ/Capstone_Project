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
                string path = HttpContext.Current.Request.Url.AbsolutePath.ToLower();

                // Pages allowed even during maintenance
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

                // Robust matching using IndexOf for case-insensitive check
                bool isAllowedPage = allowedPages.Any(page => path.IndexOf(page, StringComparison.OrdinalIgnoreCase) >= 0);

                // Check if current user is RootAdmin (ensure Session is not null)
                bool isRootAdmin = HttpContext.Current.Session != null &&
                                   HttpContext.Current.Session["Role"] != null &&
                                   string.Equals(HttpContext.Current.Session["Role"].ToString(), "RootAdmin", StringComparison.OrdinalIgnoreCase);

                // If not allowed page and not RootAdmin, enforce maintenance
                if (!isAllowedPage && !isRootAdmin)
                {
                    string maintenanceMode = Application["MaintenanceMode"]?.ToString() ?? "false";

                    if (string.Equals(maintenanceMode, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        HttpContext.Current.Response.Redirect("~/Maintenance.aspx", false);
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
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
                var app = HttpContext.Current?.ApplicationInstance;
                if (app != null)
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT SettingValue FROM SettingSystem WHERE SettingKey='MaintenanceMode'", conn))
                        {
                            var result = cmd.ExecuteScalar()?.ToString();
                            app.Application["MaintenanceMode"] = string.Equals(result, "true", StringComparison.OrdinalIgnoreCase) ? "true" : "false";
                        }
                    }
                }
            }
            catch
            {
                // Ignore caching errors to avoid breaking requests
            }
        }
    }
}
