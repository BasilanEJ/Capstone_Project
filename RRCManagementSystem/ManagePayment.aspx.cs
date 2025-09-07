using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using RRCManagementSystem.Helpers; // AESHelper, BlockchainLogger

namespace RRCManagementSystem
{
    public partial class ManagePayment : Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private static string lastGeneratedEncryptedPDF = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            var role = Session["Role"].ToString();
            if (role == "SuperAdmin" || role == "Inspector")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // 🔐 Check CanEdit permission for Sales & Transaction
            if (!HasEditPermission(userId, "Sales&Transaction"))
            {
                lblMessage.Text = "❌ You do not have permission to manage payment.";
                lblMessage.CssClass = "message error";
                pnlChosen.Visible = false; // hide the payment panel
                pnlResults.Visible = false;
                btnSaveReal.Enabled = false; // disable save button
                btnPrintReceipt.Visible = false;
                btnDownloadReceipt.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                BindPaymentDDL(ddlMethod1);
                BindPaymentDDL(ddlMethod2);
                txtRemainingBalance.Text = "0.00";
            }
        }

        private bool HasEditPermission(int userId, string moduleName)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spAdminPermission_Check", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@ModuleName", SqlDbType.NVarChar, 100).Value = moduleName;
                    cmd.Parameters.Add("@Permission", SqlDbType.NVarChar, 10).Value = "CanEdit";

                    con.Open();
                    object allowed = cmd.ExecuteScalar();
                    return allowed != null && allowed != DBNull.Value && Convert.ToBoolean(allowed);
                }
            }
            catch
            {
                return false;
            }
        }


        // -------- Search / pick client ----------
        protected void btnSearchClient_Click(object sender, EventArgs e)
        {
            pnlChosen.Visible = false;
            hfClientID.Value = "";
            rpResults.DataSource = SearchClients(txtClientSearch.Text.Trim());
            rpResults.DataBind();
            pnlResults.Visible = true;
            lblMessage.Text = "";
        }

        protected void btnClearClient_Click(object sender, EventArgs e)
        {
            txtClientSearch.Text = "";
            pnlResults.Visible = false;
            pnlChosen.Visible = false;
            hfClientID.Value = "";
            txtRemainingBalance.Text = "0.00";
           // txtProjectedBalance.Text = "0.00";
           // txtLatestBooking.Text = "-";
            hfBookingId.Value = "";
        }

        protected void rpResults_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Pick") return;

            int clientId = int.Parse(e.CommandArgument.ToString());
            hfClientID.Value = clientId.ToString(CultureInfo.InvariantCulture);

            // show chosen label
            var (display, bookingId, remaining) = LoadClientSummary(clientId);
            lblChosen.Text = display;
            pnlChosen.Visible = true;
            pnlResults.Visible = false;

            hfBookingId.Value = bookingId > 0 ? bookingId.ToString() : "";
           // txtLatestBooking.Text = bookingId > 0 ? $"Booking #{bookingId}" : "—";
            txtRemainingBalance.Text = remaining.ToString("N2", new CultureInfo("en-PH"));
           // txtProjectedBalance.Text = remaining.ToString("N2", new CultureInfo("en-PH"));
        }

        private DataTable SearchClients(string query)
        {
            var dt = new DataTable();
            string sql = @"
                SELECT TOP 30 c.ClientID,
                       CASE WHEN ISNULL(LTRIM(RTRIM(c.MiddleName)),'')=''
                            THEN CONCAT(c.LastName, ', ', c.FirstName)
                            ELSE CONCAT(c.LastName, ', ', c.FirstName, ' ', c.MiddleName) END AS DisplayName
                FROM dbo.Clients c
                WHERE (@qInt IS NOT NULL AND c.ClientID=@qInt)
                   OR (c.FirstName + ' ' + ISNULL(c.MiddleName,'') + ' ' + c.LastName LIKE '%' + @q + '%')
                   OR (c.LastName + ', ' + c.FirstName + ' ' + ISNULL(c.MiddleName,'') LIKE '%' + @q + '%')
                ORDER BY c.LastName, c.FirstName;";

            int qInt;
            int? asInt = int.TryParse(query, out qInt) ? qInt : (int?)null;

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@q", SqlDbType.NVarChar, 100).Value = (object)query ?? DBNull.Value;
                cmd.Parameters.Add("@qInt", SqlDbType.Int).Value = (object)asInt ?? DBNull.Value;
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }
            return dt;
        }

        private (string display, int bookingId, decimal remaining) LoadClientSummary(int clientId)
        {
            // chosen display name
            string display = "";
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"SELECT TOP 1 
                    CASE WHEN ISNULL(LTRIM(RTRIM(MiddleName)),'')=''
                         THEN CONCAT(LastName, ', ', FirstName)
                         ELSE CONCAT(LastName, ', ', FirstName, ' ', MiddleName) END
                FROM dbo.Clients WHERE ClientID=@id", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = clientId;
                con.Open();
                display = (cmd.ExecuteScalar() ?? "").ToString();
            }

            // latest payable booking
            var (bookingId, _) = GetLatestPayableBooking(clientId);

            // ensure sale & remaining
            decimal remaining = 0m;
            if (bookingId > 0)
            {
                int saleId = EnsureSaleForBooking(bookingId);
                remaining = GetRemainingBySale(saleId);
                if (remaining < 0) remaining = 0;
            }
            return (display, bookingId, remaining);
        }

        // -------- Enable/disable Payment 2 UI ----------
        protected void chkUseSecond_CheckedChanged(object sender, EventArgs e)
        {
            bool on = chkUseSecond.Checked;
            ddlMethod2.Enabled = on;
            txtAmount2.Enabled = on;
            fuReceipt2.Enabled = on;
            txtRemarks2.Enabled = on;
        }

        // -------- Save ----------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";

            if (string.IsNullOrWhiteSpace(hfClientID.Value))
            {
                ShowError("Please search and select a client.");
                return;
            }
            int clientId = int.Parse(hfClientID.Value, CultureInfo.InvariantCulture);

            // payment 1 validations
            if (string.IsNullOrEmpty(ddlMethod1.SelectedValue))
            {
                ShowError("Select Payment Method 1.");
                return;
            }
            if (!decimal.TryParse(txtAmount1.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount1) || amount1 <= 0)
            {
                ShowError("Enter a valid amount for Payment 1.");
                return;
            }
            if (!fuReceipt1.HasFile)
            {
                ShowError("Upload Receipt for Payment 1.");
                return;
            }
            if (!IsJpgOrPng(fuReceipt1.FileName))
            {
                ShowError("Receipt 1 must be JPG or PNG.");
                return;
            }

            // payment 2 validations (optional)
            bool use2 = chkUseSecond.Checked;
            decimal amount2 = 0m;
            if (use2)
            {
                if (string.IsNullOrEmpty(ddlMethod2.SelectedValue))
                {
                    ShowError("Select Payment Method 2.");
                    return;
                }
                if (!decimal.TryParse(txtAmount2.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out amount2) || amount2 <= 0)
                {
                    ShowError("Enter a valid amount for Payment 2.");
                    return;
                }
                if (!fuReceipt2.HasFile)
                {
                    ShowError("Upload Receipt for Payment 2.");
                    return;
                }
                if (!IsJpgOrPng(fuReceipt2.FileName))
                {
                    ShowError("Receipt 2 must be JPG or PNG.");
                    return;
                }
            }

            // remaining & booking
            var (bookingId, _) = GetLatestPayableBooking(clientId);
            if (bookingId <= 0)
            {
                ShowError("No payable booking found for this client.");
                return;
            }
            hfBookingId.Value = bookingId.ToString();
            int saleId = EnsureSaleForBooking(bookingId);
            decimal remaining = GetRemainingBySale(saleId);

            decimal total = amount1 + amount2;
            if (total > remaining)
            {
                ShowError($"Total payment (₱{total:N2}) exceeds remaining (₱{remaining:N2}).");
                return;
            }

            try
            {
                string performedBy = Session["AdminName"]?.ToString() ?? "Admin";

                // --- Payment 1 ---
                string receipt1 = SaveEncryptedReceiptFile(bookingId, fuReceipt1, "p1");
                int tx1 = InsertTransaction(
                    saleId, amount1, ddlMethod1.SelectedValue, "Completed",
                    (txtRemarks1.Text ?? "").Trim(), MakeManualReference(bookingId), receipt1);

                BlockchainLogger.AppendSaleLog(cs, tx1, new
                {
                    TransactionID = tx1,
                    ClientID = clientId,
                    BookingID = bookingId,
                    SaleID = saleId,
                    Amount = amount1,
                    Currency = "PHP",
                    Method = ddlMethod1.SelectedValue,
                    Status = "Completed",
                    PaidAtUtc = DateTime.UtcNow
                });

                // generate a printable for the last one (tx2 will overwrite path if present)
                lastGeneratedEncryptedPDF = GenerateReceiptPDF(tx1, clientId, bookingId, amount1, ddlMethod1.SelectedValue, txtRemarks1.Text ?? "", performedBy);

                // --- Payment 2 (optional) ---
                if (use2)
                {
                    string receipt2 = SaveEncryptedReceiptFile(bookingId, fuReceipt2, "p2");
                    int tx2 = InsertTransaction(
                        saleId, amount2, ddlMethod2.SelectedValue, "Completed",
                        (txtRemarks2.Text ?? "").Trim(), MakeManualReference(bookingId), receipt2);

                    BlockchainLogger.AppendSaleLog(cs, tx2, new
                    {
                        TransactionID = tx2,
                        ClientID = clientId,
                        BookingID = bookingId,
                        SaleID = saleId,
                        Amount = amount2,
                        Currency = "PHP",
                        Method = ddlMethod2.SelectedValue,
                        Status = "Completed",
                        PaidAtUtc = DateTime.UtcNow
                    });

                    lastGeneratedEncryptedPDF = GenerateReceiptPDF(tx2, clientId, bookingId, amount2, ddlMethod2.SelectedValue, txtRemarks2.Text ?? "", performedBy);
                }

                // recompute balances
                decimal newRemaining = GetRemainingBySale(saleId);
                txtRemainingBalance.Text = newRemaining.ToString("N2", new CultureInfo("en-PH"));
               // txtProjectedBalance.Text = newRemaining.ToString("N2", new CultureInfo("en-PH"));
                //txtLatestBooking.Text = $"Booking #{bookingId}";

                lblMessage.Text = "✅ Payment(s) recorded and balance updated.";
                lblMessage.CssClass = "message success";
                btnPrintReceipt.Visible = true;
                btnDownloadReceipt.Visible = true;

                // clear amounts (keep client locked)
                txtAmount1.Text = "";
                txtRemarks1.Text = "";
                txtAmount2.Text = "";
                txtRemarks2.Text = "";
            }
            catch (Exception ex)
            {
                ShowError("Error saving payment: " + ex.Message);
            }
        }

        // -------- Helpers (DB & files) ----------
        private void BindPaymentDDL(DropDownList ddl)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("-- Select Method --", ""));
            ddl.Items.Add(new ListItem("Cash", "Cash"));
            ddl.Items.Add(new ListItem("Bank Transfer", "Bank Transfer"));
            ddl.Items.Add(new ListItem("Online Payment", "Online Payment"));

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand(
                    "SELECT DISTINCT PaymentMethod FROM dbo.Transactions WHERE PaymentMethod IS NOT NULL AND PaymentMethod<>'' ORDER BY PaymentMethod", con))
                {
                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var pm = r["PaymentMethod"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(pm) && ddl.Items.FindByValue(pm) == null)
                                ddl.Items.Add(new ListItem(pm, pm));
                        }
                    }
                }
            }
            catch { /* non-blocking */ }
        }

        private (int bookingId, decimal price) GetLatestPayableBooking(int clientId)
        {
            const string sql = @"
                SELECT TOP(1) b.BookingID, ISNULL(b.Price,0) AS Price
                FROM dbo.Bookings b
                WHERE b.ClientID=@ClientID AND b.Status IN ('Assigned','Confirmed','Approved')
                ORDER BY b.BookingID DESC;";
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                con.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!r.Read()) return (0, 0m);
                    return (Convert.ToInt32(r["BookingID"], CultureInfo.InvariantCulture),
                            Convert.ToDecimal(r["Price"], CultureInfo.InvariantCulture));
                }
            }
        }

        private int EnsureSaleForBooking(int bookingId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Sales_GetOrCreateByBooking", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                var pOut = cmd.Parameters.Add("@SaleID", SqlDbType.Int);
                pOut.Direction = ParameterDirection.Output;
                con.Open();
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(pOut.Value, CultureInfo.InvariantCulture);
            }
        }

        private int InsertTransaction(int saleId, decimal amount, string method, string status, string remarks, string reference, string receiptPath)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Transactions_Insert", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;

                var pAmt = cmd.Parameters.Add("@Amount", SqlDbType.Decimal);
                pAmt.Precision = 18; pAmt.Scale = 2; pAmt.Value = amount;

                cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = method ?? "Cash";
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status ?? "Completed";
                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 255).Value = (remarks ?? "").Trim();
                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 200).Value = (object)reference ?? DBNull.Value;
                cmd.Parameters.Add("@Receipt", SqlDbType.NVarChar, 255).Value =
                    string.IsNullOrWhiteSpace(receiptPath) ? (object)DBNull.Value : receiptPath;

                var pTx = cmd.Parameters.Add("@TransactionID", SqlDbType.Int);
                pTx.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(pTx.Value, CultureInfo.InvariantCulture);
            }
        }

        private string SaveEncryptedReceiptFile(int bookingId, FileUpload fu, string suffix)
        {
            string folder = Server.MapPath("~/Receipts/");
            Directory.CreateDirectory(folder);
            string ext = Path.GetExtension(fu.FileName) ?? ".jpg";
            string fileName = $"receipt_{bookingId}_{suffix}_{DateTime.UtcNow.Ticks}{ext}";
            string fullPath = Path.Combine(folder, fileName);

            using (var ms = new MemoryStream())
            {
                fu.PostedFile.InputStream.CopyTo(ms);
                byte[] original = ms.ToArray();
                byte[] encrypted = AESHelper.Encrypt(original);
                File.WriteAllBytes(fullPath, encrypted);
            }
            return "~/Receipts/" + fileName; // stored encrypted on disk; path saved in DB
        }

        private bool IsJpgOrPng(string name)
        {
            string ext = (Path.GetExtension(name) ?? "").ToLowerInvariant();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png";
        }

        private decimal GetRemainingBySale(int saleId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Sales_GetRemaining", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@SaleID", SqlDbType.Int).Value = saleId;
                con.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? 0m : Convert.ToDecimal(o, CultureInfo.InvariantCulture);
            }
        }

        private string GenerateReceiptPDF(int transactionId, int clientId, int bookingId, decimal amount, string method, string remarks, string performedBy)
        {
            string folderPath = Server.MapPath("~/ReceiptsPDF/");
            Directory.CreateDirectory(folderPath);

            string plainPDF = Path.Combine(folderPath, $"Receipt_{transactionId}.pdf");
            using (var fs = new FileStream(plainPDF, FileMode.Create, FileAccess.Write))
            {
                var doc = new iTextSharp.text.Document();
                iTextSharp.text.pdf.PdfWriter.GetInstance(doc, fs);
                doc.Open();
                doc.Add(new iTextSharp.text.Paragraph($"Receipt #{transactionId}"));
                doc.Add(new iTextSharp.text.Paragraph($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"));
                doc.Add(new iTextSharp.text.Paragraph($"Client ID: {clientId}"));
                doc.Add(new iTextSharp.text.Paragraph($"Booking ID: {bookingId}"));
                doc.Add(new iTextSharp.text.Paragraph($"Amount Paid: ₱ {amount:N2}"));
                doc.Add(new iTextSharp.text.Paragraph($"Method: {method}"));
                if (!string.IsNullOrWhiteSpace(remarks))
                    doc.Add(new iTextSharp.text.Paragraph($"Remarks: {remarks}"));
                doc.Add(new iTextSharp.text.Paragraph($"Handled By: {performedBy}"));
                doc.Close();
            }

            byte[] pdfBytes = File.ReadAllBytes(plainPDF);
            byte[] encrypted = AESHelper.Encrypt(pdfBytes);
            string encryptedPath = Path.Combine(folderPath, $"Receipt_{transactionId}_encrypted.pdf");
            File.WriteAllBytes(encryptedPath, encrypted);
            return encryptedPath;
        }

        protected void btnPrintReceipt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastGeneratedEncryptedPDF)) return;
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "inline; filename=Receipt.pdf");
            byte[] enc = File.ReadAllBytes(lastGeneratedEncryptedPDF);
            byte[] dec = AESHelper.Decrypt(enc);
            Response.BinaryWrite(dec);
            Response.End();
        }

        protected void btnDownloadReceipt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastGeneratedEncryptedPDF)) return;
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment; filename=Receipt.pdf");
            byte[] enc = File.ReadAllBytes(lastGeneratedEncryptedPDF);
            byte[] dec = AESHelper.Decrypt(enc);
            Response.BinaryWrite(dec);
            Response.End();
        }

        private static string MakeManualReference(int bookingId) =>
            $"MANUAL:{bookingId}:{Guid.NewGuid():N}";

        private void ShowError(string message)
        {
            lblMessage.Text = "❌ " + message;
            lblMessage.CssClass = "message error";
        }
    }
}
