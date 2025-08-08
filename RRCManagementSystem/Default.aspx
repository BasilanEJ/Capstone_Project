<%@ Page Title="" Language="C#" EnableEventValidation="true" MasterPageFile="~/Inquiry.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RRCManagementSystem.Default" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:ScriptManager ID="ScriptManager1" runat="server" />


    <style> /* Hero Section */
.hero {
    margin-top: 5px; /* Adds 5px space below the navbar */
}

.hero-banner {
    width: 100%;
    height: auto; /* Keeps aspect ratio */
    object-fit: cover;
}

.hero h1, .hero p, .hero .btn {
    animation: fadeIn 1s ease-in-out;
}

/* Services Section */
.services {
    padding: 60px 0;
    background-color: #f8f9fa;
}

.services h2 {
    margin-bottom: 30px;
    animation: fadeIn 1s ease-in-out;
}

.service-card {
    background: white;
    padding: 20px;
    border-radius: 8px;
    text-align: center;
    transition: transform 0.3s ease-in-out;
}

.service-card:hover {
    transform: translateY(-10px);
}

/* Fade-in effect */
@keyframes fadeIn {
    from { opacity: 0; transform: translateY(20px); }
    to { opacity: 1; transform: translateY(0); }
}

.fade-in {
    opacity: 0;
    animation: fadeIn 1s ease-in-out forwards;
}

.hero {
    padding: 40px 0;
}

.hero-img {
    max-width: 100%;
    height: auto;
    border-radius: 10px;
}

.hero-title {
    font-size: 32px;
    font-weight: bold;
    color: #2c3e50;
    margin-bottom: 15px;
}

p {
    font-size: 16px;
    line-height: 1.6;
}

h2 {
    font-size: 22px;
    color: blue;
    margin-top: 20px;
}

h1 {
    font-size: 30px;
    color: blue;
    margin-top: 20px;
}


    </style>


<section class="hero">
    <div class="container-fluid p-0 position-relative">
        <!-- Banner Image -->
        <img src="/images/rrc1.png" alt="Pest Control Banner" class="hero-banner img-fluid">

    </div>
</section>
  

    <!-- Services Section -->
<section class="services">
    <div class="container">
        <div class="text-center">
            <h2 style="color: gray;">WE PROVIDE THE BEST</h2>
            <h1 style="color: blue;">Termite and Pest Control Services</h1>
        </div>

        <div class="row justify-content-center">
            <div class="col-lg-2 col-md-3 col-sm-4 col-6 fade-in">
                <div class="service-card text-center">
                    <img src="/images/service-baiting.jpg" alt="Baiting System" class="img-fluid">
                    <h4>Baiting System</h4>
                </div>
            </div>

            <div class="col-lg-2 col-md-3 col-sm-4 col-6 fade-in">
                <div class="service-card text-center">
                    <img src="/images/service-termite.jpg" alt="Termite Control" class="img-fluid">
                    <h4>Termite Control</h4>
                </div>
            </div>

            <div class="col-lg-2 col-md-3 col-sm-4 col-6 fade-in">
                <div class="service-card text-center">
                    <img src="/images/service-gen pest.jpg" alt="General Pest Control" class="img-fluid">
                    <h4>General Pest Control</h4>
                </div>
            </div>

            <div class="col-lg-2 col-md-3 col-sm-4 col-6 fade-in">
                <div class="service-card text-center">
                    <img src="/images/service-reticulation.jpg" alt="Reticulation System" class="img-fluid">
                    <h4>Reticulation System</h4>
                </div>
            </div>

            <div class="col-lg-2 col-md-3 col-sm-4 col-6 fade-in">
                <div class="service-card text-center">
                    <img src="/images/service-soil.jpg" alt="Soil Poisoning" class="img-fluid">
                    <h4>Soil Poisoning</h4>
                </div>
            </div>
        </div>
    </div>
</section>



    <section class="hero">
    <div class="container">
        <div class="row align-items-center">
            <!-- Left Side - Image -->
            <div class="col-md-6 text-center">
                <img src="/images/ppe.png" alt="Pest Control Worker" class="img-fluid hero-img">
            </div>

            <!-- Right Side - Content -->
           <div class="col-md-6">

               <h1> R.R.C. Termite and Pest Control</h1>
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


<section class="hero" style="position: relative; width: 100%; height: 100vh; overflow: hidden; background-color: black;">
    <!-- Video Thumbnail -->
    <img id="videoThumbnail" src="/images/banner tv.jpg" 
        alt="Video Thumbnail" 
        style="position: absolute; top: 0; left: 0; width: 100%; height: 100%; object-fit: cover; cursor: pointer;">

    <!-- Vimeo Embedded Video (Initially Hidden) -->
   <iframe id="vimeoVideo" 
    src="https://player.vimeo.com/video/1009218555?loop=1&muted=0"
    style="position: absolute; top: 0; left: 0; width: 100%; height: 100%; border: none; display: none;"
    frameborder="0" allow="autoplay; fullscreen" allowfullscreen>
</iframe>


    <!-- Play Button -->
    <div id="playButton" 
        style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
               background: rgba(0, 0, 0, 0.7); color: white; padding: 20px 30px; border-radius: 50px;
               font-size: 1.5rem; cursor: pointer; text-align: center;">
        ▶ Play
    </div>

    <!-- Title at the Top -->
    <h1 style="position: absolute; top: 5%; left: 50%; transform: translateX(-50%);
               color: white; font-size: 3rem; text-shadow: 2px 2px 10px rgba(0, 0, 0, 0.7);">
        News and Announcements
    </h1>
</section>

<script>
    const playButton = document.getElementById("playButton");
    const videoThumbnail = document.getElementById("videoThumbnail");
    const vimeoVideo = document.getElementById("vimeoVideo");

    playButton.addEventListener("click", function () {
        // Replace src to trigger autoplay
        const src = vimeoVideo.getAttribute("src");
        vimeoVideo.setAttribute("src", src + "&autoplay=1");

        videoThumbnail.style.display = "none"; // Hide thumbnail
        playButton.style.display = "none";     // Hide play button
        vimeoVideo.style.display = "block";    // Show video
    });
</script>

<section style="text-align: center; padding: 40px 0;">
    <h2 style="color: #0B2A63; font-size: 2rem; font-weight: bold;">
         Certifications & Organizations
    </h2>
    
    <div style="display: flex; justify-content: center; align-items: center; margin-top: 20px;">
        <img src="/images/c&o.png" alt="Certifications & Organizations" 
             style="max-width: 90%; height: auto;">
    </div>
</section>


<section style="text-align: center; padding: 40px 20px; font-family: Arial, sans-serif;">
    <h2 style="font-size: 24px; font-weight: bold; color: #121481;">What our customers are saying</h2>

    <div style="display: flex; flex-wrap: wrap; justify-content: center; gap: 20px; margin-top: 20px;">
        
        <!-- Review 1 -->
        <div style="width: 250px; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.2);">
            <strong>Czarina Joy T. Chang</strong>
            <p style="font-size: 12px; color: red;">❤️ recommends</p>
            <p style="font-size: 14px; color: #333;">They were on time, very professional and mababait mga staff ni RRC team. Mabusisi sila sa bawat sulok ng bahay at maayos silang magtrabaho. Very polite and courteous pa yun technicians and staff na nag execute ng baiting system. Highly recommended! Good job!</p>
        </div>

        <!-- Review 2 -->
        <div style="width: 250px; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.2);">
            <strong>Mel Lareza</strong>
            <p style="font-size: 12px; color: red;">❤️ recommends</p>
            <p style="font-size: 14px; color: #333;">Excellent Service!!! ⭐️⭐️⭐️⭐️⭐️</p>
        </div>

        <!-- Review 3 -->
        <div style="width: 250px; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.2);">
            <strong>Dannica Manaluz.</strong>
            <p style="font-size: 12px; color: red;">❤️ recommends</p>
            <p style="font-size: 14px; color: #333;">Very satisfied client here 5/5 stars ⭐️⭐️⭐️⭐️⭐️</p>
        </div>

        <!-- Review 4 -->
        <div style="width: 250px; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.2);">
            <strong>Chelsea Erese Ong</strong>
            <p style="font-size: 12px; color: red;">❤️ recommends</p>
            <p style="font-size: 14px; color: #333;">RRC/Sir Victor and staff of technicians were very accommodating and understanding despite us having to change schedule of termite treatment...</p>
        </div>

    </div>
</section>

    
<!-- SweetAlert2 -->
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>


<section style="display: flex; justify-content: center; align-items: center; min-height: 100vh; background: white; font-family: 'Segoe UI', sans-serif;">
        <div style="display: flex; max-width: 1000px; width: 100%; border-radius: 12px; overflow: hidden; background: white; box-shadow: 0 8px 20px rgba(0,0,0,0.2);">

            <!-- Left Side - Form -->
            <div style="flex: 1; padding: 40px; background: #63b3ed;">
                <h3 style="text-transform: uppercase; font-size: 14px; font-weight: 600; color: white;">Schedule Your</h3>
                <h1 style="font-size: 28px; font-weight: 700; color: #1a202c; margin-bottom: 20px;">Free Inspection</h1>
                <p style="font-size: 14px; color: #1a202c; margin-bottom: 25px;">Schedule today! Please fill-in this form and RRC Pest and Termite Control Representative will contact you soon.</p>

               <!-- Email -->
<div style="margin-bottom: 15px;">
    <asp:Label runat="server" AssociatedControlID="txtEmail" Text="Your email *" />
    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"
        placeholder="email@gmail.com" required
        style="width: 100%; border: none; border-bottom: 2px solid #1a202c; padding: 10px; background: transparent; color: #1a202c; font-size: 14px;" />
    
    <!-- Email domain validator -->
    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
        ErrorMessage="Please enter a valid Gmail, Yahoo, or Outlook email address."
        ForeColor="Red"
        Display="Dynamic"
        ValidationExpression="^[a-zA-Z0-9._%+-]+@(gmail\.com|yahoo\.com|outlook\.com)$" />
</div>


                <!-- Contact -->
                <asp:TextBox ID="txtContactNumber" runat="server" CssClass="form-control"
    placeholder="09xxxxxxxxx" required MaxLength="11"
    onkeypress="return isDigit(event)"
    onkeydown="return blockNonDigits(event)"
    oninput="validateContactNumber(this)"
    onpaste="handlePaste(event)"
    style="width: 100%; border: none; border-bottom: 2px solid #1a202c; padding: 10px; background: transparent; color: #1a202c; font-size: 14px;" />



                <!-- Photo Upload -->
                <div style="margin-bottom: 15px;">
                    <asp:Label runat="server" AssociatedControlID="fuPestPhoto" Text="Photo of Pest (optional)" />
                    <asp:FileUpload ID="fuPestPhoto" runat="server"
    style="width: 100%; background: white; padding: 8px; border-radius: 4px;"
    accept=".png,.jpg,.jpeg,image/png,image/jpeg" />

                    <small style="color: #1a202c;">Upload a photo if available.</small>
                </div>

                <!-- Message -->
                <div style="margin-bottom: 15px;">
                    <asp:Label runat="server" AssociatedControlID="txtMessage" Text="Describe what you observed*" />
                    <asp:TextBox ID="txtMessage" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4"
                        placeholder="Describe what you observed..." required
                        style="width: 100%; border-radius: 4px; padding: 10px; font-size: 14px;" />
                </div>

<!-- Trigger Link -->
<div style="margin-bottom: 10px;">
    <a href="#" data-bs-toggle="modal" data-bs-target="#termsModal" style="font-size: 14px; color: #1a202c; text-decoration: underline;">
        View Terms and Conditions
    </a>
</div>

<!-- Terms & Conditions Modal -->
<div class="modal fade" id="termsModal" tabindex="-1" aria-labelledby="termsModalLabel" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered">
    <div class="modal-content">

      <div class="modal-header">
        <h5 class="modal-title" id="termsModalLabel">Terms and Conditions</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>

      <div class="modal-body" style="color: #1a202c;">
        <p><strong>1.</strong> By submitting this form, you agree to our pest inspection process.</p>
        <p><strong>2.</strong> We collect your data to coordinate inspections and services.</p>
        <p><strong>3.</strong> Uploaded photos will only be used to assess the pest problem.</p>

        <div style="margin-top: 10px;">
            <asp:CheckBox ID="chkTerms" runat="server" />
            <label for="chkTerms">I agree to the terms and conditions stated above.</label>
        </div>
      </div>

      <div class="modal-footer">
        <button type="button" class="btn btn-primary" data-bs-dismiss="modal">OK</button>
      </div>

    </div>
  </div>
</div>


                <!-- Submit Button -->
                <asp:Button ID="btnSubmitInquiry" runat="server" Text="Submit Inquiry" CssClass="btn-submit"
                    OnClick="btnSubmitInquiry_Click"
                    Style="width: 100%; padding: 12px; background-color: #1a202c; color: white; border: none; border-radius: 4px; font-weight: bold; font-size: 16px;" />
            </div>

            <!-- Right Side Image -->
            <div style="flex: 1; background: url('/Images/service-baiting.jpg') center center / cover no-repeat;"></div>
        </div>
    </section>  



<!-- JavaScript for Toggle Functionality -->
<script>
    function toggleTerms() {
        var termsDiv = document.getElementById("termsContent");
        if (termsDiv.style.display === "none" || termsDiv.style.display === "") {
            termsDiv.style.display = "block";
        } else {
            termsDiv.style.display = "none";
        }
    }

    function isDigit(e) {
        const charCode = e.which || e.keyCode;
        // Allow digits, backspace, arrow keys
        return (charCode >= 48 && charCode <= 57);
    }

    function blockNonDigits(e) {
        const key = e.key;
        const isCtrlOrCmd = e.ctrlKey || e.metaKey;

        // Allow backspace, delete, arrows, tab, home, end
        const allowedKeys = ["Backspace", "Delete", "ArrowLeft", "ArrowRight", "Tab", "Home", "End"];
        if (allowedKeys.includes(key) || isCtrlOrCmd) return true;

        // Block non-numeric keys
        return /^\d$/.test(key);
    }

    function validateContactNumber(input) {
        // Remove non-digit characters
        input.value = input.value.replace(/\D/g, '');

        // Enforce max 11 digits
        if (input.value.length > 11) {
            input.value = input.value.slice(0, 11);
        }

        // Enforce starting with 09
        if (input.value.length > 0 && !input.value.startsWith("09")) {
            input.setCustomValidity("Contact number must start with 09.");
        } else {
            input.setCustomValidity("");
        }
    }

    function handlePaste(e) {
        const paste = (e.clipboardData || window.clipboardData).getData('text');
        if (!/^09\d{0,9}$/.test(paste)) {
            e.preventDefault();
        }
    }

    function acceptCookies() {
        const checkbox = document.getElementById('chkCookiePolicy');
        if (checkbox.checked) {
            // Commented out so it won't save to localStorage for now
             localStorage.setItem('cookieConsentAccepted', 'true');

            // Fade out smoothly
            const banner = document.getElementById('cookieConsentBanner');
            banner.classList.remove('show');
            banner.classList.add('hide');

            // Optional: completely hide after animation finishes
            setTimeout(function () {
                banner.style.display = 'none';
            }, 800); // match the fade-out time
        } else {
            alert('Please check "I accept the cookie policy" before proceeding.');
        }
    }

    function toggleTerms() {
        const box = document.getElementById("termsContent");
        box.style.display = box.style.display === "none" ? "block" : "none";
    }

    function toggleCookiePolicy() {
        document.getElementById('cookiePolicyOverlay').style.display = 'block';
    }

    function closeCookiePolicyModal() {
        document.getElementById('cookiePolicyOverlay').style.display = 'none';
    }

    // 🆕 Close when clicking outside the modal
    window.addEventListener('click', function (event) {
        var modal = document.getElementById('cookiePolicyModal');
        var overlay = document.getElementById('cookiePolicyOverlay');

        if (event.target === overlay) {
            overlay.style.display = 'none';
        }
    });



</script>

   <div id="cookieConsentBanner">
    <span style="font-size: 16px;">🍪 This website uses cookies to ensure you get the best experience. 
        <a href="#" onclick="toggleCookiePolicy(); return false;" style="color: #007bff; text-decoration: underline;">Learn more</a>.
    </span>
    <br><br>
    <div>
        <label style="font-size: 14px; cursor: pointer;">
            <input type="checkbox" id="chkCookiePolicy" style="
                margin-right: 8px;
                accent-color: #007bff;
                width: 18px;
                height: 18px;
                cursor: pointer;
            "> I accept the cookie policy.
        </label>
    </div>
    <div style="margin-top: 15px;">
      <button type="button" onclick="acceptCookies();" 
    style="
        background: #007bff; 
        color: white; 
        border: none; 
        padding: 10px 20px; 
        border-radius: 6px; 
        cursor: pointer; 
        font-weight: bold;
        font-size: 14px;
    ">
    Accept
</button>
    </div>
</div>


<style>
    #cookieConsentBanner {
        position: fixed;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        background: rgba(255, 255, 255, 0.95);
        color: #333;
        padding: 30px;
        text-align: center;
        font-size: 14px;
        z-index: 9999;
        border: 2px solid #007bff;
        border-radius: 10px;
        box-shadow: 0px 4px 15px rgba(0, 0, 0, 0.1);
        opacity: 0;
        visibility: hidden;
        transition: opacity 0.6s ease, visibility 0.6s ease;
    }

    #cookieConsentBanner.show {
        opacity: 1;
        visibility: visible;
    }

    #cookieConsentBanner.hide {
        opacity: 0;
        visibility: hidden;
        transition: opacity 0.8s ease; /* smoother fade out */
    }

    #cookieConsentBanner button:hover {
        background-color: #0056b3;
    }
</style>

<div id="cookiePolicyOverlay" style="
    display: none;
    position: fixed;
    top: 0; left: 0; right: 0; bottom: 0;
    background: rgba(0, 0, 0, 0.5); /* 🟰 dark transparent */
    z-index: 9999;
">
    <!-- Modal Box -->
    <div id="cookiePolicyModal" style="
        position: absolute;
        top: 50%; left: 50%;
        transform: translate(-50%, -50%);
        background: white;
        padding: 25px;
        border-radius: 10px;
        box-shadow: 0 4px 15px rgba(0,0,0,0.3);
        width: 90%;
        max-width: 500px;
        max-height: 80vh;
        overflow-y: auto;
    ">
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
                style="
                    background: #007bff; 
                    color: white; 
                    border: none; 
                    padding: 10px 20px; 
                    border-radius: 5px; 
                    font-weight: bold;
                    cursor: pointer;
                ">
                Close
            </button>
        </div>
    </div>
</div>





</asp:Content>