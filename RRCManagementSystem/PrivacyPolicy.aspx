<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="PrivacyPolicy.aspx.cs" Inherits="RRCManagementSystem.PrivacyPolicy" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .policy-container {
            max-width: 900px;
            margin: 0 auto;
            background: #fff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }
        .policy-container h2 {
            text-align: center;
            margin-bottom: 20px;
            color: #0d6efd;
        }
        .policy-container h4 {
            margin-top: 20px;
            color: #004085;
        }
        .policy-container p, 
        .policy-container li {
            font-size: 15px;
            line-height: 1.6;
            color: #333;
        }
        .policy-container ul {
            padding-left: 20px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="policy-container">
        <h2>Privacy Policy</h2>
        <p><strong>Last Updated:</strong> August 19, 2025</p>

        <p>RRC Termite & Pest Control respects your privacy and is committed to protecting your personal data in compliance with the <strong>Data Privacy Act of 2012 (Republic Act No. 10173)</strong> of the Philippines. This Privacy Policy explains how we collect, use, store, and protect your information when you use our inquiry page.</p>

        <h4>1. Collection of Personal Data</h4>
        <p>When you use the RRC Inquiry page, we may collect personal data such as:</p>
        <ul>
            <li>Name</li>
            <li>Contact number</li>
            <li>Email address</li>
            <li>Details of your inquiry, bookings, bills, and schedules</li>
        </ul>

        <h4>2. Purpose of Data Collection</h4>
        <ul>
            <li>To respond to your inquiries</li>
            <li>To allow you to view your bookings, bills, and schedules</li>
            <li>To improve our services and customer experience</li>
            <li>To comply with legal and regulatory requirements</li>
        </ul>

        <h4>3. Data Sharing and Disclosure</h4>
        <p>We do not sell, trade, or otherwise transfer your personal data to third parties without your consent, except when required by law, regulation, or competent authority.</p>

        <h4>4. Data Retention</h4>
        <p>We will retain your personal data only for as long as necessary to fulfill the purposes stated above, and as required by applicable laws and regulations.</p>

        <h4>5. Data Security</h4>
        <p>We implement appropriate organizational, physical, and technical measures to protect your personal data from unauthorized access, alteration, disclosure, or destruction.</p>

        <h4>6. User Rights Under the DPA 2012</h4>
        <ul>
            <li>The right to be informed</li>
            <li>The right to access</li>
            <li>The right to rectification</li>
            <li>The right to object (in certain cases)</li>
            <li>The right to erasure/blocking when no longer necessary</li>
            <li>The right to data portability</li>
            <li>The right to lodge a complaint with the NPC</li>
        </ul>

        <h4>7. Updates to this Privacy Policy</h4>
        <p>We may update this Privacy Policy from time to time to reflect changes in laws, technology, or business practices. Any updates will be posted on this page with a new effective date.</p>

        <h4>8. Contact Information</h4>
        <p>
            Email: <a href="mailto:rrctermiteandpestcontrol@gmail.com">rrctermiteandpestcontrol@gmail.com</a><br />
            Address: #33 Kaligatasan Street, Brgy. Holy Spirit, Quezon City, Philippines
        </p>
    </div>
</asp:Content>