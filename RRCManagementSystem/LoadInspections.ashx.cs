using System;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace RRCManagementSystem
{
    public class LoadInspections : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            // 🔐 Verify session and role
            if (context.Session["UserID"] == null || context.Session["Role"]?.ToString() != "Inspector")
            {
                context.Response.Write("[]");
                return;
            }

            int inspectorId = Convert.ToInt32(context.Session["UserID"]);
            var events = new List<object>();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString))
                {
                    string query = @"
                        SELECT 
                            InquiryID,
                            InquiryNumber,
                            PestType,
                            InspectionDate,
                            InspectionTime,
                            Status
                        FROM dbo.Inquiries
                        WHERE AssignedInspectorID = @InspectorID
                            AND IsDeleted = 0
                            AND Status IN ('Assigned', 'Inspected', 'In-Progress', 'Completed')
                            AND InspectionDate IS NOT NULL
                        ORDER BY InspectionDate ASC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        // Extract status for color coding
                        string status = reader["Status"].ToString();
                        string bgColor = "#3b82f6"; // Blue (default)

                        switch (status)
                        {
                            case "Assigned": bgColor = "#facc15"; break;   // Yellow
                            case "In-Progress": bgColor = "#fb923c"; break; // Orange
                            case "Inspected": bgColor = "#22d3ee"; break;  // Cyan
                            case "Completed": bgColor = "#22c55e"; break;  // Green
                            case "Cancelled": bgColor = "#ef4444"; break;  // Red
                        }

                        // Convert InspectionDate and InspectionTime to valid ISO datetime
                        DateTime date = Convert.ToDateTime(reader["InspectionDate"]);
                        string inspectionTime = reader["InspectionTime"]?.ToString() ?? "";

                        string startTime = ExtractStartTime(inspectionTime);
                        string endTime = ExtractEndTime(inspectionTime);

                        events.Add(new
                        {
                            id = reader["InquiryID"].ToString(),
                            title = $"{reader["InquiryNumber"]} - {reader["PestType"]}",
                            start = $"{date:yyyy-MM-dd}T{startTime}",
                            end = $"{date:yyyy-MM-dd}T{endTime}",
                            color = bgColor,
                            extendedProps = new
                            {
                                status = status,
                                pestType = reader["PestType"].ToString(),
                                inquiryNumber = reader["InquiryNumber"].ToString()
                            }
                        });
                    }
                }

                JavaScriptSerializer js = new JavaScriptSerializer();
                context.Response.Write(js.Serialize(events));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"error\": \"" + ex.Message.Replace("\"", "\\\"") + "\"}");
            }
        }

        public bool IsReusable => false;

        // Helper to extract start time from "2:00 PM - 5:00 PM"
        private static string ExtractStartTime(string timeRange)
        {
            try
            {
                string[] parts = timeRange.Split('-');
                if (parts.Length > 0)
                {
                    DateTime parsed = DateTime.Parse(parts[0].Trim());
                    return parsed.ToString("HH:mm:ss");
                }
            }
            catch { }
            return "08:00:00"; // Default 8AM
        }

        // Helper to extract end time from "2:00 PM - 5:00 PM"
        private static string ExtractEndTime(string timeRange)
        {
            try
            {
                string[] parts = timeRange.Split('-');
                if (parts.Length > 1)
                {
                    DateTime parsed = DateTime.Parse(parts[1].Trim());
                    return parsed.ToString("HH:mm:ss");
                }
            }
            catch { }
            return "17:00:00"; // Default 5PM
        }
    }
}
