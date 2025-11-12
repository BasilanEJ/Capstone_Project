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
        // Set headers FIRST
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.AddHeader("Access-Control-Allow-Origin", "*");
        context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        context.Response.Cache.SetNoStore();
        context.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
        context.Response.AppendHeader("Pragma", "no-cache");

        // Handle OPTIONS preflight request
        if (context.Request.HttpMethod == "OPTIONS")
        {
            context.Response.StatusCode = 200;
            context.Response.End();
            return;
        }

        try
        {
            // Log request
            System.Diagnostics.Debug.WriteLine("=== GetFAQs Handler Called ===");
            System.Diagnostics.Debug.WriteLine($"Request URL: {context.Request.Url}");
            System.Diagnostics.Debug.WriteLine($"Physical Path: {context.Request.PhysicalPath}");

            string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string 'RRCDB' not found in Web.config");
            }

            System.Diagnostics.Debug.WriteLine("Connection string found");

            List<FAQ> faqs = new List<FAQ>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP (1000) [ID]
                          ,[Question]
                          ,[Answer]
                          ,[DisplayOrder]
                          ,[IsActive]
                          ,[CreatedDate]
                          ,[UpdatedDate]
                    FROM [EJBasilan_RRCDB].[EJBasilan_admin].[FAQs]
                    WHERE [IsActive] = 1
                    ORDER BY [DisplayOrder], [ID]";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("✅ Database connection opened");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string question = reader["Question"]?.ToString()?.Trim() ?? "";
                            string answer = reader["Answer"]?.ToString()?.Trim() ?? "";

                            if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                            {
                                faqs.Add(new FAQ
                                {
                                    q = question,
                                    a = answer
                                });
                                System.Diagnostics.Debug.WriteLine($"Added FAQ: {question}");
                            }
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"✅ Total FAQs loaded: {faqs.Count}");

            // Serialize to JSON
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(faqs);

            System.Diagnostics.Debug.WriteLine($"JSON Output: {json}");

            // Send success response
            context.Response.StatusCode = 200;
            context.Response.Write(json);
            context.Response.Flush();
        }
        catch (SqlException sqlEx)
        {
            System.Diagnostics.Debug.WriteLine("❌ SQL Error: " + sqlEx.Message);
            System.Diagnostics.Debug.WriteLine("Error Number: " + sqlEx.Number);
            System.Diagnostics.Debug.WriteLine("Stack Trace: " + sqlEx.StackTrace);

            context.Response.StatusCode = 500;
            var error = new { error = "Database error", message = sqlEx.Message, number = sqlEx.Number };
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            context.Response.Write(serializer.Serialize(error));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("❌ General Error: " + ex.Message);
            System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);

            context.Response.StatusCode = 500;
            var error = new { error = "Server error", message = ex.Message };
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            context.Response.Write(serializer.Serialize(error));
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