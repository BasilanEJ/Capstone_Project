<%@ Page Title="All Inspection Reports" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="AllInspectionDetails.aspx.cs"
    Inherits="RRCManagementSystem.AllInspectionDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageTitle" runat="server">
    All Inspection Reports
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-7xl mx-auto bg-white rounded-xl shadow-lg p-6 mt-6 mb-12">
        <h2 class="text-2xl font-bold text-blue-700 mb-6 flex items-center">
            <i class="fa-solid fa-file-lines mr-2"></i> Submitted Inspection Reports
        </h2>

        <asp:GridView ID="gvReports" runat="server" AutoGenerateColumns="False"
            CssClass="min-w-full border rounded-lg overflow-hidden text-sm" 
            DataKeyNames="ReportID" OnRowCommand="gvReports_RowCommand">
            <HeaderStyle CssClass="bg-blue-600 text-white text-left" />
            <RowStyle CssClass="border-b hover:bg-blue-50 transition" />

            <Columns>
                <asp:BoundField DataField="ReportID" HeaderText="Report ID" />
                <asp:BoundField DataField="QuotationCode" HeaderText="Quotation Code" />
                <asp:BoundField DataField="InquiryNumber" HeaderText="Inquiry Code" />
                <asp:BoundField DataField="InspectorName" HeaderText="Inspector" />
                <asp:BoundField DataField="TotalEstimatedCost" HeaderText="Total Cost" DataFormatString="₱{0:N2}" />
                <asp:BoundField DataField="UpdatedAt" HeaderText="Submitted On" DataFormatString="{0:MM/dd/yyyy}" />
                <asp:TemplateField HeaderText="Action">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnView" runat="server" 
                            CommandName="ViewDetails" 
                            CommandArgument='<%# Eval("ReportID") %>'
                            CssClass="text-blue-600 hover:underline font-semibold">
                            View Details
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="lblNoData" runat="server" 
            CssClass="text-center text-gray-600 mt-6 block" 
            Visible="false"
            Text="No submitted inspection reports found."></asp:Label>
    </div>
</asp:Content>
