<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ArchivedInquiries.aspx.cs" Inherits="RRCManagementSystem.ArchivedInquiries" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    <style>
        .table-rounded-corners {
            border-collapse: separate;
            border-spacing: 0;
            border-radius: 0.5rem; /* rounded-lg */
            overflow: hidden;
        }

        .table-rounded-corners thead tr:first-child th:first-child {
            border-top-left-radius: 0.5rem;
        }

        .table-rounded-corners thead tr:first-child th:last-child {
            border-top-right-radius: 0.5rem;
        }
    </style>

    <div class="container mx-auto p-4 md:p-8 bg-white rounded-lg shadow-lg mt-8">
        <h2 class="text-2xl font-bold mb-6 text-gray-800 text-center">🗄 Archived Inquiries</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="text-red-500 font-semibold text-center block mb-4" />

        <asp:HiddenField ID="hfActionInquiryID" runat="server" />
        <asp:Button ID="btnRestoreHidden" runat="server" Style="display:none;" OnClick="btnRestoreHidden_Click" />
        <asp:Button ID="btnDeleteHidden" runat="server" Style="display:none;" OnClick="btnDeleteHidden_Click" />

        <div class="overflow-x-auto shadow-lg rounded-lg">
            <asp:GridView ID="gvArchived" runat="server" AutoGenerateColumns="False"
                CssClass="min-w-full bg-white table-rounded-corners"
                HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors"
                AlternatingRowStyle-CssClass="bg-gray-50 hover:bg-gray-100 transition-colors">
                <Columns>
                    <asp:BoundField DataField="InquiryCode" HeaderText="Reference Code" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="Email" HeaderText="Client Email" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />
                    <asp:BoundField DataField="SubmittedAt" HeaderText="Submitted At" DataFormatString="{0:g}" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />

                    <asp:TemplateField HeaderText="Restore" HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <button type="button" class="px-4 py-2 bg-green-500 text-white font-bold rounded-md hover:bg-green-600 transition-colors text-sm"
                                onclick='confirmRestore(<%# Eval("InquiryID") %>)'>
                                Restore
                            </button>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Delete" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <button type="button" class="px-4 py-2 bg-red-500 text-white font-bold rounded-md hover:bg-red-600 transition-colors text-sm"
                                onclick='confirmDelete(<%# Eval("InquiryID") %>)'>
                                Delete
                            </button>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <script type="text/javascript">
        // Restore Confirmation
        function confirmRestore(inquiryId) {
            Swal.fire({
                title: 'Restore this inquiry?',
                text: "The inquiry will be moved back to the active list.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, restore it',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#10b981',
                cancelButtonColor: '#6b7280'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfActionInquiryID.ClientID %>').value = inquiryId;
                    document.getElementById('<%= btnRestoreHidden.ClientID %>').click();
                }
            });
        }

        // Delete Confirmation
        function confirmDelete(inquiryId) {
            Swal.fire({
                title: 'Permanently delete this inquiry?',
                text: "This action cannot be undone!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, delete it',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#dc2626',
                cancelButtonColor: '#6b7280'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfActionInquiryID.ClientID %>').value = inquiryId;
                    document.getElementById('<%= btnDeleteHidden.ClientID %>').click();
                }
            });
        }
    </script>
</asp:Content>