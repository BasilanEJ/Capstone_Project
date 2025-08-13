using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
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
        protected TextBox txtTeamDate;
        protected GridView gvTeamsSummary, gvTeamMembers;


        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFromDate.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtTeamDate.Text = DateTime.Today.ToString("yyyy-MM-dd"); // 👈 default availability date
                LoadReports();
                LoadUserAccounts();
                LoadTeamReports(); // 👈 initial load

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

        protected void btnExportTeamsSummary_Click(object sender, EventArgs e)
        {
            if (gvTeamsSummary.Rows.Count > 0)
                ExportGridViewToPDF(gvTeamsSummary, "Team_Summary_Report");
        }

        protected void btnExportTeamMembers_Click(object sender, EventArgs e)
        {
            if (gvTeamMembers.Rows.Count > 0)
                ExportGridViewToPDF(gvTeamMembers, "Team_Members_Report");
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadReports();
            LoadTeamReports(); // 👈 keep Team section in sync with range filter
            AddAuditLog(Convert.ToInt32(Session["UserID"]), "Filtered Admin Reports");
        }

        private void LoadTeamReports()
        {
            DateTime from = DateTime.Parse(txtFromDate.Text);
            DateTime to = DateTime.Parse(txtToDate.Text).AddDays(1);
            DateTime teamDate = DateTime.TryParse(txtTeamDate.Text, out var d) ? d : DateTime.Today;

            LoadTeamsSummary(from, to, teamDate);
            LoadTeamMembers();
        }

        private void LoadTeamsSummary(DateTime from, DateTime to, DateTime teamDate)
        {
            string query = @"
;WITH Members AS (
    SELECT tm.TeamID, COUNT(*) AS MembersCount
    FROM TeamMembers tm
    INNER JOIN Employees e ON tm.EmployeeID = e.EmployeeID
    GROUP BY tm.TeamID
),
JobsOnDate AS (
    SELECT b.TeamID, COUNT(*) AS AssignmentsOnDate
    FROM Bookings b
    WHERE b.TeamID IS NOT NULL
      AND CAST(b.ScheduledDate AS DATE) = @teamDate
      AND b.Status NOT IN ('Cancelled')
    GROUP BY b.TeamID
),
JobsInRange AS (
    SELECT b.TeamID, COUNT(*) AS AssignmentsInRange, MAX(b.ScheduledDate) AS LastScheduled
    FROM Bookings b
    WHERE b.TeamID IS NOT NULL
      AND b.ScheduledDate BETWEEN @from AND @to
      AND b.Status NOT IN ('Cancelled')
    GROUP BY b.TeamID
)
SELECT 
    t.TeamID,
    t.GroupName,
    ISNULL(m.MembersCount, 0) AS MembersCount,
    ISNULL(jd.AssignmentsOnDate, 0) AS AssignmentsOnDate,
    ISNULL(jr.AssignmentsInRange, 0) AS AssignmentsInRange,
    jr.LastScheduled,
    CASE WHEN ISNULL(jd.AssignmentsOnDate, 0) >= 2 THEN 'Unavailable' ELSE 'Available' END AS Status
FROM Teams t
LEFT JOIN Members m   ON m.TeamID = t.TeamID
LEFT JOIN JobsOnDate jd ON jd.TeamID = t.TeamID
LEFT JOIN JobsInRange jr ON jr.TeamID = t.TeamID
ORDER BY t.GroupName;";

            BindGrid(query, gvTeamsSummary, from, to, teamDate);
        }


        private void LoadTeamMembers()
        {
            string query = @"
SELECT 
    t.TeamID,
    t.GroupName,
    e.EmployeeID,
    e.LastName, e.FirstName, e.MiddleName,
    e.Department
FROM Teams t
LEFT JOIN TeamMembers tm ON tm.TeamID = t.TeamID
LEFT JOIN Employees e ON e.EmployeeID = tm.EmployeeID
ORDER BY t.GroupName, e.LastName, e.FirstName;";

            BindGrid(query, gvTeamMembers);
        }

        protected void btnTeamDateApply_Click(object sender, EventArgs e)
        {
            LoadTeamReports();
            AddAuditLog(Convert.ToInt32(Session["UserID"]), "Applied Team Availability Date in Reports");
        }




        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            // make sure everything is bound for this export postback
            LoadReports();
            LoadTeamReports();   // <— include team data

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

                // ✅ Add these two sections
                AddGridToPDF(doc, gvTeamsSummary, "👥 Team Summary");
                AddGridToPDF(doc, gvTeamMembers, "👨‍👩‍👧‍👦 Team Members");

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

            int visibleCols = grid.HeaderRow?.Cells.Count ?? grid.Columns.Count;
            PdfPTable table = new PdfPTable(visibleCols)
            {
                WidthPercentage = 100,
                SpacingBefore = 10f
            };

            // headers
            if (grid.HeaderRow != null)
            {
                foreach (TableCell hc in grid.HeaderRow.Cells)
                {
                    string headerText = HttpUtility.HtmlDecode(GetCellText(hc));
                    PdfPCell headerCell = new PdfPCell(new Phrase(headerText, FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE)))
                    {
                        BackgroundColor = BaseColor.DARK_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 6
                    };
                    table.AddCell(headerCell);
                }
            }

            // rows – use GetCellText so TemplateFields (your "User0004") are captured
            foreach (GridViewRow row in grid.Rows)
            {
                for (int c = 0; c < row.Cells.Count; c++)
                {
                    string text = GetCellText(row.Cells[c]);   // <-- key change

                    // simple alignment rules
                    string header = (grid.HeaderRow != null && c < grid.HeaderRow.Cells.Count)
                                    ? (grid.HeaderRow.Cells[c].Text ?? "").ToLower()
                                    : "";
                    int align = Element.ALIGN_LEFT;
                    if (header.Contains("quantity") || header.Contains("price") || header.Contains("amount") ||
                                        header.Contains("sqm") || header.Contains("ml"))
                        align = Element.ALIGN_RIGHT;
                    else if (header.Contains("status") || header.Contains("date"))
                        align = Element.ALIGN_CENTER;
                    else if (int.TryParse(text, out _))
                        align = Element.ALIGN_CENTER;

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
            DateTime to = DateTime.Parse(txtToDate.Text); 

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
            lblTotalBookings.Text = GetBookingsCount(from, to).ToString();
        } // ✅ closed here

        private int GetBookingsCount(DateTime from, DateTime to)
        {
            DateTime toExclusive = to.Date.AddDays(1);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
        SELECT COUNT(*)
        FROM Bookings
        WHERE ISNULL(ScheduledDate, CreatedAt) >= @from
          AND ISNULL(ScheduledDate, CreatedAt) <  @toExclusive;", conn))
            {
                cmd.Parameters.AddWithValue("@from", from.Date);
                cmd.Parameters.AddWithValue("@toExclusive", toExclusive);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }




        private void LoadUserAccounts()
        {
            string query = "SELECT UserID, Name, Email, Role, CreatedAt FROM Users WHERE Role <> 'SuperAdmin' ORDER BY CreatedAt DESC";
            BindGrid(query, gvUserAccounts);
        }

        private void LoadInquiries(DateTime from, DateTime to)
        {
            string query = @"
        SELECT 
            InquiryID, 
            Email, 
            ContactNumber, 
            CONCAT(Lastname, ', ', Firstname, ' ', Middlename) AS FullName,
            StreetAndUnit + ', ' + Barangay + ', ' + City + ', ' + Region + ', ' + Country AS Address,
            SubmittedAt 
        FROM InquirySimple 
        WHERE SubmittedAt BETWEEN @from AND @to 
        ORDER BY SubmittedAt DESC";

            BindGrid(query, gvInquiries, from, to);
        }



        private void LoadApprovedClients(DateTime from, DateTime to)
        {
            string query = @"
        SELECT 
            ClientID, 
            CONCAT(Lastname, ', ', Firstname, ' ', Middlename) AS FullName,
            Email, 
            CreatedAt,
            CONCAT(StreetAndUnit, ', ', Barangay, ', ', City, ', ', Region, ', ', Country) AS Address
        FROM Clients 
        WHERE Status = 'Approved' 
        AND CreatedAt BETWEEN @from AND @to";

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
            DateTime toExclusive = to.Date.AddDays(1);

            string query = @"
SELECT 
    b.BookingID,
    (c.Lastname + ', ' + c.Firstname + ' ' + ISNULL(c.Middlename, '')) AS ClientName,
    (
        SELECT STRING_AGG(s.Name, ', ')
        FROM BookingServices bs
        INNER JOIN Services s ON bs.ServiceID = s.ServiceID
        WHERE bs.BookingID = b.BookingID
    ) AS Services,
    t.GroupName AS TeamName,
    b.ScheduledDate,
    b.Status
FROM Bookings b
LEFT JOIN Clients c ON b.ClientID = c.ClientID
LEFT JOIN Teams   t ON b.TeamID   = t.TeamID
WHERE
(
    b.ScheduledDate IS NOT NULL
    AND b.ScheduledDate >= @From AND b.ScheduledDate < @ToExclusive
)
OR
(
    b.ScheduledDate IS NULL AND b.CreatedAt IS NOT NULL
    AND b.CreatedAt >= @From AND b.CreatedAt < @ToExclusive
)
OR
(
    b.ScheduledDate IS NULL AND b.CreatedAt IS NULL
    -- include truly undated bookings so they don't disappear
    -- comment this OR out if you prefer to hide them
)
ORDER BY 
    CASE WHEN b.ScheduledDate IS NOT NULL THEN b.ScheduledDate ELSE b.CreatedAt END DESC;";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@From", from.Date);
                cmd.Parameters.AddWithValue("@ToExclusive", toExclusive);

                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                gvBookings.DataSource = dt;
                gvBookings.DataBind();
            }
        }




        private void LoadInspections(DateTime from, DateTime to)
        {
            string sql = @"
SELECT 
    ins.InspectionID,
    ISNULL(usr.Name, CONCAT('Inspector #', ins.InspectorID)) AS InspectorName,
    CONCAT(ISNULL(iq.LastName,''), ', ', ISNULL(iq.FirstName,''),
           CASE WHEN NULLIF(iq.MiddleName,'') IS NOT NULL THEN ' ' + iq.MiddleName ELSE '' END) AS ClientName,
    RTRIM(
      CONCAT(
        ISNULL(NULLIF(iq.StreetAndUnit,'' ) + ', ', ''),
        ISNULL(NULLIF(iq.Barangay    ,'' ) + ', ', ''),
        ISNULL(NULLIF(iq.City        ,'' ) + ', ', ''),
        ISNULL(NULLIF(iq.Region      ,'' ) + ', ', ''),
        ISNULL(NULLIF(iq.Country     ,'' ), '')
      )
    ) AS ClientAddress,
    ins.ScheduledDate,
    ins.InspectionStatus,
    ins.Remarks
FROM Inspections AS ins
LEFT JOIN InquirySimple AS iq ON iq.InquiryID = ins.InquiryID
LEFT JOIN Users        AS usr ON usr.UserID    = ins.InspectorID
WHERE CAST(ins.ScheduledDate AS date) BETWEEN CAST(@From AS date) AND CAST(@To AS date)
ORDER BY ins.ScheduledDate DESC;";

            BindGrid(sql, gvInspections, from, to);
        }




        private void BindGrid(string query, GridView grid, DateTime? from = null, DateTime? to = null, DateTime? teamDate = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                string q = query.ToLowerInvariant();

                if (q.Contains("@from"))
                {
                    var p = cmd.Parameters.Add("@from", SqlDbType.Date);
                    p.Value = (object)from?.Date ?? DBNull.Value;
                }

                if (q.Contains("@to"))
                {
                    var p = cmd.Parameters.Add("@to", SqlDbType.Date);
                    p.Value = (object)to?.Date ?? DBNull.Value;
                }

                if (q.Contains("@teamdate"))
                {
                    var p = cmd.Parameters.Add("@teamDate", SqlDbType.Date);
                    p.Value = (object)teamDate?.Date ?? DBNull.Value;
                }

                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
                grid.DataBind();
            }
        }



        private int GetCount(string table, string dateField, DateTime from, DateTime to, string where = null)
        {
            bool useDate = !string.IsNullOrEmpty(dateField);

            // compute end-exclusive only if needed, and clamp at MaxValue
            DateTime toExclusive = DateTime.MinValue;
            if (useDate)
            {
                var toDate = to.Date;
                toExclusive = (toDate == DateTime.MaxValue.Date) ? DateTime.MaxValue : toDate.AddDays(1);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                var sb = new StringBuilder();
                sb.Append($"SELECT COUNT(*) FROM {table} WHERE 1=1");
                if (useDate)
                    sb.Append($" AND {dateField} >= @from AND {dateField} < @toExclusive");
                if (!string.IsNullOrEmpty(where))
                    sb.Append($" AND {where}");

                using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
                {
                    if (useDate)
                    {
                        cmd.Parameters.AddWithValue("@from", from.Date);
                        cmd.Parameters.AddWithValue("@toExclusive", toExclusive);
                    }

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }


        private static string GetIdPrefix(string keyName)
        {
            if (keyName.IndexOf("user", StringComparison.OrdinalIgnoreCase) >= 0) return "User";
            if (keyName.IndexOf("client", StringComparison.OrdinalIgnoreCase) >= 0) return "Client";
            if (keyName.IndexOf("booking", StringComparison.OrdinalIgnoreCase) >= 0) return "Booking";
            if (keyName.IndexOf("item", StringComparison.OrdinalIgnoreCase) >= 0) return "Item";
            if (keyName.IndexOf("inspect", StringComparison.OrdinalIgnoreCase) >= 0) return "Inspect";
            if (keyName.IndexOf("team", StringComparison.OrdinalIgnoreCase) >= 0) return "Team";
            return "ID";
        }

        private static string PrettyId(string keyName, object rawVal)
        {
            var s = rawVal?.ToString() ?? "";
            if (int.TryParse(s, out var n)) return $"{GetIdPrefix(keyName)}{n:D4}";
            return $"{GetIdPrefix(keyName)}{s}";
        }

        private static string GetCellText(TableCell cell)
        {
            // Read inner controls (TemplateFields) first
            if (cell.Controls != null && cell.Controls.Count > 0)
            {
                foreach (Control ctrl in cell.Controls)
                {
                    if (ctrl is ITextControl t) return (t.Text ?? "").Trim();
                    if (ctrl is IButtonControl b) return (b.Text ?? "").Trim();
                    if (ctrl is Literal l) return (l.Text ?? "").Trim();
                    if (ctrl is LinkButton lb) return (lb.Text ?? "").Trim();
                }
            }
            var raw = HttpUtility.HtmlDecode(cell.Text ?? "").Trim();
            return raw == "&nbsp;" ? "" : raw;
        }

        private void ExportGridViewToPDF(GridView grid, string title)
        {
            if (grid.Rows.Count == 0) return;

            Document doc = new Document(PageSize.A4.Rotate(), 10f, 10f, 20f, 10f);
            using (var ms = new MemoryStream())
            {
                var writer = PdfWriter.GetInstance(doc, ms);
                writer.PageEvent = new PdfWatermark();

                string userPassword = "default123";
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

                int keyCount = grid.DataKeyNames?.Length ?? 0;
                int visibleCols = grid.HeaderRow?.Cells.Count ?? grid.Columns.Count;
                int columnCount = Math.Max(1, visibleCols) + keyCount;

                var table = new PdfPTable(columnCount) { WidthPercentage = 100 };

                // 1) Headers: DataKey headers first (prettified ID columns)
                if (keyCount > 0)
                {
                    foreach (var keyName in grid.DataKeyNames)
                    {
                        string headerText = $"{GetIdPrefix(keyName)} ID";
                        var h = new PdfPCell(new Phrase(headerText, FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE)))
                        { BackgroundColor = BaseColor.DARK_GRAY, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 6 };
                        table.AddCell(h);
                    }
                }
                // Then visible headers
                if (grid.HeaderRow != null)
                {
                    foreach (TableCell hc in grid.HeaderRow.Cells)
                    {
                        var h = new PdfPCell(new Phrase(HttpUtility.HtmlDecode(hc.Text ?? "").Trim(),
                            FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE)))
                        { BackgroundColor = BaseColor.DARK_GRAY, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 6 };
                        table.AddCell(h);
                    }
                }
                else
                {
                    for (int i = 0; i < visibleCols; i++)
                    {
                        var h = new PdfPCell(new Phrase($"Column {i + 1}", FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE)))
                        { BackgroundColor = BaseColor.DARK_GRAY, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 6 };
                        table.AddCell(h);
                    }
                }

                // 2) Rows
                for (int r = 0; r < grid.Rows.Count; r++)
                {
                    var row = grid.Rows[r];

                    // a) Prettified ID cells from DataKeys
                    if (keyCount > 0)
                    {
                        var keys = grid.DataKeys[r];
                        foreach (var keyName in grid.DataKeyNames)
                        {
                            string pretty = PrettyId(keyName, keys?.Values[keyName]);
                            var idCell = new PdfPCell(new Phrase(pretty, FontFactory.GetFont("Arial", 11)))
                            { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 };
                            table.AddCell(idCell);
                        }
                    }

                    // b) Visible cells (TemplateField-safe)
                    for (int c = 0; c < row.Cells.Count; c++)
                    {
                        string text = GetCellText(row.Cells[c]);
                        string header = (grid.HeaderRow != null && c < grid.HeaderRow.Cells.Count)
                                        ? (grid.HeaderRow.Cells[c].Text ?? "").ToLower()
                                        : "";

                        int align = Element.ALIGN_LEFT;
                        if (header.Contains("quantity") || header.Contains("price") || header.Contains("amount") ||
                            header.Contains("sqm") || header.Contains("ml"))
                            align = Element.ALIGN_RIGHT;
                        else if (header.Contains("status") || header.Contains("date"))
                            align = Element.ALIGN_CENTER;
                        else if (int.TryParse(text, out _))
                            align = Element.ALIGN_CENTER;

                        // Optional: if a visible "...ID" column exists, replace raw with pretty (uses first key)
                        if (header.EndsWith("id") && keyCount > 0)
                        {
                            var firstKey = grid.DataKeyNames[0];
                            text = PrettyId(firstKey, grid.DataKeys[r]?.Values[firstKey]);
                            align = Element.ALIGN_CENTER;
                        }

                        var cell = new PdfPCell(new Phrase(text, FontFactory.GetFont("Arial", 11)))
                        { HorizontalAlignment = align, Padding = 5 };
                        table.AddCell(cell);
                    }
                }

                doc.Add(table);
                doc.Close();

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", $"attachment;filename={title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
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


    }
}