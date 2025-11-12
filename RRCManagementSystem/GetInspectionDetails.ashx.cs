using System;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web.SessionState;
using RRCManagementSystem.Helpers; // ✅ For AESHelper

namespace RRCManagementSystem
{
    public class GetInspectionDetails : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            try
            {
                // ✅ Ensure inspector is logged in
                if (context.Session["UserID"] == null || context.Session["Role"]?.ToString() != "Inspector")
                {
                    context.Response.Write("<p class='text-red-500'>Unauthorized access.</p>");
                    return;
                }

                // ✅ Get the InquiryID from query string (from calendar event)
                string idStr = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out int inquiryId))
                {
                    context.Response.Write("<p class='text-red-500'>Invalid or missing inspection ID.</p>");
                    return;
                }

                int inspectorId = Convert.ToInt32(context.Session["UserID"]);
                StringBuilder sb = new StringBuilder();

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString))
                {
                    string query = @"
                        SELECT 
                            i.InquiryID,
                            i.InquiryNumber,
                            i.PestType,
                            i.ProblemDescription,
                            i.Status,
                            i.InspectionDate,
                            i.InspectionTime,
                            i.RegionEnc,
                            i.CityEnc,
                            i.BarangayEnc,
                            i.AddressEnc,
                            i.LandmarkEnc,
                            c.FirstName,
                            c.MiddleName,
                            c.LastName,
                            c.EmailEnc,
                            c.ContactEnc
                        FROM dbo.Inquiries AS i
                        LEFT JOIN dbo.Clients AS c ON i.ClientID = c.ClientID
                        WHERE i.InquiryID = @InquiryID
                          AND i.AssignedInspectorID = @InspectorID
                          AND i.IsDeleted = 0";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@InquiryID", inquiryId);
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string status = HttpUtility.HtmlEncode(reader["Status"].ToString());
                        string statusColor = "bg-blue-500"; // default

                        switch (status)
                        {
                            case "Assigned": statusColor = "bg-yellow-500"; break;
                            case "In-Progress": statusColor = "bg-orange-500"; break;
                            case "Inspected": statusColor = "bg-cyan-500"; break;
                            case "Completed": statusColor = "bg-green-500"; break;
                            case "Cancelled": statusColor = "bg-red-500"; break;
                        }

                        // 🔐 Decrypt helper
                        string Decrypt(string val) => string.IsNullOrEmpty(val) ? "" : AESHelper.DecryptField(val);

                        string clientName = $"{reader["FirstName"]} {reader["MiddleName"]} {reader["LastName"]}".Trim();
                        string contact = Decrypt(reader["ContactEnc"]?.ToString());
                        string email = AESHelper.DecryptEmail(reader["EmailEnc"]?.ToString());

                        // ✅ Build detailed HTML
                        sb.Append("<div class='p-4 space-y-3 text-gray-700'>");

                        sb.AppendFormat(@"
                            <div class='flex items-center space-x-3 mb-3'>
                                <div class='w-4 h-4 rounded-full {0}'></div>
                                <h4 class='text-lg font-semibold text-gray-900'>
                                    {1} - {2}
                                </h4>
                            </div>", statusColor, reader["InquiryNumber"], reader["PestType"]);

                        sb.AppendFormat("<p><strong>Status:</strong> {0}</p>", status);
                        sb.AppendFormat("<p><strong>Inspection Date:</strong> {0:MMMM dd, yyyy}</p>", reader["InspectionDate"]);
                        sb.AppendFormat("<p><strong>Inspection Time:</strong> {0}</p>", reader["InspectionTime"]);

                        sb.Append("<hr class='my-2'/>");

                        sb.AppendFormat("<p><strong>Client:</strong> {0}</p>", clientName);
                        sb.AppendFormat("<p><strong>Contact:</strong> {0}</p>", string.IsNullOrEmpty(contact) ? "N/A" : contact);
                        sb.AppendFormat("<p><strong>Email:</strong> {0}</p>", string.IsNullOrEmpty(email) ? "N/A" : email);

                        sb.Append("<hr class='my-2'/>");

                        sb.AppendFormat("<p><strong>Problem Description:</strong><br/>{0}</p>",
                            HttpUtility.HtmlEncode(reader["ProblemDescription"].ToString()));

                        sb.Append("<hr class='my-2'/>");

                        sb.AppendFormat("<p><strong>Address:</strong> {0}, {1}, {2}, {3}</p>",
                            Decrypt(reader["AddressEnc"]?.ToString()),
                            Decrypt(reader["BarangayEnc"]?.ToString()),
                            Decrypt(reader["CityEnc"]?.ToString()),
                            Decrypt(reader["RegionEnc"]?.ToString()));

                        sb.AppendFormat("<p><strong>Landmark:</strong> {0}</p>", Decrypt(reader["LandmarkEnc"]?.ToString()));

                        sb.Append("</div>");
                    }
                    else
                    {
                        sb.Append("<p class='text-gray-500 text-center py-6'>No details found for this inspection.</p>");
                    }
                }

                context.Response.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("<p class='text-red-500'>Error loading inspection details: "
                                       + HttpUtility.HtmlEncode(ex.Message) + "</p>");
            }
        }

        public bool IsReusable => false;
    }
}
