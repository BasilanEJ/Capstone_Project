<%@ Page Title="Soil Poisoning" Language="C#" MasterPageFile="~/Inquiry.Master" AutoEventWireup="true" CodeBehind="SoilPoisoning.aspx.cs" Inherits="RRCManagementSystem.SoilPoisoning" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Internal CSS -->
    <style>
        /* ======= Page Styling ======= */
        .soil-section {
            padding: 50px 0;
        }

        .soil-image {
            width: 100%;
            max-width: 550px;
            height: 280px;
            object-fit: cover;
            border-radius: 8px;
            display: block;
            margin: 0 auto;
            border: 2px solid #e0e0e0;
        }

        .soil-title {
            font-size: 2rem;
            font-weight: bold;
            color: #007bff;
            text-align: center;
            margin-top: 20px;
        }

        .soil-subtitle {
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
            .soil-title {
                font-size: 1.6rem;
            }

            .soil-image {
                height: 220px;
            }
        }
    </style>

    <section class="soil-section">
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
                                <img src="/images/service-soil.jpg" alt="Soil Poisoning" class="soil-image" />
                            </div>

                            <!-- Title & Subtitle -->
                            <h2 class="soil-title">Soil Poisoning</h2>
                            <p class="soil-subtitle">Pre and Post Construction Termite Defense</p>
                            <hr />

                            <!-- Description -->
                            <p>
                                Soil treatment, also called soil poisoning, is one of the most trusted termite-proofing techniques. 
                                For pre-construction, we apply a termiticide treatment to the soil before the foundation is laid, 
                                creating a shield that termites cannot cross.
                            </p>
                            <p>
                                For post-construction, we drill around the foundation and inject chemicals deep into the soil to 
                                reinforce protection. Both methods create a continuous chemical barrier that protects against termite entry.
                            </p>
                            <p>
                                This treatment is essential for long-term termite defense and is highly recommended for anyone 
                                building, buying, or maintaining a property.
                            </p>

                            <!-- Key Benefits -->
                            <h4 class="mt-4 mb-3">Why Choose Our Soil Poisoning Service:</h4>
                            <ul class="list-group list-group-flush mb-4">
                                <li class="list-group-item">✅ Essential for long-term structural protection</li>
                                <li class="list-group-item">✅ Provides defense for both new builds and existing properties</li>
                                <li class="list-group-item">✅ Creates a continuous barrier termites cannot cross</li>
                                <li class="list-group-item">✅ Provides peace of mind against hidden termite threats</li>
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
