using iTextSharp.text;
using iTextSharp.text.pdf;
using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class BookingDetails : System.Web.UI.Page
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
        private const string PDF_PASSWORD = "default123"; // Default password for PDF

        // ViewState property to persist BookingID across postbacks
        private int BookingID
        {
            get
            {
                if (ViewState["BookingID"] != null)
                    return (int)ViewState["BookingID"];
                return 0;
            }
            set
            {
                ViewState["BookingID"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                string bookingIDParam = Request.QueryString["BookingID"];
                if (!string.IsNullOrEmpty(bookingIDParam))
                {
                    if (int.TryParse(bookingIDParam, out int tempBookingID) && tempBookingID > 0)
                    {
                        BookingID = tempBookingID;
                        LoadBookingDetails();
                        pnlDetails.Visible = true;
                    }
                    else
                    {
                        ShowError($"Invalid Booking ID: '{bookingIDParam}'");
                        pnlDetails.Visible = false;
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                    lblMessage.Visible = false;
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string bookingCode = txtSearchBookingCode.Text.Trim();
            if (string.IsNullOrEmpty(bookingCode))
            {
                ShowError("Please enter a Booking Code to search.");
                pnlDetails.Visible = false;
                return;
            }

            int? foundBookingID = GetBookingIDByCode(bookingCode);
            if (foundBookingID.HasValue)
            {
                BookingID = foundBookingID.Value;
                LoadBookingDetails();
                pnlDetails.Visible = true;
                lblMessage.Visible = false;
            }
            else
            {
                ShowError($"No booking found with code: '{bookingCode}'");
                pnlDetails.Visible = false;
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearchBookingCode.Text = string.Empty;
            BookingID = 0;
            pnlDetails.Visible = false;
            lblMessage.Visible = false;
        }

        protected void btnExportPdf_Click(object sender, EventArgs e)
        {
            try
            {
                if (BookingID <= 0)
                {
                    ShowError("No booking selected for export.");
                    return;
                }

                string bookingCode = lblBookingCode.Text;
                if (string.IsNullOrEmpty(bookingCode))
                {
                    bookingCode = "Booking";
                }

                GeneratePasswordProtectedPdf(BookingID, bookingCode);
            }
            catch (Exception ex)
            {
                ShowError("Error exporting to PDF: " + ex.Message);
            }
        }

        private void GeneratePasswordProtectedPdf(int bookingID, string bookingCode)
        {
            Document document = null;
            MemoryStream ms = null;

            try
            {
                document = new Document(PageSize.A4, 40, 40, 80, 60);
                ms = new MemoryStream();

                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                writer.CloseStream = false;

                // PASSWORD PROTECTION
                writer.SetEncryption(
                    System.Text.Encoding.UTF8.GetBytes(PDF_PASSWORD),
                    System.Text.Encoding.UTF8.GetBytes(PDF_PASSWORD + "_owner"),
                    PdfWriter.ALLOW_PRINTING | PdfWriter.ALLOW_COPY,
                    PdfWriter.ENCRYPTION_AES_128
                );

                string logoPath = Server.MapPath("~/Images/logo.png");
                string generatedBy = Session["Username"]?.ToString() ?? "Admin";
                writer.PageEvent = new PdfHeaderFooter("Booking Details Report", generatedBy, logoPath);

                document.Open();
                AddBookingDetailsToPdf(document, bookingID);
                document.Close();

                byte[] pdfBytes = ms.ToArray();

                // ✅ Stable response handling
                Response.Clear();
                Response.BufferOutput = true;
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition",
                    $"attachment; filename=BookingDetails_{bookingCode}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();

                // ✅ Write PDF safely using OutputStream
                Response.OutputStream.Write(pdfBytes, 0, pdfBytes.Length);
                Response.Flush();

                // ✅ Suppress further content and gracefully complete
                Response.SuppressContent = true;
                HttpContext.Current.ApplicationInstance.CompleteRequest();

                // Cleanup
                ms.Close();
                ms.Dispose();
            }
            catch (Exception ex)
            {
                if (document != null && document.IsOpen())
                    document.Close();

                if (ms != null)
                {
                    ms.Close();
                    ms.Dispose();
                }

                ShowError("Error exporting to PDF: " + ex.Message);
            }


        }



        private void AddBookingDetailsToPdf(Document document, int bookingID)
        {
            Font titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD, new BaseColor(30, 64, 175));
            Font headingFont = FontFactory.GetFont("Arial", 14, Font.BOLD, new BaseColor(30, 64, 175));
            Font labelFont = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.BLACK);
            Font valueFont = FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK);

            Paragraph title = new Paragraph("BOOKING DETAILS REPORT", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20f;
            document.Add(title);

            DataTable bookingData = GetBookingDataForPdf(bookingID);
            if (bookingData.Rows.Count == 0)
            {
                document.Add(new Paragraph("No booking data found.", valueFont));
                return;
            }

            DataRow row = bookingData.Rows[0];

            PdfPTable headerTable = new PdfPTable(1);
            headerTable.WidthPercentage = 100;
            PdfPCell headerCell = new PdfPCell(new Phrase("Booking Code: " + row["BookingCode"].ToString(), headingFont));
            headerCell.BackgroundColor = new BaseColor(219, 234, 254);
            headerCell.Padding = 10f;
            headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
            headerCell.Border = Rectangle.NO_BORDER;
            headerTable.AddCell(headerCell);
            document.Add(headerTable);
            document.Add(new Paragraph(" "));

            // Booking Information
            document.Add(new Paragraph("BOOKING INFORMATION", headingFont));
            document.Add(new Paragraph(" "));
            PdfPTable bookingTable = CreateDetailTable();
            AddDetailRow(bookingTable, "Service Type:", row["ServiceNames"]?.ToString() ?? "N/A", labelFont, valueFont);
            AddDetailRow(bookingTable, "Status:", row["Status"]?.ToString() ?? "N/A", labelFont, valueFont);

            string scheduledDate = row["ScheduledDate"] != DBNull.Value
                ? Convert.ToDateTime(row["ScheduledDate"]).ToString("MMMM dd, yyyy (dddd)")
                : "Not scheduled";
            AddDetailRow(bookingTable, "Scheduled Date:", scheduledDate, labelFont, valueFont);

            string startTime = row["StartTime"] != DBNull.Value
                ? DateTime.Today.Add((TimeSpan)row["StartTime"]).ToString("hh:mm tt")
                : "Not set";
            AddDetailRow(bookingTable, "Start Time:", startTime, labelFont, valueFont);
            AddDetailRow(bookingTable, "Square Meters:", (row["SQM"]?.ToString() ?? "0") + " sqm", labelFont, valueFont);

            string price = row["Price"] != DBNull.Value
                ? "₱" + Convert.ToDecimal(row["Price"]).ToString("N2")
                : "₱0.00";
            AddDetailRow(bookingTable, "Total Cost:", price, labelFont, valueFont);

            string createdAt = row["CreatedAt"] != DBNull.Value
                ? Convert.ToDateTime(row["CreatedAt"]).ToString("MMM dd, yyyy hh:mm tt")
                : "N/A";
            AddDetailRow(bookingTable, "Created At:", createdAt, labelFont, valueFont);
            document.Add(bookingTable);
            document.Add(new Paragraph(" "));

            // Customer Information
            document.Add(new Paragraph("CUSTOMER INFORMATION", headingFont));
            document.Add(new Paragraph(" "));
            PdfPTable customerTable = CreateDetailTable();
            AddDetailRow(customerTable, "Name:", row["ClientName"]?.ToString() ?? "N/A", labelFont, valueFont);

            string email = "N/A";
            try
            {
                string encryptedEmail = row["ClientEmail"]?.ToString();
                if (!string.IsNullOrEmpty(encryptedEmail))
                    email = AESHelper.DecryptEmail(encryptedEmail);
            }
            catch { }
            AddDetailRow(customerTable, "Email:", email, labelFont, valueFont);

            string phone = "N/A";
            try
            {
                string encryptedPhone = row["ClientPhone"]?.ToString();
                if (!string.IsNullOrEmpty(encryptedPhone))
                    phone = AESHelper.DecryptField(encryptedPhone);
            }
            catch { }
            AddDetailRow(customerTable, "Phone:", phone, labelFont, valueFont);

            string address = "N/A";
            try
            {
                string concatenatedAddress = row["ClientAddress"]?.ToString();
                if (!string.IsNullOrEmpty(concatenatedAddress))
                {
                    string[] parts = concatenatedAddress.Split(new[] { ", " }, StringSplitOptions.None);
                    if (parts.Length >= 4)
                    {
                        string street = AESHelper.DecryptField(parts[0]);
                        string barangay = AESHelper.DecryptField(parts[1]);
                        string city = AESHelper.DecryptField(parts[2]);
                        string region = AESHelper.DecryptField(parts[3]);
                        address = $"{street}, {barangay}, {city}, {region}";
                    }
                }
            }
            catch { }
            AddDetailRow(customerTable, "Address:", address, labelFont, valueFont);
            document.Add(customerTable);
            document.Add(new Paragraph(" "));

            // Summary
            DataTable summaryData = GetAssignmentSummary(bookingID);
            if (summaryData.Rows.Count > 0)
            {
                DataRow summaryRow = summaryData.Rows[0];
                document.Add(new Paragraph("RESOURCE ASSIGNMENT SUMMARY", headingFont));
                document.Add(new Paragraph(" "));
                PdfPTable summaryTable = CreateDetailTable();
                AddDetailRow(summaryTable, "Team Assigned:", summaryRow["TeamAssigned"].ToString(), labelFont, valueFont);
                AddDetailRow(summaryTable, "Equipment Items:", summaryRow["EquipmentCount"].ToString(), labelFont, valueFont);
                AddDetailRow(summaryTable, "Chemicals:", summaryRow["ChemicalCount"].ToString(), labelFont, valueFont);
                document.Add(summaryTable);
                document.Add(new Paragraph(" "));
            }

            AddTeamTableToPdf(document, bookingID, headingFont, labelFont, valueFont);
            AddEquipmentTableToPdf(document, bookingID, headingFont, labelFont, valueFont);
            AddChemicalsTableToPdf(document, bookingID, headingFont, labelFont, valueFont);
        }

        private PdfPTable CreateDetailTable()
        {
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 35f, 65f });
            return table;
        }

        private void AddDetailRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
            labelCell.Padding = 8f;
            labelCell.BackgroundColor = new BaseColor(249, 250, 251);
            labelCell.Border = Rectangle.BOTTOM_BORDER;
            labelCell.BorderColor = BaseColor.LIGHT_GRAY;
            table.AddCell(labelCell);

            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont));
            valueCell.Padding = 8f;
            valueCell.Border = Rectangle.BOTTOM_BORDER;
            valueCell.BorderColor = BaseColor.LIGHT_GRAY;
            table.AddCell(valueCell);
        }

        private void AddTeamTableToPdf(Document document, int bookingID, Font headingFont, Font labelFont, Font valueFont)
        {
            DataTable teamData = GetAssignedTeam(bookingID);
            document.Add(new Paragraph("ASSIGNED TEAM", headingFont));
            document.Add(new Paragraph(" "));

            if (teamData.Rows.Count == 0)
            {
                document.Add(new Paragraph("No team assigned yet.", valueFont));
                document.Add(new Paragraph(" "));
                return;
            }

            PdfPTable table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 40f, 30f, 30f });
            AddTableHeader(table, "Team Name", labelFont);
            AddTableHeader(table, "Status", labelFont);
            AddTableHeader(table, "Created At", labelFont);

            foreach (DataRow row in teamData.Rows)
            {
                AddTableCell(table, row["TeamName"].ToString(), valueFont);
                AddTableCell(table, row["TeamStatus"].ToString(), valueFont);
                string createdAt = row["TeamCreatedAt"] != DBNull.Value
                    ? Convert.ToDateTime(row["TeamCreatedAt"]).ToString("MMM dd, yyyy")
                    : "";
                AddTableCell(table, createdAt, valueFont);
            }
            document.Add(table);
            document.Add(new Paragraph(" "));
        }

        private void AddEquipmentTableToPdf(Document document, int bookingID, Font headingFont, Font labelFont, Font valueFont)
        {
            DataTable equipmentData = GetAssignedEquipment(bookingID);
            document.Add(new Paragraph("ASSIGNED EQUIPMENT", headingFont));
            document.Add(new Paragraph(" "));

            if (equipmentData.Rows.Count == 0)
            {
                document.Add(new Paragraph("No equipment assigned.", valueFont));
                document.Add(new Paragraph(" "));
                return;
            }

            PdfPTable table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 40f, 20f, 40f });
            AddTableHeader(table, "Equipment ID", labelFont);
            AddTableHeader(table, "Quantity", labelFont);
            AddTableHeader(table, "Assigned At", labelFont);

            foreach (DataRow row in equipmentData.Rows)
            {
                AddTableCell(table, row["EquipmentID"].ToString(), valueFont);
                AddTableCell(table, row["QuantityAssigned"].ToString(), valueFont);
                string assignedAt = row["AssignedAt"] != DBNull.Value
                    ? Convert.ToDateTime(row["AssignedAt"]).ToString("MMM dd, yyyy hh:mm tt")
                    : "";
                AddTableCell(table, assignedAt, valueFont);
            }
            document.Add(table);
            document.Add(new Paragraph(" "));
        }

        private void AddChemicalsTableToPdf(Document document, int bookingID, Font headingFont, Font labelFont, Font valueFont)
        {
            DataTable chemicalData = GetAssignedChemicals(bookingID);
            document.Add(new Paragraph("ASSIGNED CHEMICALS", headingFont));
            document.Add(new Paragraph(" "));

            if (chemicalData.Rows.Count == 0)
            {
                document.Add(new Paragraph("No chemicals assigned.", valueFont));
                document.Add(new Paragraph(" "));
                return;
            }

            PdfPTable table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 40f, 20f, 40f });
            AddTableHeader(table, "Item ID", labelFont);
            AddTableHeader(table, "Quantity", labelFont);
            AddTableHeader(table, "Assigned At", labelFont);

            foreach (DataRow row in chemicalData.Rows)
            {
                AddTableCell(table, row["ItemID"].ToString(), valueFont);
                AddTableCell(table, row["QuantityAssigned"].ToString(), valueFont);
                string assignedAt = row["AssignedAt"] != DBNull.Value
                    ? Convert.ToDateTime(row["AssignedAt"]).ToString("MMM dd, yyyy hh:mm tt")
                    : "";
                AddTableCell(table, assignedAt, valueFont);
            }
            document.Add(table);
            document.Add(new Paragraph(" "));
        }

        private void AddTableHeader(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = new BaseColor(249, 250, 251);
            cell.Padding = 8f;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.BorderColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
        }

        private void AddTableCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = 8f;
            cell.BorderColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
        }

        private DataTable GetBookingDataForPdf(int bookingID)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetDetails", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        private DataTable GetAssignmentSummary(int bookingID)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetAssignmentSummary", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        private DataTable GetAssignedTeam(int bookingID)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetAssignedTeamWithMembers", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        private DataTable GetAssignedEquipment(int bookingID)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetAssignedEquipment", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        private DataTable GetAssignedChemicals(int bookingID)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetAssignedChemicals", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        private int? GetBookingIDByCode(string bookingCode)
        {
            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spBooking_GetIDByCode", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingCode", SqlDbType.NVarChar, 16).Value = bookingCode;
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                ShowError("Error searching for booking: " + ex.Message);
                return null;
            }
        }

        private void LoadBookingDetails()
        {
            try
            {
                LoadMainBookingInfo();
                LoadAssignmentSummary();
                LoadAssignedTeam();
                LoadAssignedEquipment();
                LoadAssignedChemicals();
            }
            catch (Exception ex)
            {
                ShowError("Error loading booking details: " + ex.Message);
            }
        }

        private void LoadMainBookingInfo()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetDetails", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = BookingID;
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblBookingCode.Text = reader["BookingCode"].ToString();
                        lblDetailBookingCode.Text = reader["BookingCode"].ToString();
                        lblServiceType.Text = reader["ServiceNames"]?.ToString() ?? "N/A";

                        string status = reader["Status"]?.ToString() ?? "Unknown";
                        lblStatus.Text = status;
                        lblStatus.CssClass = "status-badge " + GetStatusClass(status);

                        if (reader["ScheduledDate"] != DBNull.Value)
                        {
                            DateTime scheduledDate = Convert.ToDateTime(reader["ScheduledDate"]);
                            lblScheduledDate.Text = scheduledDate.ToString("MMMM dd, yyyy (dddd)");
                        }
                        else
                        {
                            lblScheduledDate.Text = "Not scheduled";
                        }

                        if (reader["StartTime"] != DBNull.Value)
                        {
                            TimeSpan startTime = (TimeSpan)reader["StartTime"];
                            lblStartTime.Text = DateTime.Today.Add(startTime).ToString("hh:mm tt");
                        }
                        else
                        {
                            lblStartTime.Text = "Not set";
                        }

                        lblSquareMeters.Text = reader["SQM"]?.ToString() ?? "0" + " sqm";

                        if (reader["Price"] != DBNull.Value)
                        {
                            decimal price = Convert.ToDecimal(reader["Price"]);
                            lblTotalCost.Text = "₱" + price.ToString("N2");
                        }
                        else
                        {
                            lblTotalCost.Text = "₱0.00";
                        }

                        if (reader["CreatedAt"] != DBNull.Value)
                        {
                            DateTime createdAt = Convert.ToDateTime(reader["CreatedAt"]);
                            lblCreatedAt.Text = createdAt.ToString("MMM dd, yyyy hh:mm tt");
                        }
                        else
                        {
                            lblCreatedAt.Text = "N/A";
                        }

                        lblCustomerName.Text = reader["ClientName"]?.ToString() ?? "N/A";

                        try
                        {
                            string encryptedEmail = reader["ClientEmail"]?.ToString();
                            lblCustomerEmail.Text = !string.IsNullOrEmpty(encryptedEmail)
                                ? AESHelper.DecryptEmail(encryptedEmail)
                                : "N/A";
                        }
                        catch
                        {
                            lblCustomerEmail.Text = "Error decrypting email";
                        }

                        try
                        {
                            string encryptedPhone = reader["ClientPhone"]?.ToString();
                            lblCustomerPhone.Text = !string.IsNullOrEmpty(encryptedPhone)
                                ? AESHelper.DecryptField(encryptedPhone)
                                : "N/A";
                        }
                        catch
                        {
                            lblCustomerPhone.Text = "Error decrypting phone";
                        }

                        try
                        {
                            string concatenatedAddress = reader["ClientAddress"]?.ToString();
                            if (!string.IsNullOrEmpty(concatenatedAddress))
                            {
                                string[] parts = concatenatedAddress.Split(new[] { ", " }, StringSplitOptions.None);
                                if (parts.Length >= 4)
                                {
                                    string street = AESHelper.DecryptField(parts[0]);
                                    string barangay = AESHelper.DecryptField(parts[1]);
                                    string city = AESHelper.DecryptField(parts[2]);
                                    string region = AESHelper.DecryptField(parts[3]);
                                    lblCustomerAddress.Text = $"{street}, {barangay}, {city}, {region}";
                                }
                                else
                                {
                                    lblCustomerAddress.Text = "Incomplete address data";
                                }
                            }
                            else
                            {
                                lblCustomerAddress.Text = "N/A";
                            }
                        }
                        catch
                        {
                            lblCustomerAddress.Text = "Error decrypting address";
                        }
                    }
                    else
                    {
                        ShowError("Booking not found.");
                    }
                }
            }
        }

        private void LoadAssignmentSummary()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetAssignmentSummary", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = BookingID;
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblTeamCount.Text = reader["TeamAssigned"].ToString();
                        lblEquipmentCount.Text = reader["EquipmentCount"].ToString();
                        lblChemicalCount.Text = reader["ChemicalCount"].ToString();
                    }
                }
            }
        }

        private void LoadAssignedTeam()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetAssignedTeamWithMembers", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = BookingID;
                con.Open();
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        gvTeams.DataSource = dt;
                        gvTeams.DataBind();
                        gvTeams.Visible = true;
                        pnlNoTeam.Visible = false;
                    }
                    else
                    {
                        pnlNoTeam.Visible = true;
                        gvTeams.Visible = false;
                    }
                }
            }
        }

        private void LoadAssignedEquipment()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetAssignedEquipment", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = BookingID;
                con.Open();
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        gvEquipment.DataSource = dt;
                        gvEquipment.DataBind();
                        gvEquipment.Visible = true;
                        pnlNoEquipment.Visible = false;
                    }
                    else
                    {
                        pnlNoEquipment.Visible = true;
                        gvEquipment.Visible = false;
                    }
                }
            }
        }

        private void LoadAssignedChemicals()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.spBooking_GetAssignedChemicals", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = BookingID;

                con.Open();
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        gvChemicals.DataSource = dt;
                        gvChemicals.DataBind();
                        gvChemicals.Visible = true;
                        pnlNoChemicals.Visible = false;
                    }
                    else
                    {
                        pnlNoChemicals.Visible = true;
                        gvChemicals.Visible = false;
                    }
                }
            }
        }

        private string GetStatusClass(string status)
        {
            switch (status?.ToLower())
            {
                case "assigned":
                    return "status-assigned";
                case "pending":
                case "for approval":
                    return "status-pending";
                case "completed":
                    return "status-completed";
                default:
                    return "status-pending";
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllBooking.aspx");
        }

        private void ShowError(string message)
        {
            lblMessage.Visible = true;
            lblMessage.Text = "❌ " + message;
            lblMessage.CssClass = "block mt-6 p-4 rounded-lg font-semibold text-center border-l-4 border-red-500 text-red-800 bg-red-100";
        }

        private void ShowSuccess(string message)
        {
            lblMessage.Visible = true;
            lblMessage.Text = "✅ " + message;
            lblMessage.CssClass = "block mt-6 p-4 rounded-lg font-semibold text-center border-l-4 border-green-500 text-green-800 bg-green-100";
        }
    }
}