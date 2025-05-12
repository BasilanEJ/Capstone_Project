using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class BookService : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["ClientID"] != null)
            {
                LoadQuotation();
            }
        }

        private void LoadQuotation()
        {
            int clientId = Convert.ToInt32(Session["ClientID"]);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT TOP 1 ServiceNames, SQM, Price FROM PendingQuotations WHERE ClientID = @ClientID ORDER BY CreatedAt DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", clientId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    lblServices.Text = reader["ServiceNames"].ToString();
                    lblSQM.Text = reader["SQM"].ToString();
                    lblPrice.Text = $"\u20B1{Convert.ToDecimal(reader["Price"]):N2}";
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "noQuote", "Swal.fire('No Quotation', 'Please wait for the inspector to create a quotation.', 'info');", true);
                    btnBook.Enabled = false;
                }
            }
        }

        protected void btnBook_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDate.Text) || string.IsNullOrWhiteSpace(txtTime.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "missing", "Swal.fire('Missing Info', 'Please select a preferred date and time.', 'warning');", true);
                return;
            }

            DateTime selectedDateTime;
            try
            {
                selectedDateTime = DateTime.Parse($"{txtDate.Text} {txtTime.Text}");
            }
            catch
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalid", "Swal.fire('Invalid Input', 'Invalid date or time format.', 'error');", true);
                return;
            }

            if (selectedDateTime < DateTime.Now)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "pastDate", "Swal.fire('Invalid Schedule', 'Please choose a future date and time.', 'error');", true);
                return;
            }

            int clientId = Convert.ToInt32(Session["ClientID"]);
            string serviceNames = lblServices.Text;
            int sqm = int.TryParse(lblSQM.Text, out int parsedSQM) ? parsedSQM : 0;
            decimal price = decimal.TryParse(lblPrice.Text.Replace("\u20B1", ""), out decimal parsedPrice) ? parsedPrice : 0;
            string notes = txtNotes.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string insertQuery = @"
            INSERT INTO Bookings (ClientID, ServiceNames, SQM, Price, ScheduledDate, StartTime, Notes, Status, CreatedAt)
            VALUES (@ClientID, @ServiceNames, @SQM, @Price, @Date, @Time, @Notes, 'Pending', GETDATE())";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                cmd.Parameters.AddWithValue("@ServiceNames", serviceNames);
                cmd.Parameters.AddWithValue("@SQM", sqm);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@Date", selectedDateTime.Date);
                cmd.Parameters.AddWithValue("@Time", selectedDateTime.TimeOfDay);
                cmd.Parameters.AddWithValue("@Notes", notes);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "booked", "Swal.fire('Success', 'Your service has been booked!', 'success');", true);
            txtDate.Text = "";
            txtTime.Text = "";
            txtNotes.Text = "";
        }

    }
}