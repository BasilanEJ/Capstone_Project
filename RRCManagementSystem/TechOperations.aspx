<%@ Page Title="My Operations" Language="C#" MasterPageFile="~/HeadTechnician.master" 
    AutoEventWireup="true" CodeBehind="TechOperations.aspx.cs" Inherits="RRCManagementSystem.TechOperations" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- SweetAlert2 for modals -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    
    <style>
        .booking-card {
            background: white;
            border-radius: 12px;
            padding: 1.5rem;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            margin-bottom: 1.5rem;
            border-left: 4px solid #2563eb;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }
        
        .booking-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        }
        
        .booking-card.in-progress {
            border-left-color: #f59e0b;
            background: linear-gradient(to right, #fffbeb 0%, white 100%);
        }
        
        .booking-card.completed {
            border-left-color: #10b981;
            background: linear-gradient(to right, #f0fdf4 0%, white 100%);
        }
        
        .info-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 1rem;
            margin-top: 1rem;
        }
        
        .info-item {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
        }
        
        .info-label {
            font-size: 0.75rem;
            color: #64748b;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .info-value {
            font-size: 0.9375rem;
            color: #1e293b;
            font-weight: 500;
        }
        
        .status-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            border-radius: 6px;
            font-size: 0.875rem;
            font-weight: 600;
        }
        
        .status-assigned {
            background: #dbeafe;
            color: #1e40af;
        }
        
        .status-in-progress {
            background: #fef3c7;
            color: #b45309;
        }
        
        .status-completed {
            background: #d1fae5;
            color: #065f46;
        }
        
        .resource-section {
            background: #f8fafc;
            border-radius: 8px;
            padding: 1rem;
            margin-top: 1rem;
        }
        
        .btn-primary {
            padding: 0.625rem 1.25rem;
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
            border-radius: 8px;
            font-weight: 600;
            font-size: 0.875rem;
            transition: all 0.3s ease;
            border: none;
            cursor: pointer;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
        }
        
        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
            background: linear-gradient(135deg, #1e40af 0%, #1e3a8a 100%);
        }

        .btn-primary:active {
            transform: translateY(0);
        }

        /* Button variants */
        .btn-success {
            padding: 0.625rem 1.25rem;
            background: linear-gradient(135deg, #16a34a 0%, #15803d 100%);
            color: white;
            border-radius: 8px;
            font-weight: 600;
            font-size: 0.875rem;
            transition: all 0.3s ease;
            border: none;
            cursor: pointer;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
        }

        .btn-success:hover {
            background: linear-gradient(135deg, #15803d 0%, #166534 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(22, 163, 74, 0.3);
        }

        .btn-purple {
            padding: 0.625rem 1.25rem;
            background: linear-gradient(135deg, #9333ea 0%, #7e22ce 100%);
            color: white;
            border-radius: 8px;
            font-weight: 600;
            font-size: 0.875rem;
            transition: all 0.3s ease;
            border: none;
            cursor: pointer;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
        }

        .btn-purple:hover {
            background: linear-gradient(135deg, #7e22ce 0%, #6b21a8 100%);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(147, 51, 234, 0.3);
        }

        /* Operation Number Badge */
        .operation-badge {
            display: inline-block;
            background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%);
            color: white;
            padding: 0.25rem 0.75rem;
            border-radius: 6px;
            font-size: 0.75rem;
            font-weight: 700;
            margin-left: 0.5rem;
            box-shadow: 0 2px 4px rgba(139, 92, 246, 0.3);
        }

        /* Mobile responsiveness */
        @media (max-width: 768px) {
            .booking-card {
                padding: 1rem;
            }

            .info-grid {
                grid-template-columns: 1fr;
            }

            .resource-section .grid {
                grid-template-columns: 1fr !important;
            }

            .btn-primary, .btn-success, .btn-purple {
                width: 100%;
                justify-content: center;
            }
        }
    </style>

<script>
    document.addEventListener('DOMContentLoaded', function () {
        attachConfirmationHandlers();
    });

    function attachConfirmationHandlers() {
        // Start Service buttons
        const startButtons = document.querySelectorAll('[id*="btnStartService"]');
        startButtons.forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                const card = this.closest('.booking-card');
                const bookingCode = card.querySelector('h2').textContent.trim().split('\n')[0].trim();
                const hdnConfirm = card.querySelector('[id*="hdnConfirmAction"]');
                const hdnBookingID = card.querySelector('[id*="hdnBookingID"]');
                const hdnScheduleID = card.querySelector('[id*="hdnScheduleID"]');

                Swal.fire({
                    title: 'Start Service',
                    html: `Are you sure you want to start service for:<br><strong>${bookingCode}</strong>?`,
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#16a34a',
                    cancelButtonColor: '#6b7280',
                    confirmButtonText: '<i class="fas fa-play-circle mr-2"></i>Yes, Start Service',
                    cancelButtonText: 'Cancel'
                }).then((result) => {
                    if (result.isConfirmed) {
                        if (hdnConfirm && hdnBookingID && hdnScheduleID) {
                            // Set hidden field values
                            hdnConfirm.value = 'StartService';

                            // Submit the form
                            __doPostBack('ctl00$MainContent$rptBookings', 'StartService|' + hdnBookingID.value + '|' + hdnScheduleID.value);
                        }
                    }
                });

                return false;
            });
        });

        // Complete Service buttons
        const completeButtons = document.querySelectorAll('[id*="btnCompleteService"]');
        completeButtons.forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                const card = this.closest('.booking-card');
                const bookingCode = card.querySelector('h2').textContent.trim().split('\n')[0].trim();
                const hdnConfirm = card.querySelector('[id*="hdnConfirmAction"]');
                const hdnBookingID = card.querySelector('[id*="hdnBookingID"]');
                const hdnScheduleID = card.querySelector('[id*="hdnScheduleID"]');

                Swal.fire({
                    title: 'Complete Service',
                    html: `Are you sure you want to mark this service as completed?<br><strong>${bookingCode}</strong>`,
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#9333ea',
                    cancelButtonColor: '#6b7280',
                    confirmButtonText: '<i class="fas fa-check-circle mr-2"></i>Yes, Complete Service',
                    cancelButtonText: 'Cancel'
                }).then((result) => {
                    if (result.isConfirmed) {
                        if (hdnConfirm && hdnBookingID && hdnScheduleID) {
                            // Set hidden field values
                            hdnConfirm.value = 'CompleteService';

                            // Submit the form
                            __doPostBack('ctl00$MainContent$rptBookings', 'CompleteService|' + hdnBookingID.value + '|' + hdnScheduleID.value);
                        }
                    }
                });

                return false;
            });
        });
    }

    // Re-attach handlers after partial postback
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    if (prm) {
        prm.add_endRequest(function () {
            attachConfirmationHandlers();
        });
    }
</script>


</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="mb-6">
        <h1 class="text-3xl font-bold text-gray-800">
            <i class="fas fa-clipboard-list mr-2"></i>My Team's Operations
        </h1>
        <p class="text-gray-600 mt-2">View all bookings and inspection details assigned to your team</p>
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="text-red-600 font-semibold mb-4 block" Visible="false" />

    <!-- ====== FILTER BAR ====== -->
<div class="bg-white shadow-sm rounded-lg p-4 mb-6 flex flex-wrap gap-3 items-end">
    <div>
        <label for="ddlStatus" class="block text-sm font-semibold text-gray-700 mb-1">
            Filter by Status
        </label>
        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="border-gray-300 rounded-md px-3 py-2 text-sm">
            <asp:ListItem Text="All" Value="" />
            <asp:ListItem Text="Assigned" Value="Assigned" />
            <asp:ListItem Text="In Progress" Value="In Progress" />
            <asp:ListItem Text="Completed" Value="Completed" />
        </asp:DropDownList>
    </div>

    <div>
        <label for="txtDate" class="block text-sm font-semibold text-gray-700 mb-1">
            Scheduled Date
        </label>
        <asp:TextBox ID="txtDate" runat="server" TextMode="Date"
            CssClass="border-gray-300 rounded-md px-3 py-2 text-sm" />
    </div>

    <div>
        <asp:Button ID="btnFilter" runat="server" Text="Apply Filter"
            CssClass="btn-primary" OnClick="btnFilter_Click" />
        <asp:Button ID="btnClear" runat="server" Text="Reset"
            CssClass="btn-success ml-2" OnClick="btnClear_Click" />
    </div>
</div>



    <asp:Repeater ID="rptBookings" runat="server" OnItemDataBound="rptBookings_ItemDataBound" OnItemCommand="rptBookings_ItemCommand">
        <ItemTemplate>
            <div class="booking-card <%# GetStatusClass(Eval("OperationStatus")) %>">
                <!-- Header Section -->
                <div class="flex flex-col sm:flex-row sm:justify-between sm:items-start mb-4 gap-2">
                    <div>
                        <h2 class="text-xl font-bold text-gray-800">
                            <%# Eval("BookingCode") %>
                            <%# Convert.ToBoolean(Eval("IsContract")) ? "<span class='ml-2 text-xs bg-purple-100 text-purple-700 px-2 py-1 rounded'>CONTRACT</span>" : "" %>
                            <%# Eval("OperationNumber") != DBNull.Value && Convert.ToInt32(Eval("OperationNumber")) > 0 ? 
                                "<span class='operation-badge'>Op #" + Eval("OperationNumber") + "</span>" : "" %>
                        </h2>
                        <p class="text-sm text-gray-600 mt-1">
                            <i class="far fa-calendar mr-1"></i>
                            <%# Convert.ToDateTime(Eval("ScheduledDate")).ToString("MMMM dd, yyyy") %>
                            <span class="ml-3">
                                <i class="far fa-clock mr-1"></i>
                                <%# FormatTime(Eval("StartTime")) %>
                            </span>
                        </p>
                    </div>
                    <span class="status-badge <%# GetStatusBadgeClass(Eval("OperationStatus")) %>">
                        <i class="fas <%# GetStatusIcon(Eval("OperationStatus")) %>"></i>
                        <%# Eval("OperationStatus") %>
                    </span>
                </div>

                <!-- Client & Service Info -->
                <div class="info-grid">
                    <div class="info-item">
                        <span class="info-label"><i class="fas fa-user mr-1"></i>Client</span>
                        <span class="info-value"><%# Eval("ClientName") %></span>
                        <span class="text-xs text-gray-500">
                            <%# GetDecryptedContact(Eval("ClientContactEnc")?.ToString()) %>
                        </span>
                    </div>
                    
                    <div class="info-item">
                        <span class="info-label"><i class="fas fa-map-marker-alt mr-1"></i>Address</span>
                        <asp:Literal ID="litAddress" runat="server" />
                    </div>
                    
                    <div class="info-item">
                        <span class="info-label"><i class="fas fa-tools mr-1"></i>Services</span>
                        <asp:Literal ID="litServices" runat="server" />
                    </div>
                    
                    <div class="info-item">
                        <span class="info-label"><i class="fas fa-ruler-combined mr-1"></i>Coverage Area</span>
                        <span class="info-value"><%# Eval("SQM") %> m²</span>
                    </div>
                    
                    <div class="info-item">
                        <span class="info-label"><i class="fas fa-peso-sign mr-1"></i>Total Price</span>
                        <span class="info-value">₱<%# Convert.ToDecimal(Eval("Price")).ToString("N2") %></span>
                    </div>
                    
                    <div class="info-item">
                        <span class="info-label"><i class="fas fa-user-tie mr-1"></i>Inspector</span>
                        <span class="info-value"><%# Eval("InspectorName") ?? "N/A" %></span>
                    </div>
                </div>

                <div class="resource-section">
                    <h3 class="text-sm font-semibold text-gray-700 mb-3">
                        <i class="fas fa-toolbox mr-2"></i>Assigned Resources
                    </h3>
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
                        <div class="text-sm">
                            <strong class="text-blue-600">Equipment:</strong>
                            <p class="text-gray-800 mt-1"><%# Eval("AssignedEquipment") ?? "None assigned" %></p>
                        </div>
                        <div class="text-sm">
                            <strong class="text-green-600">Chemicals:</strong>
                            <p class="text-gray-800 mt-1"><%# Eval("AssignedChemicals") ?? "None assigned" %></p>
                        </div>
                        <div class="text-sm">
                            <strong class="text-orange-600">Safety Gear:</strong>
                            <p class="text-gray-800 mt-1"><%# Eval("AssignedSafetyGear") ?? "None assigned" %></p>
                        </div>
                    </div>
                </div>

                <div class="mt-4 flex gap-2 flex-wrap">
                    <!-- View Details Button -->
                    <asp:LinkButton ID="btnViewDetails" runat="server" 
                        CssClass="btn-primary"
                        CommandName="ViewDetails" 
                        CommandArgument='<%# Eval("BookingID") + "|" + (Eval("ReportID") ?? "0") %>'>
                        <i class="fas fa-eye"></i>
                        <span>View Full Details</span>
                    </asp:LinkButton>
                    
                    <!-- ✅ Start Service Button (only if operation status = Assigned) -->
                    <asp:LinkButton ID="btnStartService" runat="server"
                        CssClass="btn-success"
                        CommandName="StartService"
                        CommandArgument='<%# Eval("BookingID") + "|" + (Eval("ScheduleID") ?? "0") %>'
                        Visible='<%# Eval("OperationStatus").ToString() == "Assigned" %>'
                        OnClientClick="return false;">
                        <i class="fas fa-play-circle"></i>
                        <span>Start Service</span>
                    </asp:LinkButton>
                    
                    <!-- ✅ Complete Service Button (only if operation status = In Progress) -->
                    <asp:LinkButton ID="btnCompleteService" runat="server"
                        CssClass="btn-purple"
                        CommandName="CompleteService"
                        CommandArgument='<%# Eval("BookingID") + "|" + (Eval("ScheduleID") ?? "0") %>'
                        Visible='<%# Eval("OperationStatus").ToString() == "In Progress" %>'
                        OnClientClick="return false;">
                        <i class="fas fa-check-circle"></i>
                        <span>Complete Service</span>
                    </asp:LinkButton>
                    
                    <!-- ✅ View Photos Button -->
                    <asp:LinkButton ID="btnViewPhotos" runat="server" 
                        CssClass="btn-primary"
                        CommandName="ViewPhotos" 
                        CommandArgument='<%# Eval("InspectionPhotosPath") %>'
                        Visible='<%# !string.IsNullOrEmpty(Eval("InspectionPhotosPath")?.ToString()) %>'>
                        <i class="fas fa-images"></i>
                        <span>View Photos</span>
                    </asp:LinkButton>
                    
                    <!-- Hidden fields for SweetAlert confirmation -->
                    <asp:HiddenField ID="hdnConfirmAction" runat="server" />
                    <asp:HiddenField ID="hdnBookingID" runat="server" Value='<%# Eval("BookingID") %>' />
                    <asp:HiddenField ID="hdnScheduleID" runat="server" Value='<%# Eval("ScheduleID") ?? "0" %>' />
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>

    <asp:Label ID="lblNoBookings" runat="server" 
        CssClass="text-center text-gray-500 text-lg block mt-8 p-8 bg-white rounded-lg shadow" 
        Visible="false">
        <i class="fas fa-inbox text-4xl text-gray-300 block mb-3"></i>
        <p class="mt-2">No bookings assigned to your team yet.</p>
    </asp:Label>
</asp:Content>