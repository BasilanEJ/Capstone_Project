<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="GroupEmployees.aspx.cs" Inherits="RRCManagementSystem.GroupEmployees" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap -->
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>

    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f8f9fa;
        }

        .main-content {
            padding: 30px;
            min-height: calc(100vh - 100px);
            background-color: #f8f9fa;
        }

        .team-form-container {
            background: #ffffff;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
            max-width: 950px;
            margin: 0 auto;
        }

        label {
            font-weight: 500;
            margin-bottom: 8px;
            display: block;
            color: #333;
        }

        .form-control, .form-select {
            width: 100%;
            padding: 10px 12px;
            border: 1px solid #ced4da;
            border-radius: 4px;
            margin-bottom: 20px;
            font-size: 14px;
        }

        .btn-primary {
            background-color: #004085;
            border-color: #004085;
            color: #ffffff;
            padding: 10px 20px;
            border-radius: 4px;
            font-size: 14px;
            transition: background-color 0.3s ease;
            margin-right: 10px;
        }

        .btn-primary:hover {
            background-color: #003366;
        }

        .btn-cancel {
            background-color: #6c757d;
            color: #ffffff;
            padding: 10px 20px;
            border-radius: 4px;
            font-size: 14px;
            transition: background-color 0.3s ease;
        }

        .btn-cancel:hover {
            background-color: #5a6268;
        }

        .gridview-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
            font-size: 14px;
            background-color: #ffffff;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05);
            border-radius: 8px;
            overflow: hidden;
        }

        .gridview-table th, .gridview-table td {
            padding: 12px 15px;
            border-bottom: 1px solid #dee2e6;
            text-align: left;
            vertical-align: middle;
        }

        .gridview-table th {
            background-color: #004085;
            color: #ffffff;
            font-weight: 600;
            font-size: 14px;
        }

        .gridview-table tr:nth-child(even) td {
            background-color: #f9f9f9;
        }

        .gridview-table tr:hover td {
            background-color: #f1f1f1;
        }

        .gridview-table input[type="checkbox"] {
            display: block;
            margin: 0 auto;
            width: 18px;
            height: 18px;
        }

        .gridview-table .form-select {
            width: 180px;
            padding: 8px 10px;
            font-size: 13px;
        }

        .message-label {
            display: block;
            margin-top: 20px;
            font-weight: 600;
            font-size: 14px;
            color: #333;
        }

        .message-label.error {
            color: #dc3545;
        }

        .message-label.success {
            color: #28a745;
        }

        @media (max-width: 768px) {
            .team-form-container {
                padding: 20px;
            }

            .btn-primary,
            .btn-cancel {
                width: 100%;
                margin-bottom: 10px;
            }
        }
    </style>

    <script type="text/javascript">
    function closeCreateTeamModal() {
        $('#createTeamModal').modal('hide');
    }
    </script>


</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="main-content">
        <div class="team-form-container">
            <h3>Create or Assign Technicians to a Team</h3>

            <!-- Button to open the Create Team modal -->
            <asp:Button ID="btnOpenModal" runat="server" Text="➕ Create New Team" CssClass="btn btn-primary mb-4"
                OnClientClick="$('#createTeamModal').modal('show'); return false;" />

            <!-- Existing teams dropdown -->
            <div class="mb-3">
                <label for="ddlExistingTeams">Assign to Existing Team</label>
                <asp:DropDownList ID="ddlExistingTeams" runat="server" CssClass="form-select">
                    <asp:ListItem Text="Select an existing team" Value="" />
                </asp:DropDownList>
            </div>

            <h5>Select Technicians</h5>
            <asp:GridView ID="gvTechnicians" runat="server" AutoGenerateColumns="False"
                CssClass="gridview-table" DataKeyNames="EmployeeID"
                OnRowDataBound="gvTechnicians_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="Select">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkSelect" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="EmployeeID" HeaderText="ID" />
                    <asp:BoundField DataField="FullName" HeaderText="Full Name" />
                    <asp:BoundField DataField="CurrentTeam" HeaderText="Current Team" />

                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlAction" runat="server" CssClass="form-select">
                                <asp:ListItem Text="No Action" Value="" />
                                <asp:ListItem Text="Remove from team" Value="REMOVE" />
                            </asp:DropDownList>
                            <asp:HiddenField ID="hfEmployeeName" runat="server" Value='<%# Eval("FullName") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <div class="action-buttons">
                <asp:Button ID="btnSaveChanges" runat="server" CssClass="btn btn-primary mt-3" Text="Save Changes"
                    OnClick="btnSaveChanges_Click"
                    OnClientClick="return confirm('Are you sure you want to save these changes?');" />

                <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-cancel mt-3" Text="Cancel"
                    PostBackUrl="Dashboard.aspx"
                    OnClientClick="return confirm('Are you sure you want to cancel and go back to the dashboard?');" />
            </div>

            <asp:Label ID="lblMessage" runat="server" CssClass="message-label" />
        </div>
    </div>

    <!-- Modal: Create Team -->
    <div class="modal fade" id="createTeamModal" tabindex="-1" role="dialog" aria-labelledby="createTeamModalLabel" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="createTeamModalLabel">Create New Team</h5>
                    <button type="button" class="close text-white" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <label for="txtModalTeamName">Team Name</label>
                    <asp:TextBox ID="txtModalTeamName" runat="server" CssClass="form-control" placeholder="Enter team name" />
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnCreateTeamModal" runat="server" CssClass="btn btn-primary" Text="Create Team"
                        OnClick="btnCreateTeamModal_Click" />
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>