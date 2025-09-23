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
                        // Redirect if no findings provided
                        Response.Redirect("MyInspections.aspx?err=nofindings");
                        return;
                    }

                    try
                    {
                        // 🔹 Validate scheduled date before marking as done
                        using (var conn = new SqlConnection(connectionString))
                        using (var cmd = new SqlCommand(@"
                            SELECT ScheduledDate, InspectionStatus
                            FROM Inspections
                            WHERE InspectionID = @InspectionID AND InspectorID = @InspectorID", conn))
                        {
                            cmd.Parameters.Add("@InspectionID", SqlDbType.Int).Value = inspectionId;
                            cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;

                            conn.Open();
                            var reader = cmd.ExecuteReader();

                            if (!reader.Read())
                            {
                                throw new Exception("Inspection not found or you are not authorized to update this inspection.");
                            }

                            DateTime scheduledDate = Convert.ToDateTime(reader["ScheduledDate"]);
                            string status = reader["InspectionStatus"].ToString();
                            reader.Close();

                            // ❌ Cannot mark as done if scheduled date is in the future
                            if (scheduledDate.Date > DateTime.Now.Date)
                            {
                                throw new Exception("You cannot mark this inspection as done before its scheduled date.");
                            }

                            // ❌ Cannot mark as done if already completed
                            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new Exception("This inspection is already marked as completed.");
                            }
                        }

                        // ✅ If validation passed, save the findings and mark as done
                        MarkInspectionAsDone(inspectionId, inspectorId, findings);

                        Response.Redirect("MyInspections.aspx?marked=1");
                        return;
                    }
                    catch (Exception ex)
                    {
                        // 🔹 Show error in SweetAlert after redirect
                        string script = $@"
                            <script>
                                window.onload = function() {{
                                    Swal.fire('Error', '{ex.Message.Replace("'", "\\'")}', 'error');
                                }};
                            </script>";

                        ClientScript.RegisterStartupScript(this.GetType(), "ErrorAlert", script);
                    }
                }

                // ✅ Load inspections when page first loads
                LoadMyInspections();
            }
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMyInspections();
        }

        /// <summary>
        /// Helper method to return the action button HTML
        /// </summary>
        protected string GetActionButton(object scheduledDateObj, object inspectionStatusObj, object inspectionIdObj)
        {
            if (scheduledDateObj == null || inspectionStatusObj == null)
                return "";

            DateTime scheduledDate = Convert.ToDateTime(scheduledDateObj);
            string status = inspectionStatusObj.ToString();
            DateTime today = DateTime.Now.Date;

            // Show "Mark as Done" if scheduled date is today or earlier and status is Pending
            if (scheduledDate <= today && status == "Pending")
            {
                return $"<button type='button' class='bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded-md text-sm transition-colors' onclick=\"markDoneWithFindings('{inspectionIdObj}')\">Mark as Done</button>";
            }
            // Show disabled button if scheduled date is in the future
            else if (scheduledDate > today && status == "Pending")
            {
                return "<button type='button' class='bg-gray-400 text-white font-bold py-2 px-4 rounded-md text-sm cursor-not-allowed' disabled>Mark Done</button>";
            }

            return ""; // Completed or other cases → no button
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

                var json = new JavaScriptSerializer().Serialize(services);
                Response.ContentType = "application/json";
                Response.Write(json);
                Response.End();
            }
        }

        private void MarkInspectionAsDone(int inspectionId, int inspectorId, string findings)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Validate the inspection first
                using (var cmd = new SqlCommand(@"
            SELECT ScheduledDate, InspectionStatus 
            FROM Inspections 
            WHERE InspectionID = @InspectionID AND InspectorID = @InspectorID", conn))
                {
                    cmd.Parameters.Add("@InspectionID", SqlDbType.Int).Value = inspectionId;
                    cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;

                    var reader = cmd.ExecuteReader();

                    if (!reader.Read())
                        throw new Exception("Inspection not found or you are not authorized to update this inspection.");

                    DateTime scheduledDate = Convert.ToDateTime(reader["ScheduledDate"]);
                    string status = reader["InspectionStatus"].ToString();
                    reader.Close();

                    if (scheduledDate.Date > DateTime.Now.Date)
                        throw new Exception("You cannot mark this inspection as done before its scheduled date.");

                    if (status == "Completed")
                        throw new Exception("This inspection is already marked as completed.");

                    // ✅ Call the stored procedure
                    using (var cmdUpdate = new SqlCommand("dbo.usp_Inspection_MarkCompleted", conn))
                    {
                        cmdUpdate.CommandType = CommandType.StoredProcedure;

                        // Required input parameters
                        cmdUpdate.Parameters.Add("@InspectionID", SqlDbType.Int).Value = inspectionId;
                        cmdUpdate.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;
                        cmdUpdate.Parameters.Add("@Findings", SqlDbType.NVarChar, -1).Value = findings;

                        // ✅ OUTPUT parameter
                        var rowsAffectedParam = cmdUpdate.Parameters.Add("@RowsAffected", SqlDbType.Int);
                        rowsAffectedParam.Direction = ParameterDirection.Output;

                        cmdUpdate.ExecuteNonQuery();

                        // Get the value back
                        int rowsAffected = (int)rowsAffectedParam.Value;

                        // Check if the update actually happened
                        if (rowsAffected == 0)
                        {
                            throw new Exception("No inspection was updated. Ensure you are assigned to this inspection.");
                        }
                    }
                }
            }
        }


    }
}
