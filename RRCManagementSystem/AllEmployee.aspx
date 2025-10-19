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
            
            <!-- Search Bar Section -->
            <div class="p-6 bg-gray-50 border-b border-gray-200">
                <div class="max-w-md">
                    <label for="txtSearch" class="block text-sm font-medium text-gray-700 mb-2">
                        Search Employees
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
                        <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors employee-row" />
                        <PagerStyle CssClass="bg-white text-gray-500 font-medium py-3 px-4 flex justify-between items-center" />
                        <PagerSettings Mode="NumericFirstLast" />
                        <EmptyDataTemplate>
                            <div class="py-4 px-6 text-center text-sm text-gray-500">
                                No active employees found.
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
                confirmButtonColor: '#dc2626',
                cancelButtonColor: '#4b5563',
                confirmButtonText: 'Yes, archive it'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfEmployeeToArchive.ClientID %>').value = employeeId;
                    document.getElementById('<%= btnHiddenArchive.ClientID %>').click();
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
            var gridView = document.getElementById('<%= gvEmployees.ClientID %>');
            var noResultsMsg = document.getElementById('noResultsMessage');
            
            if (visibleCount === 0 && filter !== '') {
                if (!noResultsMsg) {
                    noResultsMsg = document.createElement('div');
                    noResultsMsg.id = 'noResultsMessage';
                    noResultsMsg.className = 'py-4 px-6 text-center text-sm text-gray-500 bg-yellow-50 border-t border-yellow-200';
                    noResultsMsg.innerHTML = '<i class="fas fa-search mr-2"></i>No employees found matching your search.';
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