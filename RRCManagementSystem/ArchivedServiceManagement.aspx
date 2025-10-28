<%@ Page Title="Archived Management" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="ArchivedServiceManagement.aspx.cs" Inherits="RRCManagementSystem.ArchivedServiceManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* ============ Archived Management Styles ============ */
        .archived-container {
            padding: 30px;
            max-width: 1400px;
            margin: 0 auto;
        }

        .archived-header {
            background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%);
            color: white;
            padding: 25px;
            border-radius: 12px;
            margin-bottom: 30px;
            box-shadow: 0 4px 12px rgba(220, 38, 38, 0.2);
        }

        .archived-header h1 {
            margin: 0;
            font-size: 28px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .archived-header p {
            margin: 8px 0 0;
            opacity: 0.9;
            font-size: 14px;
        }

        /* ============ Tab Navigation ============ */
        .tab-navigation {
            display: flex;
            gap: 10px;
            margin-bottom: 25px;
            border-bottom: 2px solid #e2e8f0;
            flex-wrap: wrap;
        }

        .tab-btn {
            padding: 12px 24px;
            background: transparent;
            border: none;
            border-bottom: 3px solid transparent;
            color: #64748b;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            font-size: 14px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .tab-btn:hover {
            color: #dc2626;
            background: #fef2f2;
        }

        .tab-btn.active {
            color: #dc2626;
            border-bottom-color: #dc2626;
            background: #fef2f2;
        }

        /* ============ Info Banner ============ */
        .info-banner {
            background: #fef3c7;
            border-left: 4px solid #f59e0b;
            padding: 15px 20px;
            border-radius: 8px;
            margin-bottom: 25px;
            display: flex;
            align-items: start;
            gap: 12px;
        }

        .info-banner i {
            color: #f59e0b;
            font-size: 20px;
            margin-top: 2px;
        }

        .info-banner-content h4 {
            margin: 0 0 5px 0;
            color: #92400e;
            font-size: 16px;
            font-weight: 600;
        }

        .info-banner-content p {
            margin: 0;
            color: #92400e;
            font-size: 14px;
            line-height: 1.5;
        }

        /* ============ List Card ============ */
        .archived-list-card {
            background: white;
            border-radius: 12px;
            padding: 30px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            border: 1px solid #e2e8f0;
        }

        .archived-list-title {
            font-size: 20px;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        /* ============ Empty State ============ */
        .empty-state {
            text-align: center;
            padding: 60px 20px;
        }

        .empty-state i {
            font-size: 64px;
            color: #cbd5e1;
            margin-bottom: 20px;
        }

        .empty-state h3 {
            color: #475569;
            font-size: 20px;
            margin-bottom: 10px;
        }

        .empty-state p {
            color: #64748b;
            font-size: 14px;
        }

        /* ============ GridView Styles ============ */
        .archived-grid {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

        .archived-grid thead {
            background: #f8fafc;
        }

        .archived-grid th {
            padding: 14px;
            text-align: left;
            font-weight: 600;
            color: #475569;
            border-bottom: 2px solid #e2e8f0;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .archived-grid td {
            padding: 16px 14px;
            border-bottom: 1px solid #f1f5f9;
            vertical-align: middle;
        }

        .archived-grid tr:hover {
            background: #f8fafc;
        }

        /* ============ Service/Blog Image ============ */
        .service-image {
            width: 80px;
            height: 80px;
            object-fit: cover;
            border-radius: 8px;
            border: 2px solid #e2e8f0;
        }

        .blog-image {
            width: 120px;
            height: 90px;
            object-fit: cover;
            border-radius: 8px;
            border: 2px solid #e2e8f0;
        }

        /* ============ Service Type Badge ============ */
        .service-type-badge {
            display: inline-block;
            padding: 6px 12px;
            border-radius: 6px;
            font-size: 12px;
            font-weight: 600;
        }

        .badge-termite {
            background: #fef3c7;
            color: #92400e;
        }

        .badge-pest {
            background: #dbeafe;
            color: #1e40af;
        }

        /* ============ FAQ Styles ============ */
        .faq-question {
            color: #1e293b;
            font-weight: 600;
            font-size: 14px;
            display: block;
            margin-bottom: 5px;
        }

        .faq-answer {
            color: #475569;
            font-size: 13px;
            line-height: 1.6;
        }

        /* ============ Blog Title ============ */
        .blog-title {
            color: #1e293b;
            font-weight: 600;
            font-size: 15px;
            margin-bottom: 5px;
        }

        .blog-description {
            color: #64748b;
            font-size: 13px;
            line-height: 1.5;
        }

        /* ============ Action Buttons ============ */
        .btn-action {
            padding: 8px 16px;
            border: none;
            border-radius: 6px;
            font-size: 13px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 6px;
            text-decoration: none;
            margin-right: 8px;
        }

        .btn-restore {
            background: #10b981;
            color: white;
        }

        .btn-restore:hover {
            background: #059669;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
        }

        .btn-delete-permanent {
            background: #ef4444;
            color: white;
        }

        .btn-delete-permanent:hover {
            background: #dc2626;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
        }

        /* ============ Deleted Date ============ */
        .deleted-date {
            color: #64748b;
            font-size: 13px;
        }

        .deleted-date i {
            margin-right: 6px;
            color: #94a3b8;
        }

        /* ============ Responsive Table ============ */
        @media screen and (max-width: 1024px) {
            .archived-grid thead {
                display: none;
            }

            .archived-grid tr {
                display: block;
                margin-bottom: 20px;
                border: 1px solid #e2e8f0;
                border-radius: 8px;
                overflow: hidden;
            }

            .archived-grid td {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 12px 16px;
                border-bottom: 1px solid #f1f5f9;
            }

            .archived-grid td:last-child {
                border-bottom: none;
            }

            .archived-grid td::before {
                content: attr(data-label);
                font-weight: 600;
                color: #475569;
                margin-right: 10px;
                flex-shrink: 0;
            }

            .archived-grid td[data-label="Image"] {
                justify-content: center;
            }

            .archived-grid td[data-label="Actions"] {
                flex-direction: column;
                gap: 8px;
            }

            .btn-action {
                width: 100%;
                justify-content: center;
            }
        }

        @media screen and (max-width: 768px) {
            .archived-container {
                padding: 15px;
            }

            .archived-header h1 {
                font-size: 22px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="archived-container">
        <%-- Header Section --%>
        <div class="archived-header">
            <h1>
                <i class="fas fa-archive"></i>
                Archived Management
            </h1>
            <p>Manage and restore archived services, blogs, and FAQs or permanently remove them</p>
        </div>

        <%-- Tab Navigation --%>
        <div class="tab-navigation">
            <asp:Button ID="btnTabServices" runat="server" Text="📦 Archived Services" 
                CssClass="tab-btn active" OnClick="btnTabServices_Click" 
                OnClientClick="setActiveTab(this); return true;" />
            <asp:Button ID="btnTabBlogs" runat="server" Text="📝 Archived Blogs" 
                CssClass="tab-btn" OnClick="btnTabBlogs_Click" 
                OnClientClick="setActiveTab(this); return true;" />
            <asp:Button ID="btnTabFaqs" runat="server" Text="❓ Archived FAQs" 
                CssClass="tab-btn" OnClick="btnTabFaqs_Click" 
                OnClientClick="setActiveTab(this); return true;" />
        </div>

        <%-- Info Banner --%>
        <div class="info-banner">
            <i class="fas fa-info-circle"></i>
            <div class="info-banner-content">
                <h4>About Archived Items</h4>
                <p>
                    Items listed here have been archived and are no longer visible on the main pages. 
                    You can <strong>restore</strong> them to make them active again, or <strong>permanently delete</strong> them to remove them from the database completely.
                </p>
            </div>
        </div>

        <%-- Hidden Fields for Actions --%>
        <asp:HiddenField ID="hdnAction" runat="server" />
        <asp:HiddenField ID="hdnItemId" runat="server" />
        <asp:Button ID="btnHiddenSubmit" runat="server" Style="display:none;" OnClick="btnHiddenSubmit_Click" />

        <asp:UpdatePanel ID="upArchived" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <%-- ============================================= --%>
                <%-- ARCHIVED SERVICES SECTION --%>
                <%-- ============================================= --%>
                <asp:Panel ID="pnlServices" runat="server" Visible="true">
                    <div class="archived-list-card">
                        <div class="archived-list-title">
                            <i class="fas fa-list"></i>
                            Archived Services List
                        </div>

                        <asp:Panel ID="pnlEmptyStateServices" runat="server" CssClass="empty-state" Visible="false">
                            <i class="fas fa-check-circle"></i>
                            <h3>No Archived Services</h3>
                            <p>All services are active. There are no archived services to display.</p>
                        </asp:Panel>

                        <asp:GridView 
                            ID="gvDeletedServices" 
                            runat="server" 
                            AutoGenerateColumns="False"
                            DataKeyNames="ServiceID"
                            CssClass="archived-grid"
                            GridLines="None"
                            EmptyDataText="No archived services found.">
                            <Columns>
                                <asp:TemplateField HeaderText="Image">
                                    <ItemTemplate>
                                        <asp:Image 
                                            ID="imgService" 
                                            runat="server" 
                                            ImageUrl='<%# Eval("ImagePath") %>' 
                                            CssClass="service-image"
                                            AlternateText="Service Image" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Type">
                                    <ItemTemplate>
                                        <span class='<%# Eval("ServiceType").ToString() == "Termite Control" ? "service-type-badge badge-termite" : "service-type-badge badge-pest" %>'>
                                            <%# Eval("ServiceType") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField DataField="ServiceTitle" HeaderText="Title" />

                                <asp:TemplateField HeaderText="Description">
                                    <ItemTemplate>
                                        <%# Eval("ServiceDescription").ToString().Length > 80 
                                            ? Eval("ServiceDescription").ToString().Substring(0, 80) + "..." 
                                            : Eval("ServiceDescription") %>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Archived Date">
                                    <ItemTemplate>
                                        <div class="deleted-date">
                                            <i class="far fa-clock"></i>
                                            <%# Eval("LastUpdated") != DBNull.Value 
                                                ? Convert.ToDateTime(Eval("LastUpdated")).ToString("MMM dd, yyyy") 
                                                : "N/A" %>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <button type="button" class="btn-action btn-restore" 
                                                onclick="confirmRestore('<%# Eval("ServiceID") %>', 'service')">
                                            <i class="fas fa-undo"></i> Restore
                                        </button>
                                        
                                        <button type="button" class="btn-action btn-delete-permanent" 
                                                onclick="confirmDelete('<%# Eval("ServiceID") %>', 'service')">
                                            <i class="fas fa-trash"></i> Delete 
                                        </button>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </asp:Panel>

                <%-- ============================================= --%>
                <%-- ARCHIVED BLOGS SECTION --%>
                <%-- ============================================= --%>
                <asp:Panel ID="pnlBlogs" runat="server" Visible="false">
                    <div class="archived-list-card">
                        <div class="archived-list-title">
                            <i class="fas fa-list"></i>
                            Archived Blogs List
                        </div>

                        <asp:Panel ID="pnlEmptyStateBlogs" runat="server" CssClass="empty-state" Visible="false">
                            <i class="fas fa-check-circle"></i>
                            <h3>No Archived Blogs</h3>
                            <p>All blogs are active. There are no archived blogs to display.</p>
                        </asp:Panel>

                        <asp:GridView 
                            ID="gvDeletedBlogs" 
                            runat="server" 
                            AutoGenerateColumns="False"
                            DataKeyNames="BlogID"
                            CssClass="archived-grid"
                            GridLines="None"
                            EmptyDataText="No archived blogs found.">
                            <Columns>
                                <asp:TemplateField HeaderText="Image">
                                    <ItemTemplate>
                                        <asp:Image 
                                            ID="imgBlog" 
                                            runat="server" 
                                            ImageUrl='<%# Eval("ImagePath") %>' 
                                            CssClass="blog-image"
                                            AlternateText="Blog Image" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Blog Title">
                                    <ItemTemplate>
                                        <div class="blog-title">
                                            <%# Eval("BlogTitle") %>
                                        </div>
                                        <div class="blog-description">
                                            <%# !string.IsNullOrEmpty(Eval("BlogDescription").ToString()) 
                                                ? (Eval("BlogDescription").ToString().Length > 60 
                                                    ? Eval("BlogDescription").ToString().Substring(0, 60) + "..." 
                                                    : Eval("BlogDescription").ToString())
                                                : (Eval("BlogContent").ToString().Length > 60 
                                                    ? Eval("BlogContent").ToString().Substring(0, 60) + "..." 
                                                    : Eval("BlogContent").ToString()) %>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Display Order">
                                    <ItemTemplate>
                                        <span style="color: #64748b; font-size: 14px;">
                                            <%# Eval("DisplayOrder") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Archived Date">
                                    <ItemTemplate>
                                        <div class="deleted-date">
                                            <i class="far fa-clock"></i>
                                            <%# Eval("ModifiedDate") != DBNull.Value 
                                                ? Convert.ToDateTime(Eval("ModifiedDate")).ToString("MMM dd, yyyy") 
                                                : (Eval("CreatedDate") != DBNull.Value 
                                                    ? Convert.ToDateTime(Eval("CreatedDate")).ToString("MMM dd, yyyy") 
                                                    : "N/A") %>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <button type="button" class="btn-action btn-restore" 
                                                onclick="confirmRestore('<%# Eval("BlogID") %>', 'blog')">
                                            <i class="fas fa-undo"></i> Restore
                                        </button>
                                        
                                        <button type="button" class="btn-action btn-delete-permanent" 
                                                onclick="confirmDelete('<%# Eval("BlogID") %>', 'blog')">
                                            <i class="fas fa-trash"></i> Delete 
                                        </button>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </asp:Panel>

                <%-- ============================================= --%>
                <%-- ARCHIVED FAQs SECTION --%>
                <%-- ============================================= --%>
                <asp:Panel ID="pnlFaqs" runat="server" Visible="false">
                    <div class="archived-list-card">
                        <div class="archived-list-title">
                            <i class="fas fa-list"></i>
                            Archived FAQs List
                        </div>

                        <asp:Panel ID="pnlEmptyStateFaqs" runat="server" CssClass="empty-state" Visible="false">
                            <i class="fas fa-check-circle"></i>
                            <h3>No Archived FAQs</h3>
                            <p>All FAQs are active. There are no archived FAQs to display.</p>
                        </asp:Panel>

                        <asp:GridView 
                            ID="gvDeletedFaqs" 
                            runat="server" 
                            AutoGenerateColumns="False"
                            DataKeyNames="ID"
                            CssClass="archived-grid"
                            GridLines="None"
                            EmptyDataText="No archived FAQs found.">
                            <Columns>
                                <asp:TemplateField HeaderText="Question">
                                    <ItemTemplate>
                                        <strong class="faq-question">
                                            <%# Eval("Question") %>
                                        </strong>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Answer">
                                    <ItemTemplate>
                                        <div class="faq-answer">
                                            <%# Eval("Answer").ToString().Length > 100 
                                                ? Eval("Answer").ToString().Substring(0, 100) + "..." 
                                                : Eval("Answer") %>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Archived Date">
                                    <ItemTemplate>
                                        <div class="deleted-date">
                                            <i class="far fa-clock"></i>
                                            <%# Eval("UpdatedDate") != DBNull.Value 
                                                ? Convert.ToDateTime(Eval("UpdatedDate")).ToString("MMM dd, yyyy") 
                                                : "N/A" %>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <button type="button" class="btn-action btn-restore" 
                                                onclick="confirmRestore('<%# Eval("ID") %>', 'faq')">
                                            <i class="fas fa-undo"></i> Restore
                                        </button>
                                        
                                        <button type="button" class="btn-action btn-delete-permanent" 
                                                onclick="confirmDelete('<%# Eval("ID") %>', 'faq')">
                                            <i class="fas fa-trash"></i> Delete 
                                        </button>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        // Set active tab
        function setActiveTab(tabButton) {
            document.querySelectorAll('.tab-btn').forEach(btn => {
                btn.classList.remove('active');
            });
            if (tabButton) {
                tabButton.classList.add('active');
            }
        }

        // Confirm Restore
        function confirmRestore(itemId, type) {
            let itemName = type === 'service' ? 'service' : (type === 'blog' ? 'blog' : 'FAQ');
            
            Swal.fire({
                title: 'Restore Item?',
                text: 'This ' + itemName + ' will be restored and become active again.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#10b981',
                cancelButtonColor: '#6b7280',
                confirmButtonText: '<i class="fas fa-undo"></i> Yes, Restore it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Set hidden field values
                    let action = type === 'service' ? 'RestoreService' : (type === 'blog' ? 'RestoreBlog' : 'RestoreFaq');
                    document.getElementById('<%= hdnAction.ClientID %>').value = action;
                    document.getElementById('<%= hdnItemId.ClientID %>').value = itemId;
                    
                    // Trigger postback
                    document.getElementById('<%= btnHiddenSubmit.ClientID %>').click();
                }
            });
        }

        // Confirm Delete
        function confirmDelete(itemId, type) {
            let itemName = type === 'service' ? 'service' : (type === 'blog' ? 'blog' : 'FAQ');
            
            Swal.fire({
                title: 'Are you absolutely sure?',
                html: '<div style="text-align: left;"><p><strong>⚠️ WARNING:</strong> This action cannot be undone!</p><p>This will permanently delete this ' + itemName + ' from the database.</p></div>',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ef4444',
                cancelButtonColor: '#6b7280',
                confirmButtonText: '<i class="fas fa-trash"></i> Yes, Delete Forever!',
                cancelButtonText: 'Cancel',
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    // Set hidden field values
                    let action = type === 'service' ? 'DeletePermanent' : (type === 'blog' ? 'DeletePermanentBlog' : 'DeletePermanentFaq');
                    document.getElementById('<%= hdnAction.ClientID %>').value = action;
                    document.getElementById('<%= hdnItemId.ClientID %>').value = itemId;
                    
                    // Trigger postback
                    document.getElementById('<%= btnHiddenSubmit.ClientID %>').click();
                }
            });
        }

        // Apply responsive table headers
        function applyResponsiveTableHeaders() {
            const grids = document.querySelectorAll('.archived-grid');

            grids.forEach(grid => {
                const headers = [];
                const headerRow = grid.querySelector('thead tr');
                if (!headerRow) return;

                headerRow.querySelectorAll('th').forEach(th => {
                    headers.push(th.innerText.trim());
                });

                if (headers.length === 0) return;

                grid.querySelectorAll('tbody tr').forEach(row => {
                    row.querySelectorAll('td').forEach((td, index) => {
                        if (headers[index]) {
                            td.setAttribute('data-label', headers[index]);
                        }
                    });
                });
            });
        }

        document.addEventListener('DOMContentLoaded', applyResponsiveTableHeaders);

        // Reapply after UpdatePanel postback
        if (typeof Sys !== 'undefined') {
            try {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(applyResponsiveTableHeaders);
            } catch (e) {
                console.error("Error attaching to PageRequestManager:", e);
            }
        }
    </script>
</asp:Content>