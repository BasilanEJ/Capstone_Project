<%@ Page Title="Content Management System" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="CMS.aspx.cs" Inherits="RRCManagementSystem.CMS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
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

        .section-card {
            background: white;
            border-radius: 12px;
            padding: 25px;
            margin-bottom: 25px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            border: 1px solid #e2e8f0;
        }

        .section-title {
            font-size: 20px;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 20px;
            padding-bottom: 12px;
            border-bottom: 2px solid #e2e8f0;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .section-title i {
            color: #2563eb;
        }

        .image-preview-container {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
            gap: 20px;
            margin-top: 15px;
        }

        .image-preview-box {
            border: 2px dashed #cbd5e1;
            border-radius: 8px;
            padding: 15px;
            text-align: center;
            transition: all 0.3s ease;
            background: #f8fafc;
        }

        .image-preview-box:hover {
            border-color: #2563eb;
            background: #eff6ff;
        }

        .preview-label {
            font-size: 13px;
            font-weight: 600;
            color: #475569;
            margin-bottom: 10px;
            display: block;
        }

        .current-image {
            width: 100%;
            height: 140px;
            object-fit: cover;
            border-radius: 6px;
            margin-bottom: 10px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .no-image {
            height: 140px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: #e2e8f0;
            border-radius: 6px;
            color: #64748b;
            font-size: 13px;
            margin-bottom: 10px;
        }

        .file-upload-wrapper {
            position: relative;
            margin-top: 10px;
        }

        .file-upload-input {
            width: 100%;
            padding: 8px;
            font-size: 13px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            cursor: pointer;
        }

        .btn-update {
            background: linear-gradient(135deg, #2563eb, #1e40af);
            color: white;
            padding: 12px 32px;
            border: none;
            border-radius: 8px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
            margin-top: 20px;
        }

        .btn-update:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(37, 99, 235, 0.4);
        }

        .video-section {
            background: #f8fafc;
            padding: 20px;
            border-radius: 8px;
            margin-top: 15px;
        }

        .form-group {
            margin-bottom: 15px;
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
            padding: 10px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            font-size: 14px;
            transition: border-color 0.3s ease;
        }

        .form-input:focus {
            outline: none;
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
        }

        .help-text {
            font-size: 12px;
            color: #64748b;
            margin-top: 5px;
        }

        .alert-info {
            background: #dbeafe;
            border-left: 4px solid #2563eb;
            padding: 15px;
            border-radius: 6px;
            margin-bottom: 20px;
            color: #1e40af;
            font-size: 14px;
        }

        .grid-2 {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
        }

        @media (max-width: 768px) {
            .cms-container {
                padding: 15px;
            }

            .image-preview-container {
                grid-template-columns: 1fr;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />

    <div class="cms-container">
        <!-- Header -->
        <div class="cms-header">
            <h1><i class="fas fa-cog"></i> Content Management System</h1>
            <p>Manage homepage images and video content</p>
        </div>

        <!-- Alert Message -->
        <div class="alert-info">
            <i class="fas fa-info-circle"></i> <strong>Note:</strong> Changes will be reflected immediately on the homepage. Recommended image formats: JPG, PNG. Max file size: 5MB.
        </div>

        <asp:UpdatePanel ID="upCMS" runat="server" UpdateMode="Conditional">
            <ContentTemplate>

                <!-- Hero Banner Section -->
                <div class="section-card">
                    <div class="section-title">
                        <i class="fas fa-image"></i> Hero Banner
                    </div>
                    <div class="image-preview-container">
                        <div class="image-preview-box">
                            <span class="preview-label">Main Banner Image</span>
                            <asp:Image ID="imgHeroBannerPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuHeroBanner" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>
                    </div>
                    <asp:Button ID="btnUpdateHeroBanner" runat="server" Text="Update Hero Banner" 
                        CssClass="btn-update" OnClick="btnUpdateHeroBanner_Click" />
                </div>

                <!-- Service Images Section -->
                <div class="section-card">
                    <div class="section-title">
                        <i class="fas fa-th-large"></i> Service Images
                    </div>
                    <p style="margin-bottom: 20px; color: #64748b; font-size: 14px;">
                        Update images for each service card displayed on the homepage
                    </p>

                    <div class="image-preview-container">
                        <!-- Baiting System -->
                        <div class="image-preview-box">
                            <span class="preview-label">Baiting System</span>
                            <asp:Image ID="imgBaitingPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuBaiting" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <!-- Termite Prevention -->
                        <div class="image-preview-box">
                            <span class="preview-label">Termite Prevention</span>
                            <asp:Image ID="imgTermitePreventionPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuTermitePrevention" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <!-- Soil Poisoning -->
                        <div class="image-preview-box">
                            <span class="preview-label">Soil Poisoning</span>
                            <asp:Image ID="imgSoilPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuSoil" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <!-- Reticulation -->
                        <div class="image-preview-box">
                            <span class="preview-label">Reticulation</span>
                            <asp:Image ID="imgReticulationPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuReticulation" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <!-- Mound Demolition -->
                        <div class="image-preview-box">
                            <span class="preview-label">Mound Demolition</span>
                            <asp:Image ID="imgMoundPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuMound" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <!-- General Pest -->
                        <div class="image-preview-box">
                            <span class="preview-label">General Pest Control</span>
                            <asp:Image ID="imgGeneralPestPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuGeneralPest" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <!-- Tick & Fleas -->
                        <div class="image-preview-box">
                            <span class="preview-label">Tick & Fleas</span>
                            <asp:Image ID="imgTickFleasPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuTickFleas" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <!-- Bedbugs -->
                        <div class="image-preview-box">
                            <span class="preview-label">Bedbugs Control</span>
                            <asp:Image ID="imgBedbugsPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuBedbugs" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <!-- Rats -->
                        <div class="image-preview-box">
                            <span class="preview-label">Rat/Rodents Control</span>
                            <asp:Image ID="imgRatsPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuRats" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>
                    </div>

                    <asp:Button ID="btnUpdateServices" runat="server" Text="Update Service Images" 
                        CssClass="btn-update" OnClick="btnUpdateServices_Click" />
                </div>

                <!-- About Section Image -->
                <div class="section-card">
                    <div class="section-title">
                        <i class="fas fa-user-circle"></i> About Section
                    </div>
                    <div class="image-preview-container">
                        <div class="image-preview-box">
                            <span class="preview-label">About Image (PPE Worker)</span>
                            <asp:Image ID="imgAboutPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuAbout" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>
                    </div>
                    <asp:Button ID="btnUpdateAbout" runat="server" Text="Update About Image" 
                        CssClass="btn-update" OnClick="btnUpdateAbout_Click" />
                </div>

                <!-- Video Section -->
                <div class="section-card">
                    <div class="section-title">
                        <i class="fas fa-video"></i> Video Section
                    </div>
                    <div class="video-section">
                        <div class="form-group">
                            <label class="form-label">Video Thumbnail Image</label>
                            <asp:Image ID="imgVideoThumbPreview" runat="server" CssClass="current-image" style="max-width: 400px;" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuVideoThumbnail" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="form-group">
                            <label class="form-label">Vimeo Video ID</label>
                            <asp:TextBox ID="txtVimeoVideoId" runat="server" CssClass="form-input" 
                                placeholder="e.g., 1009218555" />
                            <div class="help-text">
                                Enter only the video ID from your Vimeo URL. Example: vimeo.com/1009218555 → enter "1009218555"
                            </div>
                        </div>
                    </div>
                    <asp:Button ID="btnUpdateVideo" runat="server" Text="Update Video Section" 
                        CssClass="btn-update" OnClick="btnUpdateVideo_Click" />
                </div>

                <!-- C&O Section -->
                <div class="section-card">
                    <div class="section-title">
                        <i class="fas fa-certificate"></i> Certifications & Organizations
                    </div>
                    <div class="image-preview-container">
                        <div class="image-preview-box">
                            <span class="preview-label">C&O Banner Image</span>
                            <asp:Image ID="imgCOPreview" runat="server" CssClass="current-image" />
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuCO" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>
                    </div>
                    <asp:Button ID="btnUpdateCO" runat="server" Text="Update C&O Image" 
                        CssClass="btn-update" OnClick="btnUpdateCO_Click" />
                </div>

            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btnUpdateHeroBanner" />
                <asp:PostBackTrigger ControlID="btnUpdateServices" />
                <asp:PostBackTrigger ControlID="btnUpdateAbout" />
                <asp:PostBackTrigger ControlID="btnUpdateVideo" />
                <asp:PostBackTrigger ControlID="btnUpdateCO" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>