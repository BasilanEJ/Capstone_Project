using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;

namespace RRCManagementSystem
{
    public class Anchor : IHttpHandler
    {
        private static readonly string Cs =
            ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        public void ProcessRequest(HttpContext ctx)
        {
            ctx.Response.ContentType = "text/plain";

            var configuredKey = ConfigurationManager.AppSettings["AnchorJobKey"] ?? "";
            var suppliedKey = ctx.Request.QueryString["key"] ?? "";
            bool authorized = ctx.Request.IsLocal ||
                              (!string.IsNullOrWhiteSpace(configuredKey) && string.Equals(configuredKey, suppliedKey));
            if (!authorized)
            {
                ctx.Response.StatusCode = 401;
                ctx.Response.Write("Unauthorized. Provide a valid ?key= or call locally.");
                return;
            }

            string dateParam = ctx.Request.QueryString["date"];
            DateTime utcNow = DateTime.UtcNow;
            DateTime anchorDateUtc;

            if (string.IsNullOrWhiteSpace(dateParam) || dateParam.Equals("yesterday", StringComparison.OrdinalIgnoreCase))
                anchorDateUtc = utcNow.Date.AddDays(-1);
            else if (dateParam.Equals("today", StringComparison.OrdinalIgnoreCase))
                anchorDateUtc = utcNow.Date;
            else if (!DateTime.TryParseExact(dateParam, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                             DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                             out anchorDateUtc))
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.Write("Invalid ?date=. Use yyyy-MM-dd, 'today', or 'yesterday'.");
                return;
            }

            try
            {
                using (var con = new SqlConnection(Cs))
                {
                    con.Open();

                    using (var check = con.CreateCommand())
                    {
                        check.CommandText = @"
IF EXISTS (SELECT 1 FROM dbo.BlockchainAnchors WHERE AnchorDate = @d) 
    SELECT 1 ELSE SELECT 0;";
                        check.Parameters.Add("@d", SqlDbType.Date).Value = anchorDateUtc.Date;
                        bool exists = Convert.ToInt32(check.ExecuteScalar()) == 1;

                        if (exists)
                        {
                            ctx.Response.Write($"Anchor already exists for {anchorDateUtc:yyyy-MM-dd}.");
                            return;
                        }
                    }

                    using (var get = con.CreateCommand())
                    {
                        get.CommandText = @"
SELECT TOP 1 
       LogID,
       COALESCE(ChainHash, SaleHash) AS FinalHash
FROM dbo.BlockchainSalesLog
WHERE CONVERT(date, [Timestamp]) = @d
ORDER BY LogID DESC;";
                        get.Parameters.Add("@d", SqlDbType.Date).Value = anchorDateUtc.Date;

                        using (var rd = get.ExecuteReader())
                        {
                            if (!rd.Read())
                            {
                                ctx.Response.Write($"No blockchain rows for {anchorDateUtc:yyyy-MM-dd}; nothing to anchor.");
                                return;
                            }

                            int lastLogId = rd.GetInt32(0);
                            string chainHash = rd.GetString(1);

                            using (var ins = con.CreateCommand())
                            {
                                ins.CommandText = @"
INSERT INTO dbo.BlockchainAnchors (AnchorDate, LastLogID, ChainHash)
VALUES (@d, @last, @hash);";
                                ins.Parameters.Add("@d", SqlDbType.Date).Value = anchorDateUtc.Date;
                                ins.Parameters.Add("@last", SqlDbType.Int).Value = lastLogId;
                                ins.Parameters.Add("@hash", SqlDbType.Char, 64).Value = chainHash;

                                int rows = ins.ExecuteNonQuery();
                                ctx.Response.Write(rows == 1
                                    ? $"Anchor written for {anchorDateUtc:yyyy-MM-dd}: LogID={lastLogId}, Hash={chainHash}"
                                    : "No anchor written.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.Write("Error: " + ex.Message);
            }
        }

        public bool IsReusable => false;
    }
}