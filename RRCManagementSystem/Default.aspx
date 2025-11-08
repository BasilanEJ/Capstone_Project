<%@ Page Title="RRC Termite & Pest Control" Language="C#" EnableEventValidation="true" MasterPageFile="~/Inquiry.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RRCManagementSystem.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:ScriptManager ID="ScriptManager1" runat="server" />

    <style>
        /* ============ Modern Base Styles ============ */
        :root {
            --primary: #2563eb;
            --primary-dark: #1e40af;
            --primary-light: #3b82f6;
            --secondary: #8b5cf6;
            --accent: #06b6d4;
            --dark: #0f172a;
            --gray: #64748b;
            --light-bg: #f8fafc;
            --white: #ffffff;
            --shadow-sm: 0 2px 8px rgba(0,0,0,0.06);
            --shadow-md: 0 4px 16px rgba(0,0,0,0.08);
            --shadow-lg: 0 8px 32px rgba(0,0,0,0.12);
            --radius-sm: 8px;
            --radius-md: 12px;
            --radius-lg: 16px;
        }

        body {
            font-family: 'Inter', 'Segoe UI', system-ui, -apple-system, sans-serif;
            background: linear-gradient(to bottom, #f8fafc 0%, #ffffff 100%);
            color: var(--dark);
            line-height: 1.6;
        }

        img { max-width: 100%; height: auto; }
        .container, .container-fluid { width: 100%; }
        .container { padding-left: 20px; padding-right: 20px; }

        /* ============ Modern Typography ============ */
        p { 
            font-size: 16px; 
            line-height: 1.7; 
            margin: 0 0 16px; 
            color: var(--gray);
        }
        
        h1 { 
            font-size: 36px; 
            font-weight: 700;
            background: linear-gradient(135deg, var(--primary) 0%, var(--secondary) 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            margin: 24px 0 16px;
            letter-spacing: -0.5px;
        }
        
        h2 { 
            font-size: 28px; 
            font-weight: 600;
            color: var(--dark);
            margin: 20px 0 12px;
            letter-spacing: -0.3px;
        }

        h3 {
            font-size: 20px;
            font-weight: 600;
            color: var(--dark);
        }


.hero:first-of-type {
    margin-top: 0;
    padding: 0;
    position: relative;
    overflow: hidden;
}

/* About section (second hero section) - Add padding */
.hero:not(:first-of-type) {
    margin-top: 0;
    padding: 80px 0; /* Add vertical padding */
    position: relative;
    overflow: hidden;
    background: var(--white);
}

/* About section specific spacing */
.hero .container {
    padding-top: 20px;
    padding-bottom: 20px;
}

/* Video section spacing */
.video-hero {
    position: relative;
    width: 100%;
    background-color: #000;
    overflow: hidden;
    margin-top: 60px; /* Add top margin */
    margin-bottom: 60px; /* Add bottom margin */
}

/* Ensure all major sections have proper spacing */
section {
    margin-bottom: 40px;
}

/* Services section already has padding, ensure it's consistent */
.services { 
    padding: 80px 0; 
    background: linear-gradient(to bottom, #ffffff 0%, var(--light-bg) 100%);
}

.services .row {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 20px;
    margin: 0 auto;
    max-width: 1200px;
}

@media (min-width: 1200px) {
    .services .row {
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 24px;
    }
}

@media (max-width: 575.98px) {
    .services .row {
        grid-template-columns: repeat(2, 1fr);
        gap: 12px;
    }
}

.service-card {
    background: var(--white);
    padding: 20px;
    border-radius: var(--radius-md);
    text-align: center;
    transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
    height: 100%;
    border: 1px solid #e2e8f0;
    position: relative;
    overflow: hidden;
    cursor: pointer;
    display: flex;
    flex-direction: column;
    min-height: 240px;
}

.service-image {
    width: 100%;
    height: 120px;
    object-fit: cover;
    border-radius: var(--radius-sm);
    margin-bottom: 12px;
    transition: transform 0.4s ease;
}

.service-card h4 { 
    font-size: 15px;
    font-weight: 600;
    margin: 8px 0;
    color: var(--dark);
}

.service-card p {
    font-size: 13px;
    color: var(--gray);
    line-height: 1.5;
}

/* Blog section spacing */
.blog-section {
    text-align: center;
    padding: 80px 20px;
    background: var(--white);
    margin-bottom: 0;
}

/* C&O section spacing */
.co-section {
    text-align: center;
    padding: 80px 20px;
    background: var(--light-bg);
    margin-bottom: 0;
}

    .reviews-section {
        text-align: center;
        padding: 80px 20px;
        background: var(--white);
        position: relative;
    }
    
    .reviews-section h2 {
        font-size: 32px;
        font-weight: 700;
        color: var(--dark);
        margin-bottom: 48px;
    }
    
    /* Static Grid (for 4 or fewer reviews) */
    .reviews-grid {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 24px;
        margin: 0 auto;
        max-width: 1200px;
        padding: 0 20px;
    }
    
    /* Carousel Container (for 5+ reviews) */
    .reviews-carousel-container {
        position: relative;
        max-width: 1200px;
        margin: 0 auto;
        padding: 0 60px;
    }
    
    .reviews-carousel {
        overflow: hidden;
        position: relative;
    }
    
    .reviews-carousel-track {
        display: flex;
        gap: 24px;
        transition: transform 0.5s ease;
    }
    
    .reviews-carousel .review-card {
        flex: 0 0 calc(25% - 18px);
        min-width: calc(25% - 18px);
    }
    
    /* Navigation Buttons */
    .carousel-btn {
        position: absolute;
        top: 50%;
        transform: translateY(-50%);
        background: white;
        border: 2px solid #e2e8f0;
        border-radius: 50%;
        width: 48px;
        height: 48px;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        z-index: 10;
        transition: all 0.3s ease;
        box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    
    .carousel-btn:hover {
        background: #f8f9fa;
        border-color: #cbd5e0;
    }
    
    .carousel-btn:disabled {
        opacity: 0.3;
        cursor: not-allowed;
    }
    
    .carousel-btn.prev {
        left: 0;
    }
    
    .carousel-btn.next {
        right: 0;
    }
    /* Blog Modal Specific Styles - Enhanced */
.modal-xl {
    max-width: 1140px;
}

.modal-content {
    border: none;
    border-radius: 16px;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
}

.modal-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border-radius: 16px 16px 0 0;
    padding: 24px 30px;
    border: none;
}

.modal-header .modal-title {
    font-size: 28px;
    font-weight: 700;
    letter-spacing: -0.5px;
    margin: 0;
}

.modal-header .btn-close {
    filter: brightness(0) invert(1);
    opacity: 0.8;
    transition: opacity 0.3s ease;
}

.modal-header .btn-close:hover {
    opacity: 1;
}

.modal-body {
    padding: 40px 50px;
    background-color: #fafafa;
}

.modal-body .img-fluid {
    border-radius: 12px;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
    margin-bottom: 32px;
    transition: transform 0.3s ease;
}

.modal-body .img-fluid:hover {
    transform: scale(1.02);
}

.blog-content {
    font-size: 17px;
    color: #374151;
    line-height: 1.9;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
}

.blog-content h2 {
    color: #1e293b;
    font-size: 26px;
    margin-top: 40px;
    margin-bottom: 18px;
    font-weight: 700;
    padding-bottom: 12px;
    border-bottom: 3px solid #667eea;
    display: inline-block;
    position: relative;
}

.blog-content h2::after {
    content: '';
    position: absolute;
    bottom: -3px;
    left: 0;
    width: 50%;
    height: 3px;
    background: linear-gradient(90deg, #667eea, #764ba2);
}

.blog-content h3 {
    color: #475569;
    font-size: 22px;
    margin-top: 32px;
    margin-bottom: 16px;
    font-weight: 600;
    padding-left: 16px;
    border-left: 4px solid #667eea;
}

.blog-content p {
    margin-bottom: 20px;
    text-align: justify;
    color: #4b5563;
}

.blog-content p:first-of-type::first-letter {
    font-size: 3.5em;
    line-height: 0.9;
    font-weight: 700;
    color: #667eea;
    float: left;
    margin: 8px 12px 0 0;
}

.blog-content ul {
    margin-left: 0;
    margin-bottom: 24px;
    list-style: none;
    padding-left: 0;
}

.blog-content ul li {
    margin-bottom: 14px;
    line-height: 1.8;
    padding-left: 32px;
    position: relative;
}

.blog-content ul li::before {
    content: '✦';
    position: absolute;
    left: 8px;
    color: #667eea;
    font-size: 16px;
    font-weight: bold;
}

.blog-content strong {
    color: #1e293b;
    font-weight: 700;
    background: linear-gradient(120deg, #fef3c7 0%, #fef3c7 100%);
    background-repeat: no-repeat;
    background-size: 100% 40%;
    background-position: 0 85%;
    padding: 0 4px;
}

.blog-content a {
    color: #667eea;
    text-decoration: none;
    border-bottom: 2px solid transparent;
    transition: border-color 0.3s ease;
}

.blog-content a:hover {
    border-bottom-color: #667eea;
}

.modal-footer {
    background-color: #f9fafb;
    border-radius: 0 0 16px 16px;
    padding: 20px 30px;
    border: none;
}

.modal-footer .btn-secondary {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border: none;
    padding: 12px 32px;
    font-weight: 600;
    border-radius: 8px;
    transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.modal-footer .btn-secondary:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 16px rgba(102, 126, 234, 0.3);
}

/* Responsive blog modal */
@media (max-width: 991.98px) {
    .modal-xl {
        max-width: 90%;
    }
    
    .modal-body {
        padding: 30px 35px;
    }
    
    .blog-content {
        font-size: 16px;
    }
    
    .blog-content h2 {
        font-size: 24px;
        margin-top: 32px;
    }
    
    .blog-content h3 {
        font-size: 20px;
        margin-top: 28px;
    }
}

@media (max-width: 767.98px) {
    .modal-xl {
        max-width: 95%;
        margin: 10px;
    }
    
    .modal-header {
        padding: 20px;
    }
    
    .modal-header .modal-title {
        font-size: 22px;
    }
    
    .modal-body {
        padding: 24px 20px;
    }
    
    .blog-content {
        font-size: 15px;
    }
    
    .blog-content h2 {
        font-size: 22px;
        margin-top: 28px;
    }
    
    .blog-content h3 {
        font-size: 18px;
        margin-top: 24px;
        padding-left: 12px;
    }
    
    .blog-content p:first-of-type::first-letter {
        font-size: 2.5em;
    }
    
    .blog-content ul li {
        padding-left: 28px;
    }
    
    .modal-footer {
        padding: 16px 20px;
    }
}

@media (max-width: 575.98px) {
    .modal-body .img-fluid {
        margin-bottom: 24px;
    }
    
    .blog-content p {
        text-align: left;
    }
}

    /* Review Card Styles */
    .review-card {
        width: 100%;
        padding: 24px;
        background: var(--white);
        border-radius: 8px;
        box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        text-align: left;
        transition: all 0.3s ease;
        border: 1px solid #e2e8f0;
        display: flex;
        flex-direction: column;
        min-height: 180px;
    }
    
    .review-card:hover {
        transform: translateY(-4px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    }
    
    .review-card strong {
        color: var(--dark);
        font-weight: 600;
        font-size: 16px;
        margin-bottom: 4px;
    }
    
    .review-card .recommends {
        font-size: 13px;
        color: #ef4444;
        margin: 4px 0 12px;
    }
    
    .review-card p {
        font-size: 14px;
        color: var(--gray);
        line-height: 1.6;
        margin: 0 0 12px 0;
        flex-grow: 1;
    }
    
    .review-rating {
        color: #fbbf24;
        font-size: 18px;
    }
    
    /* Responsive Design */
    @media (max-width: 1024px) {
        .reviews-grid {
            grid-template-columns: repeat(3, 1fr);
        }
        
        .reviews-carousel .review-card {
            flex: 0 0 calc(33.333% - 16px);
            min-width: calc(33.333% - 16px);
        }
    }
    
    @media (max-width: 768px) {
        .reviews-grid {
            grid-template-columns: repeat(2, 1fr);
        }
        
        .reviews-carousel .review-card {
            flex: 0 0 calc(50% - 12px);
            min-width: calc(50% - 12px);
        }
        
        .reviews-carousel-container {
            padding: 0 50px;
        }
        
        .carousel-btn {
            width: 40px;
            height: 40px;
        }
    }
    
    @media (max-width: 575px) {
        .reviews-grid {
            grid-template-columns: 1fr;
            gap: 16px;
        }
        
        .reviews-carousel .review-card {
            flex: 0 0 100%;
            min-width: 100%;
        }
        
        .reviews-carousel-container {
            padding: 0 40px;
        }
    }

/* Inquiry form spacing */
.inquiry-wrap {
    display: flex;
    justify-content: center;
    align-items: stretch;
    min-height: 60vh;
    background: var(--light-bg);
    padding: 60px 20px;
    margin-top: 0;
}

/* Mobile responsive spacing adjustments */
@media (max-width: 575.98px) {
    .hero {
        padding: 40px 0;
    }
    
    .video-hero {
        margin-top: 40px;
        margin-bottom: 40px;
    }
    
    .services,
    .blog-section,
    .co-section,
    .reviews-section {
        padding: 60px 20px;
    }
}
        
     .blog-description {
        font-size: 14px;
        color: #64748b;
        margin-top: 8px;
        line-height: 1.5;
    }
    .hero-banner { 
            width: 100%; 
            height: auto; 
            object-fit: cover;
            filter: brightness(1.05);
        }
        
        .hero-img { 
            max-width: 100%; 
            height: auto; 
            border-radius: var(--radius-lg);
            box-shadow: var(--shadow-lg);
        }

#chkTerms.ClientID + .form-check-label {
    color: #000000;
    font-weight: 500;
}

/* Specifically for the terms modal */
#termsModal .form-check-label {
    color: #000000 !important;
    font-weight: 500;
}

        /* Smooth fade-in animations */
        @keyframes fadeInUp { 
            from { 
                opacity: 0; 
                transform: translateY(30px); 
            } 
            to { 
                opacity: 1; 
                transform: translateY(0); 
            } 
        }
        
        .fade-in { 
            opacity: 0; 
            animation: fadeInUp 0.8s ease-out forwards; 
        }

        /* ============ Modern Services Section ============ */
        .services { 
            padding: 80px 0; 
            background: linear-gradient(to bottom, #ffffff 0%, var(--light-bg) 100%);
        }
        
        .services h2 { 
            margin-bottom: 48px;
            position: relative;
            display: inline-block;
        }

        .services h2::after {
            content: '';
            position: absolute;
            bottom: -12px;
            left: 50%;
            transform: translateX(-50%);
            width: 60px;
            height: 4px;
            background: linear-gradient(90deg, var(--primary), var(--secondary));
            border-radius: 2px;
        }

        .service-card {
            background: var(--white);
            padding: 24px;
            border-radius: var(--radius-md);
            text-align: center;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            height: 100%;
            border: 1px solid #e2e8f0;
            position: relative;
            overflow: hidden;
        }

        .service-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 3px;
            background: linear-gradient(90deg, var(--primary), var(--secondary));
            transform: scaleX(0);
            transition: transform 0.4s ease;
        }
        .service-highlight {
    background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
    border-left: 4px solid #f59e0b;
    padding: 16px 20px;
    border-radius: 8px;
    margin: 20px 0;
    font-style: italic;
    color: #78350f;
    font-weight: 500;
}


        .service-card:hover {
            transform: translateY(-8px);
            box-shadow: var(--shadow-lg);
            border-color: var(--primary-light);
        }

        .service-card:hover::before {
            transform: scaleX(1);
        }

        .service-image {
            width: 100%;
            height: 160px;
            object-fit: cover;
            border-radius: var(--radius-sm);
            margin-bottom: 16px;
            transition: transform 0.4s ease;
        }

        .service-card:hover .service-image {
            transform: scale(1.05);
        }

        .service-card h4 { 
            font-size: 17px;
            font-weight: 600;
            margin: 12px 0 8px;
            color: var(--dark);
        }

        .service-card p {
            font-size: 14px;
            color: var(--gray);
            line-height: 1.6;
        }

        /* ============ Modern Carousel Controls ============ */
        .carousel-control-prev-icon,
        .carousel-control-next-icon {
            background-size: 60% 60%;
            width: 48px;
            height: 48px;
            border-radius: 50%;
            background-color: rgba(37, 99, 235, 0.9);
            box-shadow: var(--shadow-md);
            transition: all 0.3s ease;
        }

        .carousel-control-prev:hover .carousel-control-prev-icon,
        .carousel-control-next:hover .carousel-control-next-icon {
            background-color: var(--primary-dark);
            transform: scale(1.1);
        }

        .carousel-indicators button {
            background-color: var(--primary);
            width: 10px;
            height: 10px;
            border-radius: 50%;
            opacity: 0.5;
            transition: all 0.3s ease;
        }

        .carousel-indicators button.active {
            opacity: 1;
            width: 32px;
            border-radius: 5px;
        }

        /* ============ Modern Modal Design ============ */
      
.modal-backdrop {
    background-color: rgba(15, 23, 42, 0.75);
    backdrop-filter: blur(8px);
}

.modal.fade .modal-dialog {
    transform: translateY(-30px) scale(0.97);
    opacity: 0;
    transition: all 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.modal.show .modal-dialog {
    transform: translateY(0) scale(1);
    opacity: 1;
}

.modal-content {
    border-radius: 20px;
    border: none;
    overflow: hidden;
    box-shadow: 0 25px 70px rgba(0,0,0,0.25);
    background: #ffffff;
}

.modal-header {
    background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
    color: white;
    border-bottom: none;
    padding: 28px 32px;
    position: relative;
}

.modal-header::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    height: 4px;
    background: linear-gradient(90deg, rgba(255,255,255,0.3) 0%, rgba(255,255,255,0.1) 100%);
}

.modal-header .modal-title {
    font-size: 26px;
    font-weight: 700;
    letter-spacing: -0.5px;
    text-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.modal-header .btn-close {
    filter: invert(1) brightness(1.2);
    opacity: 0.95;
    transition: all 0.3s ease;
    padding: 12px;
}

.modal-header .btn-close:hover {
    opacity: 1;
    transform: rotate(90deg);
}

.modal-body {
    padding: 40px 32px;
    background: linear-gradient(to bottom, #ffffff 0%, #f8fafc 100%);
}

.modal-body img {
    border-radius: 16px;
    margin-bottom: 32px;
    max-width: 100%;
    width: 100%;
    max-height: 380px;
    height: auto;
    display: block;
    object-fit: cover;
    box-shadow: 0 8px 24px rgba(0,0,0,0.12);
    transition: transform 0.3s ease;
}

.modal-body img:hover {
    transform: scale(1.02);
}

.modal-body p {
    font-size: 16px;
    color: #475569;
    line-height: 1.8;
    margin-bottom: 24px;
    text-align: left;
}

.modal-body p:first-of-type::first-letter {
    font-size: 2.2em;
    font-weight: 700;
    color: #2563eb;
    float: left;
    line-height: 0.9;
    margin-right: 8px;
    margin-top: 4px;
}

.modal-body ul {
    list-style: none;
    padding: 0;
    margin: 32px 0 0 0;
    background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
    border-radius: 12px;
    padding: 24px 28px;
    border-left: 4px solid #2563eb;
}

.modal-body ul li {
    font-size: 15px;
    padding: 12px 0;
    color: #334155;
    font-weight: 500;
    display: flex;
    align-items: flex-start;
    line-height: 1.6;
}

.modal-body ul li::before {
    content: '✓';
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 24px;
    height: 24px;
    background: linear-gradient(135deg, #2563eb, #3b82f6);
    color: white;
    border-radius: 50%;
    margin-right: 12px;
    font-weight: bold;
    font-size: 14px;
    flex-shrink: 0;
    margin-top: 2px;
    box-shadow: 0 2px 8px rgba(37, 99, 235, 0.3);
}

.modal-body strong {
    color: #1e293b;
    font-weight: 600;
}

/* Enhanced scrollbar for modal */
.modal-body::-webkit-scrollbar {
    width: 8px;
}

.modal-body::-webkit-scrollbar-track {
    background: #f1f5f9;
    border-radius: 10px;
}

.modal-body::-webkit-scrollbar-thumb {
    background: #cbd5e1;
    border-radius: 10px;
    transition: background 0.3s ease;
}

.modal-body::-webkit-scrollbar-thumb:hover {
    background: #94a3b8;
}

        .modal-footer {
            background: var(--light-bg);
            border-top: 1px solid #e2e8f0;
            padding: 20px 28px;
        }

        .modal-footer .btn {
            min-width: 100px;
            font-weight: 500;
            border-radius: var(--radius-sm);
        }

        body.modal-open {
            overflow: hidden;
            padding-right: 0 !important;
        }

        /* ============ Modern Video Hero ============ */
        .video-hero {
            position: relative;
            width: 100%;
            background-color: #000;
            overflow: hidden;
        }

        @media (min-width: 768px) {
            .video-hero { height: 100vh; }
            .video-hero .video-layer { position: absolute; inset: 0; }
        }

        @media (max-width: 767.98px) {
            .video-hero { height: auto; }
            .video-hero .ratio-box { position: relative; width: 100%; padding-top: 56.25%; }
            .video-hero .video-layer { position: absolute; inset: 0; }
             .modal-header {
        padding: 24px;
    }
    
    .modal-header .modal-title {
        font-size: 22px;
    }
    
    .modal-body {
        padding: 28px 24px;
    }
    
    .modal-body img {
        max-height: 280px;
        margin-bottom: 24px;
    }
    
    .modal-body p {
        font-size: 15px;
    }
    
    .modal-body ul {
        padding: 20px;
    }
    
    .modal-body ul li {
        font-size: 14px;
        padding: 10px 0;
    }
}
        

        .video-hero .thumb,
        .video-hero iframe {
            width: 100%;
            height: 100%;
            object-fit: cover;
            border: 0;
            display: block;
        }

        .video-hero .play-btn {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: linear-gradient(135deg, var(--primary), var(--secondary));
            color: #fff;
            padding: 18px 32px;
            border-radius: 50px;
            font-size: 1.25rem;
            cursor: pointer;
            text-align: center;
            line-height: 1;
            user-select: none;
            box-shadow: 0 8px 24px rgba(37, 99, 235, 0.4);
            transition: all 0.3s ease;
            font-weight: 600;
        }

        .video-hero .play-btn:hover {
            transform: translate(-50%, -50%) scale(1.05);
            box-shadow: 0 12px 32px rgba(37, 99, 235, 0.5);
        }

        .video-hero .video-title {
            position: absolute;
            top: 5%;
            left: 50%;
            transform: translateX(-50%);
            color: #fff;
            font-size: 2rem;
            text-shadow: 2px 2px 16px rgba(0,0,0,0.7);
            margin: 0;
            font-weight: 700;
        }

        @media (min-width: 768px) {
            .video-hero .play-btn { font-size: 1.5rem; padding: 24px 42px; }
            .video-hero .video-title { font-size: 3rem; }
        }

        /* ============ Modern Inquiry Form ============ */
        .inquiry-wrap {
            display: flex;
            justify-content: center;
            align-items: stretch;
            min-height: 60vh;
            background: var(--light-bg);
            padding: 60px 20px;
        }

        .inquiry-card {
            display: flex;
            width: 100%;
            max-width: 1100px;
            border-radius: var(--radius-lg);
            overflow: hidden;
            background: var(--white);
            box-shadow: var(--shadow-lg);
        }

        .inquiry-left {
            flex: 1;
            padding: 48px;
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
        }

        .inquiry-left h3 {
            text-transform: uppercase;
            font-size: 14px;
            font-weight: 600;
            color: rgba(255,255,255,0.9);
            letter-spacing: 1px;
        }

        .inquiry-left h1 {
            font-size: 32px;
            font-weight: 700;
            color: var(--white);
            margin-bottom: 16px;
            background: none;
            -webkit-text-fill-color: var(--white);
        }

        .inquiry-left > p {
            font-size: 15px;
            color: rgba(255,255,255,0.9);
            margin-bottom: 24px;
            line-height: 1.6;
        }

        .inquiry-right {
            flex: 1;
            min-height: 300px;
            background: url('/Images/service-baiting.jpg') center center / cover no-repeat;
        }

        .form-underline {
            width: 100%;
            border: none;
            border-bottom: 2px solid rgba(255,255,255,0.3);
            padding: 12px 0;
            background: transparent;
            color: var(--white);
            font-size: 15px;
            transition: border-color 0.3s ease;
        }

        .form-underline:focus {
            outline: none;
            border-bottom-color: var(--white);
        }

        .form-underline::placeholder {
            color: rgba(255,255,255,0.7);
            opacity: 1;
        }

        .inquiry-left label {
            color: rgba(255,255,255,0.9);
            font-size: 14px;
            font-weight: 500;
            margin-bottom: 4px;
            display: block;
        }

        .textarea-box {
            width: 100%;
            border: 2px solid rgba(255,255,255,0.3);
            border-radius: var(--radius-sm);
            padding: 12px;
            font-size: 15px;
            min-height: 120px;
            background: rgba(255,255,255,0.1);
            color: var(--white);
            transition: border-color 0.3s ease;
        }

        .textarea-box:focus {
            outline: none;
            border-color: var(--white);
            background: rgba(255,255,255,0.15);
        }

        .textarea-box::placeholder {
            color: rgba(255,255,255,0.7);
        }

        .btn-submit {
            width: 100%;
            padding: 16px;
            background: linear-gradient(135deg, var(--dark) 0%, #1e293b 100%);
            color: var(--white);
            border: 0;
            border-radius: var(--radius-sm);
            font-weight: 600;
            font-size: 16px;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(0,0,0,0.2);
        }

        .btn-submit:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0,0,0,0.3);
        }

        @media (max-width: 991.98px) {
            .inquiry-card { flex-direction: column; }
            .inquiry-right { height: 280px; }
            .inquiry-left { padding: 32px; }
        }

        .is-invalid {
            outline: 2px solid #ef4444 !important;
            border-radius: 4px;
        }

        /* ============ Modern Blog Section ============ */
        .blog-section {
            text-align: center;
            padding: 80px 20px;
            background: var(--white);
        }

        .blog-section h2 {
            font-size: 32px;
            font-weight: 700;
            color: var(--dark);
            margin-bottom: 48px;
        }

        .blog-cards {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: 28px;
            margin-top: 40px;
        }

        .blog-card {
            text-decoration: none;
            color: inherit;
            width: 320px;
            border-radius: var(--radius-md);
            overflow: hidden;
            transition: all 0.4s ease;
            background: var(--white);
            box-shadow: var(--shadow-sm);
            border: 1px solid #e2e8f0;
        }

        .blog-card:hover {
            transform: translateY(-8px);
            box-shadow: var(--shadow-lg);
            text-decoration: none;
        }

        .blog-card img {
            width: 100%;
            height: 200px;
            object-fit: cover;
            transition: transform 0.4s ease;
        }

        .blog-card:hover img {
            transform: scale(1.05);
        }

        .blog-card-content {
            padding: 24px;
            text-align: left;
        }

        .blog-card h3 {
            font-size: 18px;
            font-weight: 600;
            margin-bottom: 0;
            color: var(--dark);
            line-height: 1.4;
        }

        /* ============ Modern C&O Section ============ */
        .co-section {
            text-align: center;
            padding: 80px 20px;
            background: var(--light-bg);
        }

        .co-section h2 {
            color: var(--dark);
            font-size: 32px;
            font-weight: 700;
            margin-bottom: 40px;
        }

        .co-section img {
            max-width: 900px;
            width: 90%;
            height: auto;
            border-radius: var(--radius-lg);
            box-shadow: var(--shadow-md);
        }

        /* ============ Modern Reviews Section ============ */
    

        /* ============ Modern Cookie Banner ============ */
        #cookieConsentBanner {
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: var(--white);
            color: var(--dark);
            padding: 32px;
            text-align: center;
            font-size: 15px;
            z-index: 9999;
            border-radius: var(--radius-lg);
            box-shadow: 0 20px 60px rgba(0,0,0,0.2);
            opacity: 0;
            visibility: hidden;
            transition: opacity 0.6s ease, visibility 0.6s ease;
            max-width: 520px;
            width: 90%;
            border: 1px solid #e2e8f0;
        }

        #cookieConsentBanner.show {
            opacity: 1;
            visibility: visible;
        }

        #cookieConsentBanner.hide {
            opacity: 0;
            visibility: hidden;
        }

        #cookieConsentBanner button {
            background: linear-gradient(135deg, var(--primary), var(--primary-dark));
            color: white;
            border: none;
            padding: 12px 28px;
            border-radius: var(--radius-sm);
            cursor: pointer;
            font-weight: 600;
            transition: all 0.3s ease;
        }

        #cookieConsentBanner button:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
        }

        /* ============ Responsive Adjustments ============ */
        @media (max-width: 575.98px) {
            h1 { font-size: 28px; }
            h2 { font-size: 24px; }
            .hero-title { font-size: 26px; }
            p { font-size: 15px; }
            .service-card h4 { font-size: 15px; }
            .services { padding: 60px 0; }
            .blog-section, .co-section, .reviews-section { padding: 60px 20px; }
        }

        /* ============ Enhanced About Section ============ */
        .hero .check-icon {
            color: var(--primary);
            margin-right: 8px;
        }

        .hero h2 {
            display: flex;
            align-items: center;
            font-size: 20px;
            font-weight: 600;
            color: var(--dark);
        }

        /* ============ Small polish touches ============ */
        a {
            color: var(--primary);
            transition: color 0.3s ease;
        }

        a:hover {
            color: var(--primary-dark);
        }

        .text-center.mb-4 h2 {
            color: var(--gray);
            font-weight: 600;
            font-size: 18px;
        }

        .text-center.mb-4 h1 {
            font-size: 36px;
            font-weight: 700;
        }

        /* ============ Call to Action Section ============ */
.cta-section {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    padding: 80px 20px;
    text-align: center;
    position: relative;
    overflow: hidden;
}

.cta-section::before {
    content: '';
    position: absolute;
    top: -50%;
    right: -50%;
    width: 200%;
    height: 200%;
    background: radial-gradient(circle, rgba(255,255,255,0.1) 1px, transparent 1px);
    background-size: 50px 50px;
    animation: moveBackground 20s linear infinite;
    pointer-events: none;
}

@keyframes moveBackground {
    0% { transform: translate(0, 0); }
    100% { transform: translate(50px, 50px); }
}

.cta-container {
    max-width: 900px;
    margin: 0 auto;
    position: relative;
    z-index: 1;
}

.cta-content {
    background: rgba(255, 255, 255, 0.98);
    backdrop-filter: blur(10px);
    padding: 60px 40px;
    border-radius: 20px;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
    animation: fadeInUp 0.8s ease;
}

.cta-icon {
    font-size: 64px;
    color: #667eea;
    margin-bottom: 20px;
    animation: bounce 2s infinite;
}

@keyframes bounce {
    0%, 100% { transform: translateY(0); }
    50% { transform: translateY(-10px); }
}

.cta-content h2 {
    font-size: clamp(28px, 5vw, 36px);
    color: #1a1a1a;
    margin-bottom: 15px;
    font-weight: 700;
    background: none;
    -webkit-text-fill-color: #1a1a1a;
}

.cta-content p {
    font-size: clamp(16px, 4vw, 18px);
    color: #666;
    margin-bottom: 40px;
    line-height: 1.6;
}

.cta-buttons {
    display: flex;
    gap: 20px;
    justify-content: center;
    flex-wrap: wrap;
    margin-bottom: 40px;
}

.btn-cta {
    padding: 16px 32px;
    font-size: clamp(15px, 4vw, 17px);
    font-weight: 600;
    border-radius: 12px;
    text-decoration: none;
    display: inline-flex;
    align-items: center;
    gap: 10px;
    transition: all 0.3s ease;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
    min-width: 180px;
    justify-content: center;
}

.btn-cta i {
    font-size: 18px;
}

.btn-primary {
    background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
    color: white;
}

.btn-primary:hover {
    background: linear-gradient(135deg, #0056b3 0%, #004494 100%);
    transform: translateY(-3px);
    box-shadow: 0 6px 20px rgba(0, 123, 255, 0.4);
}

.btn-secondary {
    background: white;
    color: #007bff;
    border: 2px solid #007bff;
}

.btn-secondary:hover {
    background: #007bff;
    color: white;
    transform: translateY(-3px);
    box-shadow: 0 6px 20px rgba(0, 123, 255, 0.3);
}

.cta-features {
    display: flex;
    justify-content: center;
    gap: 30px;
    flex-wrap: wrap;
    padding-top: 30px;
    border-top: 1px solid #e0e0e0;
}

.feature-item {
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 15px;
    color: #555;
    font-weight: 500;
}

.feature-item i {
    color: #28a745;
    font-size: 20px;
}

/* Responsive */
@media (max-width: 768px) {
    .cta-section {
        padding: 60px 20px;
    }

    .cta-content {
        padding: 40px 30px;
    }

    .cta-icon {
        font-size: 48px;
    }

    .cta-buttons {
        flex-direction: column;
        gap: 15px;
    }

    .btn-cta {
        width: 100%;
        max-width: 300px;
    }

    .cta-features {
        flex-direction: column;
        gap: 15px;
    }
}

@media (max-width: 480px) {
    .cta-content {
        padding: 30px 20px;
    }

    .cta-icon {
        font-size: 40px;
    }

    .feature-item {
        font-size: 14px;
    }
}

    </style>


    <!-- ====== HERO BANNER ====== -->
     <section class="hero lazy-section">
        <div class="container-fluid p-0 position-relative">
            <asp:Image ID="imgHeroBanner" runat="server" alt="Pest Control Banner" CssClass="hero-banner img-fluid" />
        </div>
    </section>

<section class="services py-5 lazy-section">
    <div class="container">
        <div class="text-center mb-4">
            <h2 style="color: gray;">WE PROVIDE THE BEST</h2>
            <h1 style="color: blue;">Termite and Pest Control Services</h1>
        </div>
        <div id="servicesCarousel" class="carousel slide">
            <div class="carousel-inner">

                <!-- ====== TERMITE CONTROL SERVICES ====== -->
                <div class="carousel-item active">
                    <h3 class="text-dark mb-3 text-center">Termite Control Services</h3>
                    <div class="row justify-content-center g-3">
                        
                        <!-- Dynamic Termite Services -->
                     <asp:Repeater ID="rptTermiteServices" runat="server">
    <ItemTemplate>
        <div class="service-card text-center h-100" 
             data-bs-toggle="modal" 
             data-bs-target='#modalService<%# Eval("ServiceID") %>'>
            <asp:Image ID="imgService" runat="server" 
                ImageUrl='<%# Eval("ImagePath") %>' 
                AlternateText='<%# Eval("ServiceTitle") %>' 
                CssClass="service-image" />
            <h4><%# Eval("ServiceTitle") %></h4>
            <p class="small"><%# GetShortDescription(Eval("ServiceDescription").ToString()) %></p>
        </div>
    </ItemTemplate>
</asp:Repeater>

                    </div>
                </div>

                <!-- ====== GENERAL PEST CONTROL SERVICES ====== -->
                <div class="carousel-item">
                    <h3 class="text-dark mb-3 text-center">General Pest Control Services</h3>
                    <div class="row justify-content-center g-3">
                        
                        <!-- Dynamic Pest Control Services -->
                       <asp:Repeater ID="rptPestServices" runat="server">
    <ItemTemplate>
        <div class="service-card text-center h-100" 
             data-bs-toggle="modal" 
             data-bs-target='#modalService<%# Eval("ServiceID") %>'>
            <asp:Image ID="imgService" runat="server" 
                ImageUrl='<%# Eval("ImagePath") %>' 
                AlternateText='<%# Eval("ServiceTitle") %>' 
                CssClass="service-image" />
            <h4><%# Eval("ServiceTitle") %></h4>
            <p class="small"><%# GetShortDescription(Eval("ServiceDescription").ToString()) %></p>
        </div>
    </ItemTemplate>
</asp:Repeater>

                    </div>
                </div>

            </div>

            <!-- Carousel Controls -->
            <button class="carousel-control-prev" type="button" data-bs-target="#servicesCarousel" data-bs-slide="prev">
                <span class="carousel-control-prev-icon bg-dark rounded-circle p-3" aria-hidden="true"></span>
                <span class="visually-hidden">Previous</span>
            </button>
            <button class="carousel-control-next" type="button" data-bs-target="#servicesCarousel" data-bs-slide="next">
                <span class="carousel-control-next-icon bg-dark rounded-circle p-3" aria-hidden="true"></span>
                <span class="visually-hidden">Next</span>
            </button>
        </div>
    </div>
</section>


<asp:Repeater ID="rptTermiteModals" runat="server">
    <ItemTemplate>
        <div class="modal fade" id='modalService<%# Eval("ServiceID") %>' tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title"><%# Eval("ServiceTitle") %></h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <asp:Image ID="imgModalService" runat="server" 
                            ImageUrl='<%# Eval("ImagePath") %>' 
                            AlternateText='<%# Eval("ServiceTitle") %>' 
                            loading="lazy" 
                            CssClass="img-fluid rounded mb-3" />
                        
                        <p><%# Eval("ServiceDescription") %></p>
                        
                        <!-- Display Bullet Points if available -->
                        <%# !string.IsNullOrEmpty(Eval("BulletPoints").ToString()) ? 
                            "<ul>" + FormatBulletPoints(Eval("BulletPoints").ToString()) + "</ul>" : "" %>
                    </div>
                </div>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>

<!-- General Pest Control Service Modals -->
<asp:Repeater ID="rptPestModals" runat="server">
    <ItemTemplate>
        <div class="modal fade" id='modalService<%# Eval("ServiceID") %>' tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title"><%# Eval("ServiceTitle") %></h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <asp:Image ID="imgModalService" runat="server" 
                            ImageUrl='<%# Eval("ImagePath") %>' 
                            AlternateText='<%# Eval("ServiceTitle") %>' 
                            loading="lazy" 
                            CssClass="img-fluid rounded mb-3" />
                        
                        <p><%# Eval("ServiceDescription") %></p>
                        
                        <!-- Display Bullet Points if available -->
                        <%# !string.IsNullOrEmpty(Eval("BulletPoints").ToString()) ? 
                            "<ul>" + FormatBulletPoints(Eval("BulletPoints").ToString()) + "</ul>" : "" %>
                    </div>
                </div>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>

    <!-- ====== ABOUT SPLIT ====== -->
      <section class="hero lazy-section">
        <div class="container">
            <div class="row align-items-center gy-4">
                <div class="col-md-6 text-center">
                    <asp:Image ID="imgAbout" runat="server" alt="Pest Control Worker" CssClass="img-fluid hero-img" />
                </div>

                <div class="col-md-6">
                    <h1>R.R.C. Termite and Pest Control</h1>
                    <p>We are your trusted partners in safeguarding your home and business against unwelcome intruders. Our mission is to provide affordable and reliable pest control services that prioritize your peace of mind and the well-being of your property.</p>

                    <h2><i class="fas fa-check-circle check-icon"></i> Innovative Solutions</h2>
                    <p>We provide cutting-edge solutions to safeguard your home from termites and pests. With a strong focus on environmentally friendly and effective treatments, we use advanced techniques to ensure your property is protected.</p>

                    <h2><i class="fas fa-check-circle check-icon"></i> Certified Services</h2>
                    <p>As a certified pest control provider, we adhere to the highest industry standards. Our team of licensed professionals undergoes rigorous training to offer you expert service and long-lasting results. Every treatment is tailored to meet your unique needs, ensuring your home or business remains pest-free.</p>

                    <h2><i class="fas fa-check-circle check-icon"></i> Local and Trusted</h2>
                    <p>We take pride in serving our community. With years of experience in the area, we understand the specific challenges homeowners face. Our reputation as trusted experts is built on reliability, honesty, and exceptional customer care.</p>
                </div>
            </div>
        </div>
    </section>

    <!-- ====== RESPONSIVE VIDEO (VIMEO) ====== -->
  <section class="hero video-hero lazy-section">
        <div class="ratio-box d-md-none"></div>
        <h1 class="video-title"></h1>

        <div class="video-layer">
            <asp:Image ID="imgVideoThumbnail" runat="server" alt="Video Thumbnail" CssClass="thumb" style="cursor:pointer;" />
            <iframe id="vimeoVideo"
                    src=""
                    allow="autoplay; fullscreen"
                    allowfullscreen
                    style="display:none;"></iframe>
            <div id="playButton" class="play-btn">▶ Play</div>
        </div>
    </section>
    
    <asp:HiddenField ID="hfVimeoVideoId" runat="server" />
    <asp:HiddenField ID="hfVideoType" runat="server" />

   <!-- Replace the existing blog section -->
<section class="blog-section lazy-section">
    <h2>Read our Blogs</h2>
    <div class="blog-cards">
        
        <!-- Dynamic Blog Cards -->
        <asp:Repeater ID="rptBlogs" runat="server">
            <ItemTemplate>
                <a href="#" class="blog-card" 
                   data-bs-toggle="modal" 
                   data-bs-target='#modalBlog<%# Eval("BlogID") %>'>
                    <asp:Image ID="imgBlog" runat="server" 
                        ImageUrl='<%# Eval("ImagePath") %>' 
                        AlternateText='<%# Eval("BlogTitle") %>' />
                    <div class="blog-card-content">
                        <h3><%# Eval("BlogTitle") %></h3>
                        <%# !string.IsNullOrEmpty(Eval("BlogDescription").ToString()) ? 
                            "<p class='blog-description'>" + Eval("BlogDescription") + "</p>" : "" %>
                    </div>
                </a>
            </ItemTemplate>
        </asp:Repeater>

    </div>
</section>

<!-- Blog Modals -->
<asp:Repeater ID="rptBlogModals" runat="server">
    <ItemTemplate>
        <div class="modal fade" id='modalBlog<%# Eval("BlogID") %>' tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title"><%# Eval("BlogTitle") %></h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <asp:Image ID="imgModalBlog" runat="server" 
                            ImageUrl='<%# Eval("ImagePath") %>' 
                            AlternateText='<%# Eval("BlogTitle") %>' 
                            loading="lazy" 
                            CssClass="img-fluid rounded mb-4" 
                            style="max-height: 400px; width: 100%; object-fit: cover;" />
                        
                        <!-- Blog Content -->
                        <div class="blog-content" style="text-align: left; line-height: 1.8;">
                            <%# FormatBlogContent(Eval("BlogContent").ToString()) %>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>



   <section class="co-section lazy-section">
        <h2>Certifications & Organizations</h2>
        <div style="display:flex; justify-content:center; align-items:center; margin-top:20px;">
            <asp:Image ID="imgCO" runat="server" alt="Certifications & Organizations" />
        </div>
    </section>


    <section class="reviews-section lazy-section">
    <h2>What our customers are saying</h2>
    <div class="reviews-grid">

        <asp:Repeater ID="rptReviews" runat="server">
            <ItemTemplate>
                <div class="review-card">
                    <strong><%# Eval("CustomerName") %></strong>
                    <%# Convert.ToBoolean(Eval("Recommends")) ? "<p class='recommends'>❤️ recommends</p>" : "" %>
                    <p><%# Eval("ReviewText") %></p>
                    <div class="review-rating">
                        <%# GetStarRatingForReview(Convert.ToInt32(Eval("Rating"))) %>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</section>



    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>


<section class="cta-section">
    <div class="cta-container">
        <div class="cta-content">
            <i class="fas fa-user-plus cta-icon"></i>
            <h2>Ready to Protect Your Property?</h2>
            <p>Create your account today and get started with professional pest control services</p>
            
            <div class="cta-buttons">
                <a href="ClientSignup.aspx" class="btn-cta btn-primary">
                    <i class="fas fa-user-plus"></i> Create Account
                </a>
                <a href="Login.aspx" class="btn-cta btn-secondary">
                    <i class="fas fa-sign-in-alt"></i> Sign In
                </a>
            </div>

            <div class="cta-features">
                <div class="feature-item">
                    <i class="fas fa-check-circle"></i>
                    <span>Free Inspection</span>
                </div>
                <div class="feature-item">
                    <i class="fas fa-shield-alt"></i>
                    <span>Certified Professionals</span>
                </div>
                <div class="feature-item">
                    <i class="fas fa-clock"></i>
                    <span>24/7 Support</span>
                </div>
            </div>
        </div>
    </div>
</section>

    <script>

        const videoId = document.getElementById('<%= hfVimeoVideoId.ClientID %>').value;
    const videoType = document.getElementById('<%= hfVideoType.ClientID %>') ?
            document.getElementById('<%= hfVideoType.ClientID %>').value : 'YouTube';

        const videoIframe = document.getElementById("vimeoVideo");

        if (videoIframe && videoId) {
            let embedUrl = '';

            if (videoType === 'YouTube') {
                embedUrl = `https://www.youtube.com/embed/${videoId}?rel=0&modestbranding=1&autohide=1&showinfo=0`;
            } else if (videoType === 'Vimeo') {
                embedUrl = `https://player.vimeo.com/video/${videoId}?loop=1&muted=0`;
            }

            videoIframe.setAttribute("src", embedUrl);
        }

        // Video play logic
        const playButton = document.getElementById("playButton");
        const videoThumbnail = document.getElementById('<%= imgVideoThumbnail.ClientID %>');

        if (playButton && videoThumbnail && videoIframe) {
            playButton.addEventListener("click", function () {
                const src = videoIframe.getAttribute("src");
                let nextSrc = '';

                if (videoType === 'YouTube') {
                    // Add autoplay parameter for YouTube
                    nextSrc = src.includes("autoplay=1") ? src : (src + (src.includes("?") ? "&" : "?") + "autoplay=1");
                } else if (videoType === 'Vimeo') {
                    // Add autoplay parameter for Vimeo
                    nextSrc = src.includes("autoplay=1") ? src : (src + (src.includes("?") ? "&" : "?") + "autoplay=1");
                }

                videoIframe.setAttribute("src", nextSrc);

                videoThumbnail.style.display = "none";
                playButton.style.display = "none";
                videoIframe.style.display = "block";
            });
        }
    </script>

    <script>
     
    
        // --- Cookie banner & modal ---
        function acceptCookies() {
            const checkbox = document.getElementById('chkCookiePolicy');
            if (checkbox && checkbox.checked) {
                localStorage.setItem('cookieConsentAccepted', 'true');
                const banner = document.getElementById('cookieConsentBanner');
                banner.classList.remove('show');
                banner.classList.add('hide');
                setTimeout(function () { banner.style.display = 'none'; }, 800);
            } else {
                alert('Please check "I accept the cookie policy" before proceeding.');
            }
        }

        function toggleCookiePolicy() {
            document.getElementById('cookiePolicyOverlay').style.display = 'block';
        }

        function closeCookiePolicyModal() {
            document.getElementById('cookiePolicyOverlay').style.display = 'none';
        }

        // Show cookie banner on first visit
        (function () {
            const banner = document.getElementById('cookieConsentBanner');
            if (banner && localStorage.getItem('cookieConsentAccepted') !== 'true') {
                banner.style.display = 'block';
                requestAnimationFrame(() => banner.classList.add('show'));
            }
        })();

        // Close overlay if clicking outside modal
        window.addEventListener('click', function (event) {
            var overlay = document.getElementById('cookiePolicyOverlay');
            if (event.target === overlay) overlay.style.display = 'none';
        });
    </script>

    <!-- ====== Cookie Banner ====== -->
    <div id="cookieConsentBanner">
        <span style="font-size: 16px;">🍪 This website uses cookies to ensure you get the best experience.
            <a href="#" onclick="toggleCookiePolicy(); return false;" style="color: var(--primary); text-decoration: underline;">Learn more</a>.
        </span>
        <br><br>
        <div>
            <label style="font-size: 14px; cursor: pointer;">
                <input type="checkbox" id="chkCookiePolicy"
                    style="margin-right: 8px; accent-color: var(--primary); width: 18px; height: 18px; cursor: pointer;">
                I accept the cookie policy.
            </label>
        </div>
        <div style="margin-top: 15px;">
            <button type="button" onclick="acceptCookies();">
                Accept
            </button>
        </div>
    </div>

    <!-- ====== Cookie Policy Modal Overlay ====== -->
    <div id="cookiePolicyOverlay" style="
        display: none; position: fixed; inset: 0; background: rgba(0, 0, 0, 0.5); z-index: 9999;">
        <div id="cookiePolicyModal" style="
            position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
            background: white; padding: 25px; border-radius: var(--radius-lg); box-shadow: 0 4px 15px rgba(0,0,0,0.3);
            width: 90%; max-width: 500px; max-height: 80vh; overflow-y: auto;">
            <h2 style="margin-top: 0; color: var(--primary);">🍪 Cookie Policy</h2>
            <p style="font-size: 14px; color: #333; text-align: justify;">
                We use cookies to enhance your browsing experience, serve personalized content, and analyze our traffic.<br><br>
                By using this website, you consent to our use of cookies as described below:<br><br>
                <strong>Essential Cookies:</strong> Necessary for website functionality (e.g., login, navigation).<br>
                <strong>Performance Cookies:</strong> Help us understand how visitors interact with our website by collecting and reporting information anonymously.<br>
                <strong>Functional Cookies:</strong> Allow the website to remember your preferences (such as language or location).<br>
                <strong>Targeting Cookies:</strong> May be used to deliver relevant advertisements.<br><br>
                🔵 You can choose to accept or decline cookies. Most web browsers automatically accept cookies, but you can modify your browser setting to decline cookies if you prefer.<br><br>
                For more information, please review our Privacy Policy or contact us at <strong>rrctermite@gmail.com</strong>.<br><br>
                Thank you for trusting RRC Termite and Pest Control!
            </p>
            <div style="text-align: center; margin-top: 20px;">
                <button type="button" onclick="closeCookiePolicyModal()"
                    style="background: var(--primary); color: white; border: none; padding: 10px 20px; border-radius: var(--radius-sm); font-weight: bold; cursor: pointer;">
                    Close
                </button>
            </div>
        </div>
    </div>

    <script>
        // Save scroll position before postback
        window.addEventListener('beforeunload', function () {
            sessionStorage.setItem('scrollPosition', window.scrollY);
        });

        // Restore scroll position after reload
        window.addEventListener('load', function () {
            const scrollPos = sessionStorage.getItem('scrollPosition');
            if (scrollPos) {
                window.scrollTo(0, parseInt(scrollPos));
                sessionStorage.removeItem('scrollPosition');
            }
        });
    </script>

    <script>
        document.addEventListener('DOMContentLoaded', function () {
            const modals = document.querySelectorAll('.modal');

            modals.forEach(modal => {
                modal.addEventListener('hidden.bs.modal', function () {
                    document.documentElement.style.scrollBehavior = 'auto';
                    window.scrollTo(window.scrollX, window.scrollY);
                    document.documentElement.style.scrollBehavior = '';
                });
            });
        });
    </script>


</asp:Content>