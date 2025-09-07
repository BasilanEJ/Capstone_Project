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
            // Only RootAdmin can access
            string role = Session["Role"] as string;
            if (Session["UserID"] == null || !string.Equals(role ?? "", "RootAdmin", StringComparison.OrdinalIgnoreCase))
            {
                SafeRedirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                InitializeSwitch();
            }
        }

        /// <summary>
        /// Sets the switch class based on current maintenance mode in DB
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
                        "SELECT SettingValue FROM SettingSystem WHERE SettingKey='MaintenanceMode'", conn))
                    {
                        var result = cmd.ExecuteScalar()?.ToString();
                        isMaintenance = string.Equals(result, "true", StringComparison.OrdinalIgnoreCase);
                    }
                }

                // Set switch class for client-side indicator
                string switchClass = isMaintenance ? "switch on" : "switch";
                string js = $@"
<script>
    document.addEventListener('DOMContentLoaded', function(){{
        var switchToggle = document.getElementById('switchToggle');
        if(switchToggle) {{
            switchToggle.className = '{switchClass}';
        }}
    }});
</script>";
                ClientScript.RegisterStartupScript(this.GetType(), "initSwitch", js, false);

                // Clear any previous messages
                lblMessage.Text = "";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error initializing maintenance switch: " + ex.Message;
                lblMessage.CssClass = "text-danger fw-bold";
            }
        }

        protected void btnToggle_Click(object sender, EventArgs e)
        {
            try
            {
                bool currentState;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Read current state
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT SettingValue FROM SettingSystem WHERE SettingKey='MaintenanceMode'", conn))
                    {
                        var result = cmd.ExecuteScalar()?.ToString();
                        currentState = string.Equals(result, "true", StringComparison.OrdinalIgnoreCase);
                    }

                    // Flip the state
                    bool newState = !currentState;

                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE SettingSystem SET SettingValue=@value WHERE SettingKey='MaintenanceMode'", conn))
                    {
                        cmd.Parameters.AddWithValue("@value", newState ? "true" : "false");
                        cmd.ExecuteNonQuery();
                    }
                }

                // Refresh global cache
                Global.RefreshMaintenanceModeCache();

                // Re-initialize switch to reflect new state
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

        private void SafeRedirect(string url)
        {
            try
            {
                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch { }
        }
    }
}
