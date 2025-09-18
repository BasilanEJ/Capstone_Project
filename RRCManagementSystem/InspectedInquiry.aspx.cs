using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class InspectedInquiry : Page
    {
        private readonly string _cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ Admin-only
            if (Session["UserID"] == null || Session["Role"] == null ||
                !string.Equals(Session["Role"].ToString(), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ViewState["SortExpression"] = "ScheduledDate";
                ViewState["SortDirection"] = "DESC";
                gvCompleted.Columns[0].Visible = false;
                BindCompleted();
            }
        }

        #region Data Binding

        private void BindCompleted()
        {
            var dt = GetCompletedData();

            // Apply sorting based on ViewState
            string sortExp = (ViewState["SortExpression"] as string) ?? "ScheduledDate";
            string sortDir = (ViewState["SortDirection"] as string) ?? "DESC";

            if (dt.Rows.Count > 0)
            {
                var dv = dt.DefaultView;
                dv.Sort = $"{sortExp} {sortDir}";
                dt = dv.ToTable();
            }

            pnlEmpty.Visible = dt.Rows.Count == 0;

            gvCompleted.DataSource = dt;
            gvCompleted.DataBind();

            // Save to ViewState for reuse
            ViewState["CurrentData"] = dt;
        }

        private DataTable GetCompletedData()
        {
            using (var conn = new SqlConnection(_cs))
            using (var da = new SqlDataAdapter("dbo.usp_Admin_ListCompletedInspections", conn))
            {
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                var dt = new DataTable();
                da.Fill(dt);

                // ✅ Add FullName column dynamically
                if (!dt.Columns.Contains("FullName"))
                    dt.Columns.Add("FullName", typeof(string));

                // ✅ Decrypt and rename columns
                foreach (DataRow row in dt.Rows)
                {
                    // Decrypt Email
                    if (row["EmailEnc"] != DBNull.Value)
                        row["EmailEnc"] = AESHelper.DecryptEmail(row["EmailEnc"].ToString());

                    // Decrypt Contact
                    if (row["ContactEnc"] != DBNull.Value)
                        row["ContactEnc"] = AESHelper.DecryptField(row["ContactEnc"].ToString());

                    // Decrypt Address
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

                    // Build FullName
                    string first = row["FirstName"]?.ToString() ?? "";
                    string middle = row["MiddleName"]?.ToString() ?? "";
                    string last = row["LastName"]?.ToString() ?? "";
                    row["FullName"] = $"{last}, {first} {middle}".Replace("  ", " ").Trim();
                }

                // ✅ Rename columns to match old UI
                dt.Columns["EmailEnc"].ColumnName = "Email";
                dt.Columns["ContactEnc"].ColumnName = "ContactNumber";
                dt.Columns["StreetEnc"].ColumnName = "StreetAndUnit";
                dt.Columns["BarangayEnc"].ColumnName = "Barangay";
                dt.Columns["CityEnc"].ColumnName = "City";
                dt.Columns["RegionEnc"].ColumnName = "Region";
                dt.Columns["CountryEnc"].ColumnName = "Country";
                dt.Columns["LandmarkEnc"].ColumnName = "Landmark";

                // ✅ Add column to check if email exists
                if (!dt.Columns.Contains("HasAccount"))
                    dt.Columns.Add("HasAccount", typeof(bool));

                using (var checkConn = new SqlConnection(_cs))
                {
                    checkConn.Open();
                    foreach (DataRow row in dt.Rows)
                    {
                        string email = row["Email"].ToString();
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            // 🔹 Compute SHA-256 hash for search
                            string emailHash = AESHelper.ComputeSHA256WithPepper(email);

                            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Clients WHERE EmailHash = @EmailHash", checkConn))
                            {
                                cmd.Parameters.AddWithValue("@EmailHash", emailHash);
                                int count = (int)cmd.ExecuteScalar();
                                row["HasAccount"] = count > 0;
                            }
                        }
                        else
                        {
                            row["HasAccount"] = false;
                        }
                    }
                }

                return dt;
            }
        }

        #endregion

        #region Grid Events

        protected void gvCompleted_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCompleted.PageIndex = e.NewPageIndex;
            BindCompleted();
        }

        protected void gvCompleted_Sorting(object sender, GridViewSortEventArgs e)
        {
            string currentExp = (ViewState["SortExpression"] as string) ?? "ScheduledDate";
            string currentDir = (ViewState["SortDirection"] as string) ?? "DESC";

            if (string.Equals(currentExp, e.SortExpression, StringComparison.OrdinalIgnoreCase))
            {
                // toggle direction
                ViewState["SortDirection"] = currentDir.Equals("ASC", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
            }
            else
            {
                ViewState["SortExpression"] = e.SortExpression;
                ViewState["SortDirection"] = "ASC";
            }

            BindCompleted();
        }

        protected void gvCompleted_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (string.IsNullOrEmpty(e.CommandName))
                return;

            // 🔹 Handle "See More" for Address
            if (e.CommandName.Equals("viewAddress", StringComparison.OrdinalIgnoreCase))
            {
                string fullAddress = e.CommandArgument.ToString();
                ScriptManager.RegisterStartupScript(this, GetType(), "ViewAddress",
                    $"alert('Full Address:\\n\\n{fullAddress.Replace("'", "\\'")}');", true);
                return;
            }

            // 🔹 Handle "See More" for Findings
            if (e.CommandName.Equals("viewFindings", StringComparison.OrdinalIgnoreCase))
            {
                string findings = e.CommandArgument.ToString();
                ScriptManager.RegisterStartupScript(this, GetType(), "ViewFindings",
                    $"alert('Full Findings:\\n\\n{findings.Replace("'", "\\'")}');", true);
                return;
            }

            // 🔹 Handle "Create Client"
            if (e.CommandName.Equals("create", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(e.CommandArgument?.ToString(), out int inspectionId))
                    return;

                var dt = LoadCompletedRows(inspectionId);
                if (dt.Rows.Count == 0) return;

                DataRow r = dt.Rows[0];

                // Prefill session data for CreateCustomerAccount
                Session["Prefill_LastName"] = Safe(r["LastName"]);
                Session["Prefill_FirstName"] = Safe(r["FirstName"]);
                Session["Prefill_MiddleName"] = Safe(r["MiddleName"]);
                Session["Prefill_Email"] = Safe(r["Email"]);
                Session["Prefill_Contact"] = Safe(r["ContactNumber"]);
                Session["Prefill_Street"] = Safe(r["StreetAndUnit"]);
                Session["Prefill_Barangay"] = Safe(r["Barangay"]);
                Session["Prefill_City"] = Safe(r["City"]);
                Session["Prefill_Region"] = Safe(r["Region"]);
                Session["Prefill_Country"] = string.IsNullOrWhiteSpace(Safe(r["Country"])) ? "Philippines" : Safe(r["Country"]);
                Session["Prefill_Landmark"] = Safe(r["Landmark"]);

                // Include extra fields
                Session["Prefill_InspectionID"] = Safe(r["InspectionID"]);
                Session["Prefill_InquiryCode"] = r.Table.Columns.Contains("InquiryCode") ? Safe(r["InquiryCode"]) : "";
                Session["Prefill_Findings"] = r.Table.Columns.Contains("Findings") ? Safe(r["Findings"]) : "";

                Response.Redirect("~/CreateCustomerAccount.aspx?prefill=1", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        #endregion

        #region Helpers

        private DataTable LoadCompletedRows(int inspectionId)
        {
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("dbo.usp_GetCompletedInspectionDetails", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InspectionID", SqlDbType.Int).Value = inspectionId;

                var dt = new DataTable();
                da.Fill(dt);

                // ✅ Decrypt encrypted fields
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
                }

                // ✅ Rename decrypted columns
                dt.Columns["EmailEnc"].ColumnName = "Email";
                dt.Columns["ContactEnc"].ColumnName = "ContactNumber";
                dt.Columns["StreetEnc"].ColumnName = "StreetAndUnit";
                dt.Columns["BarangayEnc"].ColumnName = "Barangay";
                dt.Columns["CityEnc"].ColumnName = "City";
                dt.Columns["RegionEnc"].ColumnName = "Region";
                dt.Columns["CountryEnc"].ColumnName = "Country";
                dt.Columns["LandmarkEnc"].ColumnName = "Landmark";

                return dt;
            }
        }

        private static string Safe(object v) =>
            (v == null || v == DBNull.Value) ? "" : v.ToString();

        #endregion
    }
}
