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

        protected void btnCalculate_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtSQM.Text.Trim(), out int sqm))
            {
                lblTotal.Text = "❌ Please enter a valid SQM.";
                return;
            }

            decimal total = 0;
            foreach (ListItem item in cblServices.Items)
            {
                if (item.Selected)
                {
                    int serviceId = int.Parse(item.Value);
                    total += GetServicePrice(serviceId, sqm);
                }
            }

            lblTotal.Text = $"💰 Total Price: ₱{total:N2}";
            lblTotal.ForeColor = System.Drawing.Color.Green;
        }

        private decimal GetServicePrice(int serviceId, int sqm)
        {
            decimal price = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Price100SQM, Price200SQM, PriceAbove200SQM FROM Services WHERE ServiceID = @ServiceID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    if (sqm <= 100)
                        price = Convert.ToDecimal(reader["Price100SQM"]);
                    else if (sqm <= 200)
                        price = Convert.ToDecimal(reader["Price200SQM"]);
                    else
                        price = Convert.ToDecimal(reader["PriceAbove200SQM"]);
                }
            }
            return price;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
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

            var selectedItems = cblServices.Items.Cast<ListItem>().Where(i => i.Selected).ToList();
            if (!selectedItems.Any())
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noServices", "Swal.fire('No Services', 'Please select at least one service.', 'error');", true);
                return;
            }

            string selectedServiceIDs = string.Join(",", selectedItems.Select(i => i.Value));
            string selectedServiceNames = string.Join(", ", selectedItems.Select(i => i.Text));
            decimal total = 0;
            bool isContract = false;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (ListItem item in selectedItems)
                {
                    int serviceId = int.Parse(item.Value);
                    total += GetServicePrice(serviceId, sqm);

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
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string insertQuery = @"
                    INSERT INTO PendingQuotations 
                        (ClientID, InspectorID, ServiceNames, ServiceID, SQM, Price, IsContract, CreatedAt) 
                    VALUES 
                        (@ClientID, @InspectorID, @ServiceNames, @ServiceID, @SQM, @Price, @IsContract, GETDATE())";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@ClientID", hfClientID.Value);
                cmd.Parameters.AddWithValue("@InspectorID", Convert.ToInt32(Session["AdminID"]));
                cmd.Parameters.AddWithValue("@ServiceNames", selectedServiceNames);
                cmd.Parameters.AddWithValue("@ServiceID", selectedServiceIDs);
                cmd.Parameters.AddWithValue("@SQM", sqm);
                cmd.Parameters.AddWithValue("@Price", total);
                cmd.Parameters.AddWithValue("@IsContract", isContract);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Success', 'Quotation submitted for client!', 'success');", true);
            txtClientSearch.Text = "";
            hfClientID.Value = "";
            cblServices.ClearSelection();
            txtSQM.Text = "";
            lblTotal.Text = "";
        }


        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static List<string> SearchClients(string prefixText, int count)
        {
            List<string> clients = new List<string>();
            string connStr = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT TOP (@count) Name, ClientID 
            FROM Clients 
            WHERE Name LIKE @prefix + '%' AND Status = 'Approved'
            ORDER BY Name ASC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@prefix", prefixText);
                cmd.Parameters.AddWithValue("@count", count);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(
                        AjaxControlToolkit.AutoCompleteExtender.CreateAutoCompleteItem(
                            reader["Name"].ToString(),
                            reader["ClientID"].ToString()
                        )
                    );
                }
            }

            return clients;
        }


        /*    protected void txtClientSearch_TextChanged(object sender, EventArgs e)
            {

                string input = txtClientSearch.Text;
                if (input.Contains("|"))
                {
                    var parts = input.Split('|');
                    txtClientSearch.Text = parts[0];
                    hfClientID.Value = parts[1];
                }
            } */


    }
}
