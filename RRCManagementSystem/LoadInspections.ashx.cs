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
                        SELECT i.InspectionID, i.ScheduledDate, i.InspectionStatus, q.InquiryCode,
                               q.FirstName, q.LastName
                        FROM Inspections i
                        INNER JOIN InquirySimple q ON i.InquiryID = q.InquiryID
                        WHERE i.InspectorID = @InspectorID";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        // Determine color based on status
                        string status = reader["InspectionStatus"].ToString();
                        string bgColor = "#3b82f6"; // Default blue
                        if (status == "Pending") bgColor = "#facc15";   // Yellow
                        if (status == "Completed") bgColor = "#22c55e"; // Green
                        if (status == "Cancelled") bgColor = "#ef4444"; // Red

                        events.Add(new
                        {
                            id = reader["InspectionID"].ToString(),
                            title = $"{reader["FirstName"]} {reader["LastName"]} - {status}",
                            start = Convert.ToDateTime(reader["ScheduledDate"]).ToString("yyyy-MM-ddTHH:mm:ss"),
                            color = bgColor,
                            extendedProps = new
                            {
                                inquiryCode = reader["InquiryCode"].ToString(),
                                clientName = $"{reader["FirstName"]} {reader["LastName"]}",
                                status = status
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
                context.Response.Write("{\"error\": \"" + ex.Message + "\"}");
            }
        }

        public bool IsReusable => false;
    }
}
