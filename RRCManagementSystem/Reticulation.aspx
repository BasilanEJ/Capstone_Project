<%@ Page Title="Reticulation" Language="C#" MasterPageFile="~/Inquiry.Master" AutoEventWireup="true" CodeBehind="Reticulation.aspx.cs" Inherits="RRCManagementSystem.Reticulation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Internal CSS -->
    <style>
        /* ======= Page Styling ======= */
        .reticulation-section {
            padding: 50px 0;
        }

        .reticulation-image {
            width: 100%;
            max-width: 550px;
            height: 350px;
            object-fit: cover;
            border-radius: 8px;
            display: block;
            margin: 0 auto;
            border: 2px solid #e0e0e0;
        }

        .reticulation-title {
            font-size: 2rem;
            font-weight: bold;
            color: #007bff;
            text-align: center;
            margin-top: 20px;
        }

        .reticulation-subtitle {
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
            .reticulation-title {
                font-size: 1.6rem;
            }

            .reticulation-image {
                height: 220px;
            }
        }
    </style>

    <section class="reticulation-section">
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
                                <img src="/images/service-reticulation.jpg" alt="Reticulation" class="reticulation-image" />
                            </div>

                            <!-- Title & Subtitle -->
                            <h2 class="reticulation-title">Reticulation</h2>
                            <p class="reticulation-subtitle">Perforated Pipe System for Efficient Termite Management</p>
                            <hr />

                            <!-- Description -->
                            <p>
                                Our reticulation system is a modern termite management solution that makes re-treatment simple and efficient.
                                A network of underground perforated pipes is installed around your property’s foundation,
                                allowing termiticide to be evenly distributed in the soil.
                            </p>
                            <p>
                                When it’s time for re-application, chemicals can be delivered directly into the system without drilling
                                or damaging floors. This provides a long-term, cost-effective solution for termite management
                                and ensures consistent soil coverage.
                            </p>

                            <!-- Key Benefits -->
                            <h4 class="mt-4 mb-3">Why Choose Our Reticulation System:</h4>
                            <ul class="list-group list-group-flush mb-4">
                                <li class="list-group-item">✅ Even, reliable distribution of termiticide</li>
                                <li class="list-group-item">✅ Easy re-application without drilling or disruption</li>
                                <li class="list-group-item">✅ Long-lasting and cost-effective protection</li>
                                <li class="list-group-item">✅ Discreet system that preserves your property’s appearance</li>
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
