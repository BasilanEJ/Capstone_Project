using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class EditEquipment : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["RRCConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string id = Request.QueryString["id"];
                if (!string.IsNullOrEmpty(id))
                {
                    LoadEquipment(id);
                }
                else
                {
                    lblMessage.Text = "Invalid Equipment ID.";
                    btnUpdate.Enabled = false;
                }
            }
        }

        private void LoadEquipment(string id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT EquipmentID, EquipmentName, Status, ImagePath FROM Equipment WHERE EquipmentID = @ID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ID", id);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtEquipmentID.Text = reader["EquipmentID"].ToString();
                    txtEquipmentName.Text = reader["EquipmentName"].ToString();
                    ddlStatus.SelectedValue = reader["Status"].ToString();
                    imagePreview.Src = reader["ImagePath"].ToString(); // note: imagePreview is an <img>, not <asp:Image>
                }
                else
                {
                    lblMessage.Text = "Equipment not found.";
                    btnUpdate.Enabled = false;
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            string id = txtEquipmentID.Text.Trim();
            string name = txtEquipmentName.Text.Trim();
            string status = ddlStatus.SelectedValue;
            string imagePath = imagePreview.Src;

            if (fuEquipmentImage.HasFile)
            {
                string ext = Path.GetExtension(fuEquipmentImage.FileName).ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    string fileName = $"Equipment_{id}{ext}";
                    string savePath = "~/Uploads/Equipment/" + fileName;
                    string physicalPath = Server.MapPath(savePath);

                    fuEquipmentImage.SaveAs(physicalPath);
                    imagePath = savePath;
                }
                else
                {
                    lblMessage.Text = "Only JPG, JPEG, and PNG files are allowed.";
                    return;
                }
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"UPDATE Equipment 
                                 SET EquipmentName = @Name, 
                                     Status = @Status, 
                                     ImagePath = @ImagePath 
                                 WHERE EquipmentID = @ID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@ImagePath", imagePath);

                con.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "success", @"
                        Swal.fire({
                            icon: 'success',
                            title: 'Updated!',
                            text: 'Equipment has been updated successfully.',
                            confirmButtonColor: '#007bff'
                        }).then(() => {
                            window.location.href = 'ViewEquipment.aspx';
                        });
                    ", true);
                }
                else
                {
                    lblMessage.Text = "Update failed. Please try again.";
                }
            }
        }
    }
}
