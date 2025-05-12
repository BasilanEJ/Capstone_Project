using System;
using System.Configuration;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class SystemSettings : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSettings();
            }
        }

        private void LoadSettings()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    txtSystemName.Text = GetSetting(conn, "SystemName");
                    txtSenderEmail.Text = GetSetting(conn, "SenderEmail");
                    ddlMaintenanceMode.SelectedValue = GetSetting(conn, "MaintenanceMode");
                    txtLogRetentionDays.Text = GetSetting(conn, "LogRetentionDays");
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error loading settings: " + ex.Message;
                }
            }
        }

        private string GetSetting(SqlConnection conn, string settingKey)
        {
            string query = "SELECT SettingValue FROM SystemSettings WHERE SettingKey = @SettingKey";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@SettingKey", settingKey);
                var result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : string.Empty;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    UpdateSetting(conn, "SystemName", txtSystemName.Text.Trim());
                    UpdateSetting(conn, "SenderEmail", txtSenderEmail.Text.Trim());
                    UpdateSetting(conn, "MaintenanceMode", ddlMaintenanceMode.SelectedValue);
                    UpdateSetting(conn, "LogRetentionDays", txtLogRetentionDays.Text.Trim());

                    lblSuccess.Text = "✅ Settings updated successfully.";
                    lblMessage.Text = "";
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "⚠ Error saving settings: " + ex.Message;
                    lblSuccess.Text = "";
                }
            }
        }

        private void UpdateSetting(SqlConnection conn, string settingKey, string settingValue)
        {
            string query = @"
                UPDATE SystemSettings
                SET SettingValue = @SettingValue, UpdatedAt = GETDATE()
                WHERE SettingKey = @SettingKey";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@SettingKey", settingKey);
                cmd.Parameters.AddWithValue("@SettingValue", settingValue);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

