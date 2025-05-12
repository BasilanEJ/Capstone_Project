using System;
using System.Configuration;
using System.Data.SqlClient;

namespace RRCManagementSystem
{
    public partial class Receipt : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["TransactionID"] != null)
                {
                    int transactionId;
                    if (int.TryParse(Request.QueryString["TransactionID"], out transactionId))
                    {
                        LoadReceipt(transactionId);
                    }
                    else
                    {
                        Response.Write("Invalid Transaction ID");
                    }
                }
                else
                {
                    Response.Write("No Transaction ID Provided");
                }
            }
        }

        private void LoadReceipt(int transactionId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT t.Amount, t.PaymentMethod, t.TransactionDate, t.Remarks, c.Name AS ClientName
                    FROM Transactions t
                    INNER JOIN Bookings b ON t.SaleID = b.BookingID
                    INNER JOIN Clients c ON b.ClientID = c.ClientID
                    WHERE t.TransactionID = @TransactionID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TransactionID", transactionId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    lblClientName.Text = reader["ClientName"].ToString();
                    lblAmount.Text = Convert.ToDecimal(reader["Amount"]).ToString("N2");
                    lblPaymentMethod.Text = reader["PaymentMethod"].ToString();
                    lblTransactionDate.Text = Convert.ToDateTime(reader["TransactionDate"]).ToString("yyyy-MM-dd hh:mm tt");
                    lblRemarks.Text = reader["Remarks"].ToString();
                    lblTransactionID.Text = transactionId.ToString();
                }
            }
        }
    }
}