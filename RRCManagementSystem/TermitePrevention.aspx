<%@ Page Title="Termite Prevention" Language="C#" MasterPageFile="~/Inquiry.Master" AutoEventWireup="true" CodeBehind="TermitePrevention.aspx.cs" Inherits="RRCManagementSystem.TermitePrevention" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Internal CSS -->
    <style>
        /* ======= Page Styling ======= */
        .prevention-section {
            padding: 50px 0;
        }

        .prevention-image {
            width: 100%;
            max-width: 550px;
            height: 280px;
            object-fit: cover;
            border-radius: 8px;
            display: block;
            margin: 0 auto;
            border: 2px solid #e0e0e0;
        }

        .prevention-title {
            font-size: 2rem;
            font-weight: bold;
            color: #007bff;
            text-align: center;
            margin-top: 20px;
        }

        .prevention-subtitle {
            text-align: center;
            color: #6c757d;
            margin-bottom: 20px;
        }

        .card {
            border-radius: 10px;
            border: 1px solid #ddd;
        }

        .card-body {
            padding: 30px;
        }

        .list-group-item {
            font-size: 1rem;
            padding: 12px 15px;
        }

        /* Back Button Styling */
        .btn-back {
            background-color: #6c757d;
            color: white;
            font-weight: 500;
            padding: 8px 18px;
            border-radius: 6px;
            transition: background 0.3s ease;
            text-decoration: none;
        }

        .btn-back:hover {
            background-color: #5a6268;
            color: white;
        }

        /* Top Back Button */
        .top-back {
            margin-bottom: 25px;
        }

        /* Responsive Adjustments */
        @media (max-width: 768px) {
            .prevention-title {
                font-size: 1.6rem;
            }

            .prevention-image {
                height: 220px;
            }
        }
    </style>

    <section class="prevention-section">
        <div class="container">

            <!-- Back Button (Top) -->
            <div class="top-back">
                <a href="Default.aspx#servicesCarousel" class="btn-back">
                    &#8592; Back to Services
                </a>
            </div>

            <div class="row justify-content-center">
                <div class="col-lg-8">
                    <div class="card shadow-sm">
                        <div class="card-body">

                            <!-- Main Image -->
                            <div class="text-center mb-4">
                                <img src="/images/service-termite-prevention.jpg" alt="Termite Prevention" class="prevention-image" />
                            </div>

                            <!-- Title & Subtitle -->
                            <h2 class="prevention-title">Termite Prevention</h2>
                            <p class="prevention-subtitle">Smart Protection Against Termite Infestation</p>
                            <hr />

                            <!-- Description -->
                            <p>
                                Prevention is the smartest and most cost-effective way to handle termites before they cause damage. 
                                Our preventive services create a protective barrier that blocks termites from ever reaching your property.
                            </p>
                            <p>
                                Using a mix of physical barriers, liquid termiticides, and advanced technology, 
                                we provide coverage for both new and existing structures. 
                                Regular inspections allow us to identify risks early and reinforce your defenses.
                            </p>
                            <p>
                                This proactive approach protects your property value and spares you from expensive repairs in the future.
                            </p>

                            <!-- Key Benefits -->
                            <h4 class="mt-4 mb-3">Why Choose Our Termite Prevention Services:</h4>
                            <ul class="list-group list-group-flush mb-4">
                                <li class="list-group-item">✅ Stops infestations before they begin</li>
                                <li class="list-group-item">✅ Long-term barrier protection for homes and businesses</li>
                                <li class="list-group-item">✅ Reduces costly repair risks by preventing structural damage</li>
                                <li class="list-group-item">✅ Ideal for both residential and commercial properties</li>
                            </ul>

                            <!-- Back Button (Bottom) -->
                            <div class="text-center">
                                <a href="Default.aspx#servicesCarousel" class="btn-back">
                                    &#8592; Back to Services
                                </a>
                            </div>

                        </div>
                    </div>
                </div>
            </div>

        </div>
    </section>

</asp:Content>
