<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="SystemChanges.aspx.cs" Inherits="RRCManagementSystem.SystemChanges" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .card-container {
            max-width: 700px;
            margin: 30px auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            margin-bottom: 30px;
        }

        .card-header {
            font-size: 22px;
            font-weight: bold;
            color: #1f2937;
            margin-bottom: 25px;
            border-bottom: 2px solid #e5e7eb;
            padding-bottom: 10px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-label {
            font-weight: bold;
            color: #333;
            margin-bottom: 8px;
            display: block;
        }

        .form-control {
            width: 100%;
            padding: 10px 15px;
            border-radius: 6px;
            border: 1px solid #ccc;
            font-size: 16px;
        }

        .btn-save {
            background-color: #1d4ed8;
            color: white;
            padding: 12px 25px;
            font-size: 16px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

        .btn-save:hover {
            background-color: #1e40af;
        }

        .message {
            text-align: center;
            font-weight: bold;
            margin-top: 15px;
            font-size: 16px;
        }
    </style>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <!-- 🧪 Chemical Usage Settings -->
    <div class="card-container">
        <div class="card-header">🧪 Chemical Usage Based on SQM</div>

        <div class="form-group">
            <label class="form-label">Usage for 0–100 sqm (mL)</label>
            <asp:TextBox ID="txtUsage100" runat="server" CssClass="form-control" TextMode="Number" />
        </div>

        <div class="form-group">
            <label class="form-label">Usage for 101–200 sqm (mL)</label>
            <asp:TextBox ID="txtUsage200" runat="server" CssClass="form-control" TextMode="Number" />
        </div>

        <div class="form-group">
            <label class="form-label">Usage for 201+ sqm (mL)</label>
            <asp:TextBox ID="txtUsage200Plus" runat="server" CssClass="form-control" TextMode="Number" />
        </div>
    </div>

    <!-- 📅 Max Inspections Settings -->
    <div class="card-container">
        <div class="card-header">📅 Max Inspections Per Day</div>

        <div class="form-group">
            <label class="form-label">Max Inspections per Day</label>
            <asp:TextBox ID="txtMaxInspections" runat="server" CssClass="form-control" TextMode="Number" />
        </div>

        <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn-save" OnClick="btnSave_Click" />
        <asp:Label ID="lblMessage" runat="server" CssClass="message" />
    </div>
</asp:Content>