using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class CreateInquiry : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        // Where to save uploaded photos (change if you prefer a different path)
        private const string UploadVirtualFolder = "~/Uploads/InquiryPhotos/";

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            var role = Session["Role"].ToString();
            // 🔐 Deny SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Ensure master page form supports file uploads
            if (Page.Form != null)
            {
                Page.Form.Enctype = "multipart/form-data";
            }

            if (!IsPostBack)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                if (!HasPermission(userId, "ManageInquiry", "CanView"))
                {
                    Response.Redirect("~/Unauthorized.aspx");
                    return;
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            if (!HasPermission(userId, "ManageInquiry", "CanAdd"))
            {
                ShowSwal("Permission Denied", "You do not have permission to add inquiries.", "error");
                return;
            }

            string email = (txtEmail.Text ?? "").Trim();
            string contact = (txtContact.Text ?? "").Trim();
            string message = (txtMessage.Text ?? "").Trim();

            // If message is empty, set to N/A
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "N/A";
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contact))
            {
                string which = string.Join(" and ",
                    string.IsNullOrWhiteSpace(email) ? new[] { "Email" } : Array.Empty<string>())
                    + (string.IsNullOrWhiteSpace(contact)
                        ? (string.IsNullOrWhiteSpace(email) ? "Contact Number" : " and Contact Number")
                        : "");
                ShowSwal("Missing Fields", $"Please fill in {(string.IsNullOrWhiteSpace(which) ? "the required fields" : which)}.", "warning");
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowSwal("Invalid Email", "Please enter a valid email address.", "warning");
                return;
            }
            if (!Regex.IsMatch(contact, @"^\d{11}$"))
            {
                ShowSwal("Invalid Contact Number", "Contact Number must be exactly 11 digits.", "warning");
                return;
            }

            string photoPath = null;

            try
            {
                // Handle optional photo upload
                if (fuPhoto.HasFile)
                {
                    if (!ValidateUpload(fuPhoto))
                    {
                        ShowSwal("Invalid File", "Only .jpg, .jpeg, .png up to 5 MB are allowed.", "warning");
                        return;
                    }

                    string uploadsPhysicalPath = Server.MapPath(UploadVirtualFolder);
                    if (!Directory.Exists(uploadsPhysicalPath))
                        Directory.CreateDirectory(uploadsPhysicalPath);

                    string ext = Path.GetExtension(fuPhoto.FileName);
                    string uniqueName = $"inq_{Guid.NewGuid():N}{ext}";
                    string savePath = Path.Combine(uploadsPhysicalPath, uniqueName);
                    fuPhoto.SaveAs(savePath);

                    photoPath = UploadVirtualFolder + uniqueName;
                }

                int newId = InsertInquiry(
                    email, contact, message,
                    (txtFirstName.Text ?? "").Trim(),
                    (txtMiddleName.Text ?? "").Trim(),
                    (txtLastName.Text ?? "").Trim(),
                    (txtStreet.Text ?? "").Trim(),
                    (txtBarangay.Text ?? "").Trim(),
                    (txtCity.Text ?? "").Trim(),
                    (txtRegion.Text ?? "").Trim(),
                    (txtCountry.Text ?? "").Trim(),
                    (txtLandmark.Text ?? "").Trim(),
                    photoPath
                );

                if (newId > 0)
                {
                    Response.Redirect("AllInquiry.aspx", endResponse: true);
                }
                else
                {
                    ShowSwal("Error", "Failed to save inquiry. Please try again.", "error");
                }
            }
            catch (Exception ex)
            {
                ShowSwal("Error", "An error occurred: " + HttpUtility.HtmlEncode(ex.Message), "error");
            }
        }

        private int InsertInquiry(
            string email, string contact, string message,
            string firstName, string middleName, string lastName,
            string street, string barangay, string city, string region, string country, string landmark,
            string photoPath)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO InquirySimple
                    (Email, ContactNumber, Message, SubmittedAt, PhotoPath,
                     FirstName, MiddleName, LastName, StreetAndUnit, Barangay, City, Region, Country, Landmark)
                VALUES
                    (@Email, @ContactNumber, @Message, DATEADD(HOUR, 8, GETUTCDATE()), @PhotoPath,
                     @FirstName, @MiddleName, @LastName, @StreetAndUnit, @Barangay, @City, @Region, @Country, @Landmark);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@ContactNumber", contact);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@PhotoPath", (object)photoPath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FirstName", string.IsNullOrWhiteSpace(firstName) ? (object)DBNull.Value : firstName);
                cmd.Parameters.AddWithValue("@MiddleName", string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName);
                cmd.Parameters.AddWithValue("@LastName", string.IsNullOrWhiteSpace(lastName) ? (object)DBNull.Value : lastName);
                cmd.Parameters.AddWithValue("@StreetAndUnit", string.IsNullOrWhiteSpace(street) ? (object)DBNull.Value : street);
                cmd.Parameters.AddWithValue("@Barangay", string.IsNullOrWhiteSpace(barangay) ? (object)DBNull.Value : barangay);
                cmd.Parameters.AddWithValue("@City", string.IsNullOrWhiteSpace(city) ? (object)DBNull.Value : city);
                cmd.Parameters.AddWithValue("@Region", string.IsNullOrWhiteSpace(region) ? (object)DBNull.Value : region);
                cmd.Parameters.AddWithValue("@Country", string.IsNullOrWhiteSpace(country) ? (object)DBNull.Value : country);
                cmd.Parameters.AddWithValue("@Landmark", string.IsNullOrWhiteSpace(landmark) ? (object)DBNull.Value : landmark);

                conn.Open();
                object result = cmd.ExecuteScalar();
                return (result != null && int.TryParse(result.ToString(), out int id)) ? id : 0;
            }
        }

        private static bool ValidateUpload(FileUpload fu)
        {
            const int MAX = 5 * 1024 * 1024;
            if (fu.PostedFile.ContentLength <= 0 || fu.PostedFile.ContentLength > MAX) return false;

            string ext = (Path.GetExtension(fu.FileName) ?? "").ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png") return false;

            string mime = (fu.PostedFile.ContentType ?? "").ToLowerInvariant();
            if (!mime.StartsWith("image/")) return false;

            return true;
        }

        private static bool IsValidEmail(string email)
            => Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private bool HasPermission(int userId, string moduleName, string permissionColumn)
        {
            string col = permissionColumn;
            if (col != "CanView" && col != "CanAdd" && col != "CanEdit" && col != "CanDelete")
                col = "CanView";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand($@"
                SELECT {col}
                FROM AdminPermissions
                WHERE UserID = @UserID AND ModuleName = @ModuleName;", conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && Convert.ToBoolean(result);
            }
        }

        private void ShowSwal(string title, string text, string icon)
        {
            string script = $@"
                setTimeout(function(){{
                    Swal.fire({{
                        title: {ToJsString(title)},
                        text: {ToJsString(text)},
                        icon: '{icon}'
                    }});
                }}, 0);";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), script, true);
        }

        private static string ToJsString(string s)
        {
            if (s == null) return "''";
            return "'" + s.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "").Replace("\n", "\\n") + "'";
        }
    }
}
