<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="MyInspections.aspx.cs" Inherits="RRCManagementSystem.MyInspections" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .card-container {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
            gap: 20px;
        }

        .card {
            background-color: white;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            border-left: 6px solid #007bff;
            position: relative;
        }

        .card h4 {
            margin: 0 0 10px;
            color: #004085;
        }

        .card .info {
            font-size: 14px;
            margin-bottom: 10px;
        }

        .card .actions button {
            margin-right: 8px;
            padding: 6px 12px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }

        .btn-done {
            background-color: #28a745;
            color: white;
        }

        .filter-row {
            margin-bottom: 20px;
        }
    </style>

    <h2 style="color:#004085;">🕵️ My Inspections</h2>

    <div class="filter-row">
        <asp:DropDownList ID="ddlStatusFilter" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged" CssClass="form-control" Width="200px">
            <asp:ListItem Text="All" Value="All" />
            <asp:ListItem Text="Pending" Value="Pending" />
            <asp:ListItem Text="Completed" Value="Completed" />
        </asp:DropDownList>
    </div>

    <asp:Repeater ID="rptInspections" runat="server">
        <ItemTemplate>
            <div class="card">
                <h4>Inspection #<%# Eval("InspectionID") %></h4>
                <div class="info"><strong>Name:</strong> <%# Eval("Name") %></div>
                <div class="info"><strong>Address:</strong> <%# Eval("StreetAndUnit") %>, <%# Eval("Barangay") %>, <%# Eval("City") %>, <%# Eval("Region") %>, <%# Eval("Country") %></div>
                <div class="info"><strong>Scheduled:</strong> <%# Eval("ScheduledDate", "{0:yyyy-MM-dd hh:mm tt}") %></div>
                <div class="info"><strong>Status:</strong> <%# Eval("InspectionStatus") %></div>
                <div class="info"><strong>Remarks:</strong> <%# Eval("Remarks") %></div>
                <div class="info"><strong>Assigned:</strong> <%# Eval("CreatedAt", "{0:yyyy-MM-dd}") %></div>
                <div class="actions">
                    <%# Eval("InspectionStatus").ToString() == "Pending" 
                        ? "<button type='button' class='btn-done' onclick=\"markDone('" + Eval("InspectionID") + "')\">Done</button>" 
                        : "" %>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>

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