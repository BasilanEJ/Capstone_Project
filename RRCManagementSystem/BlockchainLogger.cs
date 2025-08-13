using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RRCManagementSystem.Helpers
{
    public static class BlockchainLogger
    {
        /// <summary>
        /// Serialize with stable key order and no whitespace so hashes are consistent.
        /// </summary>
        public static string ToDeterministicJson(object obj)
        {
            var j = JObject.FromObject(obj);
            var ordered = new JObject(j.Properties().OrderBy(p => p.Name));
            return ordered.ToString(Newtonsoft.Json.Formatting.None);
        }

        /// <summary>
        /// SHA-256 hex (lowercase).
        /// </summary>
        public static string Sha256(string text)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>
        /// HMAC-SHA256 over provided material using key from Web.config appSetting "BlockchainHmacKey".
        /// Returns 32-byte tag.
        /// </summary>
        public static byte[] ComputeAuthTag(string material)
        {
            var base64 = ConfigurationManager.AppSettings["BlockchainHmacKey"];
            if (string.IsNullOrWhiteSpace(base64))
                throw new InvalidOperationException("BlockchainHmacKey appSetting is missing.");
            byte[] key = Convert.FromBase64String(base64);
            using (var h = new HMACSHA256(key))
                return h.ComputeHash(Encoding.UTF8.GetBytes(material ?? ""));
        }

        /// <summary>
        /// Append a blockchain log entry for a finalized transaction.
        /// Writes: TransactionID, SaleHash, Timestamp(UTC), SaleDataJson, PrevHash, ChainHash, AuthTag.
        /// Uses a short SERIALIZABLE transaction + UPDLOCK/HOLDLOCK to avoid prev-hash races.
        /// </summary>
        public static void AppendSaleLog(string connectionString, int transactionId, object saleDataObject, DateTime? timestampUtc = null)
        {
            // 1) Stable JSON & SaleHash
            string json = ToDeterministicJson(saleDataObject);
            string saleHash = Sha256(json);
            DateTime tsUtc = timestampUtc ?? DateTime.UtcNow;

            // 2) HMAC over content+identity+time (prevents forged recomputation)
            string paidAtIso = tsUtc.ToString("o", CultureInfo.InvariantCulture);
            string macMaterial = $"{json}|{transactionId}|{paidAtIso}";
            byte[] authTagBytes = ComputeAuthTag(macMaterial);

            // 3) Chain: (PrevHash from last row) -> ChainHash
            string prevHash;
            string chainHash;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;

                    // Lock the tail row so two writers can't take the same prev
                    cmd.CommandText = "SELECT TOP 1 ChainHash FROM dbo.BlockchainSalesLog WITH (UPDLOCK, HOLDLOCK) ORDER BY LogID DESC;";
                    var prevObj = cmd.ExecuteScalar();
                    prevHash = (prevObj == null || prevObj == DBNull.Value) ? new string('0', 64) : (string)prevObj;

                    string chainMaterial = $"{prevHash}|{saleHash}|{transactionId}|{tsUtc:O}";
                    chainHash = Sha256(chainMaterial);

                    // Insert row
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
INSERT INTO dbo.BlockchainSalesLog
 (TransactionID, SaleHash, Timestamp, SaleDataJson, PrevHash, ChainHash, AuthTag)
VALUES
 (@tx, @saleHash, @ts, @json, @prev, @chain, @authTag);";

                    cmd.Parameters.Add("@tx", SqlDbType.Int).Value = transactionId;
                    cmd.Parameters.Add("@saleHash", SqlDbType.Char, 64).Value = saleHash;
                    cmd.Parameters.Add("@ts", SqlDbType.DateTime2).Value = tsUtc;           // store UTC
                    cmd.Parameters.Add("@json", SqlDbType.NVarChar).Value = json;           // ensure column is NVARCHAR(MAX)
                    cmd.Parameters.Add("@prev", SqlDbType.Char, 64).Value = prevHash;
                    cmd.Parameters.Add("@chain", SqlDbType.Char, 64).Value = chainHash;
                    cmd.Parameters.Add("@authTag", SqlDbType.VarBinary, 32).Value = authTagBytes;

                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
        }
    }
}
