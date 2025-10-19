<%@ Page Title="About Us" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="ClientAboutUs.aspx.cs" Inherits="RRCManagementSystem.ClientAboutUs" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- AOS (Animate On Scroll) -->
    <link href="https://unpkg.com/aos@2.3.4/dist/aos.css" rel="stylesheet" />
    <script src="https://unpkg.com/aos@2.3.4/dist/aos.js"></script>

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
        }

        .btn-back-home:hover {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(59, 130, 246, 0.4);
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

        /* Team Grid */
        .team-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 2rem;
        }

        @media (max-width: 640px) {
            .team-grid {
                grid-template-columns: repeat(2, 1fr);
                gap: 1rem;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-content">
        <!-- HERO SECTION -->
        <section class="hero-section py-16 md:py-24 text-gray-800 relative -mx-4 sm:-mx-6 lg:-mx-8 mb-8">
            <div class="container mx-auto px-6 md:px-8 text-center relative z-10">
                <div data-aos="fade-down" data-aos-duration="900">
                    <h1 class="text-4xl md:text-5xl lg:text-6xl font-extrabold text-blue-700 mb-4 leading-tight">
                        Take Command and Control of Your Pest Problem
                    </h1>
                    <p class="text-lg md:text-xl text-gray-700 mb-3 font-medium">
                        Your Partner in Safe and Reliable Pest Control
                    </p>
                    <p class="text-sm md:text-base text-gray-500 italic">
                        Please don't pet the pests.
                    </p>
                </div>
            </div>
        </section>

        <!-- ABOUT SECTION -->
        <section id="about" class="mb-16">
            <div class="bg-white rounded-xl shadow-md p-6 md:p-8">
                <div class="grid md:grid-cols-2 gap-8 md:gap-12 items-center">
                    <div data-aos="fade-right" data-aos-duration="800">
                        <h2 class="text-3xl md:text-4xl font-bold text-gray-800 mb-4 flex items-center gap-3">
                            <i class="fas fa-building text-blue-600"></i>
                            About RRC
                        </h2>
                        <h3 class="text-xl font-semibold text-blue-600 mb-4">Termite & Pest Control Services</h3>
                        <p class="text-gray-600 leading-relaxed mb-4">
                            It all began with a small capital in 2008. Through hard work, skilled staff, and continuous improvement,
                            RRC grew into a quality-focused pest control service that balances world-class methods with affordability.
                        </p>
                        <p class="text-gray-700 leading-relaxed mb-4">
                            <strong class="text-blue-600">RRC — Recovery, Resource, and Control</strong> — helps clients retrieve assets from pest damage,
                            control infestations, and prevent recurrences.
                        </p>
                        <p class="text-gray-600 leading-relaxed">
                            We pride ourselves on having the talent, experience, and tools necessary to provide effective, safe, and timely
                            pest management solutions for homes and businesses.
                        </p>
                    </div>

                    <div data-aos="fade-left" data-aos-duration="800">
                        <div class="image-container shadow-lg">
                            <img src="/Images/rrcteam.png" alt="RRC Team" class="w-full h-full object-cover">
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- MISSION / VISION / VALUES SECTION -->
        <section id="mv" class="mb-16">
            <h3 class="section-header text-3xl font-bold text-center text-gray-800" data-aos="fade-up">
                Mission • Vision • Values
            </h3>
            <div class="grid sm:grid-cols-1 md:grid-cols-3 gap-6">
                <!-- Mission -->
                <div data-aos="zoom-in" data-aos-delay="50" class="hover-card bg-white rounded-xl p-6 md:p-8 shadow-md border-t-4 border-blue-600">
                    <div class="flex items-center gap-3 mb-4">
                        <div class="icon-box">
                            <i class="fas fa-bullseye text-2xl text-blue-600"></i>
                        </div>
                        <h4 class="text-xl font-bold text-gray-800">Our Mission</h4>
                    </div>
                    <p class="text-gray-600 leading-relaxed">
                        To satisfy our clients by giving safe and effective treatments, creating jobs, and providing quality service at reasonable prices.
                    </p>
                </div>

                <!-- Vision -->
                <div data-aos="zoom-in" data-aos-delay="150" class="hover-card bg-white rounded-xl p-6 md:p-8 shadow-md border-t-4 border-indigo-600">
                    <div class="flex items-center gap-3 mb-4">
                        <div class="icon-box">
                            <i class="fas fa-eye text-2xl text-blue-600"></i>
                        </div>
                        <h4 class="text-xl font-bold text-gray-800">Our Vision</h4>
                    </div>
                    <p class="text-gray-600 leading-relaxed">
                        To be the leading pest control service in the country that values clients' needs by providing safe and effective treatment for peace of mind.
                    </p>
                </div>

                <!-- Values -->
                <div data-aos="zoom-in" data-aos-delay="250" class="hover-card bg-white rounded-xl p-6 md:p-8 shadow-md border-t-4 border-blue-500">
                    <div class="flex items-center gap-3 mb-4">
                        <div class="icon-box">
                            <i class="fas fa-heart text-2xl text-blue-600"></i>
                        </div>
                        <h4 class="text-xl font-bold text-gray-800">Our Values</h4>
                    </div>
                    <ul class="values-list text-gray-600 space-y-3 text-sm">
                        <li><strong>Integrity:</strong> We conduct business responsibly and ethically.</li>
                        <li><strong>Accountability:</strong> We honor commitments and accept responsibility.</li>
                        <li><strong>Transparency:</strong> We promote open communication and trust.</li>
                        <li><strong>Teamwork:</strong> We collaborate to achieve great results.</li>
                    </ul>
                </div>
            </div>
        </section>

        <!-- PHILOSOPHY SECTION -->
        <section id="philosophy" class="mb-16">
            <div class="bg-white rounded-xl shadow-md p-6 md:p-8">
                <div class="md:flex md:items-center md:gap-10">
                    <div class="md:w-1/2" data-aos="fade-right">
                        <h3 class="text-3xl font-bold mb-4 text-gray-800 flex items-center gap-3">
                            <i class="fas fa-lightbulb text-blue-600"></i>
                            Our Philosophies
                        </h3>
                        <p class="text-gray-600 leading-relaxed mb-4">
                            Working in a team spirit towards a common vision, our commitment and dedication strive to satisfy client requirements in the most cost-effective manner.
                        </p>
                        <p class="text-gray-700 leading-relaxed">
                            We follow a <strong class="text-blue-600">"customer comes first"</strong> philosophy and foster quality excellence, continuous improvement, and long-term partnerships.
                        </p>
                    </div>
                    <div class="md:w-1/2 mt-6 md:mt-0" data-aos="fade-left">
                        <div class="image-container shadow-lg">
                            <img src="/Images/teamrrc.jpg" alt="Philosophy" class="w-full h-80 object-cover" />
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- CLIENTS SECTION -->
        <section id="clients" class="mb-16">
            <h3 class="section-header text-3xl font-bold text-center text-gray-800" data-aos="fade-up">
                Our Trusted Clients
            </h3>
            <p class="text-center text-gray-600 mb-8">
                Partnering with leading organizations across industries
            </p>

            <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
                <!-- TESDA -->
                <div class="client-card p-4 rounded-xl shadow-md" data-aos="zoom-in">
                    <div class="client-logo-container">
                        <img src="/Images/tesda.png" alt="TESDA" class="client-logo">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">TESDA</p>
                    </div>
                </div>

                <!-- Petron -->
                <div class="client-card p-4 rounded-xl shadow-md" data-aos="zoom-in" data-aos-delay="80">
                    <div class="client-logo-container">
                        <img src="/Images/petron.png" alt="Petron" class="client-logo">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">Petron Corporation</p>
                    </div>
                </div>

                <!-- Brinks -->
                <div class="client-card p-4 rounded-xl shadow-md" data-aos="zoom-in" data-aos-delay="160">
                    <div class="client-logo-container">
                        <img src="/Images/brinks.png" alt="Brinks" class="client-logo">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">Brinks Philippines Inc.</p>
                    </div>
                </div>

                <!-- Biosolutions -->
                <div class="client-card p-4 rounded-xl shadow-md" data-aos="zoom-in" data-aos-delay="240">
                    <div class="client-logo-container">
                        <img src="/Images/biosolution.png" alt="Biosolutions" class="client-logo">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">Biosolutions International</p>
                    </div>
                </div>

                <!-- VCMC -->
                <div class="client-card p-4 rounded-xl shadow-md" data-aos="zoom-in" data-aos-delay="320">
                    <div class="client-logo-container">
                        <img src="/Images/vcmc.png" alt="VCMC" class="client-logo">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">Valenzuela Citicare Medical Center</p>
                    </div>
                </div>

                <!-- BOC -->
                <div class="client-card p-4 rounded-xl shadow-md" data-aos="zoom-in" data-aos-delay="400">
                    <div class="client-logo-container">
                        <img src="/Images/boc.png" alt="BOC" class="client-logo">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">Bureau of Customs</p>
                    </div>
                </div>
            </div>
        </section>

        <!-- TEAM SECTION -->
        <section id="team" class="mb-16">
            <h3 class="section-header text-3xl font-bold text-center text-gray-800" data-aos="fade-up">
                Our Dedicated Team
            </h3>
            <div class="team-grid">
                <div data-aos="zoom-in">
                    <div class="project-card shadow-lg">
                        <img src="/Images/Sprayer.png" alt="Team Member 1" class="w-full h-80 object-cover" />
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="100">
                    <div class="project-card shadow-lg">
                        <img src="/Images/team2.png" alt="Team Member 2" class="w-full h-80 object-cover" />
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="200">
                    <div class="project-card shadow-lg">
                        <img src="/Images/team3.png" alt="Team Member 3" class="w-full h-80 object-cover" />
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="300">
                    <div class="project-card shadow-lg">
                        <img src="/Images/teamapt.png" alt="Team Member 4" class="w-full h-80 object-cover" />
                    </div>
                </div>
            </div>
        </section>

        <!-- COMPLETED PROJECTS SECTION -->
        <section id="completed" class="mb-16">
            <h3 class="section-header text-3xl font-bold text-center text-gray-800" data-aos="fade-up">
                Completed Projects
            </h3>
            <div class="grid sm:grid-cols-1 md:grid-cols-3 gap-6">
                <div data-aos="zoom-in">
                    <div class="project-card shadow-lg">
                        <img src="/Images/completed1.jpg" alt="Completed Project 1" class="w-full h-80 object-cover">
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="100">
                    <div class="project-card shadow-lg">
                        <img src="/Images/completed2.png" alt="Completed Project 2" class="w-full h-80 object-cover">
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="200">
                    <div class="project-card shadow-lg">
                        <img src="/Images/completed3.png" alt="Completed Project 3" class="w-full h-80 object-cover">
                    </div>
                </div>
            </div>
        </section>

        <!-- ONGOING PROJECTS SECTION -->
        <section id="ongoing" class="mb-16">
            <h3 class="section-header text-3xl font-bold text-center text-gray-800" data-aos="fade-up">
                Ongoing Projects
            </h3>
            <div class="grid sm:grid-cols-1 md:grid-cols-3 gap-6">
                <div data-aos="zoom-in">
                    <div class="project-card shadow-lg">
                        <img src="/Images/ongoing1.png" alt="Ongoing Project 1" class="w-full h-80 object-cover">
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="100">
                    <div class="project-card shadow-lg">
                        <img src="/Images/ongoing2.png" alt="Ongoing Project 2" class="w-full h-80 object-cover">
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="200">
                    <div class="project-card shadow-lg">
                        <img src="/Images/ongoing3.png" alt="Ongoing Project 3" class="w-full h-80 object-cover">
                    </div>
                </div>
            </div>
        </section>

        <!-- BACK TO HOME BUTTON -->
        <section class="text-center py-12 bg-gradient-to-r from-gray-50 to-blue-50 rounded-xl shadow-inner -mx-4 sm:-mx-6 lg:-mx-8 mt-16">
            <div data-aos="fade-up">
                <h4 class="text-2xl font-bold text-gray-800 mb-4">Ready to Get Started?</h4>
                <p class="text-gray-600 mb-6">Return to home to explore our services and book your appointment</p>
                <a href="Home.aspx" class="btn-back-home">
                    <i class="fas fa-arrow-left"></i>
                    Back to Home
                </a>
            </div>
        </section>
    </div>

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