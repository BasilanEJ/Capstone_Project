using System;
using System.IO;
using RRCManagementSystem.Helpers; // Make sure you import AESHelper

namespace RRCManagementSystem
{
    public partial class DecryptPDF : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminID"] == null)
            {
                Response.Redirect("~/Login.aspx");
            }
        }


        protected void btnDecrypt_Click(object sender, EventArgs e)
        {
            if (fileUpload.HasFile)
            {
                byte[] encryptedBytes = fileUpload.FileBytes;

                try
                {
                    // 🔵 Decrypt using your AESHelper
                    byte[] decryptedBytes = AESHelper.Decrypt(encryptedBytes);

                    // Send decrypted PDF to browser
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "inline;filename=DecryptedFile.pdf");
                    Response.BinaryWrite(decryptedBytes);
                    Response.End();
                }
                catch (Exception ex)
                {
                    Response.Write($"<script>alert('Error decrypting file: {ex.Message}');</script>");
                }
            }
            else
            {
                Response.Write("<script>alert('Please upload a file first.');</script>");
            }
        }
    }
}