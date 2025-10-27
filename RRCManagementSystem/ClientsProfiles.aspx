<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ClientsProfiles.aspx.cs" Inherits="RRCManagementSystem.ClientsProfiles" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-8 px-4">
        <div class="bg-white rounded-xl shadow-lg overflow-hidden border border-gray-200">
            <div class="bg-blue-800 text-white text-center font-bold p-5">
                Approved Client Profiles
            </div>

            <div class="p-6">
                <div class="flex flex-col md:flex-row items-center justify-between mb-6 space-y-4 md:space-y-0">
                    <h2 class="text-2xl font-bold text-gray-800">Client Profiles</h2>
                </div>

                <!-- Search Bar Section -->
                <div class="mb-6 bg-gray-50 p-4 rounded-lg border border-gray-200">
                    <div class="max-w-md">
                        <label for="txtSearch" class="block text-sm font-medium text-gray-700 mb-2">
                            Search Clients
                        </label>
                        <div class="relative">
                            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                <i class="fas fa-search text-gray-400"></i>
                            </div>
                            <asp:TextBox ID="txtSearch" runat="server" 
                                placeholder="Search by name or email..." 
                                CssClass="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                                onkeyup="filterTable()" />
                        </div>
                        <p class="mt-1 text-xs text-gray-500">
                            Start typing to filter results automatically
                        </p>
                    </div>
                </div>

                <div class="overflow-x-auto rounded-lg border border-gray-200 shadow-sm">
                   <asp:GridView ID="gvClients" runat="server" AutoGenerateColumns="False"
    CssClass="min-w-full divide-y divide-gray-200 border-collapse"
    AllowPaging="True" PageSize="10"
    OnPageIndexChanging="gvClients_PageIndexChanging"
    OnRowCommand="gvClients_RowCommand"
    PagerStyle-CssClass="pagination-container"
    PagerSettings-Mode="NumericFirstLast"
    PagerSettings-Position="Bottom"
    PagerSettings-PageButtonCount="5"
    PagerSettings-FirstPageText="<i class='fas fa-angle-double-left'></i>"
    PagerSettings-LastPageText="<i class='fas fa-angle-double-right'></i>"
    PagerSettings-PreviousPageText="<i class='fas fa-angle-left'></i>"
    PagerSettings-NextPageText="<i class='fas fa-angle-right'></i>">

    <HeaderStyle CssClass="bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
    <RowStyle CssClass="bg-white even:bg-gray-50 hover:bg-gray-100 transition-colors client-row" />

    <EmptyDataTemplate>
        <div class="py-4 px-6 text-center text-sm text-gray-500">
            No client profiles found.
        </div>
    </EmptyDataTemplate>

    <Columns>
        <asp:BoundField DataField="ClientID" HeaderText="Client ID" ReadOnly="True" Visible="False" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 border-r border-gray-200" />

        <asp:BoundField DataField="ClientNumber" HeaderText="Client Number" ReadOnly="True" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm font-semibold text-blue-600 border-r border-gray-200" />

        <asp:TemplateField HeaderText="Name" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 border-r border-gray-200 client-name">
            <ItemTemplate>
                <%# Eval("LastName") %>, <%# Eval("FirstName") %> <%# Eval("MiddleName") %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:BoundField DataField="Email" HeaderText="Email" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200 client-email" />
        <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
        <asp:BoundField DataField="City" HeaderText="City" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />
        <asp:BoundField DataField="Country" HeaderText="Country" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 border-r border-gray-200" />

        <asp:TemplateField HeaderText="Actions" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
            <ItemTemplate>
                <div class="flex items-center space-x-2">
                    <asp:Button ID="btnResendEmail" runat="server"
                        CssClass="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-yellow-500 hover:bg-yellow-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-yellow-400 transition-colors"
                        Text="Resend Email"
                        CommandName="ResendEmail"
                        CommandArgument='<%# Eval("ClientID") %>' />

                    <asp:Button ID="btnView" runat="server"
                        CssClass="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                        Text="View Profile"
                        CommandName="ViewProfile"
                        CommandArgument='<%# Eval("ClientID") %>' />

                    <asp:Button ID="btnArchive" runat="server"
                        CssClass="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
                        Text="Archive"
                        CommandName="ArchiveClient"
                        CommandArgument='<%# Eval("ClientID") %>'
                        UseSubmitBehavior="false"
                        OnClientClick="return confirmArchive(this);" />
                </div>
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- SAME PAGINATION STYLE AS AllBooking.aspx -->
    <style>
        .pagination-container {
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 1rem;
            border-top: 1px solid #e5e7eb;
            background-color: #f9fafb;
            border-bottom-left-radius: 0.5rem;
            border-bottom-right-radius: 0.5rem;
        }

        .pagination-container table {
            border-collapse: separate;
            border-spacing: 0.25rem;
        }

        .pagination-container td {
            padding: 0;
        }

        .pagination-container a,
        .pagination-container span {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 2.5rem;
            height: 2.5rem;
            padding: 0.5rem 0.75rem;
            font-size: 0.875rem;
            font-weight: 500;
            border-radius: 0.375rem;
            transition: all 0.2s ease;
            text-decoration: none;
        }

        .pagination-container a {
            background-color: white;
            color: #374151;
            border: 1px solid #d1d5db;
        }

        .pagination-container a:hover {
            background-color: #3b82f6;
            color: white;
            border-color: #3b82f6;
            transform: translateY(-1px);
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .pagination-container span {
            background-color: #3b82f6;
            color: white;
            border: 1px solid #3b82f6;
            font-weight: 600;
            box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
        }

        .pagination-container a:first-child,
        .pagination-container a:last-child {
            background-color: #f3f4f6;
            border-color: #d1d5db;
        }

        .pagination-container a:first-child:hover,
        .pagination-container a:last-child:hover {
            background-color: #2563eb;
            border-color: #2563eb;
        }

        .pagination-container i {
            font-size: 1rem;
        }

        @media (max-width: 640px) {
            .pagination-container {
                padding: 0.75rem 0.5rem;
            }

            .pagination-container a,
            .pagination-container span {
                min-width: 2rem;
                height: 2rem;
                padding: 0.375rem 0.5rem;
                font-size: 0.75rem;
            }
        }
    </style>

    <script type="text/javascript">
        function confirmArchive(btn) {
            if (window.event) window.event.preventDefault();

            Swal.fire({
                title: 'Archive Client?',
                text: 'The client will be archived and no longer appear in the active list.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#2563eb',
                cancelButtonColor: '#dc2626',
                confirmButtonText: 'Yes, archive'
            }).then((result) => {
                if (result.isConfirmed) {
                    setTimeout(function () {
                        __doPostBack(btn.name, '');
                    }, 100);
                }
            });
            return false;
        }

        function filterTable() {
            var input = document.getElementById('<%= txtSearch.ClientID %>');
            var filter = input.value.toLowerCase().trim();
            var rows = document.querySelectorAll('.client-row');
            var visibleCount = 0;

            rows.forEach(function(row) {
                var clientName = row.querySelector('.client-name');
                var clientEmail = row.querySelector('.client-email');

                if (clientName && clientEmail) {
                    var name = clientName.textContent.toLowerCase();
                    var email = clientEmail.textContent.toLowerCase();

                    if (name.includes(filter) || email.includes(filter)) {
                        row.style.display = '';
                        visibleCount++;
                    } else {
                        row.style.display = 'none';
                    }
                }
            });

            // Show message if no results
            var gridView = document.getElementById('<%= gvClients.ClientID %>');
            var noResultsMsg = document.getElementById('noResultsMessage');
            
            if (visibleCount === 0 && filter !== '') {
                if (!noResultsMsg) {
                    noResultsMsg = document.createElement('div');
                    noResultsMsg.id = 'noResultsMessage';
                    noResultsMsg.className = 'py-4 px-6 text-center text-sm text-gray-500 bg-yellow-50 border-t border-yellow-200';
                    noResultsMsg.innerHTML = '<i class="fas fa-search mr-2"></i>No clients found matching your search.';
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