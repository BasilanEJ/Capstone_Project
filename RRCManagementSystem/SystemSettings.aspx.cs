using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class SystemSettings : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // ✅ Only RootAdmin can access this page
                string role = Session["Role"] as string;
                if (Session["UserID"] == null ||
                    !string.Equals(role ?? "", "RootAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    SafeRedirect("~/Login.aspx");
                    return;
                }

                if (!IsPostBack)
                {
                    InitializeSwitch();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading page: " + ex.Message;
                lblMessage.CssClass = "text-danger fw-bold";
            }
        }

        /// <summary>
        /// Loads the current Maintenance Mode setting from the database
        /// and updates the toggle switch UI.
        /// </summary>
        private void InitializeSwitch()
        {
            try
            {
                bool isMaintenance = false;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT SettingValue FROM SettingSystem WHERE SettingKey = 'MaintenanceMode'", conn))
                    {
                        var result = cmd.ExecuteScalar()?.ToString();
                        isMaintenance = string.Equals(result, "true", StringComparison.OrdinalIgnoreCase);
                    }
                }

                // Update the switch UI via JavaScript
                string switchClass = isMaintenance ? "switch on" : "switch";

                string js = $@"
<script>
    document.addEventListener('DOMContentLoaded', function() {{
        var switchToggle = document.getElementById('switchToggle');
        if (switchToggle) {{
            switchToggle.className = '{switchClass}';
        }}
    }});
</script>";

                ClientScript.RegisterStartupScript(this.GetType(), "initSwitch", js, false);

                lblMessage.Text = ""; // Clear any previous messages
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error initializing maintenance switch: " + ex.Message;
                lblMessage.CssClass = "text-danger fw-bold";
            }
        }

        /// <summary>
        /// Handles toggle click event. 
        /// Flips the maintenance mode state in the database and updates cache.
        /// </summary>
        protected void btnToggle_Click(object sender, EventArgs e)
        {
            try
            {
                bool currentState;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ✅ Step 1: Get the current state
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT SettingValue FROM SettingSystem WHERE SettingKey = 'MaintenanceMode'", conn))
                    {
                        var result = cmd.ExecuteScalar()?.ToString();
                        currentState = string.Equals(result, "true", StringComparison.OrdinalIgnoreCase);
                    }

                    // ✅ Step 2: Flip the state
                    bool newState = !currentState;

                    // ✅ Step 3: Update database
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE SettingSystem SET SettingValue = @value WHERE SettingKey = 'MaintenanceMode'", conn))
                    {
                        cmd.Parameters.AddWithValue("@value", newState ? "true" : "false");
                        cmd.ExecuteNonQuery();
                    }
                }

                // ✅ Step 4: Refresh global cache so that other pages immediately know the new state
                Global.RefreshMaintenanceModeCache();

                // ✅ Step 5: Update UI
                InitializeSwitch();

                lblMessage.Text = $"Maintenance mode {(currentState ? "disabled" : "enabled")}.";
                lblMessage.CssClass = "text-success fw-bold";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error updating maintenance mode: " + ex.Message;
                lblMessage.CssClass = "text-danger fw-bold";
            }
        }

        /// <summary>
        /// Safely redirect without throwing a ThreadAbortException
        /// </summary>
        private void SafeRedirect(string url)
        {
            try
            {
                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch
            {
                // Suppress any redirect errors
            }
        }
    }
}
