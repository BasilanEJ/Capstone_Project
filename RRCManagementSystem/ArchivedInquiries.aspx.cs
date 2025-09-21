using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class ArchivedInquiries : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadArchivedInquiries();
            }
        }

        private void LoadArchivedInquiries()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("spInquiries_Archived_List", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);

                // Decrypt and rename like in AllInquiry.aspx
                foreach (DataRow row in dt.Rows)
                {
                    if (row["EmailEnc"] != DBNull.Value)
                        row["EmailEnc"] = Helpers.AESHelper.DecryptEmail(row["EmailEnc"].ToString());

                    if (row["ContactEnc"] != DBNull.Value)
                        row["ContactEnc"] = Helpers.AESHelper.DecryptField(row["ContactEnc"].ToString());
                }

                dt.Columns["EmailEnc"].ColumnName = "Email";
                dt.Columns["ContactEnc"].ColumnName = "ContactNumber";

                gvArchived.DataSource = dt;
                gvArchived.DataBind();
            }
        }

        // Restore
        protected void btnRestoreHidden_Click(object sender, EventArgs e)
        {
            int inquiryId = Convert.ToInt32(hfActionInquiryID.Value);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("spInquiry_Restore", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);

                conn.Open();
                int rows = Convert.ToInt32(cmd.ExecuteScalar());

                if (rows > 0)
                {
                    // ✅ SweetAlert with redirect to AllInquiry.aspx
                    ScriptManager.RegisterStartupScript(this, GetType(), "RestoredOK",
                        "Swal.fire('Restored','Inquiry has been moved back to active list.','success').then((result) => { window.location='AllInquiry.aspx'; });", true);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "RestoredErr",
                        "Swal.fire('Error','Inquiry not found or could not be restored.','error').then((result) => { window.location='AllInquiry.aspx'; });", true);
                }
            }
        }


        // Delete Permanently
        protected void btnDeleteHidden_Click(object sender, EventArgs e)
        {
            int inquiryId = Convert.ToInt32(hfActionInquiryID.Value);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("spInquiry_DeletePermanent", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);

                conn.Open();
                int rows = Convert.ToInt32(cmd.ExecuteScalar());

                if (rows > 0)
                {
                    // ✅ SweetAlert with redirect to AllInquiry.aspx
                    ScriptManager.RegisterStartupScript(this, GetType(), "DeletedOK",
                        "Swal.fire('Deleted','Inquiry has been permanently deleted.','success').then((result) => { window.location='AllInquiry.aspx'; });", true);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "DeletedErr",
                        "Swal.fire('Error','Inquiry not found or could not be deleted.','error').then((result) => { window.location='AllInquiry.aspx'; });", true);
                }
            }
        }

    }
}
