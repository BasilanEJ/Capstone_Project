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

        // Keys we care about (avoid magic strings everywhere)
        private const string KEY_100 = "BottledChemicalUsageML100sqm";
        private const string KEY_200 = "BottledChemicalUsageML200sqm";
        private const string KEY_200PLUS = "BottledChemicalUsageML200Plus";
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

                txtUsage100.Text = map.ContainsKey(KEY_100) ? map[KEY_100] : "";
                txtUsage200.Text = map.ContainsKey(KEY_200) ? map[KEY_200] : "";
                txtUsage200Plus.Text = map.ContainsKey(KEY_200PLUS) ? map[KEY_200PLUS] : "";
                txtMaxInspections.Text = map.ContainsKey(KEY_MAXINSP) ? map[KEY_MAXINSP] : "";
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "LoadErr",
                    $"Swal.fire('Error','Failed to load settings: {ex.Message.Replace("'", "\\'")}','error');", true);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Validate inputs (same rules as before)
            if (!decimal.TryParse((txtUsage100.Text ?? "").Trim(), out decimal usage100) ||
                !decimal.TryParse((txtUsage200.Text ?? "").Trim(), out decimal usage200) ||
                !decimal.TryParse((txtUsage200Plus.Text ?? "").Trim(), out decimal usage200Plus) ||
                !int.TryParse((txtMaxInspections.Text ?? "").Trim(), out int maxInspections))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "ValErr",
                    "Swal.fire('Error','Please enter valid numeric values in all fields.','error');", true);
                return;
            }

            // Build TVP
            var tvp = new DataTable();
            tvp.Columns.Add("SettingName", typeof(string));
            tvp.Columns.Add("SettingValue", typeof(string));

            tvp.Rows.Add(KEY_100, usage100.ToString());
            tvp.Rows.Add(KEY_200, usage200.ToString());
            tvp.Rows.Add(KEY_200PLUS, usage200Plus.ToString());
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

                ScriptManager.RegisterStartupScript(this, GetType(), "Saved",
                    "Swal.fire('Saved!','System settings updated successfully!','success');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "SaveErr",
                    $"Swal.fire('Error','Failed to save: {ex.Message.Replace("'", "\\'")}','error');", true);
            }
        }
    }
}
