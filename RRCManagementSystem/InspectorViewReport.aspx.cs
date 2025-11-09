using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class InspectorViewReport : System.Web.UI.Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Inspector")
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                int inquiryId = GetQueryInt("id");
                int reportId = GetQueryInt("reportId");

                if (inquiryId == 0 || reportId == 0)
                {
                    Response.Redirect("MyInspections.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                LoadClientInquiryInfo(inquiryId);
                LoadReport(reportId);
            }
        }

        private int GetQueryInt(string key)
        {
            return int.TryParse(Request.QueryString[key], out int val) ? val : 0;
        }

        private void LoadClientInquiryInfo(int inquiryId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT 
                    i.InquiryNumber, i.PestType,
                    i.AddressEnc, i.BarangayEnc, i.CityEnc, i.RegionEnc, i.LandmarkEnc,
                    c.FirstName, c.MiddleName, c.LastName, c.ContactEnc
                FROM dbo.Inquiries i
                INNER JOIN dbo.Clients c ON i.ClientID = c.ClientID
                WHERE i.InquiryID = @InquiryID", conn))
            {
                cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                conn.Open();

                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        lblInquiryNumber.InnerText = r["InquiryNumber"].ToString();
                        lblPestType.InnerText = r["PestType"].ToString();

                        lblClientName.InnerText = $"{r["FirstName"]} {r["MiddleName"]} {r["LastName"]}".Trim();
                        lblClientContact.InnerText = AESHelper.DecryptField(
                            r["ContactEnc"] == DBNull.Value ? string.Empty : r["ContactEnc"].ToString()
                        );

                        string addr =
                            $"{AESHelper.DecryptField(r["AddressEnc"] == DBNull.Value ? string.Empty : r["AddressEnc"].ToString())} " +
                            $"{AESHelper.DecryptField(r["BarangayEnc"] == DBNull.Value ? string.Empty : r["BarangayEnc"].ToString())}, " +
                            $"{AESHelper.DecryptField(r["CityEnc"] == DBNull.Value ? string.Empty : r["CityEnc"].ToString())}, " +
                            $"{AESHelper.DecryptField(r["RegionEnc"] == DBNull.Value ? string.Empty : r["RegionEnc"].ToString())}";

                        if (r["LandmarkEnc"] != DBNull.Value)
                        {
                            addr += $" (Near: {AESHelper.DecryptField(r["LandmarkEnc"].ToString())})";
                        }

                        lblClientAddress.InnerText = addr;
                    }
                }
            }
        }

        private void LoadReport(int reportId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT * FROM dbo.InspectionReports WHERE ReportID = @ReportID", conn))
            {
                cmd.Parameters.AddWithValue("@ReportID", reportId);
                conn.Open();

                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        lblQuotationCode.InnerText = r["QuotationCode"].ToString();
                        lblInfestationLevel.InnerText = r["InfestationLevel"].ToString();
                        lblFindings.InnerText = r["FindingsDescription"].ToString();
                        lblAdditionalNotes.InnerText = r["AdditionalNotes"].ToString();
                        lblFollowupRequired.InnerText = (Convert.ToBoolean(r["FollowupRequired"])) ? "Yes" : "No";
                        lblFollowupDate.InnerText = r["FollowupDate"] == DBNull.Value
                            ? "—"
                            : Convert.ToDateTime(r["FollowupDate"]).ToString("MMMM dd, yyyy");
                        lblFollowupReason.InnerText = r["FollowupReason"]?.ToString();

                        decimal travel = r["TravelCost"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TravelCost"]);
                        lblTravelCost.InnerText = $"₱{travel:N2}";

                        decimal total = r["TotalEstimatedCost"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalEstimatedCost"]);
                        lblGrandTotal.InnerText = $"₱{total:N2}";

                        lblMiscExpenses.InnerText = ParseMisc(r["MiscellaneousExpenses"]?.ToString());

                        string servicesJson = r["SelectedServices"]?.ToString() ?? "[]";
                        BindServices(servicesJson);

                        if (r["InspectionPhotosPath"] != DBNull.Value)
                        {
                            RenderImages(r["InspectionPhotosPath"].ToString());
                        }
                    }
                }
            }
        }

        private void BindServices(string json)
        {
            var serializer = new JavaScriptSerializer();
            var list = serializer.Deserialize<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>>(json);

            var dt = new DataTable();
            dt.Columns.Add("ServiceName", typeof(string));
            dt.Columns.Add("SQM", typeof(decimal));
            dt.Columns.Add("FlatPrice", typeof(decimal));
            dt.Columns.Add("IsContract", typeof(bool));

            foreach (var s in list)
            {
                var row = dt.NewRow();
                row["ServiceName"] = s.ContainsKey("ServiceName") ? s["ServiceName"].ToString() : "Unknown Service";
                row["SQM"] = s.ContainsKey("SQM") ? Convert.ToDecimal(s["SQM"]) : 0;
                row["FlatPrice"] = s.ContainsKey("FlatPrice") ? Convert.ToDecimal(s["FlatPrice"]) : 0;
                row["IsContract"] = s.ContainsKey("IsContract") && Convert.ToBoolean(s["IsContract"]);
                dt.Rows.Add(row);
            }

            rptServices.DataSource = dt;
            rptServices.DataBind();
        }

        private string ParseMisc(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "[]")
                return "—";

            try
            {
                var serializer = new JavaScriptSerializer();
                var list = serializer.Deserialize<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>>(json);

                decimal total = 0;
                foreach (var e in list)
                {
                    if (e.ContainsKey("amount"))
                        total += Convert.ToDecimal(e["amount"]);
                }

                return $"₱{total:N2}";
            }
            catch
            {
                return "—";
            }
        }

        private void RenderImages(string paths)
        {
            var imgList = paths.Split(',');
            string html = "";

            foreach (var p in imgList)
            {
                string path = p.Trim().Replace("~", "");
                html += $"<img src='{path}' onclick='window.open(\"{path}\", \"_blank\")' />";
            }

            photoContainer.InnerHtml = html;
        }
    }
}
