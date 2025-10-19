<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ArchiveEmployee.aspx.cs" Inherits="RRCManagementSystem.ArchiveEmployee" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-10 px-4">
        <div class="w-full bg-white p-8 rounded-2xl shadow-xl border border-gray-200">
            <h2 class="text-center text-3xl font-extrabold text-blue-800 mb-6">Archived Employees</h2>

            <div class="mb-6 bg-gray-50 p-4 rounded-lg border border-gray-200">
                <div class="max-w-md">
                    <label for="txtSearch" class="block text-sm font-medium text-gray-700 mb-2">
                        Search Archived Employees
                    </label>
                    <div class="relative">
                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <i class="fas fa-search text-gray-400"></i>
                        </div>
                        <asp:TextBox ID="txtSearch" runat="server" 
                            placeholder="Search by name or employee number..." 
                            CssClass="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                            onkeyup="filterTable()" />
                    </div>
                    <p class="mt-1 text-xs text-gray-500">
                        Start typing to filter results automatically
                    </p>
                </div>
            </div>

            <div class="overflow-x-auto">
                <asp:GridView ID="gvArchivedEmployees" runat="server" AutoGenerateColumns="False"
                    CssClass="min-w-full divide-y divide-gray-200 border-collapse"
                    AllowPaging="True" PageSize="10"
                    DataKeyNames="EmployeeID"
                    OnRowCommand="gvArchivedEmployees_RowCommand"
                    OnPageIndexChanging="gvArchivedEmployees_PageIndexChanging">
                    <HeaderStyle CssClass="bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                    <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors employee-row" />
                    <PagerStyle CssClass="bg-white text-gray-500 font-medium py-3 px-4 flex justify-between items-center" />
                    <PagerSettings Mode="NumericFirstLast" />
                    <EmptyDataTemplate>
                        <div class="py-4 px-6 text-center text-sm text-gray-500">
                            No archived employees found.
                        </div>
                    </EmptyDataTemplate>
                    <Columns>

                        <asp:BoundField DataField="EmployeeID" HeaderText="ID" Visible="false" />
                        

                        <asp:BoundField DataField="EmployeeNumber" HeaderText="Employee #" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm font-semibold text-blue-700 border-r border-gray-200 employee-number" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="LastName" HeaderText="Last Name" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200 employee-lastname" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="FirstName" HeaderText="First Name" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200 employee-firstname" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="MiddleName" HeaderText="Middle Name" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200 employee-middlename" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="Email" HeaderText="Email" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="Position" HeaderText="Position" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="Phone" HeaderText="Phone" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:ImageField DataImageUrlField="ProfileImage" HeaderText="Profile" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200"
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200">
                            <ControlStyle CssClass="h-12 w-12 rounded-full object-cover shadow-sm" />
                        </asp:ImageField>
                        
                        <asp:TemplateField HeaderText="Actions" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-right text-sm font-medium"
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            <ItemTemplate>
                                <div class="flex items-center space-x-2">
                                    <asp:LinkButton ID="btnRestore" runat="server"
                                        CommandName="Restore"
                                        CommandArgument='<%# Eval("EmployeeID") %>'
                                        CssClass="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 transition-colors"
                                        OnClientClick='<%# "return confirmRestore(" + Eval("EmployeeID") + ");" %>'>
                                        <i class="fas fa-undo mr-2"></i>Restore
                                    </asp:LinkButton>

                                    <asp:LinkButton ID="btnDelete" runat="server"
                                        CommandName="DeleteEmp"
                                        CommandArgument='<%# Eval("EmployeeID") %>'
                                        CssClass="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
                                        CausesValidation="false"
                                        OnClientClick='<%# "return confirmDelete(" + Eval("EmployeeID") + ");" %>'>
                                        <i class="fas fa-trash-alt mr-2"></i>Delete
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function confirmDelete(employeeId) {
            event.preventDefault();
            Swal.fire({
                title: 'Permanently Delete?',
                text: 'This employee record will be permanently deleted and cannot be recovered.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc2626',
                cancelButtonColor: '#4b5563',
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
                confirmButtonColor: '#16a34a',
                cancelButtonColor: '#4b5563',
                confirmButtonText: 'Yes, restore!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= gvArchivedEmployees.UniqueID %>', 'Restore$' + employeeId);
                }
            });
            return false;
        }

        function filterTable() {
            var input = document.getElementById('<%= txtSearch.ClientID %>');
            var filter = input.value.toLowerCase().trim();
            var rows = document.querySelectorAll('.employee-row');
            var visibleCount = 0;

            rows.forEach(function(row) {
                var employeeNumber = row.querySelector('.employee-number');
                var firstName = row.querySelector('.employee-firstname');
                var lastName = row.querySelector('.employee-lastname');
                var middleName = row.querySelector('.employee-middlename');

                if (employeeNumber && firstName && lastName) {
                    var empNum = employeeNumber.textContent.toLowerCase();
                    var fName = firstName.textContent.toLowerCase();
                    var lName = lastName.textContent.toLowerCase();
                    var mName = middleName ? middleName.textContent.toLowerCase() : '';
                    var fullName = fName + ' ' + mName + ' ' + lName;

                    if (empNum.includes(filter) || 
                        fName.includes(filter) || 
                        lName.includes(filter) || 
                        mName.includes(filter) ||
                        fullName.includes(filter)) {
                        row.style.display = '';
                        visibleCount++;
                    } else {
                        row.style.display = 'none';
                    }
                }
            });

            // Show message if no results
            var gridView = document.getElementById('<%= gvArchivedEmployees.ClientID %>');
            var noResultsMsg = document.getElementById('noResultsMessage');
            
            if (visibleCount === 0 && filter !== '') {
                if (!noResultsMsg) {
                    noResultsMsg = document.createElement('div');
                    noResultsMsg.id = 'noResultsMessage';
                    noResultsMsg.className = 'py-4 px-6 text-center text-sm text-gray-500 bg-yellow-50 border-t border-yellow-200';
                    noResultsMsg.innerHTML = '<i class="fas fa-search mr-2"></i>No archived employees found matching your search.';
                    gridView.parentElement.appendChild(noResultsMsg);
                }
                noResultsMsg.style.display = 'block';
            } else if (noResultsMsg) {
                noResultsMsg.style.display = 'none';
            }
        }

        // Clear search on page load if needed
        window.addEventListener('load', function() {
            var searchBox = document.getElementById('<%= txtSearch.ClientID %>');
            if (searchBox && searchBox.value === '') {
                filterTable();
            }
        });
    </script>
</asp:Content>