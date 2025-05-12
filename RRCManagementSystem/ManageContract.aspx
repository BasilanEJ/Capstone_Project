<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageContract.aspx.cs" Inherits="RRCManagementSystem.ManageContract" %>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .contract-form {
            max-width: 700px;
            margin: 0 auto;
            background: #fff;
            padding: 30px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            border-radius: 10px;
        }

        .contract-form h2 {
            text-align: center;
            margin-bottom: 25px;
            color: #004085;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-label {
            font-weight: 600;
            display: block;
            margin-bottom: 5px;
        }

        .form-control {
            width: 100%;
            padding: 10px;
            border-radius: 6px;
            border: 1px solid #ccc;
        }

        .btn-submit {
            background-color: #004085;
            color: #fff;
            padding: 10px 25px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: bold;
        }

        .btn-submit:hover {
            background-color: #002f6c;
        }

        .message {
            margin-top: 15px;
            font-weight: bold;
            color: green;
            text-align: center;
        }

        .message.error {
            color: red;
        }
    </style>

    <div class="contract-form">
        <h2>Upload New Client Contract</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="message" />

        <div class="form-group">
            <label class="form-label">Client</label>
            <asp:DropDownList ID="ddlClients" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group">
            <label class="form-label">Contract File (PDF Only)</label>
            <asp:FileUpload ID="fuContract" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group">
            <label class="form-label">Start Date</label>
            <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date" />
        </div>

        <div class="form-group">
            <label class="form-label">End Date</label>
            <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date" />
        </div>

        <div class="form-group">
            <label class="form-label">Remarks</label>
            <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
        </div>

       <asp:Button ID="btnUpload" runat="server" Text="Upload Contract"
    CssClass="btn-submit" OnClick="btnUpload_Click"
    OnClientClick="return confirm('Are you sure you want to upload this contract?');" />

    </div>
</asp:Content>