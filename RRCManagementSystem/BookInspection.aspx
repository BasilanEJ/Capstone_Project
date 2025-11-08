<%@ Page Title="Book Free Inspection" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="BookInspection.aspx.cs" Inherits="RRCManagementSystem.BookInspection" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css">
    <script src="https://cdn.jsdelivr.net/npm/flatpickr"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    
    <style>
        /* Custom Styles */
        .booking-container {
            max-width: 900px;
            margin: 0 auto;
        }

        .form-section {
            background: white;
            border-radius: 16px;
            padding: 32px;
            margin-bottom: 24px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            border: 1px solid #e5e7eb;
        }

        .section-title {
            font-size: 20px;
            font-weight: 700;
            color: #1f2937;
            margin-bottom: 20px;
            padding-bottom: 12px;
            border-bottom: 2px solid #3b82f6;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .section-title i {
            color: #3b82f6;
            font-size: 22px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-label {
            display: block;
            font-size: 14px;
            font-weight: 600;
            color: #374151;
            margin-bottom: 8px;
        }

        .form-label .required {
            color: #ef4444;
            margin-left: 4px;
        }

        .form-control {
            width: 100%;
            padding: 12px 16px;
            border: 2px solid #e5e7eb;
            border-radius: 10px;
            font-size: 15px;
            transition: all 0.3s ease;
            background: #f9fafb;
        }

        .form-control:focus {
            outline: none;
            border-color: #3b82f6;
            background: white;
            box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
        }

        .form-control:disabled {
            background: #e5e7eb;
            cursor: not-allowed;
        }

        textarea.form-control {
            min-height: 120px;
            resize: vertical;
        }

        .form-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 20px;
        }

        @media (max-width: 768px) {
            .form-grid {
                grid-template-columns: 1fr;
            }
            
            .form-section {
                padding: 24px 20px;
            }
        }

        /* Availability Badge */
        .availability-badge {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 13px;
            font-weight: 600;
            margin-top: 8px;
        }

        .availability-badge.available {
            background: #d1fae5;
            color: #065f46;
        }

        .availability-badge.limited {
            background: #fef3c7;
            color: #92400e;
        }

        .availability-badge.unavailable {
            background: #fee2e2;
            color: #991b1b;
        }

        .availability-badge.checking {
            background: #dbeafe;
            color: #1e40af;
        }

        /* Submit Button */
        .btn-submit {
            width: 100%;
            padding: 16px;
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            border: none;
            border-radius: 12px;
            font-size: 16px;
            font-weight: 700;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 16px rgba(16, 185, 129, 0.3);
        }

        .btn-submit:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 6px 24px rgba(16, 185, 129, 0.4);
        }

        .btn-submit:disabled {
            background: #9ca3af;
            cursor: not-allowed;
            box-shadow: none;
        }

        /* Info Box */
        .info-box {
            background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
            border-left: 4px solid #3b82f6;
            padding: 16px;
            border-radius: 8px;
            margin-bottom: 24px;
        }

        .info-box-title {
            font-weight: 700;
            color: #1e40af;
            margin-bottom: 8px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .info-box-content {
            color: #1e3a8a;
            font-size: 14px;
            line-height: 1.6;
        }

        .info-box-content ul {
            margin: 8px 0 0 20px;
            padding: 0;
        }

        .info-box-content li {
            margin-bottom: 4px;
        }

        /* Radio Styling */
        .radio-group {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
        }

        .radio-option {
            flex: 1;
            min-width: 140px;
        }

        .radio-option input[type="radio"] {
            display: none;
        }

        .radio-label {
            display: block;
            padding: 12px 16px;
            border: 2px solid #e5e7eb;
            border-radius: 10px;
            cursor: pointer;
            transition: all 0.3s ease;
            text-align: center;
            font-weight: 500;
            background: #f9fafb;
        }

        .radio-option input[type="radio"]:checked + .radio-label {
            border-color: #3b82f6;
            background: #eff6ff;
            color: #1e40af;
        }

        .radio-label:hover {
            border-color: #3b82f6;
            background: white;
        }

        /* Loading Spinner */
        .spinner {
            display: inline-block;
            width: 16px;
            height: 16px;
            border: 2px solid rgba(255, 255, 255, 0.3);
            border-radius: 50%;
            border-top-color: white;
            animation: spin 0.8s linear infinite;
        }

        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        /* Free Badge */
        .free-badge {
            display: inline-block;
            background: linear-gradient(135deg, #10b981, #059669);
            color: white;
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 700;
            margin-left: 8px;
            animation: pulse-badge 2s infinite;
        }

        @keyframes pulse-badge {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.05); }
        }

        /* Image Upload Styling */
        .image-upload-container {
            border: 2px dashed #cbd5e0;
            border-radius: 12px;
            padding: 24px;
            text-align: center;
            background: #f9fafb;
            transition: all 0.3s ease;
            cursor: pointer;
            position: relative;
        }

        .image-upload-container:hover {
            border-color: #3b82f6;
            background: #eff6ff;
        }

        .image-upload-container.drag-over {
            border-color: #10b981;
            background: #d1fae5;
        }

        .upload-icon {
            font-size: 48px;
            color: #94a3b8;
            margin-bottom: 12px;
        }

        .upload-text {
            color: #64748b;
            font-size: 14px;
            margin-bottom: 8px;
        }

        .upload-hint {
            color: #94a3b8;
            font-size: 12px;
        }

        .file-input-hidden {
            position: absolute;
            width: 100%;
            height: 100%;
            top: 0;
            left: 0;
            opacity: 0;
            cursor: pointer;
        }

        /* Image Preview */
        .image-preview-container {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
            margin-top: 16px;
        }

        .image-preview-item {
            position: relative;
            width: 120px;
            height: 120px;
            border-radius: 8px;
            overflow: hidden;
            border: 2px solid #e5e7eb;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
        }

        .image-preview-item img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

        .image-preview-remove {
            position: absolute;
            top: 4px;
            right: 4px;
            background: #ef4444;
            color: white;
            border: none;
            border-radius: 50%;
            width: 24px;
            height: 24px;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 14px;
            transition: all 0.3s ease;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
        }

        .image-preview-remove:hover {
            background: #dc2626;
            transform: scale(1.1);
        }

        .image-preview-name {
            position: absolute;
            bottom: 0;
            left: 0;
            right: 0;
            background: rgba(0, 0, 0, 0.7);
            color: white;
            font-size: 10px;
            padding: 4px;
            text-align: center;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        /* File Size Indicator */
        .file-info {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 12px;
            background: #f1f5f9;
            border-radius: 6px;
            margin-top: 8px;
            font-size: 12px;
            color: #64748b;
        }

        .file-info-success {
            background: #d1fae5;
            color: #065f46;
        }

        .file-info-error {
            background: #fee2e2;
            color: #991b1b;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="booking-container">
        
        <!-- Page Header -->
        <div class="text-center mb-8">
            <h1 class="text-4xl font-black text-gray-900 mb-3">
                <i class="fas fa-calendar-check text-blue-600"></i>
                Book Free Inspection
                <span class="free-badge">100% FREE</span>
            </h1>
            <p class="text-gray-600 text-lg">Schedule your professional pest inspection today</p>
        </div>

        <!-- Info Box -->
        <div class="info-box">
            <div class="info-box-title">
                <i class="fas fa-info-circle"></i>
                What to Expect
            </div>
            <div class="info-box-content">
                <ul>
                    <li>✅ <strong>100% Free</strong> - No hidden charges</li>
                    <li>✅ <strong>Certified Inspectors</strong> - Licensed professionals</li>
                    <li>✅ <strong>Detailed Report</strong> - Comprehensive assessment</li>
                    <li>✅ <strong>Free Quotation</strong> - Transparent pricing</li>
                </ul>
            </div>
        </div>

        <asp:UpdatePanel ID="upBooking" runat="server" UpdateMode="Conditional">
            <ContentTemplate>

                <!-- Section 1: Inspection Date & Time -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-calendar-alt"></i>
                        Select Inspection Date & Time
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            Preferred Inspection Date <span class="required">*</span>
                        </label>
                        <asp:TextBox ID="txtInspectionDate" runat="server" 
                            CssClass="form-control" 
                            placeholder="Click to select date"
                            AutoComplete="off" />
                        <asp:HiddenField ID="hfInspectionDate" runat="server" />
                        <asp:HiddenField ID="hfClientRegion" runat="server" />
                        <asp:HiddenField ID="hfClientCity" runat="server" />
                        
                        <!-- Availability Indicator -->
                        <div id="availabilityIndicator" style="display: none;">
                            <div class="availability-badge checking">
                                <span class="spinner"></span>
                                <span>Checking availability...</span>
                            </div>
                        </div>
                        <asp:Label ID="lblAvailability" runat="server" CssClass="availability-badge" 
                            Style="display: none;"></asp:Label>
                    </div>

<div class="form-group">
    <label class="form-label">
        Preferred Time Slot <span class="required">*</span>
        <span class="free-badge" style="font-size: 10px; padding: 2px 8px;">5 AM - 10 PM</span>
    </label>
    <div class="radio-group">
        <div class="radio-option">
            <asp:RadioButton ID="rb5AM8AM" runat="server" GroupName="TimeSlot" />
            <label for="<%= rb5AM8AM.ClientID %>" class="radio-label">
                🌅 5:00 AM - 8:00 AM<br/>
                <small>Early Morning</small>
            </label>
        </div>
        <div class="radio-option">
            <asp:RadioButton ID="rb8AM11AM" runat="server" GroupName="TimeSlot" Checked="true" />
            <label for="<%= rb8AM11AM.ClientID %>" class="radio-label">
                ☀️ 8:00 AM - 11:00 AM<br/>
                <small>Morning</small>
            </label>
        </div>
        <div class="radio-option">
            <asp:RadioButton ID="rb11AM2PM" runat="server" GroupName="TimeSlot" />
            <label for="<%= rb11AM2PM.ClientID %>" class="radio-label">
                ☀️ 11:00 AM - 2:00 PM<br/>
                <small>Midday</small>
            </label>
        </div>
        <div class="radio-option">
            <asp:RadioButton ID="rb2PM5PM" runat="server" GroupName="TimeSlot" />
            <label for="<%= rb2PM5PM.ClientID %>" class="radio-label">
                🌤️ 2:00 PM - 5:00 PM<br/>
                <small>Afternoon</small>
            </label>
        </div>
        <div class="radio-option">
            <asp:RadioButton ID="rb5PM8PM" runat="server" GroupName="TimeSlot" />
            <label for="<%= rb5PM8PM.ClientID %>" class="radio-label">
                🌆 5:00 PM - 8:00 PM<br/>
                <small>Evening</small>
            </label>
        </div>
        <div class="radio-option">
            <asp:RadioButton ID="rb8PM10PM" runat="server" GroupName="TimeSlot" />
            <label for="<%= rb8PM10PM.ClientID %>" class="radio-label">
                🌃 8:00 PM - 10:00 PM<br/>
                <small>Night</small>
            </label>
        </div>
    </div>
</div>
                </div>

                <!-- Section 2: Inspection Address (Read-Only) -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-map-marker-alt"></i>
                        Inspection Address
                    </div>

                    <div class="info-box" style="background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%); border-left-color: #10b981;">
                        <div class="info-box-title" style="color: #065f46;">
                            <i class="fas fa-check-circle"></i>
                            Using Your Registered Address
                        </div>
                        <div class="info-box-content" style="color: #047857;">
                            We'll use the address registered in your account.
                        </div>
                    </div>

                    <div class="form-grid">
                        <div class="form-group">
                            <label class="form-label">Street & Unit</label>
                            <asp:TextBox ID="txtStreet" runat="server" 
                                CssClass="form-control" 
                                ReadOnly="true" 
                                placeholder="Loading..." />
                        </div>

                        <div class="form-group">
                            <label class="form-label">Barangay</label>
                            <asp:TextBox ID="txtBarangay" runat="server" 
                                CssClass="form-control" 
                                ReadOnly="true" 
                                placeholder="Loading..." />
                        </div>

                        <div class="form-group">
                            <label class="form-label">City</label>
                            <asp:TextBox ID="txtCity" runat="server" 
                                CssClass="form-control" 
                                ReadOnly="true" 
                                placeholder="Loading..." />
                        </div>

                        <div class="form-group">
                            <label class="form-label">Region</label>
                            <asp:TextBox ID="txtRegion" runat="server" 
                                CssClass="form-control" 
                                ReadOnly="true" 
                                placeholder="Loading..." />
                        </div>
                    </div>

                    <div class="form-group">
                        <label class="form-label">Landmark (If any)</label>
                        <asp:TextBox ID="txtLandmark" runat="server" 
                            CssClass="form-control" 
                            ReadOnly="true" 
                            placeholder="None specified" />
                    </div>
                </div>

                <!-- Section 3: Pest Problem Details -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-bug"></i>
                        Pest Problem Details
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            Type of Pest <span class="required">*</span>
                        </label>
                        <asp:DropDownList ID="ddlPestType" runat="server" CssClass="form-control">
                            <asp:ListItem Value="">-- Select Pest Type --</asp:ListItem>
                            <asp:ListItem Value="Termites">🐜 Termites</asp:ListItem>
                            <asp:ListItem Value="Rodents">🐀 Rodents (Rats/Mice)</asp:ListItem>
                            <asp:ListItem Value="Cockroaches">🪳 Cockroaches</asp:ListItem>
                            <asp:ListItem Value="Ants">🐜 Ants</asp:ListItem>
                            <asp:ListItem Value="Bed Bugs">🛏️ Bed Bugs</asp:ListItem>
                            <asp:ListItem Value="Mosquitoes">🦟 Mosquitoes</asp:ListItem>
                            <asp:ListItem Value="Flies">🪰 Flies</asp:ListItem>
                            <asp:ListItem Value="Other">❓ Other</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            Problem Description <span class="required">*</span>
                        </label>
                        <asp:TextBox ID="txtProblemDescription" runat="server" 
                            TextMode="MultiLine" 
                            CssClass="form-control" 
                            placeholder="Please describe the pest problem in detail (e.g., location, severity, duration, any visible damage)"
                            MaxLength="1000" />
                        <small class="text-gray-500">Maximum 1000 characters</small>
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            Urgency Level <span class="required">*</span>
                        </label>
                        <div class="radio-group">
                            <div class="radio-option">
                                <asp:RadioButton ID="rbLow" runat="server" GroupName="Urgency" />
                                <label for="<%= rbLow.ClientID %>" class="radio-label">
                                    🟢 Low<br/>
                                    <small>Can wait</small>
                                </label>
                            </div>
                            <div class="radio-option">
                                <asp:RadioButton ID="rbMedium" runat="server" GroupName="Urgency" Checked="true" />
                                <label for="<%= rbMedium.ClientID %>" class="radio-label">
                                    🟡 Medium<br/>
                                    <small>Within week</small>
                                </label>
                            </div>
                            <div class="radio-option">
                                <asp:RadioButton ID="rbHigh" runat="server" GroupName="Urgency" />
                                <label for="<%= rbHigh.ClientID %>" class="radio-label">
                                    🔴 High<br/>
                                    <small>Urgent</small>
                                </label>
                            </div>
                            <div class="radio-option">
                                <asp:RadioButton ID="rbEmergency" runat="server" GroupName="Urgency" />
                                <label for="<%= rbEmergency.ClientID %>" class="radio-label">
                                    🚨 Emergency<br/>
                                    <small>ASAP</small>
                                </label>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Section 4: Upload Supporting Images -->
                <div class="form-section">
                    <div class="section-title">
                        <i class="fas fa-camera"></i>
                        Upload Supporting Images (Optional)
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            Upload Photos of the Pest Problem
                            <small class="text-gray-500">(JPEG/PNG only, Max 5MB per file, Up to 5 images)</small>
                        </label>
                        
                        <div id="imageUploadContainer" class="image-upload-container">
                            <asp:FileUpload ID="fuImages" runat="server" 
                                CssClass="file-input-hidden" 
                                AllowMultiple="true" 
                                accept=".jpg,.jpeg,.png"
                                onchange="handleFileSelect(this)" />
                            <div class="upload-icon">📷</div>
                            <div class="upload-text">
                                <strong>Click to upload</strong> or drag and drop
                            </div>
                            <div class="upload-hint">
                                JPEG or PNG (Max 5MB per file, Up to 5 images)
                            </div>
                        </div>

                        <!-- Image Preview Container -->
                        <div id="imagePreviewContainer" class="image-preview-container"></div>
                        
                        <!-- File Info -->
                        <div id="fileInfo" style="display: none;"></div>
                    </div>
                </div>

                <!-- Submit Button -->
                <div class="form-section">
                    <asp:Button ID="btnSubmit" runat="server" 
                        CssClass="btn-submit" 
                        Text="📅 Submit Inspection Request" 
                        OnClick="btnSubmit_Click"
                        OnClientClick="return validateForm();" />
                    
                    <div class="text-center mt-4 text-gray-500 text-sm">
                        <i class="fas fa-lock"></i>
                        Your information is secure and encrypted
                    </div>
                </div>

            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btnSubmit" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

<script>
    // Store current selections
    let currentDate = null;
    let currentTimeSlot = null;
    let clientRegion = null;
    let clientCity = null;

    // ===== Disable past time slots (GLOBAL FUNCTION) =====
    function disablePastTimeSlots(selectedDate) {
        const now = new Date();
        const selected = new Date(selectedDate);
        const isToday =
            now.getFullYear() === selected.getFullYear() &&
            now.getMonth() === selected.getMonth() &&
            now.getDate() === selected.getDate();

        if (!isToday) return; // Only process for today's date

        const currentHour = now.getHours();

        const slotRules = [
            { id: '<%= rb5AM8AM.ClientID %>', start: 5, end: 8, label: '🌅 5:00 AM - 8:00 AM<br/><small>Early Morning</small>' },
            { id: '<%= rb8AM11AM.ClientID %>', start: 8, end: 11, label: '☀️ 8:00 AM - 11:00 AM<br/><small>Morning</small>' },
            { id: '<%= rb11AM2PM.ClientID %>', start: 11, end: 14, label: '☀️ 11:00 AM - 2:00 PM<br/><small>Midday</small>' },
            { id: '<%= rb2PM5PM.ClientID %>', start: 14, end: 17, label: '🌤️ 2:00 PM - 5:00 PM<br/><small>Afternoon</small>' },
            { id: '<%= rb5PM8PM.ClientID %>', start: 17, end: 20, label: '🌆 5:00 PM - 8:00 PM<br/><small>Evening</small>' },
            { id: '<%= rb8PM10PM.ClientID %>', start: 20, end: 22, label: '🌃 8:00 PM - 10:00 PM<br/><small>Night</small>' }
        ];

        slotRules.forEach(slot => {
            const radio = document.getElementById(slot.id);
            const label = document.querySelector(`label[for="${slot.id}"]`);
            if (!radio || !label) return;

            if (currentHour >= slot.end) {
                // Time has passed - force disable
                radio.disabled = true;
                radio.checked = false; // Uncheck if it was selected
                label.style.opacity = '0.5';
                label.style.cursor = 'not-allowed';
                label.style.background = '#fee2e2';
                label.style.borderColor = '#fca5a5';
                label.innerHTML = slot.label.split('<br/>')[0] +
                    `<br/><small style="color:#dc2626;">⏰ Time Passed</small>`;
            }
        });
    }

    // ===== Initialize Date Picker =====
    document.addEventListener('DOMContentLoaded', function () {
        // Get client's region and city from hidden fields
        clientRegion = document.getElementById('hfClientRegion')?.value || '';
        clientCity = document.getElementById('hfClientCity')?.value || '';

        // Also try from textboxes if hidden fields don't exist
        if (!clientRegion) {
            clientRegion = document.getElementById('<%= txtRegion.ClientID %>')?.value || '';
        }
        if (!clientCity) {
            clientCity = document.getElementById('<%= txtCity.ClientID %>')?.value || '';
        }

        console.log('Client Location:', clientRegion, clientCity); // Debug

        const datePicker = flatpickr('#<%= txtInspectionDate.ClientID %>', {
            minDate: new Date().fp_incr(-0), // ✅ allows today
            maxDate: new Date().fp_incr(90),
            dateFormat: 'Y-m-d',
            onChange: function (selectedDates, dateStr, instance) {
                if (selectedDates.length > 0) {
                    document.getElementById('<%= hfInspectionDate.ClientID %>').value = dateStr;
                    currentDate = dateStr;
                    disablePastTimeSlots(dateStr);
                    checkDateAvailability(dateStr);
                }
            }
        });

        // Add event listeners to all time slot radio buttons
        attachTimeSlotListeners();
    });

    // ===== Attach listeners to time slot radio buttons =====
    function attachTimeSlotListeners() {
        const timeSlots = [
            { radio: '<%= rb5AM8AM.ClientID %>', id: 1 },
            { radio: '<%= rb8AM11AM.ClientID %>', id: 2 },
            { radio: '<%= rb11AM2PM.ClientID %>', id: 3 },
            { radio: '<%= rb2PM5PM.ClientID %>', id: 4 },
            { radio: '<%= rb5PM8PM.ClientID %>', id: 5 },
            { radio: '<%= rb8PM10PM.ClientID %>', id: 6 }
        ];

        timeSlots.forEach(slot => {
            const radio = document.getElementById(slot.radio);
            if (radio) {
                radio.addEventListener('change', function () {
                    if (this.checked && currentDate) {
                        currentTimeSlot = slot.id;
                        checkTimeSlotAvailability(currentDate, slot.id);
                    }
                });
            }
        });
    }

    // ===== Check availability for all time slots on selected date =====
    function checkDateAvailability(dateStr) {
        const indicator = document.getElementById('availabilityIndicator');
        const lblAvailability = document.getElementById('<%= lblAvailability.ClientID %>');

        // Validate location data
        if (!clientRegion || !clientCity) {
            lblAvailability.style.display = 'inline-flex';
            lblAvailability.className = 'availability-badge unavailable';
            lblAvailability.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Address incomplete';
            return;
        }

        indicator.style.display = 'block';
        lblAvailability.style.display = 'none';

        fetch('BookInspection.aspx/CheckDateAvailability', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                date: dateStr,
                region: clientRegion,
                city: clientCity
            })
        })
            .then(response => response.json())
            .then(data => {
                indicator.style.display = 'none';

                if (data.d && data.d.Success) {
                    // ✅ Update availability based on DB data
                    updateTimeSlotAvailability(data.d.TimeSlots);

                    // ✅ CRITICAL FIX: Re-apply past time disabling AFTER DB update
                    disablePastTimeSlots(dateStr);

                    // Show overall date availability
                    const hasAvailable = data.d.TimeSlots.some(ts => ts.IsAvailable);
                    lblAvailability.style.display = 'inline-flex';

                    if (hasAvailable) {
                        lblAvailability.className = 'availability-badge available';
                        lblAvailability.innerHTML = '<i class="fas fa-check-circle"></i> Time slots available';
                    } else {
                        lblAvailability.className = 'availability-badge unavailable';
                        lblAvailability.innerHTML = '<i class="fas fa-times-circle"></i> All time slots fully booked';
                    }

                } else if (data.d && data.d.NoInspectors) {
                    // No inspectors service this area
                    lblAvailability.style.display = 'inline-flex';
                    lblAvailability.className = 'availability-badge unavailable';
                    lblAvailability.innerHTML = `<i class="fas fa-map-marker-alt"></i> No inspectors in ${clientCity}`;

                    Swal.fire({
                        icon: 'info',
                        title: 'Service Area Notice',
                        html: data.d.Message,
                        confirmButtonColor: '#3b82f6'
                    });
                } else {
                    lblAvailability.style.display = 'inline-flex';
                    lblAvailability.className = 'availability-badge unavailable';
                    lblAvailability.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Error checking availability';
                }
            })
            .catch(error => {
                console.error('Date availability check error:', error);
                indicator.style.display = 'none';
            });
    }

    // ===== Check specific time slot availability =====
    function checkTimeSlotAvailability(dateStr, timeSlotId) {
        const indicator = document.getElementById('availabilityIndicator');
        const lblAvailability = document.getElementById('<%= lblAvailability.ClientID %>');

        // Validate location data
        if (!clientRegion || !clientCity) {
            return;
        }

        indicator.style.display = 'block';
        lblAvailability.style.display = 'none';

        fetch('BookInspection.aspx/CheckAvailability', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                date: dateStr,
                timeSlotId: timeSlotId,
                region: clientRegion,
                city: clientCity
            })
        })
            .then(response => response.json())
            .then(data => {
                indicator.style.display = 'none';
                lblAvailability.style.display = 'inline-flex';

                const result = data.d;

                if (result.TotalInspectors === 0) {
                    lblAvailability.className = 'availability-badge unavailable';
                    lblAvailability.innerHTML = `<i class="fas fa-map-marker-alt"></i> ${result.Message}`;
                } else if (result.IsAvailable) {
                    if (result.AvailableSlots > 1) {
                        lblAvailability.className = 'availability-badge available';
                        lblAvailability.innerHTML = `<i class="fas fa-check-circle"></i> ${result.Message}`;
                    } else {
                        lblAvailability.className = 'availability-badge limited';
                        lblAvailability.innerHTML = `<i class="fas fa-exclamation-triangle"></i> Only 1 inspector available`;
                    }
                } else {
                    lblAvailability.className = 'availability-badge unavailable';
                    lblAvailability.innerHTML = `<i class="fas fa-times-circle"></i> ${result.Message}`;
                }
            })
            .catch(error => {
                console.error('Time slot availability check error:', error);
                indicator.style.display = 'none';
            });
    }

    // ===== Update time slot radio buttons with availability indicators =====
    function updateTimeSlotAvailability(timeSlots) {
        const slotMapping = [
            { id: 1, radioId: '<%= rb5AM8AM.ClientID %>' },
            { id: 2, radioId: '<%= rb8AM11AM.ClientID %>' },
            { id: 3, radioId: '<%= rb11AM2PM.ClientID %>' },
            { id: 4, radioId: '<%= rb2PM5PM.ClientID %>' },
            { id: 5, radioId: '<%= rb5PM8PM.ClientID %>' },
            { id: 6, radioId: '<%= rb8PM10PM.ClientID %>' }
        ];

        slotMapping.forEach(mapping => {
            const timeSlot = timeSlots.find(ts => ts.TimeSlotID === mapping.id);
            const radio = document.getElementById(mapping.radioId);
            const label = radio ? document.querySelector(`label[for="${mapping.radioId}"]`) : null;

            if (radio && label && timeSlot) {
                // Disable if not available OR no inspectors
                radio.disabled = !timeSlot.IsAvailable || timeSlot.TotalInspectors === 0;
                
                // Update label styling
                if (!timeSlot.IsAvailable || timeSlot.TotalInspectors === 0) {
                    label.style.opacity = '0.5';
                    label.style.cursor = 'not-allowed';
                    label.style.background = '#fee2e2';
                    
                    const displayText = timeSlot.TotalInspectors === 0 
                        ? '❌ No Coverage' 
                        : '❌ Fully Booked';
                    
                    label.innerHTML = label.innerHTML.split('<br/>')[0] + 
                        `<br/><small style="color: #dc2626;">${displayText}</small>`;
                } else {
                    label.style.opacity = '1';
                    label.style.cursor = 'pointer';
                    label.style.background = '';
                    
                    // Add availability count
                    const originalText = label.innerHTML.split('<small>')[0];
                    const slotText = label.innerHTML.match(/<small>(.*?)<\/small>/)?.[1] || '';
                    label.innerHTML = originalText + 
                        `<small>${slotText} (${timeSlot.AvailableSlots}/${timeSlot.TotalInspectors})</small>`;
                }
            }
        });
    }

    // ===== Enhanced Form Validation =====
    function validateForm() {
        const date = document.getElementById('<%= hfInspectionDate.ClientID %>').value;

        const timeSlotChecked =
            document.getElementById('<%= rb5AM8AM.ClientID %>').checked ||
            document.getElementById('<%= rb8AM11AM.ClientID %>').checked ||
            document.getElementById('<%= rb11AM2PM.ClientID %>').checked ||
            document.getElementById('<%= rb2PM5PM.ClientID %>').checked ||
            document.getElementById('<%= rb5PM8PM.ClientID %>').checked ||
            document.getElementById('<%= rb8PM10PM.ClientID %>').checked;
        
        const pestType = document.getElementById('<%= ddlPestType.ClientID %>').value;
        const description = document.getElementById('<%= txtProblemDescription.ClientID %>').value.trim();
        
        const urgencyChecked = 
            document.getElementById('<%= rbLow.ClientID %>').checked ||
            document.getElementById('<%= rbMedium.ClientID %>').checked ||
            document.getElementById('<%= rbHigh.ClientID %>').checked ||
            document.getElementById('<%= rbEmergency.ClientID %>').checked;

        // Validate location
        if (!clientRegion || !clientCity) {
            Swal.fire({
                icon: 'error',
                title: 'Address Required',
                text: 'Your address is incomplete. Please update your profile with your region and city.',
                confirmButtonColor: '#3b82f6'
            });
            return false;
        }

        // Validate date
        if (!date) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Information',
                text: 'Please select an inspection date.',
                confirmButtonColor: '#3b82f6'
            });
            return false;
        }

        // Validate time slot
        if (!timeSlotChecked) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Information',
                text: 'Please select a preferred time slot.',
                confirmButtonColor: '#3b82f6'
            });
            return false;
        }

        // Check if selected time slot is disabled (fully booked or no coverage)
        const selectedSlotRadio = document.querySelector('input[name$="TimeSlot"]:checked');
        if (selectedSlotRadio && selectedSlotRadio.disabled) {
            Swal.fire({
                icon: 'error',
                title: 'Time Slot Unavailable',
                text: 'The selected time slot is not available. Please choose another time.',
                confirmButtonColor: '#3b82f6'
            });
            return false;
        }

        // Validate pest type
        if (!pestType) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Information',
                text: 'Please select the type of pest.',
                confirmButtonColor: '#3b82f6'
            });
            return false;
        }

        // Validate description
        if (!description || description.length < 10) {
            Swal.fire({
                icon: 'warning',
                title: 'Incomplete Description',
                text: 'Please provide a detailed problem description (at least 10 characters).',
                confirmButtonColor: '#3b82f6'
            });
            return false;
        }

        // Validate urgency
        if (!urgencyChecked) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Information',
                text: 'Please select the urgency level.',
                confirmButtonColor: '#3b82f6'
            });
            return false;
        }

        return true;
    }

    // ===== Handle File Selection for Image Upload =====
    function handleFileSelect(input) {
        const files = input.files;
        const previewContainer = document.getElementById('imagePreviewContainer');
        const fileInfo = document.getElementById('fileInfo');
        
        // Clear previous previews
        previewContainer.innerHTML = '';
        
        if (files.length === 0) {
            fileInfo.style.display = 'none';
            return;
        }

        // Validate file count
        if (files.length > 5) {
            Swal.fire({
                icon: 'warning',
                title: 'Too Many Files',
                text: 'You can upload a maximum of 5 images.',
                confirmButtonColor: '#3b82f6'
            });
            input.value = ''; // Clear selection
            fileInfo.style.display = 'none';
            return;
        }

        let validFiles = 0;
        let totalSize = 0;

        // Process each file
        Array.from(files).forEach((file, index) => {
            // Validate file type
            const validTypes = ['image/jpeg', 'image/jpg', 'image/png'];
            if (!validTypes.includes(file.type)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Invalid File Type',
                    text: `"${file.name}" is not a valid image. Only JPEG and PNG are allowed.`,
                    confirmButtonColor: '#3b82f6'
                });
                return;
            }

            // Validate file size (5MB = 5 * 1024 * 1024 bytes)
            if (file.size > 5 * 1024 * 1024) {
                Swal.fire({
                    icon: 'error',
                    title: 'File Too Large',
                    text: `"${file.name}" exceeds the 5MB limit.`,
                    confirmButtonColor: '#3b82f6'
                });
                return;
            }

            validFiles++;
            totalSize += file.size;

            // Create preview
            const reader = new FileReader();
            reader.onload = function(e) {
                const previewItem = document.createElement('div');
                previewItem.className = 'image-preview-item';
                previewItem.innerHTML = `
                    <img src="${e.target.result}" alt="${file.name}">
                    <div class="image-preview-name">${file.name}</div>
                `;
                previewContainer.appendChild(previewItem);
            };
            reader.readAsDataURL(file);
        });

        // Show file info
        if (validFiles > 0) {
            const totalSizeMB = (totalSize / (1024 * 1024)).toFixed(2);
            fileInfo.style.display = 'block';
            fileInfo.className = 'file-info file-info-success';
            fileInfo.innerHTML = `
                <span><i class="fas fa-check-circle"></i> ${validFiles} file(s) selected</span>
                <span>${totalSizeMB} MB total</span>
            `;
        } else {
            fileInfo.style.display = 'none';
            input.value = ''; // Clear invalid selection
        }
    }

    // ===== Drag and Drop Support for Image Upload =====
    document.addEventListener('DOMContentLoaded', function() {
        const uploadContainer = document.getElementById('imageUploadContainer');
        
        if (uploadContainer) {
            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
                uploadContainer.addEventListener(eventName, preventDefaults, false);
            });

            function preventDefaults(e) {
                e.preventDefault();
                e.stopPropagation();
            }

            ['dragenter', 'dragover'].forEach(eventName => {
                uploadContainer.addEventListener(eventName, () => {
                    uploadContainer.classList.add('drag-over');
                }, false);
            });

            ['dragleave', 'drop'].forEach(eventName => {
                uploadContainer.addEventListener(eventName, () => {
                    uploadContainer.classList.remove('drag-over');
                }, false);
            });

            uploadContainer.addEventListener('drop', function(e) {
                const dt = e.dataTransfer;
                const files = dt.files;
                const fileInput = document.getElementById('<%= fuImages.ClientID %>');

                if (fileInput) {
                    fileInput.files = files;
                    handleFileSelect(fileInput);
                }
            }, false);
        }
    });
</script>
</asp:Content>