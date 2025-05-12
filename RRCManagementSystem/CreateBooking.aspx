<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="CreateBooking.aspx.cs" Inherits="RRCManagementSystem.CreateBooking" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .card {
            max-width: 700px;
            margin: 20px auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }

        .card h2 {
            text-align: center;
            margin-bottom: 25px;
            color: #1f2937;
        }

        .form-label {
            font-weight: bold;
            display: block;
            margin-top: 10px;
            color: #333;
        }

        .form-control {
            width: 100%;
            padding: 10px;
            margin-bottom: 15px;
            border: 1px solid #ccc;
            border-radius: 6px;
            font-size: 16px;
        }

        .btn {
            background-color: #1d4ed8;
            color: white;
            padding: 12px 20px;
            border: none;
            border-radius: 6px;
            font-size: 16px;
            cursor: pointer;
            margin-right: 10px;
        }

        .btn:hover {
            background-color: #1e40af;
        }

        .message {
            margin-top: 15px;
            font-weight: bold;
            text-align: center;
            font-size: 16px;
        }
    </style>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <div class="card">
        <h2>🛠️ Create Booking Quotation</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="message" />

        <label class="form-label">Select Client:</label>
        <asp:DropDownList ID="ddlClients" runat="server" CssClass="form-control" />

        <label class="form-label">Select Services:</label>
        <asp:CheckBoxList ID="cblServices" runat="server" CssClass="form-control" />

        <label class="form-label">Square Meters (SQM):</label>
        <asp:TextBox ID="txtSQM" runat="server" CssClass="form-control" TextMode="Number" />

        <asp:Label ID="lblTotal" runat="server" CssClass="message" />

        <asp:Button ID="btnCalculate" runat="server" Text="Calculate Price" CssClass="btn" OnClick="btnCalculate_Click" />
        <asp:Button ID="btnSubmit" runat="server" Text="Submit Quotation" CssClass="btn" OnClick="btnSubmit_Click" />
    </div>
</asp:Content>