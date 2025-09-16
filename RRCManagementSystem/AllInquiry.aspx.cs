using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers; // Needed for AESHelper

namespace RRCManagementSystem
{
    public partial class AllInquiry : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Require login + block roles
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();
            if (role == "Inspector" || role == "SuperAdmin")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindInspectors();
                LoadInquiries();
            }
        }

        private void BindInspectors()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInspectors_ListActive", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);

                ddlInspectorSource.Items.Clear();
                ddlInspectorSource.Items.Add(new ListItem("-- Select Inspector --", ""));
                foreach (DataRow r in dt.Rows)
                    ddlInspectorSource.Items.Add(new ListItem(r["Name"].ToString(), r["UserID"].ToString()));
            }
        }

        protected void btnAssignHidden_Click(object sender, EventArgs e)
        {
            try
            {
                string[] parts = hfAssignData.Value.Split('|');
                if (parts.Length < 12)
                    throw new InvalidOperationException("Missing form data.");

                int inspectorId = int.Parse(parts[0]);
                DateTime scheduleLocal = DateTime.Parse(parts[1]); // browser local (PHT)
                string remarks = parts[2];
                string firstName = parts[3];
                string middleName = parts[4];
                string lastName = parts[5];
                string street = parts[6];
                string barangay = parts[7];
                string city = parts[8];
                string region = parts[9];
                string country = parts[10];
                string landmark = parts[11];

                int inquiryId = int.Parse(hfSelectedInquiryID.Value);

                int maxPerDay = GetSystemSettingInt("MaxInspectionsPerDay");
                if (GetInspectionsCount(inspectorId, scheduleLocal.Date) >= maxPerDay)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "LimitReached",
                        "Swal.fire('Max Limit Reached','Inspector already has the maximum inspections on this day.','error');", true);
                    return;
                }

                UpdateInquiryInfo(inquiryId, firstName, middleName, lastName, street, barangay, city, region, country, landmark);
                AssignInspector(inquiryId, inspectorId, scheduleLocal, remarks);

                BindInspectors();
                LoadInquiries();

                ScriptManager.RegisterStartupScript(this, GetType(), "Success",
                    "Swal.fire('Success','Inspector assigned successfully.','success');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "AssignErr",
                    $"Swal.fire('Error','{ex.Message.Replace("'", "\\'")}','error');", true);
            }
        }

        private void LoadInquiries()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInquiries_Unassigned_List", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);

                // ===== Decrypt sensitive fields before binding =====
                foreach (DataRow row in dt.Rows)
                {
                    // Decrypt email and contact
                    if (row["EmailEnc"] != DBNull.Value)
                        row["EmailEnc"] = AESHelper.DecryptEmail(row["EmailEnc"].ToString());

                    if (row["ContactEnc"] != DBNull.Value)
                        row["ContactEnc"] = AESHelper.DecryptField(row["ContactEnc"].ToString());

                    // Decrypt address fields
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
                }

                // ✅ Rename columns to match old UI so GridView binds correctly
                dt.Columns["EmailEnc"].ColumnName = "Email";
                dt.Columns["ContactEnc"].ColumnName = "ContactNumber";
                dt.Columns["StreetEnc"].ColumnName = "StreetAndUnit";
                dt.Columns["BarangayEnc"].ColumnName = "Barangay";
                dt.Columns["CityEnc"].ColumnName = "City";
                dt.Columns["RegionEnc"].ColumnName = "Region";
                dt.Columns["CountryEnc"].ColumnName = "Country";
                dt.Columns["LandmarkEnc"].ColumnName = "Landmark";

                // Bind to GridView
                gvInquiries.DataSource = dt;
                gvInquiries.DataBind();
            }
        }


        private int GetSystemSettingInt(string settingName)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spSystemSetting_Get", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add("@SettingName", SqlDbType.NVarChar, 100).Value = settingName;

                conn.Open();
                object val = cmd.ExecuteScalar();
                return (val != null && int.TryParse(val.ToString(), out int n)) ? n : 6;
            }
        }

        private int GetInspectionsCount(int inspectorId, DateTime date)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInspections_CountForInspectorOnDate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void UpdateInquiryInfo(
            int inquiryId, string firstName, string middleName, string lastName,
            string street, string barangay, string city, string region, string country, string landmark)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInquiry_UpdateAddress", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@InquiryID", SqlDbType.Int).Value = inquiryId;
                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName ?? "";
                cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName;
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName ?? "";

                // Encrypt address fields before saving
                cmd.Parameters.Add("@StreetAndUnit", SqlDbType.NVarChar, 255).Value =
                    string.IsNullOrWhiteSpace(street) ? (object)DBNull.Value : AESHelper.EncryptField(street);

                cmd.Parameters.Add("@Barangay", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(barangay) ? (object)DBNull.Value : AESHelper.EncryptField(barangay);

                cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(city) ? (object)DBNull.Value : AESHelper.EncryptField(city);

                cmd.Parameters.Add("@Region", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(region) ? (object)DBNull.Value : AESHelper.EncryptField(region);

                cmd.Parameters.Add("@Country", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(country) ? (object)DBNull.Value : AESHelper.EncryptField(country);

                cmd.Parameters.Add("@Landmark", SqlDbType.NVarChar, 255).Value =
                    string.IsNullOrWhiteSpace(landmark) ? (object)DBNull.Value : AESHelper.EncryptField(landmark);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // INSPECTOR-ONLY VERSION
        private void AssignInspector(int inquiryId, int inspectorUserId, DateTime scheduleLocalPHT, string remarks)
        {
            int? createdInspectionId = null;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand("dbo.spInspection_Assign", conn, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@InquiryID", SqlDbType.Int).Value = inquiryId;
                        cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorUserId;
                        cmd.Parameters.Add("@ScheduledDate", SqlDbType.DateTime).Value = scheduleLocalPHT;
                        cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, -1).Value =
                            string.IsNullOrWhiteSpace(remarks) ? (object)DBNull.Value : remarks;

                        var pOut = new SqlParameter("@InspectionID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(pOut);
                        cmd.ExecuteNonQuery();

                        if (pOut.Value != DBNull.Value)
                            createdInspectionId = Convert.ToInt32(pOut.Value);
                    }

                    // Send notification to inspector
                    string when = scheduleLocalPHT.ToString("yyyy-MM-dd HH:mm");
                    string title = "New Inspection Assigned";
                    string body = $"You have a new inspection scheduled on {when} for Inquiry #{inquiryId}.";
                    string url = createdInspectionId.HasValue
                                     ? $"~/MyInspections.aspx?InspectionID={createdInspectionId.Value}"
                                     : "~/MyInspections.aspx";
                    string dedupInspector = $"assign:{inspectorUserId}:{inquiryId}:{when}";

                    using (var cmdN = new SqlCommand("dbo.spNotification_Add", conn, tx))
                    {
                        cmdN.CommandType = CommandType.StoredProcedure;
                        cmdN.Parameters.AddWithValue("@UserID", inspectorUserId);
                        cmdN.Parameters.AddWithValue("@ClientID", DBNull.Value);
                        cmdN.Parameters.AddWithValue("@Type", "Inspection");
                        cmdN.Parameters.AddWithValue("@Title", title);
                        cmdN.Parameters.AddWithValue("@Body", body);
                        cmdN.Parameters.AddWithValue("@Url", url);
                        cmdN.Parameters.AddWithValue("@DedupKey", dedupInspector);
                        cmdN.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        protected void btnDeleteHidden_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfDeleteInquiryID.Value, out int inquiryId))
                return;

            try
            {
                DeleteInquiry(inquiryId);
                LoadInquiries();

                ScriptManager.RegisterStartupScript(this, GetType(), "DeletedOK",
                    "Swal.fire('Deleted','Inquiry has been removed.','success');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "DeletedErr",
                    $"Swal.fire('Error','Failed to delete inquiry: {ex.Message.Replace("'", "\\'")}','error');", true);
            }
        }

        private void DeleteInquiry(int inquiryId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInquiry_Delete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InquiryID", SqlDbType.Int).Value = inquiryId;

                conn.Open();
                int rows = Convert.ToInt32(cmd.ExecuteScalar());
                if (rows == 0)
                    throw new InvalidOperationException("Inquiry not found.");
            }
        }

        protected void gvInquiries_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var drv = (DataRowView)e.Row.DataItem;

                Button btnAssign = (Button)e.Row.FindControl("btnAssign");
                string inquiryId = drv["InquiryID"].ToString();
                string inquiryCode = drv["InquiryCode"].ToString();

                // ✅ Use final renamed columns here
                btnAssign.Attributes["data-fn"] = SafeAttr(drv["FirstName"]);
                btnAssign.Attributes["data-mn"] = SafeAttr(drv["MiddleName"]);
                btnAssign.Attributes["data-ln"] = SafeAttr(drv["LastName"]);
                btnAssign.Attributes["data-str"] = SafeAttr(drv["StreetAndUnit"]);
                btnAssign.Attributes["data-brgy"] = SafeAttr(drv["Barangay"]);
                btnAssign.Attributes["data-city"] = SafeAttr(drv["City"]);
                btnAssign.Attributes["data-reg"] = SafeAttr(drv["Region"]);
                btnAssign.Attributes["data-ctry"] = SafeAttr(drv["Country"]);
                btnAssign.Attributes["data-lmk"] = SafeAttr(drv["Landmark"]);
                btnAssign.Attributes["data-code"] = inquiryCode;

                btnAssign.OnClientClick = $"showAssignModal(this, {inquiryId}); return false;";
            }
        }



        private static string SafeAttr(object val)
        {
            return val == null || val == DBNull.Value ? "" : val.ToString();
        }
    }
}
