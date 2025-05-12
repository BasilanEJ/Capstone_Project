using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RRCManagementSystem
{
    public partial class CreateBooking : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadClients();
                LoadServices();
            }
        }

        private void LoadClients()
        {
            ddlClients.Items.Clear();
            ddlClients.Items.Add(new ListItem("-- Select Client --", ""));

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ClientID, Name FROM Clients WHERE Status = 'Approved' ORDER BY Name ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ddlClients.Items.Add(new ListItem(reader["Name"].ToString(), reader["ClientID"].ToString()));
                }
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
            if (string.IsNullOrEmpty(ddlClients.SelectedValue))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "selectClient", "Swal.fire('Missing', 'Please select a client.', 'warning');", true);
                return;
            }

            if (!int.TryParse(txtSQM.Text.Trim(), out int sqm))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidSQM", "Swal.fire('Invalid', 'Please enter a valid SQM.', 'error');", true);
                return;
            }

            string selectedServices = string.Join(", ", cblServices.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text));
            if (string.IsNullOrEmpty(selectedServices))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noServices", "Swal.fire('No Services', 'Please select at least one service.', 'error');", true);
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

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO PendingQuotations 
                     (ClientID, InspectorID, ServiceNames, SQM, Price, CreatedAt) 
                     VALUES 
                     (@ClientID, @InspectorID, @ServiceNames, @SQM, @Price, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", ddlClients.SelectedValue);
                cmd.Parameters.AddWithValue("@InspectorID", Convert.ToInt32(Session["AdminID"]));
                cmd.Parameters.AddWithValue("@ServiceNames", selectedServices);
                cmd.Parameters.AddWithValue("@SQM", sqm);
                cmd.Parameters.AddWithValue("@Price", total);

                conn.Open();
                cmd.ExecuteNonQuery();
            }


            ScriptManager.RegisterStartupScript(this, GetType(), "success", "Swal.fire('Success', 'Quotation submitted for client!', 'success');", true);
            ddlClients.ClearSelection();
            cblServices.ClearSelection();
            txtSQM.Text = "";
            lblTotal.Text = "";
        }
    }
}