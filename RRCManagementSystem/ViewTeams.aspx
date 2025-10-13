<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewTeams.aspx.cs" Inherits="RRCManagementSystem.ViewTeams" %>
<%@ Import Namespace="System.Data" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- SweetAlert2 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11.7.32/dist/sweetalert2.min.css" rel="stylesheet">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-10 px-4">
        <h2 class="text-center text-3xl font-extrabold text-blue-800 mb-8">Teams and Members Overview</h2>

        <div class="mb-8 flex flex-col md:flex-row items-stretch md:items-end justify-center gap-4">
            <div class="w-full md:w-auto">
                <label for="<%= txtDate.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2 md:mb-0">Check Availability Date:</label>
                <asp:TextBox ID="txtDate" runat="server" TextMode="Date" 
                    CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors" />
            </div>
            <div class="w-full md:w-auto">
                <asp:Button ID="btnFilterDate" runat="server" Text="Check Availability"
                    CssClass="w-full md:w-auto px-6 py-2 bg-blue-600 text-white font-bold rounded-lg shadow-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors" 
                    OnClick="btnFilterDate_Click" />
            </div>
        </div>

        <asp:Repeater ID="rptTeams" runat="server" OnItemDataBound="rptTeams_ItemDataBound">
            <ItemTemplate>
                <div class="bg-white p-6 rounded-2xl shadow-xl border border-gray-200 mb-6">
                    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-4 border-b pb-4">
                        <h4 class="text-2xl font-bold text-blue-700 mb-2 sm:mb-0">
                            Team: <%# Eval("GroupName") %>
                        </h4>
                        <div class="flex items-center gap-3">
                            <span class='inline-block px-4 py-1.5 text-xs font-bold leading-none rounded-full 
                                <%# Eval("Status").ToString() == "Available" ? "bg-green-500 text-white" : "bg-red-500 text-white" %>'>
                                <%# Eval("Status") %>
                            </span>
                            
                            <%-- Placeholder to conditionally show the Delete Button --%>
                            <asp:PlaceHolder ID="phDeleteTeam" runat="server" Visible="false">
                                <asp:Button ID="btnDeleteTeam" runat="server" 
                                    Text="Delete Team" 
                                    CommandArgument='<%# Eval("TeamID") %>'
                                    CssClass="px-4 py-1.5 bg-red-600 text-white font-bold rounded-lg shadow-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
                                    OnClientClick="return confirmDelete(this, event);"
                                    OnClick="btnDeleteTeam_Click" />
                            </asp:PlaceHolder>
                        </div>
                    </div>

                    <div>
                        <ul class="divide-y divide-gray-200">
                            <asp:Repeater ID="rptEmployees" runat="server">
                                <ItemTemplate>
                                    <li class="py-2 flex items-center justify-between">
                                        <div class="flex items-center space-x-3">
                                            <div class="flex-shrink-0 h-8 w-8 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 font-bold">
                                                <%# Eval("FirstName").ToString().Substring(0, 1) + Eval("LastName").ToString().Substring(0, 1) %>
                                            </div>
                                            <p class="text-sm font-medium text-gray-900">
                                                <%# Eval("LastName") %>, <%# Eval("FirstName") %> <%# string.IsNullOrEmpty(Eval("MiddleName") as string) ? "" : Eval("MiddleName") %>
                                            </p>
                                        </div>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>

                            <asp:PlaceHolder ID="phNoMembers" runat="server">
                                <li class="py-2 text-center text-sm italic text-gray-500">
                                    No members assigned to this team.
                                </li>
                            </asp:PlaceHolder>
                        </ul>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Label ID="lblMessage" runat="server" CssClass="block text-center text-xl font-bold text-gray-500 mt-8" Visible="false" />
        
        <!-- Hidden field to store delete confirmation -->
        <asp:HiddenField ID="hdnConfirmDelete" runat="server" Value="false" />
        <asp:HiddenField ID="hdnTeamIdToDelete" runat="server" Value="" />
    </div>

    <!-- SweetAlert2 JS -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11.7.32/dist/sweetalert2.all.min.js"></script>
    
    <script type="text/javascript">
        function confirmDelete(button, event) {
            event.preventDefault();
            
            var teamId = button.getAttribute('commandargument');
            
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to delete this empty team? This action cannot be undone!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc2626',
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Yes, delete it!',
                cancelButtonText: 'Cancel',
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    // Set hidden fields
                    document.getElementById('<%= hdnConfirmDelete.ClientID %>').value = 'true';
                    document.getElementById('<%= hdnTeamIdToDelete.ClientID %>').value = teamId;
                    
                    // Trigger the actual delete
                    __doPostBack(button.name, '');
                }
            });
            
            return false;
        }

        // Function to show success alert from code-behind
        function showSuccessAlert(message) {
            Swal.fire({
                title: 'Success!',
                text: message,
                icon: 'success',
                confirmButtonColor: '#2563eb',
                confirmButtonText: 'OK'
            });
        }

        // Function to show error alert from code-behind
        function showErrorAlert(message) {
            Swal.fire({
                title: 'Error!',
                text: message,
                icon: 'error',
                confirmButtonColor: '#dc2626',
                confirmButtonText: 'OK'
            });
        }

        // Function to show warning alert from code-behind
        function showWarningAlert(message) {
            Swal.fire({
                title: 'Warning!',
                text: message,
                icon: 'warning',
                confirmButtonColor: '#f59e0b',
                confirmButtonText: 'OK'
            });
        }

        // Function to show info alert from code-behind
        function showInfoAlert(message) {
            Swal.fire({
                title: 'Info',
                text: message,
                icon: 'info',
                confirmButtonColor: '#3b82f6',
                confirmButtonText: 'OK'
            });
        }
    </script>
</asp:Content>