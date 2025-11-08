using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace RRCManagementSystem.Helpers
{
    public static class EmailHelper
    {
        private static readonly string smtpHost = "smtp.gmail.com";
        private static readonly int smtpPort = 587;
        private static readonly string fromEmail = ConfigurationManager.AppSettings["emailFrom"] ?? "rrctermiteandpestcontrol@gmail.com";
        private static readonly string appPassword = ConfigurationManager.AppSettings["emailPassword"];

        public static void SendEmail(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(fromEmail, "RRC Pest Control");
                    message.To.Add(toEmail);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = isHtml;

                    using (var smtp = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                        smtp.EnableSsl = true;
                        smtp.Timeout = 10000;
                        smtp.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email Error: {ex.Message}");
                throw;
            }
        }
    }
}