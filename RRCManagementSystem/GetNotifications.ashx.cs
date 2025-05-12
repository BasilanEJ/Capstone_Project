using System;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Web.SessionState;

public class GetNotifications : IHttpHandler, IRequiresSessionState
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        if (context.Session["UserID"] == null || context.Session["Role"]?.ToString() != "Inspector")
        {
            context.Response.Write("{\"count\":0,\"items\":[]}");
            return;
        }

        int inspectorId = Convert.ToInt32(context.Session["UserID"]);
        var notifications = new List<object>();

        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString))
        {
            string query = @"
        SELECT InspectionID, ScheduledDate
        FROM Inspections
        WHERE InspectorID = @InspectorID AND IsRead = 0 AND InspectionStatus = 'Pending'
        ORDER BY ScheduledDate DESC";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
            conn.Open();

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                notifications.Add(new
                {
                    id = reader["InspectionID"].ToString(),
                    date = Convert.ToDateTime(reader["ScheduledDate"]).ToString("MMM dd yyyy hh:mm tt")
                });
            }
        }


        context.Response.Write(new JavaScriptSerializer().Serialize(new
        {
            count = notifications.Count,
            items = notifications
        }));
    }

    public bool IsReusable => false;
}