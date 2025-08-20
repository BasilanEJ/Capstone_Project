using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RRCManagementSystem
{
    public partial class Receipt : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            var qs = Request.QueryString["TransactionID"];
            if (!int.TryParse(qs, out int transactionId) || transactionId <= 0)
            {
                // Keep behavior similar to your original
                Response.Write("Invalid or missing Transaction ID");
                return;
            }

            LoadReceipt(transactionId);
        }

        private void LoadReceipt(int transactionId)
        {
            // Optional: if you want to restrict clients to only their own receipts,
            // uncomment this and compare later with the returned ClientID.
            // int sessionClientId = (Session["ClientID"] != null) ? Convert.ToInt32(Session["ClientID"]) : 0;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd  = new SqlCommand("dbo.usp_Receipt_GetByTransactionID", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@TransactionID", SqlDbType.Int).Value = transactionId;

                conn.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!r.Read())
                    {
                        Response.Write("Receipt not found.");
                        return;
                    }

                    // If you want to enforce “only my receipt” for logged-in clients:
                    // if (sessionClientId > 0 && r["ClientID"] != DBNull.Value &&
                    //     sessionClientId != Convert.ToInt32(r["ClientID"]))
                    // {
                    //     Response.Write("Receipt not found or access denied.");
                    //     return;
                    // }

                    // Safe conversions
                    string clientName = r["ClientName"] as string ?? "";
                    decimal amount    = (r["Amount"] == DBNull.Value) ? 0m : Convert.ToDecimal(r["Amount"]);
                    string method     = r["PaymentMethod"] as string ?? "";
                    DateTime when     = (r["TransactionDate"] == DBNull.Value)
                                            ? DateTime.MinValue
                                            : Convert.ToDateTime(r["TransactionDate"]);
                    string remarks    = r["Remarks"] as string ?? "";

                    // Bind to labels (same IDs as in your page)
                    lblClientName.Text      = clientName;
                    lblAmount.Text          = amount.ToString("N2", CultureInfo.InvariantCulture);
                    lblPaymentMethod.Text   = method;
                    lblTransactionDate.Text = when == DateTime.MinValue
                                                ? ""
                                                : when.ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);
                    lblRemarks.Text         = remarks;
                    lblTransactionID.Text   = transactionId.ToString(CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
