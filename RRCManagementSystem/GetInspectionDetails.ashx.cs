using System;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web.SessionState;

namespace RRCManagementSystem // ✅ Match the Class attribute in .ashx
{
    public class GetInspectionDetails : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            try
            {
                if (context.Session["UserID"] == null || context.Session["Role"]?.ToString() != "Inspector")
                {
                    context.Response.Write("<p>Unauthorized access.</p>");
                    return;
                }

                string dateStr = context.Request.QueryString["date"];
                if (string.IsNullOrEmpty(dateStr) || !DateTime.TryParse(dateStr, out DateTime selectedDate))
                {
                    context.Response.Write("<p>Invalid or missing date.</p>");
                    return;
                }

                int inspectorId = Convert.ToInt32(context.Session["UserID"]);
                StringBuilder sb = new StringBuilder();

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString))
                {
                    string query = @"
                        SELECT InspectionID, InquiryID, ScheduledDate, InspectionStatus
                        FROM Inspections
                        WHERE InspectorID = @InspectorID AND CAST(ScheduledDate AS DATE) = @Date";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    cmd.Parameters.AddWithValue("@Date", selectedDate.Date);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        sb.AppendFormat(@"
                            <div style='padding:10px 0; border-bottom:1px solid #eee;'>
                                <strong>Inspection #{0}</strong><br/>
                                Inquiry ID: {1}<br/>
                                Time: {2}<br/>
                                Status: {3}
                            </div>",
                            reader["InspectionID"],
                            reader["InquiryID"],
                            Convert.ToDateTime(reader["ScheduledDate"]).ToString("hh:mm tt"),
                            reader["InspectionStatus"]
                        );
                    }
                }

                if (sb.Length == 0)
                {
                    sb.Append("<p>No inspections scheduled for this day.</p>");
                }

                context.Response.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("<p>Error loading inspection details: " + ex.Message + "</p>");
            }
        }

        public bool IsReusable => false;
    }
}