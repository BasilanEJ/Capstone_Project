<%@ Page Title="Blog Management" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="BlogManagement.aspx.cs" Inherits="RRCManagementSystem.BlogManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .blog-management-container {
            padding: 30px;
            background: #f8f9fa;
            min-height: 100vh;
        }
        
        .blog-card-admin {
            background: white;
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }
        
        .blog-preview-img {
            max-width: 200px;
            max-height: 150px;
            object-fit: cover;
            border-radius: 8px;
        }
        
        .btn-action {
            margin-right: 10px;
        }
        
        .form-section {
            background: white;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            margin-bottom: 30px;
        }
        
        .form-group {
            margin-bottom: 20px;
        }
        
        .form-group label {
            font-weight: 600;
            margin-bottom: 8px;
            display: block;
            color: #333;
        }
        
        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 5px;
            font-size: 14px;
        }
        
        textarea.form-control {
            min-height: 300px;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
        }
        
        .btn-primary-custom {
            background: #2563eb;
            color: white;
            padding: 12px 30px;
            border: none;
            border-radius: 5px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }
        
        .btn-primary-custom:hover {
            background: #1e40af;
            transform: translateY(-2px);
        }
        
        .btn-secondary-custom {
            background: #6c757d;
            color: white;
            padding: 8px 20px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
        }
        
        .btn-warning-custom {
            background: #ff9800;
            color: white;
            padding: 8px 20px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
        }
        
        .btn-success-custom {
            background: #28a745;
            color: white;
            padding: 8px 20px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
        }
        
        .status-badge {
            padding: 5px 12px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
        }
        
        .status-active {
            background: #d4edda;
            color: #155724;
        }
        
        .status-inactive {
            background: #fff3cd;
            color: #856404;
        }
        
        .blog-list-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }
        
        .helper-text {
            font-size: 12px;
            color: #6c757d;
            margin-top: 5px;
        }
        
        .image-preview-container {
            margin-top: 10px;
            max-width: 300px;
        }
        
        .image-preview-container img {
            width: 100%;
            border-radius: 8px;
            border: 2px solid #ddd;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="blog-management-container">
        <h1 style="color: #2563eb; margin-bottom: 30px;">📝 Blog Management</h1>
        
        <!-- Add/Edit Blog Form -->
        <div class="form-section">
            <h2 style="color: #333; margin-bottom: 20px;">
                <asp:Label ID="lblFormTitle" runat="server" Text="Add New Blog"></asp:Label>
            </h2>
            
            <asp:HiddenField ID="hfBlogID" runat="server" Value="0" />
            <asp:HiddenField ID="hfArchiveBlogID" runat="server" Value="0" />
            
            <div class="form-group">
                <label>Blog Title *</label>
                <asp:TextBox ID="txtBlogTitle" runat="server" CssClass="form-control" 
                    placeholder="Enter blog title" MaxLength="200" required></asp:TextBox>
            </div>
            
            <div class="form-group">
                <label>Short Description (Optional)</label>
                <asp:TextBox ID="txtBlogDescription" runat="server" CssClass="form-control" 
                    TextMode="MultiLine" Rows="2"
                    placeholder="Brief description shown on blog card (max 500 characters)" 
                    MaxLength="500"></asp:TextBox>
                <div class="helper-text">This appears under the title on the blog card</div>
            </div>
            
            <div class="form-group">
                <label>Blog Content *</label>
                <asp:TextBox ID="txtBlogContent" runat="server" CssClass="form-control" 
                    TextMode="MultiLine" Rows="15"
                    placeholder="Enter full blog content here...&#13;&#10;&#13;&#10;You can use:&#13;&#10;- Bullet points (start line with - or •)&#13;&#10;&#13;&#10;Example:&#13;&#10;This is a paragraph.&#13;&#10;&#13;&#10;- First point&#13;&#10;- Second point&#13;&#10;&#13;&#10;Another paragraph." 
                    required></asp:TextBox>
                <div class="helper-text">
                    💡 Tip: Use line breaks for paragraphs. Start lines with "-" or "•" for bullet points.
                </div>
            </div>
            
            <div class="form-group">
                <label>Blog Image *</label>
                <asp:FileUpload ID="fuBlogImage" runat="server" CssClass="form-control" 
                    accept=".png,.jpg,.jpeg" />
                <div class="helper-text">Upload JPG or PNG (recommended size: 800x600px)</div>
                
                <!-- Image Preview -->
                <asp:Panel ID="pnlImagePreview" runat="server" CssClass="image-preview-container" Visible="false">
                    <label>Current Image:</label>
                    <asp:Image ID="imgCurrentBlog" runat="server" />
                </asp:Panel>
            </div>
            
            <div class="form-group">
                <label>Display Order</label>
                <asp:TextBox ID="txtDisplayOrder" runat="server" CssClass="form-control" 
                    TextMode="Number" Text="0" style="max-width: 150px;"></asp:TextBox>
                <div class="helper-text">Lower numbers appear first (0 = highest priority)</div>
            </div>
            
            <div class="form-group">
                <asp:CheckBox ID="chkIsActive" runat="server" Checked="true" />
                <label for="<%= chkIsActive.ClientID %>" style="display: inline; margin-left: 5px;">
                    Active (Show on website)
                </label>
            </div>
            
            <div style="margin-top: 30px;">
                <asp:Button ID="btnSaveBlog" runat="server" Text="Save Blog" 
                    CssClass="btn-primary-custom" OnClick="btnSaveBlog_Click" />
                <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" 
                    CssClass="btn-secondary-custom btn-action" OnClick="btnCancelEdit_Click" 
                    Visible="false" />
            </div>
        </div>
        
        <!-- Blog List -->
        <div class="form-section">
            <div class="blog-list-header">
                <h2 style="color: #333; margin: 0;">Existing Blogs</h2>
                <asp:Label ID="lblBlogCount" runat="server" 
                    style="color: #6c757d; font-size: 14px;"></asp:Label>
            </div>
            
            <asp:Repeater ID="rptBlogList" runat="server" OnItemCommand="rptBlogList_ItemCommand">
                <ItemTemplate>
                    <div class="blog-card-admin">
                        <div style="display: flex; gap: 20px; align-items: start;">
                            <div style="flex-shrink: 0;">
                                <asp:Image ID="imgBlogThumb" runat="server" 
                                    ImageUrl='<%# Eval("ImagePath") %>' 
                                    CssClass="blog-preview-img" 
                                    AlternateText='<%# Eval("BlogTitle") %>' />
                            </div>
                            <div style="flex-grow: 1;">
                                <h3 style="margin-top: 0; color: #2563eb;">
                                    <%# Eval("BlogTitle") %>
                                </h3>
                                
                                <p style="color: #6c757d; font-size: 14px; margin: 10px 0;">
                                    <%# GetShortContent(Eval("BlogContent").ToString(), 150) %>
                                </p>
                                
                                <div style="margin: 15px 0;">
                                    <span class="status-badge <%# Convert.ToBoolean(Eval("IsActive")) ? "status-active" : "status-inactive" %>">
                                        <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Archived" %>
                                    </span>
                                    <span style="margin-left: 15px; color: #6c757d; font-size: 13px;">
                                        Order: <%# Eval("DisplayOrder") %>
                                    </span>
                                    <span style="margin-left: 15px; color: #6c757d; font-size: 13px;">
                                        Created: <%# Convert.ToDateTime(Eval("CreatedDate")).ToString("MMM dd, yyyy") %>
                                    </span>
                                </div>
                                
                                <div style="margin-top: 15px;">
                                    <asp:LinkButton ID="btnEdit" runat="server" 
                                        CommandName="Edit" 
                                        CommandArgument='<%# Eval("BlogID") %>'
                                        CssClass="btn-secondary-custom btn-action">
                                        ✏️ Edit
                                    </asp:LinkButton>
                                    
                                    <asp:LinkButton ID="btnToggleActive" runat="server" 
                                        CommandName="ToggleActive" 
                                        CommandArgument='<%# Eval("BlogID") %>'
                                        CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "btn-warning-custom btn-action" : "btn-success-custom btn-action" %>'
                                        OnClientClick='<%# Convert.ToBoolean(Eval("IsActive")) ? "return confirmArchive(" + Eval("BlogID") + ", \"" + Eval("BlogTitle").ToString().Replace("\"", "\\\"") + "\");" : "" %>'>
                                        <%# Convert.ToBoolean(Eval("IsActive")) ? "📦 Archive" : "✅ Unarchive" %>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            
            <asp:Label ID="lblNoBlogsMessage" runat="server" 
                Text="No blogs available. Create your first blog above!" 
                Visible="false"
                style="display: block; text-align: center; color: #6c757d; padding: 40px; font-size: 16px;"></asp:Label>
        </div>
    </div>
    
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    
    <script type="text/javascript">
        function confirmArchive(blogId, blogTitle) {
            event.preventDefault();
            
            Swal.fire({
                title: 'Archive Blog?',
                html: 'Are you sure you want to archive<br/><strong>"' + blogTitle + '"</strong>?<br/><br/>This will hide it from the website but keep it in the system.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ff9800',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, Archive it',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfArchiveBlogID.ClientID %>').value = blogId;
                    <%= Page.ClientScript.GetPostBackEventReference(btnHiddenArchive, "") %>;
                }
            });
            
            return false;
        }
    </script>
    
    <!-- Hidden button for archive postback -->
    <asp:Button ID="btnHiddenArchive" runat="server" OnClick="btnHiddenArchive_Click" Style="display:none;" />
</asp:Content>