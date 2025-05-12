<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="OurContract.aspx.cs" Inherits="RRCManagementSystem.OurContract" %>


<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .contract-box {
            max-width: 700px;
            margin: 30px auto;
            background-color: #fff;
            padding: 30px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            border-radius: 10px;
        }

        .contract-box h2 {
            text-align: center;
            margin-bottom: 25px;
            color: #004085;
        }

        .info-label {
            font-weight: bold;
            color: #333;
        }

        .info-value {
            margin-bottom: 15px;
            color: #555;
        }

        .btn-download {
            background-color: #004085;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 6px;
            font-weight: bold;
            text-decoration: none;
        }

        .btn-download:hover {
            background-color: #002f6c;
        }

        .message {
            text-align: center;
            color: red;
            margin-bottom: 15px;
        }
    </style>

    <div class="contract-box">
        <h2>Your Contract</h2>
        <asp:Label ID="lblMessage" runat="server" CssClass="message" />

        <asp:Panel ID="pnlContract" runat="server" Visible="false">
            <p><span class="info-label">Start Date:</span> <asp:Label ID="lblStartDate" runat="server" CssClass="info-value" /></p>
            <p><span class="info-label">End Date:</span> <asp:Label ID="lblEndDate" runat="server" CssClass="info-value" /></p>
            <p><span class="info-label">Uploaded:</span> <asp:Label ID="lblUploaded" runat="server" CssClass="info-value" /></p>
            <p><span class="info-label">Remarks:</span> <asp:Label ID="lblRemarks" runat="server" CssClass="info-value" /></p>

            <asp:Button ID="btnDownload" runat="server" Text="Download Contract" CssClass="btn-download" OnClick="btnDownload_Click" />
        </asp:Panel>
    </div>
</asp:Content>