using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace RRCManagementSystem.Helpers
{
    public static class BlockchainLogger
    {
        // Canonical Json.NET settings (stable, compact, include nulls, UTC dates if any)
        private static readonly JsonSerializerSettings CanonJsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Include,
            StringEscapeHandling = StringEscapeHandling.Default,
            Culture = System.Globalization.CultureInfo.InvariantCulture,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
            // PropertyNamingPolicy equivalent NOT used: Json.NET preserves your property names by default
        };

        private static string Canonicalize(object payload)
        {
            // NOTE: Json.NET preserves property order from the object graph.
            // Anonymous object member order is deterministic based on compile order.
            return JsonConvert.SerializeObject(payload, CanonJsonSettings);
        }

        private static string ToLowerHex(byte[] data)
        {
            var sb = new StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("x2")); // lower hex
            return sb.ToString();
        }

        private static string Sha256Utf8(string s)
        {
            if (s == null) s = string.Empty;
            var bytes = Encoding.UTF8.GetBytes(s);
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(bytes);
                return ToLowerHex(hash);
            }
        }

        /// <summary>
        /// Append a sale log entry and maintain PrevHash/ChainHash linking.
        /// Tables:
        ///   dbo.BlockchainSalesLog(LogID PK, TransactionID, SaleDataJson, SaleHash, PrevHash, ChainHash, Timestamp)
        ///   dbo.BlockchainAnchors(AnchorDate PK, LastLogID, ChainHash, CreatedAt)
        /// </summary>
        public static void AppendSaleLog(string cs, int transactionId, object payload)
        {
            // 0) Canonical JSON + sale hash (UTF-8 -> SHA256 -> lowercase hex)
            string json = Canonicalize(payload);
            string saleHash = Sha256Utf8(json);

            using (var con = new SqlConnection(cs))
            {
                con.Open();

                // 1) Get previous chain hash (last row)
                string prevHash = null;
                using (var getPrev = new SqlCommand(
                    "SELECT TOP 1 ChainHash FROM dbo.BlockchainSalesLog ORDER BY LogID DESC", con))
                using (var r = getPrev.ExecuteReader())
                {
                    if (r.Read())
                        prevHash = r.IsDBNull(0) ? null : r.GetString(0);
                }

                // 2) Chain = SHA256( (prevChain ?? saleHash) + "." + saleHash )
                string material = (string.IsNullOrEmpty(prevHash) ? saleHash : (prevHash + "." + saleHash));
                string chainHash = Sha256Utf8(material);

                // IMPORTANT: keep PrevHash non-NULL on first row to satisfy CHECK constraint
                string prevToStore = string.IsNullOrEmpty(prevHash) ? saleHash : prevHash;

                // 3) Insert new row
                int newLogId;
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.BlockchainSalesLog
    (TransactionID, SaleDataJson, SaleHash, PrevHash, ChainHash, Timestamp)
OUTPUT INSERTED.LogID
VALUES
    (@tx, @json, @saleHash, @prev, @chain, SYSUTCDATETIME());", con))
                {
                    cmd.Parameters.Add("@tx", SqlDbType.Int).Value = transactionId;

                    // Use NVARCHAR(MAX) for JSON to avoid truncation
                    var pJson = cmd.Parameters.Add("@json", SqlDbType.NVarChar, -1);
                    pJson.Value = json;

                    cmd.Parameters.Add("@saleHash", SqlDbType.Char, 64).Value = saleHash;
                    cmd.Parameters.Add("@prev", SqlDbType.Char, 64).Value = prevToStore;  // never NULL
                    cmd.Parameters.Add("@chain", SqlDbType.Char, 64).Value = chainHash;

                    newLogId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 4) Upsert today’s anchor with latest chain hash
                using (var anchor = new SqlCommand(@"
MERGE dbo.BlockchainAnchors AS t
USING (SELECT CONVERT(date, SYSUTCDATETIME()) AS AnchorDate) AS s
ON (t.AnchorDate = s.AnchorDate)
WHEN MATCHED THEN
  UPDATE SET LastLogID = @logId, ChainHash = @chain, CreatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
  INSERT(AnchorDate, LastLogID, ChainHash, CreatedAt)
  VALUES(s.AnchorDate, @logId, @chain, SYSUTCDATETIME());", con))
                {
                    anchor.Parameters.Add("@logId", SqlDbType.Int).Value = newLogId;
                    anchor.Parameters.Add("@chain", SqlDbType.Char, 64).Value = chainHash;
                    anchor.ExecuteNonQuery();
                }
            }
        }

    }
}
