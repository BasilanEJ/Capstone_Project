using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers;

namespace RRCManagementSystem
{
    public partial class ServiceManagement : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private string currentFilter = "All";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadServices("All");
                ViewState["CurrentFilter"] = "All";
            }
            else
            {
                currentFilter = ViewState["CurrentFilter"]?.ToString() ?? "All";
            }
        }

        // ============ Load Services ============
        private void LoadServices(string filterType)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT ServiceID, ServiceType, ServiceTitle, ServiceDescription, 
                       BulletPoints, ImagePath, DisplayOrder, IsActive, CreatedDate
                FROM ServicesCMS
                WHERE IsActive = 1";

                    if (filterType == "Termite Control")
                    {
                        query += " AND ServiceType = 'Termite Control'";
                    }
                    else if (filterType == "General Pest Control")
                    {
                        query += " AND ServiceType = 'General Pest Control'";
                    }

                    query += " ORDER BY ServiceType, DisplayOrder, ServiceID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvServices.DataSource = dt;
                        gvServices.DataBind();
                    }
                }

                UpdateTabActiveState(filterType);
                ViewState["CurrentFilter"] = filterType;
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to load services: " + ex.Message, "error");
            }
        }

        // ============ Update Tab Active State ============
        private void UpdateTabActiveState(string activeTab)
        {
            btnTabAll.CssClass = activeTab == "All" ? "tab-btn active" : "tab-btn";
            btnTabTermite.CssClass = activeTab == "Termite Control" ? "tab-btn active" : "tab-btn";
            btnTabPest.CssClass = activeTab == "General Pest Control" ? "tab-btn active" : "tab-btn";
        }

        // ============ Tab Click Events ============
        protected void btnTabAll_Click(object sender, EventArgs e)
        {
            LoadServices("All");
        }

        protected void btnTabTermite_Click(object sender, EventArgs e)
        {
            LoadServices("Termite Control");
        }

        protected void btnTabPest_Click(object sender, EventArgs e)
        {
            LoadServices("General Pest Control");
        }

        // ============ Add New Service (with SweetAlert Validation) ============
        protected void btnAddService_Click(object sender, EventArgs e)
        {
            // Client-side validation
            string title = txtServiceTitle.Text.Trim();
            string description = txtServiceDescription.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                ShowAlert("Validation Error", "Service title is required!", "warning");
                return;
            }

            if (string.IsNullOrEmpty(description))
            {
                ShowAlert("Validation Error", "Service description is required!", "warning");
                return;
            }

            if (!fuServiceImage.HasFile)
            {
                ShowAlert("Validation Error", "Please select an image for the service!", "warning");
                return;
            }

            // Validate file type
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            string extension = Path.GetExtension(fuServiceImage.FileName).ToLowerInvariant();
            if (Array.IndexOf(allowedExtensions, extension) == -1)
            {
                ShowAlert("Invalid File Type", "Only image files (JPG, PNG, GIF, WEBP) are allowed!", "error");
                return;
            }

            // Validate file size (5MB max)
            if (fuServiceImage.PostedFile.ContentLength > 5 * 1024 * 1024)
            {
                ShowAlert("File Too Large", "Image size must be less than 5MB!", "error");
                return;
            }

            try
            {
                string serviceType = rblServiceType.SelectedValue;
                string bulletPoints = txtBulletPoints.Text.Trim();

                // Save image with resize
                var serviceSize = ImageHelper.RecommendedDimensions.ServiceCard;
                string imagePath = SaveUploadedImageWithResize(
                    fuServiceImage,
                    "Service",
                    serviceSize.Width,
                    serviceSize.Height
                );

                if (string.IsNullOrEmpty(imagePath))
                {
                    ShowAlert("Upload Error", "Failed to upload image. Please try again!", "error");
                    return;
                }

                // Insert into database
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO ServicesCMS (ServiceType, ServiceTitle, ServiceDescription, 
                                                BulletPoints, ImagePath, DisplayOrder, IsActive, CreatedDate)
                        VALUES (@ServiceType, @ServiceTitle, @ServiceDescription, 
                                @BulletPoints, @ImagePath, 
                                (SELECT ISNULL(MAX(DisplayOrder), 0) + 1 FROM ServicesCMS WHERE ServiceType = @ServiceType), 
                                1, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ServiceType", serviceType);
                        cmd.Parameters.AddWithValue("@ServiceTitle", title);
                        cmd.Parameters.AddWithValue("@ServiceDescription", description);
                        cmd.Parameters.AddWithValue("@BulletPoints", bulletPoints ?? "");
                        cmd.Parameters.AddWithValue("@ImagePath", imagePath);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowAlertWithCallback("Success!", "Service added successfully!", "success", "clearForm");
                LoadServices(ViewState["CurrentFilter"]?.ToString() ?? "All");
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to add service: " + ex.Message, "error");
            }
        }

        // ============ Clear Form ============
        protected void btnClearForm_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtServiceTitle.Text = "";
            txtServiceDescription.Text = "";
            txtBulletPoints.Text = "";
            rblServiceType.SelectedIndex = 0;
        }

        // ============ GridView Row Command ============
        protected void gvServices_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "MoveUp" || e.CommandName == "MoveDown" || e.CommandName == "ToggleStatus")
            {
                try
                {
                    int serviceId = Convert.ToInt32(e.CommandArgument);

                    if (e.CommandName == "MoveUp")
                    {
                        MoveService(serviceId, -1);
                    }
                    else if (e.CommandName == "MoveDown")
                    {
                        MoveService(serviceId, 1);
                    }
                    else if (e.CommandName == "ToggleStatus")
                    {
                        ToggleServiceStatus(serviceId);
                    }

                    LoadServices(ViewState["CurrentFilter"]?.ToString() ?? "All");
                }
                catch (Exception ex)
                {
                    ShowAlert("Error", "Failed to process command: " + ex.Message, "error");
                }
            }
        }

        // ============ Move Service (Reorder) ============
        private void MoveService(int serviceId, int direction)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmdGet = new SqlCommand(
                        "SELECT DisplayOrder, ServiceType FROM ServicesCMS WHERE ServiceID = @ServiceID", conn);
                    cmdGet.Parameters.AddWithValue("@ServiceID", serviceId);

                    SqlDataReader reader = cmdGet.ExecuteReader();
                    if (!reader.Read())
                    {
                        reader.Close();
                        return;
                    }

                    int currentOrder = Convert.ToInt32(reader["DisplayOrder"]);
                    string serviceType = reader["ServiceType"].ToString();
                    reader.Close();

                    int newOrder = currentOrder + direction;

                    SqlCommand cmdSwap = new SqlCommand(@"
                        UPDATE ServicesCMS SET DisplayOrder = @TempOrder 
                        WHERE DisplayOrder = @NewOrder AND ServiceType = @ServiceType;
                        
                        UPDATE ServicesCMS SET DisplayOrder = @NewOrder 
                        WHERE ServiceID = @ServiceID;
                        
                        UPDATE ServicesCMS SET DisplayOrder = @CurrentOrder 
                        WHERE DisplayOrder = @TempOrder AND ServiceType = @ServiceType;", conn);

                    cmdSwap.Parameters.AddWithValue("@ServiceID", serviceId);
                    cmdSwap.Parameters.AddWithValue("@CurrentOrder", currentOrder);
                    cmdSwap.Parameters.AddWithValue("@NewOrder", newOrder);
                    cmdSwap.Parameters.AddWithValue("@TempOrder", 999999);
                    cmdSwap.Parameters.AddWithValue("@ServiceType", serviceType);

                    cmdSwap.ExecuteNonQuery();
                }

                ShowToast("Service order updated!", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to reorder service: " + ex.Message, "error");
            }
        }

        // ============ Toggle Service Status ============
        private void ToggleServiceStatus(int serviceId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE ServicesCMS SET IsActive = ~IsActive, LastUpdated = GETDATE() WHERE ServiceID = @ServiceID", conn);
                    cmd.Parameters.AddWithValue("@ServiceID", serviceId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowToast("Service status updated!", "success");
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to update status: " + ex.Message, "error");
            }
        }

        // ============ GridView Edit Mode ============
        protected void gvServices_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvServices.EditIndex = e.NewEditIndex;
            LoadServices(ViewState["CurrentFilter"]?.ToString() ?? "All");
        }

        protected void gvServices_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvServices.EditIndex = -1;
            LoadServices(ViewState["CurrentFilter"]?.ToString() ?? "All");
        }

        // ============ GridView Update (with SweetAlert Validation) ============
        protected void gvServices_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int serviceId = Convert.ToInt32(gvServices.DataKeys[e.RowIndex].Value);

            try
            {
                DropDownList ddlServiceType = (DropDownList)gvServices.Rows[e.RowIndex].FindControl("ddlEditServiceType");
                TextBox txtTitle = (TextBox)gvServices.Rows[e.RowIndex].FindControl("txtEditTitle");
                TextBox txtDescription = (TextBox)gvServices.Rows[e.RowIndex].FindControl("txtEditDescription");
                FileUpload fuImage = (FileUpload)gvServices.Rows[e.RowIndex].FindControl("fuEditImage");

                if (ddlServiceType == null || txtTitle == null || txtDescription == null)
                {
                    ShowAlert("Error", "Could not find edit controls!", "error");
                    return;
                }

                string serviceType = ddlServiceType.SelectedValue;
                string title = txtTitle.Text.Trim();
                string description = txtDescription.Text.Trim();

                // Validation
                if (string.IsNullOrEmpty(title))
                {
                    ShowAlert("Validation Error", "Service title cannot be empty!", "warning");
                    return;
                }

                if (string.IsNullOrEmpty(description))
                {
                    ShowAlert("Validation Error", "Service description cannot be empty!", "warning");
                    return;
                }

                // Check if new image is uploaded
                string imagePath = null;
                if (fuImage != null && fuImage.HasFile)
                {
                    // Validate file type
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    string extension = Path.GetExtension(fuImage.FileName).ToLowerInvariant();
                    if (Array.IndexOf(allowedExtensions, extension) == -1)
                    {
                        ShowAlert("Invalid File Type", "Only image files (JPG, PNG, GIF, WEBP) are allowed!", "error");
                        return;
                    }

                    // Validate file size
                    if (fuImage.PostedFile.ContentLength > 5 * 1024 * 1024)
                    {
                        ShowAlert("File Too Large", "Image size must be less than 5MB!", "error");
                        return;
                    }

                    var serviceSize = ImageHelper.RecommendedDimensions.ServiceCard;
                    imagePath = SaveUploadedImageWithResize(
                        fuImage,
                        "Service",
                        serviceSize.Width,
                        serviceSize.Height
                    );

                    if (string.IsNullOrEmpty(imagePath))
                    {
                        ShowAlert("Upload Error", "Failed to upload new image!", "error");
                        return;
                    }
                }

                // Update database
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = imagePath != null
                        ? @"UPDATE ServicesCMS 
                           SET ServiceType = @ServiceType, ServiceTitle = @ServiceTitle, 
                               ServiceDescription = @ServiceDescription, ImagePath = @ImagePath, 
                               LastUpdated = GETDATE() 
                           WHERE ServiceID = @ServiceID"
                        : @"UPDATE ServicesCMS 
                           SET ServiceType = @ServiceType, ServiceTitle = @ServiceTitle, 
                               ServiceDescription = @ServiceDescription, LastUpdated = GETDATE() 
                           WHERE ServiceID = @ServiceID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ServiceType", serviceType);
                        cmd.Parameters.AddWithValue("@ServiceTitle", title);
                        cmd.Parameters.AddWithValue("@ServiceDescription", description);
                        cmd.Parameters.AddWithValue("@ServiceID", serviceId);

                        if (imagePath != null)
                        {
                            cmd.Parameters.AddWithValue("@ImagePath", imagePath);
                        }

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowAlert("Success!", "Service updated successfully!", "success");
                gvServices.EditIndex = -1;
                LoadServices(ViewState["CurrentFilter"]?.ToString() ?? "All");
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to update service: " + ex.Message, "error");
            }
        }

        // ============ GridView Delete (Soft Delete) ============
        protected void gvServices_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int serviceId = Convert.ToInt32(gvServices.DataKeys[e.RowIndex].Value);
            DeleteService(serviceId);
        }

        // ============ Hidden Delete Button Click (for SweetAlert confirmation) ============
        protected void btnHiddenDelete_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(hdnDeleteServiceID.Value))
            {
                int serviceId = Convert.ToInt32(hdnDeleteServiceID.Value);
                DeleteService(serviceId);
                hdnDeleteServiceID.Value = ""; // Clear the hidden field
            }
        }

        // ============ Delete Service Method ============
        private void DeleteService(int serviceId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE ServicesCMS SET IsActive = 0, LastUpdated = GETDATE() WHERE ServiceID = @ServiceID", conn);
                    cmd.Parameters.AddWithValue("@ServiceID", serviceId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowAlert("Deleted!", "Service removed successfully!", "success");
                LoadServices(ViewState["CurrentFilter"]?.ToString() ?? "All");
            }
            catch (Exception ex)
            {
                ShowAlert("Error", "Failed to remove service: " + ex.Message, "error");
            }
        }

        // ============ Save Uploaded Image with Resize ============
        private string SaveUploadedImageWithResize(FileUpload fileUpload, string prefix, int targetWidth, int targetHeight)
        {
            try
            {
                string errorMessage;
                if (!ImageHelper.ValidateImage(fileUpload.PostedFile.InputStream, out errorMessage))
                {
                    ShowAlert("Invalid Image", errorMessage, "warning");
                    return null;
                }

                fileUpload.PostedFile.InputStream.Position = 0;

                string extension = Path.GetExtension(fileUpload.FileName).ToLowerInvariant();
                string folderPath = Server.MapPath("~/Uploads/Services/");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filename = prefix + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                string savePath = Path.Combine(folderPath, filename);

                ImageHelper.ResizeAndSaveImage(
                    fileUpload.PostedFile.InputStream,
                    savePath,
                    targetWidth,
                    targetHeight,
                    maintainAspect: true
                );

                return "/Uploads/Services/" + filename;
            }
            catch (Exception ex)
            {
                ShowAlert("Upload Error", "Failed to process image: " + ex.Message, "error");
                return null;
            }
        }

        // ============ SweetAlert Methods ============
        private void ShowAlert(string title, string message, string icon)
        {
            string script = $@"
                Swal.fire({{
                    title: '{EscapeJavaScript(title)}',
                    text: '{EscapeJavaScript(message)}',
                    icon: '{icon}',
                    confirmButtonColor: '#2563eb',
                    confirmButtonText: 'OK'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "alert_" + Guid.NewGuid(), script, true);
        }

        private void ShowAlertWithCallback(string title, string message, string icon, string callback)
        {
            string script = $@"
                Swal.fire({{
                    title: '{EscapeJavaScript(title)}',
                    text: '{EscapeJavaScript(message)}',
                    icon: '{icon}',
                    confirmButtonColor: '#2563eb',
                    confirmButtonText: 'OK'
                }}).then((result) => {{
                    if (result.isConfirmed && typeof {callback} === 'function') {{
                        {callback}();
                    }}
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "alert_" + Guid.NewGuid(), script, true);
        }

        private void ShowToast(string message, string icon)
        {
            string script = $@"
                const Toast = Swal.mixin({{
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 3000,
                    timerProgressBar: true,
                    didOpen: (toast) => {{
                        toast.addEventListener('mouseenter', Swal.stopTimer)
                        toast.addEventListener('mouseleave', Swal.resumeTimer)
                    }}
                }});
                Toast.fire({{
                    icon: '{icon}',
                    title: '{EscapeJavaScript(message)}'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "toast_" + Guid.NewGuid(), script, true);
        }

        private string EscapeJavaScript(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("'", "\\'")
                       .Replace("\r", "\\r")
                       .Replace("\n", "\\n")
                       .Replace("\"", "\\\"");
        }
    }
}