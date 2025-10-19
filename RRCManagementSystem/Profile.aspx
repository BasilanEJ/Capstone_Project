<%@ Page Title="My Profile" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="RRCManagementSystem.Profile" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Profile Card Animation */
        .profile-container {
            animation: fadeInUp 0.5s ease-out;
        }

        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        /* Profile Picture Container */
        .profile-pic-wrapper {
            position: relative;
            width: 160px;
            height: 160px;
            margin: 0 auto;
            transition: transform 0.3s ease;
        }

        .profile-pic-wrapper:hover {
            transform: scale(1.05);
        }

        .profile-pic {
            width: 100%;
            height: 100%;
            object-fit: cover;
            border-radius: 50%;
            border: 5px solid #3b82f6;
            box-shadow: 0 8px 24px rgba(59, 130, 246, 0.3);
            transition: all 0.3s ease;
        }

        .profile-pic-wrapper:hover .profile-pic {
            border-color: #2563eb;
            box-shadow: 0 12px 32px rgba(59, 130, 246, 0.4);
        }


        .edit-badge {
            position: absolute;
            right: 8px;
            bottom: 8px;
            width: 48px;
            height: 48px;
            border-radius: 50%;
            border: none;
            background: white;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            display: flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .edit-badge:hover {
            background: #3b82f6;
            transform: scale(1.1);
        }

        .edit-badge i {
            font-size: 18px;
            color: #3b82f6;
            transition: color 0.3s ease;
        }

        .edit-badge:hover i {
            color: white;
        }


        .form-field {
            margin-bottom: 1.25rem;
            transition: all 0.3s ease;
        }

        .form-label {
            display: block;
            font-weight: 600;
            font-size: 0.875rem;
            color: #4b5563;
            margin-bottom: 0.5rem;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .form-input-readonly {
            width: 100%;
            padding: 0.875rem 1rem;
            font-size: 0.9375rem;
            color: #1f2937;
            background: #f9fafb;
            border: 2px solid #e5e7eb;
            border-radius: 0.5rem;
            transition: all 0.3s ease;
            cursor: not-allowed;
        }

        .form-input-readonly:focus {
            outline: none;
            border-color: #d1d5db;
            background: #f3f4f6;
        }


        .btn-save {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            padding: 0.875rem 2.5rem;
            font-size: 1rem;
            font-weight: 600;
            border: none;
            border-radius: 0.75rem;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
            position: relative;
            overflow: hidden;
        }

        .btn-save:hover {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(59, 130, 246, 0.4);
        }

        .btn-save:active {
            transform: translateY(0);
        }

        .btn-cancel {
            background: #6b7280;
            color: white;
            padding: 0.875rem 2.5rem;
            font-size: 1rem;
            font-weight: 600;
            border: none;
            border-radius: 0.75rem;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(107, 114, 128, 0.3);
        }

        .btn-cancel:hover {
            background: #4b5563;
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(107, 114, 128, 0.4);
        }


        .info-card {
            background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
            border-left: 4px solid #3b82f6;
            padding: 1rem 1.25rem;
            border-radius: 0.5rem;
            margin-bottom: 1.5rem;
        }

        .info-card i {
            color: #3b82f6;
            margin-right: 0.5rem;
        }


        @media (min-width: 768px) {
            .form-grid {
                display: grid;
                grid-template-columns: repeat(2, 1fr);
                gap: 1.25rem;
            }

            .form-field-full {
                grid-column: 1 / -1;
            }
        }


        .message-success {
            background: #d1fae5;
            color: #065f46;
            padding: 1rem 1.25rem;
            border-radius: 0.5rem;
            border-left: 4px solid #10b981;
            margin-bottom: 1.5rem;
            animation: slideIn 0.3s ease-out;
        }

        .message-error {
            background: #fee2e2;
            color: #991b1b;
            padding: 1rem 1.25rem;
            border-radius: 0.5rem;
            border-left: 4px solid #ef4444;
            margin-bottom: 1.5rem;
            animation: slideIn 0.3s ease-out;
        }

        @keyframes slideIn {
            from {
                opacity: 0;
                transform: translateX(-10px);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }

       
        .sr-only {
            position: absolute;
            width: 1px;
            height: 1px;
            padding: 0;
            margin: -1px;
            overflow: hidden;
            clip: rect(0, 0, 0, 0);
            white-space: nowrap;
            border-width: 0;
        }

        
        .profile-header {
            text-align: center;
            margin-bottom: 2rem;
            padding-bottom: 2rem;
            border-bottom: 2px solid #e5e7eb;
        }

        .profile-title {
            font-size: 1.875rem;
            font-weight: 700;
            color: #1f2937;
            margin-bottom: 0.5rem;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.75rem;
        }

        .profile-subtitle {
            color: #6b7280;
            font-size: 0.9375rem;
        }

        /* Section Divider */
        .section-divider {
            border-top: 2px solid #e5e7eb;
            margin: 2rem 0;
            position: relative;
        }

        .section-divider::before {
            content: '';
            position: absolute;
            top: -2px;
            left: 50%;
            transform: translateX(-50%);
            width: 60px;
            height: 2px;
            background: linear-gradient(90deg, #3b82f6, #2563eb);
        }

        /* Photo Change Hint */
        .photo-hint {
            font-size: 0.8125rem;
            color: #6b7280;
            margin-top: 0.75rem;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
        }

        .photo-hint i {
            color: #3b82f6;
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="profile-container max-w-4xl mx-auto">

        <div class="mb-6">
            <h1 class="text-3xl font-bold text-gray-800 flex items-center gap-3">
                <i class="fas fa-user-circle text-blue-600"></i>
                My Profile
            </h1>
            <p class="text-gray-600 mt-2">View and manage your personal information</p>
        </div>


        <div class="bg-white rounded-xl shadow-md overflow-hidden">

            <div class="profile-header bg-gradient-to-r from-blue-50 to-indigo-50 p-8">
                <div class="profile-pic-wrapper mb-4">
                    <asp:Image ID="imgProfilePic" runat="server" CssClass="profile-pic" ImageUrl="~/Uploads/default-profile.png" />
                    <button type="button" class="edit-badge" onclick="openPicker()" title="Change profile picture">
                        <i class="fas fa-camera"></i>
                    </button>
                </div>
                <asp:FileUpload ID="fuProfilePic" runat="server" Style="display:none;" accept=".jpg,.jpeg,.png" />
                <div class="photo-hint">
                    <i class="fas fa-info-circle"></i>
                    <span>Click the camera icon to update your profile picture</span>
                </div>
            </div>


            <div class="p-6 sm:p-8">

                <asp:Label ID="lblMessage" runat="server" CssClass="block mb-4"></asp:Label>


                <div class="info-card">
                    <p class="text-sm text-gray-700 flex items-start gap-2">
                        <i class="fas fa-lock text-lg"></i>
                        <span>Your profile information is read-only for security purposes. Contact support if you need to update any details.</span>
                    </p>
                </div>


                <asp:Panel ID="pnlViewMode" runat="server" Visible="false" CssClass="sr-only">
                    <div class="form-field"><span class="form-label">Email</span><asp:Label ID="lblEmail" runat="server" /></div>
                    <div class="form-field"><span class="form-label">Contact Number</span><asp:Label ID="lblContactNumber" runat="server" /></div>
                    <div class="form-field"><span class="form-label">Street and Unit</span><asp:Label ID="lblStreetAndUnit" runat="server" /></div>
                    <div class="form-field"><span class="form-label">Barangay</span><asp:Label ID="lblBarangay" runat="server" /></div>
                    <div class="form-field"><span class="form-label">City</span><asp:Label ID="lblCity" runat="server" /></div>
                    <div class="form-field"><span class="form-label">Region</span><asp:Label ID="lblRegion" runat="server" /></div>
                    <div class="form-field"><span class="form-label">Country</span><asp:Label ID="lblCountry" runat="server" /></div>
                </asp:Panel>


                <asp:Panel ID="pnlEditMode" runat="server" Visible="true">

                    <div class="mb-6">
                        <h3 class="text-lg font-semibold text-gray-800 mb-4 flex items-center gap-2">
                            <i class="fas fa-user text-blue-600"></i>
                            Personal Information
                        </h3>
                        
                        <div class="form-grid">
                            <div class="form-field">
                                <label class="form-label">First Name</label>
                                <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                            <div class="form-field">
                                <label class="form-label">Middle Name</label>
                                <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                            <div class="form-field">
                                <label class="form-label">Last Name</label>
                                <asp:TextBox ID="txtLastName" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                            <div class="form-field">
                                <label class="form-label">Full Name</label>
                                <asp:TextBox ID="txtName" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                        </div>
                    </div>

                    <div class="section-divider"></div>

                    <div class="mb-6">
                        <h3 class="text-lg font-semibold text-gray-800 mb-4 flex items-center gap-2">
                            <i class="fas fa-address-book text-blue-600"></i>
                            Contact Information
                        </h3>
                        
                        <div class="form-grid">
                            <div class="form-field">
                                <label class="form-label">Email Address</label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                            <div class="form-field">
                                <label class="form-label">Contact Number</label>
                                <asp:TextBox ID="txtContactNumber" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                        </div>
                    </div>

                    <div class="section-divider"></div>


                    <div class="mb-6">
                        <h3 class="text-lg font-semibold text-gray-800 mb-4 flex items-center gap-2">
                            <i class="fas fa-map-marker-alt text-blue-600"></i>
                            Address Information
                        </h3>
                        
                        <div class="form-grid">
                            <div class="form-field form-field-full">
                                <label class="form-label">Street and Unit</label>
                                <asp:TextBox ID="txtStreetAndUnit" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                            <div class="form-field">
                                <label class="form-label">Barangay</label>
                                <asp:TextBox ID="txtBarangay" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                            <div class="form-field">
                                <label class="form-label">City</label>
                                <asp:TextBox ID="txtCity" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                            <div class="form-field">
                                <label class="form-label">Region</label>
                                <asp:TextBox ID="txtRegion" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                            <div class="form-field">
                                <label class="form-label">Country</label>
                                <asp:TextBox ID="txtCountry" runat="server" CssClass="form-input-readonly" ReadOnly="true" />
                            </div>
                        </div>
                    </div>

  
                    <div class="flex flex-wrap justify-center gap-3 pt-4 border-t-2 border-gray-100">
                        <asp:Button ID="btnSaveProfile" runat="server" CssClass="btn-save" Text="Save Photo" OnClick="btnSaveProfile_Click" />
                        <asp:Button ID="btnCancelEdit" runat="server" CssClass="btn-cancel" Text="Cancel" OnClick="btnCancelEdit_Click" Visible="false" />
                    </div>
                </asp:Panel>
            </div>
        </div>


    <script type="text/javascript">
        function openPicker() {
            document.getElementById('<%= fuProfilePic.ClientID %>').click();
        }

      
        document.addEventListener('change', function (e) {
            if (e.target && e.target.id === '<%= fuProfilePic.ClientID %>') {
                const file = e.target.files && e.target.files[0];
                if (!file) return;

             
                if (!file.type.match('image.*')) {
                    alert('Please select a valid image file (JPG, JPEG, or PNG)');
                    return;
                }

               
                if (file.size > 5 * 1024 * 1024) {
                    alert('File size must be less than 5MB');
                    return;
                }

                const reader = new FileReader();
                reader.onload = function (event) {
                    document.getElementById('<%= imgProfilePic.ClientID %>').src = event.target.result;
                };
                reader.readAsDataURL(file);
            }
        });

        window.addEventListener('load', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    </script>
</asp:Content>