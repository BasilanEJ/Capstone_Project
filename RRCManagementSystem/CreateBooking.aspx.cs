using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;
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
                LoadServices();
            }
        }

        private void LoadServices()
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Services_List", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    cblServices.DataSource = rdr;
                    cblServices.DataTextField = "Name";
                    cblServices.DataValueField = "ServiceID";
                    cblServices.DataBind();
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // ✅ must be logged in AND Inspector
            if (Session["UserID"] == null || Session["Role"] == null || !string.Equals(Session["Role"].ToString(), "Inspector", StringComparison.OrdinalIgnoreCase))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noInspector",
                    "Swal.fire('Unauthorized', 'You must be logged in as an Inspector.', 'error');", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(hfClientID.Value) || !int.TryParse(hfClientID.Value, out int clientId))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "selectClient",
                    "Swal.fire('Missing', 'Please select a client.', 'warning');", true);
                return;
            }

            if (!int.TryParse(txtSQM.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int sqm))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidSQM",
                    "Swal.fire('Invalid', 'Please enter a valid SQM.', 'error');", true);
                return;
            }

            if (!decimal.TryParse(txtTotalPrice.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal total) || total <= 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidPrice",
                    "Swal.fire('Invalid', 'Please enter a valid total price.', 'error');", true);
                return;
            }

            var selectedItems = cblServices.Items.Cast<ListItem>().Where(i => i.Selected).ToList();
            if (!selectedItems.Any())
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noServices",
                    "Swal.fire('No Services', 'Please select at least one service.', 'error');", true);
                return;
            }

            string selectedServiceIDs = string.Join(",", selectedItems.Select(i => i.Value));
            string selectedServiceNames = string.Join(", ", selectedItems.Select(i => i.Text));
            int inspectorId = Convert.ToInt32(Session["UserID"], CultureInfo.InvariantCulture);

            bool isContract = GetIsAnyContract(selectedServiceIDs);

            // ✅ Insert pending quotation
            int newId = InsertPendingQuotation(
                clientId: clientId,
                inspectorId: inspectorId,
                serviceNames: selectedServiceNames,
                serviceIdCsv: selectedServiceIDs,
                sqm: sqm,
                price: total,
                isContract: isContract
            );

            // ✅ Success UI
            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "Swal.fire('Success', 'Quotation submitted for client!', 'success');", true);

            // ✅ Reset fields
            txtClientSearch.Text = "";
            hfClientID.Value = "";
            cblServices.ClearSelection();
            txtSQM.Text = "";
            txtTotalPrice.Text = "";
        }

        private bool GetIsAnyContract(string serviceIdCsv)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_Services_IsAnyContract", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ServiceIDs", SqlDbType.NVarChar, -1).Value = (object)serviceIdCsv ?? DBNull.Value;

                var pOut = cmd.Parameters.Add("@IsContract", SqlDbType.Bit);
                pOut.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();

                return (pOut.Value != DBNull.Value) && Convert.ToBoolean(pOut.Value, CultureInfo.InvariantCulture);
            }
        }

        private int InsertPendingQuotation(int clientId, int inspectorId, string serviceNames, string serviceIdCsv, int sqm, decimal price, bool isContract)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("dbo.usp_PendingQuotations_Insert", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@InspectorID", SqlDbType.Int).Value = inspectorId;
                cmd.Parameters.Add("@ServiceNames", SqlDbType.NVarChar, 4000).Value = (object)serviceNames ?? DBNull.Value;
                cmd.Parameters.Add("@ServiceIDCsv", SqlDbType.NVarChar, 4000).Value = (object)serviceIdCsv ?? DBNull.Value;
                cmd.Parameters.Add("@SQM", SqlDbType.Int).Value = sqm;

                var pPrice = cmd.Parameters.Add("@Price", SqlDbType.Decimal);
                pPrice.Precision = 18; pPrice.Scale = 2; pPrice.Value = price;

                cmd.Parameters.Add("@IsContract", SqlDbType.Bit).Value = isContract;

                var pOutId = cmd.Parameters.Add("@PendingQuotationID", SqlDbType.Int);
                pOutId.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();

                return (pOutId.Value == DBNull.Value) ? 0 : Convert.ToInt32(pOutId.Value, CultureInfo.InvariantCulture);
            }
        }

        // ===== Ajax AutoComplete endpoint (stored procedure) =====
        [WebMethod]
        [ScriptMethod]
        public static System.Collections.Generic.List<string> SearchClients(string prefixText, int count)
        {
            var results = new System.Collections.Generic.List<string>();
            string cs = ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.usp_Clients_Search", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Prefix", SqlDbType.NVarChar, 200).Value =
                    (object)(prefixText ?? string.Empty) ?? DBNull.Value;

                // Bounds check for sanity
                int capped = Math.Max(1, Math.Min(count <= 0 ? 10 : count, 50));
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = capped;

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string fullName = rdr["FullName"]?.ToString() ?? "";
                        string id = rdr["ClientID"]?.ToString() ?? "";
                        // Keep the same format your extender expects
                        results.Add(AjaxControlToolkit.AutoCompleteExtender.CreateAutoCompleteItem(fullName, id));
                    }
                }
            }

            return results;
        }
    }
}
