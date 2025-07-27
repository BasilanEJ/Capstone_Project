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
                string query = @"SELECT TOP 1 QuotationID, ServiceNames, ServiceID, SQM, Price, IsContract 
                                 FROM PendingQuotations 
                                 WHERE ClientID = @ClientID 
                                 ORDER BY CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", clientId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    lblServices.Text = reader["ServiceNames"].ToString();
                    lblSQM.Text = reader["SQM"].ToString();
                    lblPrice.Text = $"\u20B1{Convert.ToDecimal(reader["Price"]):N2}";
                    hfIsContract.Value = reader["IsContract"] != DBNull.Value ? reader["IsContract"].ToString() : "False";
                    hfQuotationID.Value = reader["QuotationID"].ToString();
                    ViewState["ServiceIDs"] = reader["ServiceID"].ToString(); // comma-separated IDs
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "noQuote", @"
                        Swal.fire('No Quotation', 
                                  'Please wait for the inspector to create a quotation.', 
                                  'info');", true);
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

            if (!DateTime.TryParse($"{txtDate.Text} {txtTime.Text}", out DateTime selectedDateTime))
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
            string serviceIDsRaw = ViewState["ServiceIDs"]?.ToString();
            int sqm = int.TryParse(lblSQM.Text, out int parsedSQM) ? parsedSQM : 0;
            decimal price = decimal.TryParse(lblPrice.Text.Replace("\u20B1", ""), out decimal parsedPrice) ? parsedPrice : 0;
            string notes = txtNotes.Text.Trim();
            bool isContract = hfIsContract.Value == "True";
            int quotationId = int.TryParse(hfQuotationID.Value, out int id) ? id : 0;

            if (string.IsNullOrEmpty(serviceIDsRaw))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noServices", "Swal.fire('Error', 'Missing service details.', 'error');", true);
                return;
            }

            int bookingId = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Insert into Bookings table and get BookingID
                string insertBooking = @"
                    INSERT INTO Bookings 
                        (ClientID, ServiceNames, SQM, Price, ScheduledDate, StartTime, Notes, Status, CreatedAt, IsContract)
                    OUTPUT INSERTED.BookingID
                    VALUES 
                        (@ClientID, @ServiceNames, @SQM, @Price, @ScheduledDate, @StartTime, @Notes, 'Pending', GETDATE(), @IsContract);";

                SqlCommand bookingCmd = new SqlCommand(insertBooking, conn);
                bookingCmd.Parameters.AddWithValue("@ClientID", clientId);
                bookingCmd.Parameters.AddWithValue("@ServiceNames", serviceNames);
                bookingCmd.Parameters.AddWithValue("@SQM", sqm);
                bookingCmd.Parameters.AddWithValue("@Price", parsedPrice);
                bookingCmd.Parameters.AddWithValue("@ScheduledDate", selectedDateTime.Date);
                bookingCmd.Parameters.AddWithValue("@StartTime", selectedDateTime.TimeOfDay);
                bookingCmd.Parameters.AddWithValue("@Notes", notes);
                bookingCmd.Parameters.AddWithValue("@IsContract", isContract);

                object result = bookingCmd.ExecuteScalar();
                if (result != null)
                {
                    bookingId = Convert.ToInt32(result);
                }

                // Insert into BookingServices
                foreach (string sid in serviceIDsRaw.Split(','))
                {
                    if (int.TryParse(sid.Trim(), out int serviceId))
                    {
                        string insertService = @"INSERT INTO BookingServices (BookingID, ServiceID) VALUES (@BookingID, @ServiceID)";
                        SqlCommand serviceCmd = new SqlCommand(insertService, conn);
                        serviceCmd.Parameters.AddWithValue("@BookingID", bookingId);
                        serviceCmd.Parameters.AddWithValue("@ServiceID", serviceId);
                        serviceCmd.ExecuteNonQuery();
                    }
                }

                // Delete from PendingQuotations
                string deleteQuote = "DELETE FROM PendingQuotations WHERE QuotationID = @QuotationID";
                SqlCommand deleteCmd = new SqlCommand(deleteQuote, conn);
                deleteCmd.Parameters.AddWithValue("@QuotationID", quotationId);
                deleteCmd.ExecuteNonQuery();
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "booked", "Swal.fire('Success', 'Your service has been booked!', 'success');", true);
            txtDate.Text = "";
            txtTime.Text = "";
            txtNotes.Text = "";
        }
    }
}
