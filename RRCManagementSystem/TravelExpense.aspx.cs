using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class TravelExpense : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        // Philippine Regions and Cities mapping
        private static readonly Dictionary<string, List<string>> RegionCities = new Dictionary<string, List<string>>
        {
            {
                "NCR", new List<string> {
                    "Quezon City", "Manila", "Makati", "Caloocan", "Las Piñas", "Pasig",
                    "Taguig", "Valenzuela", "Pasay", "Malabon", "Mandaluyong", "Marikina",
                    "Muntinlupa", "Navotas", "San Juan", "Pateros", "Parañaque"
                }
            },
            {
                "REGION I - ILOCOS REGION", new List<string> {
                    "Alaminos", "Batac", "Candon", "Laoag", "Vigan", "San Fernando",
                    "San Carlos", "Dagupan", "Urdaneta"
                }
            },
            {
                "REGION II - CAGAYAN VALLEY", new List<string> {
                    "Cauayan", "Tuguegarao", "Ilagan", "Santiago"
                }
            },
            {
                "REGION III - CENTRAL LUZON", new List<string> {
                    "San Fernando", "Angeles", "Olongapo", "Balanga", "Baliwag", "Cabanatuan",
                    "Gapan", "Mabalacat", "Malolos", "Meycauayan", "Muñoz", "Palayan",
                    "San Jose", "San Jose del Monte", "Tarlac City"
                }
            },
            {
                "REGION IV-A - CALABARZON", new List<string> {
                    "Cavite", "Batangas", "Lucena", "Antipolo", "Bacoor", "Biñan", "Cabuyao",
                    "Calaca", "Calamba", "Carmona", "Dasmariñas", "General Trias", "Imus",
                    "Lipa", "San Pablo", "San Pedro", "Santa Rosa", "Santo Tomas", "Tagaytay",
                    "Tanauan", "Tayabas", "Trece Martires"
                }
            },
            {
                "REGION IV-B - MIMAROPA", new List<string> {
                    "Puerto Princesa", "Calapan"
                }
            },
            {
                "REGION V - BICOL REGION", new List<string> {
                    "Legazpi", "Naga", "Iriga", "Ligao", "Masbate City", "Sorsogon City", "Tabaco"
                }
            },
            {
                "REGION VI - WESTERN VISAYAS", new List<string> {
                    "Iloilo City", "Passi", "Bacolod", "Roxas City"
                }
            },
            {
                "REGION VII - CENTRAL VISAYAS", new List<string> {
                    "Cebu City", "Dumaguete", "Lapu-Lapu City", "Mandaue", "Bogo", "Carcar",
                    "Danao", "Naga", "Tagbilaran", "Talisay", "Toledo"
                }
            }
        };


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if user is SuperAdmin
                if (Session["Role"] == null || Session["Role"].ToString() != "SuperAdmin")
                {
                    Response.Redirect("Login.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                LoadRegionDropdowns();
                BindTravelExpenses();
            }
        }

        /// <summary>
        /// Load regions into all dropdowns
        /// </summary>
        private void LoadRegionDropdowns()
        {
            // Load form dropdown
            ddlRegion.Items.Clear();
            ddlRegion.Items.Add(new ListItem("-- Select Region --", ""));
            foreach (var region in RegionCities.Keys.OrderBy(x => x))
            {
                ddlRegion.Items.Add(new ListItem(region, region));
            }

            // Load filter dropdown
            ddlFilterRegion.Items.Clear();
            ddlFilterRegion.Items.Add(new ListItem("All Regions", ""));
            foreach (var region in RegionCities.Keys.OrderBy(x => x))
            {
                ddlFilterRegion.Items.Add(new ListItem(region, region));
            }

            // Load edit modal dropdown
            ddlEditRegion.Items.Clear();
            ddlEditRegion.Items.Add(new ListItem("-- Select Region --", ""));
            foreach (var region in RegionCities.Keys.OrderBy(x => x))
            {
                ddlEditRegion.Items.Add(new ListItem(region, region));
            }
        }

        /// <summary>
        /// Populate cities when region is selected (Add form)
        /// </summary>
        protected void ddlRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateCities(ddlRegion, ddlCity);
        }

        /// <summary>
        /// Populate cities when region is selected (Edit modal)
        /// </summary>
        protected void ddlEditRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateCities(ddlEditRegion, ddlEditCity);
            UpdatePanelMain.Update();
        }

        /// <summary>
        /// Helper method to populate cities
        /// </summary>
        private void PopulateCities(DropDownList ddlRegionControl, DropDownList ddlCityControl)
        {
            ddlCityControl.Items.Clear();

            if (string.IsNullOrEmpty(ddlRegionControl.SelectedValue))
            {
                ddlCityControl.Items.Add(new ListItem("-- Select Region First --", ""));
                return;
            }

            ddlCityControl.Items.Add(new ListItem("-- Select City --", ""));

            if (RegionCities.TryGetValue(ddlRegionControl.SelectedValue, out var cities))
            {
                foreach (var city in cities.OrderBy(x => x))
                {
                    ddlCityControl.Items.Add(new ListItem(city, city));
                }
            }
        }

        /// <summary>
        /// Bind travel expenses to GridView
        /// </summary>
        private void BindTravelExpenses(string regionFilter = null, string citySearch = null)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_TravelExpenses_GetAll", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RegionFilter", (object)regionFilter ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CitySearch", (object)citySearch ?? DBNull.Value);

                con.Open();
                DataTable dt = new DataTable();
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }

                gvTravelExpenses.DataSource = dt;
                gvTravelExpenses.DataBind();

                lblTotalCount.Text = dt.Rows.Count.ToString();
            }
        }

        /// <summary>
        /// Save new travel expense (Add form only)
        /// </summary>
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(ddlRegion.SelectedValue))
                {
                    ShowMessage("Please select a region.", "warning");
                    return;
                }

                if (string.IsNullOrEmpty(ddlCity.SelectedValue))
                {
                    ShowMessage("Please select a city.", "warning");
                    return;
                }

                if (string.IsNullOrEmpty(txtTravelPrice.Text) || !decimal.TryParse(txtTravelPrice.Text, out decimal price) || price < 0)
                {
                    ShowMessage("Please enter a valid travel price.", "warning");
                    return;
                }

                int userId = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("sp_TravelExpenses_InsertOrUpdate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TravelExpenseID", 0); // Always 0 for new records
                    cmd.Parameters.AddWithValue("@Region", ddlRegion.SelectedValue);
                    cmd.Parameters.AddWithValue("@City", ddlCity.SelectedValue);
                    cmd.Parameters.AddWithValue("@TravelPrice", price);
                    cmd.Parameters.AddWithValue("@IsActive", true); // Always active for new records
                    cmd.Parameters.AddWithValue("@UpdatedBy", userId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowMessage("Travel expense added successfully!", "success");
                ClearAddForm();
                BindTravelExpenses();
                UpdatePanelMain.Update();
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    ShowMessage("This region and city combination already exists!", "danger");
                }
                else
                {
                    ShowMessage($"Database error: {sqlEx.Message}", "danger");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error: {ex.Message}", "danger");
            }
        }

        /// <summary>
        /// Handle GridView row commands (Edit only - Delete now handled by btnDeleteHidden_Click)
        /// </summary>
        protected void gvTravelExpenses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int travelExpenseId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditExpense")
            {
                LoadTravelExpenseForEdit(travelExpenseId);
            }
            // DeleteExpense is now handled by btnDeleteHidden_Click via SweetAlert
        }

        /// <summary>
        /// Load travel expense data for editing in modal
        /// </summary>
        private void LoadTravelExpenseForEdit(int travelExpenseId)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_TravelExpenses_GetByID", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TravelExpenseID", travelExpenseId);

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hfEditTravelExpenseID.Value = reader["TravelExpenseID"].ToString();

                        string region = reader["Region"].ToString();
                        ddlEditRegion.SelectedValue = region;

                        // Trigger city dropdown population
                        PopulateCities(ddlEditRegion, ddlEditCity);

                        ddlEditCity.SelectedValue = reader["City"].ToString();
                        txtEditTravelPrice.Text = Convert.ToDecimal(reader["TravelPrice"]).ToString("F2");

                        // Store the current IsActive status
                        hfEditIsActive.Value = Convert.ToBoolean(reader["IsActive"]).ToString();
                    }
                }
            }

            // JavaScript will open the modal
            ScriptManager.RegisterStartupScript(this, GetType(), "OpenModal", "openEditModal();", true);
            UpdatePanelMain.Update();
        }

        /// <summary>
        /// Update travel expense from modal
        /// </summary>
        protected void btnUpdateExpense_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(ddlEditRegion.SelectedValue))
                {
                    ShowMessage("Please select a region.", "warning");
                    return;
                }

                if (string.IsNullOrEmpty(ddlEditCity.SelectedValue))
                {
                    ShowMessage("Please select a city.", "warning");
                    return;
                }

                if (string.IsNullOrEmpty(txtEditTravelPrice.Text) || !decimal.TryParse(txtEditTravelPrice.Text, out decimal price) || price < 0)
                {
                    ShowMessage("Please enter a valid travel price.", "warning");
                    return;
                }

                int travelExpenseId = Convert.ToInt32(hfEditTravelExpenseID.Value);
                int userId = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;
                bool isActive = Convert.ToBoolean(hfEditIsActive.Value);

                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("sp_TravelExpenses_InsertOrUpdate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TravelExpenseID", travelExpenseId);
                    cmd.Parameters.AddWithValue("@Region", ddlEditRegion.SelectedValue);
                    cmd.Parameters.AddWithValue("@City", ddlEditCity.SelectedValue);
                    cmd.Parameters.AddWithValue("@TravelPrice", price);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);
                    cmd.Parameters.AddWithValue("@UpdatedBy", userId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowMessage("Travel expense updated successfully!", "success");

                // Close modal
                ScriptManager.RegisterStartupScript(this, GetType(), "CloseModal", "closeEditModal();", true);

                ClearEditForm();
                BindTravelExpenses();
                UpdatePanelMain.Update();
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    ShowMessage("This region and city combination already exists!", "danger");
                }
                else
                {
                    ShowMessage($"Database error: {sqlEx.Message}", "danger");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error: {ex.Message}", "danger");
            }
        }

        /// <summary>
        /// Handle delete from hidden button (triggered by SweetAlert)
        /// </summary>
        protected void btnDeleteHidden_Click(object sender, EventArgs e)
        {
            try
            {
                int travelExpenseId = Convert.ToInt32(hfDeleteID.Value);

                if (travelExpenseId > 0)
                {
                    using (var con = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand("sp_TravelExpenses_Delete", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TravelExpenseID", travelExpenseId);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    ShowMessage("Travel expense deleted successfully!", "success");

                    // Clear the hidden field
                    hfDeleteID.Value = "0";

                    BindTravelExpenses();
                    UpdatePanelMain.Update();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error deleting travel expense: {ex.Message}", "danger");
            }
        }

        /// <summary>
        /// Apply filter to GridView
        /// </summary>
        protected void ApplyFilter(object sender, EventArgs e)
        {
            string regionFilter = ddlFilterRegion.SelectedValue;
            string citySearch = txtSearchCity.Text.Trim();

            BindTravelExpenses(
                string.IsNullOrEmpty(regionFilter) ? null : regionFilter,
                string.IsNullOrEmpty(citySearch) ? null : citySearch
            );

            UpdatePanelMain.Update();
        }

        /// <summary>
        /// Clear filter
        /// </summary>
        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            ddlFilterRegion.SelectedIndex = 0;
            txtSearchCity.Text = "";
            BindTravelExpenses();
            UpdatePanelMain.Update();
        }

        /// <summary>
        /// Clear add form fields
        /// </summary>
        private void ClearAddForm()
        {
            hfTravelExpenseID.Value = "0";
            hfIsActive.Value = "True";
            ddlRegion.SelectedIndex = 0;
            ddlCity.Items.Clear();
            ddlCity.Items.Add(new ListItem("-- Select Region First --", ""));
            txtTravelPrice.Text = "";
            lblMessage.Visible = false;
        }

        /// <summary>
        /// Clear edit modal fields
        /// </summary>
        private void ClearEditForm()
        {
            hfEditTravelExpenseID.Value = "0";
            hfEditIsActive.Value = "True";
            ddlEditRegion.SelectedIndex = 0;
            ddlEditCity.Items.Clear();
            ddlEditCity.Items.Add(new ListItem("-- Select Region First --", ""));
            txtEditTravelPrice.Text = "";
        }

        /// <summary>
        /// Display message to user
        /// </summary>
        private void ShowMessage(string message, string type)
        {
            string icon;
            string title;

            switch (type)
            {
                case "success":
                    icon = "success";
                    title = "Success! 🎉";
                    break;
                case "warning":
                    icon = "warning";
                    title = "Attention!";
                    break;
                case "danger":
                    icon = "error";
                    title = "Error! 🛑";
                    break;
                case "info":
                    icon = "info";
                    title = "Information";
                    break;
                default:
                    icon = "info";
                    title = "Information";
                    break;
            }

            string escapedMessage = message.Replace("'", "\\'");

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "ShowSwal",
                $"showSwalMessage('{title}', '{escapedMessage}', '{icon}');",
                true
            );

            lblMessage.Visible = false;
        }
    }
}