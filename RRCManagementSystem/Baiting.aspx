<%@ Page Title="Baiting System" Language="C#" MasterPageFile="~/Inquiry.Master" AutoEventWireup="true" CodeBehind="Baiting.aspx.cs" Inherits="RRCManagementSystem.Baiting" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Internal CSS -->
    <style>
        /* ======= Page Styling ======= */
        .baiting-section {
            padding: 50px 0;
        }

        .baiting-image {
            width: 100%;
            max-width: 550px;
            height: 350px;
            object-fit: cover;
            border-radius: 8px;
            display: block;
            margin: 0 auto;
            border: 2px solid #e0e0e0;
        }

        .baiting-title {
            font-size: 2rem;
            font-weight: bold;
            color: #007bff;
            text-align: center;
            margin-top: 20px;
        }

        .baiting-subtitle {
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
            .baiting-title {
                font-size: 1.6rem;
            }

            .baiting-image {
                height: 220px;
            }
        }
    </style>

    <section class="baiting-section">
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
                                <img src="/images/service-baiting.jpg" alt="Baiting System" class="baiting-image" />
                            </div>

                            <!-- Title & Subtitle -->
                            <h2 class="baiting-title">Baiting System</h2>
                            <p class="baiting-subtitle">Above Ground &amp; In-Ground for Colony Elimination</p>
                            <hr />

                            <!-- Description -->
                            <p>
                                Our termite baiting system is one of the most advanced and environmentally responsible methods
                                available for colony elimination. Bait stations are carefully placed above ground in active areas
                                and in-ground around the perimeter of your property.
                            </p>

                            <p>
                                Termites consume the specially formulated bait and unknowingly spread it throughout the colony,
                                including to the queen. This process leads to the gradual but complete elimination of the entire
                                termite population. Regular inspections ensure the stations remain effective and monitored over time.
                            </p>

                            <!-- Key Benefits -->
                            <h4 class="mt-4 mb-3">Why Choose Our Baiting System:</h4>
                            <ul class="list-group list-group-flush mb-4">
                                <li class="list-group-item">✅ Targets the colony at its source</li>
                                <li class="list-group-item">✅ Minimal chemical use, safe for sensitive environments</li>
                                <li class="list-group-item">✅ Monitored and maintained for long-term effectiveness</li>
                                <li class="list-group-item">✅ Safe for families, pets, and the environment</li>
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
