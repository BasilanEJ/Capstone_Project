using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;

namespace RRCManagementSystem
{
    public partial class DecryptReceipt : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ Clear any previous output
            Response.Clear();
            Response.Buffer = false; // Disable buffering for large files

            try
            {
                // Only allow GET requests
                if (!Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    ServeError(405, "Method not allowed");
                    return;
                }

                // Get TransactionID from query string
                string txIdParam = Request.QueryString["tx"];
                if (string.IsNullOrWhiteSpace(txIdParam) || !int.TryParse(txIdParam, out int txId))
                {
                    ServeError(400, "Missing or invalid transaction ID");
                    return;
                }

                // Get plain filename from database
                string fileName = GetReceiptFromDB(txId);

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    ServeError(404, "Receipt not found in database");
                    return;
                }

                // Build path to PDF file
                string receiptsFolder = Server.MapPath("~/Receipts/");
                string filePath = Path.Combine(receiptsFolder, fileName);

                // Security check: ensure file is within Receipts folder
                string normalizedPath = Path.GetFullPath(filePath);
                string normalizedFolder = Path.GetFullPath(receiptsFolder);

                if (!normalizedPath.StartsWith(normalizedFolder, StringComparison.OrdinalIgnoreCase))
                {
                    ServeError(403, "Access denied");
                    return;
                }

                // Check if file exists
                if (!File.Exists(filePath))
                {
                    ServeError(404, $"Receipt file not found: {fileName}");
                    return;
                }

                // Determine content type
                string ext = Path.GetExtension(fileName).ToLowerInvariant();
                string contentType = "application/pdf";

                if (ext == ".jpg" || ext == ".jpeg")
                    contentType = "image/jpeg";
                else if (ext == ".png")
                    contentType = "image/png";
                else if (ext == ".gif")
                    contentType = "image/gif";
                else if (ext == ".webp")
                    contentType = "image/webp";

                // ✅ Set proper headers for file streaming
                Response.ContentType = contentType;
                Response.AddHeader("Content-Disposition", $"inline; filename=\"{fileName}\"");
                Response.AddHeader("Content-Length", new FileInfo(filePath).Length.ToString());
                Response.AddHeader("Cache-Control", "public, max-age=86400"); // Cache for 1 day

                // Stream the file
                Response.TransmitFile(filePath);
                Response.Flush();

                // ✅ Use HttpContext.Current.ApplicationInstance.CompleteRequest() instead of Response.End()
                // This prevents ThreadAbortException
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException)
            {
                // Client disconnected - ignore
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                System.Diagnostics.Debug.WriteLine($"DecryptReceipt Error: {ex.Message}");

                try
                {
                    ServeError(500, $"Error loading receipt: {ex.Message}");
                }
                catch
                {
                    // If we can't even serve an error, just complete the request
                    try { HttpContext.Current.ApplicationInstance.CompleteRequest(); } catch { }
                }
            }
        }

        private void ServeError(int statusCode, string message)
        {
            Response.Clear();
            Response.StatusCode = statusCode;
            Response.ContentType = "text/html; charset=utf-8";

            string html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Error {statusCode}</title>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ 
            font-family: 'Segoe UI', Arial, sans-serif; 
            padding: 20px; 
            background: #f9fafb;
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            margin: 0;
        }}
        .error-box {{ 
            background: white; 
            padding: 40px; 
            border-radius: 12px; 
            max-width: 500px; 
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
            text-align: center;
        }}
        .error-code {{ 
            color: #dc2626; 
            font-size: 64px; 
            font-weight: 800;
            margin-bottom: 16px;
        }}
        .error-message {{ 
            color: #374151; 
            font-size: 18px;
            line-height: 1.6;
        }}
        .back-link {{
            display: inline-block;
            margin-top: 24px;
            padding: 12px 24px;
            background: #2563eb;
            color: white;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            transition: background 0.2s;
        }}
        .back-link:hover {{
            background: #1d4ed8;
        }}
    </style>
</head>
<body>
    <div class='error-box'>
        <div class='error-code'>{statusCode}</div>
        <div class='error-message'>{HttpUtility.HtmlEncode(message)}</div>
        <a href='javascript:window.close()' class='back-link'>Close Window</a>
    </div>
</body>
</html>";

            Response.Write(html);
            Response.Flush();
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private string GetReceiptFromDB(int transactionId)
        {
            string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("SELECT Receipt FROM Transactions WHERE TransactionID = @TxID", con))
            {
                cmd.Parameters.Add("@TxID", SqlDbType.Int).Value = transactionId;
                con.Open();
                object result = cmd.ExecuteScalar();
                return result?.ToString() ?? "";
            }
        }
    }
}