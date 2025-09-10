using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class SystemChanges : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        // Keys for each SQM range
        private const string KEY_0_100 = "Usage_0_100";
        private const string KEY_101_250 = "Usage_101_250";
        private const string KEY_251_400 = "Usage_251_400";
        private const string KEY_401_600 = "Usage_401_600";
        private const string KEY_601_800 = "Usage_601_800";
        private const string KEY_801_1000 = "Usage_801_1000";
        private const string KEY_1000PLUS = "Usage_1000plus";

        // Max inspections
        private const string KEY_MAXINSP = "MaxInspectionsPerDay";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCurrentSettings();
            }
        }

        private void LoadCurrentSettings()
        {
            try
            {
                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spSystemSettings_GetAll", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        var name = row["SettingName"]?.ToString() ?? "";
                        var value = row["SettingValue"]?.ToString() ?? "";
                        if (!map.ContainsKey(name)) map.Add(name, value);
                    }
                }

                // Assign values to textboxes
                txtUsage_0_100.Text = map.ContainsKey(KEY_0_100) ? map[KEY_0_100] : "";
                txtUsage_101_250.Text = map.ContainsKey(KEY_101_250) ? map[KEY_101_250] : "";
                txtUsage_251_400.Text = map.ContainsKey(KEY_251_400) ? map[KEY_251_400] : "";
                txtUsage_401_600.Text = map.ContainsKey(KEY_401_600) ? map[KEY_401_600] : "";
                txtUsage_601_800.Text = map.ContainsKey(KEY_601_800) ? map[KEY_601_800] : "";
                txtUsage_801_1000.Text = map.ContainsKey(KEY_801_1000) ? map[KEY_801_1000] : "";
                txtUsage_1000plus.Text = map.ContainsKey(KEY_1000PLUS) ? map[KEY_1000PLUS] : "";
                txtMaxInspections.Text = map.ContainsKey(KEY_MAXINSP) ? map[KEY_MAXINSP] : "";
            }
            catch (Exception ex)
            {
                hfAlertMessage.Value = "error|Failed to load settings: " + ex.Message;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (!decimal.TryParse(txtUsage_0_100.Text.Trim(), out decimal usage0_100) ||
                !decimal.TryParse(txtUsage_101_250.Text.Trim(), out decimal usage101_250) ||
                !decimal.TryParse(txtUsage_251_400.Text.Trim(), out decimal usage251_400) ||
                !decimal.TryParse(txtUsage_401_600.Text.Trim(), out decimal usage401_600) ||
                !decimal.TryParse(txtUsage_601_800.Text.Trim(), out decimal usage601_800) ||
                !decimal.TryParse(txtUsage_801_1000.Text.Trim(), out decimal usage801_1000) ||
                !decimal.TryParse(txtUsage_1000plus.Text.Trim(), out decimal usage1000plus) ||
                !int.TryParse(txtMaxInspections.Text.Trim(), out int maxInspections))
            {
                hfAlertMessage.Value = "error|Please enter valid numeric values in all fields.";
                return;
            }

            // Prepare TVP
            var tvp = new DataTable();
            tvp.Columns.Add("SettingName", typeof(string));
            tvp.Columns.Add("SettingValue", typeof(string));

            tvp.Rows.Add(KEY_0_100, usage0_100.ToString());
            tvp.Rows.Add(KEY_101_250, usage101_250.ToString());
            tvp.Rows.Add(KEY_251_400, usage251_400.ToString());
            tvp.Rows.Add(KEY_401_600, usage401_600.ToString());
            tvp.Rows.Add(KEY_601_800, usage601_800.ToString());
            tvp.Rows.Add(KEY_801_1000, usage801_1000.ToString());
            tvp.Rows.Add(KEY_1000PLUS, usage1000plus.ToString());
            tvp.Rows.Add(KEY_MAXINSP, maxInspections.ToString());

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spSystemSettings_BulkUpsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var p = cmd.Parameters.AddWithValue("@Settings", tvp);
                    p.SqlDbType = SqlDbType.Structured;
                    p.TypeName = "dbo.SystemSettingTVP";

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                // Set SweetAlert message to success
                hfAlertMessage.Value = "success|System settings updated successfully!";

                // Clear the form
              
            }
            catch (Exception ex)
            {
                hfAlertMessage.Value = "error|Failed to save: " + ex.Message;
            }
        }

   
    }
}
