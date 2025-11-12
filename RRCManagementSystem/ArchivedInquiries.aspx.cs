using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ArchivedInquiries : System.Web.UI.Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if admin is logged in
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                lblPermission.Text = "⚠️ Access Denied. Admin privileges required.";
                lblPermission.Visible = true;
                gvArchived.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadArchivedInquiries();
            }
        }

        #region Load Archived Inquiries

        /// <summary>
        /// Load all archived inquiries
        /// </summary>
        private void LoadArchivedInquiries()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        i.InquiryID,
                        i.InquiryNumber,
                        i.ClientID,
                        i.InspectionDate,
                        i.InspectionTime,
                        i.PestType,
                        i.ProblemDescription,
                        i.Urgency,
                        i.Status,
                        i.UpdatedAt as ArchivedAt,
                        i.InspectionReportPath,
                        -- Encrypted Address Fields
                        i.AddressEnc,
                        i.BarangayEnc,
                        i.CityEnc,
                        i.RegionEnc,
                        i.LandmarkEnc,
                        -- Client Info from Clients table
                        c.FirstName,
                        c.LastName,
                        c.MiddleName,
                        c.EmailEnc,
                        c.ContactEnc
                    FROM dbo.Inquiries i
                    INNER JOIN dbo.Clients c ON i.ClientID = c.ClientID
                    WHERE i.IsDeleted = 0
                        AND i.Status = 'Archived'
                    ORDER BY i.UpdatedAt DESC
                ", conn))
                {
                    var dt = new DataTable();
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    // Decrypt sensitive data
                    foreach (DataRow row in dt.Rows)
                    {
                        // Decrypt Email
                        if (row["EmailEnc"] != DBNull.Value)
                        {
                            row["EmailEnc"] = DecryptField(row["EmailEnc"]);
                        }

                        // Decrypt Contact
                        if (row["ContactEnc"] != DBNull.Value)
                        {
                            row["ContactEnc"] = DecryptField(row["ContactEnc"]);
                        }

                        // Decrypt Address Fields
                        if (row["AddressEnc"] != DBNull.Value)
                        {
                            row["AddressEnc"] = DecryptField(row["AddressEnc"]);
                        }

                        if (row["BarangayEnc"] != DBNull.Value)
                        {
                            row["BarangayEnc"] = DecryptField(row["BarangayEnc"]);
                        }

                        if (row["CityEnc"] != DBNull.Value)
                        {
                            row["CityEnc"] = DecryptField(row["CityEnc"]);
                        }

                        if (row["RegionEnc"] != DBNull.Value)
                        {
                            row["RegionEnc"] = DecryptField(row["RegionEnc"]);
                        }

                        if (row["LandmarkEnc"] != DBNull.Value)
                        {
                            row["LandmarkEnc"] = DecryptField(row["LandmarkEnc"]);
                        }
                    }

                    // Add computed columns for display
                    dt.Columns.Add("ClientName", typeof(string));
                    dt.Columns.Add("ClientEmail", typeof(string));
                    dt.Columns.Add("ClientContact", typeof(string));
                    dt.Columns.Add("Street", typeof(string));
                    dt.Columns.Add("Barangay", typeof(string));
                    dt.Columns.Add("City", typeof(string));
                    dt.Columns.Add("Region", typeof(string));
                    dt.Columns.Add("Landmark", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        // Build full name
                        string firstName = row["FirstName"]?.ToString() ?? "";
                        string middleName = row["MiddleName"]?.ToString() ?? "";
                        string lastName = row["LastName"]?.ToString() ?? "";
                        row["ClientName"] = $"{firstName} {middleName} {lastName}".Trim();

                        // Set decrypted values
                        row["ClientEmail"] = row["EmailEnc"];
                        row["ClientContact"] = row["ContactEnc"];
                        row["Street"] = row["AddressEnc"];
                        row["Barangay"] = row["BarangayEnc"];
                        row["City"] = row["CityEnc"];
                        row["Region"] = row["RegionEnc"];
                        row["Landmark"] = row["LandmarkEnc"];
                    }

                    gvArchived.DataSource = dt;
                    gvArchived.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadArchivedInquiries Error: {ex.Message}");
                lblPermission.Text = $"⚠️ Error loading archived inquiries: {ex.Message}";
                lblPermission.Visible = true;
            }
        }

        #endregion

        #region GridView Events

        /// <summary>
        /// Handle GridView row commands
        /// </summary>
        protected void gvArchived_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int inquiryId = Convert.ToInt32(e.CommandArgument);

                // Check if this is a confirmed action from SweetAlert
                GridViewRow row = ((Control)e.CommandSource).NamingContainer as GridViewRow;
                if (row != null)
                {
                    var hdnConfirmAction = row.FindControl("hdnConfirmAction") as HiddenField;

                    if (hdnConfirmAction != null && !string.IsNullOrEmpty(hdnConfirmAction.Value))
                    {
                        if (hdnConfirmAction.Value == "Restore")
                        {
                            RestoreInquiry(inquiryId);
                        }
                        else if (hdnConfirmAction.Value == "Delete")
                        {
                            DeleteInquiry(inquiryId);
                        }

                        // Clear the confirmation flag
                        hdnConfirmAction.Value = "";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RowCommand Error: {ex.Message}");
                ShowError("Error processing request.");
            }
        }

        /// <summary>
        /// Handle GridView row data bound
        /// </summary>
        protected void gvArchived_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var dataItem = (DataRowView)e.Row.DataItem;

                // Handle image display
                var litImages = (Literal)e.Row.FindControl("litImages");
                if (litImages != null)
                {
                    string imagePaths = dataItem["InspectionReportPath"]?.ToString();
                    litImages.Text = RenderImages(imagePaths);
                }
            }
        }

        #endregion

        #region Restore Inquiry

        /// <summary>
        /// Restore inquiry back to Pending status
        /// </summary>
        private void RestoreInquiry(int inquiryId)
        {
            try
            {
                int adminId = Convert.ToInt32(Session["UserID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.Inquiries
                    SET 
                        Status = 'Pending',
                        UpdatedAt = GETDATE()
                    WHERE InquiryID = @InquiryID
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@InquiryID", inquiryId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowSuccess("Inquiry restored successfully! It's now back in the pending list.");
                LoadArchivedInquiries();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RestoreInquiry Error: {ex.Message}");
                ShowError("Failed to restore inquiry.");
            }
        }

        #endregion

        #region Delete Inquiry

        /// <summary>
        /// Permanently delete inquiry (soft delete by setting IsDeleted = 1)
        /// </summary>
        private void DeleteInquiry(int inquiryId)
        {
            try
            {
                int adminId = Convert.ToInt32(Session["UserID"]);

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.Inquiries
                    SET 
                        IsDeleted = 1,
                        UpdatedAt = GETDATE()
                    WHERE InquiryID = @InquiryID
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@InquiryID", inquiryId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowSuccess("Inquiry permanently deleted.");
                LoadArchivedInquiries();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteInquiry Error: {ex.Message}");
                ShowError("Failed to delete inquiry.");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Decrypt encrypted field
        /// </summary>
        private string DecryptField(object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            string encrypted = value.ToString();
            if (string.IsNullOrEmpty(encrypted))
                return string.Empty;

            try
            {
                return AESHelper.DecryptField(encrypted);
            }
            catch
            {
                return "[Decryption Error]";
            }
        }

        /// <summary>
        /// Render images for display
        /// </summary>
        private string RenderImages(string imagePaths)
        {
            if (string.IsNullOrEmpty(imagePaths))
                return "<span class='text-gray-500 italic text-sm'>No photos</span>";

            var images = imagePaths.Split(',');
            var sb = new StringBuilder();

            sb.Append("<div class='images-grid'>");
            foreach (var img in images)
            {
                if (!string.IsNullOrWhiteSpace(img))
                {
                    string imgUrl = ResolveUrl(img.Trim());
                    sb.Append($@"
                        <img src='{imgUrl}' 
                             class='inquiry-photo' 
                             onclick='showImageModal(""{imgUrl}""); return false;' 
                             alt='Inspection Photo' />
                    ");
                }
            }
            sb.Append("</div>");

            return sb.ToString();
        }

        #endregion

        #region UI Messages

        /// <summary>
        /// Show error message
        /// </summary>
        private void ShowError(string message)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'error',
                    title: 'Error',
                    text: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#ef4444'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowError", script, true);
        }

        /// <summary>
        /// Show success message
        /// </summary>
        private void ShowSuccess(string message)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'success',
                    title: 'Success!',
                    text: '{message.Replace("'", "\\'")}',
                    confirmButtonColor: '#10b981'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccess", script, true);
        }

        #endregion
    }
}