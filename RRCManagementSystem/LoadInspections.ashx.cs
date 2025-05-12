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

            int inspectorId;
            if (context.Session["UserID"] == null || !int.TryParse(context.Session["UserID"].ToString(), out inspectorId))
            {
                context.Response.Write("[]");
                return;
            }

            List<object> events = new List<object>();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString))
                {
                    string query = "SELECT InspectionID, ScheduledDate, InquiryID FROM Inspections WHERE InspectorID = @InspectorID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        events.Add(new
                        {
                            id = reader["InspectionID"].ToString(),
                            title = "Inspection",
                            start = Convert.ToDateTime(reader["ScheduledDate"]).ToString("yyyy-MM-dd"),
                            extendedProps = new
                            {
                                inquiryId = reader["InquiryID"].ToString()
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