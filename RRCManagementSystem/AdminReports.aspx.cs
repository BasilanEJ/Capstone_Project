using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RRCManagementSystem.Helpers;

namespace RRCManagementSystem
{
    public partial class AdminReports : System.Web.UI.Page
    {
        protected Label lblTotalInquiries;
        protected Label lblTotalClients;
        protected Label lblTotalEquipment;
        protected Label lblTotalBookings;
        protected TextBox txtFromDate;
        protected TextBox txtToDate;
        protected GridView gvUserAccounts, gvInquiries, gvApprovedClients, gvInventorySnapshots, gvInventory, gvEquipment, gvBookings, gvInspections;

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFromDate.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadReports();
                LoadUserAccounts();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadReports();
            AddAuditLog(Convert.ToInt32(Session["UserID"]), "Filtered Admin Reports");
        }

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            Document doc = new Document(PageSize.A4.Rotate(), 10f, 10f, 20f, 10f);
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                writer.PageEvent = new PdfWatermark();
                string userPassword = Session["Password"]?.ToString() ?? "default123";
                writer.SetEncryption(
                    Encoding.UTF8.GetBytes(userPassword),
                    Encoding.UTF8.GetBytes(userPassword),
                    PdfWriter.ALLOW_PRINTING,
                    PdfWriter.ENCRYPTION_AES_128);

                doc.Open();

                AddGridToPDF(doc, gvUserAccounts, "👤 User Accounts");
                AddGridToPDF(doc, gvInquiries, "📬 Inquiries");
                AddGridToPDF(doc, gvApprovedClients, "✅ Approved Clients");
                AddGridToPDF(doc, gvInventorySnapshots, "📦 Total Stocks Snapshot (Daily)");
                AddGridToPDF(doc, gvInventory, "📦 Inventory Details");
                AddGridToPDF(doc, gvEquipment, "🛠️ Equipment Status");
                AddGridToPDF(doc, gvBookings, "📅 Booking Details");
                AddGridToPDF(doc, gvInspections, "🔍 Inspection Details");

                doc.Close();

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", $"attachment;filename=All_Reports_{DateTime.Now:yyyyMMdd}.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(ms.ToArray());
                Response.Flush();
                Response.SuppressContent = true;
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }

            AddAuditLog(Convert.ToInt32(Session["UserID"]), "Exported All Reports to PDF");
        }

        private void AddGridToPDF(Document doc, GridView grid, string title)
        {
            if (grid.Rows.Count == 0) return;

            doc.NewPage();
            doc.Add(new Paragraph(title, FontFactory.GetFont("Arial", 16, Font.BOLD)));
            doc.Add(new Paragraph("Generated at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            doc.Add(new Paragraph(" "));

            PdfPTable table = new PdfPTable(grid.Columns.Count)
            {
                WidthPercentage = 100,
                SpacingBefore = 10f
            };

            foreach (DataControlField column in grid.Columns)
            {
                PdfPCell headerCell = new PdfPCell(new Phrase(column.HeaderText, FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE)))
                {
                    BackgroundColor = BaseColor.DARK_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                };
                table.AddCell(headerCell);
            }

            foreach (GridViewRow row in grid.Rows)
            {
                foreach (TableCell cell in row.Cells)
                {
                    string text = HttpUtility.HtmlDecode(cell.Text).Trim();
                    if (string.IsNullOrWhiteSpace(text) || text == "&nbsp;") text = "";

                    int align = decimal.TryParse(text.Replace(",", ""), out _) ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;

                    PdfPCell bodyCell = new PdfPCell(new Phrase(text, FontFactory.GetFont("Arial", 11)))
                    {
                        HorizontalAlignment = align,
                        Padding = 5
                    };

                    table.AddCell(bodyCell);
                }
            }

            doc.Add(table);
        }

        private void LoadReports()
        {
            DateTime from = DateTime.Parse(txtFromDate.Text);
            DateTime to = DateTime.Parse(txtToDate.Text).AddDays(1);

            LoadSummaryCounts(from, to);
            LoadInquiries(from, to);
            LoadApprovedClients(from, to);
            LoadInventorySnapshots(from, to);
            LoadInventory();
            LoadEquipment();
            LoadBookings(from, to);
            LoadInspections(from, to);
        }

        private void LoadSummaryCounts(DateTime from, DateTime to)
        {
            lblTotalInquiries.Text = GetCount("InquirySimple", "SubmittedAt", from, to).ToString();
            lblTotalClients.Text = GetCount("Clients", "CreatedAt", from, to, "Status = 'Approved'").ToString();
            lblTotalEquipment.Text = GetCount("EquipmentStatus", null, DateTime.MinValue, DateTime.MaxValue, "Status = 'Available'").ToString();
            lblTotalBookings.Text = GetCount("Bookings", "BookingDate", from, to).ToString();
        }

        private void LoadUserAccounts()
        {
            string query = "SELECT UserID, Name, Email, Role, CreatedAt FROM Users WHERE Role <> 'SuperAdmin' ORDER BY CreatedAt DESC";
            BindGrid(query, gvUserAccounts);
        }

        private void LoadInquiries(DateTime from, DateTime to)
        {
            string query = @"SELECT InquiryID, Email, ContactNumber, Name,
                             StreetAndUnit + ', ' + Barangay + ', ' + City + ', ' + Region + ', ' + Country AS Address,
                             SubmittedAt FROM InquirySimple WHERE SubmittedAt BETWEEN @from AND @to ORDER BY SubmittedAt DESC";
            BindGrid(query, gvInquiries, from, to);
        }

        private void LoadApprovedClients(DateTime from, DateTime to)
        {
            string query = @"SELECT ClientID, Name, Email, CreatedAt,
                             CONCAT(StreetAndUnit, ', ', Barangay, ', ', City, ', ', Region, ', ', Country) AS Address
                             FROM Clients WHERE Status = 'Approved' AND CreatedAt BETWEEN @from AND @to";
            BindGrid(query, gvApprovedClients, from, to);
        }

        private void LoadInventorySnapshots(DateTime from, DateTime to)
        {
            string query = "SELECT Name, Type, Quantity, ExcessML, SnapshotDate FROM InventorySnapshots WHERE SnapshotDate BETWEEN @from AND @to";
            BindGrid(query, gvInventorySnapshots, from, to);
        }

        private void LoadInventory()
        {
            string query = "SELECT ItemID, Name, Quantity FROM Inventory";
            BindGrid(query, gvInventory);
        }

        private void LoadEquipment()
        {
            string query = "SELECT EquipmentID, Name, Status FROM EquipmentStatus WHERE Status = 'Available'";
            BindGrid(query, gvEquipment);
        }

        private void LoadBookings(DateTime from, DateTime to)
        {
            string query = @"SELECT b.BookingID, c.Name AS ClientName, s.Name AS Service, t.GroupName AS TeamName,
                             b.ScheduledDate, b.Status FROM Bookings b
                             LEFT JOIN Clients c ON b.ClientID = c.ClientID
                             LEFT JOIN Services s ON b.ServiceID = s.ServiceID
                             LEFT JOIN Teams t ON b.TeamID = t.TeamID
                             WHERE b.BookingDate BETWEEN @from AND @to";
            BindGrid(query, gvBookings, from, to);
        }

        private void LoadInspections(DateTime from, DateTime to)
        {
            string query = @"SELECT ins.InspectionID, usr.Name AS InspectorName, iq.Name AS ClientName,
                             (iq.StreetAndUnit + ', ' + iq.Barangay + ', ' + iq.City + ', ' + iq.Region + ', ' + iq.Country) AS ClientAddress,
                             ins.ScheduledDate, ins.InspectionStatus, ins.Remarks
                             FROM Inspections ins
                             LEFT JOIN InquirySimple iq ON ins.InquiryID = iq.InquiryID
                             LEFT JOIN Users usr ON ins.InspectorID = usr.UserID
                             WHERE ins.ScheduledDate BETWEEN @from AND @to ORDER BY ins.ScheduledDate DESC";
            BindGrid(query, gvInspections, from, to);
        }

        private void BindGrid(string query, GridView grid, DateTime? from = null, DateTime? to = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                if (query.Contains("@from"))
                {
                    cmd.Parameters.AddWithValue("@from", from);
                    cmd.Parameters.AddWithValue("@to", to);
                }
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
                grid.DataBind();
            }
        }

        private int GetCount(string table, string dateField, DateTime from, DateTime to, string where = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM {table} WHERE 1=1";
                if (!string.IsNullOrEmpty(dateField)) query += $" AND {dateField} BETWEEN @from AND @to";
                if (!string.IsNullOrEmpty(where)) query += $" AND {where}";

                SqlCommand cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrEmpty(dateField))
                {
                    cmd.Parameters.AddWithValue("@from", from);
                    cmd.Parameters.AddWithValue("@to", to);
                }
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        protected void btnExportUsers_Click(object sender, EventArgs e)
        {
            if (gvUserAccounts.Rows.Count > 0)
                ExportGridViewToPDF(gvUserAccounts, "Users_Report");
        }

        protected void btnExportInquiries_Click(object sender, EventArgs e)
        {
            if (gvInquiries.Rows.Count > 0)
                ExportGridViewToPDF(gvInquiries, "Inquiry_Report");
        }

        protected void btnExportClients_Click(object sender, EventArgs e)
        {
            if (gvApprovedClients.Rows.Count > 0)
                ExportGridViewToPDF(gvApprovedClients, "ApprovedClients_Report");
        }

        protected void btnExportInventory_Click(object sender, EventArgs e)
        {
            if (gvInventory.Rows.Count > 0)
                ExportGridViewToPDF(gvInventory, "Inventory_Report");
        }

        protected void btnExportEquipment_Click(object sender, EventArgs e)
        {
            if (gvEquipment.Rows.Count > 0)
                ExportGridViewToPDF(gvEquipment, "Equipment_Report");
        }

        protected void btnExportBookings_Click(object sender, EventArgs e)
        {
            if (gvBookings.Rows.Count > 0)
                ExportGridViewToPDF(gvBookings, "Bookings_Report");
        }

        protected void btnExportInspections_Click(object sender, EventArgs e)
        {
            if (gvInspections.Rows.Count > 0)
                ExportGridViewToPDF(gvInspections, "Inspections_Report");
        }


        private void AddAuditLog(int? userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AuditLogs (UserID, Action, Timestamp) VALUES (@UserID, @Action, GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", (object)userID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);
                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Optional logging
                    }
                }
            }
        }
        private void ExportGridViewToPDF(GridView grid, string title)
        {
            if (grid.Rows.Count == 0) return; // ⛔ Skip empty grids

            Document doc = new Document(PageSize.A4.Rotate(), 10f, 10f, 20f, 10f);
            MemoryStream ms = new MemoryStream();

            PdfWriter writer = PdfWriter.GetInstance(doc, ms);
            writer.PageEvent = new PdfWatermark();

            string userPassword = Session["Password"]?.ToString() ?? "default123";

            writer.SetEncryption(
                Encoding.UTF8.GetBytes(userPassword),
                Encoding.UTF8.GetBytes(userPassword),
                PdfWriter.ALLOW_PRINTING,
                PdfWriter.ENCRYPTION_AES_128
            );

            doc.Open();
            doc.Add(new Paragraph(title, FontFactory.GetFont("Arial", 16, Font.BOLD)));
            doc.Add(new Paragraph("Generated at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            doc.Add(new Paragraph(" "));

            int columnCount = grid.Columns.Count > 0 ? grid.Columns.Count : 1;
            PdfPTable table = new PdfPTable(columnCount)
            {
                WidthPercentage = 100 // Let iTextSharp auto-resize
            };

            // Add header
            foreach (TableCell cell in grid.HeaderRow.Cells)
            {
                PdfPCell headerCell = new PdfPCell(new Phrase(cell.Text, FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE)))
                {
                    BackgroundColor = BaseColor.DARK_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                };
                table.AddCell(headerCell);
            }

            // Add data
            foreach (GridViewRow row in grid.Rows)
            {
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    string rawText = HttpUtility.HtmlDecode(row.Cells[i].Text).Trim();
                    string text = string.IsNullOrWhiteSpace(rawText) || rawText == "&nbsp;" ? "" : rawText;
                    string header = grid.HeaderRow.Cells[i].Text.ToLower();
                    int alignment = Element.ALIGN_LEFT;

                    // Format known ID fields
                    if (int.TryParse(text, out int numericId))
                    {
                        if (header.Contains("user id"))
                            text = "User" + String.Format("{0:D4}", numericId);
                        else if (header.Contains("client id"))
                            text = "Client" + String.Format("{0:D4}", numericId);
                        else if (header.Contains("booking id"))
                            text = "Booking" + String.Format("{0:D4}", numericId);
                        else if (header.Contains("item id"))
                            text = "Itemid" + String.Format("{0:D4}", numericId);
                        else if (header.Contains("inspection id"))
                            text = "Inspect" + String.Format("{0:D4}", numericId);
                        else if (header.Contains("snapshot id"))
                            text = "Snap" + String.Format("{0:D4}", row.RowIndex + 1);
                        else if (header.Contains("id"))
                            text = "ID" + String.Format("{0:D4}", numericId);

                        alignment = Element.ALIGN_CENTER;
                    }

                    // Auto-align by type
                    if (header.Contains("quantity") || header.Contains("price") || header.Contains("amount") || header.Contains("sqm") || header.Contains("ml"))
                        alignment = Element.ALIGN_RIGHT;
                    else if (header.Contains("status") || header.Contains("date"))
                        alignment = Element.ALIGN_CENTER;
                    else if (header.Contains("name") || header.Contains("remarks") || header.Contains("address"))
                        alignment = Element.ALIGN_LEFT;

                    PdfPCell pdfCell = new PdfPCell(new Phrase(text, FontFactory.GetFont("Arial", 11)))
                    {
                        HorizontalAlignment = alignment,
                        Padding = 5
                    };

                    table.AddCell(pdfCell);
                }
            }

            doc.Add(table);
            doc.Close();

            // Return file
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.BinaryWrite(ms.ToArray());
            Response.End();
        }



    }
}