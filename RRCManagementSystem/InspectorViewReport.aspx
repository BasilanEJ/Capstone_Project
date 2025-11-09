<%@ Page Title="View Inspection Report" Language="C#" MasterPageFile="~/Inspector.master"
    AutoEventWireup="true" CodeBehind="InspectorViewReport.aspx.cs"
    Inherits="RRCManagementSystem.InspectorViewReport" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        .view-card {
            background: white;
            border-radius: 16px;
            padding: 28px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
            margin-bottom: 24px;
            border: 1px solid #e5e7eb;
        }
        .label-title {
            font-weight: 600;
            color: #374151;
        }
        .label-value {
            color: #1f2937;
            font-size: 1rem;
        }
        .section-header {
            font-size: 1.1rem;
            font-weight: 700;
            color: #2563eb;
            border-bottom: 2px solid #2563eb;
            margin-bottom: 12px;
            padding-bottom: 4px;
        }
        .photo-preview img {
            width: 120px;
            height: 120px;
            border-radius: 10px;
            object-fit: cover;
            margin: 5px;
            cursor: pointer;
            box-shadow: 0 2px 6px rgba(0,0,0,0.1);
        }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-6">
        <h2 class="text-2xl font-bold mb-6 text-blue-700">
            <i class="fas fa-file-alt"></i> Inspection Report (Read-Only)
        </h2>

        <!-- Client & Inquiry Info -->
        <div class="view-card">
            <div class="section-header">Client & Inquiry Details</div>
            <p><span class="label-title">Inquiry Number:</span>
                <span class="label-value" id="lblInquiryNumber" runat="server"></span></p>
            <p><span class="label-title">Client Name:</span>
                <span class="label-value" id="lblClientName" runat="server"></span></p>
            <p><span class="label-title">Contact:</span>
                <span class="label-value" id="lblClientContact" runat="server"></span></p>
            <p><span class="label-title">Address:</span>
                <span class="label-value" id="lblClientAddress" runat="server"></span></p>
            <p><span class="label-title">Pest Type:</span>
                <span class="label-value" id="lblPestType" runat="server"></span></p>
        </div>

        <!-- Report Details -->
        <div class="view-card">
            <div class="section-header">Inspection Report Details</div>
            <p><span class="label-title">Quotation Code:</span>
                <span class="label-value" id="lblQuotationCode" runat="server"></span></p>
            <p><span class="label-title">Infestation Level:</span>
                <span class="label-value" id="lblInfestationLevel" runat="server"></span></p>
            <p><span class="label-title">Findings:</span><br />
                <span class="label-value block whitespace-pre-line" id="lblFindings" runat="server"></span></p>
            <p><span class="label-title">Affected Areas:</span><br />
                <span class="label-value block whitespace-pre-line" id="lblAffectedAreas" runat="server"></span></p>
            <p><span class="label-title">Additional Notes:</span><br />
                <span class="label-value block whitespace-pre-line" id="lblAdditionalNotes" runat="server"></span></p>
        </div>

        <!-- Services -->
        <div class="view-card">
            <div class="section-header">Selected Services</div>
            <asp:Repeater ID="rptServices" runat="server">
                <ItemTemplate>
                    <div class="p-3 mb-2 bg-gray-50 rounded-md border-l-4 border-green-600">
                        <p><strong><%# Eval("ServiceName") %></strong></p>
                        <p>Area: <%# Eval("SQM") %> sqm</p>
                        <p>Flat Price: ₱<%# string.Format("{0:N2}", Eval("FlatPrice")) %></p>
                        <p><%# (bool)Eval("IsContract") ? "📄 Contractual Service" : "🕒 One-Time Service" %></p>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <!-- Expenses -->
        <div class="view-card">
            <div class="section-header">Expenses</div>
            <p><span class="label-title">Travel Expense:</span>
                <span class="label-value" id="lblTravelCost" runat="server"></span></p>
            <p><span class="label-title">Miscellaneous:</span>
                <span class="label-value" id="lblMiscExpenses" runat="server"></span></p>
            <p><span class="label-title">Grand Total:</span>
                <span class="label-value text-green-700 font-bold" id="lblGrandTotal" runat="server"></span></p>
        </div>

        <!-- Follow-Up -->
        <div class="view-card">
            <div class="section-header">Follow-Up</div>
            <p><span class="label-title">Required:</span>
                <span class="label-value" id="lblFollowupRequired" runat="server"></span></p>
            <p><span class="label-title">Date:</span>
                <span class="label-value" id="lblFollowupDate" runat="server"></span></p>
            <p><span class="label-title">Reason:</span>
                <span class="label-value" id="lblFollowupReason" runat="server"></span></p>
        </div>

        <!-- Photos -->
        <div class="view-card">
            <div class="section-header">Inspection Photos</div>
            <div class="photo-preview" id="photoContainer" runat="server"></div>
        </div>

        <div class="mt-6 text-right">
            <a href="MyInspections.aspx" class="btn btn-secondary px-4 py-2 rounded-md bg-gray-500 text-white hover:bg-gray-600">
                ← Back to My Inspections
            </a>
        </div>
    </div>
</asp:Content>
