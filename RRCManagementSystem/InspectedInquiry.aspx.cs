// (Your existing using statements)
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
            // ✅ Admin-only access check
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
                gvCompleted.Columns[0].Visible = false; // Hide InspectionID column
                BindCompleted();
            }
        }

        #region Data Binding
        // (Your existing BindCompleted and GetCompletedData methods remain the same)
        private void BindCompleted()
        {
            var dt = GetCompletedData();

            // Sorting based on ViewState
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
            ViewState["CurrentData"] = dt;

            lblCount.Text = $"Total: {dt.Rows.Count} record(s)";
        }

        private DataTable GetCompletedData()
        {
            using (var conn = new SqlConnection(_cs))
            using (var da = new SqlDataAdapter("dbo.usp_Admin_ListCompletedInspections", conn))
            {
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                var dt = new DataTable();
                da.Fill(dt);

                // Add FullName column if missing
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

                    // Build FullName for display
                    string first = row["FirstName"]?.ToString() ?? "";
                    string middle = row["MiddleName"]?.ToString() ?? "";
                    string last = row["LastName"]?.ToString() ?? "";
                    row["FullName"] = $"{last}, {first} {middle}".Trim();
                }

                // ===== Rename decrypted columns =====
                dt.Columns["EmailEnc"].ColumnName = "Email";
                dt.Columns["ContactEnc"].ColumnName = "ContactNumber";
                dt.Columns["StreetEnc"].ColumnName = "StreetAndUnit";
                dt.Columns["BarangayEnc"].ColumnName = "Barangay";
                dt.Columns["CityEnc"].ColumnName = "City";
                dt.Columns["RegionEnc"].ColumnName = "Region";
                dt.Columns["CountryEnc"].ColumnName = "Country";
                dt.Columns["LandmarkEnc"].ColumnName = "Landmark";

                // ===== Add HasAccount column =====
                if (!dt.Columns.Contains("HasAccount"))
                    dt.Columns.Add("HasAccount", typeof(bool));

                // Check if client already has an account
                using (var checkConn = new SqlConnection(_cs))
                {
                    checkConn.Open();
                    foreach (DataRow row in dt.Rows)
                    {
                        string email = row["Email"].ToString();
                        if (!string.IsNullOrWhiteSpace(email))
                        {
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
            if (e.CommandName.Equals("create", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(e.CommandArgument?.ToString(), out int inspectionId))
                    return;

                // Load details for prefill
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

                Session["Prefill_InspectionID"] = Safe(r["InspectionID"]);
                Session["Prefill_InquiryCode"] = r.Table.Columns.Contains("InquiryCode") ? Safe(r["InquiryCode"]) : "";
                Session["Prefill_Findings"] = r.Table.Columns.Contains("Findings") ? Safe(r["Findings"]) : "";

                // --- MODIFIED LINE ---
                // Use ScriptManager to redirect after the AJAX postback completes,
                // preventing a full page refresh before the redirect.
                string redirectScript = $"window.location.href = '{ResolveUrl("~/CreateCustomerAccount.aspx?prefill=1")}';";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "redirect", redirectScript, true);
                return;
            }
        }

        protected void gvCompleted_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var row = (DataRowView)e.Row.DataItem;

                /* ===========================
                    1. ADDRESS WITH SEE MORE
                    =========================== */
                string street = row["StreetAndUnit"]?.ToString() ?? "";
                string barangay = row["Barangay"]?.ToString() ?? "";
                string city = row["City"]?.ToString() ?? "";
                string region = row["Region"]?.ToString() ?? "";
                string country = row["Country"]?.ToString() ?? "";
                string landmark = row["Landmark"]?.ToString() ?? "";

                string fullAddress = $"{street}, {barangay}, {city}, {region}, {country}";
                if (!string.IsNullOrWhiteSpace(landmark))
                    fullAddress += $" • (Landmark: {landmark})";

                // Escape for JS
                string safeFullAddress = fullAddress.Replace("\\", "\\\\")
                                                    .Replace("'", "\\'")
                                                    .Replace("\"", "\\\"")
                                                    .Replace("\r", "")
                                                    .Replace("\n", "\\n");

                var litAddress = (Literal)e.Row.FindControl("litAddress");

                if (fullAddress.Length > 50)
                {
                    string preview = fullAddress.Substring(0, 50) + "...";
                    litAddress.Text = $"{preview} <br/><a href='#' class='text-blue-500 hover:text-blue-700 font-semibold' " +
                                     $"onclick='openModal(\"Full Address\", \"{safeFullAddress}\"); return false;'>See more</a>";
                }
                else
                {
                    litAddress.Text = fullAddress;
                }

                /* ===========================
                    2. FINDINGS WITH SEE MORE
                    =========================== */
                string findings = row["Findings"]?.ToString() ?? "";

                string safeFindings = findings.Replace("\\", "\\\\")
                                              .Replace("'", "\\'")
                                              .Replace("\"", "\\\"")
                                              .Replace("\r", "")
                                              .Replace("\n", "\\n");

                var litFindings = (Literal)e.Row.FindControl("litFindings");

                if (findings.Length > 50)
                {
                    string preview = findings.Substring(0, 50) + "...";
                    litFindings.Text = $"{preview} <br/><a href='#' class='text-blue-500 hover:text-blue-700 font-semibold' " +
                                        $"onclick='openModal(\"Full Findings\", \"{safeFindings}\"); return false;'>See more</a>";
                }
                else
                {
                    litFindings.Text = findings;
                }

                /* ===========================
                    3. ACTION BUTTON STYLING
                    =========================== */
                LinkButton btnCreate = (LinkButton)e.Row.FindControl("btnCreate");
                bool hasAccount = row.Row.Table.Columns.Contains("HasAccount") && Convert.ToBoolean(row["HasAccount"]);

                if (btnCreate != null)
                {
                    if (hasAccount)
                    {
                        btnCreate.Text = "<i class='fa fa-check-circle mr-2'></i>Already Created";
                        btnCreate.Enabled = false;
                        btnCreate.CssClass = "inline-flex items-center bg-green-600 text-white font-semibold py-2 px-3 rounded-full opacity-80 cursor-not-allowed";
                    }
                    else
                    {
                        btnCreate.Text = "<i class='fa fa-user-plus mr-2'></i>Create Client";
                        btnCreate.Enabled = true;
                        btnCreate.CssClass = "inline-flex items-center bg-blue-600 text-white font-semibold py-2 px-3 rounded-full hover:bg-blue-700 transition-colors";
                    }
                }
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

                // Decrypt fields
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

                // Rename decrypted columns
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