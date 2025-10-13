<%@ Page Title="About Us" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="ClientAboutUs.aspx.cs" Inherits="RRCManagementSystem.ClientAboutUs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Tailwind CDN + AOS (Animate On Scroll) -->
    <script src="https://cdn.tailwindcss.com"></script>
    <link href="https://unpkg.com/aos@2.3.4/dist/aos.css" rel="stylesheet" />
    <script src="https://unpkg.com/aos@2.3.4/dist/aos.js"></script>

    <script>
        // Tailwind config for RRC colors
        tailwind.config = {
            theme: {
                extend: {
                    colors: {
                        rrcblue: {
                            DEFAULT: '#60A5FA',
                            50: '#F3F9FF',
                            100: '#E8F3FF',
                            200: '#CFE8FF',
                            300: '#A7D3FF',
                            700: '#1E40AF'
                        }
                    },
                    boxShadow: {
                        'card-lg': '0 10px 30px rgba(20, 23, 34, 0.08)'
                    }
                }
            }
        };
    </script>

    <style>
        /* Remove hero image background */
        .hero-section {
            background-color: #F9FBFF;
            background-image: linear-gradient(180deg, #E8F3FF 0%, #FFFFFF 100%);
        }

        /* Client logos animation */
        .client-logo {
            margin-bottom: 5px;
            filter: none !important;
            opacity: 1 !important;
            transition: transform 0.3s ease, filter 0.3s ease;
        }

        /* Optional: small hover zoom effect */
        .client-logo:hover {
            transform: scale(1.05);
        }

        /* Uniform client card sizing */
        .client-card {
            height: 160px;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
        }

        .client-logo-container {
            height: 80px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 12px;
        }

        .client-name {
            height: 40px;
            display: flex;
            align-items: center;
            justify-content: center;
            text-align: center;
        }
    </style>

    <!-- HERO SECTION -->
    <section class="hero-section py-20 md:py-28 text-gray-800 relative overflow-hidden">
        <div class="container mx-auto px-6 md:px-8 text-center">
            <div data-aos="fade-down" data-aos-duration="900">
                <h1 class="text-4xl md:text-6xl font-extrabold text-rrcblue-700 mb-4 leading-tight">
                    Take Command and Control of Your Pest Problem
                </h1>
                <p class="text-lg md:text-2xl text-gray-700 mb-3">
                    Your Partner in Safe and Reliable Pest Control.
                </p>
                <p class="text-sm md:text-base text-gray-500 italic">
                    Please don't pet the pests.
                </p>
            </div>
        </div>
        <div class="absolute inset-0 -z-10 opacity-10 bg-gradient-to-r from-rrcblue-100 via-white to-rrcblue-50"></div>
    </section>

    <!-- ABOUT -->
    <section id="about" class="bg-white py-20">
        <div class="container mx-auto px-6 md:px-8">
            <div class="grid md:grid-cols-2 gap-10 items-center">
                <div data-aos="fade-right" data-aos-duration="800">
                    <h2 class="text-3xl md:text-4xl font-bold text-gray-800 mb-4">About RRC Termite & Pest Control Services</h2>
                    <p class="text-gray-600 leading-relaxed mb-4">
                        It all began with a small capital in 2008. Through hard work, skilled staff, and continuous improvement,
                        RRC grew into a quality-focused pest control service that balances world-class methods with affordability.
                        <strong>RRC — Recovery, Resource, and Control</strong> — helps clients retrieve assets from pest damage,
                        control infestations, and prevent recurrences.
                    </p>
                    <p class="text-gray-600 leading-relaxed">
                        We pride ourselves on having the talent, experience, and tools necessary to provide effective, safe, and timely
                        pest management solutions for homes and businesses.
                    </p>
                </div>

                <div data-aos="fade-left" data-aos-duration="800">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:scale-105 transition-transform duration-500">
                        <img src="/Images/rrcteam.png" alt="RRC Team" class="w-full h-100 object-cover">
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- MISSION / VISION / VALUES -->
    <section id="mv" class="bg-rrcblue-50 py-20">
        <div class="container mx-auto px-6 md:px-8">
            <h3 class="text-center text-3xl font-bold text-gray-800 mb-12" data-aos="fade-up">Mission • Vision • Values</h3>
            <div class="grid sm:grid-cols-1 md:grid-cols-3 gap-8">
                <!-- Mission -->
                <div data-aos="zoom-in" data-aos-delay="50" class="bg-white rounded-2xl p-8 shadow hover:shadow-xl transform hover:-translate-y-2 transition">
                    <div class="flex items-center gap-3 mb-4">
                        <div class="p-3 bg-rrcblue-100 rounded-lg">
                            <svg class="w-7 h-7 text-rrcblue-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12l2 2 4-4"></path>
                            </svg>
                        </div>
                        <h4 class="text-lg font-semibold">Our Mission</h4>
                    </div>
                    <p class="text-gray-600">
                        To satisfy our clients by giving safe and effective treatments, creating jobs, and providing quality service at reasonable prices.
                    </p>
                </div>

                <!-- Vision -->
                <div data-aos="zoom-in" data-aos-delay="150" class="bg-white rounded-2xl p-8 shadow hover:shadow-xl transform hover:-translate-y-2 transition">
                    <div class="flex items-center gap-3 mb-4">
                        <div class="p-3 bg-rrcblue-100 rounded-lg">
                            <svg class="w-7 h-7 text-rrcblue-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M12 2l3 7h7l-5.5 4.2L20 22l-8-5-8 5 1.5-8.8L0 9h7l3-7z"></path>
                            </svg>
                        </div>
                        <h4 class="text-lg font-semibold">Our Vision</h4>
                    </div>
                    <p class="text-gray-600">
                        To be the leading pest control service in the country that values clients' needs by providing safe and effective treatment for peace of mind.
                    </p>
                </div>

                <!-- Values -->
                <div data-aos="zoom-in" data-aos-delay="250" class="bg-white rounded-2xl p-8 shadow hover:shadow-xl transform hover:-translate-y-2 transition">
                    <div class="flex items-center gap-3 mb-4">
                        <div class="p-3 bg-rrcblue-100 rounded-lg">
                            <svg class="w-7 h-7 text-rrcblue-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M12 8c1.657 0 3-1.343 3-3S13.657 2 12 2 9 3.343 9 5s1.343 3 3 3zM4 20v-2a4 4 0 014-4h8a4 4 0 014 4v2"></path>
                            </svg>
                        </div>
                        <h4 class="text-lg font-semibold">Our Values</h4>
                    </div>
                    <ul class="text-gray-600 space-y-2">
                        <li><strong>Integrity:</strong> We conduct business responsibly and ethically.</li>
                        <li><strong>Accountability:</strong> We honor commitments and accept responsibility.</li>
                        <li><strong>Transparency:</strong> We promote open communication and trust.</li>
                        <li><strong>Teamwork:</strong> We collaborate to achieve great results.</li>
                    </ul>
                </div>
            </div>
        </div>
    </section>

    <!-- PHILOSOPHY -->
    <section id="philosophy" class="bg-white py-20">
        <div class="container mx-auto px-6 md:px-8">
            <div class="md:flex md:items-center md:gap-10">
                <div class="md:w-1/2" data-aos="fade-right">
                    <h3 class="text-3xl font-bold mb-4 text-gray-800">Our Philosophies</h3>
                    <p class="text-gray-600 leading-relaxed">
                        Working in a team spirit towards a common vision, our commitment and dedication strive to satisfy client requirements in the most cost-effective manner.
                        We follow a <strong>"customer comes first"</strong> philosophy and foster quality excellence, continuous improvement, and long-term partnerships.
                    </p>
                </div>
                <div class="md:w-1/2 mt-6 md:mt-0" data-aos="fade-left">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:scale-105 transition-transform duration-500">
                        <img src="/Images/teamrrc.jpg" alt="Philosophy" class="w-full h-64 object-cover" />
                    </div>
                </div>
            </div>
        </div>
    </section>

    <section id="clients" class="bg-rrcblue-50 py-20">
        <div class="container mx-auto px-6 md:px-8">
            <h3 class="text-3xl font-bold text-center text-gray-800" data-aos="fade-up">
                Our Clients
            </h3>
            <p class="text-center text-gray-600 mt-2 mb-12">
                Trusted by organizations across industries.
            </p>

            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-6 items-stretch">
                <!-- TESDA -->
                <div class="client-card p-4 bg-white rounded-xl shadow hover:shadow-lg transition" data-aos="zoom-in">
                    <div class="client-logo-container">
                        <img src="/Images/tesda.png" alt="TESDA" class="client-logo max-h-16 object-contain">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">TESDA</p>
                    </div>
                </div>

                <!-- Petron -->
                <div class="client-card p-4 bg-white rounded-xl shadow hover:shadow-lg transition" data-aos="zoom-in" data-aos-delay="80">
                    <div class="client-logo-container">
                        <img src="/Images/petron.png" alt="Petron" class="client-logo max-h-16 object-contain">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">Petron Corporation</p>
                    </div>
                </div>

                <!-- Brinks -->
                <div class="client-card p-4 bg-white rounded-xl shadow hover:shadow-lg transition" data-aos="zoom-in" data-aos-delay="160">
                    <div class="client-logo-container">
                        <img src="/Images/brinks.png" alt="Brinks" class="client-logo max-h-16 object-contain">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">Brinks Philippines Inc.</p>
                    </div>
                </div>

                <!-- Biosolutions -->
                <div class="client-card p-4 bg-white rounded-xl shadow hover:shadow-lg transition" data-aos="zoom-in" data-aos-delay="240">
                    <div class="client-logo-container">
                        <img src="/Images/biosolution.png" alt="Biosolutions" class="client-logo max-h-16 object-contain">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">Biosolutions International Corp.</p>
                    </div>
                </div>

                <!-- VCMC -->
                <div class="client-card p-4 bg-white rounded-xl shadow hover:shadow-lg transition" data-aos="zoom-in" data-aos-delay="320">
                    <div class="client-logo-container">
                        <img src="/Images/vcmc.png" alt="VCMC" class="client-logo max-h-16 object-contain">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">Valenzuela Citicare Medical Center</p>
                    </div>
                </div>

                <!-- BOC -->
                <div class="client-card p-4 bg-white rounded-xl shadow hover:shadow-lg transition" data-aos="zoom-in" data-aos-delay="400">
                    <div class="client-logo-container">
                        <img src="/Images/boc.png" alt="BOC" class="client-logo max-h-16 object-contain">
                    </div>
                    <div class="client-name">
                        <p class="text-sm font-semibold text-gray-700">Bureau of Customs</p>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <section id="team" class="bg-white py-20">
        <div class="container mx-auto px-6 md:px-8">
            <h3 class="text-3xl font-bold text-center text-gray-800 mb-12" data-aos="fade-up">Our Dedicated Team</h3>
            <div class="grid sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                <div data-aos="zoom-in">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:scale-105 transition-transform duration-500">
                        <img src="/Images/Sprayer.png" alt="Team 1" class="w-full h-80 object-cover" />
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="100">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:scale-105 transition-transform duration-500">
                        <img src="/Images/team2.png" alt="Team 2" class="w-full h-80 object-cover" />
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="200">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:scale-105 transition-transform duration-500">
                        <img src="/Images/team3.png" alt="Team 3" class="w-full h-80 object-cover" />
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="300">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:scale-105 transition-transform duration-500">
                        <img src="/Images/teamapt.png" alt="Team 4" class="w-full h-80 object-cover" />
                    </div>
                </div>
            </div>
        </div>
    </section>

    <section id="completed" class="bg-white py-20">
        <div class="container mx-auto px-6 md:px-8 text-center">
            <h3 class="text-3xl font-bold text-gray-800 mb-12" data-aos="fade-up">Completed Projects</h3>
            <div class="grid sm:grid-cols-1 md:grid-cols-3 gap-8">
                <div data-aos="zoom-in">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:shadow-2xl transition-transform duration-500 hover:-translate-y-2">
                        <img src="/Images/completed1.jpg" alt="Taytay Rizal" class="w-full h-80 object-cover">
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="100">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:shadow-2xl transition-transform duration-500 hover:-translate-y-2">
                        <img src="/Images/completed2.png" alt="Completed Project 2" class="w-full h-80 object-cover">
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="200">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:shadow-2xl transition-transform duration-500 hover:-translate-y-2">
                        <img src="/Images/completed3.png" alt="Completed Project 3" class="w-full h-80 object-cover">
                    </div>
                </div>
            </div>
        </div>
    </section>

    <section id="ongoing" class="bg-rrcblue-50 py-20">
        <div class="container mx-auto px-6 md:px-8 text-center">
            <h3 class="text-3xl font-bold text-gray-800 mb-12" data-aos="fade-up">Ongoing Projects</h3>
            <div class="grid sm:grid-cols-1 md:grid-cols-3 gap-8">
                <div data-aos="zoom-in">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:shadow-2xl transition-transform duration-500 hover:-translate-y-2">
                        <img src="/Images/ongoing1.png" alt="Punta Fuego" class="w-full h-80 object-cover">
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="100">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:shadow-2xl transition-transform duration-500 hover:-translate-y-2">
                        <img src="/Images/ongoing2.png" alt="Ongoing Project 2" class="w-full h-80 object-cover">
                    </div>
                </div>
                <div data-aos="zoom-in" data-aos-delay="200">
                    <div class="rounded-2xl overflow-hidden shadow-card-lg hover:shadow-2xl transition-transform duration-500 hover:-translate-y-2">
                        <img src="/Images/ongoing3.png" alt="Ongoing Project 3" class="w-full h-80 object-cover">
                    </div>
                </div>
            </div>
        </div>
    </section>


    <script>
        // Initialize AOS
        AOS.init({
            once: true,
            offset: 100,
            duration: 800,
            easing: 'ease-in-out'
        });
    </script>
</asp:Content>