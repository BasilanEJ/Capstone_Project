<%@ Page Title="Blog Management" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="BlogManagement.aspx.cs" Inherits="RRCManagementSystem.BlogManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css">
    <link href="https://cdn.jsdelivr.net/npm/summernote@0.8.20/dist/summernote-bs5.min.css" rel="stylesheet">
    
    <style>
        .blog-management-container {
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

        .page-header p {
            margin: 10px 0 0 0;
            opacity: 0.9;
        }

        .action-card {
            background: white;
            border-radius: 15px;
            padding: 25px;
            margin-bottom: 25px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
            transition: all 0.3s ease;
        }

        .action-card.edit-mode {
            border: 3px solid #f093fb;
            box-shadow: 0 4px 20px rgba(240, 147, 251, 0.3);
            animation: pulse 2s infinite;
        }

        @keyframes pulse {
            0%, 100% {
                box-shadow: 0 4px 20px rgba(240, 147, 251, 0.3);
            }
            50% {
                box-shadow: 0 6px 30px rgba(240, 147, 251, 0.5);
            }
        }

        .action-card-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }

        .action-card-header h3 {
            color: #667eea;
            margin: 0;
            font-weight: 600;
        }

        .action-card-header h3.edit-mode {
            color: #f093fb;
            font-size: 28px;
        }

        .edit-mode-badge {
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
            padding: 8px 20px;
            border-radius: 20px;
            font-weight: 600;
            font-size: 14px;
            display: inline-block;
            animation: slideIn 0.3s ease;
        }

        @keyframes slideIn {
            from {
                opacity: 0;
                transform: translateX(-20px);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }

        .form-label {
            font-weight: 600;
            color: #495057;
            margin-bottom: 8px;
        }

        .form-control, .form-select {
            border-radius: 8px;
            border: 2px solid #e0e0e0;
            padding: 10px 15px;
            transition: all 0.3s ease;
        }

        .form-control:focus, .form-select:focus {
            border-color: #667eea;
            box-shadow: 0 0 0 0.2rem rgba(102, 126, 234, 0.25);
        }

        .btn-custom {
            padding: 12px 30px;
            border-radius: 8px;
            font-weight: 600;
            transition: all 0.3s ease;
            border: none;
        }

        .btn-add {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }

        .btn-add:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
            color: white;
        }

        .btn-update {
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
        }

        .btn-update:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(240, 147, 251, 0.4);
            color: white;
        }

        .btn-cancel {
            background: #6c757d;
            color: white;
        }

        .btn-cancel:hover {
            background: #5a6268;
            color: white;
        }

        .gridview-container {
            background: white;
            border-radius: 15px;
            padding: 25px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
            overflow-x: auto;
        }

        .table {
            margin-bottom: 0;
        }

        .table thead {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }

        .table thead th {
            border: none;
            padding: 15px;
            font-weight: 600;
            text-transform: uppercase;
            font-size: 13px;
            letter-spacing: 0.5px;
            white-space: nowrap;
        }

        .table tbody tr {
            transition: all 0.3s ease;
        }

        .table tbody tr:hover {
            background-color: #f8f9ff;
        }

        .table tbody td {
            padding: 15px;
            vertical-align: middle;
            border-bottom: 1px solid #e0e0e0;
        }

        .badge {
            padding: 6px 12px;
            border-radius: 6px;
            font-weight: 600;
            font-size: 12px;
        }

        .badge-active {
            background-color: #10b981;
            color: white;
        }

        .badge-inactive {
            background-color: #ef4444;
            color: white;
        }

        .blog-thumbnail {
            width: 80px;
            height: 80px;
            object-fit: cover;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }

        .blog-title-preview {
            max-width: 250px;
            font-weight: 600;
            color: #2d3748;
            white-space: normal;
        }

        .action-buttons .btn {
            margin: 2px;
            padding: 6px 12px;
            font-size: 13px;
            white-space: nowrap;
        }

        .char-counter {
            font-size: 12px;
            color: #6c757d;
            float: right;
            margin-top: 5px;
        }

        .form-check-input:checked {
            background-color: #667eea;
            border-color: #667eea;
        }

        .image-upload-area {
            border: 2px dashed #cbd5e0;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
            background: #f7fafc;
            transition: all 0.3s ease;
        }

        .image-upload-area:hover {
            border-color: #667eea;
            background: #edf2f7;
        }

        .image-preview {
            max-width: 300px;
            max-height: 200px;
            margin: 15px auto;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }

        .note-editor.note-frame {
            border-radius: 8px;
            border: 2px solid #e0e0e0;
        }
        
        .note-editor.note-frame:focus-within {
            border-color: #667eea;
            box-shadow: 0 0 0 0.2rem rgba(102, 126, 234, 0.25);
        }
        
        .note-toolbar {
            background: #f8f9fa;
            border-bottom: 2px solid #e0e0e0;
            border-radius: 8px 8px 0 0;
        }

        /* Responsive */
        @media (max-width: 768px) {
            .blog-management-container {
                padding: 15px;
            }
            .page-header {
                padding: 20px;
            }
            .page-header h1 {
                font-size: 24px;
            }
            .action-card {
                padding: 15px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="blog-management-container">
        <div class="page-header">
            <h1>📚 Blog Management</h1>
            <p>Create, edit, and manage your blog posts</p>
        </div>

        <asp:UpdatePanel ID="UpdatePanelForm" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="action-card" id="formCard">
                    <div class="action-card-header">
                        <h3>
                            <asp:Label ID="lblFormTitle" runat="server" Text="📝 Add New Blog Post" />
                        </h3>
                        <div id="editModeBadge" style="display:none;">
                            <span class="edit-mode-badge">✏️ EDITING MODE</span>
                        </div>
                    </div>

                    <asp:HiddenField ID="hfBlogID" runat="server" />
                    <asp:HiddenField ID="hfCurrentImagePath" runat="server" />

                    <div class="row g-3">
                        <div class="col-md-8">
                            <label class="form-label">Blog Title <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtBlogTitle" runat="server" CssClass="form-control" 
                                placeholder="Enter blog title" MaxLength="200" />
                            <asp:RequiredFieldValidator ID="rfvBlogTitle" runat="server" 
                                ControlToValidate="txtBlogTitle" ErrorMessage="Blog title is required" 
                                CssClass="text-danger" Display="Dynamic" />
                        </div>

                        <div class="col-md-4">
                            <label class="form-label">Display Order</label>
                            <asp:TextBox ID="txtDisplayOrder" runat="server" CssClass="form-control" 
                                Text="0" TextMode="Number" />
                            <asp:RangeValidator ID="rvDisplayOrder" runat="server"
                                ControlToValidate="txtDisplayOrder"
                                MinimumValue="0" MaximumValue="9999" Type="Integer"
                                ErrorMessage="Order must be between 0 and 9999."
                                CssClass="text-danger" Display="Dynamic" />
                        </div>

                        <div class="col-12">
                            <label class="form-label">Short Description (for homepage preview)</label>
                            <asp:TextBox ID="txtBlogDescription" runat="server" CssClass="form-control" 
                                TextMode="MultiLine" Rows="3" MaxLength="500"
                                placeholder="Brief description shown on homepage (optional)"
                                onkeyup="updateCharCount(this, 500, 'charCountDesc')" />
                            <div id="charCountDesc" class="char-counter">0 / 500 characters</div>
                        </div>

                        <div class="col-12">
                            <label class="form-label">Full Blog Content <span class="text-danger">*</span></label>
                            <button type="button" id="btnLoadTemplate" class="btn btn-sm btn-outline-primary float-end mb-2">
                                📋 Load Template
                            </button>
                            <asp:TextBox ID="txtBlogContent" runat="server" TextMode="MultiLine" 
                                CssClass="form-control" style="display:none;" /> 
                            <asp:RequiredFieldValidator ID="rfvBlogContent" runat="server" 
                                ControlToValidate="txtBlogContent" ErrorMessage="Blog content is required" 
                                CssClass="text-danger" Display="Dynamic" />
                        </div>

                        <div class="col-md-6">
                            <label class="form-label">Featured Image</label>
                            <div class="image-upload-area">
                                <i class="fas fa-cloud-upload-alt fa-3x text-secondary mb-2"></i>
                                <p class="mb-2">Upload Blog Image (Max 5MB)</p>
                                <asp:FileUpload ID="fuBlogImage" runat="server" CssClass="form-control" 
                                    accept="image/*" onchange="previewImage(this)" />
                                <small class="text-muted">Accepted: JPG, PNG, GIF, WebP</small>
                            </div>
                            <div class="mt-2">
                                <small class="text-muted">Current Image: 
                                    <asp:Label ID="lblCurrentImage" runat="server" Text="None" />
                                </small>
                            </div>
                        </div>

                        <div class="col-md-6">
                            <label class="form-label">Preview</label>
                            <div style="text-align: center;">
                                <asp:Image ID="imgPreview" runat="server" CssClass="image-preview" 
                                    Visible="false" AlternateText="Blog Image Preview" />
                            </div>
                        </div>

                        <div class="col-md-4">
                            <label class="form-label">Blog Link (URL slug)</label>
                            <asp:TextBox ID="txtBlogLink" runat="server" CssClass="form-control" 
                                placeholder="e.g., /blog/pest-control-tips" />
                            <asp:RegularExpressionValidator ID="revBlogLink" runat="server"
                                ControlToValidate="txtBlogLink"
                                ValidationExpression="^/[a-zA-Z0-9_-]+(/[a-zA-Z0-9_-]+)*$"
                                ErrorMessage="Link must start with '/' and contain only letters, numbers, hyphens, underscores."
                                CssClass="text-danger" Display="Dynamic" />
                        </div>

                        <div class="col-md-4">
                            <label class="form-label">Author</label>
                            <asp:TextBox ID="txtAuthor" runat="server" CssClass="form-control" 
                                Text="RRC Team" MaxLength="100" />
                        </div>

                        <div class="col-md-4">
                            <label class="form-label">Read Time (minutes)</label>
                            <asp:TextBox ID="txtReadTime" runat="server" CssClass="form-control" 
                                Text="5" TextMode="Number" />
                            <asp:RangeValidator ID="rvReadTime" runat="server"
                                ControlToValidate="txtReadTime"
                                MinimumValue="1" MaximumValue="120" Type="Integer"
                                ErrorMessage="Read time must be between 1 and 120 minutes."
                                CssClass="text-danger" Display="Dynamic" />
                        </div>

                        <div class="col-12">
                            <div class="form-check">
                                <asp:CheckBox ID="chkIsActive" runat="server" CssClass="form-check-input" 
                                    Checked="true" />
                                <label class="form-check-label">
                                    Make this blog post active (visible on website)
                                </label>
                            </div>
                        </div>

                        <div class="col-12">
                            <asp:Button ID="btnAddBlog" runat="server" Text="➕ Add Blog Post" 
                                CssClass="btn btn-custom btn-add" OnClick="btnAddBlog_Click" />
                            
                            <asp:Button ID="btnUpdateBlog" runat="server" Text="💾 Update Blog Post" 
                                CssClass="btn btn-custom btn-update" OnClick="btnUpdateBlog_Click" 
                                Visible="false" />
                            
                            <asp:Button ID="btnCancelEdit" runat="server" Text="❌ Cancel" 
                                CssClass="btn btn-custom btn-cancel" OnClick="btnCancelEdit_Click" 
                                CausesValidation="false" Visible="false" />
                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btnAddBlog" />
                <asp:PostBackTrigger ControlID="btnUpdateBlog" />
            </Triggers>
        </asp:UpdatePanel>

        <asp:UpdatePanel ID="UpdatePanelGrid" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="gridview-container">
                    <h3 class="mb-4" style="color: #667eea; font-weight: 600;">
                        📋 All Blog Posts
                    </h3>
                    
                    <asp:GridView ID="gvBlogs" runat="server" AutoGenerateColumns="False" 
                        CssClass="table table-hover" 
                        OnRowCommand="gvBlogs_RowCommand"
                        OnRowDataBound="gvBlogs_RowDataBound"
                        DataKeyNames="BlogID">
                        
                        <Columns>
                            <asp:BoundField DataField="BlogID" HeaderText="ID" ItemStyle-Width="5%" 
                                ItemStyle-HorizontalAlign="Center" HtmlEncode="true" />

                            <asp:TemplateField HeaderText="Image" ItemStyle-Width="10%" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Image ID="imgThumbnail" runat="server" 
                                        ImageUrl='<%# GetSafeImageUrl(Eval("BlogImagePath")) %>'
                                        CssClass="blog-thumbnail" 
                                        AlternateText="Blog Thumbnail" />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Title" ItemStyle-Width="25%">
                                <ItemTemplate>
                                    <div class="blog-title-preview">
                                        <%# Server.HtmlEncode(Eval("BlogTitle") != null ? Eval("BlogTitle").ToString() : "") %>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="DisplayOrder" HeaderText="Order" 
                                ItemStyle-Width="8%" ItemStyle-HorizontalAlign="Center" HtmlEncode="true" />

                            <asp:BoundField DataField="CreatedDate" HeaderText="Created" 
                                DataFormatString="{0:MMM dd, yyyy}" ItemStyle-Width="12%" 
                                ItemStyle-HorizontalAlign="Center" HtmlEncode="true" />

                            <asp:TemplateField HeaderText="Link" ItemStyle-Width="10%" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%# GetSafeLinkButton(Eval("BlogLink")) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Status" ItemStyle-Width="10%" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge badge-active" : "badge badge-inactive" %>'>
                                        <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Actions" ItemStyle-Width="20%" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <div class="action-buttons">
                                        <asp:Button ID="btnEdit" runat="server"
                                            Text="✏️ Edit"
                                            CssClass="btn btn-sm btn-primary"
                                            CommandName="EditBlog"
                                            CommandArgument='<%# Eval("BlogID") %>'
                                            CausesValidation="false" />

                                        <asp:Button ID="btnToggle" runat="server"
                                            Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "🔴 Hide" : "🟢 Show" %>'
                                            CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "btn btn-sm btn-warning" : "btn btn-sm btn-success" %>'
                                            CommandName="ToggleStatus"
                                            CommandArgument='<%# Eval("BlogID") %>'
                                            CausesValidation="false" />

                                        <asp:Button ID="btnDelete" runat="server"
                                            Text="🗑️ Delete"
                                            CssClass="btn btn-sm btn-danger"
                                            CommandName="DeleteBlog"
                                            CommandArgument='<%# Eval("BlogID") %>'
                                            OnClientClick="return showDeleteConfirm(this);"
                                            CausesValidation="false" />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        
                        <EmptyDataTemplate>
                            <div style="text-align: center; padding: 50px; color: #6c757d;">
                                <i class="fas fa-inbox fa-3x mb-3"></i>
                                <p class="mb-0">No blogs found. Add your first blog post using the form above!</p>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>

    </div>
    
    <script src="https://cdn.jsdelivr.net/npm/summernote@0.8.20/dist/summernote-bs5.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        // Function to sync Summernote content
        function syncSummernoteContent() {
            var summernoteElement = $('#<%= txtBlogContent.ClientID %>');
            if (summernoteElement.length && summernoteElement.data('summernote')) {
                var content = summernoteElement.summernote('code');
                summernoteElement.val(content);
                console.log('Content synced. Length: ' + content.length);
                return true;
            }
            console.warn('Summernote not initialized');
            return false;
        }

        // Sync before UpdatePanel postback
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_initializeRequest(function (sender, args) {
                syncSummernoteContent();
            });
        }

        // Sync when buttons are clicked
        $(document).ready(function () {
            $('#<%= btnAddBlog.ClientID %>').on('click', function () {
                syncSummernoteContent();
            });
            $('#<%= btnUpdateBlog.ClientID %>').on('click', function () {
                syncSummernoteContent();
            });
        });

        // Toggle edit mode visual indicator
        function setEditMode(isEditMode) {
            var formCard = document.getElementById('formCard');
            var editBadge = document.getElementById('editModeBadge');
            var formTitle = document.getElementById('<%= lblFormTitle.ClientID %>');
            if (isEditMode) {
                formCard.classList.add('edit-mode');
                editBadge.style.display = 'block';
                if (formTitle) formTitle.classList.add('edit-mode');
            } else {
                formCard.classList.remove('edit-mode');
                editBadge.style.display = 'none';
                if (formTitle) formTitle.classList.remove('edit-mode');
            }
        }

        function showDeleteConfirm(btn) {
            Swal.fire({
                title: 'Are you sure?',
                text: "You won't be able to revert this blog post!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#667eea',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn.name, '');
                }
            });
            return false;
        }

        function updateCharCount(textarea, maxLength, counterId) {
            const currentLength = textarea.value.length;
            const counter = document.getElementById(counterId);
            if (!counter) return;
            counter.textContent = currentLength + ' / ' + maxLength + ' characters';
            if (currentLength >= maxLength) {
                counter.style.color = '#ef4444';
            } else if (currentLength >= maxLength * 0.9) {
                counter.style.color = '#f59e0b';
            } else {
                counter.style.color = '#6c757d';
            }
        }

        function previewImage(input) {
            const preview = document.getElementById('<%= imgPreview.ClientID %>');
            if (!preview) return;
            if (input.files && input.files[0]) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.style.display = 'block';
                }
                reader.readAsDataURL(input.files[0]);
            } else {
                preview.style.display = 'none';
                preview.src = '#';
            }
        }

        function loadBlogTemplate() {
            const template = `
<p style="font-size:18px; line-height:1.6; margin-bottom:40px;">
    Start writing your amazing blog post here...
</p>
<h2>Main Heading</h2>
<p>Some details...</p>
<h3>Sub Heading</h3>
<ul>
    <li>Point 1</li>
    <li>Point 2</li>
</ul>
            `;
            if ($('#<%= txtBlogContent.ClientID %>').data('summernote')) {
                $('#<%= txtBlogContent.ClientID %>').summernote('code', template);
                syncSummernoteContent();
            } else {
                console.error("Summernote not initialized");
            }
        } 

        function initializeSummernote() {
            console.log('Attempting to initialize Summernote...');
            if (typeof jQuery === 'undefined' || typeof jQuery.fn.summernote === 'undefined') {
                console.error('jQuery or Summernote is not loaded');
                return;
            }
            var summernoteElement = $('#<%= txtBlogContent.ClientID %>');
            if (summernoteElement.length === 0) {
                console.error('Summernote textarea not found');
                return;
            }
            if (summernoteElement.data('summernote')) {
                summernoteElement.summernote('destroy');
            }
            summernoteElement.summernote({
                placeholder: 'Enter your full blog content here...',
                height: 400,
                toolbar: [
                    ['style', ['style']],
                    ['font', ['bold', 'italic', 'underline', 'clear']],
                    ['fontname', ['fontname']],
                    ['color', ['color']],
                    ['para', ['ul', 'ol', 'paragraph']],
                    ['table', ['table']],
                    ['insert', ['link', 'picture', 'video']],
                    ['view', ['fullscreen', 'codeview', 'help']]
                ],
                callbacks: {
                    onChange: function(contents, $editable) {
                        $('#<%= txtBlogContent.ClientID %>').val(contents);
                    }
                }
            });
            console.log('Summernote initialized successfully');
        }

        function runPageInitScripts() {
            const descTextarea = document.getElementById('<%= txtBlogDescription.ClientID %>');
            if (descTextarea) {
                updateCharCount(descTextarea, 500, 'charCountDesc');
            }
            initializeSummernote();
            const templateBtn = document.getElementById('btnLoadTemplate');
            if (templateBtn && !templateBtn.hasAttribute('data-click-wired')) {
                templateBtn.addEventListener('click', loadBlogTemplate);
                templateBtn.setAttribute('data-click-wired', 'true'); 
            }
            var updateBtn = document.getElementById('<%= btnUpdateBlog.ClientID %>');
            if (updateBtn && updateBtn.style.display !== 'none') {
                setEditMode(true);
            } else {
                setEditMode(false);
            }
        }

        // Handle both initial load and partial postbacks from UpdatePanels
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            try {
                var prm = Sys.WebForms.PageRequestManager.getInstance();
                prm.add_endRequest(function (sender, args) {
                    var formPanelUpdated = args.get_panelsUpdated().some(panel => 
                        panel.id === '<%= UpdatePanelForm.ClientID %>'
                    );
                    if (formPanelUpdated) {
                        runPageInitScripts();
                    }
                });
                prm.add_pageLoaded(function () {
                    runPageInitScripts();
                });
            } catch (e) {
                console.error("Error attaching to PageRequestManager:", e);
                $(document).ready(runPageInitScripts);
            }
        } else {
            $(document).ready(runPageInitScripts);
        }
    </script>
</asp:Content>