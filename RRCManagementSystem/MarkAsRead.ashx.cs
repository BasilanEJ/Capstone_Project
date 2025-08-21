using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

public class MarkAsRead : IHttpHandler
{
    private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        var uidObj = context.Session?["UserID"];
        var role = context.Session?["Role"] as string;
        if (uidObj == null || !string.Equals(role, "Inspector", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Write("{\"ok\":false}");
            return;
        }

        int userId = Convert.ToInt32(uidObj);
        using (var conn = new SqlConnection(Cs))
        using (var cmd = new SqlCommand(@"
            UPDATE dbo.Notifications
            SET IsRead = 1, Status = 'Read'
            WHERE UserID = @UserID AND IsRead = 0;", conn))
        {
            cmd.Parameters.AddWithValue("@UserID", userId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        context.Response.Write("{\"ok\":true}");
    }

    public bool IsReusable => false;
}