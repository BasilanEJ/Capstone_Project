using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ViewServicePricing : Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

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
                // Ensure ServiceID is provided in query string
                if (Request.QueryString["ServiceID"] == null)
                {
                    Response.Redirect("ViewServices.aspx");
                    return;
                }

                if (!int.TryParse(Request.QueryString["ServiceID"], out int serviceId))
                {
                    Response.Redirect("ViewServices.aspx");
                    return;
                }

                hfServiceID.Value = serviceId.ToString();
                LoadServicePricing(serviceId);
            }
        }

        /// <summary>
        /// Load all pricing records for a specific service
        /// </summary>
        private void LoadServicePricing(int serviceId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("EJBasilan_admin.spServicePricing_ListByService", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ServiceID", serviceId);

                    var dt = new DataTable();
                    da.Fill(dt);

                    gvServicePricing.DataSource = dt;
                    gvServicePricing.DataBind();
                }
            }
            catch (Exception ex)
            {
                AlertError($"Error loading service pricing: {ex.Message}");
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            // Redirect back to the ViewServices page
            Response.Redirect("ViewServices.aspx");
        }




        /// <summary>
        /// Put GridView into Edit mode
        /// </summary>
        protected void gvServicePricing_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvServicePricing.EditIndex = e.NewEditIndex;
            LoadServicePricing(Convert.ToInt32(hfServiceID.Value));
        }

        /// <summary>
        /// Cancel Edit mode
        /// </summary>
        protected void gvServicePricing_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvServicePricing.EditIndex = -1;
            LoadServicePricing(Convert.ToInt32(hfServiceID.Value));
        }

        /// <summary>
        /// Update the price after editing inline
        /// </summary>
        protected void gvServicePricing_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                // Get PricingID from DataKeys
                int pricingId = Convert.ToInt32(gvServicePricing.DataKeys[e.RowIndex].Value);

                // Find the textbox inside the row
                GridViewRow row = gvServicePricing.Rows[e.RowIndex];
                TextBox txtEditPrice = (TextBox)row.FindControl("txtEditPrice");

                if (txtEditPrice == null)
                {
                    AlertError("Unable to find the price input field.");
                    return;
                }

                // Validate price
                if (!decimal.TryParse(txtEditPrice.Text.Trim(), out decimal newPrice))
                {
                    AlertError("Please enter a valid decimal price.");
                    return;
                }

                // Update the record in the database
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("EJbasilan_admin.spServicePricing_UpdatePrice", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PricingID", pricingId);
                    cmd.Parameters.AddWithValue("@Price", newPrice);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                // Reset grid to normal mode and reload data
                gvServicePricing.EditIndex = -1;
                LoadServicePricing(Convert.ToInt32(hfServiceID.Value));

                AlertSuccess("Price updated successfully.");
            }
            catch (Exception ex)
            {
                AlertError($"Error updating price: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle custom commands like Delete
        /// </summary>
        protected void gvServicePricing_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Only process DeletePrice here
            if (e.CommandName == "DeletePrice")
            {
                if (!int.TryParse(Convert.ToString(e.CommandArgument), out int pricingId)) return;

                try
                {
                    using (var conn = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand("EJbasilan_admin.spServicePricing_Delete", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PricingID", pricingId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    LoadServicePricing(Convert.ToInt32(hfServiceID.Value));
                    AlertSuccess("Pricing deleted successfully.");
                }
                catch (Exception ex)
                {
                    AlertError($"Error deleting pricing: {ex.Message}");
                }
            }
        }

        protected void gvServicePricing_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            // Role-based visibility can be added here later if needed
        }

        // ---------- SweetAlert helper methods ----------
        private void AlertSuccess(string msg) =>
            ClientScript.RegisterStartupScript(
                this.GetType(), Guid.NewGuid().ToString("N"),
                $"Swal.fire('Success','{Js(msg)}','success');", true);

        private void AlertError(string msg) =>
            ClientScript.RegisterStartupScript(
                this.GetType(), Guid.NewGuid().ToString("N"),
                $"Swal.fire('Error','{Js(msg)}','error');", true);

        private static string Js(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace(@"\", @"\\")
                    .Replace("'", @"\'")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n");
        }
    }
}
