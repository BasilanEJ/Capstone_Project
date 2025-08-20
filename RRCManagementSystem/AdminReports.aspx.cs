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
    using RRCManagementSystem.Helpers; // keep if you actually have PdfWatermark

    namespace RRCManagementSystem
    {
        public partial class AdminReports : System.Web.UI.Page
        {
            private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            protected void Page_Load(object sender, EventArgs e)
            {
                if (!IsPostBack)
                {
                    txtFromDate.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                    txtToDate.Text   = DateTime.Now.ToString("yyyy-MM-dd");
                    txtTeamDate.Text = DateTime.Today.ToString("yyyy-MM-dd");

                    LoadReports();
                    LoadUserAccounts();
                    LoadTeamReports();
                    LoadSales(DateTime.Parse(txtFromDate.Text), DateTime.Parse(txtToDate.Text));
                }
            }

            // ---------------------- UI actions ----------------------
            protected void btnFilter_Click(object sender, EventArgs e)
            {
                LoadReports();
                LoadTeamReports();
                AddAuditLog(Convert.ToInt32(Session["UserID"]), "Filtered Admin Reports");
            }

            protected void btnTeamDateApply_Click(object sender, EventArgs e)
            {
                LoadTeamReports();
                AddAuditLog(Convert.ToInt32(Session["UserID"]), "Applied Team Availability Date in Reports");
            }

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            // Refresh data so the export matches the current filters
            LoadReports();
            LoadTeamReports();

            var doc = new iTextSharp.text.Document(PageSize.A4.Rotate(), 10f, 10f, 20f, 10f);
            using (var ms = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);

                // <<< attach watermark (drawn over content)
                writer.PageEvent = new PdfWatermark();

                // Optional encryption (same as you used before)
                string userPassword = Session["Password"]?.ToString() ?? "default123";
                writer.SetEncryption(
                    Encoding.UTF8.GetBytes(userPassword),
                    Encoding.UTF8.GetBytes(userPassword),
                    PdfWriter.ALLOW_PRINTING,
                    PdfWriter.ENCRYPTION_AES_128
                );

                doc.Open();

                // Add each grid as a new section/page
                AddGridToPDF(doc, gvUserAccounts, "👤 User Accounts");
                AddGridToPDF(doc, gvInquiries, "📬 Inquiries");
                AddGridToPDF(doc, gvApprovedClients, "✅ Approved Clients");
                AddGridToPDF(doc, gvInventorySnapshots, "📦 Total Stocks Snapshot (Daily)");
                AddGridToPDF(doc, gvInventory, "📦 Inventory Details");
                AddGridToPDF(doc, gvSales, "💳 Sales");
                AddGridToPDF(doc, gvEquipment, "🛠️ Equipment Status");
                AddGridToPDF(doc, gvBookings, "📅 Booking Details");
                AddGridToPDF(doc, gvInspections, "🔍 Inspection Details");
                AddGridToPDF(doc, gvTeamsSummary, "👥 Team Summary");
                AddGridToPDF(doc, gvTeamMembers, "👨‍👩‍👧‍👦 Team Members");

                doc.Close();

                // Return the file
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

            protected void btnExportUsers_Click(object sender, EventArgs e)
            {
                if (gvUserAccounts.Rows.Count > 0) ExportGridViewToPDF(gvUserAccounts, "Users_Report");
            }
            protected void btnExportSales_Click(object sender, EventArgs e)
            {
                if (gvSales.Rows.Count > 0) ExportGridViewToPDF(gvSales, "Sales_Report");
            }
            protected void btnExportInquiries_Click(object sender, EventArgs e)
            {
                if (gvInquiries.Rows.Count > 0) ExportGridViewToPDF(gvInquiries, "Inquiry_Report");
            }
            protected void btnExportClients_Click(object sender, EventArgs e)
            {
                if (gvApprovedClients.Rows.Count > 0) ExportGridViewToPDF(gvApprovedClients, "ApprovedClients_Report");
            }
            protected void btnExportInventorySnapshots_Click(object sender, EventArgs e)
            {
                if (gvInventorySnapshots.Rows.Count > 0) ExportGridViewToPDF(gvInventorySnapshots, "InventorySnapshots_Report");
            }
            protected void btnExportInventory_Click(object sender, EventArgs e)
            {
                if (gvInventory.Rows.Count > 0) ExportGridViewToPDF(gvInventory, "Inventory_Report");
            }
            protected void btnExportEquipment_Click(object sender, EventArgs e)
            {
                if (gvEquipment.Rows.Count > 0) ExportGridViewToPDF(gvEquipment, "Equipment_Report");
            }
            protected void btnExportBookings_Click(object sender, EventArgs e)
            {
                if (gvBookings.Rows.Count > 0) ExportGridViewToPDF(gvBookings, "Bookings_Report");
            }
            protected void btnExportInspections_Click(object sender, EventArgs e)
            {
                if (gvInspections.Rows.Count > 0) ExportGridViewToPDF(gvInspections, "Inspections_Report");
            }

            // ---------------------- Loaders (SP-based) ----------------------
            private void LoadReports()
            {
                var from = DateTime.Parse(txtFromDate.Text).Date;
                var to   = DateTime.Parse(txtToDate.Text).Date;

                LoadSummaryCounts(from, to);
                LoadInquiries(from, to);
                LoadApprovedClients(from, to);
                LoadInventorySnapshots(from, to);
                LoadInventory();
                LoadEquipment();
                LoadBookings(from, to);
                LoadInspections(from, to);
                LoadSales(from, to);
            }

            private void LoadSummaryCounts(DateTime from, DateTime to)
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spReports_SummaryCounts", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.Date){Value = from});
                    cmd.Parameters.Add(new SqlParameter("@ToDate",   SqlDbType.Date){Value = to});

                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            lblTotalInquiries.Text = Convert.ToInt32(r["TotalInquiries"]).ToString();
                            lblTotalClients.Text   = Convert.ToInt32(r["TotalApprovedClients"]).ToString();
                            lblTotalEquipment.Text = Convert.ToInt32(r["TotalAvailableEquipment"]).ToString();
                            lblTotalBookings.Text  = Convert.ToInt32(r["TotalBookings"]).ToString();
                        }
                    }
                }
            }

            private void LoadUserAccounts()
            {
                gvUserAccounts.DataSource = ExecToTable("dbo.spReports_UserAccounts");
                gvUserAccounts.DataBind();
            }

            private void LoadInquiries(DateTime from, DateTime to)
            {
                gvInquiries.DataSource = ExecToTable("dbo.spReports_Inquiries",
                    new SqlParameter("@FromDate", SqlDbType.Date){Value = from},
                    new SqlParameter("@ToDate",   SqlDbType.Date){Value = to});
                gvInquiries.DataBind();
            }

            private void LoadApprovedClients(DateTime from, DateTime to)
            {
                gvApprovedClients.DataSource = ExecToTable("dbo.spReports_ApprovedClients",
                    new SqlParameter("@FromDate", SqlDbType.Date){Value = from},
                    new SqlParameter("@ToDate",   SqlDbType.Date){Value = to});
                gvApprovedClients.DataBind();
            }

            private void LoadInventorySnapshots(DateTime from, DateTime to)
            {
                gvInventorySnapshots.DataSource = ExecToTable("dbo.spReports_InventorySnapshots",
                    new SqlParameter("@FromDate", SqlDbType.Date){Value = from},
                    new SqlParameter("@ToDate",   SqlDbType.Date){Value = to});
                gvInventorySnapshots.DataBind();
            }

            private void LoadInventory()
            {
                gvInventory.DataSource = ExecToTable("dbo.spReports_Inventory");
                gvInventory.DataBind();
            }

        private void LoadEquipment()
        {
            DateTime reportDate = DateTime.Today; // or DateTime.Parse(txtTeamDate.Text).Date;

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spReports_EquipmentAvailableOnDate", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Date", reportDate);

                var dt = new DataTable();
                da.Fill(dt);

                gvEquipment.DataSource = dt;
                gvEquipment.DataBind();
            }
        }


        private void LoadBookings(DateTime from, DateTime to)
            {
                gvBookings.DataSource = ExecToTable("dbo.spReports_Bookings",
                    new SqlParameter("@FromDate", SqlDbType.Date){Value = from},
                    new SqlParameter("@ToDate",   SqlDbType.Date){Value = to});
                gvBookings.DataBind();
            }

            private void LoadInspections(DateTime from, DateTime to)
            {
                gvInspections.DataSource = ExecToTable("dbo.spReports_Inspections",
                    new SqlParameter("@FromDate", SqlDbType.Date){Value = from},
                    new SqlParameter("@ToDate",   SqlDbType.Date){Value = to});
                gvInspections.DataBind();
            }

            private void LoadSales(DateTime from, DateTime to)
            {
                var dt = ExecToTable("dbo.spReports_Sales",
                    new SqlParameter("@FromDate", SqlDbType.Date){Value = from},
                    new SqlParameter("@ToDate",   SqlDbType.Date){Value = to});

                if (!dt.Columns.Contains("TransactionIDFormatted"))
                    dt.Columns.Add("TransactionIDFormatted", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    row["TransactionIDFormatted"] = PrettyId("Transaction", row["TransactionID"]);
                }

                gvSales.DataSource = dt;
                gvSales.DataBind();

                decimal total = 0;
                foreach (DataRow row in dt.Rows) total += row.Field<decimal>("Amount");
                lblSalesSummary.Text = $"Total Sales: ₱{total:N2} ({dt.Rows.Count} transactions)";
            }

            private void LoadTeamReports()
            {
                var from     = DateTime.Parse(txtFromDate.Text).Date;
                var to       = DateTime.Parse(txtToDate.Text).Date;
                var teamDate = DateTime.TryParse(txtTeamDate.Text, out var d) ? d.Date : DateTime.Today;

                LoadTeamsSummary(from, to, teamDate);
                LoadTeamMembers();
            }

            private void LoadTeamsSummary(DateTime from, DateTime to, DateTime teamDate)
            {
                gvTeamsSummary.DataSource = ExecToTable("dbo.spReports_TeamsSummary",
                    new SqlParameter("@FromDate", SqlDbType.Date){Value = from},
                    new SqlParameter("@ToDate",   SqlDbType.Date){Value = to},
                    new SqlParameter("@TeamDate", SqlDbType.Date){Value = teamDate});
                gvTeamsSummary.DataBind();
            }

            private void LoadTeamMembers()
            {
                gvTeamMembers.DataSource = ExecToTable("dbo.spReports_TeamMembers");
                gvTeamMembers.DataBind();
            }

            // ---------------------- Helpers ----------------------
            private DataTable ExecToTable(string procName, params SqlParameter[] parameters)
            {
                var dt = new DataTable();
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
                return dt;
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

                foreach (GridViewRow row in grid.Rows)
                {
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

            private static string GetCellText(TableCell cell)
            {
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

            private static string GetIdPrefix(string keyName)
            {
                if (keyName.IndexOf("user", StringComparison.OrdinalIgnoreCase) >= 0) return "User";
                if (keyName.IndexOf("client", StringComparison.OrdinalIgnoreCase) >= 0) return "Client";
                if (keyName.IndexOf("booking", StringComparison.OrdinalIgnoreCase) >= 0) return "Booking";
                if (keyName.IndexOf("item", StringComparison.OrdinalIgnoreCase) >= 0) return "Item";
                if (keyName.IndexOf("inspect", StringComparison.OrdinalIgnoreCase) >= 0) return "Inspect";
                if (keyName.IndexOf("team", StringComparison.OrdinalIgnoreCase) >= 0) return "Team";
                if (keyName.IndexOf("sales", StringComparison.OrdinalIgnoreCase) >= 0) return "Sales";
                return "ID";
            }

            private static string PrettyId(string keyName, object rawVal)
            {
                var s = rawVal?.ToString() ?? "";
                if (int.TryParse(s, out var n)) return $"{GetIdPrefix(keyName)}{n:D4}";
                return $"{GetIdPrefix(keyName)}{s}";
            }

        private void ExportGridViewToPDF(GridView grid, string title)
        {
            if (grid.Rows.Count == 0) return;

            var doc = new iTextSharp.text.Document(PageSize.A4.Rotate(), 10f, 10f, 20f, 10f);
            using (var ms = new MemoryStream())
            {
                var writer = PdfWriter.GetInstance(doc, ms);

                // <<< attach watermark (drawn over content)
                writer.PageEvent = new PdfWatermark();

                // Optional encryption
                string userPassword = "default123";
                writer.SetEncryption(
                    Encoding.UTF8.GetBytes(userPassword),
                    Encoding.UTF8.GetBytes(userPassword),
                    PdfWriter.ALLOW_PRINTING,
                    PdfWriter.ENCRYPTION_AES_128
                );

                doc.Open();

                // Title + timestamp
                doc.Add(new Paragraph(title, FontFactory.GetFont("Arial", 16, Font.BOLD)));
                doc.Add(new Paragraph("Generated at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                doc.Add(new Paragraph(" "));

                int visibleCols = grid.HeaderRow?.Cells.Count ?? grid.Columns.Count;
                var table = new PdfPTable(visibleCols) { WidthPercentage = 100, SpacingBefore = 10f };

                // Headers
                if (grid.HeaderRow != null)
                {
                    foreach (TableCell hc in grid.HeaderRow.Cells)
                    {
                        string headerText = HttpUtility.HtmlDecode(hc.Text ?? "").Trim();
                        var headerCell = new PdfPCell(new Phrase(
                                headerText,
                                FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE)))
                        {
                            BackgroundColor = BaseColor.DARK_GRAY,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 6
                        };
                        table.AddCell(headerCell);
                    }
                }

                // Rows
                foreach (GridViewRow row in grid.Rows)
                {
                    for (int c = 0; c < row.Cells.Count; c++)
                    {
                        string text = GetCellText(row.Cells[c]);

                        // simple alignment heuristics
                        int align = Element.ALIGN_LEFT;
                        string header = (grid.HeaderRow != null && c < grid.HeaderRow.Cells.Count)
                                        ? (grid.HeaderRow.Cells[c].Text ?? "").ToLower()
                                        : "";
                        if (header.Contains("quantity") || header.Contains("price") || header.Contains("amount") ||
                            header.Contains("sqm") || header.Contains("ml"))
                            align = Element.ALIGN_RIGHT;
                        else if (header.Contains("status") || header.Contains("date"))
                            align = Element.ALIGN_CENTER;
                        else if (int.TryParse(text, out _))
                            align = Element.ALIGN_CENTER;

                        var bodyCell = new PdfPCell(new Phrase(text, FontFactory.GetFont("Arial", 11)))
                        {
                            HorizontalAlignment = align,
                            Padding = 5
                        };
                        table.AddCell(bodyCell);
                    }
                }

                doc.Add(table);
                doc.Close();

                // Return the file
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", $"attachment;filename={title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(ms.ToArray());
                Response.Flush();
                Response.SuppressContent = true;
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }


        private void AddAuditLog(int adminId, string action)
            {
                try
                {
                    using (var con = new SqlConnection(cs))
                    using (var cmd = new SqlCommand("dbo.spAudit_Insert", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@AdminID", SqlDbType.Int){Value = adminId});
                        cmd.Parameters.Add(new SqlParameter("@Action",  SqlDbType.NVarChar, 255){Value = action});
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { /* swallow */ }
            }
        }
    }
