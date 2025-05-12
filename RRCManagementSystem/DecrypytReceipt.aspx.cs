using System;
using System.Configuration;
using System.IO;
using System.Web;
using RRCManagementSystem.Helpers; // For AESHelper

namespace RRCManagementSystem
{
    public partial class DecryptReceipt : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string fileName = Request.QueryString["file"];
            if (string.IsNullOrEmpty(fileName))
            {
                Response.StatusCode = 400;
                return;
            }

            string path = Server.MapPath("~/Receipts/" + fileName);

            if (!File.Exists(path))
            {
                Response.StatusCode = 404;
                return;
            }

            byte[] encryptedBytes = File.ReadAllBytes(path);
            byte[] decryptedBytes = AESHelper.Decrypt(encryptedBytes);

            string ext = Path.GetExtension(fileName).ToLower();
            if (ext == ".jpg" || ext == ".jpeg")
                Response.ContentType = "image/jpeg";
            else if (ext == ".png")
                Response.ContentType = "image/png";
            else
                Response.ContentType = "application/octet-stream";

            Response.BinaryWrite(decryptedBytes);
            Response.End();
        }
    }
}