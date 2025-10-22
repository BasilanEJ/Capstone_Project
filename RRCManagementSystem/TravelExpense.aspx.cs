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

        // Philippine Regions and Cities mapping (kept for context)
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
                    Response.Redirect("Login.aspx");
                    return;
                }

                LoadRegionDropdowns();
                BindTravelExpenses();
            }
        }

        /// <summary>
        /// Load regions into both form and filter dropdowns
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
        }

        /// <summary>
        /// Populate cities when region is selected
        /// </summary>
        protected void ddlRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlCity.Items.Clear();

            if (string.IsNullOrEmpty(ddlRegion.SelectedValue))
            {
                ddlCity.Items.Add(new ListItem("-- Select Region First --", ""));
                return;
            }

            ddlCity.Items.Add(new ListItem("-- Select City --", ""));

            if (RegionCities.TryGetValue(ddlRegion.SelectedValue, out var cities))
            {
                foreach (var city in cities.OrderBy(x => x))
                {
                    ddlCity.Items.Add(new ListItem(city, city));
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
        /// Save or update travel expense
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

                int travelExpenseId = Convert.ToInt32(hfTravelExpenseID.Value);
                int userId = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : 0;

                // Determine IsActive value
                bool isActive = true; // Default to Active when adding a new record

                // If updating (ID > 0), use the value stored in the hidden field from the edit load
                if (travelExpenseId > 0)
                {
                    isActive = Convert.ToBoolean(hfIsActive.Value);
                }


                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("sp_TravelExpenses_InsertOrUpdate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TravelExpenseID", travelExpenseId);
                    cmd.Parameters.AddWithValue("@Region", ddlRegion.SelectedValue);
                    cmd.Parameters.AddWithValue("@City", ddlCity.SelectedValue);
                    cmd.Parameters.AddWithValue("@TravelPrice", price);
                    cmd.Parameters.AddWithValue("@IsActive", isActive); // Use the determined value
                    cmd.Parameters.AddWithValue("@UpdatedBy", userId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                string message = travelExpenseId == 0 ? "Travel expense added successfully!" : "Travel expense updated successfully!";
                ShowMessage(message, "success");

                ClearForm();
                BindTravelExpenses();
                UpdatePanelMain.Update();
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Duplicate key error
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
        /// Handle GridView row commands (Edit/Delete)
        /// </summary>
        protected void gvTravelExpenses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int travelExpenseId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditExpense")
            {
                LoadTravelExpenseForEdit(travelExpenseId);
            }
            else if (e.CommandName == "DeleteExpense")
            {
                DeleteTravelExpense(travelExpenseId);
            }
        }

        /// <summary>
        /// Load travel expense data for editing
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
                        hfTravelExpenseID.Value = reader["TravelExpenseID"].ToString();

                        string region = reader["Region"].ToString();
                        ddlRegion.SelectedValue = region;

                        // Trigger city dropdown population
                        ddlRegion_SelectedIndexChanged(null, null);

                        ddlCity.SelectedValue = reader["City"].ToString();
                        txtTravelPrice.Text = Convert.ToDecimal(reader["TravelPrice"]).ToString("F2");

                        // Store the current IsActive status in the new hidden field
                        hfIsActive.Value = Convert.ToBoolean(reader["IsActive"]).ToString();

                        lblFormTitle.Text = "Edit Travel Expense";
                        btnCancel.Visible = true;
                        btnSave.Text = "Update Travel Expense";
                    }
                }
            }

            UpdatePanelMain.Update();
        }

        /// <summary>
        /// Delete travel expense
        /// </summary>
        private void DeleteTravelExpense(int travelExpenseId)
        {
            try
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
                BindTravelExpenses();
                UpdatePanelMain.Update();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error deleting travel expense: {ex.Message}", "danger");
            }
        }

        /// <summary>
        /// Cancel edit mode
        /// </summary>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
            UpdatePanelMain.Update();
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
        /// Clear form fields
        /// </summary>
        private void ClearForm()
        {
            hfTravelExpenseID.Value = "0";
            hfIsActive.Value = "True"; // Ensure hidden status defaults to True
            ddlRegion.SelectedIndex = 0;
            ddlCity.Items.Clear();
            ddlCity.Items.Add(new ListItem("-- Select Region First --", ""));
            txtTravelPrice.Text = "";
            lblFormTitle.Text = "Add Travel Expense";
            btnCancel.Visible = false;
            btnSave.Text = "Save Travel Expense";
            lblMessage.Visible = false;
        }

        /// <summary>
        /// Display message to user
        /// </summary>
        private void ShowMessage(string message, string type)
        {
            string icon;
            string title;

            // Map the severity type to the correct SweetAlert icon (C# 7.3 compatible switch)
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
                    icon = "error"; // SweetAlert uses 'error' for danger/red alerts
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
                // Passing three arguments: title, escapedMessage, and icon
                $"showSwalMessage('{title}', '{escapedMessage}', '{icon}');",
                true
            );

            lblMessage.Visible = false;
        }
    }
}