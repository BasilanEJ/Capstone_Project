using System;

namespace RRCManagementSystem
{
    public partial class VerifyOTP : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Text = "";
            }
        }

        protected void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            string enteredOTP = txtOTP.Text.Trim();
            string sessionOTP = Session["OTP"]?.ToString();
            DateTime? expiry = Session["OTP_Expiry"] as DateTime?;

            if (string.IsNullOrEmpty(enteredOTP))
            {
                lblMessage.Text = "⚠ Please enter the OTP.";
                return;
            }

            if (sessionOTP == null || expiry == null)
            {
                lblMessage.Text = "⚠ Session expired. Please request a new OTP.";
                Response.Redirect("ForgotPassword.aspx");
                return;
            }

            if (DateTime.Now > expiry)
            {
                lblMessage.Text = "⚠ OTP has expired. Please request a new one.";
                Response.Redirect("ForgotPassword.aspx");
                return;
            }

            if (enteredOTP == sessionOTP)
            {
                // ✅ OTP verified
                Session["IsOTPVerified"] = true;
                Session["Email"] = Session["OTP_Email"]; // Store email for ResetPassword

                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.Text = "✅ OTP verified! Redirecting...";

                Response.Redirect("ResetPassword.aspx");
            }
            else
            {
                lblMessage.Text = "⚠ Invalid OTP. Please try again.";
            }
        }
    }
}