using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace RRCManagementSystem
{
    /// <summary>
    /// Handler to load team bookings for FullCalendar
    /// </summary>
    public class LoadTeamBookings : IHttpHandler, IRequiresSessionState
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            // Verify session
            if (context.Session["UserID"] == null || context.Session["Role"]?.ToString() != "Headtechnician")
            {
                context.Response.Write("[]");
                return;
            }

            int teamLeaderID = Convert.ToInt32(context.Session["UserID"]);

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spTeamLeader_GetCalendarBookings", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TeamLeaderID", SqlDbType.Int).Value = teamLeaderID;

                    con.Open();
                    var events = new StringBuilder();
                    events.Append("[");

                    using (var reader = cmd.ExecuteReader())
                    {
                        bool first = true;
                        while (reader.Read())
                        {
                            if (!first) events.Append(",");
                            first = false;

                            int bookingID = Convert.ToInt32(reader["BookingID"]);
                            string bookingCode = reader["BookingCode"].ToString();
                            string clientName = reader["ClientName"].ToString();
                            DateTime scheduledDate = Convert.ToDateTime(reader["ScheduledDate"]);
                            TimeSpan startTime = reader["StartTime"] != DBNull.Value
                                ? (TimeSpan)reader["StartTime"]
                                : new TimeSpan(8, 0, 0);
                            string status = reader["Status"].ToString();
                            string services = reader["ServiceNames"].ToString();

                            // Combine date and time
                            DateTime fullDateTime = scheduledDate.Date.Add(startTime);

                            // Determine color based on status
                            string color = status == "Completed" ? "#10b981"
                                         : status == "In Progress" ? "#f59e0b"
                                         : "#2563eb";

                            events.Append($@"{{
                                ""id"": ""{bookingID}"",
                                ""title"": ""{EscapeJson(bookingCode)} - {EscapeJson(clientName)}"",
                                ""start"": ""{fullDateTime:yyyy-MM-ddTHH:mm:ss}"",
                                ""color"": ""{color}"",
                                ""extendedProps"": {{
                                    ""status"": ""{EscapeJson(status)}"",
                                    ""services"": ""{EscapeJson(services)}""
                                }}
                            }}");
                        }
                    }

                    events.Append("]");
                    context.Response.Write(events.ToString());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTeamBookings error: {ex.Message}");
                context.Response.Write("[]");
            }
        }

        private string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t");
        }

        public bool IsReusable => false;
    }
}