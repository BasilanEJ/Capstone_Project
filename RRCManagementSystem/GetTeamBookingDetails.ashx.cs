using RRCManagementSystem.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace RRCManagementSystem
{
    /// <summary>
    /// Handler to get detailed booking information for modal display
    /// </summary>
    public class GetTeamBookingDetails : IHttpHandler, IRequiresSessionState
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            // Verify session
            if (context.Session["UserID"] == null || context.Session["Role"]?.ToString() != "Headtechnician")
            {
                context.Response.Write("<div class='text-red-600'>Unauthorized access</div>");
                return;
            }

            string bookingIdStr = context.Request.QueryString["id"];
            if (!int.TryParse(bookingIdStr, out int bookingID))
            {
                context.Response.Write("<div class='text-red-600'>Invalid booking ID</div>");
                return;
            }

            int teamLeaderID = Convert.ToInt32(context.Session["UserID"]);

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.spTeamLeader_GetBookingDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingID;
                    cmd.Parameters.Add("@TeamLeaderID", SqlDbType.Int).Value = teamLeaderID;

                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var html = BuildDetailsHtml(reader);
                            context.Response.Write(html);
                        }
                        else
                        {
                            context.Response.Write("<div class='text-gray-600'>Booking not found or you don't have access to view it.</div>");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTeamBookingDetails error: {ex.Message}");
                context.Response.Write($"<div class='text-red-600'>Error loading details: {HttpUtility.HtmlEncode(ex.Message)}</div>");
            }
        }

        private string BuildDetailsHtml(IDataReader reader)
        {
            var html = new StringBuilder();

            string bookingCode = reader["BookingCode"].ToString();
            string clientName = reader["ClientName"].ToString();
            string status = reader["Status"].ToString();
            DateTime scheduledDate = Convert.ToDateTime(reader["ScheduledDate"]);
            TimeSpan startTime = reader["StartTime"] != DBNull.Value ? (TimeSpan)reader["StartTime"] : new TimeSpan(8, 0, 0);
            string services = reader["ServiceNames"].ToString();
            int sqm = Convert.ToInt32(reader["SQM"]);
            decimal price = Convert.ToDecimal(reader["Price"]);

            // Decrypt contact and address
            string contact = "N/A";
            string address = "";

            try
            {
                if (reader["ClientContactEnc"] != DBNull.Value)
                    contact = AESHelper.DecryptField(reader["ClientContactEnc"].ToString());

                var addressParts = new System.Collections.Generic.List<string>();
                if (reader["StreetEnc"] != DBNull.Value)
                    addressParts.Add(AESHelper.DecryptField(reader["StreetEnc"].ToString()));
                if (reader["BarangayEnc"] != DBNull.Value)
                    addressParts.Add(AESHelper.DecryptField(reader["BarangayEnc"].ToString()));
                if (reader["CityEnc"] != DBNull.Value)
                    addressParts.Add(AESHelper.DecryptField(reader["CityEnc"].ToString()));

                address = string.Join(", ", addressParts);
            }
            catch { }

            // Status badge color
            string statusColor = status == "Completed" ? "green"
                               : status == "In Progress" ? "orange"
                               : "blue";

            html.Append($@"
                <div class='space-y-4'>
                    <div class='detail-item'>
                        <span class='detail-label'>Booking Code:</span>
                        <span class='detail-value font-semibold'>{HttpUtility.HtmlEncode(bookingCode)}</span>
                    </div>
                    
                    <div class='detail-item'>
                        <span class='detail-label'>Status:</span>
                        <span class='detail-value'>
                            <span class='px-3 py-1 rounded-full text-sm font-semibold bg-{statusColor}-100 text-{statusColor}-700'>
                                {HttpUtility.HtmlEncode(status)}
                            </span>
                        </span>
                    </div>
                    
                    <div class='detail-item'>
                        <span class='detail-label'>Client:</span>
                        <span class='detail-value'>{HttpUtility.HtmlEncode(clientName)}</span>
                    </div>
                    
                    <div class='detail-item'>
                        <span class='detail-label'>Contact:</span>
                        <span class='detail-value'>{HttpUtility.HtmlEncode(contact)}</span>
                    </div>
                    
                    <div class='detail-item'>
                        <span class='detail-label'>Address:</span>
                        <span class='detail-value'>{HttpUtility.HtmlEncode(address)}</span>
                    </div>
                    
                    <div class='detail-item'>
                        <span class='detail-label'>Scheduled:</span>
                        <span class='detail-value'>
                            <i class='far fa-calendar mr-1'></i>{scheduledDate:MMMM dd, yyyy}
                            <span class='ml-2'><i class='far fa-clock mr-1'></i>{DateTime.Today.Add(startTime):hh:mm tt}</span>
                        </span>
                    </div>
                    
                    <div class='detail-item'>
                        <span class='detail-label'>Services:</span>
                        <span class='detail-value'>{HttpUtility.HtmlEncode(services)}</span>
                    </div>
                    
                    <div class='detail-item'>
                        <span class='detail-label'>Coverage:</span>
                        <span class='detail-value'>{sqm} m²</span>
                    </div>
                    
                    <div class='detail-item'>
                        <span class='detail-label'>Total Price:</span>
                        <span class='detail-value font-semibold text-green-600'>₱{price:N2}</span>
                    </div>
                </div>
            ");

            return html.ToString();
        }

        public bool IsReusable => false;
    }
}