using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace RRCManagementSystem
{
    public partial class Reports : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"]?.ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Session validation
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // dropdown items are in .aspx
                btnExportPDF.Visible = false;
            }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            gvReports.DataSource = null;
            gvReports.DataBind();
            btnExportPDF.Visible = false;

            string selected = ddlModule.SelectedValue;
            if (string.IsNullOrEmpty(selected))
            {
                lblMessage.Text = "Please select a module.";
                return;
            }

            // Parse yyyy-MM-dd (HTML5 date inputs)
            DateTime fromDate, toDate;
            bool hasFrom = DateTime.TryParseExact(txtDateFrom.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate);
            bool hasTo = DateTime.TryParseExact(txtDateTo.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate);

            // Optional guard
            if (hasFrom && hasTo && fromDate.Date > toDate.Date)
            {
                lblMessage.Text = "Date From must be earlier than or equal to Date To.";

                return;
            }

            string procName;
            bool sendDates = true;

            switch (selected)
            {
                case "Admins":
                    procName = "dbo.spReport_Admins";
                    break;
                case "ArchivedAdmins":
                    procName = "dbo.spReport_ArchivedAdmins";
                    break;
                case "Roles":
                    procName = "dbo.spReport_Roles";
                    sendDates = false; // this proc has no date params
                    break;
                case "AuditLogs":
                    procName = "dbo.spReport_AuditLogs";
                    break;
                case "SystemChanges":
                    procName = "dbo.spReport_SystemChanges";
                    break;
                default:
                    lblMessage.Text = "Invalid module selected.";
                    return;
            }

            DataTable dt = new DataTable();

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(procName, conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (sendDates)
                {
                    var pFrom = cmd.Parameters.Add("@From", SqlDbType.Date);
                    pFrom.Value = hasFrom ? (object)fromDate.Date : DBNull.Value;

                    var pTo = cmd.Parameters.Add("@To", SqlDbType.Date);
                    pTo.Value = hasTo ? (object)toDate.Date : DBNull.Value;
                }

                try
                {
                    da.Fill(dt);

                    // ✅ Decrypt the Email column here
                    if (dt.Columns.Contains("Email"))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            string encryptedEmail = row["Email"].ToString();
                            if (!string.IsNullOrWhiteSpace(encryptedEmail))
                            {
                                try
                                {
                                    row["Email"] = AESHelper.DecryptEmail(encryptedEmail);
                                }
                                catch
                                {
                                    // If decryption fails, mark it clearly instead of crashing
                                    row["Email"] = "[Decryption Error]";
                                }
                            }
                        }
                    }

                    gvReports.DataSource = dt;
                    gvReports.DataBind();

                    if (dt.Rows.Count == 0)
                    {
                        lblMessage.Text = "No data found for the selected module and date range.";
                        btnExportPDF.Visible = false;
                    }
                    else
                    {
                        // Store data in ViewState for PDF export
                        ViewState["ReportData"] = dt;
                        ViewState["ReportModule"] = selected;
                        ViewState["ReportDateFrom"] = hasFrom ? fromDate.ToString("yyyy-MM-dd") : "";
                        ViewState["ReportDateTo"] = hasTo ? toDate.ToString("yyyy-MM-dd") : "";

                        // Show export button
                        btnExportPDF.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error generating report: " + ex.Message;
                    return;
                }
            }

            // ✅ Log the report generation (non-blocking of the main result)
            try
            {
                int userId = 0;
                if (Session["UserID"] != null)
                {
                    int.TryParse(Session["UserID"].ToString(), out userId);
                }

                LogReport(selected,
                          userId,
                          hasFrom ? fromDate.Date : (DateTime?)null,
                          hasTo ? toDate.Date : (DateTime?)null,
                          $"{dt.Rows.Count} row(s) returned");
            }
            catch
            {
                // swallow logging errors by design; don't break the report
            }
        }

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            DataTable dt = ViewState["ReportData"] as DataTable;
            string module = ViewState["ReportModule"] as string;
            string dateFrom = ViewState["ReportDateFrom"] as string;
            string dateTo = ViewState["ReportDateTo"] as string;

            if (dt == null || dt.Rows.Count == 0)
            {
                lblMessage.Text = "No data available to export.";
                return;
            }

            try
            {
                // Create PDF document
                Document document = new Document(PageSize.A4.Rotate(), 10f, 10f, 60f, 40f); // Increased margins for header/footer
                MemoryStream memoryStream = new MemoryStream();
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                // Get the username for the footer
                string generatedBy = Session["Name"]?.ToString() ?? "System";

                string logoPath = Server.MapPath("~/Images/logorrc.png"); // Change path as needed

                // Add header/footer with watermark
                PdfHeaderFooter headerFooter = new PdfHeaderFooter(module + " Report", generatedBy, logoPath);
                writer.PageEvent = headerFooter;

                document.Open();

                // Add Date Range Info (if applicable)
                Font dateFont = FontFactory.GetFont("Arial", 10, Font.NORMAL);
                string dateRange = "";
                if (!string.IsNullOrEmpty(dateFrom) && !string.IsNullOrEmpty(dateTo))
                {
                    dateRange = $"Date Range: {dateFrom} to {dateTo}";
                }
                else if (!string.IsNullOrEmpty(dateFrom))
                {
                    dateRange = $"From: {dateFrom}";
                }
                else if (!string.IsNullOrEmpty(dateTo))
                {
                    dateRange = $"To: {dateTo}";
                }

                if (!string.IsNullOrEmpty(dateRange))
                {
                    Paragraph dateInfo = new Paragraph(dateRange, dateFont);
                    dateInfo.Alignment = Element.ALIGN_CENTER;
                    dateInfo.SpacingAfter = 15f;
                    document.Add(dateInfo);
                }

                // Create PDF Table
                PdfPTable pdfTable = new PdfPTable(dt.Columns.Count);
                pdfTable.WidthPercentage = 100;
                pdfTable.SpacingBefore = 10f;
                pdfTable.DefaultCell.Padding = 5;

                // Add Headers
                Font headerFont = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
                foreach (DataColumn column in dt.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName, headerFont));
                    cell.BackgroundColor = new BaseColor(41, 128, 185); // Blue color
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    pdfTable.AddCell(cell);
                }

                // Add Data Rows
                Font cellFont = FontFactory.GetFont("Arial", 9, Font.NORMAL);
                foreach (DataRow row in dt.Rows)
                {
                    foreach (var item in row.ItemArray)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(item?.ToString() ?? "", cellFont));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.Padding = 5;
                        pdfTable.AddCell(cell);
                    }
                }

                document.Add(pdfTable);

                // Add Footer
                Paragraph footer = new Paragraph($"Total Records: {dt.Rows.Count}", dateFont);
                footer.Alignment = Element.ALIGN_RIGHT;
                footer.SpacingBefore = 10f;
                document.Add(footer);

                document.Close();
                writer.Close();

                // Send PDF to browser
                byte[] bytes = memoryStream.ToArray();
                memoryStream.Close();


                int userId = 0;
                if (Session["UserID"] != null)
                {
                    int.TryParse(Session["UserID"].ToString(), out userId);
                }

                DateTime? from = null;
                DateTime? to = null;

                if (!string.IsNullOrEmpty(dateFrom))
                {
                    DateTime tempFrom;
                    if (DateTime.TryParseExact(dateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempFrom))
                    {
                        from = tempFrom;
                    }
                }

                if (!string.IsNullOrEmpty(dateTo))
                {
                    DateTime tempTo;
                    if (DateTime.TryParseExact(dateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempTo))
                    {
                        to = tempTo;
                    }
                }

                try
                {
                    LogReport(module + " (PDF Export)", userId, from, to, $"{dt.Rows.Count} row(s) exported to PDF");
                }
                catch { }

                // Send PDF to browser
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", $"attachment; filename={module}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                Response.Buffer = true;
                Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.SuppressContent = true;
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error exporting to PDF: " + ex.Message;
            }
        }

        private void LogReport(string reportType, int generatedBy, DateTime? from, DateTime? to, string remarks)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.spReportsLog_Insert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ReportType", SqlDbType.NVarChar, 100).Value = (object)reportType ?? DBNull.Value;
                cmd.Parameters.Add("@GeneratedBy", SqlDbType.Int).Value = generatedBy; // 0 is OK if not logged-in context
                cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = from.HasValue ? (object)from.Value : DBNull.Value;
                cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = to.HasValue ? (object)to.Value : DBNull.Value;
                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, -1).Value = string.IsNullOrWhiteSpace(remarks) ? (object)DBNull.Value : remarks;

                var pOut = new SqlParameter("@NewReportID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pOut);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}