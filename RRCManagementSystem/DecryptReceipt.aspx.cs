using System;
using System.IO;
using System.Web;
using System.Web.UI;
using RRCManagementSystem.Helpers;

namespace RRCManagementSystem
{
    public partial class DecryptReceipt : Page
    {
        protected void DecryptReceipt_Load(object sender, EventArgs e)
        {
            // Only GET
            if (!Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                Response.StatusCode = 405;
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Accept ?file=filename.ext
            string fileName = Path.GetFileName(Request.QueryString["file"]);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Response.StatusCode = 400; // Bad Request
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Always resolve under ~/Receipts
            string baseFolder = Server.MapPath("~/Receipts/");
            string path = Path.Combine(baseFolder, fileName);

            if (!File.Exists(path))
            {
                Response.StatusCode = 404; // Not Found
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            byte[] encryptedBytes = File.ReadAllBytes(path);
            byte[] decryptedBytes = AESHelper.Decrypt(encryptedBytes);

            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            string contentType =
                (ext == ".jpg" || ext == ".jpeg") ? "image/jpeg" :
                (ext == ".png") ? "image/png" :
                "application/octet-stream";

            Response.Clear();
            Response.BufferOutput = true;
            Response.ContentType = contentType;
            Response.AddHeader("Content-Disposition", $"inline; filename=\"{fileName}\"");
            Response.Cache.SetCacheability(HttpCacheability.Private);
            Response.Cache.SetMaxAge(TimeSpan.FromMinutes(10));

            Response.BinaryWrite(decryptedBytes);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
