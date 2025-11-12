using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class InspectorAreaScope : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        private static readonly Dictionary<string, string[]> RegionCities =
            new Dictionary<string, string[]>
        {
            { "NCR", new[] { "Quezon City", "Manila", "Makati", "Caloocan", "Las Piñas", "Pasig", "Taguig", "Valenzuela", "Pasay", "Malabon", "Mandaluyong", "Marikina", "Muntinlupa", "Navotas", "San Juan", "Pateros", "Parañaque" } },
            { "Region I", new[] { "Alaminos", "Batac", "Candon", "Laoag", "Vigan", "San Fernando", "San Carlos", "Dagupan", "Urdaneta" } },
            { "Region II", new[] { "Cauayan", "Tuguegarao", "Ilagan", "Santiago" } },
            { "Region III", new[] { "San Fernando", "Angeles", "Olongapo", "Balanga", "Baliwag", "Cabanatuan", "Gapan", "Mabalacat", "Malolos", "Meycauayan", "Muñoz", "Palayan", "San Jose", "San Jose del Monte", "Tarlac City" } },
            { "Region IV-A", new[] { "Cavite", "Batangas", "Lucena", "Antipolo", "Bacoor", "Biñan", "Cabuyao", "Calaca", "Calamba", "Carmona", "Dasmariñas", "General Trias", "Imus", "Lipa", "San Pablo", "San Pedro", "Santa Rosa", "Santo Tomas", "Tagaytay", "Tanauan", "Tayabas", "Trece Martires" } },
            { "Region IV-B", new[] { "Puerto Princesa", "Calapan" } },
            { "Region V", new[] { "Legazpi", "Naga", "Iriga", "Ligao", "Masbate City", "Sorsogon City", "Tabaco" } },
            { "Region VI", new[] { "Iloilo City", "Passi", "Bacolod", "Roxas City" } },
            { "Region VII", new[] { "Cebu City", "Dumaguete", "Lapu-Lapu City", "Mandaue", "Bogo", "Carcar", "Danao", "Naga", "Tagbilaran", "Talisay", "Toledo" } },
        };

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null || Session["Role"].ToString() != "SuperAdmin")
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // REMOVE the delete handling code from here completely

            if (IsPostBack)
            {
                // Only recreate checkboxes if a region is selected
                if (!string.IsNullOrEmpty(ddlRegion.SelectedValue))
                {
                    BindCityCheckboxes(ddlRegion.SelectedValue);
                }
            }

            if (!IsPostBack)
            {
                BindInspectors();
                LoadAreaScopes();
            }
        }
        protected void btnDeleteHidden_Click(object sender, EventArgs e)
        {
            int areaScopeId;
            if (int.TryParse(hdnDeleteId.Value, out areaScopeId))
            {
                try
                {
                    RemoveAreaScope(areaScopeId);
                    LoadAreaScopes();

                    ShowSweetAlert("Removed! 👋", "Area scope successfully removed.", "success");
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Error!", $"An error occurred: {ex.Message}", "error");
                }

                UpdatePanel2.Update();
            }
        }

        private void HandleDelete(int areaScopeId)
        {
            try
            {
                RemoveAreaScope(areaScopeId);
                LoadAreaScopes();

                ShowSweetAlert("Removed! 👋", "Area scope successfully removed.", "success");

                // Force update of the grid panel
                UpdatePanel2.Update();
            }
            catch (Exception ex)
            {
                ShowSweetAlert("Error!", $"An error occurred while removing the scope: {ex.Message}", "error");
                UpdatePanel2.Update();
            }
        }

        private void ShowSweetAlert(string title, string text, string icon)
        {
            string script = $@"
            <script type='text/javascript'>
                Swal.fire({{
                    title: '{title}',
                    text: '{text}',
                    icon: '{icon}',
                    confirmButtonText: 'OK',
                    customClass: {{
                        confirmButton: 'swal-custom-button'
                    }},
                    buttonsStyling: false
                }});
            </script>";
            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", script, false);
        }

        private void BindInspectors()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("usp_GetActiveInspectors", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);

                ddlInspector.DataSource = dt;
                ddlInspector.DataTextField = "Name";
                ddlInspector.DataValueField = "UserID";
                ddlInspector.DataBind();
                ddlInspector.Items.Insert(0, new ListItem("-- Select Inspector --", ""));
            }
        }

        private void LoadAreaScopes()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("usp_GetAllAreaScopes", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);

                gvAreaScopes.DataSource = dt;
                gvAreaScopes.DataBind();

                // Update custom pagination
                RenderCustomPagination();
            }
            UpdatePanel2.Update();
        }

        private void RenderCustomPagination()
        {
            int currentPage = gvAreaScopes.PageIndex;
            int totalPages = gvAreaScopes.PageCount;

            // Hide pagination if only one page or no data
            if (totalPages <= 1)
            {
                pnlCustomPagination.Visible = false;
                return;
            }

            pnlCustomPagination.Visible = true;

            // Enable/disable navigation buttons
            btnFirstPage.Enabled = currentPage > 0;
            btnPrevPage.Enabled = currentPage > 0;
            btnNextPage.Enabled = currentPage < totalPages - 1;
            btnLastPage.Enabled = currentPage < totalPages - 1;

            // Add disabled class styling through attributes
            if (!btnFirstPage.Enabled)
                btnFirstPage.Attributes["style"] = "opacity: 0.5; cursor: not-allowed;";
            else
                btnFirstPage.Attributes.Remove("style");

            if (!btnPrevPage.Enabled)
                btnPrevPage.Attributes["style"] = "opacity: 0.5; cursor: not-allowed;";
            else
                btnPrevPage.Attributes.Remove("style");

            if (!btnNextPage.Enabled)
                btnNextPage.Attributes["style"] = "opacity: 0.5; cursor: not-allowed;";
            else
                btnNextPage.Attributes.Remove("style");

            if (!btnLastPage.Enabled)
                btnLastPage.Attributes["style"] = "opacity: 0.5; cursor: not-allowed;";
            else
                btnLastPage.Attributes.Remove("style");

            // Display page info with current page number
            int totalRecords = GetTotalRecordCount();
            int startRecord = totalRecords == 0 ? 0 : (currentPage * gvAreaScopes.PageSize) + 1;
            int endRecord = Math.Min((currentPage + 1) * gvAreaScopes.PageSize, totalRecords);

            litPageInfo.Text = $"Page {currentPage + 1} of {totalPages} | Showing {startRecord}-{endRecord} of {totalRecords} records";
        }

        private int GetTotalRecordCount()
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.InspectorAreaScope", conn))
            {
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        protected void gvAreaScopes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAreaScopes.PageIndex = e.NewPageIndex;
            LoadAreaScopes();
        }

        // Custom pagination button handlers
        protected void btnFirstPage_Click(object sender, EventArgs e)
        {
            gvAreaScopes.PageIndex = 0;
            LoadAreaScopes();
        }

        protected void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (gvAreaScopes.PageIndex > 0)
            {
                gvAreaScopes.PageIndex--;
                LoadAreaScopes();
            }
        }

        protected void btnNextPage_Click(object sender, EventArgs e)
        {
            if (gvAreaScopes.PageIndex < gvAreaScopes.PageCount - 1)
            {
                gvAreaScopes.PageIndex++;
                LoadAreaScopes();
            }
        }

        protected void btnLastPage_Click(object sender, EventArgs e)
        {
            gvAreaScopes.PageIndex = gvAreaScopes.PageCount - 1;
            LoadAreaScopes();
        }

        private void BindCityCheckboxes(string region)
        {
            pnlCityCheckboxes.Controls.Clear();

            if (RegionCities.ContainsKey(region))
            {
                string[] cities = RegionCities[region];

                var selectAll = new CheckBox
                {
                    ID = "chkSelectAll",
                    Text = "Select All",
                    AutoPostBack = true,
                    CssClass = "mr-2 mb-2 font-semibold text-blue-700"
                };
                selectAll.CheckedChanged += SelectAll_CheckedChanged;
                pnlCityCheckboxes.Controls.Add(selectAll);
                pnlCityCheckboxes.Controls.Add(new LiteralControl("<hr class='my-1'>"));

                foreach (string city in cities)
                {
                    var chk = new CheckBox
                    {
                        ID = "chk_" + city.Replace(" ", "_").Replace("-", ""),
                        Text = city,
                        Checked = false,
                        CssClass = "mr-2"
                    };
                    pnlCityCheckboxes.Controls.Add(chk);
                    pnlCityCheckboxes.Controls.Add(new LiteralControl("<br/>"));
                }
            }
            else
            {
                pnlCityCheckboxes.Controls.Add(lblCityPlaceholder);
            }
        }

        protected void SelectAll_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox selectAll = (CheckBox)sender;
            foreach (Control control in pnlCityCheckboxes.Controls)
            {
                if (control is CheckBox chk && chk.ID != "chkSelectAll")
                {
                    chk.Checked = selectAll.Checked;
                }
            }
            UpdatePanel1.Update();
        }

        protected void ddlRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedRegion = ddlRegion.SelectedValue;
            BindCityCheckboxes(selectedRegion);
            UpdatePanel1.Update();
        }

        protected void cvCityRequired_ServerValidate(object source, ServerValidateEventArgs args)
        {
            bool isAnyChecked = pnlCityCheckboxes.Controls.Cast<Control>()
                        .OfType<CheckBox>()
                        .Any(chk => chk.Checked && chk.ID != "chkSelectAll");

            args.IsValid = isAnyChecked;
        }

        protected void btnAddScope_Click(object sender, EventArgs e)
        {
            Page.Validate("ScopeGroup");

            if (!Page.IsValid)
            {
                litMessage.Text = "<p class='text-red-500 mt-3'>Please fix the validation errors above.</p>";
                UpdatePanel1.Update();
                return;
            }

            int userId = int.Parse(ddlInspector.SelectedValue);
            string region = ddlRegion.SelectedValue;

            var selectedCities = pnlCityCheckboxes.Controls.Cast<Control>()
                                 .OfType<CheckBox>()
                                 .Where(chk => chk.Checked && chk.ID != "chkSelectAll")
                                 .Select(chk => chk.Text)
                                 .ToList();

            if (selectedCities.Count == 0)
            {
                ShowSweetAlert("Error!", "No cities were selected. Please select at least one city.", "error");
                litMessage.Text = "";
                UpdatePanel1.Update();
                return;
            }

            int successCount = 0;
            int duplicateCount = 0;

            foreach (string city in selectedCities)
            {
                try
                {
                    AddAreaScope(userId, region, city);
                    successCount++;
                }
                catch (SqlException ex) when (ex.Number == 2627)
                {
                    duplicateCount++;
                }
                catch (Exception ex)
                {
                    ShowSweetAlert("Critical Error!", $"A critical error occurred while adding scopes: {ex.Message}", "error");
                    litMessage.Text = "";
                    UpdatePanel1.Update();
                    return;
                }
            }

            LoadAreaScopes();

            ddlInspector.SelectedIndex = 0;
            ddlRegion.SelectedIndex = 0;
            pnlCityCheckboxes.Controls.Clear();
            pnlCityCheckboxes.Controls.Add(lblCityPlaceholder);

            string title = "Area Scopes Added!";
            string icon = "success";
            string text = $"{successCount} area scope(s) successfully added for inspector.";

            if (duplicateCount > 0)
            {
                icon = "warning";
                text += $" ({duplicateCount} assignment(s) were skipped as they already exist).";
            }

            ShowSweetAlert(title, text, icon);
            litMessage.Text = "";
            UpdatePanel1.Update();
        }

        private void AddAreaScope(int userId, string region, string city)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("usp_InsertAreaScope", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Region", region);
                cmd.Parameters.AddWithValue("@City", city);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void RemoveAreaScope(int areaScopeId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("usp_DeleteAreaScope", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AreaScopeID", areaScopeId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}