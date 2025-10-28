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

        // Permission flags
        private bool canAdd = false;
        private bool canEdit = false;
        private bool canDelete = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx", false); // Pass false
                return;
            }

            string role = Session["Role"].ToString();

            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx", false); // Pass false
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            if (!HasPermission(userId, "ManageItem", "CanView"))
            {
                Response.Redirect("~/Unauthorized.aspx", false); // Pass false
                return;
            }

            canAdd = HasPermission(userId, "ManageItem", "CanAdd");
            canEdit = HasPermission(userId, "ManageItem", "CanEdit");
            canDelete = HasPermission(userId, "ManageItem", "CanDelete");

            if (!IsPostBack)
            {
                LoadItems();
            }
        }

        // ================== Permissions ==================
        private bool HasPermission(int adminId, string moduleName, string permission)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", permission);

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

        // ================== Load Items ==================
        private void LoadItems(string typeFilter = "All")
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInventory_List", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

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

                        // Show "No items found" message if the table is empty
                        lblMessage.Text = dt.Rows.Count == 0 ? "⚠ No items found." : "";
                        lblMessage.Visible = dt.Rows.Count == 0;

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

                        // Check if any restock messages were generated
                        bool showRestockNotice = !string.IsNullOrEmpty(restockMsg);

                        // If messages exist, set the label text and show the container div
                        if (showRestockNotice)
                        {
                            lblRestockNotice.Text = $"<strong>⚠️ Restocking Reminder:</strong><br/>{restockMsg}";
                            divRestockNotice.Visible = true;
                        }
                        else
                        {
                            // If no messages, hide the entire container div
                            divRestockNotice.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error loading items: " + ex.Message;
                lblMessage.Visible = true;
                divRestockNotice.Visible = false; // Always hide the notice on error
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

        // ================== GridView Commands ==================
        protected void gvItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // The CommandArgument is already the integer ItemID
            int itemId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditItem")
            {
                if (!canEdit)
                {
                    ShowSweetAlert("Error", "❌ You do not have permission to edit items.", "error");
                    return;
                }

                string encodedId = EncodeID(itemId.ToString());
                Response.Redirect($"EditItem.aspx?ItemID={encodedId}", false);
                return; 
            }
            else if (e.CommandName == "AddStocks")
            {
                if (!canAdd)
                {
                    ShowSweetAlert("Error", "❌ You do not have permission to add stocks.", "error");
                    return;
                }

                hiddenItemId.Value = itemId.ToString();
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowAddStockModal", $"showAddStockModal({itemId});", true);
            }
            else if (e.CommandName == "DeleteItem")
            {
                if (!canDelete)
                {
                    ShowSweetAlert("Error", "❌ You do not have permission to delete items.", "error");
                    return;
                }

                // The hidden field is already set by the OnClientClick
                // We do nothing here, the postback is handled by btnConfirmDelete_Click
            }
        }

        // ================== GridView Row Styling ==================
        protected void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int quantity = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Quantity"));
                int itemId = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "ItemID"));
                string itemType = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "Type"));

                Label lblQuantity = (Label)e.Row.FindControl("lblQuantity");

                if (lblQuantity != null)
                {
                    // Dynamic thresholds based on item type
                    bool isLowStock = false;

                    if (itemType == "Bottled Chemical" && quantity <= 3)
                    {
                        isLowStock = true;
                    }
                    else if (itemType == "Sachet Pack Chemical" && quantity <= 10)
                    {
                        isLowStock = true;
                    }
                    else if (itemType == "Safety Gear" && quantity <= 5)
                    {
                        isLowStock = true;
                    }

                    if (isLowStock)
                    {
                        lblQuantity.ForeColor = System.Drawing.Color.Red;
                        lblQuantity.Font.Bold = true;
                        // Add a pulsing animation class for critical items
                        e.Row.CssClass += " bg-red-50";
                    }
                    else if (quantity > 5 && quantity <= 10)
                    {
                        lblQuantity.ForeColor = System.Drawing.Color.Orange;
                        lblQuantity.Font.Bold = true;
                    }
                    else
                    {
                        lblQuantity.ForeColor = System.Drawing.Color.Green;
                    }
                }

                // Permission-based button styling
                var btnEditWrapper = e.Row.FindControl("btnEditWrapper") as System.Web.UI.HtmlControls.HtmlGenericControl;
                var btnAddWrapper = e.Row.FindControl("btnAddWrapper") as System.Web.UI.HtmlControls.HtmlGenericControl;
                var btnDeleteWrapper = e.Row.FindControl("btnDeleteWrapper") as System.Web.UI.HtmlControls.HtmlGenericControl;

                if (!canEdit && btnEditWrapper != null)
                {
                    btnEditWrapper.Attributes["class"] += " link-button-disabled";
                    btnEditWrapper.Attributes["title"] = "You do not have permission to edit.";
                }
                if (!canAdd && btnAddWrapper != null)
                {
                    btnAddWrapper.Attributes["class"] += " link-button-disabled";
                    btnAddWrapper.Attributes["title"] = "You do not have permission to add stocks.";
                }
                if (!canDelete && btnDeleteWrapper != null)
                {
                    btnDeleteWrapper.Attributes["class"] += " link-button-disabled";
                    btnDeleteWrapper.Attributes["title"] = "You do not have permission to delete.";
                }

                LinkButton btnAdd = (LinkButton)e.Row.FindControl("btnAddStocks");
                if (btnAdd != null)
                {
                    btnAdd.OnClientClick = $"showAddStockModal({itemId}); return false;";
                }

                LinkButton btnDelete = (LinkButton)e.Row.FindControl("btnDelete");
                if (btnDelete != null)
                {
                    btnDelete.OnClientClick = $"confirmDelete('{hiddenItemId.ClientID}', '{itemId}'); return false;";
                }
            }
        }

        // ================== Add Stocks Logic ==================
        protected void btnConfirmAddStock_Click(object sender, EventArgs e)
        {
            if (!canAdd)
            {
                ShowSweetAlert("Error", "❌ You do not have permission to add stocks.", "error");
                return;
            }

            if (int.TryParse(hiddenItemId.Value, out int itemId) &&
                int.TryParse(txtAddQuantity.Text, out int quantity) &&
                quantity > 0)
            {
                try
                {
                    AddStock(itemId, quantity);
                    LoadItems(ddlType.SelectedValue);
                    txtAddQuantity.Text = "";
                    ShowSweetAlert("Success", "Stocks added successfully!", "success");
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Error", "❌ Error adding stocks: " + ex.Message, "error");
                }
            }
            else
            {
                ShowSweetAlert("Error", "❌ Invalid Item ID or Quantity.", "error");
            }
        }

        private void AddStock(int itemId, int quantity)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spInventory_AddStock", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ItemID", itemId);
                cmd.Parameters.AddWithValue("@AddQty", quantity);

                SqlParameter newQuantityParam = new SqlParameter("@NewQuantity", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(newQuantityParam);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ================== Delete Logic ==================
        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("btnConfirmDelete_Click triggered");

            if (!int.TryParse(hiddenItemId.Value, out int itemId))
            {
                ShowSweetAlert("Error", "Invalid Item ID.", "error");
                return;
            }

            DeleteItem(itemId);
            LoadItems(ddlType.SelectedValue);

            ShowSweetAlert("Deleted!", "Item deleted successfully.", "success");
        }




        private string DeleteItem(int itemId)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spInventory_Delete", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemID", itemId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }


                return null;
            }
            catch (SqlException ex)
            {

                if (ex.Number == 547)
                {

                    return "This item cannot be deleted because it is already assigned to a booking.";
                }


                return "A database error occurred. Could not delete the item.";
            }
            catch (Exception ex)
            {

                return "A general error occurred: " + ex.Message;
            }
        }




        // ================== SweetAlert Helper ==================
        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $"Swal.fire({{title: '{title}', text: '{message}', icon: '{icon}'}});";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, true);
        }

        // ================== Encode ID ==================
        public static string EncodeID(string id)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(id);
            return Convert.ToBase64String(bytes)
                .Replace("=", "")
                .Replace("+", "-")
                .Replace("/", "_");
        }
    }
}