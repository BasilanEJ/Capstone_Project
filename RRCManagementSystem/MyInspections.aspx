<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="MyInspections.aspx.cs" Inherits="RRCManagementSystem.MyInspections" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <h2 class="text-primary mb-4 fw-bold">🕵️ My Inspections</h2>

        <!-- Status Filter -->
        <div class="row mb-4">
            <div class="col-md-4">
                <asp:DropDownList ID="ddlStatusFilter" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged" CssClass="form-select">
                    <asp:ListItem Text="All" Value="All" />
                    <asp:ListItem Text="Pending" Value="Pending" />
                    <asp:ListItem Text="Completed" Value="Completed" />
                </asp:DropDownList>
            </div>
        </div>

        <!-- Inspections List -->
        <div class="row g-4">
            <asp:Repeater ID="rptInspections" runat="server">
                <ItemTemplate>
                    <div class="col-md-6 col-lg-4">
                        <div class="card h-100 border-start border-primary shadow-sm">
                            <div class="card-body">
                                <h5 class="card-title text-primary">Inspection #<%# Eval("InspectionID") %></h5>
                                <p class="mb-1"><strong>Name:</strong> <%# Eval("Name") %></p>
                                <p class="mb-1"><strong>Address:</strong> <%# Eval("StreetAndUnit") %>, <%# Eval("Barangay") %>, <%# Eval("City") %>, <%# Eval("Region") %>, <%# Eval("Country") %></p>
                                <p class="mb-1"><strong>Scheduled:</strong> <%# Eval("ScheduledDate", "{0:yyyy-MM-dd hh:mm tt}") %></p>
                                <p class="mb-1"><strong>Status:</strong> <%# Eval("InspectionStatus") %></p>
                                <p class="mb-1"><strong>Remarks:</strong> <%# Eval("Remarks") %></p>
                                <p class="mb-3"><strong>Assigned:</strong> <%# Eval("CreatedAt", "{0:yyyy-MM-dd}") %></p>

                                <%# Eval("InspectionStatus").ToString() == "Pending" 
                                    ? "<button type='button' class='btn btn-success btn-sm' onclick=\"markDone('" + Eval("InspectionID") + "')\">Mark Done</button>" 
                                    : "" %>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </div>

    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function markDone(inspectionId) {
            Swal.fire({
                title: 'Mark as Done?',
                text: 'Are you sure you want to mark this inspection as completed?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, mark done'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = 'MyInspections.aspx?done=' + inspectionId;
                }
            });
        }
    </script>
</asp:Content>
