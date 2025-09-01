using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;

namespace RRCManagementSystem
{
    public class MarkAsRead : IHttpHandler, IRequiresSessionState
    {
        private static readonly string Cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            var uidObj = context.Session?["UserID"];
            var role = context.Session?["Role"] as string;

            if (uidObj == null || !string.Equals(role, "Inspector", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 204;
                return;
            }

            int userId = Convert.ToInt32(uidObj);

            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(
                "UPDATE dbo.Notifications SET IsRead=1, Status='Read' WHERE UserID=@U AND IsRead=0;", conn))
            {
                cmd.Parameters.AddWithValue("@U", userId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            context.Response.StatusCode = 204; // No Content
        }

        public bool IsReusable => false;
    }
}
