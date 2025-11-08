<%@ Page Title="Archived Inspection Reports" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="ArchivedInspectionDetails.aspx.cs"
    Inherits="RRCManagementSystem.ArchivedInspectionDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageTitle" runat="server">
    Archived Inspection Reports
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-7xl mx-auto bg-white rounded-xl shadow-lg p-6 mt-6 mb-12">
        <h2 class="text-2xl font-bold text-blue-700 mb-6 flex items-center">
            <i class="fa-solid fa-box-archive mr-2"></i> Archived Inspection Reports
        </h2>

        <asp:GridView ID="gvArchived" runat="server" AutoGenerateColumns="False"
            CssClass="min-w-full border rounded-lg overflow-hidden text-sm" DataKeyNames="ReportID"
            OnRowCommand="gvArchived_RowCommand">
            <HeaderStyle CssClass="bg-blue-600 text-white text-left" />
            <RowStyle CssClass="border-b hover:bg-blue-50 transition" />
            <Columns>
                <asp:BoundField DataField="ReportID" HeaderText="Report ID" />
                <asp:BoundField DataField="QuotationCode" HeaderText="Quotation Code" />
                <asp:BoundField DataField="InquiryCode" HeaderText="Inquiry Code" />
                <asp:BoundField DataField="InspectorName" HeaderText="Inspector" />
                <asp:BoundField DataField="TotalEstimatedCost" HeaderText="Total Cost" DataFormatString="₱{0:N2}" />
                <asp:BoundField DataField="UpdatedAt" HeaderText="Archived On" DataFormatString="{0:MM/dd/yyyy}" />
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <div class="flex gap-3">
                            <a href='InspectionDetails.aspx?ReportID=<%# Eval("ReportID") %>'
                               class="text-blue-600 hover:underline font-semibold">View</a>

                            <asp:LinkButton ID="btnRestore" runat="server" CommandName="Restore"
                                CommandArgument='<%# Eval("ReportID") %>'
                                CssClass="text-green-600 hover:underline font-semibold"
                                OnClientClick="return confirmRestore(event);">Restore</asp:LinkButton>

                            <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteReport"
                                CommandArgument='<%# Eval("ReportID") %>'
                                CssClass="text-red-600 hover:underline font-semibold"
                                OnClientClick="return confirmDelete(event);">Delete</asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="lblNoData" runat="server" CssClass="text-center text-gray-600 mt-6 block"
            Visible="false" Text="No archived reports found."></asp:Label>
    </div>

    <script>
        function confirmRestore(e) {
            e.preventDefault();
            const link = e.target.closest("a,button,input");
            Swal.fire({
                title: 'Restore this report?',
                text: "This will move it back to Submitted status.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#16a34a',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Yes, Restore it!'
            }).then((result) => {
                if (result.isConfirmed) link.closest('form').submit();
            });
            return false;
        }

        function confirmDelete(e) {
            e.preventDefault();
            const link = e.target.closest("a,button,input");
            Swal.fire({
                title: 'Delete permanently?',
                text: "This action cannot be undone!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc2626',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Yes, Delete it!'
            }).then((result) => {
                if (result.isConfirmed) link.closest('form').submit();
            });
            return false;
        }
    </script>
</asp:Content>
