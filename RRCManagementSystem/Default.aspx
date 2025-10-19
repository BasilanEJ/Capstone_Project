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

        /* ============ Modern Hero Banner ============ */
        .hero { 
            margin-top: 0; 
            padding: 0;
            position: relative;
            overflow: hidden;
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
            background-color: rgba(15, 23, 42, 0.7);
            backdrop-filter: blur(4px);
        }

        .modal.fade .modal-dialog {
            transform: translateY(-50px) scale(0.95);
            opacity: 0;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
        }

        .modal.show .modal-dialog {
            transform: translateY(0) scale(1);
            opacity: 1;
        }

        .modal-content {
            border-radius: var(--radius-lg);
            border: none;
            overflow: hidden;
            box-shadow: 0 20px 60px rgba(0,0,0,0.2);
            background: var(--white);
        }

        .modal-header {
            background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
            color: white;
            border-bottom: none;
            padding: 20px 28px;
        }

        .modal-header .modal-title {
            font-size: 22px;
            font-weight: 600;
            letter-spacing: -0.3px;
        }

        .modal-header .btn-close {
            filter: invert(1);
            opacity: 0.9;
        }

        .modal-body {
            padding: 32px 28px;
        }

        .modal-body img {
            border-radius: var(--radius-md);
            margin-bottom: 24px;
            max-width: 400px;
            max-height: 350px;
            width: 100%;
            height: auto;
            display: block;
            margin-left: auto;
            margin-right: auto;
            object-fit: cover;
            box-shadow: var(--shadow-md);
        }

        .modal-body p {
            font-size: 16px;
            color: var(--gray);
            line-height: 1.7;
            text-align: justify;
            margin-bottom: 20px;
        }

        .modal-body ul {
            list-style-type: none;
            padding-left: 0;
            margin-top: 20px;
        }

        .modal-body ul li {
            text-align: justify;
            font-size: 15px;
            padding: 8px 0;
            color: var(--dark);
            text-indent: -1.5rem;
            padding-left: 1.5rem;
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
        .reviews-section {
            text-align: center;
            padding: 80px 20px;
            background: var(--white);
        }

        .reviews-section h2 {
            font-size: 32px;
            font-weight: 700;
            color: var(--dark);
            margin-bottom: 48px;
        }

        .reviews-grid {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: 24px;
            margin-top: 32px;
        }

        .review-card {
            width: 280px;
            max-width: 90vw;
            padding: 24px;
            background: var(--white);
            border-radius: var(--radius-md);
            box-shadow: var(--shadow-sm);
            text-align: left;
            transition: all 0.3s ease;
            border: 1px solid #e2e8f0;
        }

        .review-card:hover {
            transform: translateY(-4px);
            box-shadow: var(--shadow-md);
        }

        .review-card strong {
            color: var(--dark);
            font-weight: 600;
            font-size: 16px;
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
            margin: 0;
        }

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
    </style>

    <!-- ====== HERO BANNER ====== -->
    <section class="hero">
        <div class="container-fluid p-0 position-relative">
            <img src="/images/rrc1.png" alt="Pest Control Banner" class="hero-banner img-fluid">
        </div>
    </section>

    <!-- ====== SERVICES SECTION ====== -->
    <section class="services py-5">
        <div class="container">
            <div class="text-center mb-4">
                <h2 style="color: gray;">WE PROVIDE THE BEST</h2>
                <h1 style="color: blue;">Termite and Pest Control Services</h1>
            </div>

            <div id="servicesCarousel" class="carousel slide" data-bs-ride="carousel">
                <div class="carousel-inner">

                    <!-- ====== TERMITE CONTROL SERVICES ====== -->
                    <div class="carousel-item active">
                        <h3 class="text-dark mb-3 text-center">Termite Control Services</h3>
                        <div class="row justify-content-center g-3">

                            <!-- Baiting System -->
                            <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                                <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalBaiting">
                                    <img src="/images/service-baiting.jpg" alt="Baiting System" class="service-image">
                                    <h4>Baiting System</h4>
                                    <p class="small">Above Ground and In Ground for Colony Elimination</p>
                                </div>
                            </div>

                            <!-- Termite Prevention -->
                            <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                                <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalTermitePrevention">
                                    <img src="/images/service-termite-prevention.jpg" alt="Termite Prevention" class="service-image">
                                    <h4>Termite Prevention</h4>
                                    <p class="small">Stops infestations before they begin with long-term barrier protection</p>
                                </div>
                            </div>

                            <!-- Soil Poisoning -->
                            <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                                <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalSoilPoisoning">
                                    <img src="/images/service-soil.jpg" alt="Soil Poisoning" class="service-image">
                                    <h4>Soil Poisoning</h4>
                                    <p class="small">Build a strong foundation with termite-proof protection for new & existing homes.</p>
                                </div>
                            </div>

                            <!-- Reticulation -->
                            <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                                <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalReticulation">
                                    <img src="/images/services-reticulations.jpg" alt="Reticulation" class="service-image">
                                    <h4>Reticulation</h4>
                                    <p class="small">Smart termite defense with a hidden pipe system for hassle-free treatments.</p>
                                </div>
                            </div>

                            <!-- Mound Demolition -->
                            <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                                <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalMoundDemolition">
                                    <img src="/images/service-mound.jpg" alt="Mound Demolition" class="service-image">
                                    <h4>Mound Demolition</h4>
                                    <p class="small">Directly eliminate termite colonies by targeting the queen at the source.</p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- ====== GENERAL PEST CONTROL SERVICES ====== -->
                    <div class="carousel-item">
                        <h3 class="text-dark mb-3 text-center">General Pest Control Services</h3>
                        <div class="row justify-content-center g-3">
                            
                            <!-- General Pest Control -->
                            <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                                <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalGeneralPest">
                                    <img src="/images/service-general-pest.jpg" alt="General Pest Control" class="service-image">
                                    <h4>General Pest Control</h4>
                                    <p class="small">Cockroaches, Ants, Mosquitoes, Flies</p>
                                </div>
                            </div>

                            <!-- Tick & Fleas Control -->
                            <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                                <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalTickFleas">
                                    <img src="/images/service-tick-fleas.jpg" alt="Tick & Fleas Control" class="service-image">
                                    <h4>Tick & Fleas Control</h4>
                                    <p class="small">Breaks the breeding cycle and protects your pets & family</p>
                                </div>
                            </div>

                            <!-- Bedbugs Control -->
                            <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                                <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalBedbugs">
                                    <img src="/images/service-bedbugs.jpg" alt="Bedbugs Control" class="service-image">
                                    <h4>Bedbugs Control</h4>
                                    <p class="small">Thorough inspection and treatment for lasting relief</p>
                                </div>
                            </div>

                            <!-- Rat / Rodents Control -->
                            <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                                <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalRatControl">
                                    <img src="/images/service-rats.jpg" alt="Rat Control" class="service-image">
                                    <h4>Rat / Rodents Control</h4>
                                    <p class="small">Safe and effective removal to prevent health risks</p>
                                </div>
                            </div>
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

    <!-- ======================================
         TERMITE CONTROL SERVICES MODALS
    ====================================== -->

    <!-- Baiting System Modal -->
    <div class="modal fade" id="modalBaiting" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Baiting System</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <img src="/images/service-baiting.jpg" alt="Baiting System" class="img-fluid mb-3 rounded">
                    <p>
                        Our termite baiting system is one of the most advanced and environmentally responsible methods
                        available for colony elimination. Bait stations are carefully placed above ground in active areas
                        and in-ground around the perimeter of your property. Termites consume the specially formulated
                        bait and unknowingly spread it throughout the colony, including to the queen.
                    </p>
                    <p>
                        This process leads to the gradual but complete elimination of the entire termite population.
                        Regular inspections ensure the stations remain effective and monitored over time.
                    </p>
                    <ul>
                        <li>✅ Targets the colony at its source</li>
                        <li>✅ Minimal chemical use, safe for sensitive environments</li>
                        <li>✅ Monitored and maintained for long-term effectiveness</li>
                        <li>✅ Safe for families, pets, and the environment</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- Termite Prevention Modal -->
    <div class="modal fade" id="modalTermitePrevention" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Termite Prevention</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <img src="/images/service-termite-prevention.jpg" alt="Termite Prevention" class="img-fluid mb-3 rounded">
                    <p>
                        Prevention is the smartest and most cost-effective way to handle termites before they cause damage.
                        Our preventive services create a protective barrier that blocks termites from ever reaching your property.
                        Using a mix of physical barriers, liquid termiticides, and advanced technology,
                        we provide coverage for both new and existing structures.
                    </p>
                    <p>
                        Regular inspections allow us to identify risks early and reinforce your defenses,
                        protecting your property value and preventing expensive repairs in the future.
                    </p>
                    <ul>
                        <li>✅ Stops infestations before they begin</li>
                        <li>✅ Long-term barrier protection for homes and businesses</li>
                        <li>✅ Reduces costly repair risks by preventing structural damage</li>
                        <li>✅ Ideal for both residential and commercial properties</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- Soil Poisoning Modal -->
    <div class="modal fade" id="modalSoilPoisoning" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Soil Poisoning (Pre & Post Construction)</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <img src="/images/service-soil.jpg" alt="Soil Poisoning" class="img-fluid mb-3 rounded">
                    <p>
                        Soil treatment, also called soil poisoning, is one of the most trusted termite-proofing techniques.
                        For pre-construction, we apply a termiticide treatment to the soil before the foundation is laid,
                        creating a shield that termites cannot cross.
                    </p>
                    <p>
                        For post-construction, we drill around the foundation and inject chemicals deep into the soil to
                        reinforce protection. Both methods create a continuous chemical barrier that protects against termite entry.
                    </p>
                    <ul>
                        <li>✅ Essential for long-term structural protection</li>
                        <li>✅ Provides defense for both new builds and existing properties</li>
                        <li>✅ Creates a continuous barrier termites cannot cross</li>
                        <li>✅ Peace of mind against hidden termite threats</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- Reticulation Modal -->
    <div class="modal fade" id="modalReticulation" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Reticulation (Perforated Pipe System)</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <img src="/images/services-reticulations.jpg" alt="Reticulation" class="img-fluid mb-3 rounded">
                    <p>
                        Our reticulation system is a modern termite management solution that makes re-treatment simple and efficient.
                        A network of underground perforated pipes is installed around your property's foundation,
                        allowing termiticide to be evenly distributed in the soil.
                    </p>
                    <p>
                        When it's time for re-application, chemicals can be delivered directly into the system without drilling
                        or damaging floors. This provides a long-term, cost-effective solution for termite management.
                    </p>
                    <ul>
                        <li>✅ Even, reliable distribution of termiticide</li>
                        <li>✅ Easy re-application without drilling or disruption</li>
                        <li>✅ Long-lasting and cost-effective protection</li>
                        <li>✅ Discreet system that preserves your property's appearance</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- Mound Demolition Modal -->
    <div class="modal fade" id="modalMoundDemolition" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Mound Demolition (Queen Finder)</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <img src="/images/service-mound.jpg" alt="Mound Demolition" class="img-fluid mb-3 rounded">
                    <p>
                        For properties with visible termite mounds, mound demolition provides a direct and highly effective treatment.
                        Our specialists carefully dismantle the mound, apply treatment, and locate the queen for elimination.
                        Removing the queen ensures the colony cannot rebuild and prevents further spread.
                    </p>
                    <ul>
                        <li>✅ Fast and decisive colony elimination</li>
                        <li>✅ Long-term results by directly targeting the queen</li>
                        <li>✅ Prevents spread of termites to nearby areas</li>
                        <li>✅ Effective solution for large outdoor infestations</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- ======================================
         GENERAL PEST CONTROL SERVICES MODALS
    ====================================== -->

    <!-- General Pest Control Modal -->
    <div class="modal fade" id="modalGeneralPest" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">General Pest Control Treatment</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <img src="/images/service-general-pest.jpg" alt="General Pest Control" class="img-fluid mb-3 rounded">
                    <p>
                        Our general pest control services cover the most common pests that threaten health, hygiene, and comfort.
                        Each pest is treated using specialized methods:
                    </p>
                    <ul>
                        <li><strong>Cockroaches:</strong> Targeted sprays and gel baits reach deep into hiding places.</li>
                        <li><strong>Ants:</strong> Eliminate visible ants and hidden colonies to prevent reinfestation.</li>
                        <li><strong>Mosquitoes:</strong> Reduce breeding areas and apply larvicides and residual sprays.</li>
                        <li><strong>Flies:</strong> Traps, sprays, and sanitation recommendations reduce contamination risks.</li>
                    </ul>
                    <p>
                        This integrated approach ensures healthier, cleaner, and pest-free surroundings.
                    </p>
                    <ul>
                        <li>✅ Comprehensive coverage for common household pests</li>
                        <li>✅ Treatments tailored to each species and infestation level</li>
                        <li>✅ Protects families and businesses from health risks</li>
                        <li>✅ Creates a cleaner, more comfortable environment</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- Tick & Fleas Control Modal -->
    <div class="modal fade" id="modalTickFleas" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Tick & Fleas Control</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <img src="/images/service-tick-fleas.jpg" alt="Tick & Fleas Control" class="img-fluid mb-3 rounded">
                    <p>
                        Ticks and fleas are more than just annoying—they can transmit harmful diseases to both humans and pets.
                        Our treatments target every stage of their lifecycle, from eggs to adults, to break the cycle of infestation.
                    </p>
                    <p>
                        We focus on key areas such as carpets, pet bedding, gardens, and shaded spots where these pests thrive,
                        while providing preventive advice for long-term protection.
                    </p>
                    <ul>
                        <li>✅ Safe for households with pets and children</li>
                        <li>✅ Breaks the breeding cycle for long-term results</li>
                        <li>✅ Reduces risks of diseases like Lyme disease and flea-borne fevers</li>
                        <li>✅ Targets indoor and outdoor hotspots</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- Bedbugs Control Modal -->
    <div class="modal fade" id="modalBedbugs" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Bedbugs Control</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <img src="/images/service-bedbugs.jpg" alt="Bedbugs Control" class="img-fluid mb-3 rounded">
                    <p>
                        Bedbugs are notorious for hiding in hard-to-reach areas like mattresses, furniture, and cracks.
                        Our service begins with a thorough inspection to identify all hiding spots, followed by targeted treatments
                        like heat and residual sprays to kill bedbugs at every stage.
                    </p>
                    <p>
                        Follow-up visits ensure complete eradication and prevent re-infestation, restoring comfort and peace of mind.
                    </p>
                    <ul>
                        <li>✅ Comprehensive inspection to locate infestations</li>
                        <li>✅ Treatments that eliminate eggs, nymphs, and adults</li>
                        <li>✅ Effective for both minor and severe infestations</li>
                        <li>✅ Restful, bite-free sleep restored</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- Rat & Rodents Control Modal -->
    <div class="modal fade" id="modalRatControl" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Rat & Rodents Control</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <img src="/images/service-rats.jpg" alt="Rat & Rodents Control" class="img-fluid mb-3 rounded">
                    <p>
                        Rodents like rats and mice pose serious threats to health and property. They spread diseases,
                        contaminate food, and can cause electrical fires by chewing wires.
                    </p>
                    <p>
                        Our service uses traps, baits, and exclusion methods to remove infestations and prevent recurrence.
                        We also seal entry points and advise on sanitation practices to keep them away for good.
                    </p>
                    <ul>
                        <li>✅ Safe and effective elimination methods</li>
                        <li>✅ Preventive measures to block future infestations</li>
                        <li>✅ Protects property from costly damage</li>
                        <li>✅ Reduces health risks linked to rodent-borne diseases</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- ====== ABOUT SPLIT ====== -->
    <section class="hero">
        <div class="container">
            <div class="row align-items-center gy-4">
                <div class="col-md-6 text-center">
                    <img src="/images/ppe.png" alt="Pest Control Worker" class="img-fluid hero-img">
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
    <section class="hero video-hero">
        <div class="ratio-box d-md-none"></div>
        <h1 class="video-title"></h1>

        <div class="video-layer">
            <img id="videoThumbnail" src="/images/banner tv.jpg" alt="Video Thumbnail" class="thumb" style="cursor:pointer;">
            <iframe id="vimeoVideo"
                    src="https://player.vimeo.com/video/1009218555?loop=1&muted=0"
                    allow="autoplay; fullscreen"
                    allowfullscreen
                    style="display:none;"></iframe>
            <div id="playButton" class="play-btn">▶ Play</div>
        </div>
    </section>

    <!-- ====== Blog Section ====== -->
    <section class="blog-section">
        <h2>Read our Blogs</h2>
        <div class="blog-cards">
            
            <!-- Blog Card 1 -->
            <a href="DIY.aspx" class="blog-card">
                <img src="/Images/DIY.jpg" alt="DIY vs Professional Pest Control">
                <div class="blog-card-content">
                    <h3>DIY pest control vs. hiring a pest control company: What's the difference?</h3>
                </div>
            </a>

            <!-- Blog Card 2 -->
            <a href="Eskwela.aspx" class="blog-card">
                <img src="/Images/blog2.jpg" alt="Brigada Eskwela Anti-Dengue">
                <div class="blog-card-content">
                    <h3>Brigada Eskwela Anti-Dengue Campaign: Ensuring a Safe and Healthy Learning Environment</h3>
                </div>
            </a>

            <!-- Blog Card 3 -->
            <a href="Termite.aspx" class="blog-card">
                <img src="/Images/blog3.jpg" alt="Termite Swarms">
                <div class="blog-card-content">
                    <h3>Don't Let Termite Swarms Take Over Your Home!</h3>
                </div>
            </a>

        </div>
    </section>

    <!-- ====== C&O ====== -->
    <section class="co-section">
        <h2>Certifications & Organizations</h2>
        <div style="display:flex; justify-content:center; align-items:center; margin-top:20px;">
            <img src="/images/c&o.png" alt="Certifications & Organizations">
        </div>
    </section>

    <!-- ====== REVIEWS ====== -->
    <section class="reviews-section">
        <h2>What our customers are saying</h2>

        <div class="reviews-grid">
            <div class="review-card">
                <strong>Czarina Joy T. Chang</strong>
                <p class="recommends">❤️ recommends</p>
                <p>They were on time, very professional and mababait mga staff ni RRC team. Mabusisi sila sa bawat sulok ng bahay at maayos silang magtrabaho. Very polite and courteous pa yun technicians and staff na nag execute ng baiting system. Highly recommended! Good job!</p>
            </div>

            <div class="review-card">
                <strong>Mel Lareza</strong>
                <p class="recommends">❤️ recommends</p>
                <p>Excellent Service!!! ⭐️⭐️⭐️⭐️⭐️</p>
            </div>

            <div class="review-card">
                <strong>Dannica Manaluz.</strong>
                <p class="recommends">❤️ recommends</p>
                <p>Very satisfied client here 5/5 stars ⭐️⭐️⭐️⭐️⭐️</p>
            </div>

            <div class="review-card">
                <strong>Chelsea Erese Ong</strong>
                <p class="recommends">❤️ recommends</p>
                <p>RRC/Sir Victor and staff of technicians were very accommodating and understanding despite us having to change schedule of termite treatment...</p>
            </div>
        </div>
    </section>

    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <!-- ====== INQUIRY FORM (Responsive with UpdatePanel) ====== -->
    <asp:UpdatePanel ID="upInquiryForm" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <section id="inquiryForm" class="inquiry-wrap">
                <div class="inquiry-card">
          
                    <div class="inquiry-left">
                        <h3>Schedule Your</h3>
                        <h1>Free Inspection</h1>
                        <p>
                            Schedule today! Please fill-in this form and RRC Pest and Termite Control Representative will contact you soon.
                        </p>

                        <div style="margin-bottom: 15px;">
                            <asp:Label runat="server" AssociatedControlID="txtEmail" Text="Your email *" />
                            <asp:TextBox ID="txtEmail" runat="server"
                                CssClass="form-control form-underline"
                                TextMode="Email"
                                placeholder="email@gmail.com"
                                required />
                            <asp:RegularExpressionValidator ID="revEmail" runat="server"
                                ControlToValidate="txtEmail"
                                ErrorMessage="Please enter a valid Gmail, Yahoo, Outlook, iCloud, or school/government email address."
                                ForeColor="Red"
                                Display="Dynamic"
                                ValidationExpression="^[A-Za-z0-9._%+\-]+@(?:(?:gmail|yahoo|outlook|hotmail|live|icloud)\.com|(?:[A-Za-z0-9-]+\.)*edu\.ph|(?:[A-Za-z0-9-]+\.)*gov\.ph)$" />
                        </div>

                        <div style="margin-bottom: 15px;">
                            <asp:TextBox ID="txtContactNumber" runat="server"
                                placeholder="09xxxxxxxxx" required MaxLength="11"
                                onkeypress="return isDigit(event)"
                                onkeydown="return blockNonDigits(event)"
                                oninput="validateContactNumber(this)"
                                onpaste="handlePaste(event)"
                                CssClass="form-underline" />
                        </div>

                        <div style="margin-bottom: 15px;">
                            <asp:Label runat="server" AssociatedControlID="fuPestPhoto" Text="Photo of Pest (optional)" />
                            <asp:FileUpload ID="fuPestPhoto" runat="server"
                                style="width: 100%; background: rgba(255,255,255,0.1); padding: 10px; border-radius: 8px; color: white;"
                                accept=".png,.jpg,.jpeg,image/png,image/jpeg" />
                            <small style="color: rgba(255,255,255,0.8);">Upload a photo if available.</small>
                        </div>

                        <div style="margin-bottom: 15px;">
                            <asp:Label runat="server" AssociatedControlID="txtMessage" Text="Describe what you observed (optional)" />
                            <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" Rows="4"
                                placeholder="Describe what you observed..." CssClass="textarea-box" />
                        </div>

                        <div style="margin-bottom: 10px;">
                            <a href="#" data-bs-toggle="modal" data-bs-target="#termsModal" style="font-size: 14px; color: rgba(255,255,255,0.9); text-decoration: underline;">
                                View Terms and Conditions
                            </a>
                        </div>

                        <!-- Terms Modal -->
                        <div class="modal fade" id="termsModal" tabindex="-1" aria-labelledby="termsModalLabel" aria-hidden="true">
                            <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
                                <div class="modal-content">

                                    <div class="modal-header">
                                        <h5 class="modal-title" id="termsModalLabel">Terms & Conditions / Privacy Policy</h5>
                                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                                    </div>

                                    <div class="modal-body" style="color:#1a202c; line-height:1.45;">
                                    
                                        <h6 class="mb-2">1. Agreement to Terms</h6>
                                        <p>
                                            Welcome to RRC Termite &amp; Pest Control Inquiry! By accessing and using our inquiry page,
                                            you agree to comply with and be bound by these Terms and Conditions.
                                            Your use of RRC Inquiry is subject to the Philippine Data Privacy Act of 2012 (DPA 2012) and other relevant laws.
                                            These Terms govern your use of the inquiry form, the accuracy of information you provide,
                                            and how we may process your submitted data. You agree to:
                                        </p>
                                        <ul>
                                            <li>Use the inquiry page only for lawful purposes.</li>
                                            <li>Provide truthful, accurate, and current information.</li>
                                            <li>Refrain from submitting harmful, offensive, or unsolicited content.</li>
                                        </ul>
                                        <p>
                                            If you do not agree to these Terms, you must not submit any inquiries or otherwise use the inquiry page.
                                            By proceeding, you acknowledge that you have read, understood, and accepted these Terms.
                                        </p>

                                        <h6 class="mt-3 mb-2">2. Definitions</h6>
                                        <ul>
                                            <li><strong>"RRC Inquiry"</strong> – The official online platform of RRC Termite &amp; Pest Control,
                                                provided for submitting inquiries, viewing bookings, bills, and booking schedules.</li>
                                            <li><strong>"User"</strong> – Any individual accessing RRC Inquiry, including customers, inspectors, and administrators.</li>
                                            <li><strong>"Personal Data"</strong> – Information that identifies or can identify a person, as defined under the DPA 2012.</li>
                                            <li><strong>"Services"</strong> – The inquiry-related functions including submission of inquiries, viewing of bookings, bills, and schedules.</li>
                                        </ul>

                                        <h6 class="mt-3 mb-2">3. Purpose of the Inquiry Page</h6>
                                        <p>
                                            This page is provided solely for users to submit inquiries, view their bookings, bills, and schedules.
                                            It is not intended for placing orders, entering into contracts, or seeking emergency assistance unless explicitly stated otherwise.
                                        </p>

                                        <h6 class="mt-3 mb-2">4. Information Accuracy</h6>
                                        <p>
                                            You agree that all information submitted is accurate, current, and complete.
                                            You are responsible for maintaining the confidentiality of any account or contact information
                                            and notifying us immediately of any unauthorized use.
                                        </p>

                                        <h6 class="mt-3 mb-2">5. Use Restrictions</h6>
                                        <ul>
                                            <li>Submitting offensive, discriminatory, defamatory, or harassing content.</li>
                                            <li>Uploading or sharing harmful, dangerous, or spam content.</li>
                                            <li>Using automated systems or bots to send inquiries.</li>
                                        </ul>
                                        <p>
                                            We reserve the right to decline inquiries that violate these guidelines or block repeat offenders.
                                        </p>

                                        <h6 class="mt-3 mb-2">6. Privacy &amp; Data Use</h6>
                                        <p>
                                            Any personal data you submit will be processed in accordance with our Privacy Policy.
                                            We keep data only as long as necessary to respond to inquiries and for legitimate business purposes.
                                        </p>

                                        <h6 class="mt-3 mb-2">7. Intellectual Property</h6>
                                        <p>
                                            All content of the inquiry form and related materials is our property or licensed to us.
                                            You may not reproduce, distribute, modify, or create derivative works from it without written permission.
                                        </p>

                                        <h6 class="mt-3 mb-2">8. Disclaimer of Warranty</h6>
                                        <p>
                                            The inquiry form is provided "as-is" and "as-available."
                                            We make no warranties—express or implied—regarding its accuracy, reliability, or availability.
                                            All inquiries submitted are at your own risk.
                                        </p>

                                        <h6 class="mt-3 mb-2">9. Limitation of Liability</h6>
                                        <p>
                                            To the maximum extent permitted by law, we shall not be liable for any indirect, incidental, special,
                                            or consequential damages arising from the use of the inquiry page, even if we have been advised of the possibility of such damages.
                                        </p>

                                        <h6 class="mt-3 mb-2">10. Modification and Interruptions</h6>
                                        <p>
                                            We reserve the right to modify, suspend, or discontinue the inquiry page at any time, with or without notice.
                                            We are not liable for any interruptions or errors and may revise these Terms at our discretion.
                                        </p>

                                        <h6 class="mt-3 mb-2">11. Governing Law</h6>
                                        <p>
                                            These Terms are governed by the laws of the Republic of the Philippines.
                                            Any disputes will be subject to the jurisdiction of Philippine courts.
                                        </p>

                                        <h6 class="mt-3 mb-2">12. Changes to Terms</h6>
                                        <p>
                                            We may update these Terms periodically. Continued use after changes constitutes agreement to those revisions.
                                        </p>

                                        <h6 class="mt-3 mb-2">13. Severability</h6>
                                        <p>
                                            If any provision is found unenforceable, the remainder will remain in effect to the fullest extent permitted by law.
                                        </p>

                                        <h6 class="mt-3 mb-2">14. Contact Information</h6>
                                        <p>
                                            Email: <a href="mailto:rrctermiteandpestcontrol@gmail.com">rrctermiteandpestcontrol@gmail.com</a><br />
                                            Address: #33 Kaligatasan Street, Brgy. Holy Spirit, Quezon City, Philippines
                                        </p>

                                        <hr class="my-3" />

                                        <h6 class="mb-2">Privacy Policy — Inquiry Page</h6>
                                        <p><em>Last Updated: August 19, 2025</em></p>

                                        <p>
                                            RRC Termite &amp; Pest Control respects your privacy and is committed to protecting your personal data
                                            in compliance with the Data Privacy Act of 2012 (Republic Act No. 10173) of the Philippines.
                                            This Privacy Policy explains how we collect, use, store, and protect your information when you use our inquiry page.
                                        </p>

                                        <h6 class="mt-3 mb-2">1. Collection of Personal Data</h6>
                                        <p>When you use the RRC Inquiry page, we may collect personal data such as:</p>
                                        <ul>
                                            <li>Name</li>
                                            <li>Contact number</li>
                                            <li>Email address</li>
                                            <li>Details of your inquiry, bookings, bills, and schedules</li>
                                        </ul>

                                        <h6 class="mt-3 mb-2">2. Purpose of Data Collection</h6>
                                        <ul>
                                            <li>To respond to your inquiries</li>
                                            <li>To allow you to view your bookings, bills, and schedules</li>
                                            <li>To improve our services and customer experience</li>
                                            <li>To comply with legal and regulatory requirements</li>
                                        </ul>

                                        <h6 class="mt-3 mb-2">3. Data Sharing and Disclosure</h6>
                                        <p>
                                            We do not sell, trade, or otherwise transfer your personal data to third parties without your consent,
                                            except when required by law, regulation, or competent authority.
                                        </p>

                                        <h6 class="mt-3 mb-2">4. Data Retention</h6>
                                        <p>
                                            We will retain your personal data only for as long as necessary to fulfill the purposes stated above,
                                            and as required by applicable laws and regulations.
                                        </p>

                                        <h6 class="mt-3 mb-2">5. Data Security</h6>
                                        <p>
                                            We implement appropriate organizational, physical, and technical measures to protect your personal data
                                            from unauthorized access, alteration, disclosure, or destruction.
                                        </p>

                                        <h6 class="mt-3 mb-2">6. User Rights Under the DPA 2012</h6>
                                        <ul>
                                            <li>The right to be informed</li>
                                            <li>The right to access</li>
                                            <li>The right to rectification</li>
                                            <li>The right to object (in certain cases)</li>
                                            <li>The right to erasure/blocking when no longer necessary</li>
                                            <li>The right to data portability</li>
                                            <li>The right to lodge a complaint with the NPC</li>
                                        </ul>

                                        <h6 class="mt-3 mb-2">7. Updates to this Privacy Policy</h6>
                                        <p>
                                            We may update this Privacy Policy from time to time to reflect changes in laws, technology, or business practices.
                                            Any updates will be posted on this page with a new effective date.
                                        </p>

                                        <h6 class="mt-3 mb-2">8. Contact Information</h6>
                                        <p>
                                            Email: <a href="mailto:rrctermiteandpestcontrol@gmail.com">rrctermiteandpestcontrol@gmail.com</a><br />
                                            Address: #33 Kaligatasan Street, Brgy. Holy Spirit, Quezon City, Philippines
                                        </p>

                                        <div class="mt-3 form-check">
                                            <asp:CheckBox ID="chkTerms" runat="server" CssClass="form-check-input" />
                                            <label class="form-check-label" for="<%= chkTerms.ClientID %>">
                                                I agree to the Terms &amp; Conditions and Privacy Policy.
                                            </label>
                                            
                                            <span id="termsError" style="display:none; color:red; font-size:13px; margin-top:5px;">
                                                Please agree to the terms and conditions before submitting.
                                            </span>
                                        </div>
                                    </div>

                                    <div class="modal-footer">
                                        <button type="button" class="btn btn-primary" data-bs-dismiss="modal">OK</button>
                                    </div>

                                </div>
                            </div>
                        </div>

                        <asp:Button 
                            ID="btnSubmitInquiry" 
                            runat="server" 
                            Text="Submit Inquiry" 
                            CssClass="btn-submit"
                            OnClick="btnSubmitInquiry_Click" />

                    </div>

                    <div class="inquiry-right" aria-hidden="true"></div>
                </div>
            </section>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnSubmitInquiry" />
        </Triggers>
    </asp:UpdatePanel>

    <script>
        function validateTerms() {
            var chkTerms = document.getElementById('<%= chkTerms.ClientID %>');

            if (!chkTerms.checked) {
                var termsModal = new bootstrap.Modal(document.getElementById('termsModal'));
                termsModal.show();
                return false;
            }
            return true;
        }
    </script>

    <script>
        // --- Vimeo Play logic ---
        const playButton = document.getElementById("playButton");
        const videoThumbnail = document.getElementById("videoThumbnail");
        const vimeoVideo = document.getElementById("vimeoVideo");

        if (playButton && videoThumbnail && vimeoVideo) {
            playButton.addEventListener("click", function () {
                const src = vimeoVideo.getAttribute("src");
                const nextSrc = src.includes("autoplay=1") ? src : (src + (src.includes("?") ? "&" : "?") + "autoplay=1");
                vimeoVideo.setAttribute("src", nextSrc);

                videoThumbnail.style.display = "none";
                playButton.style.display = "none";
                vimeoVideo.style.display = "block";
            });
        }

        // --- Contact number validation ---
        function isDigit(e) {
            const charCode = e.which || e.keyCode;
            return (charCode >= 48 && charCode <= 57);
        }

        function blockNonDigits(e) {
            const key = e.key;
            const isCtrlOrCmd = e.ctrlKey || e.metaKey;
            const allowedKeys = ["Backspace", "Delete", "ArrowLeft", "ArrowRight", "Tab", "Home", "End"];
            if (allowedKeys.includes(key) || isCtrlOrCmd) return true;
            return /^\d$/.test(key);
        }

        function validateContactNumber(input) {
            input.value = input.value.replace(/\D/g, '');
            if (input.value.length > 11) input.value = input.value.slice(0, 11);
            if (input.value.length > 0 && !input.value.startsWith("09")) {
                input.setCustomValidity("Contact number must start with 09.");
            } else {
                input.setCustomValidity("");
            }
        }

        function handlePaste(e) {
            const paste = (e.clipboardData || window.clipboardData).getData('text');
            if (!/^09\d{0,9}$/.test(paste)) e.preventDefault();
        }

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