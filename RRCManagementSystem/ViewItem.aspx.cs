using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ViewItem : Page
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int adminId = Convert.ToInt32(Session["AdminID"]);

            // 🔐 Check CanView permission for ManageItem module
            if (!HasViewPermission(adminId, "ManageItem"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadItems();
            }
        }

        private bool HasViewPermission(int adminId, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CanView FROM AdminPermissions WHERE UserID = @UserID AND ModuleName = @ModuleName";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", adminId);
                cmd.Parameters.AddWithValue("@ModuleName", moduleName);

                try
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
                catch
                {
                    return false;
                }
            }
        }

        private void LoadItems(string typeFilter = "All")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT ItemID, Name, Type, Quantity, ExpirationDate, CreatedAt, ImagePath, ExcessML FROM Inventory";
                    if (typeFilter != "All")
                    {
                        query += " WHERE Type = @Type";
                    }
                    query += " ORDER BY CreatedAt DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (typeFilter != "All")
                        {
                            cmd.Parameters.AddWithValue("@Type", typeFilter);
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvItems.DataSource = dt;
                        gvItems.DataBind();

                        lblMessage.Text = dt.Rows.Count == 0 ? "⚠ No items found." : "";

                        // 🔔 Restocking Alert Logic
                        string restockMsg = "";

                        foreach (DataRow row in dt.Rows)
                        {
                            string itemName = row["Name"].ToString();
                            string itemType = row["Type"].ToString();
                            int quantity = Convert.ToInt32(row["Quantity"]);

                            if (itemType == "Bottled Chemical" && quantity <= 3)
                            {
                                restockMsg += $"• <strong>{itemName}</strong> is a bottled chemical and only <strong>{quantity} bottles</strong> are left. Restock soon!<br/>";
                            }
                            else if (itemType == "Sachet Pack Chemical" && quantity <= 10)
                            {
                                restockMsg += $"• <strong>{itemName}</strong> is a sachet pack chemical and only <strong>{quantity} packs</strong> are left. Restock soon!<br/>";
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
                Response.Redirect($"EditItem.aspx?ItemID={itemId}");
            }
        }
    }
}