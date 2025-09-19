<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllEmployee.aspx.cs" Inherits="RRCManagementSystem.AllEmployee" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
    </asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-8 px-4">
        <div class="bg-white rounded-xl shadow-lg overflow-hidden border border-gray-200">
            <div class="bg-blue-800 text-white text-center font-bold p-5">
                Active Employees
            </div>
            <div class="p-6">
                <div class="overflow-x-auto rounded-lg border border-gray-200 shadow-sm">
                    <asp:GridView ID="gvEmployees" runat="server" AutoGenerateColumns="False"
                        CssClass="min-w-full divide-y divide-gray-200 border-collapse"
                        AllowPaging="True" PageSize="10"
                        DataKeyNames="EmployeeID"
                        OnRowCommand="gvEmployees_RowCommand"
                        OnPageIndexChanging="gvEmployees_PageIndexChanging"
                        OnRowDataBound="gvEmployees_RowDataBound">
                        <HeaderStyle CssClass="bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                        <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors" />
                        <PagerStyle CssClass="bg-white text-gray-500 font-medium py-3 px-4 flex justify-between items-center" />
                        <PagerSettings Mode="NumericFirstLast" />
                        <EmptyDataTemplate>
                            <div class="py-4 px-6 text-center text-sm text-gray-500">
                                No active employees found.
                            </div>
                        </EmptyDataTemplate>
                        <Columns>
                            <asp:BoundField DataField="EmployeeID" HeaderText="ID" ItemStyle-Width="50px" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200" />
                            <asp:BoundField DataField="LastName" HeaderText="Last Name" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200" />
                            <asp:BoundField DataField="FirstName" HeaderText="First Name" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200" />
                            <asp:BoundField DataField="MiddleName" HeaderText="Middle Name" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200" />
                            <asp:BoundField DataField="Email" HeaderText="Email" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
                            <asp:BoundField DataField="Position" HeaderText="Position" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
                            <asp:BoundField DataField="Phone" HeaderText="Phone" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
                            <asp:ImageField DataImageUrlField="ProfileImage" HeaderText="Profile" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200">
                                <ControlStyle CssClass="h-12 w-12 rounded-full object-cover shadow-sm" />
                            </asp:ImageField>
                            <asp:TemplateField HeaderText="Actions" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                <ItemTemplate>
                                    <div class="flex items-center space-x-2">
                                        <asp:LinkButton ID="btnEdit" runat="server"
                                            CommandName="EditEmployee"
                                            CommandArgument='<%# Eval("EmployeeID") %>'
                                            CssClass="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors">
                                            <i class="fas fa-edit mr-2"></i>Edit
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnArchive" runat="server"
                                            CssClass="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
                                            OnClientClick='<%# "return confirmArchive(" + Eval("EmployeeID") + ");" %>'>
                                            <i class="fas fa-archive mr-2"></i>Archive
                                        </asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
    
    <asp:HiddenField ID="hfEmployeeToArchive" runat="server" />
    <asp:Button ID="btnHiddenArchive" runat="server" Style="display:none;" OnClick="btnHiddenArchive_Click" />

    <script type="text/javascript">
        function confirmArchive(employeeId) {
            event.preventDefault();
            Swal.fire({
                title: 'Archive Employee?',
                text: 'This employee will be archived and removed from the active list.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc2626', // Tailwind's red-600
                cancelButtonColor: '#4b5563', // Tailwind's gray-600
                confirmButtonText: 'Yes, archive it'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Trigger the WebForms postback for the hidden button
                    document.getElementById('<%= hfEmployeeToArchive.ClientID %>').value = employeeId;
                    document.getElementById('<%= btnHiddenArchive.ClientID %>').click();
                }
            });
            return false;
        }
    </script>
</asp:Content>