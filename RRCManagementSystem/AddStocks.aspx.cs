using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class AddStocks : System.Web.UI.Page
    {
        private readonly string connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        private int itemId;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();
            // 🔐 Only standard admins/managers; block SuperAdmin/Inspector as per your pattern
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Permission: we’ll use CanEdit on ManageItem for stock changes
            int adminId = Convert.ToInt32(Session["UserID"]);
            if (!HasPermission(adminId, "ManageItem", "CanEdit"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            string encodedId = Request.QueryString["ItemID"];
            if (string.IsNullOrEmpty(encodedId) || !TryDecodeID(encodedId, out itemId))
            {
                lblMessage.Text = "❌ Invalid item ID.";
                btnSubmit.Enabled = false;
                return;
            }

            if (!IsPostBack)
            {
                LoadItem();
            }
        }

        private bool HasPermission(int userId, string moduleName, string perm)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", perm);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return false;
            }
        }

        private void LoadItem()
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInventory_GetBasic", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ItemID", itemId);

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        lblItemName.Text = rdr["Name"].ToString();
                        lblItemType.Text = rdr["Type"].ToString();
                        imgItem.ImageUrl = rdr["ImagePath"] == DBNull.Value ? null : rdr["ImagePath"].ToString();
                    }
                    else
                    {
                        lblMessage.Text = "❌ Item not found.";
                        btnSubmit.Enabled = false;
                    }
                }
            }
        }

        private bool TryDecodeID(string encoded, out int decodedId)
        {
            try
            {
                string base64 = encoded.Replace("-", "+").Replace("_", "/");
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                byte[] data = Convert.FromBase64String(base64);
                string idString = System.Text.Encoding.UTF8.GetString(data);
                return int.TryParse(idString, out decodedId);
            }
            catch
            {
                decodedId = 0;
                return false;
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);

            if (!int.TryParse(txtAddQty.Text.Trim(), out int addQty) || addQty <= 0)
            {
                lblMessage.Text = "⚠ Please enter a valid quantity.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            try
            {
                int newQty = 0;
                int affected = 0;

                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInventory_AddStock", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemID", itemId);
                    cmd.Parameters.AddWithValue("@AddQty", addQty);

                    var pOut = cmd.Parameters.Add("@NewQuantity", SqlDbType.Int);
                    pOut.Direction = ParameterDirection.Output;

                    con.Open();
                    // We also SELECT Affected as a row; read it here:
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                            affected = Convert.ToInt32(rdr["Affected"]);
                    }

                    // After reader closes, OUTPUT param is populated
                    if (cmd.Parameters["@NewQuantity"].Value != DBNull.Value)
                        newQty = Convert.ToInt32(cmd.Parameters["@NewQuantity"].Value);
                }

                if (affected == 1)
                {
                    // 🧾 Audit
                    InsertAudit(adminId, $"Added {addQty} stock to ItemID={itemId}. New quantity: {newQty}");

                    string script = @"
<script>
    Swal.fire({
        icon: 'success',
        title: 'Stock Added!',
        text: 'Quantity was added successfully.',
        confirmButtonText: 'OK'
    }).then(() => { window.location.href = 'ViewItem.aspx'; });
</script>";
                    ClientScript.RegisterStartupScript(this.GetType(), "stockAdded", script);
                }
                else
                {
                    lblMessage.Text = "❌ Item not found or update failed.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void InsertAudit(int? adminId, string action)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAudit_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AdminID", (object)adminId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // optional logging
            }
        }
    }
}
