<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EmployeeStatus.aspx.cs" Inherits="RRCManagementSystem.EmployeeStatus" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-5" style="max-width:1100px;">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white text-center fs-4 fw-bold">
                Employee Status
            </div>
            <div class="card-body">

                <!-- Filter Dropdown -->
                <div class="mb-4">
                    <label for="ddlStatus" class="form-label fw-semibold">Filter by Status:</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" CssClass="form-select"
                        OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged" />
                </div>

                <!-- Employee Status Grid -->
                <div class="table-responsive">
                    <asp:GridView ID="gvEmployees" runat="server" CssClass="table table-bordered table-striped align-middle text-center"
                        AutoGenerateColumns="False" AllowPaging="True" PageSize="10"
                        OnPageIndexChanging="gvEmployees_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="EmployeeID" HeaderText="ID" ReadOnly="True" ItemStyle-Width="50px" />
                            <asp:BoundField DataField="FullName" HeaderText="Full Name" ReadOnly="True" ItemStyle-Width="200px" />
                            <asp:BoundField DataField="Email" HeaderText="Email" ReadOnly="True" ItemStyle-Width="250px" />
                            <asp:BoundField DataField="Phone" HeaderText="Phone" ReadOnly="True" ItemStyle-Width="150px" />
                            <asp:BoundField DataField="Position" HeaderText="Position" ReadOnly="True" ItemStyle-Width="200px" />
                            <asp:BoundField DataField="Status" HeaderText="Status" ReadOnly="True" ItemStyle-Width="150px" />
                        </Columns>
                    </asp:GridView>
                </div>

            </div>
        </div>
    </div>

    <script>
        // Optional: Add SweetAlert confirmation on filter change
        document.getElementById('<%= ddlStatus.ClientID %>').addEventListener('change', function (e) {
            e.preventDefault();
            const ddl = this;
            Swal.fire({
                title: 'Change Filter?',
                text: 'Apply this filter to the employee list?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#0d6efd',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes',
                cancelButtonText: 'No, cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(ddl.name, '');
                } else {
                    // Reset to previous value or do nothing
                    // (You can implement this if needed)
                }
            });
        });
    </script>

    <!-- Bootstrap 5 JS Bundle (Popper.js included) -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>
