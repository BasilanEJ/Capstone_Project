using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class EditAdmin : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private int userID;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Allow only SuperAdmins
            if (role != "SuperAdmin")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }


            if (!IsPostBack)
            {
                if (Request.QueryString["UserID"] != null)
                {
                    userID = Convert.ToInt32(Request.QueryString["UserID"]);
                    LoadAdminDetails(userID);
                    LoadPermissions(userID);
                }
                else
                {
                    lblMessage.Text = "⚠ No Admin selected.";
                }

                // Register ItemDataBound after binding
                rptPermissions.ItemDataBound += rptPermissions_ItemDataBound;
            }

            // ✅ Show SweetAlert2 success message after postback
            if (Session["ShowSuccess"] != null && (bool)Session["ShowSuccess"])
            {
                Session.Remove("ShowSuccess"); // Clear the flag

                string script = @"<script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
        <script>
            Swal.fire({
                icon: 'success',
                title: 'Changes Saved',
                text: 'The admin permissions were updated successfully!',
                confirmButtonColor: '#007bff'
            });
        </script>";

                ClientScript.RegisterStartupScript(this.GetType(), "SuccessAlert", script);
            }
        }


        private void LoadAdminDetails(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Name, Email FROM Users WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", id);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        txtName.Text = reader["Name"].ToString();
                        txtEmail.Text = reader["Email"].ToString();
                    }
                }
            }
        }

        private void LoadPermissions(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM AdminPermissions WHERE UserID = @UserID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", id);
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    string[] modules = {
                        "Dashboard",
                        "ManageInquiry",
                        "ClientApproval",
                        "CreateCustomerAccount",
                        "ManageEmployees",
                        "ManageItem",
                        "ManageEquipment",
                        "ManageClient",
                        "ManageBooking",
                        "Sales&Transaction",
                        "ManageSupplier",
                        "ManageServices",
                        "AdminReports",
                        "AdminGuide"
                    };

                    foreach (string module in modules)
                    {
                        if (!dt.AsEnumerable().Any(row => row["ModuleName"].ToString() == module))
                        {
                            DataRow newRow = dt.NewRow();
                            newRow["ModuleName"] = module;
                            newRow["CanView"] = false;
                            newRow["CanAdd"] = false;
                            newRow["CanEdit"] = false;
                            newRow["CanDelete"] = false;
                            dt.Rows.Add(newRow);
                        }
                    }

                    DataView dv = dt.DefaultView;
                    dv.Sort = "ModuleName ASC";

                    rptPermissions.DataSource = dv.ToTable();
                    rptPermissions.DataBind();
                }
            }
        }

        protected void rptPermissions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // No need to modify class here since we're handling class in markup
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (Request.QueryString["UserID"] != null)
            {
                userID = Convert.ToInt32(Request.QueryString["UserID"]);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string updateAdminQuery = @"
                            UPDATE Users
                            SET Name = @Name, Email = @Email
                            WHERE UserID = @UserID";

                        using (SqlCommand cmdUpdate = new SqlCommand(updateAdminQuery, conn, transaction))
                        {
                            cmdUpdate.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                            cmdUpdate.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                            cmdUpdate.Parameters.AddWithValue("@UserID", userID);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        foreach (RepeaterItem item in rptPermissions.Items)
                        {
                            string moduleName = ((HiddenField)item.FindControl("hfModuleName")).Value;
                            bool canView = ((CheckBox)item.FindControl("chkView")).Checked;
                            bool canAdd = ((CheckBox)item.FindControl("chkAdd")).Checked;
                            bool canEdit = ((CheckBox)item.FindControl("chkEdit")).Checked;
                            bool canDelete = ((CheckBox)item.FindControl("chkDelete")).Checked;

                            string upsertQuery = @"
                                MERGE AdminPermissions AS target
                                USING (SELECT @UserID AS UserID, @ModuleName AS ModuleName) AS source
                                ON target.UserID = source.UserID AND target.ModuleName = source.ModuleName
                                WHEN MATCHED THEN
                                    UPDATE SET CanView = @CanView, CanAdd = @CanAdd, CanEdit = @CanEdit, 
                                               CanDelete = @CanDelete
                                WHEN NOT MATCHED THEN
                                    INSERT (UserID, ModuleName, CanView, CanAdd, CanEdit, CanDelete)
                                    VALUES (@UserID, @ModuleName, @CanView, @CanAdd, @CanEdit, @CanDelete);";

                            using (SqlCommand cmd = new SqlCommand(upsertQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userID);
                                cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                                cmd.Parameters.AddWithValue("@CanView", canView);
                                cmd.Parameters.AddWithValue("@CanAdd", canAdd);
                                cmd.Parameters.AddWithValue("@CanEdit", canEdit);
                                cmd.Parameters.AddWithValue("@CanDelete", canDelete);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        lblMessage.Text = "✅ Admin updated successfully!";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        lblMessage.Text = "⚠ Error updating admin: " + ex.Message;

                        string errorScript = $@"<script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
                        <script>
                            Swal.fire({{
                                icon: 'error',
                                title: 'Error Saving',
                                text: '{ex.Message.Replace("'", "\\'")}',
                                confirmButtonColor: '#dc3545'
                            }});
                        </script>";

                        ClientScript.RegisterStartupScript(this.GetType(), "ErrorAlert", errorScript);
                    }
                }
            }
        }
    }
}