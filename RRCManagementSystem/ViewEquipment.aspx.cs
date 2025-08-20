using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class ViewEquipment : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

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

            // 🔐 Check CanView permission for ManageEquipment (via SP)
            if (!HasViewPermission(userId, "ManageEquipment"))
            {
                Response.Redirect("~/Unauthorized.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtFilterDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                LoadEquipmentsWithAvailability(DateTime.Today);
            }
        }

        private bool HasViewPermission(int adminId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", adminId);
                    cmd.Parameters.AddWithValue("@ModuleName", moduleName);
                    cmd.Parameters.AddWithValue("@Permission", "CanView");

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
            catch
            {
                return false;
            }
        }

        private void LoadEquipmentsWithAvailability(DateTime targetDate)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("dbo.spEquipment_ListWithAvailability", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ForDate", SqlDbType.Date).Value = targetDate.Date;

                    // Normalize the dropdown value
                    var wanted = (ddlStatus.SelectedValue ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(wanted) || wanted.Equals("All", StringComparison.OrdinalIgnoreCase))
                    {
                        // send NULL
                        var p = cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20);
                        p.Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = wanted;
                    }

                    cmd.Parameters.Add("@DailyCapacity", SqlDbType.Int).Value = 2;

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        dt.Columns.Add("FormattedID", typeof(string));
                        foreach (DataRow r in dt.Rows)
                            r["FormattedID"] = "Equipment" + ((int)r["EquipmentID"]).ToString("D3");

                        gvEquipment.DataSource = dt;
                        gvEquipment.DataBind();

                        lblMessage.Text = $"✅ Equipments loaded for {targetDate:yyyy-MM-dd}.";
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error loading equipments: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }


        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAndLoadEquipment();
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtFilterDate.Text, out DateTime selectedDate))
                LoadEquipmentsWithAvailability(selectedDate);
            else
            {
                lblMessage.Text = "⚠️ Please select a valid date!";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void FilterAndLoadEquipment()
        {
            DateTime selectedDate = DateTime.TryParse(txtFilterDate.Text, out DateTime date)
                ? date.Date : DateTime.Today;
            LoadEquipmentsWithAvailability(selectedDate);
        }

        protected void gvEquipment_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEquipment.PageIndex = e.NewPageIndex;
            FilterAndLoadEquipment();
        }

        private void ShowSwal(string icon, string title, string text)
        {
            string script = $@"Swal.fire({{
                icon: '{icon}', title: '{title}', text: '{text}',
                showConfirmButton: false, timer: 1800
            }});";
            var key = Guid.NewGuid().ToString();
            if (ScriptManager.GetCurrent(this) != null)
                ScriptManager.RegisterStartupScript(this, GetType(), key, script, true);
            else
                ClientScript.RegisterStartupScript(GetType(), key, script, true);
        }

        protected void gvEquipment_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteEquipment")
            {
                int equipmentId = Convert.ToInt32(e.CommandArgument);
                try
                {
                    int affected = 0;
                    using (var con = new SqlConnection(connectionString))
                    using (var cmd = new SqlCommand("dbo.spEquipment_Delete", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@EquipmentID", equipmentId);
                        con.Open();
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                                affected = Convert.ToInt32(rdr["Affected"]);
                        }
                    }

                    if (affected > 0)
                        ShowSwal("success", "Deleted!", "Equipment deleted successfully.");
                    else
                        ShowSwal("info", "Not found", "Equipment record was not found.");
                }
                catch (Exception ex)
                {
                    ShowSwal("error", "Error", $"Failed to delete: {ex.Message.Replace("'", "\\'")}");
                }

                FilterAndLoadEquipment();
            }
        }

        protected string EncodeID(string id)
        {
            if (string.IsNullOrEmpty(id)) return string.Empty;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(id);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

    }
}
