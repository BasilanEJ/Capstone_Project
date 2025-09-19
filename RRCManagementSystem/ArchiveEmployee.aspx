<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ArchiveEmployee.aspx.cs" Inherits="RRCManagementSystem.ArchiveEmployee" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
    </asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-10 px-4 flex justify-center items-start">
        <div class="w-full max-w-5xl bg-white p-8 rounded-2xl shadow-xl border border-gray-200">
            <h2 class="text-center text-3xl font-extrabold text-blue-800 mb-6">Archived Employees</h2>

            <div class="overflow-x-auto">
                <asp:GridView ID="gvArchivedEmployees" runat="server" AutoGenerateColumns="False"
                    CssClass="w-full text-left border-collapse border-separate border-spacing-0.5"
                    GridLines="None"
                    OnRowCommand="gvArchivedEmployees_RowCommand"
                    DataKeyNames="EmployeeID">
                    <Columns>
                        <asp:BoundField DataField="EmployeeID" HeaderText="ID" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                        <asp:BoundField DataField="LastName" HeaderText="Last Name" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                        <asp:BoundField DataField="FirstName" HeaderText="First Name" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                        <asp:BoundField DataField="MiddleName" HeaderText="Middle Name" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                        <asp:BoundField DataField="Position" HeaderText="Position" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                        <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200">
                            <ItemTemplate>
                                <div class="flex flex-col sm:flex-row justify-center items-center gap-2">
                                    <asp:LinkButton ID="btnRestore" runat="server"
                                        CommandName="Restore"
                                        CommandArgument='<%# Eval("EmployeeID") %>'
                                        CssClass="px-4 py-2 text-xs font-semibold rounded-lg text-white bg-green-600 hover:bg-green-700 transition-colors"
                                        OnClientClick='<%# "return confirmRestore(" + Eval("EmployeeID") + ");" %>'>
                                        <i class="fas fa-undo mr-1"></i>Restore
                                    </asp:LinkButton>

                                    <asp:LinkButton ID="btnDelete" runat="server"
                                        CommandName="DeleteEmp"
                                        CommandArgument='<%# Eval("EmployeeID") %>'
                                        CssClass="px-4 py-2 text-xs font-semibold rounded-lg text-white bg-red-600 hover:bg-red-700 transition-colors"
                                        CausesValidation="false"
                                        OnClientClick='<%# "return confirmDelete(" + Eval("EmployeeID") + ");" %>'>
                                        <i class="fas fa-trash-alt mr-1"></i>Delete
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            
            <asp:Label ID="lblNoData" runat="server" Text="No archived employees found." Visible="false" CssClass="block text-center text-lg font-medium text-gray-500 mt-8 mb-4" />
        </div>
    </div>

    <script type="text/javascript">
        // This function will check for rows and show the message if no data exists.
        function checkGridViewData() {
            var gv = document.getElementById('<%= gvArchivedEmployees.ClientID %>');
            var noDataLabel = document.getElementById('<%= lblNoData.ClientID %>');
            
            // Checks if the GridView has at least one data row (tr element with a data-row class or similar)
            // or if the NoRowsTemplate is rendered (which is usually a single row with specific text).
            var hasData = gv.rows.length > 1; // 1 for the header row

            if (!hasData) {
                noDataLabel.style.display = 'block';
                gv.style.display = 'none';
            } else {
                noDataLabel.style.display = 'none';
                gv.style.display = 'table';
            }
        }
        
        // Run the function on page load
        window.onload = checkGridViewData;
        
        function confirmDelete(employeeId) {
            event.preventDefault();
            Swal.fire({
                title: 'Permanently Delete?',
                text: 'This employee record will be permanently deleted and cannot be recovered.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc2626', // Tailwind's red-600
                cancelButtonColor: '#4b5563',  // Tailwind's gray-600
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= gvArchivedEmployees.UniqueID %>', 'DeleteEmp$' + employeeId);
                }
            });
            return false;
        }

        function confirmRestore(employeeId) {
            event.preventDefault();
            Swal.fire({
                title: 'Restore Employee?',
                text: 'This employee will be restored to the active list.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#16a34a', // Tailwind's green-600
                cancelButtonColor: '#4b5563',  // Tailwind's gray-600
                confirmButtonText: 'Yes, restore!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= gvArchivedEmployees.UniqueID %>', 'Restore$' + employeeId);
                }
            });
            return false;
        }
    </script>
</asp:Content>