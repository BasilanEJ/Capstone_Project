
using System;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Collections.Generic;

public class GetFAQs : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.AddHeader("Access-Control-Allow-Origin", "*");
        context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        context.Response.Cache.SetNoStore();

        // Handle OPTIONS preflight request
        if (context.Request.HttpMethod == "OPTIONS")
        {
            context.Response.StatusCode = 200;
            context.Response.End();
            return;
        }

        try
        {
            string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string 'RRCDB' not found in Web.config");
            }

            List<FAQ> faqs = new List<FAQ>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ID, Question, Answer, DisplayOrder, IsActive FROM FAQs WHERE IsActive = 1 ORDER BY DisplayOrder, ID",
                    conn))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("✅ Database connection opened");

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        faqs.Add(new FAQ
                        {
                            q = reader["Question"].ToString().Trim(),
                            a = reader["Answer"].ToString().Trim()
                        });
                    }
                    reader.Close();
                }
            }

            // Serialize to JSON
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(faqs);

            // Debug log
            System.Diagnostics.Debug.WriteLine($"✅ FAQs loaded successfully: {faqs.Count} items");

            context.Response.StatusCode = 200;
            context.Response.Write(json);
        }
        catch (SqlException sqlEx)
        {
            System.Diagnostics.Debug.WriteLine("❌ SQL Error: " + sqlEx.Message);
            System.Diagnostics.Debug.WriteLine("Error Number: " + sqlEx.Number);
            System.Diagnostics.Debug.WriteLine("Stack Trace: " + sqlEx.StackTrace);

            context.Response.StatusCode = 500;
            context.Response.Write("{\"error\":\"Database error: " + sqlEx.Message.Replace("\"", "'") + "\"}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("❌ Error: " + ex.Message);
            System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);

            context.Response.StatusCode = 500;
            context.Response.Write("{\"error\":\"" + ex.Message.Replace("\"", "'") + "\"}");
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }

    public class FAQ
    {
        public string q { get; set; }
        public string a { get; set; }
    }
}