<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EmployeeStatus.aspx.cs" Inherits="RRCManagementSystem.EmployeeStatus" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-10 px-4">
        <div class="w-full bg-white p-8 rounded-2xl shadow-xl border border-gray-200">
            <h2 class="text-center text-3xl font-extrabold text-blue-800 mb-6">Employee Status</h2>
            
            <!-- Filter and Search Section -->
            <div class="mb-6 grid grid-cols-1 md:grid-cols-2 gap-4">
                <!-- Status Filter -->
                <div>
                    <label for="<%= ddlStatus.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Filter by Status:</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true"
                        CssClass="block w-full px-4 py-2 bg-white text-gray-900 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
                        OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged" />
                </div>

                <!-- Search Box -->
                <div>
                    <label for="txtSearch" class="block text-sm font-semibold text-gray-700 mb-2">Search Employees:</label>
                    <div class="relative">
                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <i class="fas fa-search text-gray-400"></i>
                        </div>
                        <asp:TextBox ID="txtSearch" runat="server" 
                            placeholder="Search by name or employee number..." 
                            CssClass="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                            onkeyup="filterTable()" />
                    </div>
                </div>
            </div>
            
            <div class="overflow-x-auto">
                <asp:GridView ID="gvEmployees" runat="server"
                    CssClass="min-w-full divide-y divide-gray-200 border-collapse"
                    AutoGenerateColumns="False"
                    AllowPaging="True" PageSize="10"
                    OnPageIndexChanging="gvEmployees_PageIndexChanging"
                    DataKeyNames="EmployeeID">
                    <HeaderStyle CssClass="bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                    <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors employee-row" />
                    <PagerStyle CssClass="bg-white text-gray-500 font-medium py-3 px-4 flex justify-between items-center" />
                    <PagerSettings Mode="NumericFirstLast" />
                    <EmptyDataTemplate>
                        <div class="py-4 px-6 text-center text-sm text-gray-500">
                            No employees found for this status.
                        </div>
                    </EmptyDataTemplate>
                    <Columns>

                        <asp:BoundField DataField="EmployeeID" HeaderText="ID" Visible="false" />
                        
 
                        <asp:BoundField DataField="EmployeeNumber" HeaderText="Employee #" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm font-semibold text-blue-700 border-r border-gray-200 employee-number" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="FullName" HeaderText="Full Name" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200 employee-fullname" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="Email" HeaderText="Email" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="Phone" HeaderText="Phone" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="Position" HeaderText="Position" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:BoundField DataField="Status" HeaderText="Status" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 font-semibold border-r border-gray-200" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" />
                        
                        <asp:ImageField DataImageUrlField="ProfileImage" HeaderText="Profile" 
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            <ControlStyle CssClass="h-12 w-12 rounded-full object-cover shadow-sm" />
                        </asp:ImageField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function filterTable() {
            var input = document.getElementById('<%= txtSearch.ClientID %>');
            var filter = input.value.toLowerCase().trim();
            var rows = document.querySelectorAll('.employee-row');
            var visibleCount = 0;

            rows.forEach(function(row) {
                var employeeNumber = row.querySelector('.employee-number');
                var fullName = row.querySelector('.employee-fullname');

                if (employeeNumber && fullName) {
                    var empNum = employeeNumber.textContent.toLowerCase();
                    var name = fullName.textContent.toLowerCase();

                    if (empNum.includes(filter) || name.includes(filter)) {
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