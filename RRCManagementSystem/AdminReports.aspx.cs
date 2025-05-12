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

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            Document doc = new Document(PageSize.A4.Rotate(), 10f, 10f, 20f, 10f);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = null;

            try
            {
                writer = PdfWriter.GetInstance(doc, ms);

                // ✅ Set watermark
                writer.PageEvent = new PdfWatermark();

                // ✅ Use password from session
                string userPassword = Session["Password"] != null ? Session["Password"].ToString() : "default123";

                writer.SetEncryption(
                    Encoding.UTF8.GetBytes(userPassword),
                    Encoding.UTF8.GetBytes(userPassword),
                    PdfWriter.ALLOW_PRINTING,
                    PdfWriter.ENCRYPTION_AES_128
                );

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
                Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                Response.BinaryWrite(ms.ToArray());
                Response.Flush();
                Response.SuppressContent = true;
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                // ✅ Display user-friendly error message
                lblMessage.Text = $"❌ Error exporting PDF: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                // 🔥 Always clear password no matter what
                Session.Remove("Password");

                if (doc.IsOpen())
                    doc.Close();

                ms.Dispose();
            }

            AddAuditLog(Convert.ToInt32(Session["UserID"]), "Exported All Reports to PDF");
        }




        private void LoadUserAccounts()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT UserID, FullName, Email, Role, CreatedAt
                FROM Users
                WHERE Role <> 'SuperAdmin'
                ORDER BY CreatedAt DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvUserAccounts.DataSource = dt;
                    gvUserAccounts.DataBind();
                }
            }
            catch (Exception)
            {
                // Optional error handling
            }
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

        private void LoadInquiries(DateTime from, DateTime to)
        {
            BindGrid(@"
        SELECT 
            InquiryID, 
            Email, 
            ContactNumber, 
            Name,
            StreetAndUnit + ', ' + Barangay + ', ' + City + ', ' + Region + ', ' + Country AS Address,
            SubmittedAt
        FROM InquirySimple
        WHERE SubmittedAt BETWEEN @from AND @to
        ORDER BY SubmittedAt DESC",
                gvInquiries, from, to);
        }



        private void LoadApprovedClients(DateTime from, DateTime to)
        {
            string query = @"
        SELECT 
            ClientID, 
            Name, 
            Email, 
            CreatedAt,
            CONCAT(StreetAndUnit, ', ', Barangay, ', ', City, ', ', Region, ', ', Country) AS Address
        FROM Clients 
        WHERE Status = 'Approved' AND CreatedAt BETWEEN @from AND @to";

            BindGrid(query, gvApprovedClients, from, to);
        }




        private void LoadInventorySnapshots(DateTime from, DateTime to) =>
            BindGrid(@"SELECT Name, Type, Quantity, ExcessML, SnapshotDate 
                       FROM InventorySnapshots 
                       WHERE SnapshotDate BETWEEN @from AND @to", gvInventorySnapshots, from, to);

        private void LoadInventory() =>
            BindGrid("SELECT ItemID, Name, Quantity FROM Inventory", gvInventory);

        private void LoadEquipment() =>
            BindGrid("SELECT EquipmentID, Name, Status FROM EquipmentStatus WHERE Status = 'Available'", gvEquipment);

        private void LoadBookings(DateTime from, DateTime to) =>
            BindGrid(@"SELECT b.BookingID, c.Name AS ClientName, s.Name AS Service, 
                              t.GroupName AS TeamName, b.ScheduledDate, b.Status 
                       FROM Bookings b 
                       LEFT JOIN Clients c ON b.ClientID = c.ClientID 
                       LEFT JOIN Services s ON b.ServiceID = s.ServiceID 
                       LEFT JOIN Teams t ON b.TeamID = t.TeamID 
                       WHERE b.BookingDate BETWEEN @from AND @to", gvBookings, from, to);

        private void LoadInspections(DateTime from, DateTime to)
        {
            BindGrid(@"
        SELECT 
            ins.InspectionID,
            usr.Name AS InspectorName,
            iq.Name AS ClientName,
            (iq.StreetAndUnit + ', ' + iq.Barangay + ', ' + iq.City + ', ' + iq.Region + ', ' + iq.Country) AS ClientAddress,
            ins.ScheduledDate,
            ins.InspectionStatus,
            ins.Remarks
        FROM Inspections ins
        LEFT JOIN InquirySimple iq ON ins.InquiryID = iq.InquiryID
        LEFT JOIN Users usr ON ins.InspectorID = usr.UserID
        WHERE ins.ScheduledDate BETWEEN @from AND @to
        ORDER BY ins.ScheduledDate DESC",
            gvInspections, from, to);
        }



        private int GetCount(string table, string dateField, DateTime from, DateTime to, string where = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM {table} WHERE 1=1";
                if (!string.IsNullOrEmpty(dateField)) query += $" AND {dateField} BETWEEN @from AND @to";
                if (!string.IsNullOrEmpty(where)) query += $" AND {where}";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(dateField))
                    {
                        cmd.Parameters.AddWithValue("@from", from);
                        cmd.Parameters.AddWithValue("@to", to);
                    }
                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
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

        private void AddGridToPDF(Document doc, GridView grid, string title)
        {
            if (grid.Rows.Count == 0) return;

            doc.NewPage();

            // Add title and timestamp
            doc.Add(new Paragraph(title, FontFactory.GetFont("Arial", 16, Font.BOLD)));
            doc.Add(new Paragraph("Generated at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            doc.Add(new Paragraph(" "));

            int columnCount = grid.Columns.Count;
            PdfPTable table = new PdfPTable(columnCount)
            {
                WidthPercentage = 100, // Fill the page width
                SpacingBefore = 10f
                // ✨ Don't set SetWidths() → this lets iTextSharp autosize based on content
            };

            // 🔹 Add header row (center-aligned)
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

            // 🔹 Add data rows
            foreach (GridViewRow row in grid.Rows)
            {
                foreach (TableCell cell in row.Cells)
                {
                    string text = HttpUtility.HtmlDecode(cell.Text).Trim();
                    if (string.IsNullOrWhiteSpace(text) || text == "&nbsp;") text = "";

                    // Align numbers to the right
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

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadReports(); // 📥 Reload the data based on the new date range
            AddAuditLog(Convert.ToInt32(Session["UserID"]), "Filtered Admin Reports");
        }

        private void ExportGridViewToPDF(GridView grid, string title)
        {
            Document doc = new Document(PageSize.A4.Rotate(), 10f, 10f, 20f, 10f);
            MemoryStream ms = new MemoryStream();

            PdfWriter writer = PdfWriter.GetInstance(doc, ms);

            // 🔵 Watermark setup
            writer.PageEvent = new PdfWatermark();

            // 🔵 Get current user's password from Session
            string userPassword = Session["Password"] != null ? Session["Password"].ToString() : "default123";

            // 🔵 Set password using user's own password
            writer.SetEncryption(
                Encoding.UTF8.GetBytes(userPassword), // Open password (user's login password)
                Encoding.UTF8.GetBytes(userPassword), // Owner password
                PdfWriter.ALLOW_PRINTING, // 🔥 Allow Printing Only (Block Copy)
                PdfWriter.ENCRYPTION_AES_128
            );

            doc.Open();
            doc.Add(new Paragraph(title, FontFactory.GetFont("Arial", 16, Font.BOLD)));
            doc.Add(new Paragraph("Generated at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            doc.Add(new Paragraph(" "));

            int columnCount = grid.Columns.Count > 0 ? grid.Columns.Count : 1;
            PdfPTable table = new PdfPTable(columnCount);
            table.WidthPercentage = 100;

            if (grid.HeaderRow != null)
            {
                foreach (TableCell cell in grid.HeaderRow.Cells)
                {
                    PdfPCell pdfCell = new PdfPCell(new Phrase(cell.Text, FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE)))
                    {
                        BackgroundColor = BaseColor.GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(pdfCell);
                }
            }

            foreach (GridViewRow row in grid.Rows)
            {
                foreach (TableCell cell in row.Cells)
                {
                    PdfPCell pdfCell = new PdfPCell(new Phrase(cell.Text, FontFactory.GetFont("Arial", 11, Font.NORMAL, BaseColor.BLACK)))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(pdfCell);
                }
            }

            doc.Add(table);
            doc.Close();

            byte[] pdfBytes = ms.ToArray();

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf");
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.BinaryWrite(pdfBytes);
            Response.End();
        }

        private string GetUserHashedPassword(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT PasswordHash FROM Users WHERE UserID = @UserID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                conn.Open();
                return cmd.ExecuteScalar()?.ToString() ?? "default123";
            }
        }



        protected void btnExportInquiries_Click(object sender, EventArgs e) => ExportGridViewToPDF(gvInquiries, "Inquiry_Report");
        protected void btnExportClients_Click(object sender, EventArgs e) => ExportGridViewToPDF(gvApprovedClients, "ApprovedClients_Report");
        protected void btnExportInventory_Click(object sender, EventArgs e) => ExportGridViewToPDF(gvInventory, "Inventory_Report");
        protected void btnExportEquipment_Click(object sender, EventArgs e) => ExportGridViewToPDF(gvEquipment, "Equipment_Report");
        protected void btnExportBookings_Click(object sender, EventArgs e) => ExportGridViewToPDF(gvBookings, "Bookings_Report");
        protected void btnExportInspections_Click(object sender, EventArgs e) => ExportGridViewToPDF(gvInspections, "Inspections_Report");
        protected void btnExportUsers_Click(object sender, EventArgs e) => ExportGridViewToPDF(gvUserAccounts, "Users_Report");


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
                        // Optional: log error
                    }
                }
            }
        }
    }
}