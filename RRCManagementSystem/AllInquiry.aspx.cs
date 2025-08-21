using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class AllInquiry : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
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
                {
                    ddlInspectorSource.Items.Add(new ListItem(r["Name"].ToString(), r["UserID"].ToString()));
                }
            }
        }

        protected void btnAssignHidden_Click(object sender, EventArgs e)
        {
            string[] parts = hfAssignData.Value.Split('|');
            if (parts.Length >= 12)
            {
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
                        "Swal.fire('Max Limit Reached', 'Inspector already has the maximum inspections on this day.', 'error');", true);
                    return;
                }

                UpdateInquiryInfo(inquiryId, firstName, middleName, lastName, street, barangay, city, region, country, landmark);
                AssignInspector(inquiryId, inspectorId, scheduleLocal, remarks);

                BindInspectors();
                LoadInquiries();

                ScriptManager.RegisterStartupScript(this, GetType(), "Success",
                    "Swal.fire('Success', 'Inspector assigned successfully.', 'success');", true);
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

                gvInquiries.DataSource = dt;
                gvInquiries.DataBind();
            }
        }

        private int GetSystemSettingInt(string settingName)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spSystemSetting_Get", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
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

        private void UpdateInquiryInfo(int inquiryId, string firstName, string middleName, string lastName,
                                      string street, string barangay, string city, string region, string country, string landmark)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInquiry_UpdateAddress", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@InquiryID", SqlDbType.Int).Value = inquiryId;
                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName ?? "";
                cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName;
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName ?? "";
                cmd.Parameters.Add("@StreetAndUnit", SqlDbType.NVarChar, 255).Value = string.IsNullOrWhiteSpace(street) ? (object)DBNull.Value : street;
                cmd.Parameters.Add("@Barangay", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(barangay) ? (object)DBNull.Value : barangay;
                cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(city) ? (object)DBNull.Value : city;
                cmd.Parameters.Add("@Region", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(region) ? (object)DBNull.Value : region;
                cmd.Parameters.Add("@Country", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(country) ? (object)DBNull.Value : country;
                cmd.Parameters.Add("@Landmark", SqlDbType.NVarChar, 255).Value = string.IsNullOrWhiteSpace(landmark) ? (object)DBNull.Value : landmark;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void AssignInspector(int inquiryId, int inspectorUserId, DateTime scheduleLocalPHT, string remarks)
        {
            int? createdInspectionId = null;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    // 1) Assign
                    using (var cmd = new SqlCommand("dbo.spInspection_Assign", conn, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@InquiryID", SqlDbType.Int).Value = inquiryId;
                        cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorUserId;
                        cmd.Parameters.Add("@ScheduledDate", SqlDbType.DateTime).Value = scheduleLocalPHT;
                        cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(remarks) ? (object)DBNull.Value : remarks;

                        // Optional: capture output if your proc supports it
                        var pOut = new SqlParameter("@InspectionID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(pOut);

                        cmd.ExecuteNonQuery();

                        if (pOut.Value != DBNull.Value) createdInspectionId = Convert.ToInt32(pOut.Value);
                    }

                    // 2) Build notification payload
                    string title = "New Inspection Assigned";
                    string when = scheduleLocalPHT.ToString("yyyy-MM-dd HH:mm");
                    string body = $"You have a new inspection scheduled on {when} for Inquiry #{inquiryId}.";
                    string url = createdInspectionId.HasValue
                                   ? $"~/MyInspections.aspx?InspectionID={createdInspectionId.Value}"
                                   : "~/MyInspections.aspx";

                    // Use a dedup key to avoid double inserts if user double-clicks
                    string dedup = $"assign:{inspectorUserId}:{inquiryId}:{when}";

                    // 3) Insert notification for the inspector
                    using (var cmdN = new SqlCommand("dbo.spNotification_Add", conn, tx))
                    {
                        cmdN.CommandType = CommandType.StoredProcedure;
                        cmdN.Parameters.AddWithValue("@UserID", inspectorUserId);
                        cmdN.Parameters.AddWithValue("@ClientID", DBNull.Value);
                        cmdN.Parameters.AddWithValue("@Type", "Inspection");
                        cmdN.Parameters.AddWithValue("@Title", title);
                        cmdN.Parameters.AddWithValue("@Body", body);
                        cmdN.Parameters.AddWithValue("@Url", url);
                        cmdN.Parameters.AddWithValue("@DedupKey", dedup);
                        cmdN.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        private void CreateNotification(int userId, int? clientId, string type, string title, string body, string url, string dedupKey = null)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spNotification_Add", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@ClientID", (object)clientId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Type", type ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Title", title ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Body", (object)body ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Url", (object)url ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DedupKey", (object)dedupKey ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
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
                    "Swal.fire('Deleted', 'Inquiry has been removed.', 'success');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "DeletedErr",
                    $"Swal.fire('Error', 'Failed to delete inquiry: {ex.Message.Replace("'", "\\'")}', 'error');", true);
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
                int rows = Convert.ToInt32(cmd.ExecuteScalar()); // proc SELECTs @@ROWCOUNT
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

                btnAssign.Attributes["data-fn"] = SafeAttr(drv["FirstName"]);
                btnAssign.Attributes["data-mn"] = SafeAttr(drv["MiddleName"]);
                btnAssign.Attributes["data-ln"] = SafeAttr(drv["LastName"]);
                btnAssign.Attributes["data-str"] = SafeAttr(drv["StreetAndUnit"]);
                btnAssign.Attributes["data-brgy"] = SafeAttr(drv["Barangay"]);
                btnAssign.Attributes["data-city"] = SafeAttr(drv["City"]);
                btnAssign.Attributes["data-reg"] = SafeAttr(drv["Region"]);
                btnAssign.Attributes["data-ctry"] = SafeAttr(drv["Country"]);
                btnAssign.Attributes["data-lmk"] = SafeAttr(drv["Landmark"]);

                btnAssign.OnClientClick = $"showAssignModal(this, {inquiryId}); return false;";
            }
        }

        private static string SafeAttr(object val)
        {
            return val == null || val == DBNull.Value ? "" : val.ToString();
        }
    }
}
