using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;

public class GetNotifications : IHttpHandler
{
    private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        var uidObj = context.Session?["UserID"];
        var role = context.Session?["Role"] as string;
        if (uidObj == null || !string.Equals(role, "Inspector", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Write("[]");
            return;
        }

        int userId = Convert.ToInt32(uidObj);
        var list = new List<object>();

        using (var conn = new SqlConnection(Cs))
        using (var cmd = new SqlCommand(@"
            SELECT TOP (20)
                NotificationID, Title, Body, Url, CreatedAt, IsRead
            FROM dbo.Notifications
            WHERE UserID = @UserID
            ORDER BY IsRead ASC, CreatedAt DESC;", conn))
        {
            cmd.Parameters.AddWithValue("@UserID", userId);
            conn.Open();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    var created = r.GetDateTime(r.GetOrdinal("CreatedAt"));
                    list.Add(new
                    {
                        id = r.GetInt32(r.GetOrdinal("NotificationID")),
                        title = r["Title"] as string ?? "",
                        body = r["Body"] as string ?? "",
                        url = r["Url"] as string ?? "",
                        date = created.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                        isRead = r.GetBoolean(r.GetOrdinal("IsRead"))
                    });
                }
            }
        }

        var json = new JavaScriptSerializer().Serialize(list);
        context.Response.Write(json);
    }

    public bool IsReusable => false;
}