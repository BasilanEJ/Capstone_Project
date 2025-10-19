<%@ Page Title="Privacy Policy" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="PrivacyPolicy.aspx.cs" Inherits="RRCManagementSystem.PrivacyPolicy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .policy-container {
            max-width: 900px;
            margin: 0 auto;
        }

        .policy-card {
            background: white;
            padding: 2rem;
            border-radius: 1rem;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07);
        }

        @media (min-width: 640px) {
            .policy-card {
                padding: 2.5rem;
            }
        }

        @media (min-width: 768px) {
            .policy-card {
                padding: 3rem;
            }
        }

        .policy-header {
            text-align: center;
            padding-bottom: 2rem;
            border-bottom: 2px solid #e5e7eb;
            margin-bottom: 2rem;
        }

        .policy-title {
            font-size: 2rem;
            font-weight: 700;
            color: #1f2937;
            margin-bottom: 0.75rem;
        }

        @media (min-width: 768px) {
            .policy-title {
                font-size: 2.5rem;
            }
        }

        .policy-subtitle {
            color: #6b7280;
            font-size: 0.875rem;
        }

        .policy-section {
            margin-bottom: 2rem;
        }

        .section-title {
            font-size: 1.25rem;
            font-weight: 600;
            color: #1e40af;
            margin-bottom: 1rem;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .section-number {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 2rem;
            height: 2rem;
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            border-radius: 0.5rem;
            font-size: 0.875rem;
            font-weight: 700;
            flex-shrink: 0;
        }

        .policy-text {
            font-size: 0.9375rem;
            line-height: 1.75;
            color: #4b5563;
            margin-bottom: 1rem;
        }

        .policy-list {
            list-style: none;
            padding-left: 0;
            margin-bottom: 1rem;
        }

        .policy-list li {
            position: relative;
            padding-left: 2rem;
            margin-bottom: 0.75rem;
            font-size: 0.9375rem;
            line-height: 1.75;
            color: #4b5563;
        }

        .policy-list li::before {
            content: '✓';
            position: absolute;
            left: 0.5rem;
            color: #3b82f6;
            font-weight: 700;
            font-size: 1.125rem;
        }

        .highlight-box {
            background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
            border-left: 4px solid #3b82f6;
            padding: 1.25rem;
            border-radius: 0.5rem;
            margin-bottom: 1.5rem;
        }

        .highlight-box p {
            margin: 0;
            color: #1e40af;
            font-weight: 500;
        }

        .contact-box {
            background: #f9fafb;
            border: 2px solid #e5e7eb;
            border-radius: 0.75rem;
            padding: 1.5rem;
            margin-top: 1.5rem;
        }

        .contact-box p {
            margin-bottom: 0.5rem;
            color: #4b5563;
        }

        .contact-box a {
            color: #3b82f6;
            text-decoration: none;
            font-weight: 500;
            transition: color 0.3s ease;
        }

        .contact-box a:hover {
            color: #2563eb;
            text-decoration: underline;
        }

        .btn-back-home {
            display: inline-flex;
            align-items: center;
            gap: 0.75rem;
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            padding: 0.875rem 2rem;
            border-radius: 0.75rem;
            font-weight: 600;
            text-decoration: none;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
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

        .back-section {
            text-align: center;
            padding: 2rem;
            background: linear-gradient(135deg, #f9fafb 0%, #eff6ff 100%);
            border-radius: 1rem;
            margin-top: 2rem;
        }

        .back-section h4 {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1f2937;
            margin-bottom: 0.75rem;
        }

        .back-section p {
            color: #6b7280;
            margin-bottom: 1.5rem;
        }

        .dpa-badge {
            display: inline-block;
            background: #1e40af;
            color: white;
            padding: 0.25rem 0.75rem;
            border-radius: 0.375rem;
            font-size: 0.75rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="policy-container">

        <div class="mb-6">
            <h1 class="text-3xl font-bold text-gray-800 flex items-center gap-3">
                <i class="fas fa-shield-alt text-blue-600"></i>
                Privacy Policy
            </h1>
            <p class="text-gray-600 mt-2">How we protect and handle your personal information</p>
        </div>

        <div class="policy-card">

            <div class="policy-header">
                <h2 class="policy-title">Privacy Policy</h2>
                <div class="policy-subtitle">
                    <strong>Last Updated:</strong> August 19, 2025
                </div>
                <div class="mt-3">
                    <span class="dpa-badge">DPA 2012 Compliant</span>
                </div>
            </div>


            <div class="highlight-box">
                <p>
                    <i class="fas fa-info-circle mr-2"></i>
                    RRC Termite & Pest Control respects your privacy and is committed to protecting your personal data in compliance with the <strong>Data Privacy Act of 2012 (Republic Act No. 10173)</strong> of the Philippines.
                </p>
            </div>


            <div class="policy-section">
                <h4 class="section-title">
                    <span class="section-number">1</span>
                    Collection of Personal Data
                </h4>
                <p class="policy-text">When you use the RRC Inquiry page, we may collect personal data such as:</p>
                <ul class="policy-list">
                    <li>Name</li>
                    <li>Contact number</li>
                    <li>Email address</li>
                    <li>Details of your inquiry, bookings, bills, and schedules</li>
                </ul>
            </div>


            <div class="policy-section">
                <h4 class="section-title">
                    <span class="section-number">2</span>
                    Purpose of Data Collection
                </h4>
                <ul class="policy-list">
                    <li>To respond to your inquiries</li>
                    <li>To allow you to view your bookings, bills, and schedules</li>
                    <li>To improve our services and customer experience</li>
                    <li>To comply with legal and regulatory requirements</li>
                </ul>
            </div>


            <div class="policy-section">
                <h4 class="section-title">
                    <span class="section-number">3</span>
                    Data Sharing and Disclosure
                </h4>
                <p class="policy-text">
                    We do not sell, trade, or otherwise transfer your personal data to third parties without your consent, except when required by law, regulation, or competent authority.
                </p>
            </div>


            <div class="policy-section">
                <h4 class="section-title">
                    <span class="section-number">4</span>
                    Data Retention
                </h4>
                <p class="policy-text">
                    We will retain your personal data only for as long as necessary to fulfill the purposes stated above, and as required by applicable laws and regulations.
                </p>
            </div>


            <div class="policy-section">
                <h4 class="section-title">
                    <span class="section-number">5</span>
                    Data Security
                </h4>
                <p class="policy-text">
                    We implement appropriate organizational, physical, and technical measures to protect your personal data from unauthorized access, alteration, disclosure, or destruction.
                </p>
            </div>

          
            <div class="policy-section">
                <h4 class="section-title">
                    <span class="section-number">6</span>
                    User Rights Under the DPA 2012
                </h4>
                <ul class="policy-list">
                    <li>The right to be informed</li>
                    <li>The right to access</li>
                    <li>The right to rectification</li>
                    <li>The right to object (in certain cases)</li>
                    <li>The right to erasure/blocking when no longer necessary</li>
                    <li>The right to data portability</li>
                    <li>The right to lodge a complaint with the NPC</li>
                </ul>
            </div>

        
            <div class="policy-section">
                <h4 class="section-title">
                    <span class="section-number">7</span>
                    Updates to this Privacy Policy
                </h4>
                <p class="policy-text">
                    We may update this Privacy Policy from time to time to reflect changes in laws, technology, or business practices. Any updates will be posted on this page with a new effective date.
                </p>
            </div>

            
            <div class="policy-section">
                <h4 class="section-title">
                    <span class="section-number">8</span>
                    Contact Information
                </h4>
                <div class="contact-box">
                    <p class="mb-3">
                        <i class="fas fa-question-circle text-blue-600 mr-2"></i>
                        <strong>Have questions about this Privacy Policy?</strong>
                    </p>
                    <p>
                        <i class="fas fa-envelope text-gray-500 mr-2"></i>
                        <strong>Email:</strong> <a href="mailto:rrctermiteandpestcontrol@gmail.com">rrctermiteandpestcontrol@gmail.com</a>
                    </p>
                    <p class="mb-0">
                        <i class="fas fa-map-marker-alt text-gray-500 mr-2"></i>
                        <strong>Address:</strong> #33 Kaligatasan Street, Brgy. Holy Spirit, Quezon City, Philippines
                    </p>
                </div>
            </div>
        </div>


        <div class="back-section">
            <h4>Ready to Experience Our Services?</h4>
            <p>Return to home to explore our pest control solutions and book your appointment</p>
            <a href="Home.aspx" class="btn-back-home">
                <i class="fas fa-arrow-left"></i>
                Back to Home
            </a>
        </div>
    </div>
</asp:Content>