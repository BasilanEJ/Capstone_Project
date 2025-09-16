<%@ Page Title="" Language="C#" EnableEventValidation="true" MasterPageFile="~/Inquiry.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RRCManagementSystem.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:ScriptManager ID="ScriptManager1" runat="server" />

    <style>
        /* ============ Base / Resets ============ */
        img { max-width: 100%; height: auto; }
        .container, .container-fluid { width: 100%; }
        /* Use Bootstrap's container paddings if present; otherwise add a safe fallback */
        .container { padding-left: 12px; padding-right: 12px; }

        /* ============ Typography ============ */
        p { font-size: 16px; line-height: 1.6; margin: 0 0 12px; }
        h1 { font-size: 30px; color: blue; margin: 20px 0 12px; }
        h2 { font-size: 22px; color: blue; margin: 20px 0 10px; }

        /* ============ Hero (Banner) ============ */
        .hero { margin-top: 5px; padding: 40px 0; }
        .hero-banner { width: 100%; height: auto; object-fit: cover; }
        .hero-img { max-width: 100%; height: auto; border-radius: 10px; }
        .hero-title { font-size: 32px; font-weight: bold; color: #2c3e50; margin-bottom: 15px; }

        /* Fade-in effect */
        @keyframes fadeIn { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
        .hero h1, .hero p, .hero .btn, .services h2 { animation: fadeIn 1s ease-in-out; }
        .fade-in { opacity: 0; animation: fadeIn 1s ease-in-out forwards; }

        /* ============ Services Section ============ */
        .services { padding: 60px 0; background-color: #f8f9fa; }
        .services h2 { margin-bottom: 30px; }
        .service-card {
            background: white; padding: 20px; border-radius: 8px; text-align: center;
            transition: transform 0.3s ease-in-out; height: 100%;
        }
        .service-card:hover { transform: translateY(-10px); }
        .service-card h4 { font-size: 16px; margin-top: 10px; }

        /* ============ Video Hero (Vimeo) ============ */
        .video-hero {
            position: relative; width: 100%; background-color: #000; overflow: hidden;
        }
        /* Desktop/Laptop: fill viewport height like your original */
        @media (min-width: 768px) {
            .video-hero { height: 100vh; }
            .video-hero .video-layer { position: absolute; inset: 0; }
        }
        /* Phones: use 16:9 aspect ratio wrapper to avoid awkward vertical scroll */
        @media (max-width: 767.98px) {
            .video-hero { height: auto; }
            .video-hero .ratio-box { position: relative; width: 100%; padding-top: 56.25%; } /* 16:9 */
            .video-hero .video-layer { position: absolute; inset: 0; }
        }
        .video-hero .thumb,
        .video-hero iframe {
            width: 100%; height: 100%; object-fit: cover; border: 0; display: block;
        }
        .video-hero .play-btn {
            position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
            background: rgba(0, 0, 0, 0.7); color: #fff; padding: 14px 22px; border-radius: 50px;
            font-size: 1.25rem; cursor: pointer; text-align: center; line-height: 1;
            user-select: none;
        }
        .video-hero .video-title {
            position: absolute; top: 5%; left: 50%; transform: translateX(-50%);
            color: #fff; font-size: 2rem; text-shadow: 2px 2px 10px rgba(0,0,0,0.7); margin: 0;
        }
        @media (min-width: 768px) {
            .video-hero .play-btn { font-size: 1.5rem; padding: 20px 30px; }
            .video-hero .video-title { font-size: 3rem; }
        }

        /* ============ Inquiry Form Card ============ */
        .inquiry-wrap {
            display: flex; justify-content: center; align-items: stretch;
            min-height: 60vh; background: #fff; font-family: 'Segoe UI', sans-serif;
        }
        .inquiry-card {
            display: flex; width: 100%; max-width: 1000px; border-radius: 12px; overflow: hidden;
            background: #fff; box-shadow: 0 8px 20px rgba(0,0,0,0.2);
        }
        .inquiry-left {
            flex: 1; padding: 32px; background: #63b3ed;
        }
        .inquiry-right {
            flex: 1; min-height: 260px;
            background: url('/Images/service-baiting.jpg') center center / cover no-repeat;
        }
        .form-underline {
            width: 100%; border: none; border-bottom: 2px solid #1a202c; padding: 10px;
            background: transparent; color: #1a202c; font-size: 14px;
        }
        .textarea-box {
            width: 100%; border-radius: 4px; padding: 10px; font-size: 14px; min-height: 120px;
        }
        .btn-submit {
            width: 100%; padding: 12px; background-color: #1a202c; color: #fff; border: 0;
            border-radius: 4px; font-weight: bold; font-size: 16px;
        }



        /* Stack on smaller screens; keep design language/colors */
        @media (max-width: 991.98px) {
            .inquiry-card { flex-direction: column; }
            .inquiry-right { height: 240px; } /* visible banner on phones/tablets */
            .inquiry-left { padding: 24px; }
        }

        .is-invalid {
    outline: 2px solid red !important;
    border-radius: 4px;
}

       .service-image {
        width: 100%;
        height: 150px; /* Adjust as needed */
        object-fit: cover; /* Crop to fit without distortion */
        border-radius: 8px; /* Optional rounded corners */
        display: block;
        margin: 0 auto;
    }

    /* Service Card Styling */
    .service-card {
        background: #fff;
        padding: 10px;
        border: 1px solid #ddd;
        border-radius: 8px;
        height: 100%;
        transition: transform 0.3s ease;
    }

    .service-card:hover {
        transform: translateY(-5px);
        box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
    }

    /* Text Styling */
    .service-card h4 {
        font-size: 1rem;
        margin-top: 10px;
        color: #333;
    }

    .service-card p {
        font-size: 0.85rem;
        color: #666;
    }


        /* ============ Cookie Banner / Modal ============ */
        #cookieConsentBanner {
            position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%);
            background: rgba(255,255,255,0.95); color:#333; padding: 30px; text-align: center;
            font-size: 14px; z-index: 9999; border: 2px solid #007bff; border-radius: 10px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1); opacity: 0; visibility: hidden;
            transition: opacity .6s ease, visibility .6s ease; max-width: 520px; width: 90%;
        }
        #cookieConsentBanner.show { opacity: 1; visibility: visible; }
        #cookieConsentBanner.hide { opacity: 0; visibility: hidden; transition: opacity .8s ease; }
        #cookieConsentBanner button:hover { background-color: #0056b3; }

        /* ============ Small Screen Type Tweaks ============ */
        @media (max-width: 575.98px) {
            h1 { font-size: 26px; }
            h2 { font-size: 20px; }
            .hero-title { font-size: 26px; }
            p { font-size: 15px; }
            .service-card h4 { font-size: 14px; }
        }

        body {
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    background-color: #f8f9fa;
}

/* Section Titles */
.services h1 {
    font-weight: 700;
    color: #007bff;
}

.services h2 {
    font-weight: 600;
    color: #6c757d;
}

/* ==========================================
   CAROUSEL & INDICATORS
========================================== */
.carousel-control-prev-icon,
.carousel-control-next-icon {
    background-size: 70% 70%;
}

.carousel-control-prev-icon.bg-dark,
.carousel-control-next-icon.bg-dark {
    background-color: rgba(0, 0, 0, 0.75);
}

/* Carousel indicators */
.carousel-indicators button {
    background-color: #007bff;
    width: 12px;
    height: 12px;
    border-radius: 50%;
}

.carousel-indicators button.active {
    background-color: #0056b3;
}

/* ==========================================
   SERVICE CARDS
========================================== */
.service-card {
    cursor: pointer;
    background: #fff;
    border-radius: 10px;
    border: 1px solid #e0e0e0;
    transition: transform 0.3s ease, box-shadow 0.3s ease, border-color 0.3s ease;
    padding: 15px 10px;
    height: 100%;
    text-align: center;
}

.service-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.12);
    border-color: #007bff;
}

/* Service Image */
.service-image {
    width: 100%;
    height: 150px;
    object-fit: cover;
    border-radius: 6px;
    margin-bottom: 10px;
}

/* Service Title */
.service-card h4 {
    font-size: 1.05rem;
    font-weight: 600;
    color: #333;
    margin: 10px 0 5px;
}

/* Service Description */
.service-card p {
    font-size: 0.85rem;
    color: #6c757d;
    margin-bottom: 0;
}

/* ==========================================
   MODAL ANIMATIONS
========================================== */

/* Smooth dark overlay */
.modal-backdrop {
    background-color: rgba(0, 0, 0, 0.6);
    transition: opacity 0.4s ease-in-out;
}

/* Slide up & fade animation */
.modal.fade .modal-dialog {
    transform: translateY(-40px);
    opacity: 0;
    transition: all 0.4s ease-out;
}

.modal.show .modal-dialog {
    transform: translateY(0);
    opacity: 1;
}

/* ==========================================
   MODAL DESIGN
========================================== */
.modal-content {
    border-radius: 12px;
    border: none;
    overflow: hidden;
    box-shadow: 0 8px 25px rgba(0, 0, 0, 0.3);
    background: #fff;
}

/* Modal Header */
.modal-header {
    background: #007bff;
    color: white;
    border-bottom: none;
    padding: 15px 20px;
}

.modal-header .modal-title {
    font-size: 1.3rem;
    font-weight: 600;
}

.modal-header .btn-close {
    filter: invert(1); /* Makes close button white */
    opacity: 0.9;
}

/* Modal Body */
.modal-body img {
    border-radius: 8px;
    margin-bottom: 15px;
    max-width: 400px;     /* Set the maximum width */
    max-height: 350px;    /* Set the maximum height */
    width: 100%;          /* Keep it responsive */
    height: auto;         /* Maintain aspect ratio */
    display: block;       /* Center the image */
    margin-left: auto;
    margin-right: auto;
    object-fit: cover;    /* Crop if necessary without stretching */
}


.modal-body p {
    font-size: 1rem;
    color: #444;
    line-height: 1.6;
}

/* Bullet List inside Modal */
.modal-body ul {
    padding-left: 20px;
    margin-top: 15px;
}

.modal-body ul li {
    font-size: 0.95rem;
    padding: 4px 0;
    color: #333;
}

/* Modal Footer */
.modal-footer {
    background: #f8f9fa;
    border-top: none;
    padding: 15px;
}

.modal-footer .btn {
    min-width: 100px;
    font-weight: 500;
}

/* Close button hover effect */
.modal-footer .btn-secondary:hover {
    background-color: #5a6268;
    color: #fff;
    transition: background 0.3s ease-in-out;
}
/* Prevent page content from shifting when modal opens */
body.modal-open {
    overflow: hidden;
    padding-right: 0 !important; /* Prevents scrollbar gap */
}


/* ==========================================
   RESPONSIVE DESIGN
========================================== */
@media (max-width: 992px) {
    .service-image {
        height: 130px;
    }
    .service-card h4 {
        font-size: 0.95rem;
    }
}

@media (max-width: 768px) {
    .service-image {
        height: 110px;
    }
    .service-card h4 {
        font-size: 0.9rem;
    }
}

@media (max-width: 576px) {
    .service-image {
        height: 100px;
    }
    .service-card h4 {
        font-size: 0.85rem;
    }
}



    </style>

    <!-- ====== HERO BANNER ====== -->
    <section class="hero">
        <div class="container-fluid p-0 position-relative">
            <img src="/images/rrc1.png" alt="Pest Control Banner" class="hero-banner img-fluid">
        </div>
    </section>
    <section class="services py-5">
    <div class="container">
        <div class="text-center mb-4">
            <h2 style="color: gray;">WE PROVIDE THE BEST</h2>
            <h1 style="color: blue;">Termite and Pest Control Services</h1>
        </div>

        <div id="servicesCarousel" class="carousel slide" data-bs-ride="carousel">
            <div class="carousel-inner">

                <!-- ====== TERMITE CONTROL SERVICES ====== -->
                <div class="carousel-item active">
                    <h3 class="text-dark mb-3 text-center">Termite Control Services</h3>
                    <div class="row justify-content-center g-3">

                        <!-- Baiting System -->
                        <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                            <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalBaiting">
                                <img src="/images/service-baiting.jpg" alt="Baiting System" class="service-image">
                                <h4>Baiting System</h4>
                                <p class="small">Above Ground and In Ground for Colony Elimination</p>
                            </div>
                        </div>

                   <!-- Termite Prevention -->
<div class="col-lg-2 col-md-3 col-sm-4 col-6">
    <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalTermitePrevention">
        <img src="/images/service-termite-prevention.jpg" alt="Termite Prevention" class="service-image">
        <h4>Termite Prevention</h4>
        <p class="small">Stops infestations before they begin with long-term barrier protection</p>
    </div>
</div>


                        <!-- Soil Poisoning -->
                        <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                            <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalSoilPoisoning">
                                <img src="/images/service-soil.jpg" alt="Soil Poisoning" class="service-image">
                                <h4>Soil Poisoning</h4>
                               <p class="small">Build a strong foundation with termite-proof protection for new & existing homes.</p>
                            </div>
                        </div>

                        <!-- Reticulation -->
                        <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                            <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalReticulation">
                                <img src="/images/services-reticulations.jpg" alt="Reticulation" class="service-image">
                                <h4>Reticulation</h4>
                                <p class="small">Smart termite defense with a hidden pipe system for hassle-free treatments.</p>
                            </div>
                        </div>

                        <!-- Mound Demolition -->
                        <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                            <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalMoundDemolition">
                                <img src="/images/service-mound.jpg" alt="Mound Demolition" class="service-image">
                                <h4>Mound Demolition</h4>
                                <p class="small">Directly eliminate termite colonies by targeting the queen at the source.</p>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ====== GENERAL PEST CONTROL SERVICES ====== -->
                <div class="carousel-item">
                    <h3 class="text-dark mb-3 text-center">General Pest Control Services</h3>
                    <div class="row justify-content-center g-3">
                        
                        <!-- General Pest Control -->
                        <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                            <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalGeneralPest">
                                <img src="/images/service-general-pest.jpg" alt="General Pest Control" class="service-image">
                                <h4>General Pest Control</h4>
                                <p class="small">Cockroaches, Ants, Mosquitoes, Flies</p>
                            </div>
                        </div>

                        <!-- Tick & Fleas Control -->
                        <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                            <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalTickFleas">
                                <img src="/images/service-tick-fleas.jpg" alt="Tick & Fleas Control" class="service-image">
                                <h4>Tick & Fleas Control</h4>
                                <p class="small">Breaks the breeding cycle and protects your pets & family</p>
                            </div>
                        </div>

                        <!-- Bedbugs Control -->
                        <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                            <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalBedbugs">
                                <img src="/images/service-bedbugs.jpg" alt="Bedbugs Control" class="service-image">
                                <h4>Bedbugs Control</h4>
                                <p class="small">Thorough inspection and treatment for lasting relief</p>
                            </div>
                        </div>

                        <!-- Rat / Rodents Control -->
                        <div class="col-lg-2 col-md-3 col-sm-4 col-6">
                            <div class="service-card text-center h-100" data-bs-toggle="modal" data-bs-target="#modalRatControl">
                                <img src="/images/service-rats.jpg" alt="Rat Control" class="service-image">
                                <h4>Rat / Rodents Control</h4>
                                <p class="small">Safe and effective removal to prevent health risks</p>
                            </div>
                        </div>
                    </div>
                </div>

            </div>

            <!-- Carousel Controls -->
            <button class="carousel-control-prev" type="button" data-bs-target="#servicesCarousel" data-bs-slide="prev">
                <span class="carousel-control-prev-icon bg-dark rounded-circle p-3" aria-hidden="true"></span>
                <span class="visually-hidden">Previous</span>
            </button>
            <button class="carousel-control-next" type="button" data-bs-target="#servicesCarousel" data-bs-slide="next">
                <span class="carousel-control-next-icon bg-dark rounded-circle p-3" aria-hidden="true"></span>
                <span class="visually-hidden">Next</span>
            </button>

            <!-- Carousel Indicators
            <div class="carousel-indicators">
                <button type="button" data-bs-target="#servicesCarousel" data-bs-slide-to="0" class="active" aria-current="true"></button>
                <button type="button" data-bs-target="#servicesCarousel" data-bs-slide-to="1"></button>
            </div> -->
        </div>
    </div>
</section>



    <!-- ======================================
     TERMITE CONTROL SERVICES MODALS
====================================== -->

<!-- Baiting System Modal -->
<div class="modal fade" id="modalBaiting" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Baiting System</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <img src="/images/service-baiting.jpg" alt="Baiting System" class="img-fluid mb-3 rounded">
        <p>
          Our termite baiting system is one of the most advanced and environmentally responsible methods
          available for colony elimination. Bait stations are carefully placed above ground in active areas
          and in-ground around the perimeter of your property. Termites consume the specially formulated
          bait and unknowingly spread it throughout the colony, including to the queen.
        </p>
        <p>
          This process leads to the gradual but complete elimination of the entire termite population.
          Regular inspections ensure the stations remain effective and monitored over time.
        </p>
        <ul>
          <li>✅ Targets the colony at its source</li>
          <li>✅ Minimal chemical use, safe for sensitive environments</li>
          <li>✅ Monitored and maintained for long-term effectiveness</li>
          <li>✅ Safe for families, pets, and the environment</li>
        </ul>
      </div>
     
    </div>
  </div>
</div>

<!-- Termite Prevention Modal -->
<div class="modal fade" id="modalTermitePrevention" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Termite Prevention</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <img src="/images/service-termite-prevention.jpg" alt="Termite Prevention" class="img-fluid mb-3 rounded">
        <p>
          Prevention is the smartest and most cost-effective way to handle termites before they cause damage.
          Our preventive services create a protective barrier that blocks termites from ever reaching your property.
          Using a mix of physical barriers, liquid termiticides, and advanced technology,
          we provide coverage for both new and existing structures.
        </p>
        <p>
          Regular inspections allow us to identify risks early and reinforce your defenses,
          protecting your property value and preventing expensive repairs in the future.
        </p>
        <ul>
          <li>✅ Stops infestations before they begin</li>
          <li>✅ Long-term barrier protection for homes and businesses</li>
          <li>✅ Reduces costly repair risks by preventing structural damage</li>
          <li>✅ Ideal for both residential and commercial properties</li>
        </ul>
      </div>
     
    </div>
  </div>
</div>

<!-- Soil Poisoning Modal -->
<div class="modal fade" id="modalSoilPoisoning" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Soil Poisoning (Pre & Post Construction)</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <img src="/images/service-soil.jpg" alt="Soil Poisoning" class="img-fluid mb-3 rounded">
        <p>
          Soil treatment, also called soil poisoning, is one of the most trusted termite-proofing techniques.
          For pre-construction, we apply a termiticide treatment to the soil before the foundation is laid,
          creating a shield that termites cannot cross.
        </p>
        <p>
          For post-construction, we drill around the foundation and inject chemicals deep into the soil to
          reinforce protection. Both methods create a continuous chemical barrier that protects against termite entry.
        </p>
        <ul>
          <li>✅ Essential for long-term structural protection</li>
          <li>✅ Provides defense for both new builds and existing properties</li>
          <li>✅ Creates a continuous barrier termites cannot cross</li>
          <li>✅ Peace of mind against hidden termite threats</li>
        </ul>
      </div>
      
    </div>
  </div>
</div>

<!-- Reticulation Modal -->
<div class="modal fade" id="modalReticulation" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Reticulation (Perforated Pipe System)</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <img src="/images/services-reticulations.jpg" alt="Reticulation" class="img-fluid mb-3 rounded">
        <p>
          Our reticulation system is a modern termite management solution that makes re-treatment simple and efficient.
          A network of underground perforated pipes is installed around your property’s foundation,
          allowing termiticide to be evenly distributed in the soil.
        </p>
        <p>
          When it’s time for re-application, chemicals can be delivered directly into the system without drilling
          or damaging floors. This provides a long-term, cost-effective solution for termite management.
        </p>
        <ul>
          <li>✅ Even, reliable distribution of termiticide</li>
          <li>✅ Easy re-application without drilling or disruption</li>
          <li>✅ Long-lasting and cost-effective protection</li>
          <li>✅ Discreet system that preserves your property’s appearance</li>
        </ul>
      </div>
     
    </div>
  </div>
</div>

<!-- Mound Demolition Modal -->
<div class="modal fade" id="modalMoundDemolition" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Mound Demolition (Queen Finder)</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <img src="/images/service-mound.jpg" alt="Mound Demolition" class="img-fluid mb-3 rounded">
        <p>
          For properties with visible termite mounds, mound demolition provides a direct and highly effective treatment.
          Our specialists carefully dismantle the mound, apply treatment, and locate the queen for elimination.
          Removing the queen ensures the colony cannot rebuild and prevents further spread.
        </p>
        <ul>
          <li>✅ Fast and decisive colony elimination</li>
          <li>✅ Long-term results by directly targeting the queen</li>
          <li>✅ Prevents spread of termites to nearby areas</li>
          <li>✅ Effective solution for large outdoor infestations</li>
        </ul>
      </div>
     
    </div>
  </div>
</div>


<!-- ======================================
     GENERAL PEST CONTROL SERVICES MODALS
====================================== -->

<!-- General Pest Control Modal -->
<div class="modal fade" id="modalGeneralPest" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">General Pest Control Treatment</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <img src="/images/service-general-pest.jpg" alt="General Pest Control" class="img-fluid mb-3 rounded">
        <p>
          Our general pest control services cover the most common pests that threaten health, hygiene, and comfort.
          Each pest is treated using specialized methods:
        </p>
        <ul>
          <li><strong>Cockroaches:</strong> Targeted sprays and gel baits reach deep into hiding places.</li>
          <li><strong>Ants:</strong> Eliminate visible ants and hidden colonies to prevent reinfestation.</li>
          <li><strong>Mosquitoes:</strong> Reduce breeding areas and apply larvicides and residual sprays.</li>
          <li><strong>Flies:</strong> Traps, sprays, and sanitation recommendations reduce contamination risks.</li>
        </ul>
        <p>
          This integrated approach ensures healthier, cleaner, and pest-free surroundings.
        </p>
        <ul>
          <li>✅ Comprehensive coverage for common household pests</li>
          <li>✅ Treatments tailored to each species and infestation level</li>
          <li>✅ Protects families and businesses from health risks</li>
          <li>✅ Creates a cleaner, more comfortable environment</li>
        </ul>
      </div>
     
    </div>
  </div>
</div>

<!-- Tick & Fleas Control Modal -->
<div class="modal fade" id="modalTickFleas" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Tick & Fleas Control</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <img src="/images/service-tick-fleas.jpg" alt="Tick & Fleas Control" class="img-fluid mb-3 rounded">
        <p>
          Ticks and fleas are more than just annoying—they can transmit harmful diseases to both humans and pets.
          Our treatments target every stage of their lifecycle, from eggs to adults, to break the cycle of infestation.
        </p>
        <p>
          We focus on key areas such as carpets, pet bedding, gardens, and shaded spots where these pests thrive,
          while providing preventive advice for long-term protection.
        </p>
        <ul>
          <li>✅ Safe for households with pets and children</li>
          <li>✅ Breaks the breeding cycle for long-term results</li>
          <li>✅ Reduces risks of diseases like Lyme disease and flea-borne fevers</li>
          <li>✅ Targets indoor and outdoor hotspots</li>
        </ul>
      </div>
     
    </div>
  </div>
</div>

<!-- Bedbugs Control Modal -->
<div class="modal fade" id="modalBedbugs" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Bedbugs Control</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <img src="/images/service-bedbugs.jpg" alt="Bedbugs Control" class="img-fluid mb-3 rounded">
        <p>
          Bedbugs are notorious for hiding in hard-to-reach areas like mattresses, furniture, and cracks.
          Our service begins with a thorough inspection to identify all hiding spots, followed by targeted treatments
          like heat and residual sprays to kill bedbugs at every stage.
        </p>
        <p>
          Follow-up visits ensure complete eradication and prevent re-infestation, restoring comfort and peace of mind.
        </p>
        <ul>
          <li>✅ Comprehensive inspection to locate infestations</li>
          <li>✅ Treatments that eliminate eggs, nymphs, and adults</li>
          <li>✅ Effective for both minor and severe infestations</li>
          <li>✅ Restful, bite-free sleep restored</li>
        </ul>
      </div>
    
    </div>
  </div>
</div>

<!-- Rat & Rodents Control Modal -->
<div class="modal fade" id="modalRatControl" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Rat & Rodents Control</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <img src="/images/service-rats.jpg" alt="Rat & Rodents Control" class="img-fluid mb-3 rounded">
        <p>
          Rodents like rats and mice pose serious threats to health and property. They spread diseases,
          contaminate food, and can cause electrical fires by chewing wires.
        </p>
        <p>
          Our service uses traps, baits, and exclusion methods to remove infestations and prevent recurrence.
          We also seal entry points and advise on sanitation practices to keep them away for good.
        </p>
        <ul>
          <li>✅ Safe and effective elimination methods</li>
          <li>✅ Preventive measures to block future infestations</li>
          <li>✅ Protects property from costly damage</li>
          <li>✅ Reduces health risks linked to rodent-borne diseases</li>
        </ul>
      </div>
     
    </div>
  </div>
</div>



    <!-- ====== ABOUT SPLIT ====== -->
    <section class="hero">
        <div class="container">
            <div class="row align-items-center gy-4">
                <div class="col-md-6 text-center">
                    <img src="/images/ppe.png" alt="Pest Control Worker" class="img-fluid hero-img">
                </div>

                <div class="col-md-6">
                    <h1>R.R.C. Termite and Pest Control</h1>
                    <p>We are your trusted partners in safeguarding your home and business against unwelcome intruders. Our mission is to provide affordable and reliable pest control services that prioritize your peace of mind and the well-being of your property.</p>

                    <h2><i class="fas fa-check-circle check-icon"></i> Innovative Solutions</h2>
                    <p>We provide cutting-edge solutions to safeguard your home from termites and pests. With a strong focus on environmentally friendly and effective treatments, we use advanced techniques to ensure your property is protected.</p>

                    <h2><i class="fas fa-check-circle check-icon"></i> Certified Services</h2>
                    <p>As a certified pest control provider, we adhere to the highest industry standards. Our team of licensed professionals undergoes rigorous training to offer you expert service and long-lasting results. Every treatment is tailored to meet your unique needs, ensuring your home or business remains pest-free.</p>

                    <h2><i class="fas fa-check-circle check-icon"></i> Local and Trusted</h2>
                    <p>We take pride in serving our community. With years of experience in the area, we understand the specific challenges homeowners face. Our reputation as trusted experts is built on reliability, honesty, and exceptional customer care.</p>
                </div>
            </div>
        </div>
    </section>

    <!-- ====== RESPONSIVE VIDEO (VIMEO) ====== -->
    <section class="hero video-hero">
        <!-- For phones we use a ratio box; for md+ heights are controlled by .video-hero -->
        <div class="ratio-box d-md-none"></div>
        <h1 class="video-title"></h1>

        <div class="video-layer">
            <!-- Thumbnail -->
            <img id="videoThumbnail" src="/images/banner tv.jpg" alt="Video Thumbnail" class="thumb" style="cursor:pointer;">

            <!-- Vimeo iframe (hidden until play) -->
            <iframe id="vimeoVideo"
                    src="https://player.vimeo.com/video/1009218555?loop=1&muted=0"
                    allow="autoplay; fullscreen"
                    allowfullscreen
                    style="display:none;"></iframe>

            <!-- Play Button -->
            <div id="playButton" class="play-btn">▶ Play</div>
        </div>
    </section>

    <!-- ====== Blog Section ====== -->
<section style="text-align:center; padding: 40px 12px;">
    <h2 style="font-size: 24px; font-weight: bold; color: #121481;">Read our Blogs</h2>
    <div style="display:flex; flex-wrap:wrap; justify-content:center; gap:20px; margin-top:30px;">
        
        <!-- Blog Card 1 -->
        <a href="DIY.aspx" style="text-decoration:none; color:inherit; width:300px; border:1px solid #ddd; border-radius:10px; overflow:hidden; transition: transform 0.3s;">
            <img src="/Images/DIY.jpg" alt="DIY vs Professional Pest Control" style="width:100%; height:200px; object-fit:cover;">
            <div style="padding:15px; text-align:left;">
                <h3 style="font-size:18px; font-weight:bold; margin-bottom:10px;">DIY pest control vs. hiring a pest control company: What’s the difference?</h3>
            </div>
        </a>

        <!-- Blog Card 2 -->
        <a href="Eskwela.aspx" style="text-decoration:none; color:inherit; width:300px; border:1px solid #ddd; border-radius:10px; overflow:hidden; transition: transform 0.3s;">
            <img src="/Images/blog2.jpg" alt="Brigada Eskwela Anti-Dengue" style="width:100%; height:200px; object-fit:cover;">
            <div style="padding:15px; text-align:left;">
                <h3 style="font-size:18px; font-weight:bold; margin-bottom:10px;">Brigada Eskwela Anti-Dengue Campaign: Ensuring a Safe and Healthy Learning Environment</h3>
            </div>
        </a>

        <!-- Blog Card 3 -->
        <a href="Termite.aspx" style="text-decoration:none; color:inherit; width:300px; border:1px solid #ddd; border-radius:10px; overflow:hidden; transition: transform 0.3s;">
            <img src="/Images/blog3.jpg" alt="Termite Swarms" style="width:100%; height:200px; object-fit:cover;">
            <div style="padding:15px; text-align:left;">
                <h3 style="font-size:18px; font-weight:bold; margin-bottom:10px;">Don’t Let Termite Swarms Take Over Your Home!</h3>
            </div>
        </a>

    </div>
</section>


    <!-- ====== C&O ====== -->
    <section style="text-align: center; padding: 40px 12px;">
        <h2 style="color: #0B2A63; font-size: 2rem; font-weight: bold;">Certifications & Organizations</h2>
        <div style="display:flex; justify-content:center; align-items:center; margin-top:20px;">
            <img src="/images/c&o.png" alt="Certifications & Organizations" style="max-width: 900px; width: 90%; height: auto;">
        </div>
    </section>

    <!-- ====== REVIEWS ====== -->
    <section style="text-align: center; padding: 40px 12px; font-family: Arial, sans-serif;">
        <h2 style="font-size: 24px; font-weight: bold; color: #121481;">What our customers are saying</h2>

        <div style="display:flex; flex-wrap:wrap; justify-content:center; gap:20px; margin-top:20px;">
            <div style="width: 260px; max-width: 90vw; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.2); text-align:left;">
                <strong>Czarina Joy T. Chang</strong>
                <p style="font-size: 12px; color: red;">❤️ recommends</p>
                <p style="font-size: 14px; color: #333;">They were on time, very professional and mababait mga staff ni RRC team. Mabusisi sila sa bawat sulok ng bahay at maayos silang magtrabaho. Very polite and courteous pa yun technicians and staff na nag execute ng baiting system. Highly recommended! Good job!</p>
            </div>

            <div style="width: 260px; max-width: 90vw; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.2); text-align:left;">
                <strong>Mel Lareza</strong>
                <p style="font-size: 12px; color: red;">❤️ recommends</p>
                <p style="font-size: 14px; color: #333;">Excellent Service!!! ⭐️⭐️⭐️⭐️⭐️</p>
            </div>

            <div style="width: 260px; max-width: 90vw; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.2); text-align:left;">
                <strong>Dannica Manaluz.</strong>
                <p style="font-size: 12px; color: red;">❤️ recommends</p>
                <p style="font-size: 14px; color: #333;">Very satisfied client here 5/5 stars ⭐️⭐️⭐️⭐️⭐️</p>
            </div>

            <div style="width: 260px; max-width: 90vw; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.2); text-align:left;">
                <strong>Chelsea Erese Ong</strong>
                <p style="font-size: 12px; color: red;">❤️ recommends</p>
                <p style="font-size: 14px; color: #333;">RRC/Sir Victor and staff of technicians were very accommodating and understanding despite us having to change schedule of termite treatment...</p>
            </div>
        </div>
    </section>

    <!-- SweetAlert2 (kept) -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

<!-- ====== INQUIRY FORM (Responsive with UpdatePanel) ====== -->
<asp:UpdatePanel ID="upInquiryForm" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <section class="inquiry-wrap">
            <div class="inquiry-card">
      
                <div class="inquiry-left">
                    <h3 style="text-transform: uppercase; font-size: 14px; font-weight: 600; color: white;">Schedule Your</h3>
                    <h1 style="font-size: 28px; font-weight: 700; color: #1a202c; margin-bottom: 12px;">Free Inspection</h1>
                    <p style="font-size: 14px; color: #1a202c; margin-bottom: 18px;">
                        Schedule today! Please fill-in this form and RRC Pest and Termite Control Representative will contact you soon.
                    </p>

              
                    <div style="margin-bottom: 15px;">
                        <asp:Label runat="server" AssociatedControlID="txtEmail" Text="Your email *" />
                        <asp:TextBox ID="txtEmail" runat="server"
                            CssClass="form-control form-underline"
                            TextMode="Email"
                            placeholder="email@gmail.com"
                            required />
                        <asp:RegularExpressionValidator ID="revEmail" runat="server"
                            ControlToValidate="txtEmail"
                            ErrorMessage="Please enter a valid Gmail, Yahoo, Outlook, iCloud, or school/government email address."
                            ForeColor="Red"
                            Display="Dynamic"
                            ValidationExpression="^[A-Za-z0-9._%+\-]+@(?:(?:gmail|yahoo|outlook|hotmail|live|icloud)\.com|(?:[A-Za-z0-9-]+\.)*edu\.ph|(?:[A-Za-z0-9-]+\.)*gov\.ph)$" />
                    </div>

                   
                    <div style="margin-bottom: 15px;">
                        <asp:TextBox ID="txtContactNumber" runat="server"
                            placeholder="09xxxxxxxxx" required MaxLength="11"
                            onkeypress="return isDigit(event)"
                            onkeydown="return blockNonDigits(event)"
                            oninput="validateContactNumber(this)"
                            onpaste="handlePaste(event)"
                            CssClass="form-underline" />
                    </div>

                    <div style="margin-bottom: 15px;">
                        <asp:Label runat="server" AssociatedControlID="fuPestPhoto" Text="Photo of Pest (optional)" />
                        <asp:FileUpload ID="fuPestPhoto" runat="server"
                            style="width: 100%; background: white; padding: 8px; border-radius: 4px;"
                            accept=".png,.jpg,.jpeg,image/png,image/jpeg" />
                        <small style="color: #1a202c;">Upload a photo if available.</small>
                    </div>

          
                    <div style="margin-bottom: 15px;">
                        <asp:Label runat="server" AssociatedControlID="txtMessage" Text="Describe what you observed*" />
                        <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" Rows="4"
                            placeholder="Describe what you observed..." required CssClass="textarea-box" />
                    </div>

        
                    <div style="margin-bottom: 10px;">
                        <a href="#" data-bs-toggle="modal" data-bs-target="#termsModal" style="font-size: 14px; color: #1a202c; text-decoration: underline;">
                            View Terms and Conditions
                        </a>
                    </div>


                    <div class="modal fade" id="termsModal" tabindex="-1" aria-labelledby="termsModalLabel" aria-hidden="true">
                        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
                            <div class="modal-content">

                                <div class="modal-header">
                                    <h5 class="modal-title" id="termsModalLabel">Terms & Conditions / Privacy Policy</h5>
                                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                                </div>

                                <div class="modal-body" style="color:#1a202c; line-height:1.45;">
                                
                                    <h6 class="mb-2">1. Agreement to Terms</h6>
                                    <p>
                                        Welcome to RRC Termite &amp; Pest Control Inquiry! By accessing and using our inquiry page,
                                        you agree to comply with and be bound by these Terms and Conditions.
                                        Your use of RRC Inquiry is subject to the Philippine Data Privacy Act of 2012 (DPA 2012) and other relevant laws.
                                        These Terms govern your use of the inquiry form, the accuracy of information you provide,
                                        and how we may process your submitted data. You agree to:
                                    </p>
                                    <ul>
                                        <li>Use the inquiry page only for lawful purposes.</li>
                                        <li>Provide truthful, accurate, and current information.</li>
                                        <li>Refrain from submitting harmful, offensive, or unsolicited content.</li>
                                    </ul>
                                    <p>
                                        If you do not agree to these Terms, you must not submit any inquiries or otherwise use the inquiry page.
                                        By proceeding, you acknowledge that you have read, understood, and accepted these Terms.
                                    </p>

                                    <h6 class="mt-3 mb-2">2. Definitions</h6>
                                    <ul>
                                        <li><strong>“RRC Inquiry”</strong> – The official online platform of RRC Termite &amp; Pest Control,
                                            provided for submitting inquiries, viewing bookings, bills, and booking schedules.</li>
                                        <li><strong>“User”</strong> – Any individual accessing RRC Inquiry, including customers, inspectors, and administrators.</li>
                                        <li><strong>“Personal Data”</strong> – Information that identifies or can identify a person, as defined under the DPA 2012.</li>
                                        <li><strong>“Services”</strong> – The inquiry-related functions including submission of inquiries, viewing of bookings, bills, and schedules.</li>
                                    </ul>

                                    <h6 class="mt-3 mb-2">3. Purpose of the Inquiry Page</h6>
                                    <p>
                                        This page is provided solely for users to submit inquiries, view their bookings, bills, and schedules.
                                        It is not intended for placing orders, entering into contracts, or seeking emergency assistance unless explicitly stated otherwise.
                                    </p>

                                    <h6 class="mt-3 mb-2">4. Information Accuracy</h6>
                                    <p>
                                        You agree that all information submitted is accurate, current, and complete.
                                        You are responsible for maintaining the confidentiality of any account or contact information
                                        and notifying us immediately of any unauthorized use.
                                    </p>

                                    <h6 class="mt-3 mb-2">5. Use Restrictions</h6>
                                    <ul>
                                        <li>Submitting offensive, discriminatory, defamatory, or harassing content.</li>
                                        <li>Uploading or sharing harmful, dangerous, or spam content.</li>
                                        <li>Using automated systems or bots to send inquiries.</li>
                                    </ul>
                                    <p>
                                        We reserve the right to decline inquiries that violate these guidelines or block repeat offenders.
                                    </p>

                                    <h6 class="mt-3 mb-2">6. Privacy &amp; Data Use</h6>
                                    <p>
                                        Any personal data you submit will be processed in accordance with our Privacy Policy.
                                        We keep data only as long as necessary to respond to inquiries and for legitimate business purposes.
                                    </p>

                                    <h6 class="mt-3 mb-2">7. Intellectual Property</h6>
                                    <p>
                                        All content of the inquiry form and related materials is our property or licensed to us.
                                        You may not reproduce, distribute, modify, or create derivative works from it without written permission.
                                    </p>

                                    <h6 class="mt-3 mb-2">8. Disclaimer of Warranty</h6>
                                    <p>
                                        The inquiry form is provided “as-is” and “as-available.”
                                        We make no warranties—express or implied—regarding its accuracy, reliability, or availability.
                                        All inquiries submitted are at your own risk.
                                    </p>

                                    <h6 class="mt-3 mb-2">9. Limitation of Liability</h6>
                                    <p>
                                        To the maximum extent permitted by law, we shall not be liable for any indirect, incidental, special,
                                        or consequential damages arising from the use of the inquiry page, even if we have been advised of the possibility of such damages.
                                    </p>

                                    <h6 class="mt-3 mb-2">10. Modification and Interruptions</h6>
                                    <p>
                                        We reserve the right to modify, suspend, or discontinue the inquiry page at any time, with or without notice.
                                        We are not liable for any interruptions or errors and may revise these Terms at our discretion.
                                    </p>

                                    <h6 class="mt-3 mb-2">11. Governing Law</h6>
                                    <p>
                                        These Terms are governed by the laws of the Republic of the Philippines.
                                        Any disputes will be subject to the jurisdiction of Philippine courts.
                                    </p>

                                    <h6 class="mt-3 mb-2">12. Changes to Terms</h6>
                                    <p>
                                        We may update these Terms periodically. Continued use after changes constitutes agreement to those revisions.
                                    </p>

                                    <h6 class="mt-3 mb-2">13. Severability</h6>
                                    <p>
                                        If any provision is found unenforceable, the remainder will remain in effect to the fullest extent permitted by law.
                                    </p>

                                    <h6 class="mt-3 mb-2">14. Contact Information</h6>
                                    <p>
                                        Email: <a href="mailto:rrctermiteandpestcontrol@gmail.com">rrctermiteandpestcontrol@gmail.com</a><br />
                                        Address: #33 Kaligatasan Street, Brgy. Holy Spirit, Quezon City, Philippines
                                    </p>

                                    <hr class="my-3" />

                                 
                                    <h6 class="mb-2">Privacy Policy — Inquiry Page</h6>
                                    <p><em>Last Updated: August 19, 2025</em></p>

                                    <p>
                                        RRC Termite &amp; Pest Control respects your privacy and is committed to protecting your personal data
                                        in compliance with the Data Privacy Act of 2012 (Republic Act No. 10173) of the Philippines.
                                        This Privacy Policy explains how we collect, use, store, and protect your information when you use our inquiry page.
                                    </p>

                                    <h6 class="mt-3 mb-2">1. Collection of Personal Data</h6>
                                    <p>When you use the RRC Inquiry page, we may collect personal data such as:</p>
                                    <ul>
                                        <li>Name</li>
                                        <li>Contact number</li>
                                        <li>Email address</li>
                                        <li>Details of your inquiry, bookings, bills, and schedules</li>
                                    </ul>

                                    <h6 class="mt-3 mb-2">2. Purpose of Data Collection</h6>
                                    <ul>
                                        <li>To respond to your inquiries</li>
                                        <li>To allow you to view your bookings, bills, and schedules</li>
                                        <li>To improve our services and customer experience</li>
                                        <li>To comply with legal and regulatory requirements</li>
                                    </ul>

                                    <h6 class="mt-3 mb-2">3. Data Sharing and Disclosure</h6>
                                    <p>
                                        We do not sell, trade, or otherwise transfer your personal data to third parties without your consent,
                                        except when required by law, regulation, or competent authority.
                                    </p>

                                    <h6 class="mt-3 mb-2">4. Data Retention</h6>
                                    <p>
                                        We will retain your personal data only for as long as necessary to fulfill the purposes stated above,
                                        and as required by applicable laws and regulations.
                                    </p>

                                    <h6 class="mt-3 mb-2">5. Data Security</h6>
                                    <p>
                                        We implement appropriate organizational, physical, and technical measures to protect your personal data
                                        from unauthorized access, alteration, disclosure, or destruction.
                                    </p>

                                    <h6 class="mt-3 mb-2">6. User Rights Under the DPA 2012</h6>
                                    <ul>
                                        <li>The right to be informed</li>
                                        <li>The right to access</li>
                                        <li>The right to rectification</li>
                                        <li>The right to object (in certain cases)</li>
                                        <li>The right to erasure/blocking when no longer necessary</li>
                                        <li>The right to data portability</li>
                                        <li>The right to lodge a complaint with the NPC</li>
                                    </ul>

                                    <h6 class="mt-3 mb-2">7. Updates to this Privacy Policy</h6>
                                    <p>
                                        We may update this Privacy Policy from time to time to reflect changes in laws, technology, or business practices.
                                        Any updates will be posted on this page with a new effective date.
                                    </p>

                                    <h6 class="mt-3 mb-2">8. Contact Information</h6>
                                    <p>
                                        Email: <a href="mailto:rrctermiteandpestcontrol@gmail.com">rrctermiteandpestcontrol@gmail.com</a><br />
                                        Address: #33 Kaligatasan Street, Brgy. Holy Spirit, Quezon City, Philippines
                                    </p>

                                 
                                    <div class="mt-3 form-check">
                                        <asp:CheckBox ID="chkTerms" runat="server" CssClass="form-check-input" />
                                        <label class="form-check-label" for="<%= chkTerms.ClientID %>">
                                            I agree to the Terms &amp; Conditions and Privacy Policy.
                                        </label>
                                        
                                        <span id="termsError" style="display:none; color:red; font-size:13px; margin-top:5px;">
                                            Please agree to the terms and conditions before submitting.
                                        </span>
                                    </div>
                                </div>

                                <div class="modal-footer">
                                    <button type="button" class="btn btn-primary" data-bs-dismiss="modal">OK</button>
                                </div>

                            </div>
                        </div>
                    </div>

                    <asp:Button 
                        ID="btnSubmitInquiry" 
                        runat="server" 
                        Text="Submit Inquiry" 
                        CssClass="btn-submit"
                        OnClick="btnSubmitInquiry_Click" />

                </div>

             
                <div class="inquiry-right" aria-hidden="true"></div>
            </div>
        </section>
    </ContentTemplate>
    <Triggers>
       
        <asp:AsyncPostBackTrigger ControlID="btnSubmitInquiry" EventName="Click" />
    </Triggers>
</asp:UpdatePanel>

    <script>
        function validateTerms() {
            var chkTerms = document.getElementById('<%= chkTerms.ClientID %>');

            if (!chkTerms.checked) {
                // Show the Terms Modal automatically
                var termsModal = new bootstrap.Modal(document.getElementById('termsModal'));
                termsModal.show();

                // Prevent the form from submitting
                return false;
            }

            // Allow submission if checkbox is checked
            return true;
        }
    </script>


    <script>
        // --- Vimeo Play logic (kept, made responsive-ready) ---
        const playButton = document.getElementById("playButton");
        const videoThumbnail = document.getElementById("videoThumbnail");
        const vimeoVideo = document.getElementById("vimeoVideo");

        if (playButton && videoThumbnail && vimeoVideo) {
            playButton.addEventListener("click", function () {
                const src = vimeoVideo.getAttribute("src");
                // prevent duplicate autoplay param if tapped twice
                const nextSrc = src.includes("autoplay=1") ? src : (src + (src.includes("?") ? "&" : "?") + "autoplay=1");
                vimeoVideo.setAttribute("src", nextSrc);

                videoThumbnail.style.display = "none";
                playButton.style.display = "none";
                vimeoVideo.style.display = "block";
            });
        }

        // --- Contact number validation (kept) ---
        function isDigit(e) {
            const charCode = e.which || e.keyCode;
            return (charCode >= 48 && charCode <= 57);
        }

        function blockNonDigits(e) {
            const key = e.key;
            const isCtrlOrCmd = e.ctrlKey || e.metaKey;
            const allowedKeys = ["Backspace", "Delete", "ArrowLeft", "ArrowRight", "Tab", "Home", "End"];
            if (allowedKeys.includes(key) || isCtrlOrCmd) return true;
            return /^\d$/.test(key);
        }

        function validateContactNumber(input) {
            input.value = input.value.replace(/\D/g, '');
            if (input.value.length > 11) input.value = input.value.slice(0, 11);
            if (input.value.length > 0 && !input.value.startsWith("09")) {
                input.setCustomValidity("Contact number must start with 09.");
            } else {
                input.setCustomValidity("");
            }
        }

        function handlePaste(e) {
            const paste = (e.clipboardData || window.clipboardData).getData('text');
            if (!/^09\d{0,9}$/.test(paste)) e.preventDefault();
        }

        // --- Cookie banner & modal (kept) ---
        function acceptCookies() {
            const checkbox = document.getElementById('chkCookiePolicy');
            if (checkbox && checkbox.checked) {
                localStorage.setItem('cookieConsentAccepted', 'true');
                const banner = document.getElementById('cookieConsentBanner');
                banner.classList.remove('show');
                banner.classList.add('hide');
                setTimeout(function () { banner.style.display = 'none'; }, 800);
            } else {
                alert('Please check "I accept the cookie policy" before proceeding.');
            }
        }

        function toggleCookiePolicy() {
            document.getElementById('cookiePolicyOverlay').style.display = 'block';
        }

        function closeCookiePolicyModal() {
            document.getElementById('cookiePolicyOverlay').style.display = 'none';
        }

        // Show cookie banner on first visit
        (function () {
            const banner = document.getElementById('cookieConsentBanner');
            if (banner && localStorage.getItem('cookieConsentAccepted') !== 'true') {
                banner.style.display = 'block';
                requestAnimationFrame(() => banner.classList.add('show'));
            }
        })();

        // Close overlay if clicking outside modal
        window.addEventListener('click', function (event) {
            var overlay = document.getElementById('cookiePolicyOverlay');
            if (event.target === overlay) overlay.style.display = 'none';
        });
    </script>

    <!-- ====== Cookie Banner ====== -->
    <div id="cookieConsentBanner">
        <span style="font-size: 16px;">🍪 This website uses cookies to ensure you get the best experience.
            <a href="#" onclick="toggleCookiePolicy(); return false;" style="color: #007bff; text-decoration: underline;">Learn more</a>.
        </span>
        <br><br>
        <div>
            <label style="font-size: 14px; cursor: pointer;">
                <input type="checkbox" id="chkCookiePolicy"
                    style="margin-right: 8px; accent-color: #007bff; width: 18px; height: 18px; cursor: pointer;">
                I accept the cookie policy.
            </label>
        </div>
        <div style="margin-top: 15px;">
            <button type="button" onclick="acceptCookies();"
                style="background:#007bff;color:#fff;border:none;padding:10px 20px;border-radius:6px;cursor:pointer;font-weight:bold;font-size:14px;">
                Accept
            </button>
        </div>
    </div>

    <!-- ====== Cookie Policy Modal Overlay ====== -->
    <div id="cookiePolicyOverlay" style="
        display: none; position: fixed; inset: 0; background: rgba(0, 0, 0, 0.5); z-index: 9999;">
        <div id="cookiePolicyModal" style="
            position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
            background: white; padding: 25px; border-radius: 10px; box-shadow: 0 4px 15px rgba(0,0,0,0.3);
            width: 90%; max-width: 500px; max-height: 80vh; overflow-y: auto;">
            <h2 style="margin-top: 0; color: #007bff;">🍪 Cookie Policy</h2>
            <p style="font-size: 14px; color: #333; text-align: justify;">
                We use cookies to enhance your browsing experience, serve personalized content, and analyze our traffic.<br><br>
                By using this website, you consent to our use of cookies as described below:<br><br>
                <strong>Essential Cookies:</strong> Necessary for website functionality (e.g., login, navigation).<br>
                <strong>Performance Cookies:</strong> Help us understand how visitors interact with our website by collecting and reporting information anonymously.<br>
                <strong>Functional Cookies:</strong> Allow the website to remember your preferences (such as language or location).<br>
                <strong>Targeting Cookies:</strong> May be used to deliver relevant advertisements.<br><br>
                🔵 You can choose to accept or decline cookies. Most web browsers automatically accept cookies, but you can modify your browser setting to decline cookies if you prefer.<br><br>
                For more information, please review our Privacy Policy or contact us at <strong>rrctermite@gmail.com</strong>.<br><br>
                Thank you for trusting RRC Termite and Pest Control!
            </p>
            <div style="text-align: center; margin-top: 20px;">
                <button type="button" onclick="closeCookiePolicyModal()"
                    style="background:#007bff;color:#fff;border:none;padding:10px 20px;border-radius:5px;font-weight:bold;cursor:pointer;">
                    Close
                </button>
            </div>
        </div>
    </div>


    <script>
        // Save scroll position before postback
        window.addEventListener('beforeunload', function () {
            sessionStorage.setItem('scrollPosition', window.scrollY);
        });

        // Restore scroll position after reload
        window.addEventListener('load', function () {
            const scrollPos = sessionStorage.getItem('scrollPosition');
            if (scrollPos) {
                window.scrollTo(0, parseInt(scrollPos));
                sessionStorage.removeItem('scrollPosition');
            }
        });
    </script>

    <script>
        document.addEventListener('DOMContentLoaded', function () {
            const modals = document.querySelectorAll('.modal');

            modals.forEach(modal => {
                modal.addEventListener('hidden.bs.modal', function () {
                    // Keep the scroll position exactly where it was
                    document.documentElement.style.scrollBehavior = 'auto';
                    window.scrollTo(window.scrollX, window.scrollY);
                    document.documentElement.style.scrollBehavior = '';
                });
            });
        });
    </script>


</asp:Content>
