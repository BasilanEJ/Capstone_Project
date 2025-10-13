<%@ Page Title="Audit Logs" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="AuditLogs.aspx.cs" Inherits="RRCManagementSystem.AuditLogs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Tailwind CSS -->
    <script src="https://cdn.tailwindcss.com"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

    <style>
        /* === Container === */
        .log-container {
            width: 100%;
            max-width: 1200px;
            margin: 20px auto;
            background-color: #fff;
            padding: 20px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            border-radius: 10px;
        }

        .log-container h2 {
            text-align: center;
            color: #333;
            margin-bottom: 20px;
            font-size: 1.8rem;
            font-weight: bold;
        }

        /* === Date Filters === */
        .date-filters .form-label {
            font-weight: 500;
            color: #333;
        }

        /* === Table Wrapper === */
        .grid-wrapper {
            width: 100%;
            overflow-x: auto;
            display: flex;
            justify-content: center;
        }

        /* === GridView === */
        .grid {
            max-width: 1000px;
            width: 100%;
            margin: 0 auto;
            border-collapse: collapse;
            text-align: center;
        }

        .grid th,
        .grid td {
            padding: 12px;
            border: 1px solid #e0e0e0;
            font-size: 14px;
            vertical-align: middle;
        }

        .grid th {
            background-color: #007bff;
            color: #fff;
            font-weight: 600;
        }

        /* === Message Styles === */
        .alert-message {
            text-align: center;
            margin-bottom: 10px;
            color: red;
        }

        /* === Custom Pager === */
        .custom-pager {
            display: flex;
            justify-content: center;
            flex-wrap: wrap;
            gap: 6px;
            padding: 15px;
            background-color: #f9f9f9;
            border-top: 1px solid #e5e5e5;
            margin-top: 10px;
        }

        .pager-btn {
            display: inline-block;
            padding: 8px 12px;
            font-size: 14px;
            color: #007bff;
            border-radius: 4px;
            cursor: pointer;
            text-decoration: none;
            transition: background 0.3s, color 0.3s;
        }

        .pager-btn:hover {
            background-color: #007bff;
            color: white;
        }

        .selected-page {
            background-color: #007bff;
            color: white;
            font-weight: bold;
        }

        /* === Responsive === */
        @media (max-width: 576px) {
            .log-container {
                padding: 15px;
            }

            .log-container h2 {
                font-size: 1.5rem;
            }

            .grid th,
            .grid td {
                font-size: 13px;
                padding: 8px;
            }

            .date-filters .col-md-4 {
                flex: 1 1 100%;
                max-width: 100%;
            }
        }
    </style>

    <div class="log-container">
        <h2><i class="fas fa-file-alt me-2"></i> Audit Logs</h2>

 
        <div class="date-filters">
            <div class="row mb-3 align-items-end g-3">
                <div class="col-md-4 col-12">
                    <label for="txtFrom" class="form-label">From Date</label>
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control text-center" TextMode="Date" />
                </div>
                <div class="col-md-4 col-12">
                    <label for="txtTo" class="form-label">To Date</label>
                    <asp:TextBox ID="txtTo" runat="server" CssClass="form-control text-center" TextMode="Date" />
                </div>
                <div class="col-md-4 col-12 d-grid">
                    <label class="form-label">&nbsp;</label>
                    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary w-100"
                        OnClick="btnFilter_Click" />
                </div>
            </div>
        </div>

      
        <asp:UpdatePanel ID="UpdatePanelLogs" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Label ID="lblNoData" runat="server"
                    CssClass="text-danger text-center d-block mb-3"
                    Visible="false" />
                <asp:Label ID="lblMessage" runat="server" CssClass="alert-message" />

                <div class="grid-wrapper">
                    <asp:GridView ID="gvLogs" runat="server" CssClass="grid table table-hover"
                        AutoGenerateColumns="False"
                        AllowPaging="True"
                        PageSize="50"
                        OnPageIndexChanging="gvLogs_PageIndexChanging"
                        OnDataBound="gvLogs_DataBound"
                        PagerSettings-Mode="NumericFirstLast"
                        PagerSettings-FirstPageText="« First"
                        PagerSettings-LastPageText="Last »"
                        PagerSettings-NextPageText="Next ›"
                        PagerSettings-PreviousPageText="‹ Prev">

                        <PagerTemplate>
                            <div class="custom-pager">
                                <asp:LinkButton runat="server" CommandName="Page" CommandArgument="First" CssClass="pager-btn">« First</asp:LinkButton>
                                <asp:LinkButton runat="server" CommandName="Page" CommandArgument="Prev" CssClass="pager-btn">‹ Prev</asp:LinkButton>

                                <asp:Repeater ID="rptPages" runat="server" OnItemCommand="rptPages_ItemCommand" OnItemDataBound="rptPages_ItemDataBound">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" ID="lnkPage" CommandName="Page" CommandArgument='<%# Container.DataItem %>'
                                            CssClass='<%# (Container.DataItem.ToString() == (gvLogs.PageIndex + 1).ToString()) ? "selected-page" : "pager-btn" %>'
                                            Text='<%# Container.DataItem %>' />
                                    </ItemTemplate>
                                </asp:Repeater>

                                <asp:LinkButton runat="server" CommandName="Page" CommandArgument="Next" CssClass="pager-btn">Next ›</asp:LinkButton>
                                <asp:LinkButton runat="server" CommandName="Page" CommandArgument="Last" CssClass="pager-btn">Last »</asp:LinkButton>
                            </div>
                        </PagerTemplate>

                   
                        <Columns>
                            <asp:BoundField DataField="LogID" HeaderText="Log ID" />
                            <asp:BoundField DataField="AdminName" HeaderText="User Name" />
                            <asp:BoundField DataField="Action" HeaderText="Action" />
                            <asp:BoundField DataField="Timestamp" HeaderText="Timestamp" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnFilter" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
