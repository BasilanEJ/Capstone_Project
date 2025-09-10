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
        }

        private void LoadServices()
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_GetAvailableServices", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();

                using (var rdr = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(rdr);

                    // Split data by ServiceType
                    var termiteRows = dt.Select("ServiceType = 'Termite Control'");
                    var generalRows = dt.Select("ServiceType = 'General Pest Control'");

                    if (termiteRows.Length > 0)
                    {
                        cblTermite.DataSource = termiteRows.CopyToDataTable();
                        cblTermite.DataBind();
                    }

                    if (generalRows.Length > 0)
                    {
                        cblGeneral.DataSource = generalRows.CopyToDataTable();
                        cblGeneral.DataBind();
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // ✅ must be logged in AND Inspector
            if (Session["UserID"] == null || Session["Role"] == null ||
                !string.Equals(Session["Role"].ToString(), "Inspector", StringComparison.OrdinalIgnoreCase))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noInspector",
                    "Swal.fire('Unauthorized', 'You must be logged in as an Inspector.', 'error');", true);
                return;
            }

            // ✅ Client must be selected
            if (string.IsNullOrWhiteSpace(hfClientID.Value) || !int.TryParse(hfClientID.Value, out int clientId))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "selectClient",
                    "Swal.fire('Missing', 'Please select a client.', 'warning');", true);
                return;
            }

            // ✅ SQM validation
            if (!int.TryParse(txtSQM.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int sqm))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidSQM",
                    "Swal.fire('Invalid', 'Please enter a valid SQM.', 'error');", true);
                return;
            }

            // ✅ Total Price validation
            if (!decimal.TryParse(txtTotalPrice.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal total) || total <= 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidPrice",
                    "Swal.fire('Invalid', 'Please enter a valid total price.', 'error');", true);
                return;
            }

            // ✅ Must select at least one service
            var selectedItems = cblTermite.Items.Cast<ListItem>().Where(i => i.Selected)
                .Concat(cblGeneral.Items.Cast<ListItem>().Where(i => i.Selected))
                .ToList();

            if (!selectedItems.Any())
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noServices",
                    "Swal.fire('No Services', 'Please select at least one service.', 'error');", true);
                return;
            }

            string selectedServiceIDs = string.Join(",", selectedItems.Select(i => i.Value));
            string selectedServiceNames = string.Join(", ", selectedItems.Select(i => i.Text));
            int inspectorId = Convert.ToInt32(Session["UserID"], CultureInfo.InvariantCulture);

            bool isContract = GetIsAnyContract(selectedServiceIDs);

            // ✅ Insert pending quotation
            int newId = InsertPendingQuotation(
                clientId: clientId,
                inspectorId: inspectorId,
                serviceNames: selectedServiceNames,
                serviceIdCsv: selectedServiceIDs,
                sqm: sqm,
                price: total,
                isContract: isContract
            );

            // 🔔 Add notification for the client (Quotation Submitted)
            try
            {
                string svc = selectedServiceNames;
                if (svc.Length > 60) svc = svc.Substring(0, 57) + "...";

                var ph = new CultureInfo("en-PH");
                string priceText = string.Format(ph, "{0:C}", total); // ₱1,000.00 style

                string deepLink = "BookService.aspx?tab=quotes"; // page where client can review quotations

                AddNotification(
                    clientId: clientId,
                    type: "quotation",
                    title: "Quotation Submitted",
                    body: $"New quotation ready: {svc} — {priceText} for {sqm} sqm.",
                    url: deepLink,
                    dedupKey: $"QUOTE-{clientId}-{newId}"
                );
            }
            catch
            {
                // Notification failure is non-critical
            }

            // ✅ Success UI
            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "Swal.fire('Success', 'Quotation submitted for client!', 'success');", true);

            // ✅ Reset fields
            txtClientSearch.Text = "";
            hfClientID.Value = "";
            cblTermite.ClearSelection();
            cblGeneral.ClearSelection();
            txtSQM.Text = "";
            txtTotalPrice.Text = "";
        }

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

        private int InsertPendingQuotation(int clientId, int inspectorId, string serviceNames, string serviceIdCsv, int sqm, decimal price, bool isContract)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_PendingQuotations_Insert", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;
                cmd.Parameters.Add("@ServiceNames", SqlDbType.NVarChar, 4000).Value = (object)serviceNames ?? DBNull.Value;
                cmd.Parameters.Add("@ServiceIDCsv", SqlDbType.NVarChar, 4000).Value = (object)serviceIdCsv ?? DBNull.Value;
                cmd.Parameters.Add("@SQM", SqlDbType.Int).Value = sqm;

                var pPrice = cmd.Parameters.Add("@Price", SqlDbType.Decimal);
                pPrice.Precision = 18; pPrice.Scale = 2; pPrice.Value = price;

                cmd.Parameters.Add("@IsContract", SqlDbType.Bit).Value = isContract;

                var pOutId = cmd.Parameters.Add("@PendingQuotationID", SqlDbType.Int);
                pOutId.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();

                return (pOutId.Value == DBNull.Value) ? 0 : Convert.ToInt32(pOutId.Value, CultureInfo.InvariantCulture);
            }
        }

        // ===== Ajax AutoComplete endpoint (stored procedure) =====
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

                // --- Schema probes (no hard-coded assumptions) ---
                bool chHasInspectionId = ColumnExists(con, "dbo", "ClientHistory", "InspectionID");
                bool chHasInquiryCode = ColumnExists(con, "dbo", "ClientHistory", "InquiryCode");

                bool hasInspectionsTbl = TableExists(con, "dbo", "Inspections");
                bool hasInquiriesTbl = TableExists(con, "dbo", "Inquiries");

                bool insHasInquiryId = hasInspectionsTbl && ColumnExists(con, "dbo", "Inspections", "InquiryID");
                bool iqHasInquiryCode = hasInquiriesTbl && ColumnExists(con, "dbo", "Inquiries", "InquiryCode");

                string sql;

                if (chHasInspectionId && hasInspectionsTbl && hasInquiriesTbl && insHasInquiryId && iqHasInquiryCode)
                {
                    // ✅ Full path: ClientHistory.InspectionID -> Inspections -> Inquiries.InquiryCode
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
                    // ✅ Fallback: just use InquiryCode already stored in ClientHistory
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
                    // ✅ Minimal fallback: no inquiry code available from schema
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

        // --- helpers (put in the same code-behind class) ---
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
