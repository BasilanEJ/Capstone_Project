using System;
using System.Configuration;
using System.Data;
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
        private const string UploadVirtualFolder = "~/Uploads/InquiryPhotos/";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            var role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (Page.Form != null) Page.Form.Enctype = "multipart/form-data";

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
            string message = string.IsNullOrWhiteSpace(txtMessage.Text) ? "N/A" : txtMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contact))
            {
                ShowSwal("Missing Fields", "Please fill in Email and Contact Number.", "warning");
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
                // optional upload
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
                    fuPhoto.SaveAs(Path.Combine(uploadsPhysicalPath, uniqueName));
                    photoPath = UploadVirtualFolder + uniqueName;
                }

                int newId = InsertInquiry_SP(
                    email, contact, message, photoPath,
                    (txtFirstName.Text ?? "").Trim(),
                    (txtMiddleName.Text ?? "").Trim(),
                    (txtLastName.Text ?? "").Trim(),
                    (txtStreet.Text ?? "").Trim(),
                    (txtBarangay.Text ?? "").Trim(),
                    (txtCity.Text ?? "").Trim(),
                    (txtRegion.Text ?? "").Trim(),
                    (txtCountry.Text ?? "").Trim(),
                    (txtLandmark.Text ?? "").Trim()
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

        private int InsertInquiry_SP(
            string email, string contact, string message, string photoPath,
            string firstName, string middleName, string lastName,
            string street, string barangay, string city, string region, string country, string landmark)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInquiry_Create", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
                cmd.Parameters.Add("@ContactNumber", SqlDbType.NVarChar, 20).Value = contact;
                cmd.Parameters.Add("@Message", SqlDbType.NVarChar).Value = (object)message ?? DBNull.Value;
                cmd.Parameters.Add("@PhotoPath", SqlDbType.NVarChar, 255).Value = (object)photoPath ?? DBNull.Value;

                cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(firstName) ? (object)DBNull.Value : firstName;
                cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(middleName) ? (object)DBNull.Value : middleName;
                cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(lastName) ? (object)DBNull.Value : lastName;

                cmd.Parameters.Add("@StreetAndUnit", SqlDbType.NVarChar, 255).Value = string.IsNullOrWhiteSpace(street) ? (object)DBNull.Value : street;
                cmd.Parameters.Add("@Barangay", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(barangay) ? (object)DBNull.Value : barangay;
                cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(city) ? (object)DBNull.Value : city;
                cmd.Parameters.Add("@Region", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(region) ? (object)DBNull.Value : region;
                cmd.Parameters.Add("@Country", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(country) ? (object)DBNull.Value : country;
                cmd.Parameters.Add("@Landmark", SqlDbType.NVarChar, 255).Value = string.IsNullOrWhiteSpace(landmark) ? (object)DBNull.Value : landmark;

                conn.Open();
                object result = cmd.ExecuteScalar();
                return (result != null && int.TryParse(result.ToString(), out int id)) ? id : 0;
            }
        }

        private bool HasPermission(int userId, string moduleName, string permissionColumn)
        {
            string perm = (permissionColumn == "CanView" || permissionColumn == "CanAdd" ||
                           permissionColumn == "CanEdit" || permissionColumn == "CanDelete")
                          ? permissionColumn : "CanView";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;
                cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value = perm;

                conn.Open();
                object val = cmd.ExecuteScalar();
                return val != null && Convert.ToBoolean(val);
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
