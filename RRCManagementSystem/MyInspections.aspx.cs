using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.Script.Serialization; // For JSON serialization
using RRCManagementSystem.Helpers; // For AESHelper

namespace RRCManagementSystem
{
    public partial class MyInspections : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ Ensure only inspectors can access this page
            if (Session["UserID"] == null ||
                !string.Equals(Session["Role"]?.ToString(), "Inspector", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // ✅ Return services as JSON for the SweetAlert dropdown
            if (Request.QueryString["getServices"] == "1")
            {
                GetServicesAsJson();
                return;
            }

            if (!IsPostBack)
            {
                // ✅ Handle when inspector marks an inspection as done
                if (Request.QueryString["done"] != null && int.TryParse(Request.QueryString["done"], out int inspectionId))
                {
                    int inspectorId = Convert.ToInt32(Session["UserID"]);
                    string findings = Request.QueryString["findings"] == null
                        ? null
                        : Server.UrlDecode(Request.QueryString["findings"]).Trim();

                    if (string.IsNullOrWhiteSpace(findings))
                    {
                        Response.Redirect("MyInspections.aspx?err=nofindings");
                        return;
                    }

                    try
                    {
                        // Save combined findings text
                        MarkInspectionAsDone(inspectionId, inspectorId, findings);

                        Response.Redirect("MyInspections.aspx?marked=1");
                        return;
                    }
                    catch (Exception ex)
                    {
                        Response.Redirect("MyInspections.aspx?err=save&msg=" + Server.UrlEncode(ex.Message));
                        return;
                    }
                }

                // Load inspections when page first loads
                LoadMyInspections();
            }
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMyInspections();
        }

        /// <summary>
        /// Load all inspections assigned to the currently logged-in inspector
        /// </summary>
        private void LoadMyInspections()
        {
            int inspectorId = Convert.ToInt32(Session["UserID"]);
            string statusFilter = ddlStatusFilter.SelectedValue; // "All", "Pending", "Completed"

            using (var conn = new SqlConnection(connectionString))
            using (var da = new SqlDataAdapter("dbo.usp_Inspections_ListByInspector", conn))
            {
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;
                da.SelectCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value =
                    string.IsNullOrWhiteSpace(statusFilter) ? "All" : statusFilter;

                var dt = new DataTable();
                da.Fill(dt);

                // ✅ Add FullName column
                if (!dt.Columns.Contains("FullName"))
                    dt.Columns.Add("FullName", typeof(string));

                // ===== Decrypt sensitive fields =====
                foreach (DataRow row in dt.Rows)
                {
                    if (row["EmailEnc"] != DBNull.Value)
                        row["EmailEnc"] = AESHelper.DecryptEmail(row["EmailEnc"].ToString());

                    if (row["ContactEnc"] != DBNull.Value)
                        row["ContactEnc"] = AESHelper.DecryptField(row["ContactEnc"].ToString());

                    if (row["StreetEnc"] != DBNull.Value)
                        row["StreetEnc"] = AESHelper.DecryptField(row["StreetEnc"].ToString());

                    if (row["BarangayEnc"] != DBNull.Value)
                        row["BarangayEnc"] = AESHelper.DecryptField(row["BarangayEnc"].ToString());

                    if (row["CityEnc"] != DBNull.Value)
                        row["CityEnc"] = AESHelper.DecryptField(row["CityEnc"].ToString());

                    if (row["RegionEnc"] != DBNull.Value)
                        row["RegionEnc"] = AESHelper.DecryptField(row["RegionEnc"].ToString());

                    if (row["CountryEnc"] != DBNull.Value)
                        row["CountryEnc"] = AESHelper.DecryptField(row["CountryEnc"].ToString());

                    if (row["LandmarkEnc"] != DBNull.Value)
                        row["LandmarkEnc"] = AESHelper.DecryptField(row["LandmarkEnc"].ToString());

                    // Build FullName safely
                    string first = row["FirstName"]?.ToString() ?? "";
                    string middle = row["MiddleName"]?.ToString() ?? "";
                    string last = row["LastName"]?.ToString() ?? "";
                    row["FullName"] = $"{first} {middle} {last}".Replace("  ", " ").Trim();
                }

                // ✅ Rename columns for display
                dt.Columns["EmailEnc"].ColumnName = "Email";
                dt.Columns["ContactEnc"].ColumnName = "ContactNumber";
                dt.Columns["StreetEnc"].ColumnName = "StreetAndUnit";
                dt.Columns["BarangayEnc"].ColumnName = "Barangay";
                dt.Columns["CityEnc"].ColumnName = "City";
                dt.Columns["RegionEnc"].ColumnName = "Region";
                dt.Columns["CountryEnc"].ColumnName = "Country";
                dt.Columns["LandmarkEnc"].ColumnName = "Landmark";

                rptInspections.DataSource = dt;
                rptInspections.DataBind();
            }
        }

        /// <summary>
        /// Return services as JSON for SweetAlert dropdown
        /// </summary>
        private void GetServicesAsJson()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
        SELECT ServiceID, Name, ServiceType 
        FROM Services 
        WHERE Status = 'Available'
        ORDER BY ServiceType ASC, Name ASC", conn))
            {
                conn.Open();
                var reader = cmd.ExecuteReader();
                var services = new System.Collections.Generic.List<object>();

                while (reader.Read())
                {
                    services.Add(new
                    {
                        ServiceID = reader["ServiceID"],
                        Name = reader["Name"].ToString(),
                        ServiceType = reader["ServiceType"].ToString()
                    });
                }

                var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(services);
                Response.ContentType = "application/json";
                Response.Write(json);
                Response.End();
            }
        }

        /// <summary>
        /// Marks inspection as completed, saving findings as plain text
        /// </summary>
        private void MarkInspectionAsDone(int inspectionId, int inspectorId, string findings)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Inspection_MarkCompleted", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InspectionID", SqlDbType.Int).Value = inspectionId;
                cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;
                cmd.Parameters.Add("@Findings", SqlDbType.NVarChar, -1).Value = findings;

                var affectedParam = cmd.Parameters.Add("@RowsAffected", SqlDbType.Int);
                affectedParam.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
