<%@ Page Title="Services" Language="C#" MasterPageFile="~/Inquiry.Master" AutoEventWireup="true" CodeBehind="Services.aspx.cs" Inherits="RRCManagementSystem.Services" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageTitle" runat="server">
    Services - RRC Termite and Pest Control
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Services Page Specific Styles */
        .services-hero {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 120px 0 80px;
            color: white;
            text-align: center;
            position: relative;
            overflow: hidden;
        }

        .services-hero::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1440 320"><path fill="rgba(255,255,255,0.1)" d="M0,96L48,112C96,128,192,160,288,160C384,160,480,128,576,112C672,96,768,96,864,112C960,128,1056,160,1152,160C1248,160,1344,128,1392,112L1440,96L1440,320L1392,320C1344,320,1248,320,1152,320C1056,320,960,320,864,320C768,320,672,320,576,320C480,320,384,320,288,320C192,320,96,320,48,320L0,320Z"></path></svg>') bottom center no-repeat;
            background-size: cover;
        }

        .services-hero h1 {
            font-size: 3.5rem;
            font-weight: 700;
            margin-bottom: 20px;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
        }

        .services-hero p {
            font-size: 1.3rem;
            max-width: 700px;
            margin: 0 auto;
            opacity: 0.95;
        }

        /* Service Cards */
        .service-card {
            background: white;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
            transition: all 0.3s ease;
            height: 100%;
            display: flex;
            flex-direction: column;
        }

        .service-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 12px 40px rgba(0,0,0,0.15);
        }

        .service-icon {
            width: 100%;
            height: 250px;
            object-fit: cover;
            /* Placeholder while loading */
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
        }

        /* Add loading animation for lazy images */
        img[data-src] {
            opacity: 0;
            transition: opacity 0.3s ease-in;
        }

        img.lazy-loaded {
            opacity: 1;
        }

        .service-content {
            padding: 30px;
            flex-grow: 1;
            display: flex;
            flex-direction: column;
        }

        .service-title {
            font-size: 1.75rem;
            font-weight: 700;
            color: #2d3748;
            margin-bottom: 15px;
        }

        .service-description {
            color: #718096;
            line-height: 1.7;
            margin-bottom: 20px;
            flex-grow: 1;
        }

        .service-features {
            list-style: none;
            padding: 0;
            margin: 20px 0;
        }

        .service-features li {
            padding: 8px 0;
            color: #4a5568;
            position: relative;
            padding-left: 30px;
        }

        .service-features li::before {
            content: '✓';
            position: absolute;
            left: 0;
            color: #48bb78;
            font-weight: bold;
            font-size: 1.2rem;
        }

        .btn-learn-more {
            display: inline-block;
            padding: 12px 30px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            transition: all 0.3s ease;
            align-self: flex-start;
        }

        .btn-learn-more:hover {
            transform: translateX(5px);
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
            color: white;
        }

        /* CTA Section */
        .cta-section {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 80px 0;
            color: white;
            text-align: center;
            margin-top: 80px;
        }

        .cta-section h2 {
            font-size: 2.5rem;
            margin-bottom: 20px;
        }

        .cta-section p {
            font-size: 1.2rem;
            margin-bottom: 30px;
            opacity: 0.95;
        }

        .btn-cta {
            display: inline-block;
            padding: 15px 40px;
            background: white;
            color: #667eea;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 700;
            font-size: 1.1rem;
            transition: all 0.3s ease;
        }

        .btn-cta:hover {
            transform: scale(1.05);
            box-shadow: 0 8px 25px rgba(0,0,0,0.2);
            color: #764ba2;
        }

        /* Responsive */
        @media (max-width: 768px) {
            .services-hero h1 {
                font-size: 2.5rem;
            }

            .services-hero p {
                font-size: 1.1rem;
            }

            .service-icon {
                height: 200px;
            }

            .cta-section h2 {
                font-size: 2rem;
            }
        }

        /* Fade-in animation for lazy sections */
        .lazy-section {
            opacity: 0;
            transform: translateY(30px);
            transition: opacity 0.6s ease, transform 0.6s ease;
        }

        .lazy-section.loaded {
            opacity: 1;
            transform: translateY(0);
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Hero Section -->
    <section class="services-hero">
        <div class="container">
            <h1>Our Professional Services</h1>
            <p>Comprehensive termite and pest control solutions tailored to protect your property</p>
        </div>
    </section>

    <!-- Services Grid -->
    <section class="lazy-section" style="padding: 80px 0;">
        <div class="container">
            <div class="row g-4">
                <!-- Termite Baiting System -->
                <div class="col-lg-4 col-md-6">
                    <div class="service-card">
                        <img data-src="Images/baiting-system.jpg" 
                             alt="Termite Baiting System" 
                             class="service-icon"
                             src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 250'%3E%3Crect fill='%23f5f7fa' width='400' height='250'/%3E%3C/svg%3E">
                        <div class="service-content">
                            <h3 class="service-title">Termite Baiting System</h3>
                            <p class="service-description">
                                Advanced baiting technology that eliminates entire termite colonies. Our system uses strategically placed monitoring stations to detect and eliminate termites before they damage your property.
                            </p>
                            <ul class="service-features">
                                <li>Above-ground & in-ground options</li>
                                <li>Colony elimination technology</li>
                                <li>Continuous monitoring</li>
                                <li>Environmentally friendly</li>
                            </ul>
                            <a href="Inquiry.aspx" class="btn-learn-more">Get Started →</a>
                        </div>
                    </div>
                </div>

                <!-- Termite Prevention -->
                <div class="col-lg-4 col-md-6">
                    <div class="service-card">
                        <img data-src="Images/termite-prevention.jpg" 
                             alt="Termite Prevention" 
                             class="service-icon"
                             src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 250'%3E%3Crect fill='%23f5f7fa' width='400' height='250'/%3E%3C/svg%3E">
                        <div class="service-content">
                            <h3 class="service-title">Termite Prevention</h3>
                            <p class="service-description">
                                Proactive protection services including soil poisoning treatments for both pre-construction and post-construction properties. Create an impenetrable barrier against termite infestation.
                            </p>
                            <ul class="service-features">
                                <li>Pre-construction treatment</li>
                                <li>Post-construction protection</li>
                                <li>Long-lasting barriers</li>
                                <li>Annual inspections included</li>
                            </ul>
                            <a href="Inquiry.aspx" class="btn-learn-more">Get Started →</a>
                        </div>
                    </div>
                </div>

                <!-- Soil Poisoning -->
                <div class="col-lg-4 col-md-6">
                    <div class="service-card">
                        <img data-src="Images/soil-poisoning.jpg" 
                             alt="Soil Poisoning Treatment" 
                             class="service-icon"
                             src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 250'%3E%3Crect fill='%23f5f7fa' width='400' height='250'/%3E%3C/svg%3E">
                        <div class="service-content">
                            <h3 class="service-title">Soil Poisoning</h3>
                            <p class="service-description">
                                Comprehensive soil treatment that creates a chemical barrier around your property's foundation. Ideal for both new construction and existing structures requiring maximum protection.
                            </p>
                            <ul class="service-features">
                                <li>Foundation perimeter treatment</li>
                                <li>EPA-approved termiticides</li>
                                <li>Long-term protection</li>
                                <li>Warranty included</li>
                            </ul>
                            <a href="Inquiry.aspx" class="btn-learn-more">Get Started →</a>
                        </div>
                    </div>
                </div>

                <!-- Reticulation System -->
                <div class="col-lg-4 col-md-6">
                    <div class="service-card">
                        <img data-src="Images/reticulation.jpg" 
                             alt="Reticulation System" 
                             class="service-icon"
                             src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 250'%3E%3Crect fill='%23f5f7fa' width='400' height='250'/%3E%3C/svg%3E">
                        <div class="service-content">
                            <h3 class="service-title">Reticulation System</h3>
                            <p class="service-description">
                                Perforated pipe system installed beneath your property that allows for easy reapplication of termiticides without drilling. Perfect for long-term termite management with minimal disruption.
                            </p>
                            <ul class="service-features">
                                <li>Perforated pipe installation</li>
                                <li>Easy reapplication access</li>
                                <li>No drilling required after installation</li>
                                <li>Cost-effective maintenance</li>
                            </ul>
                            <a href="Inquiry.aspx" class="btn-learn-more">Get Started →</a>
                        </div>
                    </div>
                </div>

                <!-- Mound Demolition -->
                <div class="col-lg-4 col-md-6">
                    <div class="service-card">
                        <img data-src="Images/mound-demolition.jpg" 
                             alt="Mound Demolition" 
                             class="service-icon"
                             src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 250'%3E%3Crect fill='%23f5f7fa' width='400' height='250'/%3E%3C/svg%3E">
                        <div class="service-content">
                            <h3 class="service-title">Mound Demolition (Queen Finder)</h3>
                            <p class="service-description">
                                Specialized service to locate and eliminate termite queens within mounds. Our expert technicians use advanced techniques to ensure complete colony elimination at the source.
                            </p>
                            <ul class="service-features">
                                <li>Queen termite location & elimination</li>
                                <li>Complete mound treatment</li>
                                <li>Colony eradication</li>
                                <li>Follow-up inspection</li>
                            </ul>
                            <a href="Inquiry.aspx" class="btn-learn-more">Get Started →</a>
                        </div>
                    </div>
                </div>

                <!-- General Pest Control -->
                <div class="col-lg-4 col-md-6">
                    <div class="service-card">
                        <img data-src="Images/general-pest.jpg" 
                             alt="General Pest Control" 
                             class="service-icon"
                             src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 250'%3E%3Crect fill='%23f5f7fa' width='400' height='250'/%3E%3C/svg%3E">
                        <div class="service-content">
                            <h3 class="service-title">General Pest Control</h3>
                            <p class="service-description">
                                One-time comprehensive pest control service targeting cockroaches, ants, mosquitoes, flies, ticks, fleas, bedbugs, rats, and rodents. Keep your property pest-free and healthy.
                            </p>
                            <ul class="service-features">
                                <li>Cockroach & ant elimination</li>
                                <li>Flying insect control</li>
                                <li>Bedbug treatment</li>
                                <li>Rodent control & removal</li>
                            </ul>
                            <a href="Inquiry.aspx" class="btn-learn-more">Get Started →</a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- CTA Section -->
    <section class="cta-section lazy-section">
        <div class="container">
            <h2>Ready to Protect Your Property?</h2>
            <p>Contact us today for a free consultation and inspection</p>
            <a href="Inquiry.aspx" class="btn-cta">Schedule Free Inspection</a>
        </div>
    </section>
</asp:Content>