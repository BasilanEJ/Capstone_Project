<%@ Page Title="Content Management System" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="CMS.aspx.cs" Inherits="RRCManagementSystem.CMS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Existing Global Styles */
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

        .filter-btn i {
            font-size: 16px;
        }

        /* Quick action buttons */
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

        .section-card {
            background: white;
            border-radius: 12px;
            padding: 25px;
            margin-bottom: 30px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            border: 1px solid #e2e8f0;
            display: none; /* Hidden by default */
        }

        .section-card.active {
            display: block;
        }
          .table {
        border-collapse: separate;
        border-spacing: 0 8px;
    }

          /* ============ FAQ Grid Styles ============ */
.faq-grid {
    width: 100%;
    border-collapse: separate;
    border-spacing: 0 8px;
}

.faq-header {
    padding: 12px;
    border-radius: 8px 8px 0 0;
}

.faq-row, .faq-row-alt {
    box-shadow: 0 2px 4px rgba(0,0,0,0.05);
    border-radius: 8px;
}

.faq-grid tr:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.faq-question {
    color: #1e293b;
    font-size: 15px;
    padding: 12px;
    display: block;
}

.faq-answer {
    padding: 12px;
    color: #64748b;
    font-size: 14px;
    line-height: 1.6;
}

.faq-move-btn {
    margin: 2px;
    padding: 4px 8px;
    border-radius: 4px;
}

.faq-status-btn {
    padding: 4px 12px;
    border-radius: 4px;
    font-size: 12px;
}

.faq-actions {
    padding: 8px;
    display: flex;
    gap: 4px;
    justify-content: center;
}

.faq-action-btn {
    padding: 6px 12px;
    border-radius: 6px;
}

.faq-empty {
    padding: 40px;
    text-align: center;
    color: #64748b;
}

.faq-empty i {
    font-size: 48px;
    margin-bottom: 16px;
    opacity: 0.5;
}

.faq-empty p {
    font-size: 16px;
    margin: 0;
}

    .table th {
        padding: 12px;
        background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
        color: white;
        font-weight: 600;
        text-align: left;
        border: none;
    }

    .table td {
        padding: 12px;
        vertical-align: middle;
        border: none;
        background: white;
    }

    .table tr {
        box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        border-radius: 8px;
        transition: all 0.3s ease;
    }

    .table tr:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }


        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
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

        /* ============ Image Preview & Upload (No Change) ============ */
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
        
        /* ============ Service Tabs (New Styles for Organization) ============ */
        .service-tabs {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            margin-bottom: 25px;
            padding: 10px;
            background: #f1f5f9;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            overflow-x: auto;
        }

        .service-tabs .tab-btn {
            padding: 10px 15px;
            border: none;
            background: transparent;
            color: #475569;
            border-radius: 6px;
            cursor: pointer;
            transition: all 0.2s ease;
            font-weight: 500;
            font-size: 14px;
            white-space: nowrap; /* Prevent wrapping in tabs */
        }

        .service-tabs .tab-btn:hover {
            background: #e2e8f0;
            color: #1e293b;
        }

        .service-tabs .tab-btn.active {
            background: white;
            box-shadow: 0 1px 4px rgba(0,0,0,0.1);
            color: #2563eb;
            font-weight: 600;
            border: 1px solid #cbd5e1;
        }

        .service-tab-content .content-editor {
            display: none; /* Hide all content editors by default */
            margin-top: 0;
            margin-bottom: 20px;
        }

        .service-tab-content .content-editor.active {
            display: block; /* Show the active one */
            animation: fadeIn 0.3s ease-out;
        }
        
        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }

        /* Existing Form Styles (Rest of original CSS kept for context) */
        .video-section {
            background: #f8fafc;
            padding: 20px;
            border-radius: 8px;
            margin-top: 15px;
            margin-bottom: 0;
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

        .dimension-badge {
            display: inline-block;
            background: linear-gradient(135deg, #2563eb, #1e40af);
            color: white;
            padding: 4px 10px;
            border-radius: 12px;
            font-size: 11px;
            font-weight: 600;
            margin-bottom: 8px;
            letter-spacing: 0.3px;
        }

        .image-info {
            font-size: 11px;
            color: #64748b;
            margin-top: 5px;
            text-align: center;
        }

        .preview-container {
            position: relative;
        }

        .image-overlay {
            position: absolute;
            bottom: 10px;
            left: 50%;
            transform: translateX(-50%);
            background: rgba(0, 0, 0, 0.7);
            color: white;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 10px;
            opacity: 0;
            transition: opacity 0.3s ease;
            white-space: nowrap;
        }

        .image-preview-box:hover .image-overlay {
            opacity: 1;
        }

        .content-editor {
            background: linear-gradient(to bottom, #f8fafc 0%, #ffffff 100%);
            padding: 20px;
            border-radius: 8px;
            /* Removed margin-bottom: 25px; here as it's handled by service-tab-content */
            border-left: 4px solid #2563eb;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        }

        .content-title {
            font-size: 16px;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 2px solid #e2e8f0;
        }

        @media (max-width: 768px) {
            .cms-container {
                padding: 15px;
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
            <h1><i class="fas fa-cog"></i> Content Management System</h1>
            <p>Manage homepage images, videos, and service content</p>
        </div>

        <div class="filter-navigation">
            <div class="filter-title">
                <i class="fas fa-filter"></i>
                <span>Filter Content Sections</span>
            </div>
            <div class="filter-buttons">
                <button type="button" class="filter-btn active" onclick="filterSection('all')">
                    <i class="fas fa-th"></i>
                    <span>All Sections</span>
                </button>
                <button type="button" class="filter-btn" onclick="filterSection('hero')">
                    <i class="fas fa-image"></i>
                    <span>Hero Banner</span>
                </button>
                <button type="button" class="filter-btn" onclick="filterSection('services')">
                    <i class="fas fa-th-large"></i>
                    <span>Service Images</span>
                </button>
                <button type="button" class="filter-btn" onclick="filterSection('content')">
                    <i class="fas fa-edit"></i>
                    <span>Service Content</span>
                </button>
                <button type="button" class="filter-btn" onclick="filterSection('about')">
                    <i class="fas fa-user-circle"></i>
                    <span>About Section</span>
                </button>
                <button type="button" class="filter-btn" onclick="filterSection('video')">
                    <i class="fas fa-video"></i>
                    <span>Video Section</span>
                </button>
                <button type="button" class="filter-btn" onclick="filterSection('blogs')">
                    <i class="fas fa-blog"></i>
                    <span>Blog Images</span>
                </button>
                <button type="button" class="filter-btn" onclick="filterSection('co')">
                    <i class="fas fa-certificate"></i>
                    <span>Certifications</span>
                </button>
                <button type="button" class="filter-btn" onclick="filterSection('faq')">
    <i class="fas fa-question-circle"></i>
    <span>FAQ Management</span>
</button>
            </div>
            <div class="quick-actions">
                <button type="button" class="quick-action-btn" onclick="collapseAll()">
                    <i class="fas fa-compress-alt"></i> Collapse All
                </button>
                <button type="button" class="quick-action-btn" onclick="expandAll()">
                    <i class="fas fa-expand-alt"></i> Expand All
                </button>
            </div>
        </div>

        <div class="alert-info">
            <i class="fas fa-info-circle"></i> <strong>Note:</strong> All uploaded images will be automatically optimized and resized to maintain design consistency. Recommended formats: JPG, PNG. Max file size: 5MB.
        </div>

        <asp:UpdatePanel ID="upCMS" runat="server" UpdateMode="Conditional">
            <ContentTemplate>

                <div class="section-card active" data-section="hero">
                    <div class="section-title">
                        <i class="fas fa-image"></i> Hero Banner
                    </div>
                    <div class="alert-info" style="margin-bottom: 20px;">
                        <i class="fas fa-ruler-combined"></i> <strong>Optimal Size:</strong> 1920x600px (Wide landscape format). Images will be automatically resized while maintaining quality.
                    </div>
                    <div class="image-preview-container">
                        <div class="image-preview-box">
                            <span class="dimension-badge">1920 x 600px</span>
                            <span class="preview-label">Main Banner Image</span>
                            <div class="preview-container">
                                <asp:Image ID="imgHeroBannerPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Wide landscape banner</div>
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

                <div class="section-card active" data-section="services">
                    <div class="section-title">
                        <i class="fas fa-th-large"></i> Service Images
                    </div>
                    <div class="alert-info" style="margin-bottom: 20px;">
                        <i class="fas fa-ruler-combined"></i> <strong>Optimal Size:</strong> 400x400px (Square format). All service images will be automatically resized to match perfectly.
                    </div>
                    <p style="margin-bottom: 20px; color: #64748b; font-size: 14px;">
                        Update images for each service card displayed on the homepage
                    </p>

                    <div class="image-preview-container">
                        <div class="image-preview-box">
                            <span class="dimension-badge">400 x 400px</span>
                            <span class="preview-label">Baiting System</span>
                            <div class="preview-container">
                                <asp:Image ID="imgBaitingPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Square format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuBaiting" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="image-preview-box">
                            <span class="dimension-badge">400 x 400px</span>
                            <span class="preview-label">Termite Prevention</span>
                            <div class="preview-container">
                                <asp:Image ID="imgTermitePreventionPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Square format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuTermitePrevention" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="image-preview-box">
                            <span class="dimension-badge">400 x 400px</span>
                            <span class="preview-label">Soil Poisoning</span>
                            <div class="preview-container">
                                <asp:Image ID="imgSoilPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Square format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuSoil" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="image-preview-box">
                            <span class="dimension-badge">400 x 400px</span>
                            <span class="preview-label">Reticulation</span>
                            <div class="preview-container">
                                <asp:Image ID="imgReticulationPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Square format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuReticulation" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="image-preview-box">
                            <span class="dimension-badge">400 x 400px</span>
                            <span class="preview-label">Mound Demolition</span>
                            <div class="preview-container">
                                <asp:Image ID="imgMoundPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Square format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuMound" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="image-preview-box">
                            <span class="dimension-badge">400 x 400px</span>
                            <span class="preview-label">General Pest Control</span>
                            <div class="preview-container">
                                <asp:Image ID="imgGeneralPestPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Square format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuGeneralPest" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="image-preview-box">
                            <span class="dimension-badge">400 x 400px</span>
                            <span class="preview-label">Tick & Fleas</span>
                            <div class="preview-container">
                                <asp:Image ID="imgTickFleasPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Square format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuTickFleas" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="image-preview-box">
                            <span class="dimension-badge">400 x 400px</span>
                            <span class="preview-label">Bedbugs Control</span>
                            <div class="preview-container">
                                <asp:Image ID="imgBedbugsPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Square format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuBedbugs" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>

                        <div class="image-preview-box">
                            <span class="dimension-badge">400 x 400px</span>
                            <span class="preview-label">Rat/Rodents Control</span>
                            <div class="preview-container">
                                <asp:Image ID="imgRatsPreview" runat="server" CssClass="current-image" />
                                <div class="image-overlay">Square format</div>
                            </div>
                            <div class="file-upload-wrapper">
                                <asp:FileUpload ID="fuRats" runat="server" CssClass="file-upload-input" accept="image/*" />
                            </div>
                        </div>
                    </div>

                    <div class="image-info">✓ All images auto-optimized on upload</div>

                    <asp:Button ID="btnUpdateServices" runat="server" Text="Update Service Images" 
                        CssClass="btn-update" OnClick="btnUpdateServices_Click" />
                </div>

                <div class="section-card active" data-section="content">
                    <div class="section-title">
                        <i class="fas fa-edit"></i> Service Modal Content
                    </div>
                    <div class="alert-info" style="margin-bottom: 20px;">
                        <i class="fas fa-info-circle"></i> <strong>Instructions:</strong> 
                        Manage text content for service modals. Use double line breaks for paragraphs. 
                        Separate bullet points with the <strong>|</strong> (pipe) character.
                    </div>

                    <div class="service-tabs">
                        <button type="button" class="tab-btn active" data-tab="baiting" onclick="switchServiceTab('baiting')">🛡️ Baiting System</button>
                        <button type="button" class="tab-btn" data-tab="termite" onclick="switchServiceTab('termite')">🔰 Termite Prevention</button>
                        <button type="button" class="tab-btn" data-tab="soil" onclick="switchServiceTab('soil')">🏗️ Soil Poisoning</button>
                        <button type="button" class="tab-btn" data-tab="reticulation" onclick="switchServiceTab('reticulation')">⚙️ Reticulation</button>
                        <button type="button" class="tab-btn" data-tab="mound" onclick="switchServiceTab('mound')">🎯 Mound Demolition</button>
                        <button type="button" class="tab-btn" data-tab="generalpest" onclick="switchServiceTab('generalpest')">🐜 General Pest Control</button>
                        <button type="button" class="tab-btn" data-tab="tickfleas" onclick="switchServiceTab('tickfleas')">🐕 Tick & Fleas</button>
                        <button type="button" class="tab-btn" data-tab="bedbugs" onclick="switchServiceTab('bedbugs')">🛏️ Bedbugs Control</button>
                        <button type="button" class="tab-btn" data-tab="rats" onclick="switchServiceTab('rats')">🐀 Rat & Rodents Control</button>
                    </div>

                    <div class="service-tab-content">
                        <div class="content-editor active" data-content="baiting">
                            <h5 class="content-title">🛡️ Baiting System Content</h5>
                            
                            <div class="form-group">
                                <label class="form-label">Modal Title</label>
                                <asp:TextBox ID="txtBaitingTitle" runat="server" CssClass="form-input" 
                                    placeholder="🛡️ Baiting System" MaxLength="200" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Description (use double line breaks for new paragraphs)</label>
                                <asp:TextBox ID="txtBaitingDescription" runat="server" TextMode="MultiLine" 
                                    Rows="6" CssClass="form-input" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Bullet Points (separate with |)</label>
                                <asp:TextBox ID="txtBaitingBullets" runat="server" TextMode="MultiLine" 
                                    Rows="3" CssClass="form-input" 
                                    placeholder="Point 1|Point 2|Point 3|Point 4" />
                            </div>
                        </div>

                        <div class="content-editor" data-content="termite">
                            <h5 class="content-title">🔰 Termite Prevention Content</h5>
                            
                            <div class="form-group">
                                <label class="form-label">Modal Title</label>
                                <asp:TextBox ID="txtTermitePreventionTitle" runat="server" CssClass="form-input" 
                                    placeholder="🔰 Termite Prevention" MaxLength="200" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Description</label>
                                <asp:TextBox ID="txtTermitePreventionDescription" runat="server" TextMode="MultiLine" 
                                    Rows="6" CssClass="form-input" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Bullet Points (separate with |)</label>
                                <asp:TextBox ID="txtTermitePreventionBullets" runat="server" TextMode="MultiLine" 
                                    Rows="3" CssClass="form-input" />
                            </div>
                        </div>

                        <div class="content-editor" data-content="soil">
                            <h5 class="content-title">🏗️ Soil Poisoning Treatment Content</h5>
                            
                            <div class="form-group">
                                <label class="form-label">Modal Title</label>
                                <asp:TextBox ID="txtSoilTitle" runat="server" CssClass="form-input" 
                                    placeholder="🏗️ Soil Poisoning Treatment" MaxLength="200" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Description</label>
                                <asp:TextBox ID="txtSoilDescription" runat="server" TextMode="MultiLine" 
                                    Rows="6" CssClass="form-input" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Bullet Points (separate with |)</label>
                                <asp:TextBox ID="txtSoilBullets" runat="server" TextMode="MultiLine" 
                                    Rows="3" CssClass="form-input" />
                            </div>
                        </div>

                        <div class="content-editor" data-content="reticulation">
                            <h5 class="content-title">⚙️ Reticulation System Content</h5>
                            
                            <div class="form-group">
                                <label class="form-label">Modal Title</label>
                                <asp:TextBox ID="txtReticulationTitle" runat="server" CssClass="form-input" 
                                    placeholder="⚙️ Reticulation System" MaxLength="200" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Description</label>
                                <asp:TextBox ID="txtReticulationDescription" runat="server" TextMode="MultiLine" 
                                    Rows="6" CssClass="form-input" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Bullet Points (separate with |)</label>
                                <asp:TextBox ID="txtReticulationBullets" runat="server" TextMode="MultiLine" 
                                    Rows="3" CssClass="form-input" />
                            </div>
                        </div>

                        <div class="content-editor" data-content="mound">
                            <h5 class="content-title">🎯 Mound Demolition Content</h5>
                            
                            <div class="form-group">
                                <label class="form-label">Modal Title</label>
                                <asp:TextBox ID="txtMoundTitle" runat="server" CssClass="form-input" 
                                    placeholder="🎯 Mound Demolition" MaxLength="200" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Description</label>
                                <asp:TextBox ID="txtMoundDescription" runat="server" TextMode="MultiLine" 
                                    Rows="6" CssClass="form-input" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Bullet Points (separate with |)</label>
                                <asp:TextBox ID="txtMoundBullets" runat="server" TextMode="MultiLine" 
                                    Rows="3" CssClass="form-input" />
                            </div>
                        </div>

                        <div class="content-editor" data-content="generalpest">
                            <h5 class="content-title">🐜 General Pest Control Content</h5>
                            
                            <div class="form-group">
                                <label class="form-label">Modal Title</label>
                                <asp:TextBox ID="txtGeneralPestTitle" runat="server" CssClass="form-input" 
                                    placeholder="🐜 General Pest Control" MaxLength="200" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Description</label>
                                <asp:TextBox ID="txtGeneralPestDescription" runat="server" TextMode="MultiLine" 
                                    Rows="6" CssClass="form-input" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Bullet Points (separate with |)</label>
                                <asp:TextBox ID="txtGeneralPestBullets" runat="server" TextMode="MultiLine" 
                                    Rows="3" CssClass="form-input" />
                            </div>
                        </div>

                        <div class="content-editor" data-content="tickfleas">
                            <h5 class="content-title">🐕 Tick & Fleas Control Content</h5>
                            
                            <div class="form-group">
                                <label class="form-label">Modal Title</label>
                                <asp:TextBox ID="txtTickFleasTitle" runat="server" CssClass="form-input" 
                                    placeholder="🐕 Tick & Fleas Control" MaxLength="200" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Description</label>
                                <asp:TextBox ID="txtTickFleasDescription" runat="server" TextMode="MultiLine" 
                                    Rows="6" CssClass="form-input" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Bullet Points (separate with |)</label>
                                <asp:TextBox ID="txtTickFleasBullets" runat="server" TextMode="MultiLine" 
                                    Rows="3" CssClass="form-input" />
                            </div>
                        </div>

                        <div class="content-editor" data-content="bedbugs">
                            <h5 class="content-title">🛏️ Bedbugs Control Content</h5>
                            
                            <div class="form-group">
                                <label class="form-label">Modal Title</label>
                                <asp:TextBox ID="txtBedbugsTitle" runat="server" CssClass="form-input" 
                                    placeholder="🛏️ Bedbugs Control" MaxLength="200" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Description</label>
                                <asp:TextBox ID="txtBedbugsDescription" runat="server" TextMode="MultiLine" 
                                    Rows="6" CssClass="form-input" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Bullet Points (separate with |)</label>
                                <asp:TextBox ID="txtBedbugsBullets" runat="server" TextMode="MultiLine" 
                                    Rows="3" CssClass="form-input" />
                            </div>
                        </div>

                        <div class="content-editor" data-content="rats">
                            <h5 class="content-title">🐀 Rat & Rodents Control Content</h5>
                            
                            <div class="form-group">
                                <label class="form-label">Modal Title</label>
                                <asp:TextBox ID="txtRatsTitle" runat="server" CssClass="form-input" 
                                    placeholder="🐀 Rat & Rodents Control" MaxLength="200" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Description</label>
                                <asp:TextBox ID="txtRatsDescription" runat="server" TextMode="MultiLine" 
                                    Rows="6" CssClass="form-input" />
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Bullet Points (separate with |)</label>
                                <asp:TextBox ID="txtRatsBullets" runat="server" TextMode="MultiLine" 
                                    Rows="3" CssClass="form-input" />
                            </div>
                        </div>
                    </div>

                    <asp:Button ID="btnUpdateServiceContent" runat="server" Text="Update All Service Content" 
                        CssClass="btn-update" OnClick="btnUpdateServiceContent_Click" />
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
                    <div class="alert-info" style="margin-bottom: 20px;">
                        <i class="fas fa-ruler-combined"></i> <strong>Optimal Size:</strong> 640x400px (Landscape format). All blog cards will be uniformly sized.
                    </div>
                    <p style="margin-bottom: 20px; color: #64748b; font-size: 14px;">
                        Update images for the three blog cards on the homepage
                    </p>

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
                    <div class="image-info">✓ All images auto-optimized on upload</div>

                    <asp:Button ID="btnUpdateBlogs" runat="server" Text="Update Blog Images" 
                        CssClass="btn-update" OnClick="btnUpdateBlogs_Click" />
                </div>

                <div class="section-card active" data-section="co">
                    <div class="section-title">
                        <i class="fas fa-certificate"></i> Certifications & Organizations
                    </div>
                    <div class="alert-info" style="margin-bottom: 20px;">
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

        <div class="section-card active" data-section="faq">
    <div class="section-title">
        <i class="fas fa-question-circle"></i> FAQ Management
    </div>
    <div class="alert-info" style="margin-bottom: 20px;">
        <i class="fas fa-info-circle"></i> <strong>Instructions:</strong> 
        Manage frequently asked questions for the chat assistant. Add, edit, remove, or reorder questions.
    </div>

    <asp:UpdatePanel ID="upFaqSection" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div style="background: linear-gradient(to bottom, #f8fafc 0%, #ffffff 100%); padding: 20px; border-radius: 8px; margin-bottom: 25px; border-left: 4px solid #2563eb; box-shadow: 0 2px 4px rgba(0,0,0,0.05);">
                <h5 style="font-size: 16px; font-weight: 600; color: #1e293b; margin-bottom: 15px; padding-bottom: 10px; border-bottom: 2px solid #e2e8f0;">
                    <i class="fas fa-plus-circle"></i> Add New FAQ
                </h5>

                <div class="form-group">
                    <label class="form-label">Question *</label>
                    <asp:TextBox ID="txtNewFaqQuestion" runat="server" CssClass="form-input" 
                        placeholder="Enter the question..." MaxLength="500" />
                    <div class="help-text">Keep questions clear and concise</div>
                </div>

                <div class="form-group">
                    <label class="form-label">Answer *</label>
                    <asp:TextBox ID="txtNewFaqAnswer" runat="server" TextMode="MultiLine" 
                        Rows="4" CssClass="form-input" placeholder="Enter the answer..." />
                    <div class="help-text">Provide detailed but easy-to-understand answers</div>
                </div>

                <asp:Button ID="btnAddFaq" runat="server" Text="Add FAQ" 
                    CssClass="btn-update" OnClick="btnAddFaq_Click" 
                    style="background: linear-gradient(135deg, #10b981, #059669);" />
            </div>

            <div style="margin-top: 30px;">
                <h5 style="font-size: 16px; font-weight: 600; color: #1e293b; margin-bottom: 15px;">
                    <i class="fas fa-list"></i> Current FAQs
                </h5>

                <asp:GridView ID="gvFaqs" runat="server" AutoGenerateColumns="False" 
                    CssClass="table table-hover faq-grid" 
                    OnRowCommand="gvFaqs_RowCommand"
                    OnRowEditing="gvFaqs_RowEditing"
                    OnRowCancelingEdit="gvFaqs_RowCancelingEdit"
                    OnRowUpdating="gvFaqs_RowUpdating"
                    OnRowDeleting="gvFaqs_RowDeleting"
                    DataKeyNames="ID"
                    GridLines="None">
                    
                    <HeaderStyle BackColor="#2563eb" ForeColor="White" Font-Bold="True" CssClass="faq-header" />
                    <RowStyle BackColor="White" CssClass="faq-row" />
                    <AlternatingRowStyle BackColor="#f8fafc" CssClass="faq-row-alt" />

                    <Columns>
                        <asp:TemplateField HeaderText="Order" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnMoveUp" runat="server" 
                                    CommandName="MoveUp" 
                                    CommandArgument='<%# Eval("ID") %>'
                                    CssClass="btn btn-sm btn-outline-primary faq-move-btn"
                                    ToolTip="Move Up">
                                    <i class="fas fa-arrow-up"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnMoveDown" runat="server" 
                                    CommandName="MoveDown" 
                                    CommandArgument='<%# Eval("ID") %>'
                                    CssClass="btn btn-sm btn-outline-primary faq-move-btn"
                                    ToolTip="Move Down">
                                    <i class="fas fa-arrow-down"></i>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Question">
                            <ItemTemplate>
                                <strong class="faq-question">
                                    <%# Eval("Question") %>
                                </strong>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtEditQuestion" runat="server" 
                                    Text='<%# Bind("Question") %>'
                                    CssClass="form-control" 
                                    MaxLength="500" />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Answer">
                            <ItemTemplate>
                                <div class="faq-answer">
                                    <%# Eval("Answer") %>
                                </div>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtEditAnswer" runat="server" 
                                    Text='<%# Bind("Answer") %>'
                                    TextMode="MultiLine" 
                                    Rows="3"
                                    CssClass="form-control" />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Status" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnToggleActive" runat="server"
                                    CommandName="ToggleActive"
                                    CommandArgument='<%# Eval("ID") %>'
                                    CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "btn btn-sm btn-success faq-status-btn" : "btn btn-sm btn-secondary faq-status-btn" %>'>
                                    <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="180px" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <div class="faq-actions">
                                    <asp:LinkButton ID="btnEdit" runat="server" 
                                        CommandName="Edit"
                                        CssClass="btn btn-sm btn-primary faq-action-btn">
                                        <i class="fas fa-edit"></i> Edit
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnDelete" runat="server" 
                                        CommandName="Delete"
                                        CssClass="btn btn-sm btn-danger faq-action-btn"
                                        OnClientClick="return confirm('Are you sure you want to delete this FAQ?');">
                                        <i class="fas fa-trash"></i> Delete
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <div class="faq-actions">
                                    <asp:LinkButton ID="btnUpdate" runat="server" 
                                        CommandName="Update"
                                        CssClass="btn btn-sm btn-success faq-action-btn">
                                        <i class="fas fa-check"></i> Save
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnCancel" runat="server" 
                                        CommandName="Cancel"
                                        CssClass="btn btn-sm btn-secondary faq-action-btn">
                                        <i class="fas fa-times"></i> Cancel
                                    </asp:LinkButton>
                                </div>
                            </EditItemTemplate>
                        </asp:TemplateField>
                    </Columns>

                    <EmptyDataTemplate>
                        <div class="faq-empty">
                            <i class="fas fa-inbox"></i>
                            <p>No FAQs found. Add your first FAQ above!</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>

            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btnUpdateHeroBanner" />
                <asp:PostBackTrigger ControlID="btnUpdateServices" />
                <asp:PostBackTrigger ControlID="btnUpdateServiceContent" />
                <asp:PostBackTrigger ControlID="btnUpdateAbout" />
                <asp:PostBackTrigger ControlID="btnUpdateVideo" />
                <asp:PostBackTrigger ControlID="btnUpdateBlogs" />
                <asp:PostBackTrigger ControlID="btnUpdateCO" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        // Filter sections based on selected category (Original Logic - kept for compatibility)
        function filterSection(category) {
            // Get all section cards
            const sections = document.querySelectorAll('.section-card');
            const filterButtons = document.querySelectorAll('.filter-btn');
            
            // Remove active class from all buttons
            filterButtons.forEach(btn => btn.classList.remove('active'));
            
            // Add active class to clicked button
            // Note: event is available globally in modern browsers but classic ASP.NET might rely on it.
            // Using the category to find the button is more robust in this context.
            const clickedButton = document.querySelector(`.filter-btn[onclick*="filterSection('${category}')"]`);
            if (clickedButton) {
                clickedButton.classList.add('active');
            }
            
            // Show/hide sections based on filter
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
            
            // Scroll to first visible section
            setTimeout(() => {
                const firstVisible = document.querySelector('.section-card.active');
                if (firstVisible && category !== 'all') {
                    firstVisible.scrollIntoView({ behavior: 'smooth', block: 'start', inline: 'nearest' });
                }
            }, 100);
        }

        // NEW: Function to switch tabs within the Service Content section
        function switchServiceTab(tabName) {
            const tabButtons = document.querySelectorAll('.service-tabs .tab-btn');
            const tabContents = document.querySelectorAll('.service-tab-content .content-editor');

            // Deactivate all
            tabButtons.forEach(btn => btn.classList.remove('active'));
            tabContents.forEach(content => content.classList.remove('active'));

            // Activate the clicked button and content
            document.querySelector(`.service-tabs .tab-btn[data-tab='${tabName}']`).classList.add('active');
            document.querySelector(`.service-tab-content .content-editor[data-content='${tabName}']`).classList.add('active');
        }
        
        // Collapse all sections (Original Logic - kept)
        function collapseAll() {
            const sections = document.querySelectorAll('.section-card');
            sections.forEach(section => section.classList.remove('active'));
            
            // Update button states
            const filterButtons = document.querySelectorAll('.filter-btn');
            filterButtons.forEach(btn => btn.classList.remove('active'));
        }
        
        // Expand all sections (Original Logic - kept)
        function expandAll() {
            const sections = document.querySelectorAll('.section-card');
            sections.forEach(section => section.classList.add('active'));
            
            // Update button states
            const filterButtons = document.querySelectorAll('.filter-btn');
            filterButtons.forEach(btn => btn.classList.remove('active'));
            filterButtons[0].classList.add('active'); // Activate "All Sections" button
            
            // Scroll to top
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
        
        // Initializer for the service tabs on page load or after an UpdatePanel partial postback
        function initializeServiceTabs() {
            const firstTabButton = document.querySelector('.service-tabs .tab-btn[data-tab="baiting"]');
            const firstContent = document.querySelector('.service-tab-content .content-editor[data-content="baiting"]');

            if (firstTabButton && firstContent) {

                document.querySelectorAll('.service-tabs .tab-btn').forEach(btn => btn.classList.remove('active'));
                document.querySelectorAll('.service-tab-content .content-editor').forEach(content => content.classList.remove('active'));
                firstTabButton.classList.add('active');
                firstContent.classList.add('active');
            }
        }


        document.addEventListener('DOMContentLoaded', initializeServiceTabs);
        

        if (typeof Sys !== 'undefined') {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initializeServiceTabs);
        }

    </script>
</asp:Content>