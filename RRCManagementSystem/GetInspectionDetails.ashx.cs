using System;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web.SessionState;

namespace RRCManagementSystem
{
    public class GetInspectionDetails : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            try
            {
                // ✅ Ensure the inspector is logged in and authorized
                if (context.Session["UserID"] == null || context.Session["Role"]?.ToString() != "Inspector")
                {
                    context.Response.Write("<p class='text-red-500'>Unauthorized access.</p>");
                    return;
                }

                // ✅ Get the InspectionID from query string
                string idStr = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out int inspectionId))
                {
                    context.Response.Write("<p class='text-red-500'>Invalid or missing inspection ID.</p>");
                    return;
                }

                int inspectorId = Convert.ToInt32(context.Session["UserID"]);
                StringBuilder sb = new StringBuilder();

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString))
                {
                    string query = @"
                        SELECT 
                            i.InspectionID,
                            i.ScheduledDate,
                            i.InspectionStatus,
                            q.InquiryCode
                        FROM Inspections AS i
                        INNER JOIN InquirySimple AS q ON i.InquiryID = q.InquiryID
                        WHERE i.InspectorID = @InspectorID 
                          AND i.InspectionID = @InspectionID";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@InspectorID", inspectorId);
                    cmd.Parameters.AddWithValue("@InspectionID", inspectionId);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string status = HttpUtility.HtmlEncode(reader["InspectionStatus"].ToString());
                        string statusColor = "bg-blue-500"; // default color

                        // ✅ Assign color based on status
                        switch (status)
                        {
                            case "Completed":
                                statusColor = "bg-green-500";
                                break;
                            case "Pending":
                                statusColor = "bg-yellow-500";
                                break;
                            case "Cancelled":
                                statusColor = "bg-red-500";
                                break;
                        }

                        // ✅ Build HTML for single inspection
                        sb.AppendFormat(@"
                            <div class='p-4'>
                                <div class='flex items-center space-x-3 mb-4'>
                                    <div class='w-4 h-4 rounded-full {4}'></div>
                                    <h4 class='text-xl font-semibold text-gray-900'>
                                        Inspection #{0}
                                    </h4>
                                </div>
                                <div class='space-y-2 text-gray-700'>
                                    <p class='text-sm'>
                                        <i class='fas fa-clock mr-1'></i>
                                        <strong>Time:</strong> {1}
                                    </p>
                                    <p class='text-sm'>
                                        <i class='fas fa-tag mr-1'></i>
                                        <strong>Inquiry Code:</strong> {2}
                                    </p>
                                    <p class='text-sm'>
                                        <i class='fas fa-info-circle mr-1'></i>
                                        <strong>Status:</strong> {3}
                                    </p>
                                </div>
                            </div>",
                            reader["InspectionID"],
                            Convert.ToDateTime(reader["ScheduledDate"]).ToString("hh:mm tt"),
                            HttpUtility.HtmlEncode(reader["InquiryCode"].ToString()),
                            status,
                            statusColor
                        );
                    }
                    else
                    {
                        // ✅ No record found for this inspection
                        sb.Append("<p class='text-gray-500 text-center py-6'>No details found for this inspection.</p>");
                    }
                }

                context.Response.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("<p class='text-red-500'>Error loading inspection details: "
                                       + HttpUtility.HtmlEncode(ex.Message) + "</p>");
            }
        }

        public bool IsReusable => false;
    }
}
