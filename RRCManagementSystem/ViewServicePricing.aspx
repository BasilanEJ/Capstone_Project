<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="ViewServicePricing.aspx.cs"
    Inherits="RRCManagementSystem.ViewServicePricing" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .table { width: 100%; border-collapse: collapse; }
        .table th, .table td {
            border: 1px solid #dee2e6;
            padding: 12px 15px;
            text-align: left;
        }
        .table th {
            background: #0b3f7a;
            color: #fff;
        }
        .action-btn {
            color: #0b3f7a;
            text-decoration: none;
            padding: 4px 6px;
            cursor: pointer;
        }
        .action-btn:hover {
            text-decoration: underline;
        }
        .form-control {
            padding: 6px;
            width: 100%;
        }

        /* Button styles */
        .btn {
            padding: 8px 16px;
            background-color: #0b3f7a;
            color: #fff;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            text-decoration: none;
        }

        .btn:hover {
            background-color: #09315a;
        }

        .btn-secondary {
            background-color: #6c757d;
        }

        .btn-secondary:hover {
            background-color: #5a6268;
        }
    </style>

    <h2 style="margin-bottom:20px;">Service Pricing</h2>

    <asp:HiddenField ID="hfServiceID" runat="server" />


    <asp:GridView ID="gvServicePricing" runat="server"
        AutoGenerateColumns="False"
        CssClass="table"
        DataKeyNames="PricingID"
        OnRowEditing="gvServicePricing_RowEditing"
        OnRowUpdating="gvServicePricing_RowUpdating"
        OnRowCancelingEdit="gvServicePricing_RowCancelingEdit"
        OnRowCommand="gvServicePricing_RowCommand"
        EmptyDataText="No pricing data found.">

        <Columns>
           
            <asp:BoundField DataField="PricingID" HeaderText="Pricing ID" ReadOnly="True" Visible="False" />

           
            <asp:BoundField DataField="Name" HeaderText="Service Name" ReadOnly="True" />

           
            <asp:BoundField DataField="MinSQM" HeaderText="Min SQM" ReadOnly="True" />
            <asp:BoundField DataField="MaxSQM" HeaderText="Max SQM" ReadOnly="True" />

  
            <asp:TemplateField HeaderText="Price">
                <ItemTemplate>
                    <%# Eval("Price", "{0:N2}") %>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEditPrice" runat="server" CssClass="form-control"
                        Text='<%# Bind("Price", "{0:N2}") %>' />
                </EditItemTemplate>
            </asp:TemplateField>

        
            <asp:TemplateField HeaderText="Actions">
                <ItemTemplate>
                    <asp:LinkButton ID="btnEdit" runat="server"
                        CommandName="Edit" Text="Edit"
                        CssClass="action-btn" />
                    &nbsp;|&nbsp;
                    <asp:LinkButton ID="btnDelete" runat="server"
                        CommandName="DeletePrice"
                        CommandArgument='<%# Eval("PricingID") %>'
                        CssClass="action-btn"
                        CausesValidation="false"
                        UseSubmitBehavior="false"
                        OnClientClick="return confirmDelete(this, event);"
                        Text="Delete" />
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="btnUpdate" runat="server"
                        CommandName="Update" Text="Update"
                        CssClass="action-btn" />
                    &nbsp;|&nbsp;
                    <asp:LinkButton ID="btnCancel" runat="server"
                        CommandName="Cancel" Text="Cancel"
                        CssClass="action-btn" />
                </EditItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <br /><br />

    <!-- Back Button Below GridView -->
    <asp:Button ID="btnBack" runat="server" Text="← Back to Services"
        CssClass="btn btn-secondary"
        OnClick="btnBack_Click" />

    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script type="text/javascript">
        // SweetAlert confirmation for delete
        function confirmDelete(btn, evt) {
            if (btn.dataset.confirmed === '1') return true;

            if (evt) evt.preventDefault();

            Swal.fire({
                title: 'Delete this price range?',
                text: 'This pricing record will be permanently deleted.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#0b3f7a',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, delete it'
            }).then((result) => {
                if (result.isConfirmed) {
                    btn.dataset.confirmed = '1';
                    btn.click();
                }
            });

            return false;
        }
    </script>

</asp:Content>
