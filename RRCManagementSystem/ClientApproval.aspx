<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ClientApproval.aspx.cs" Inherits="RRCManagementSystem.ClientApproval" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .approval-header {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 20px;
            color: #004085;
        }

        .approval-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 30px;
        }

        .approval-table th, .approval-table td {
            border: 1px solid #dee2e6;
            padding: 10px;
            text-align: left;
        }

        .approval-table th {
            background-color: #e9ecef;
            color: #333;
        }

        .btn-approve, .btn-reject {
            padding: 6px 12px;
            font-size: 14px;
            border: none;
            cursor: pointer;
            color: white;
            border-radius: 4px;
        }

        .btn-approve {
            background-color: #28a745;
        }

        .btn-approve:hover {
            background-color: #218838;
        }
.btn-reject {
    background-color: #dc3545;
    color: white;
    padding: 6px 12px;
    border: none;
    cursor: pointer;
    font-size: 14px;
}

.btn-reject:hover {
    background-color: #c82333;
}

    </style>

    <div>
        <h2 class="approval-header">Client Email Approval</h2>

        <asp:GridView ID="gvClients" runat="server" AutoGenerateColumns="False" CssClass="approval-table"
            DataKeyNames="ClientID" OnRowCommand="gvClients_RowCommand">
            <Columns>
                <asp:BoundField DataField="ClientID" HeaderText="Client ID" />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:BoundField DataField="ContactNumber" HeaderText="Contact" />
                <asp:BoundField DataField="CreatedAt" HeaderText="Registered On" DataFormatString="{0:g}" />

      <asp:TemplateField HeaderText="Action">
    <ItemTemplate>
        <asp:Button ID="btnApprove" runat="server"
            Text="Approve"
            CommandName="Approve"
            CommandArgument='<%# Eval("ClientID") %>'
            CssClass="btn-approve"
            OnClientClick="return confirm('Are you sure you want to APPROVE this client?');" />

        &nbsp;

        <asp:Button ID="btnReject" runat="server"
            Text="Reject"
            CommandName="Reject"
            CommandArgument='<%# Eval("ClientID") %>'
            CssClass="btn-reject"
            OnClientClick="return confirm('Are you sure you want to REJECT this client?');" />
    </ItemTemplate>
</asp:TemplateField>

            </Columns>
        </asp:GridView>
    </div>

</asp:Content>