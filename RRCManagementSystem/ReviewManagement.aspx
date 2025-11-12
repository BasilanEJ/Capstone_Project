<%@ Page Title="Review Management" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ReviewManagement.aspx.cs" Inherits="RRCManagementSystem.ReviewManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css">
    <style>
        .review-management-container {
            padding: 30px;
            background: #f8f9fa;
            min-height: 100vh;
        }

        .page-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 15px;
            margin-bottom: 30px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        }

        .page-header h1 {
            margin: 0;
            font-size: 32px;
            font-weight: 700;
        }

        .gridview-container {
            background: white;
            border-radius: 15px;
            padding: 25px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
        }

        .table thead {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }

        .table tbody tr:hover {
            background-color: #f8f9ff;
        }

        .badge-active {
            background-color: #10b981;
            color: white;
        }

        .action-buttons .btn {
            margin: 2px;
            padding: 6px 12px;
            font-size: 13px;
        }

        .star-rating {
            color: #fbbf24;
            font-size: 16px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="review-management-container">
        <div class="page-header">
            <h1>🌟 Active Reviews</h1>
            <p>View and manage all currently active customer reviews. You can archive reviews anytime.</p>
        </div>

        <div class="gridview-container">
            <asp:GridView ID="gvReviews" runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover"
                DataKeyNames="ReviewID"
                OnRowCommand="gvReviews_RowCommand"
                EmptyDataText="No active reviews found.">
                
                <Columns>
                    <asp:BoundField DataField="ReviewID" HeaderText="ID" ItemStyle-Width="50px" Visible="false" />

                    <asp:TemplateField HeaderText="Customer">
                        <ItemTemplate>
                            <strong><%# Eval("CustomerName") %></strong>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Review">
                        <ItemTemplate>
                            <div class="text-truncate" style="max-width:400px;" title='<%# Eval("ReviewText") %>'>
                                <%# Eval("ReviewText") %>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Rating">
                        <ItemTemplate>
                            <span class="star-rating"><%# GetStarRating(Convert.ToInt32(Eval("Rating"))) %></span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Recommends">
                        <ItemTemplate>
                            <%# Convert.ToBoolean(Eval("Recommends")) ? "❤️ Yes" : "No" %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class="badge badge-active">Active</span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="150px">
                        <ItemTemplate>
                            <asp:Button ID="btnArchive" runat="server"
                                Text="🗄️ Archive"
                                CssClass="btn btn-sm btn-danger"
                                CommandName="ArchiveReview"
                                CommandArgument='<%# Eval("ReviewID") %>'
                                OnClientClick="return confirm('Are you sure you want to archive this review?');"
                                CausesValidation="false" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>
