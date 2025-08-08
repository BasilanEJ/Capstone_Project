using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Script.Services;

namespace RRCManagementSystem
{
    public partial class CreateBooking : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadServices();
            }
        }

        private void LoadServices()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ServiceID, Name FROM Services ORDER BY Name ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                cblServices.DataSource = cmd.ExecuteReader();
                cblServices.DataTextField = "Name";
                cblServices.DataValueField = "ServiceID";
                cblServices.DataBind();
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // ✅ Check if user is logged in and is an Inspector
            if (Session["UserID"] == null || Session["Role"] == null || Session["Role"].ToString() != "Inspector")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noInspector", "Swal.fire('Unauthorized', 'You must be logged in as an Inspector.', 'error');", true);
                return;
            }

            if (string.IsNullOrEmpty(hfClientID.Value))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "selectClient", "Swal.fire('Missing', 'Please select a client.', 'warning');", true);
                return;
            }

            if (!int.TryParse(txtSQM.Text.Trim(), out int sqm))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidSQM", "Swal.fire('Invalid', 'Please enter a valid SQM.', 'error');", true);
                return;
            }

            if (!decimal.TryParse(txtTotalPrice.Text.Trim(), out decimal total) || total <= 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidPrice", "Swal.fire('Invalid', 'Please enter a valid total price.', 'error');", true);
                return;
            }

            var selectedItems = cblServices.Items.Cast<ListItem>().Where(i => i.Selected).ToList();
            if (!selectedItems.Any())
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noServices", "Swal.fire('No Services', 'Please select at least one service.', 'error');", true);
                return;
            }

            string selectedServiceIDs = string.Join(",", selectedItems.Select(i => i.Value));
            string selectedServiceNames = string.Join(", ", selectedItems.Select(i => i.Text));
            bool isContract = false;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (ListItem item in selectedItems)
                {
                    int serviceId = int.Parse(item.Value);

                    if (!isContract)
                    {
                        string typeQuery = "SELECT ServiceType FROM Services WHERE ServiceID = @ServiceID";
                        SqlCommand typeCmd = new SqlCommand(typeQuery, conn);
                        typeCmd.Parameters.AddWithValue("@ServiceID", serviceId);
                        object typeResult = typeCmd.ExecuteScalar();
                        if (typeResult != null && typeResult.ToString() == "Termite Control")
                        {
                            isContract = true;
                        }
                    }
                }

                // ✅ Save quotation for client
                string insertQuery = @"
                    INSERT INTO PendingQuotations 
                        (ClientID, InspectorID, ServiceNames, ServiceID, SQM, Price, IsContract, CreatedAt) 
                    VALUES 
                        (@ClientID, @InspectorID, @ServiceNames, @ServiceID, @SQM, @Price, @IsContract, GETDATE())";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@ClientID", hfClientID.Value);
                cmd.Parameters.AddWithValue("@InspectorID", Convert.ToInt32(Session["UserID"]));
                cmd.Parameters.AddWithValue("@ServiceNames", selectedServiceNames);
                cmd.Parameters.AddWithValue("@ServiceID", selectedServiceIDs);
                cmd.Parameters.AddWithValue("@SQM", sqm);
                cmd.Parameters.AddWithValue("@Price", total);
                cmd.Parameters.AddWithValue("@IsContract", isContract);
                cmd.ExecuteNonQuery();
            }

            // ✅ Show success
            ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Success', 'Quotation submitted for client!', 'success');", true);

            // ✅ Reset fields
            txtClientSearch.Text = "";
            hfClientID.Value = "";
            cblServices.ClearSelection();
            txtSQM.Text = "";
            txtTotalPrice.Text = "";
        }

        [WebMethod]
        [ScriptMethod]
        public static List<string> SearchClients(string prefixText, int count)
        {
            List<string> clients = new List<string>();
            string connStr = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = $@"
            SELECT TOP {count} 
                (LastName + ', ' + FirstName + ' ' + ISNULL(MiddleName, '')) AS FullName, 
                ClientID 
            FROM Clients 
            WHERE 
                (
                    FirstName LIKE @prefix + '%' OR
                    LastName LIKE @prefix + '%' OR
                    MiddleName LIKE @prefix + '%' OR
                    (LastName + ', ' + FirstName + ' ' + ISNULL(MiddleName, '')) LIKE @prefix + '%'
                )
                AND Status = 'Approved'
            ORDER BY LastName ASC, FirstName ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@prefix", prefixText);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(
                        AjaxControlToolkit.AutoCompleteExtender.CreateAutoCompleteItem(
                            reader["FullName"].ToString(),
                            reader["ClientID"].ToString()
                        )
                    );
                }
            }

            return clients;
        }

    }
}
