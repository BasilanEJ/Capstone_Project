    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    namespace RRCManagementSystem
    {
	    public partial class AddStocks : System.Web.UI.Page
	    {
            private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RRCDB"].ConnectionString;
            private int itemId;

            protected void Page_Load(object sender, EventArgs e)
            {
                if (Session["UserID"] == null || Session["Role"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                string role = Session["Role"].ToString();
                if (role == "SuperAdmin" || role == "Inspector")
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

            string encodedId = Request.QueryString["ItemID"];
            if (string.IsNullOrEmpty(encodedId) || !TryDecodeID(encodedId, out itemId))
            {
                lblMessage.Text = "❌ Invalid item ID.";
                return;
            }


            if (!IsPostBack)
                    LoadItem();
            }


            private void LoadItem()
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT Name, Type, ImagePath FROM Inventory WHERE ItemID = @ItemID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ItemID", itemId);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        lblItemName.Text = reader["Name"].ToString();
                        lblItemType.Text = reader["Type"].ToString();
                        imgItem.ImageUrl = reader["ImagePath"]?.ToString();
                    }
                    else
                    {
                        lblMessage.Text = "❌ Item not found.";
                        btnSubmit.Enabled = false;
                    }
                }
            }

        private bool TryDecodeID(string encoded, out int decodedId)
        {
            try
            {
                string base64 = encoded.Replace("-", "+").Replace("_", "/");
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                byte[] data = Convert.FromBase64String(base64);
                string idString = System.Text.Encoding.UTF8.GetString(data);
                return int.TryParse(idString, out decodedId);
            }
            catch
            {
                decodedId = 0;
                return false;
            }
        }


        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int adminId = Convert.ToInt32(Session["UserID"]);

            if (!int.TryParse(txtAddQty.Text.Trim(), out int addQty) || addQty <= 0)
            {
                lblMessage.Text = "⚠ Please enter a valid quantity.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string updateQuery = @"UPDATE Inventory SET Quantity = Quantity + @AddQty WHERE ItemID = @ItemID";

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@AddQty", addQty);
                    cmd.Parameters.AddWithValue("@ItemID", itemId);

                    try
                    {
                        con.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            AddAuditLog(adminId, $"✅ Added {addQty} stock to Item ID {itemId}");
                            string script = @"
    <script>
        Swal.fire({
            icon: 'success',
            title: 'Stock Added!',
            text: 'Quantity was added successfully.',
            confirmButtonText: 'OK'
        }).then(() => {
            window.location.href = 'ViewItem.aspx';
        });
    </script>";

                            ClientScript.RegisterStartupScript(this.GetType(), "stockAdded", script);

                        }
                        else
                        {
                            lblMessage.Text = "❌ Item not found or update failed.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "❌ Error: " + ex.Message;
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
        }

        private void AddAuditLog(int? adminId, string action)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO AuditLogs (AdminID, Action, Timestamp) VALUES (@AdminID, @Action, GETDATE())";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@AdminID", (object)adminId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
