<%@ Page Title="About Us" Language="C#" MasterPageFile="~/Inquiry.Master" AutoEventWireup="true" CodeBehind="AboutUs.aspx.cs" Inherits="RRCManagementSystem.AboutUs" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap CSS (if not already in master page) -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- Font Awesome -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
    <!-- AOS (Animate On Scroll) -->
    <link href="https://unpkg.com/aos@2.3.4/dist/aos.css" rel="stylesheet" />

    <style>
        /* Page Animation */
        .page-content {
            animation: fadeIn 0.6s ease-out;
        }

        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(20px); }
            to { opacity: 1; transform: translateY(0); }
        }

        /* Hero Section */
        .hero-section {
            background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 50%, #ffffff 100%);
            position: relative;
            overflow: hidden;
        }

        .hero-section::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: radial-gradient(circle at 30% 50%, rgba(59, 130, 246, 0.1) 0%, transparent 50%);
            pointer-events: none;
        }

        /* Section Headers */
        .section-header {
            position: relative;
            padding-bottom: 1rem;
            margin-bottom: 3rem;
        }

        .section-header::after {
            content: '';
            position: absolute;
            bottom: 0;
            left: 50%;
            transform: translateX(-50%);
            width: 80px;
            height: 4px;
            background: linear-gradient(90deg, #3b82f6, #2563eb);
            border-radius: 2px;
        }

        /* Card Hover Effects */
        .hover-card {
            transition: all 0.3s ease;
            height: 100%;
        }

        .hover-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 20px 40px rgba(59, 130, 246, 0.2);
        }

        /* Image Container */
        .image-container {
            position: relative;
            overflow: hidden;
            border-radius: 1rem;
        }

        .image-container img {
            transition: transform 0.5s ease;
        }

        .image-container:hover img {
            transform: scale(1.05);
        }

        /* Client Logo Cards */
        .client-card {
            height: 160px;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            transition: all 0.3s ease;
            background: white;
        }

        .client-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 8px 24px rgba(59, 130, 246, 0.15);
        }

        .client-logo-container {
            height: 80px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 12px;
        }

        .client-logo {
            max-height: 64px;
            object-fit: contain;
            transition: transform 0.3s ease;
        }

        .client-card:hover .client-logo {
            transform: scale(1.1);
        }

        .client-name {
            height: 48px;
            display: flex;
            align-items: center;
            justify-content: center;
            text-align: center;
        }

        /* Values List */
        .values-list li {
            position: relative;
            padding-left: 1.5rem;
        }

        .values-list li::before {
            content: '✓';
            position: absolute;
            left: 0;
            color: #3b82f6;
            font-weight: bold;
        }

        /* Project Gallery */
        .project-card {
            position: relative;
            overflow: hidden;
            border-radius: 1rem;
            transition: all 0.3s ease;
        }

        .project-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 16px 32px rgba(0, 0, 0, 0.15);
        }

        .project-card img {
            transition: transform 0.5s ease;
        }

        .project-card:hover img {
            transform: scale(1.1);
        }

        /* Back to Home Button */
        .btn-back-home {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            padding: 1rem 2.5rem;
            border-radius: 0.75rem;
            font-weight: 600;
            display: inline-flex;
            align-items: center;
            gap: 0.75rem;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
            text-decoration: none;
            border: none;
        }

        .btn-back-home:hover {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(59, 130, 246, 0.4);
            color: white;
        }

        .btn-back-home i {
            transition: transform 0.3s ease;
        }

        .btn-back-home:hover i {
            transform: translateX(-4px);
        }

        /* Icon Containers */
        .icon-box {
            width: 56px;
            height: 56px;
            background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
            border-radius: 0.75rem;
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }

        /* Stats or Highlight Numbers */
        .stat-number {
            font-size: 3rem;
            font-weight: 800;
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        /* Additional Styling */
        .border-top-custom {
            border-top: 4px solid !important;
        }

        .text-blue-600 {
            color: #2563eb;
        }

        .text-blue-700 {
            color: #1d4ed8;
        }

        .bg-gradient-custom {
            background: linear-gradient(to right, #f9fafb, #eff6ff);
        }

        .project-card img,
        .image-container img {
            width: 100%;
            height: 320px;
            object-fit: cover;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-content">
        <!-- HERO SECTION -->
        <section class="hero-section py-5 text-dark position-relative">
            <div class="container py-md-5 text-center position-relative" style="z-index: 10;">
                <div data-aos="fade-down" data-aos-duration="900">
                    <h1 class="display-3 fw-bold text-blue-700 mb-3">
                        Take Command and Control of Your Pest Problem
                    </h1>
                    <p class="fs-4 fw-medium mb-2">
                        Your Partner in Safe and Reliable Pest Control
                    </p>
                    <p class="text-muted fst-italic">
                        Please don't pet the pests.
                    </p>
                </div>
            </div>
        </section>

        <div class="container my-5">
            <!-- ABOUT SECTION -->
            <section id="about" class="mb-5">
                <div class="card shadow-sm border-0">
                    <div class="card-body p-4 p-md-5">
                        <div class="row g-4 g-md-5 align-items-center">
                            <div class="col-md-6" data-aos="fade-right" data-aos-duration="800">
                                <h2 class="display-5 fw-bold mb-3 d-flex align-items-center gap-3">
                                    <i class="fas fa-building text-blue-600"></i>
                                    About RRC
                                </h2>
                                <h3 class="fs-4 fw-semibold text-blue-600 mb-3">Termite & Pest Control Services</h3>
                                <p class="text-muted mb-3">
                                    It all began with a small capital in 2008. Through hard work, skilled staff, and continuous improvement,
                                    RRC grew into a quality-focused pest control service that balances world-class methods with affordability.
                                </p>
                                <p class="mb-3">
                                    <strong class="text-blue-600">RRC — Recovery, Resource, and Control</strong> — helps clients retrieve assets from pest damage,
                                    control infestations, and prevent recurrences.
                                </p>
                                <p class="text-muted">
                                    We pride ourselves on having the talent, experience, and tools necessary to provide effective, safe, and timely
                                    pest management solutions for homes and businesses.
                                </p>
                            </div>

                            <div class="col-md-6" data-aos="fade-left" data-aos-duration="800">
                                <div class="image-container shadow-lg">
                                    <img src="/Images/rrcteam.png" alt="RRC Team" class="img-fluid">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- MISSION / VISION / VALUES SECTION -->
            <section id="mv" class="mb-5">
                <h3 class="section-header text-center display-6 fw-bold" data-aos="fade-up">
                    Mission • Vision • Values
                </h3>
                <div class="row g-4">
                    <!-- Mission -->
                    <div class="col-md-4" data-aos="zoom-in" data-aos-delay="50">
                        <div class="card hover-card shadow-sm border-0 border-top-custom border-primary">
                            <div class="card-body p-4">
                                <div class="d-flex align-items-center gap-3 mb-3">
                                    <div class="icon-box">
                                        <i class="fas fa-bullseye fs-3 text-blue-600"></i>
                                    </div>
                                    <h4 class="fs-5 fw-bold mb-0">Our Mission</h4>
                                </div>
                                <p class="text-muted mb-0">
                                    To satisfy our clients by giving safe and effective treatments, creating jobs, and providing quality service at reasonable prices.
                                </p>
                            </div>
                        </div>
                    </div>

                    <!-- Vision -->
                    <div class="col-md-4" data-aos="zoom-in" data-aos-delay="150">
                        <div class="card hover-card shadow-sm border-0 border-top-custom border-primary">
                            <div class="card-body p-4">
                                <div class="d-flex align-items-center gap-3 mb-3">
                                    <div class="icon-box">
                                        <i class="fas fa-eye fs-3 text-blue-600"></i>
                                    </div>
                                    <h4 class="fs-5 fw-bold mb-0">Our Vision</h4>
                                </div>
                                <p class="text-muted mb-0">
                                    To be the leading pest control service in the country that values clients' needs by providing safe and effective treatment for peace of mind.
                                </p>
                            </div>
                        </div>
                    </div>

                    <!-- Values -->
                    <div class="col-md-4" data-aos="zoom-in" data-aos-delay="250">
                        <div class="card hover-card shadow-sm border-0 border-top-custom border-primary">
                            <div class="card-body p-4">
                                <div class="d-flex align-items-center gap-3 mb-3">
                                    <div class="icon-box">
                                        <i class="fas fa-heart fs-3 text-blue-600"></i>
                                    </div>
                                    <h4 class="fs-5 fw-bold mb-0">Our Values</h4>
                                </div>
                                <ul class="values-list list-unstyled small mb-0">
                                    <li class="mb-2"><strong>Integrity:</strong> We conduct business responsibly and ethically.</li>
                                    <li class="mb-2"><strong>Accountability:</strong> We honor commitments and accept responsibility.</li>
                                    <li class="mb-2"><strong>Transparency:</strong> We promote open communication and trust.</li>
                                    <li class="mb-0"><strong>Teamwork:</strong> We collaborate to achieve great results.</li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- PHILOSOPHY SECTION -->
            <section id="philosophy" class="mb-5">
                <div class="card shadow-sm border-0">
                    <div class="card-body p-4 p-md-5">
                        <div class="row g-4 align-items-center">
                            <div class="col-md-6" data-aos="fade-right">
                                <h3 class="display-6 fw-bold mb-3 d-flex align-items-center gap-3">
                                    <i class="fas fa-lightbulb text-blue-600"></i>
                                    Our Philosophies
                                </h3>
                                <p class="text-muted mb-3">
                                    Working in a team spirit towards a common vision, our commitment and dedication strive to satisfy client requirements in the most cost-effective manner.
                                </p>
                                <p class="mb-0">
                                    We follow a <strong class="text-blue-600">"customer comes first"</strong> philosophy and foster quality excellence, continuous improvement, and long-term partnerships.
                                </p>
                            </div>
                            <div class="col-md-6" data-aos="fade-left">
                                <div class="image-container shadow-lg">
                                    <img src="/Images/teamrrc.jpg" alt="Philosophy" class="img-fluid" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- CLIENTS SECTION -->
            <section id="clients" class="mb-5">
                <h3 class="section-header text-center display-6 fw-bold" data-aos="fade-up">
                    Our Trusted Clients
                </h3>
                <p class="text-center text-muted mb-4">
                    Partnering with leading organizations across industries
                </p>

                <div class="row g-3">
                    <!-- TESDA -->
                    <div class="col-6 col-sm-4 col-lg-2" data-aos="zoom-in">
                        <div class="client-card p-3 rounded shadow-sm">
                            <div class="client-logo-container">
                                <img src="/Images/tesda.png" alt="TESDA" class="client-logo">
                            </div>
                            <div class="client-name">
                                <p class="small fw-semibold mb-0">TESDA</p>
                            </div>
                        </div>
                    </div>

                    <!-- Petron -->
                    <div class="col-6 col-sm-4 col-lg-2" data-aos="zoom-in" data-aos-delay="80">
                        <div class="client-card p-3 rounded shadow-sm">
                            <div class="client-logo-container">
                                <img src="/Images/petron.png" alt="Petron" class="client-logo">
                            </div>
                            <div class="client-name">
                                <p class="small fw-semibold mb-0">Petron Corporation</p>
                            </div>
                        </div>
                    </div>

                    <!-- Brinks -->
                    <div class="col-6 col-sm-4 col-lg-2" data-aos="zoom-in" data-aos-delay="160">
                        <div class="client-card p-3 rounded shadow-sm">
                            <div class="client-logo-container">
                                <img src="/Images/brinks.png" alt="Brinks" class="client-logo">
                            </div>
                            <div class="client-name">
                                <p class="small fw-semibold mb-0">Brinks Philippines Inc.</p>
                            </div>
                        </div>
                    </div>

                    <!-- Biosolutions -->
                    <div class="col-6 col-sm-4 col-lg-2" data-aos="zoom-in" data-aos-delay="240">
                        <div class="client-card p-3 rounded shadow-sm">
                            <div class="client-logo-container">
                                <img src="/Images/biosolution.png" alt="Biosolutions" class="client-logo">
                            </div>
                            <div class="client-name">
                                <p class="small fw-semibold mb-0">Biosolutions International</p>
                            </div>
                        </div>
                    </div>

                    <!-- VCMC -->
                    <div class="col-6 col-sm-4 col-lg-2" data-aos="zoom-in" data-aos-delay="320">
                        <div class="client-card p-3 rounded shadow-sm">
                            <div class="client-logo-container">
                                <img src="/Images/vcmc.png" alt="VCMC" class="client-logo">
                            </div>
                            <div class="client-name">
                                <p class="small fw-semibold mb-0">Valenzuela Citicare Medical Center</p>
                            </div>
                        </div>
                    </div>

                    <!-- BOC -->
                    <div class="col-6 col-sm-4 col-lg-2" data-aos="zoom-in" data-aos-delay="400">
                        <div class="client-card p-3 rounded shadow-sm">
                            <div class="client-logo-container">
                                <img src="/Images/boc.png" alt="BOC" class="client-logo">
                            </div>
                            <div class="client-name">
                                <p class="small fw-semibold mb-0">Bureau of Customs</p>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- TEAM SECTION -->
            <section id="team" class="mb-5">
                <h3 class="section-header text-center display-6 fw-bold" data-aos="fade-up">
                    Our Dedicated Team
                </h3>
                <div class="row g-4">
                    <div class="col-6 col-md-3" data-aos="zoom-in">
                        <div class="project-card shadow-lg">
                            <img src="/Images/Sprayer.png" alt="Team Member 1" />
                        </div>
                    </div>
                    <div class="col-6 col-md-3" data-aos="zoom-in" data-aos-delay="100">
                        <div class="project-card shadow-lg">
                            <img src="/Images/team2.png" alt="Team Member 2" />
                        </div>
                    </div>
                    <div class="col-6 col-md-3" data-aos="zoom-in" data-aos-delay="200">
                        <div class="project-card shadow-lg">
                            <img src="/Images/team3.png" alt="Team Member 3" />
                        </div>
                    </div>
                    <div class="col-6 col-md-3" data-aos="zoom-in" data-aos-delay="300">
                        <div class="project-card shadow-lg">
                            <img src="/Images/teamapt.png" alt="Team Member 4" />
                        </div>
                    </div>
                </div>
            </section>

            <!-- COMPLETED PROJECTS SECTION -->
            <section id="completed" class="mb-5">
                <h3 class="section-header text-center display-6 fw-bold" data-aos="fade-up">
                    Completed Projects
                </h3>
                <div class="row g-4">
                    <div class="col-md-4" data-aos="zoom-in">
                        <div class="project-card shadow-lg">
                            <img src="/Images/completed1.jpg" alt="Completed Project 1">
                        </div>
                    </div>
                    <div class="col-md-4" data-aos="zoom-in" data-aos-delay="100">
                        <div class="project-card shadow-lg">
                            <img src="/Images/completed2.png" alt="Completed Project 2">
                        </div>
                    </div>
                    <div class="col-md-4" data-aos="zoom-in" data-aos-delay="200">
                        <div class="project-card shadow-lg">
                            <img src="/Images/completed3.png" alt="Completed Project 3">
                        </div>
                    </div>
                </div>
            </section>

            <!-- ONGOING PROJECTS SECTION -->
            <section id="ongoing" class="mb-5">
                <h3 class="section-header text-center display-6 fw-bold" data-aos="fade-up">
                    Ongoing Projects
                </h3>
                <div class="row g-4">
                    <div class="col-md-4" data-aos="zoom-in">
                        <div class="project-card shadow-lg">
                            <img src="/Images/ongoing1.png" alt="Ongoing Project 1">
                        </div>
                    </div>
                    <div class="col-md-4" data-aos="zoom-in" data-aos-delay="100">
                        <div class="project-card shadow-lg">
                            <img src="/Images/ongoing2.png" alt="Ongoing Project 2">
                        </div>
                    </div>
                    <div class="col-md-4" data-aos="zoom-in" data-aos-delay="200">
                        <div class="project-card shadow-lg">
                            <img src="/Images/ongoing3.png" alt="Ongoing Project 3">
                        </div>
                    </div>
                </div>
            </section>
        </div>

        <!-- BACK TO HOME BUTTON -->
        <section class="bg-gradient-custom py-5">
            <div class="container text-center" data-aos="fade-up">
                <h4 class="fs-3 fw-bold mb-3">Ready to Get Started?</h4>
                <a href="Default.aspx" class="btn-back-home">
                    <i class="fas fa-arrow-left"></i>
                    Back to Home
                </a>
            </div>
        </section>
    </div>

    <!-- Bootstrap Bundle JS (if not already in master page) -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- AOS JS -->
    <script src="https://unpkg.com/aos@2.3.4/dist/aos.js"></script>
    <script>
        // Initialize AOS
        document.addEventListener('DOMContentLoaded', function () {
            AOS.init({
                once: true,
                offset: 80,
                duration: 800,
                easing: 'ease-in-out',
                delay: 100
            });
        });
    </script>
</asp:Content>