using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class SetServicePricing : Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadServices();

                if (Request.QueryString["ServiceID"] != null)
                {
                    string serviceId = Request.QueryString["ServiceID"];
                    if (ddlServices.Items.FindByValue(serviceId) != null)
                    {
                        ddlServices.SelectedValue = serviceId;
                        LoadPricingTiers();
                    }
                }
            }
        }

        private void LoadServices()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT ServiceID, Name FROM dbo.Services WHERE Status = 'Active' ORDER BY Name", conn))
            {
                conn.Open();
                ddlServices.DataSource = cmd.ExecuteReader();
                ddlServices.DataTextField = "Name";
                ddlServices.DataValueField = "ServiceID";
                ddlServices.DataBind();
            }

            ddlServices.Items.Insert(0, new ListItem("-- Select Service --", ""));
        }

        protected void ddlServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlServices.SelectedValue))
                LoadPricingTiers();
            else
            {
                gvPricing.DataSource = null;
                gvPricing.DataBind();
            }
        }

        private void LoadPricingTiers()
        {
            int serviceId = Convert.ToInt32(ddlServices.SelectedValue);

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT T.TierID, S.Name AS ServiceName, T.MinSQM, T.MaxSQM, T.FlatPrice
                FROM dbo.ServicePricingTiers T
                INNER JOIN dbo.Services S ON T.ServiceID = S.ServiceID
                WHERE T.ServiceID = @ServiceID
                ORDER BY T.MinSQM", conn))
            {
                cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    gvPricing.DataSource = dt;
                    gvPricing.DataBind();
                }
            }
        }

        protected void btnAddTier_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlServices.SelectedValue))
            {
                ShowAlert("warning", "Selection Required", "Please select a service first.");
                return;
            }

            int serviceId = Convert.ToInt32(ddlServices.SelectedValue);
            int minSQM = int.TryParse(txtMinSQM.Text, out var min) ? min : 0;
            int maxSQM = int.TryParse(txtMaxSQM.Text, out var max) ? max : 0;
            decimal flatPrice = decimal.TryParse(txtFlatPrice.Text, out var price) ? price : 0;

            if (flatPrice <= 0)
            {
                ShowAlert("warning", "Invalid Price", "Please enter a valid flat price.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("dbo.spServicePricing_InsertTier", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                    cmd.Parameters.AddWithValue("@MinSQM", minSQM);
                    cmd.Parameters.AddWithValue("@MaxSQM", maxSQM == 0 ? (object)DBNull.Value : maxSQM);
                    cmd.Parameters.AddWithValue("@FlatPrice", flatPrice);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowAlert("success", "Success!", "Pricing tier added successfully.");

                txtMinSQM.Text = txtMaxSQM.Text = txtFlatPrice.Text = string.Empty;
                LoadPricingTiers();
            }
            catch (Exception ex)
            {
                ShowAlert("error", "Error", $"Failed to add pricing tier: {ex.Message}");
            }
        }

        protected void gvPricing_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvPricing.EditIndex = e.NewEditIndex;
            LoadPricingTiers();
        }

        protected void gvPricing_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPricing.EditIndex = -1;
            LoadPricingTiers();
        }

        protected void gvPricing_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int tierId = Convert.ToInt32(gvPricing.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvPricing.Rows[e.RowIndex];

                // ✅ CORRECT WAY: Find TextBox controls by their position in edit mode
                // Cell 0: TierID (Hidden)
                // Cell 1: ServiceName (ReadOnly)
                // Cell 2: MinSQM (Editable)
                // Cell 3: MaxSQM (Editable)
                // Cell 4: FlatPrice (Editable)
                // Cell 5: Edit Button
                // Cell 6: Delete Button

                TextBox txtMinSQM = (TextBox)row.Cells[2].Controls[0];
                TextBox txtMaxSQM = (TextBox)row.Cells[3].Controls[0];
                TextBox txtFlatPrice = (TextBox)row.Cells[4].Controls[0];

                int minSQM = Convert.ToInt32(txtMinSQM.Text);
                int maxSQM = Convert.ToInt32(txtMaxSQM.Text);
                decimal flatPrice = Convert.ToDecimal(txtFlatPrice.Text);

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE dbo.ServicePricingTiers 
                    SET MinSQM = @MinSQM, 
                        MaxSQM = @MaxSQM, 
                        FlatPrice = @FlatPrice 
                    WHERE TierID = @TierID", conn))
                {
                    cmd.Parameters.AddWithValue("@TierID", tierId);
                    cmd.Parameters.AddWithValue("@MinSQM", minSQM);
                    cmd.Parameters.AddWithValue("@MaxSQM", maxSQM);
                    cmd.Parameters.AddWithValue("@FlatPrice", flatPrice);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                gvPricing.EditIndex = -1;
                LoadPricingTiers();

                ShowAlert("success", "Updated!", "Pricing tier updated successfully.");
            }
            catch (Exception ex)
            {
                ShowAlert("error", "Update Failed", $"Error updating pricing tier: {ex.Message}");
            }
        }

        protected void gvPricing_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int tierId = Convert.ToInt32(gvPricing.DataKeys[e.RowIndex].Value);

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.ServicePricingTiers WHERE TierID = @TierID", conn))
                {
                    cmd.Parameters.AddWithValue("@TierID", tierId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                LoadPricingTiers();
                ShowAlert("success", "Deleted!", "Pricing tier deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowAlert("error", "Delete Failed", $"Error deleting pricing tier: {ex.Message}");
            }
        }

        // Helper method to show SweetAlert
        private void ShowAlert(string icon, string title, string text)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icon}',
                    title: '{title}',
                    text: '{text.Replace("'", "\\'")}',
                    confirmButtonColor: '#3b82f6'
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowAlert", script, true);
        }
    }
}