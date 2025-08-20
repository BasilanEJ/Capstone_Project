using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ViewItem : Page
    {
        private readonly string connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Block SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanView permission for ManageItem via SP
            if (!HasViewPermission(userId, "ManageItem"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                gvItems.RowDataBound += gvItems_RowDataBound;
                LoadItems();
            }
        }

        private bool HasViewPermission(int adminId, string moduleName)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", "CanView");

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

        private void LoadItems(string typeFilter = "All")
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInventory_List", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Convert "All" -> NULL so SP returns all types
                    if (string.Equals(typeFilter, "All", StringComparison.OrdinalIgnoreCase))
                        cmd.Parameters.AddWithValue("@Type", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@Type", typeFilter);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        gvItems.DataSource = dt;
                        gvItems.DataBind();

                        lblMessage.Text = dt.Rows.Count == 0 ? "⚠ No items found." : "";

                        // 🔔 Restocking Alert Logic (from SP’s RestockMessage)
                        string restockMsg = "";
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row.Table.Columns.Contains("RestockFlag") &&
                                row.Table.Columns.Contains("RestockMessage") &&
                                row["RestockFlag"] != DBNull.Value &&
                                Convert.ToBoolean(row["RestockFlag"]))
                            {
                                var msg = Convert.ToString(row["RestockMessage"]);
                                if (!string.IsNullOrWhiteSpace(msg))
                                {
                                    restockMsg += msg + "<br/>";
                                }
                            }
                        }

                        lblRestockNotice.Text = !string.IsNullOrEmpty(restockMsg)
                            ? $"<strong>⚠ Restocking Reminder:</strong><br/>{restockMsg}"
                            : "";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error loading items: " + ex.Message;
            }
        }

        protected void ddlType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadItems(ddlType.SelectedValue);
        }

        protected void gvItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvItems.PageIndex = e.NewPageIndex;
            LoadItems(ddlType.SelectedValue);
        }

        protected void gvItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditItem")
            {
                int itemId = Convert.ToInt32(e.CommandArgument);
                string encodedId = EncodeID(itemId.ToString());
                Response.Redirect($"EditItem.aspx?ItemID={encodedId}");
            }
        }

        protected void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int quantity = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Quantity"));
                Label lblQuantity = (Label)e.Row.FindControl("lblQuantity");

                if (lblQuantity != null)
                {
                    if (quantity > 10)
                        lblQuantity.ForeColor = System.Drawing.Color.Black;
                    else if (quantity > 5 && quantity <= 10)
                        lblQuantity.ForeColor = System.Drawing.Color.Goldenrod; // Yellow
                    else
                        lblQuantity.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        protected void btnDeleteHidden_Click(object sender, EventArgs e)
        {
            if (int.TryParse(hiddenItemId.Value, out int itemId))
            {
                try
                {
                    using (var con = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand("dbo.spInventory_Delete", con))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ItemID", itemId);

                        con.Open();
                        var affected = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

                        string script = @"
<script>
    Swal.fire({
        icon: '" + (affected > 0 ? "success" : "warning") + @"',
        title: '" + (affected > 0 ? "Deleted!" : "Not Found") + @"',
        text: '" + (affected > 0 ? "Stocks have been successfully deleted." : "Item was not found.") + @"',
        confirmButtonColor: '#28a745'
    });
</script>";
                        ClientScript.RegisterStartupScript(this.GetType(), "deleteResult", script);
                    }

                    LoadItems(ddlType.SelectedValue);
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "❌ Delete failed: " + ex.Message;
                }
            }
            else
            {
                lblMessage.Text = "❌ Invalid Item ID.";
            }
        }

        // ✅ Encoding the ItemID to hide the real database ID
        public static string EncodeID(string id)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(id);
            return Convert.ToBase64String(bytes).Replace("=", "").Replace("+", "-").Replace("/", "_");
        }
    }
}
