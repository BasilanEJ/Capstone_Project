using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;

namespace RRCManagementSystem
{
    public partial class HeadTechnician : System.Web.UI.MasterPage
    {
        // Connection string
        private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Authorization / Role Check ---
            string role = (Session["Role"] as string)?.Trim().Replace(" ", "");

            // Redirect to login if session missing or role mismatch
            if (Session["UserID"] == null ||
                string.IsNullOrEmpty(role) ||
                !string.Equals(role, "Headtechnician", StringComparison.OrdinalIgnoreCase))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            int technicianId = Convert.ToInt32(Session["UserID"]);

            // 🔹 1) Check if technician's status is still valid (Active or Available)
            if (!IsTechnicianStatusStillValid(technicianId))
            {
                ForceLogout("Your account status has been changed. Please contact the administrator.");
                return;
            }

            // 🔹 2) Check if this session is still active (Single-session enforcement)
            if (!IsTechnicianSessionValid(technicianId))
            {
                ForceLogout("You were logged out because your account was accessed from another device.");
                return;
            }

            // --- Disable caching to prevent back button access after logout ---
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");

            // --- Initial page setup ---
            if (!IsPostBack)
            {
                LoadTechnicianName();
            }
        }


        /// <summary>
        /// Checks the database to ensure technician's account is still active/available
        /// </summary>
        private bool IsTechnicianStatusStillValid(int technicianId)
        {
            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand("SELECT Status FROM Users WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", technicianId);
                    conn.Open();

                    var status = cmd.ExecuteScalar()?.ToString();

                    // Only allow Active or Available
                    return status != null &&
                           (status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                            status.Equals("Available", StringComparison.OrdinalIgnoreCase));
                }
            }
            catch
            {
                return false; // If there's an error, treat as invalid for safety
            }
        }

        /// <summary>
        /// Checks whether the technician's current session matches the one in the database
        /// </summary>
        private bool IsTechnicianSessionValid(int technicianId)
        {
            if (Session["SessionID"] == null) return false;

            try
            {
                Guid currentSessionID;
                if (!Guid.TryParse(Session["SessionID"].ToString(), out currentSessionID))
                    return false;

                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(
                    "SELECT CurrentSessionID FROM Users WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", technicianId);
                    conn.Open();

                    var dbSession = cmd.ExecuteScalar();

                    // If there's no session or mismatch -> invalid
                    return dbSession != null && (Guid)dbSession == currentSessionID;
                }
            }
            catch
            {
                return false; // Treat any error as invalid to force logout
            }
        }

        /// <summary>
        /// Loads the technician's name and team name into the header labels
        /// </summary>
        private void LoadTechnicianName()
        {
            if (!int.TryParse(Session["UserID"]?.ToString(), out int technicianId))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            try
            {
                using (var conn = new SqlConnection(Cs))
                {
                    conn.Open();

                    // Get technician name
                    using (var cmd = new SqlCommand(
                        "SELECT Name FROM Users WHERE UserID=@UserID AND (Status='Active' OR Status='Available')", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", technicianId);
                        string name = cmd.ExecuteScalar() as string;
                        lblTeamLeaderName.Text = !string.IsNullOrWhiteSpace(name) ? name.Trim() : "Team Leader";
                    }
                }
            }
            catch
            {
                lblTeamLeaderName.Text = "Team Leader";
            }
        }

        /// <summary>
        /// Logs the technician out and clears session/cookies
        /// </summary>
        private void ForceLogout(string message)
        {
            // Clear database session
            ClearDatabaseSession();

            // Clear session
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            // Expire session cookie
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // Expire FormsAuth cookie
            FormsAuthentication.SignOut();
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                Response.Cookies[FormsAuthentication.FormsCookieName].Value = string.Empty;
                Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.UtcNow.AddDays(-1);
            }

            // Store message to show on Login.aspx
            Session["LogoutMessage"] = message;

            SafeRedirect("~/Login.aspx");
        }

        /// <summary>
        /// Clears CurrentSessionID in the database
        /// </summary>
        private void ClearDatabaseSession()
        {
            try
            {
                if (Session["UserID"] != null)
                {
                    using (var conn = new SqlConnection(Cs))
                    using (var cmd = new SqlCommand(
                        "UPDATE Users SET CurrentSessionID = NULL, CurrentSessionAt = NULL WHERE UserID = @UserID", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", Convert.ToInt32(Session["UserID"]));
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Fail silently to avoid blocking logout
            }
        }

        /// <summary>
        /// Logout button click handler
        /// </summary>
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            ForceLogout("You have been logged out successfully.");
        }

        /// <summary>
        /// Redirects safely without ThreadAbortException
        /// </summary>
        private void SafeRedirect(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}