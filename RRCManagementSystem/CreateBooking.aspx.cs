using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class CreateBooking : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadServices();
            }
            else
            {
                if (hfInquiryVisible.Value == "true")
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "keepVisible",
                        "document.getElementById('inquiryBackground').style.display = 'block';", true);
                }
            }
        }


        protected void ddlServices_PreRender(object sender, EventArgs e)
        {
            if (hfInquiryVisible.Value == "true")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "keepVisible",
                    "document.getElementById('inquiryBackground').style.display = 'block';", true);
            }
        }


        /// <summary>
        /// Load all available services into dropdown
        /// </summary>
        private void LoadServices()
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_GetAvailableServices", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();

                ddlServices.DataSource = cmd.ExecuteReader();
                ddlServices.DataTextField = "Name";      // Display service name
                ddlServices.DataValueField = "ServiceID"; // Store service ID
                ddlServices.DataBind();

                ddlServices.Items.Insert(0, new ListItem("-- Select a Service --", ""));
            }
        }

        /// <summary>
        /// Fetch service's price per SQM
        /// </summary>
        private decimal GetServicePricePerSQM(int serviceId, int sqm)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_GetServicePriceByID", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ServiceID", SqlDbType.Int).Value = serviceId;
                cmd.Parameters.Add("@SQM", SqlDbType.Int).Value = sqm;

                con.Open();
                object result = cmd.ExecuteScalar();
                return (result != null && result != DBNull.Value) ? Convert.ToDecimal(result) : 0;
            }
        }


        /// <summary>
        /// Generate a unique Quotation Code
        /// Format: QUO-YYYYMMDD-0001
        /// </summary>
        private string GenerateQuotationCode()
        {
            string prefix = "QUO-" + DateTime.Now.ToString("yyyyMMdd") + "-";
            int sequence = 1;

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(MAX(CAST(RIGHT(QuotationCode, 4) AS INT)), 0) + 1
                FROM dbo.PendingQuotations
                WHERE QuotationCode LIKE @Prefix + '%'", con))
            {
                cmd.Parameters.AddWithValue("@Prefix", prefix);
                con.Open();
                sequence = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return prefix + sequence.ToString("D4");
        }

        /// <summary>
        /// Recalculate total whenever SQM, travel, misc, or service changes
        /// </summary>
        protected void RecalculateTotal(object sender, EventArgs e)
        {
            if (int.TryParse(ddlServices.SelectedValue, out int serviceId) && serviceId > 0)
            {
                int sqm = 0;
                decimal travel = 0, misc = 0;

                // Parse values from textboxes
                int.TryParse(txtSQM.Text.Trim(), out sqm);
                decimal.TryParse(txtTravelExpense.Text.Trim(), out travel);
                decimal.TryParse(txtMiscellaneous.Text.Trim(), out misc);

                // ✅ Now pass both serviceId and sqm to get the correct tier price
                decimal price = GetServicePricePerSQM(serviceId, sqm);

                // Total is direct price + travel + misc
                decimal total = price + travel + misc;
                txtTotalPrice.Text = total.ToString("F2");
            }
            else
            {
                txtTotalPrice.Text = "0.00";
            }
        }


        /// <summary>
        /// Trigger recalculation when dropdown selection changes
        /// </summary>
        protected void ddlServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Mark as visible in the server-side HiddenField
            hfInquiryVisible.Value = "true";

            // Keep the Inquiry Background div visible
            ScriptManager.RegisterStartupScript(this, GetType(), "showInquiryBackground",
                "document.getElementById('inquiryBackground').style.display = 'block';", true);

            // Recalculate total
            RecalculateTotal(sender, e);
        }



        /// <summary>
        /// Submit the quotation and save to DB
        /// </summary>
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // Must be logged in as Inspector
            if (Session["UserID"] == null || Session["Role"] == null ||
                !string.Equals(Session["Role"].ToString(), "Inspector", StringComparison.OrdinalIgnoreCase))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noInspector",
                    "Swal.fire('Unauthorized', 'You must be logged in as an Inspector.', 'error');", true);
                return;
            }

            // Client validation
            if (string.IsNullOrWhiteSpace(hfClientID.Value) || !int.TryParse(hfClientID.Value, out int clientId))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "selectClient",
                    "Swal.fire('Missing', 'Please select a client.', 'warning');", true);
                return;
            }

            // SQM validation
            if (!int.TryParse(txtSQM.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int sqm))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidSQM",
                    "Swal.fire('Invalid', 'Please enter a valid SQM.', 'error');", true);
                return;
            }

            // Total Price validation
            if (!decimal.TryParse(txtTotalPrice.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal total) || total <= 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidPrice",
                    "Swal.fire('Invalid', 'Please enter a valid total price.', 'error');", true);
                return;
            }

            // Service must be selected
            if (!int.TryParse(ddlServices.SelectedValue, out int serviceId) || serviceId <= 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noServices",
                    "Swal.fire('No Services', 'Please select a service.', 'error');", true);
                return;
            }

            // Get travel and misc amounts
            decimal.TryParse(txtTravelExpense.Text.Trim(), out decimal travel);
            decimal.TryParse(txtMiscellaneous.Text.Trim(), out decimal misc);

            string serviceName = ddlServices.SelectedItem.Text;
            int inspectorId = Convert.ToInt32(Session["UserID"], CultureInfo.InvariantCulture);

            bool isContract = GetIsAnyContract(serviceId.ToString());

            // Insert into PendingQuotations
            string quotationCode;
            int newId = InsertPendingQuotation(
                clientId: clientId,
                inspectorId: inspectorId,
                serviceName: serviceName,
                serviceId: serviceId,
                sqm: sqm,
                price: total,
                travel: travel,
                misc: misc,
                isContract: isContract,
                quotationCode: out quotationCode
            );

            // Add client notification
            try
            {
                var ph = new CultureInfo("en-PH");
                string priceText = string.Format(ph, "{0:C}", total);
                string deepLink = "BookService.aspx?tab=quotes"; // client portal link

                AddNotification(
                    clientId: clientId,
                    type: "quotation",
                    title: "Quotation Submitted",
                    body: $"New quotation ready: {serviceName} — {priceText} for {sqm} sqm.",
                    url: deepLink,
                    dedupKey: $"QUOTE-{clientId}-{newId}"
                );
            }
            catch
            {
                // Notification failure is non-critical
            }

            // Success message
            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                $"Swal.fire('Success', 'Quotation submitted successfully!<br/>Quotation Code: <b>{quotationCode}</b>', 'success');", true);

            // Reset form
            txtClientSearch.Text = "";
            hfClientID.Value = "";
            ddlServices.ClearSelection();
            txtSQM.Text = "";
            txtTravelExpense.Text = "";
            txtMiscellaneous.Text = "";
            txtTotalPrice.Text = "";
        }

        /// <summary>
        /// Inserts a new notification record for the client
        /// </summary>
        private void AddNotification(
            int clientId,
            string type,
            string title,
            string body,
            string url = null,
            string dedupKey = null)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Notifications_Add", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 50).Value = (object)type ?? DBNull.Value;
                cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = (object)title ?? DBNull.Value;
                cmd.Parameters.Add("@Body", SqlDbType.NVarChar, 1000).Value = (object)body ?? DBNull.Value;
                cmd.Parameters.Add("@Url", SqlDbType.NVarChar, 400).Value = (object)url ?? DBNull.Value;
                cmd.Parameters.Add("@DedupKey", SqlDbType.NVarChar, 100).Value = (object)dedupKey ?? DBNull.Value;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Check if the selected service is a contract type
        /// </summary>
        private bool GetIsAnyContract(string serviceIdCsv)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Services_IsAnyContract", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ServiceIDs", SqlDbType.NVarChar, -1).Value = (object)serviceIdCsv ?? DBNull.Value;

                var pOut = cmd.Parameters.Add("@IsContract", SqlDbType.Bit);
                pOut.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();

                return (pOut.Value != DBNull.Value) && Convert.ToBoolean(pOut.Value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Inserts a new Pending Quotation record into the database
        /// </summary>
        private int InsertPendingQuotation(
            int clientId, int inspectorId, string serviceName, int serviceId,
            int sqm, decimal price, decimal travel, decimal misc, bool isContract,
            out string quotationCode)
        {
            quotationCode = GenerateQuotationCode();

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_PendingQuotations_Insert", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;
                cmd.Parameters.Add("@ServiceNames", SqlDbType.NVarChar, 4000).Value = serviceName;
                cmd.Parameters.Add("@ServiceIDCsv", SqlDbType.NVarChar, 4000).Value = serviceId.ToString();
                cmd.Parameters.Add("@SQM", SqlDbType.Int).Value = sqm;
                cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = price;
                cmd.Parameters.Add("@TravelExpense", SqlDbType.Decimal).Value = travel;
                cmd.Parameters.Add("@Miscellaneous", SqlDbType.Decimal).Value = misc;
                cmd.Parameters.Add("@IsContract", SqlDbType.Bit).Value = isContract;
                cmd.Parameters.Add("@QuotationCode", SqlDbType.NVarChar, 50).Value = quotationCode;

                var pOutId = cmd.Parameters.Add("@PendingQuotationID", SqlDbType.Int);
                pOutId.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();

                return (pOutId.Value == DBNull.Value) ? 0 : Convert.ToInt32(pOutId.Value);
            }
        }

        // ===== Ajax AutoComplete for Client Search =====
        [WebMethod]
        [ScriptMethod]
        public static List<string> SearchClients(string prefixText, int count)
        {
            var results = new List<string>();
            string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Clients_Search", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Prefix", SqlDbType.NVarChar, 200).Value =
                    (object)(prefixText ?? string.Empty) ?? DBNull.Value;

                int capped = Math.Max(1, Math.Min(count <= 0 ? 10 : count, 50));
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = capped;

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string fullName = rdr["FullName"]?.ToString() ?? "";
                        string id = rdr["ClientID"]?.ToString() ?? "";
                        results.Add(AjaxControlToolkit.AutoCompleteExtender.CreateAutoCompleteItem(fullName, id));
                    }
                }
            }
            return results;
        }

        /* =========================================================
           PageMethods WebMethod: Fetch InquiryCode + recent Findings
           Called by setClientID() in CreateBooking.aspx
           ========================================================= */
        public class FindingDto
        {
            public string Text { get; set; }
            public string When { get; set; }
        }

        public class InquirySummaryDto
        {
            public string InquiryCode { get; set; }
            public List<FindingDto> Findings { get; set; }
            public string LastUpdated { get; set; }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static InquirySummaryDto GetClientInquirySummary(int clientId)
        {
            var dto = new InquirySummaryDto
            {
                InquiryCode = null,
                Findings = new List<FindingDto>(),
                LastUpdated = null
            };

            string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            using (var con = new SqlConnection(cs))
            {
                con.Open();

                bool chHasInspectionId = ColumnExists(con, "dbo", "ClientHistory", "InspectionID");
                bool chHasInquiryCode = ColumnExists(con, "dbo", "ClientHistory", "InquiryCode");

                bool hasInspectionsTbl = TableExists(con, "dbo", "Inspections");
                bool hasInquiriesTbl = TableExists(con, "dbo", "Inquiries");

                bool insHasInquiryId = hasInspectionsTbl && ColumnExists(con, "dbo", "Inspections", "InquiryID");
                bool iqHasInquiryCode = hasInquiriesTbl && ColumnExists(con, "dbo", "Inquiries", "InquiryCode");

                string sql;

                if (chHasInspectionId && hasInspectionsTbl && hasInquiriesTbl && insHasInquiryId && iqHasInquiryCode)
                {
                    sql = @"
;WITH Recent AS
(
    SELECT TOP (5)
        ch.HistoryID,
        ch.ClientID,
        ch.Title,
        ch.Description,
        ch.CreatedAt,
        COALESCE(ch.InquiryCode, iq.InquiryCode) AS InquiryCode
    FROM dbo.ClientHistory ch
    LEFT JOIN dbo.Inspections ins ON ins.InspectionID = ch.InspectionID
    LEFT JOIN dbo.Inquiries   iq  ON iq.InquiryID     = ins.InquiryID
    WHERE ch.ClientID = @ClientID
      AND (ch.Title = 'Inspection Findings' OR ch.Description IS NOT NULL)
    ORDER BY ch.CreatedAt DESC
)
SELECT * FROM Recent ORDER BY CreatedAt DESC;";
                }
                else if (chHasInquiryCode)
                {
                    sql = @"
SELECT TOP (5)
    ch.HistoryID,
    ch.ClientID,
    ch.Title,
    ch.Description,
    ch.CreatedAt,
    ch.InquiryCode
FROM dbo.ClientHistory ch
WHERE ch.ClientID = @ClientID
  AND (ch.Title = 'Inspection Findings' OR ch.Description IS NOT NULL)
ORDER BY ch.CreatedAt DESC;";
                }
                else
                {
                    sql = @"
SELECT TOP (5)
    ch.HistoryID,
    ch.ClientID,
    ch.Title,
    ch.Description,
    ch.CreatedAt,
    CAST(NULL AS NVARCHAR(50)) AS InquiryCode
FROM dbo.ClientHistory ch
WHERE ch.ClientID = @ClientID
  AND (ch.Title = 'Inspection Findings' OR ch.Description IS NOT NULL)
ORDER BY ch.CreatedAt DESC;";
                }

                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                    using (var rdr = cmd.ExecuteReader())
                    {
                        DateTime? latest = null;

                        while (rdr.Read())
                        {
                            if (string.IsNullOrEmpty(dto.InquiryCode))
                                dto.InquiryCode = rdr["InquiryCode"] as string;

                            string desc = rdr["Description"] as string;
                            string title = rdr["Title"] as string;

                            DateTime? createdAt = rdr["CreatedAt"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(rdr["CreatedAt"], CultureInfo.InvariantCulture);

                            string text = !string.IsNullOrWhiteSpace(desc)
                                ? desc
                                : (!string.IsNullOrWhiteSpace(title) ? title : "(no description)");

                            dto.Findings.Add(new FindingDto
                            {
                                Text = text,
                                When = createdAt.HasValue
                                    ? createdAt.Value.ToString("MMM dd, yyyy h:mm tt", CultureInfo.InvariantCulture)
                                    : null
                            });

                            if (createdAt.HasValue && (!latest.HasValue || createdAt > latest))
                                latest = createdAt;
                        }

                        if (latest.HasValue)
                            dto.LastUpdated = latest.Value.ToString("MMM dd, yyyy h:mm tt", CultureInfo.InvariantCulture);
                    }
                }
            }

            return dto;
        }

        // Helpers
        private static bool ColumnExists(SqlConnection con, string schema, string table, string column)
        {
            using (var cmd = new SqlCommand(@"
SELECT 1
FROM sys.columns c
JOIN sys.objects o  ON o.object_id = c.object_id
JOIN sys.schemas s  ON s.schema_id = o.schema_id
WHERE s.name = @s AND o.name = @t AND c.name = @c;", con))
            {
                cmd.Parameters.AddWithValue("@s", schema);
                cmd.Parameters.AddWithValue("@t", table);
                cmd.Parameters.AddWithValue("@c", column);
                return cmd.ExecuteScalar() != null;
            }
        }

        private static bool TableExists(SqlConnection con, string schema, string table)
        {
            using (var cmd = new SqlCommand(@"
SELECT 1
FROM sys.objects o
JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE s.name = @s AND o.name = @t AND o.type IN ('U','V');", con))
            {
                cmd.Parameters.AddWithValue("@s", schema);
                cmd.Parameters.AddWithValue("@t", table);
                return cmd.ExecuteScalar() != null;
            }
        }
    }
}
