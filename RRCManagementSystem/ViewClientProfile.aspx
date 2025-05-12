<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewClientProfile.aspx.cs" Inherits="RRCManagementSystem.ViewClientProfile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .container {
            max-width: 1000px;
            margin: 30px auto;
            padding: 20px;
        }

        .card {
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            background-color: #fff;
            margin-bottom: 25px;
        }

        .card-header {
            background-color: #0073CF;
            color: #fff;
            padding: 15px;
            font-size: 20px;
            font-weight: bold;
        }

        .card-body {
            padding: 20px;
        }

        .profile-field {
            margin-bottom: 12px;
            font-size: 16px;
        }

        .profile-label {
            font-weight: bold;
            margin-right: 8px;
        }

        .history-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }

        .history-table th, .history-table td {
            border: 1px solid #ddd;
            padding: 10px;
            text-align: center;
        }

        .history-table th {
            background-color: #0073CF;
            color: white;
        }

        .no-history {
            color: #777;
            font-style: italic;
            margin-top: 10px;
        }

        .btn-back {
            background-color: #6c757d;
            color: white;
            padding: 10px 18px;
            border: none;
            border-radius: 6px;
            font-size: 14px;
            cursor: pointer;
            margin-bottom: 20px;
            transition: background-color 0.3s ease;
        }

        .btn-back:hover {
            background-color: #5a6268;
        }
    </style>

    <div class="container">
        <div class="card">
            <div class="card-header">Client Profile</div>
            <div class="card-body">
                <asp:Label ID="lblName" runat="server" CssClass="profile-field"></asp:Label><br />
                <asp:Label ID="lblEmail" runat="server" CssClass="profile-field"></asp:Label><br />
                <asp:Label ID="lblContact" runat="server" CssClass="profile-field"></asp:Label><br />
                <asp:Label ID="lblCity" runat="server" CssClass="profile-field"></asp:Label><br />
                <asp:Label ID="lblRegion" runat="server" CssClass="profile-field"></asp:Label><br />
                <asp:Label ID="lblCountry" runat="server" CssClass="profile-field"></asp:Label><br />
                <asp:Label ID="lblStatus" runat="server" CssClass="profile-field"></asp:Label><br />
            </div>
        </div>

        <asp:Button ID="btnBack" runat="server" Text="← Back to Clients" CssClass="btn-back" OnClick="btnBack_Click" />

        <div class="card">
            <div class="card-header">Action History</div>
            <div class="card-body">
                <asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="False" CssClass="history-table">
                    <Columns>
                        <asp:BoundField DataField="ActionTaken" HeaderText="Action" />
                        <asp:BoundField DataField="PerformedBy" HeaderText="Performed By" />
                        <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
                        <asp:BoundField DataField="ActionDate" HeaderText="Date" DataFormatString="{0:g}" />
                    </Columns>
                </asp:GridView>
                <asp:Label ID="lblNoHistory" runat="server" CssClass="no-history" Visible="false" Text="No history records found."></asp:Label>
            </div>
        </div>
    </div>

</asp:Content>