using System;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;

public class MarkAsRead : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";

        // Ensure inspector session is valid
        if (context.Session["UserID"] == null || context.Session["Role"]?.ToString() != "Inspector")
        {
            context.Response.Write("unauthorized");
            return;
        }

        int inspectorId = Convert.ToInt32(context.Session["UserID"]);

        try
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString))
            {
                string query = "UPDATE Inspections SET IsRead = 1 WHERE InspectorID = @InspectorID AND IsRead = 0";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            context.Response.Write("success");
        }
        catch (Exception)
        {
            context.Response.Write("error");
        }

    }

    public bool IsReusable => false;
}