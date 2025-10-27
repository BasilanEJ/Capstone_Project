<%@ Page Title="Content Management System" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="CMS.aspx.cs" Inherits="RRCManagementSystem.CMS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* ============ Container & Header ============ */
        .cms-container {
            padding: 30px;
            max-width: 1400px;
            margin: 0 auto;
        }

        .cms-header {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
            padding: 25px;
            border-radius: 12px;
            margin-bottom: 30px;
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.2);
        }

        .cms-header h1 {
            margin: 0;
            font-size: 28px;
            font-weight: 600;
        }

        .cms-header p {
            margin: 8px 0 0;
            opacity: 0.9;
            font-size: 14px;
        }

        /* ============ Filter Navigation ============ */
        .filter-navigation {
            background: white;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 30px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            border: 1px solid #e2e8f0;
        }

        .filter-title {
            font-size: 16px;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .filter-buttons {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
        }

        .filter-btn {
            padding: 10px 20px;
            border: 2px solid #e2e8f0;
            background: white;
            color: #64748b;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.3s ease;
            font-weight: 500;
            font-size: 14px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .filter-btn:hover {
            border-color: #2563eb;
            color: #2563eb;
            background: #eff6ff;
            transform: translateY(-2px);
        }

        .filter-btn.active {
            background: linear-gradient(135deg, #2563eb, #1e40af);
            color: white;
            border-color: #2563eb;
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
        }

        /* ============ Quick Actions ============ */
        .quick-actions {
            display: flex;
            gap: 10px;
            margin-top: 15px;
            padding-top: 15px;
            border-top: 1px solid #e2e8f0;
        }

        .quick-action-btn {
            padding: 8px 16px;
            border: none;
            background: #f1f5f9;
            color: #475569;
            border-radius: 6px;
            cursor: pointer;
            transition: all 0.3s ease;
            font-size: 13px;
            font-weight: 500;
        }

        .quick-action-btn:hover {
            background: #e2e8f0;
            color: #1e293b;
        }

        /* ============ Section Card ============ */
        .section-card {
            background: white;
            border-radius: 12px;
            padding: 25px;
            margin-bottom: 30px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            border: 1px solid #e2e8f0;
            display: none;
        }

        .section-card.active {
            display: block;
        }

        .section-title {
            font-size: 20px;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        /* ============ Alert Info ============ */
        .alert-info {
            background: #dbeafe;
            border-left: 4px solid #2563eb;
            padding: 12px 16px;
            border-radius: 6px;
            margin-bottom: 20px;
            display: flex;
            align-items: start;
            gap: 10px;
            font-size: 14px;
        }

        .alert-info i {
            color: #2563eb;
            margin-top: 2px;
        }

        /* ============ Form Elements ============ */
        .form-group {
            margin-bottom: 20px;
        }

        .form-label {
            display: block;
            font-weight: 600;
            color: #334155;
            margin-bottom: 8px;
            font-size: 14px;
        }

        .form-input {
            width: 100%;
            padding: 10px 14px;
            border: 1px solid #cbd5e1;
            border-radius: 8px;
            font-size: 14px;
            transition: border-color 0.3s ease;
            box-sizing: border-box;
        }

        .form-input:focus {
            outline: none;
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
        }

        textarea.form-input {
            min-height: 120px;
            resize: vertical;
            font-family: inherit;
        }

        .help-text {
            font-size: 12px;
            color: #64748b;
            margin-top: 6px;
        }

        /* ============ Buttons ============ */
        .btn-update {
            background: linear-gradient(135deg, #10b981, #059669);
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 8px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
        }

        .btn-update:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
        }

        /* ============ Image Preview ============ */
        .image-preview-container {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 20px;
            margin-bottom: 20px;
        }

        .image-preview-box {
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            padding: 15px;
            background: #f8fafc;
            position: relative;
        }

        .dimension-badge {
            position: absolute;
            top: 10px;
            right: 10px;
            background: #2563eb;
            color: white;
            padding: 4px 10px;
            border-radius: 6px;
            font-size: 11px;
            font-weight: 600;
            z-index: 2;
        }

        .preview-label {
            display: block;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 12px;
            font-size: 14px;
        }

        .preview-container {
            position: relative;
            width: 100%;
            background: #e2e8f0;
            border-radius: 8px;
            overflow: hidden;
            margin-bottom: 12px;
            aspect-ratio: 16/9;
        }

        .current-image {
            width: 100%;
            height: 100%;
            object-fit: cover;
            display: block;
        }

        .image-overlay {
            position: absolute;
            bottom: 0;
            left: 0;
            right: 0;
            background: rgba(0, 0, 0, 0.7);
            color: white;
            padding: 8px;
            text-align: center;
            font-size: 12px;
            font-weight: 500;
        }

        .file-upload-wrapper {
            position: relative;
        }

        .file-upload-input {
            width: 100%;
            padding: 10px;
            border: 2px dashed #cbd5e1;
            border-radius: 8px;
            background: white;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .file-upload-input:hover {
            border-color: #2563eb;
            background: #eff6ff;
        }

        .image-info {
            text-align: center;
            color: #10b981;
            font-size: 13px;
            margin-top: 10px;
            font-weight: 500;
        }

      


        /* ============ Video URL Display ============ */
        .video-url-display {
            background: #f1f5f9;
            padding: 15px;
            border-radius: 8px;
            margin: 15px 0;
            border-left: 4px solid #10b981;
        }

        .video-url-display strong {
            display: block;
            color: #1e293b;
            margin-bottom: 8px;
            font-size: 14px;
        }

        .video-url-display div {
            color: #475569;
            font-size: 13px;
            line-height: 1.8;
            font-family: 'Courier New', monospace;
        }

        /* ============ Responsive Design ============ */
        @media screen and (max-width: 768px) {
            .cms-container {
                padding: 15px;
            }

            .cms-header h1 {
                font-size: 22px;
            }

            .filter-buttons {
                flex-direction: column;
            }

            .filter-btn {
                width: 100%;
                justify-content: center;
            }

            .image-preview-container {
                grid-template-columns: 1fr;
            }

        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="cms-container">

        <div class="cms-header">
            <h1><i class="fas fa-cog"></i> Media Management System</h1>
            <p>Manage and update website content, images, and sections</p>
        </div>


        <div class="filter-navigation">
            <div class="filter-title">
                <i class="fas fa-filter"></i> Quick Navigate
            </div>
<div class="filter-buttons">
<button type="button" class="filter-btn active" onclick="return filterSection('all', event);">
    <i class="fas fa-border-all"></i> Show All
</button>
<button type="button" class="filter-btn" onclick="return filterSection('hero', event);">
    <i class="fas fa-image"></i> Hero Banner
</button>
<button type="button" class="filter-btn" onclick="return filterSection('about', event);">
    <i class="fas fa-info-circle"></i> About
</button>
<button type="button" class="filter-btn" onclick="return filterSection('video', event);">
    <i class="fas fa-video"></i> Video
</button>
<button type="button" class="filter-btn" onclick="return filterSection('blogs', event);">
    <i class="fas fa-blog"></i> Blogs
</button>
<button type="button" class="filter-btn" onclick="return filterSection('co', event);">
    <i class="fas fa-certificate"></i> C&O
</button>
</div>
            <div class="quick-actions">
               <button type="button" class="quick-action-btn" onclick="return expandAll(event);">
    <i class="fas fa-expand-alt"></i> Expand All
</button>
<button type="button" class="quick-action-btn" onclick="return collapseAll(event);">
    <i class="fas fa-compress-alt"></i> Collapse All
</button>
            </div>
        </div>

        <asp:UpdatePanel ID="upCMS" runat="server" UpdateMode="Conditional">
            <ContentTemplate>

                <div class="section-card active" data-section="hero">
                    <div class="section-title">
                        <i class="fas fa-image"></i> Hero Banner Section
                    </div>
                    <div class="alert-info">
                        <i class="fas fa-ruler-combined"></i> <strong>Optimal Size:</strong> 1920x1080px (Full HD). This is your homepage hero image.
                    </div>

                    <div class="image-preview-container">
                        <div class="image-preview-box">
                            <span class="dimension-badge">1920 x 1080px</span>
                            <span class="preview-label">Current Hero Banner</span>
                            <div class="preview-container">
                                <asp:Image ID="imgHeroBannerPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Full screen banner</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuHeroBanner" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                            <div class="image-info">✓ Auto-optimized on upload</div>
                        </div>
                    </div>

                    <asp:Button ID="btnUpdateHeroBanner" runat="server" Text="Update Hero Banner" 
                        CssClass="btn-update" OnClick="btnUpdateHeroBanner_Click" />
                </div>

        

                <div class="section-card active" data-section="about">
                    <div class="section-title">
                        <i class="fas fa-user-circle"></i> About Section
                    </div>
                    <div class="alert-info" style="margin-bottom: 20px;">
                        <i class="fas fa-ruler-combined"></i> <strong>Optimal Size:</strong> 600x800px (Portrait format). Perfect for showcasing worker images.
                    </div>
                    <div class="image-preview-container">
                        <div class="image-preview-box">
                            <span class="dimension-badge">600 x 800px</span>
                            <span class="preview-label">About Image (PPE Worker)</span>
                            <div class="preview-container">
                                <asp:Image ID="imgAboutPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Portrait format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuAbout" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                            <div class="image-info">✓ Auto-optimized on upload</div>
                        </div>
                    </div>
                    <asp:Button ID="btnUpdateAbout" runat="server" Text="Update About Image" 
                        CssClass="btn-update" OnClick="btnUpdateAbout_Click" />
                </div>


                 <div class="section-card active" data-section="video">
                    <div class="section-title">
                        <i class="fas fa-video"></i> Video Section
                    </div>
                    <div class="alert-info" style="margin-bottom: 20px;">
                        <i class="fas fa-ruler-combined"></i> <strong>Optimal Size:</strong> 1280x720px (16:9 HD format). Standard video thumbnail dimensions.
                    </div>
                    <div class="video-section">
                        <div class="form-group">
                            <label class="form-label">Video Thumbnail Image</label>
                            <span class="dimension-badge" style="margin-left: 10px;">1280 x 720px</span>
                            <div class="preview-container" style="max-width: 400px; margin: 15px auto;">
                                <asp:Image ID="imgVideoThumbPreview" runat="server" CssClass="current-image" style="max-width: 100%;" />
                                <div class="image-overlay">16:9 HD format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuVideoThumbnail" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                            <div class="image-info">✓ Auto-optimized on upload</div>
                        </div>

                        <div class="form-group">
                            <label class="form-label">Video Platform</label>
                            <asp:DropDownList ID="ddlVideoType" runat="server" CssClass="form-input">
                                <asp:ListItem Text="YouTube" Value="YouTube" Selected="True"></asp:ListItem>
                                <asp:ListItem Text="Vimeo" Value="Vimeo"></asp:ListItem>
                            </asp:DropDownList>
                            <div class="help-text">
                                Select the video platform you want to use
                            </div>
                        </div>

                        <div class="form-group">
                            <label class="form-label">Video URL</label>
                            <asp:TextBox ID="txtVideoUrl" runat="server" CssClass="form-input" 
                                placeholder="Paste full YouTube or Vimeo URL here" />
                            <div class="help-text">
                                <strong>YouTube examples:</strong><br/>
                                • https://www.youtube.com/watch?v=dQw4w9WgXcQ<br/>
                                • https://youtu.be/dQw4w9WgXcQ<br/><br/>
                                <strong>Vimeo examples:</strong><br/>
                                • https://vimeo.com/1009218555
                            </div>
                        </div>
                    </div>
                    <asp:Button ID="btnUpdateVideo" runat="server" Text="Update Video Section" 
                        CssClass="btn-update" OnClick="btnUpdateVideo_Click" />
                </div>


                <div class="section-card active" data-section="blogs">
                    <div class="section-title">
                        <i class="fas fa-blog"></i> Blog Section Images
                    </div>
                    <div class="alert-info">
                        <i class="fas fa-ruler-combined"></i> <strong>Optimal Size:</strong> 640x400px (Landscape format). All blog cards uniformly sized.
                    </div>

                    <div class="image-preview-container">
                        <div class="image-preview-box">
                            <span class="dimension-badge">640 x 400px</span>
                            <span class="preview-label">Blog 1 - DIY vs Professional</span>
                            <div class="preview-container">
                                <asp:Image ID="imgBlog1Preview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Landscape format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuBlog1" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="image-preview-box">
                            <span class="dimension-badge">640 x 400px</span>
                            <span class="preview-label">Blog 2 - Brigada Eskwela</span>
                            <div class="preview-container">
                                <asp:Image ID="imgBlog2Preview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Landscape format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuBlog2" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="image-preview-box">
                            <span class="dimension-badge">640 x 400px</span>
                            <span class="preview-label">Blog 3 - Termite Swarms</span>
                            <div class="preview-container">
                                <asp:Image ID="imgBlog3Preview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Landscape format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuBlog3" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>
                    </div>

                    <asp:Button ID="btnUpdateBlogs" runat="server" Text="Update Blog Images" 
                        CssClass="btn-update" OnClick="btnUpdateBlogs_Click" />
                </div>

                <div class="section-card active" data-section="co">
                    <div class="section-title">
                        <i class="fas fa-certificate"></i> Certifications & Organizations
                    </div>
                    <div class="alert-info">
                        <i class="fas fa-ruler-combined"></i> <strong>Optimal Size:</strong> 1200x400px (Wide banner format). Ideal for displaying multiple certifications.
                    </div>

                    <div class="image-preview-container">
                        <div class="image-preview-box">
                            <span class="dimension-badge">1200 x 400px</span>
                            <span class="preview-label">C&O Banner Image</span>
                            <div class="preview-container">
                                <asp:Image ID="imgCOPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Wide banner</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuCO" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                            <div class="image-info">✓ Auto-optimized on upload</div>
                        </div>
                    </div>

                    <asp:Button ID="btnUpdateCO" runat="server" Text="Update C&O Image" 
                        CssClass="btn-update" OnClick="btnUpdateCO_Click" />
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btnUpdateHeroBanner" />

                <asp:PostBackTrigger ControlID="btnUpdateAbout" />
                <asp:PostBackTrigger ControlID="btnUpdateVideo" />
                <asp:PostBackTrigger ControlID="btnUpdateBlogs" />
                <asp:PostBackTrigger ControlID="btnUpdateCO" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

<script>
    // Filter sections with postback prevention
    function filterSection(category, event) {
        // Prevent form submission if event is passed
        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }

        const sections = document.querySelectorAll('.section-card');
        const filterButtons = document.querySelectorAll('.filter-btn');

        // Remove active class from all buttons
        filterButtons.forEach(btn => btn.classList.remove('active'));

        // Add active class to clicked button
        const clickedButton = event ? event.currentTarget :
            document.querySelector(`.filter-btn[onclick*="filterSection('${category}')"]`);
        if (clickedButton) {
            clickedButton.classList.add('active');
        }

        // Show/hide sections based on category
        sections.forEach(section => {
            if (category === 'all') {
                section.classList.add('active');
            } else {
                const sectionType = section.getAttribute('data-section');
                if (sectionType === category) {
                    section.classList.add('active');
                } else {
                    section.classList.remove('active');
                }
            }
        });

        // Improved scrolling logic
        if (category !== 'all') {
            setTimeout(() => {
                const firstVisible = document.querySelector('.section-card.active');
                if (firstVisible) {
                    const headerOffset = 100;
                    const elementPosition = firstVisible.getBoundingClientRect().top;
                    const offsetPosition = elementPosition + window.pageYOffset - headerOffset;

                    window.scrollTo({
                        top: offsetPosition,
                        behavior: 'smooth'
                    });
                }
            }, 100);
        } else {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }

        return false; // Prevent form submission
    }

    // Navigate directly to a specific section
    function navigateToSection(sectionId, event) {
        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }

        filterSection(sectionId);
        return false;
    }

    // Collapse all sections
    function collapseAll(event) {
        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }

        const sections = document.querySelectorAll('.section-card');
        sections.forEach(section => section.classList.remove('active'));

        const filterButtons = document.querySelectorAll('.filter-btn');
        filterButtons.forEach(btn => btn.classList.remove('active'));

        return false;
    }

    // Expand all sections
    function expandAll(event) {
        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }

        const sections = document.querySelectorAll('.section-card');
        sections.forEach(section => section.classList.add('active'));

        const filterButtons = document.querySelectorAll('.filter-btn');
        filterButtons.forEach(btn => btn.classList.remove('active'));

        const showAllButton = document.querySelector('.filter-btn[onclick*="filterSection(\'all\')"]');
        if (showAllButton) {
            showAllButton.classList.add('active');
        }

        window.scrollTo({ top: 0, behavior: 'smooth' });
        return false;
    }

    // Alternative: Use event delegation (more robust)
    document.addEventListener('DOMContentLoaded', function () {
        // Add click handlers to all filter buttons
        document.querySelectorAll('.filter-btn').forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                const onclickAttr = this.getAttribute('onclick');
                if (onclickAttr) {
                    // Extract the category from onclick attribute
                    const match = onclickAttr.match(/filterSection\('([^']+)'\)/);
                    if (match) {
                        filterSection(match[1]);
                    }
                }
            });
        });

        // Add click handlers to quick action buttons
        document.querySelectorAll('.quick-action-btn').forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
            });
        });
    });
</script>
</asp:Content>
