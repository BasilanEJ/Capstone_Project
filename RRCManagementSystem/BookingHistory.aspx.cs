using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class BookingHistory : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 🔐 Require login
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string role = Session["Role"].ToString();

            // 🔐 Deny access for SuperAdmin and Inspector
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

           

            if (!IsPostBack)
            {
                LoadBookingHistory();
            }
        }


        private void LoadBookingHistory(DateTime? startDate = null, DateTime? endDate = null, string status = "")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                SELECT 
                    b.BookingID,
                    c.Name AS ClientName,
                    b.ServiceNames,
                    b.ScheduledDate,
                    b.StartTime,
                    b.Status,
                    b.Price
                FROM Bookings b
                INNER JOIN Clients c ON b.ClientID = c.ClientID
                WHERE 1=1";

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        query += " AND b.ScheduledDate BETWEEN @StartDate AND @EndDate";
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        query += " AND b.Status = @Status";
                    }

                    query += " ORDER BY b.ScheduledDate DESC";

                    SqlCommand cmd = new SqlCommand(query, con);

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
                        cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvBookingHistory.DataSource = dt;
                    gvBookingHistory.DataBind();

                    lblMessage.Text = dt.Rows.Count == 0 ? "No bookings found for selected filters." : "";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"❌ Error loading booking history: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }


        protected void btnFilter_Click(object sender, EventArgs e)
        {
            DateTime startDate, endDate;

            bool isStartDateValid = DateTime.TryParse(txtStartDate.Text.Trim(), out startDate);
            bool isEndDateValid = DateTime.TryParse(txtEndDate.Text.Trim(), out endDate);
            string selectedStatus = ddlStatus.SelectedValue;

            if (isStartDateValid && isEndDateValid)
            {
                LoadBookingHistory(startDate, endDate, selectedStatus);
            }
            else
            {
                LoadBookingHistory(null, null, selectedStatus);
            }
        }

        protected void gvBookingHistory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBookingHistory.PageIndex = e.NewPageIndex;
            btnFilter_Click(null, null); // Re-filter when paging
        }


    }
}