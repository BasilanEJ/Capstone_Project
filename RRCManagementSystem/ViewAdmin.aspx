<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ViewAdmin.aspx.cs" Inherits="RRCManagementSystem.ViewAdmin" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-5">
        <div class="card shadow">
            <div class="card-body">
                <h2 class="text-center mb-4 text-primary">User Accounts</h2>

                <!-- Search Box -->
                <div class="d-flex justify-content-end mb-3 gap-2 flex-wrap">
                    <div class="position-relative w-100" style="max-width:400px;">
                        <i class="fas fa-search position-absolute top-50 start-0 translate-middle-y ps-3 text-muted"></i>
                        <asp:TextBox 
                            ID="txtSearch" 
                            runat="server"
                            placeholder="Search by name, email, or Employee ID..."
                            CssClass="form-control ps-5"
                            onkeyup="filterAdmins()" />
                    </div>
                </div>

                <!-- Table -->
                    <div class="table-responsive">
                        <asp:GridView 
                            ID="gvAdmins" 
                            runat="server" 
                            CssClass="table table-bordered table-hover text-center align-middle"
                            AutoGenerateColumns="False"
                            EmptyDataText="No users found."
                            DataKeyNames="UserID"
                            OnRowCommand="gvAdmins_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="UserID" HeaderText="UserID" Visible="False" />
                                <asp:BoundField DataField="EmployeeID" HeaderText="Employee ID" 
                                    ItemStyle-CssClass="admin-empid" />
                                <asp:BoundField DataField="Name" HeaderText="Name" 
                                    ItemStyle-CssClass="admin-name" />
                                <asp:BoundField DataField="Email" HeaderText="Email" 
                                    ItemStyle-CssClass="admin-email" />
                                <asp:BoundField DataField="Role" HeaderText="Role" 
                                    ItemStyle-CssClass="admin-role" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <div class="d-flex justify-content-center gap-2">
                                            <asp:LinkButton ID="btnEdit" runat="server"
                                                Text="Edit"
                                                CommandName="EditAdmin"
                                                CommandArgument='<%# Eval("UserID") %>'
                                                CssClass="btn btn-sm btn-primary" />

                                            <asp:LinkButton ID="btnDelete" runat="server"
                                                Text="Archive"
                                                CssClass="btn btn-sm btn-danger"
                                                OnClientClick='<%# "return showArchiveConfirmation(" + Eval("UserID") + ", \u0027" + Eval("EmployeeID") + "\u0027);" %>' />
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-bold mt-3 d-block text-center" />
            </div>
        </div>
    </div>

    <asp:HiddenField ID="hfUserToArchive" runat="server" />
    <asp:Button ID="btnConfirmArchive" runat="server" Style="display:none;" OnClick="btnConfirmArchive_Click" />

    <!-- ✅ JavaScript Section -->
    <script type="text/javascript">
        function showArchiveConfirmation(userId, employeeId) {
            if (window.event) window.event.preventDefault();

            Swal.fire({
                title: 'Are you sure?',
                html: 'This will archive user account:<br/><strong>' + employeeId + '</strong>',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Yes, archive it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    setTimeout(function () {
                        document.getElementById('<%= hfUserToArchive.ClientID %>').value = userId;
                        document.getElementById('<%= btnConfirmArchive.ClientID %>').click();
                    }, 50);
                }
            });

            return false;
        }

        // ✅ Instant Search Filter (Client-Side Only)
        function filterAdmins() {
            var input = document.getElementById('<%= txtSearch.ClientID %>');
            var filter = input.value.toLowerCase().trim();
            var rows = document.querySelectorAll('#<%= gvAdmins.ClientID %> tr');
            var visibleCount = 0;

            rows.forEach(function (row, index) {
                // Skip header row
                if (index === 0) return;

                var empid = row.querySelector('.admin-empid');
                var name = row.querySelector('.admin-name');
                var email = row.querySelector('.admin-email');
                var role = row.querySelector('.admin-role');

                if (empid && name && email && role) {
                    var text = (empid.textContent + name.textContent + email.textContent + role.textContent).toLowerCase();
                    if (text.includes(filter)) {
                        row.style.display = '';
                        visibleCount++;
                    } else {
                        row.style.display = 'none';
                    }
                }
            });

            // Show message if no match
            var msg = document.getElementById('noResultsMessage');
            if (visibleCount === 0 && filter !== '') {
                if (!msg) {
                    msg = document.createElement('div');
                    msg.id = 'noResultsMessage';
                    msg.className = 'text-center text-muted py-3';
                    msg.innerHTML = '<i class="fas fa-search me-2"></i>No users found matching your search.';
                    document.querySelector('.table-responsive').appendChild(msg);
                }
                msg.style.display = 'block';
            } else if (msg) {
                msg.style.display = 'none';
            }
        }
    </script>
</asp:Content>
