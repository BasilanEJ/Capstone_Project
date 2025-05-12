using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class SystemChanges : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCurrentSettings();
            }
        }

        private void LoadCurrentSettings()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                txtUsage100.Text = GetSettingValue(conn, "BottledChemicalUsageML100sqm");
                txtUsage200.Text = GetSettingValue(conn, "BottledChemicalUsageML200sqm");
                txtUsage200Plus.Text = GetSettingValue(conn, "BottledChemicalUsageML200Plus");
                txtMaxInspections.Text = GetSettingValue(conn, "MaxInspectionsPerDay");
            }
        }

        private string GetSettingValue(SqlConnection conn, string settingName)
        {
            string query = "SELECT SettingValue FROM SystemSettings WHERE SettingName = @SettingName";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@SettingName", settingName);
                object result = cmd.ExecuteScalar();
                return result?.ToString() ?? "";
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtUsage100.Text.Trim(), out decimal usage100) &&
                decimal.TryParse(txtUsage200.Text.Trim(), out decimal usage200) &&
                decimal.TryParse(txtUsage200Plus.Text.Trim(), out decimal usage200Plus) &&
                int.TryParse(txtMaxInspections.Text.Trim(), out int maxInspections))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    UpdateSetting(conn, "BottledChemicalUsageML100sqm", usage100.ToString());
                    UpdateSetting(conn, "BottledChemicalUsageML200sqm", usage200.ToString());
                    UpdateSetting(conn, "BottledChemicalUsageML200Plus", usage200Plus.ToString());
                    UpdateSetting(conn, "MaxInspectionsPerDay", maxInspections.ToString());
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "Success",
                    "Swal.fire('Saved!', 'System settings updated successfully!', 'success');", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "Error",
                    "Swal.fire('Error', 'Please enter valid numeric values in all fields.', 'error');", true);
            }
        }

        private void UpdateSetting(SqlConnection conn, string settingName, string value)
        {
            string query = "UPDATE SystemSettings SET SettingValue = @Value, UpdatedAt = GETDATE() WHERE SettingName = @SettingName";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Value", value);
                cmd.Parameters.AddWithValue("@SettingName", settingName);
                cmd.ExecuteNonQuery();
            }
        }
    }
}