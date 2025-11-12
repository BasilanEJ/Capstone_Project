<%@ Page Title="Archived Inquiries" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ArchivedInquiries.aspx.cs" Inherits="RRCManagementSystem.ArchivedInquiries" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    
    <style>
        .table-rounded-corners {
            border-collapse: separate;
            border-spacing: 0;
            border-radius: 0.5rem;
            overflow: hidden;
        }
        .table-rounded-corners thead tr:first-child th:first-child {
            border-top-left-radius: 0.5rem;
        }
        .table-rounded-corners thead tr:first-child th:last-child {
            border-top-right-radius: 0.5rem;
        }
        
        /* Image Modal Styles */
        .image-modal-overlay {
            background-color: rgba(0, 0, 0, 0.85);
            transition: opacity 0.3s ease-in-out;
        }

        .image-modal-close-btn {
            position: absolute;
            top: 15px;
            right: 15px;
            background-color: rgba(255, 255, 255, 0.9);
            border: none;
            cursor: pointer;
            color: #1f2937;
            font-size: 1.5rem;
            line-height: 1;
            padding: 0.5rem;
            border-radius: 50%;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
            transition: background-color 0.2s, transform 0.2s;
            z-index: 60;
        }

        .image-modal-close-btn:hover {
            background-color: #f87171;
            color: white;
            transform: scale(1.05);
        }

        .inquiry-photo {
            cursor: pointer;
            transition: transform 0.2s, opacity 0.2s;
        }

        .inquiry-photo:hover {
            transform: scale(1.1);
            opacity: 0.8;
        }

        .images-grid {
            display: flex;
            gap: 8px;
            flex-wrap: wrap;
            justify-content: center;
        }

        .images-grid img {
            width: 50px;
            height: 50px;
            object-fit: cover;
            border-radius: 6px;
        }

        @media (max-width: 640px) {
            .swal2-popup {
                width: 95% !important;
                max-height: 90vh !important;
            }
        }

        /* Status Badge Styles */
        .status-badge {
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 11px;
            font-weight: 600;
            text-transform: uppercase;
            display: inline-block;
            background: #9ca3af;
            color: #1f2937;
        }

        /* Client Info Card */
        .client-info-card {
            background: #f9fafb;
            border-radius: 8px;
            padding: 12px;
            border-left: 3px solid #f97316;
        }

        .info-row {
            display: flex;
            margin-bottom: 6px;
            font-size: 13px;
        }

        .info-label {
            font-weight: 600;
            color: #4b5563;
            min-width: 80px;
            display: flex;
            align-items: center;
            gap: 6px;
        }

        .info-value {
            color: #1f2937;
            flex: 1;
        }
    </style>

    <div class="container mx-auto p-4 md:p-8 bg-white rounded-lg shadow-lg mt-8">
        <h2 class="text-2xl font-bold mb-6 text-gray-800 text-center">
            <i class="fas fa-archive"></i> Archived Inquiries
        </h2>
        <asp:Label ID="lblPermission" runat="server" CssClass="text-red-500 font-semibold text-center block mb-4" />

        <asp:HiddenField ID="hfSelectedInquiryID" runat="server" />

        <div class="overflow-x-auto shadow-lg rounded-lg">
            <asp:GridView ID="gvArchived" runat="server" AutoGenerateColumns="False"
                CssClass="min-w-full bg-white table-rounded-corners"
                DataKeyNames="InquiryID,InquiryNumber"
                OnRowCommand="gvArchived_RowCommand"
                OnRowDataBound="gvArchived_RowDataBound"
                HeaderStyle-CssClass="bg-orange-600 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors"
                AlternatingRowStyle-CssClass="bg-gray-50 hover:bg-gray-100 transition-colors"
                EnableViewState="true">

                <Columns>
                    <asp:BoundField DataField="InquiryNumber" HeaderText="Inquiry #"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200 font-semibold" />

                    <asp:TemplateField HeaderText="Client Information"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-4 border-r border-gray-200">
                        <ItemTemplate>
                            <div class="client-info-card">
                                <!-- Name -->
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-user text-blue-500"></i>
                                        <strong>Name:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("ClientName") %></span>
                                </div>

                                <!-- Email -->
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-envelope text-green-500"></i>
                                        <strong>Email:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("ClientEmail") %></span>
                                </div>

                                <!-- Contact -->
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-phone text-purple-500"></i>
                                        <strong>Contact:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("ClientContact") %></span>
                                </div>

                                <hr class="my-2 border-gray-300" />

                                <!-- Street -->
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-road text-gray-500"></i>
                                        <strong>Street:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("Street") %></span>
                                </div>

                                <!-- Barangay -->
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-map-pin text-orange-500"></i>
                                        <strong>Barangay:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("Barangay") %></span>
                                </div>

                                <!-- City -->
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-city text-red-500"></i>
                                        <strong>City:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("City") %></span>
                                </div>

                                <!-- Region -->
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-map-marked-alt text-indigo-500"></i>
                                        <strong>Region:</strong>
                                    </span>
                                    <span class="info-value"><%# Eval("Region") %></span>
                                </div>

                                <!-- Landmark -->
                                <div class="info-row">
                                    <span class="info-label">
                                        <i class="fas fa-map-marker-alt text-yellow-500"></i>
                                        <strong>Landmark:</strong>
                                    </span>
                                    <span class="info-value">
                                        <%# string.IsNullOrEmpty(Eval("Landmark")?.ToString()) ? "<em class='text-gray-400'>None</em>" : Eval("Landmark") %>
                                    </span>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Inspection Details"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <div class="text-left">
                                <div class="font-semibold text-gray-800 mb-2">
                                    <i class="fas fa-calendar text-blue-500"></i>
                                    <%# Convert.ToDateTime(Eval("InspectionDate")).ToString("MMM dd, yyyy (dddd)") %>
                                </div>
                                <div class="text-sm text-gray-600 mb-2">
                                    <i class="fas fa-clock text-green-500"></i>
                                    <%# Eval("InspectionTime") %>
                                </div>
                                <div class="text-sm">
                                    <span class="font-semibold text-red-600">
                                        <i class="fas fa-bug"></i>
                                        <%# Eval("PestType") %>
                                    </span>
                                </div>
                                <div class="mt-2">
                                    <span class="px-2 py-1 bg-yellow-100 text-yellow-800 rounded text-xs font-semibold">
                                        ⚠️ <%# Eval("Urgency") %>
                                    </span>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Problem Description"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 border-r border-gray-200 max-w-xs">
                        <ItemTemplate>
                            <span class="block max-w-xs overflow-hidden text-ellipsis whitespace-nowrap text-sm">
                                <%# Eval("ProblemDescription") %>
                            </span>
                            <asp:LinkButton ID="lnkViewMessage" runat="server"
                                CssClass="text-blue-500 hover:underline text-xs"
                                OnClientClick='<%# "return showFullMessage(\"" + HttpUtility.JavaScriptStringEncode(Eval("ProblemDescription").ToString()) + "\");" %>'
                                Visible='<%# Eval("ProblemDescription") != null && Eval("ProblemDescription").ToString().Length > 50 %>'>
                                See more
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Photos"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <asp:Literal ID="litImages" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <span class="status-badge">
                                <i class="fas fa-archive"></i> ARCHIVED
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="ArchivedAt" HeaderText="Archived At" DataFormatString="{0:g}"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center text-sm border-r border-gray-200" />

                    <asp:TemplateField HeaderText="Action"
                        HeaderStyle-CssClass="py-3 px-6 text-center"
                        ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <div class="flex flex-col gap-2">
                                <!-- ♻️ Restore button -->
                                <asp:Button ID="btnRestore" runat="server" Text="♻️ Restore"
                                    CssClass="px-4 py-2 bg-green-500 text-white font-bold rounded-md hover:bg-green-600 transition-colors text-sm"
                                    CommandName="Restore"
                                    CommandArgument='<%# Eval("InquiryID") %>'
                                    OnClientClick="return false;" />

                                <!-- 🗑️ Delete button -->
                                <asp:Button ID="btnDelete" runat="server" Text="🗑️ Delete"
                                    CssClass="px-4 py-2 bg-red-500 text-white font-bold rounded-md hover:bg-red-600 transition-colors text-sm"
                                    CommandName="Delete"
                                    CommandArgument='<%# Eval("InquiryID") %>'
                                    OnClientClick="return false;" />
                                
                                <!-- Hidden fields for SweetAlert confirmation -->
                                <asp:HiddenField ID="hdnConfirmAction" runat="server" />
                                <asp:HiddenField ID="hdnInquiryID" runat="server" Value='<%# Eval("InquiryID") %>' />
                                <asp:HiddenField ID="hdnInquiryNumber" runat="server" Value='<%# Eval("InquiryNumber") %>' />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <!-- Image Modal -->
    <div id="imageModal" class="fixed inset-0 z-50 flex items-center justify-center image-modal-overlay opacity-0 pointer-events-none" 
         onclick="if (event.target.id === 'imageModal') hideImageModal()">
        
        <div class="relative max-w-4xl max-h-[90vh]" onclick="event.stopPropagation()">
            <button type="button" onclick="hideImageModal()" class="image-modal-close-btn">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
            </button>
            
            <img id="modalImage" class="max-w-full max-h-[90vh] mx-auto rounded-lg" alt="Enlarged Inquiry Photo" />
        </div>
    </div>

<script type="text/javascript">
    // Image Modal Functions
    function showImageModal(src) {
        if (!src || src === '#') return;
        document.getElementById('modalImage').src = src;
        const modal = document.getElementById('imageModal');
        modal.classList.remove('opacity-0', 'pointer-events-none');
        document.body.style.overflow = 'hidden';
    }

    function hideImageModal() {
        const modal = document.getElementById('imageModal');
        modal.classList.add('opacity-0', 'pointer-events-none');
        document.body.style.overflow = '';
    }

    // Close modal on Escape key
    document.addEventListener('keydown', function (event) {
        const imageModal = document.getElementById('imageModal');
        if (event.key === 'Escape' && !imageModal.classList.contains('opacity-0')) {
            hideImageModal();
        }
    });

    function showFullMessage(message) {
        Swal.fire({
            title: 'Problem Description',
            html: `<div class="text-left text-sm">${message}</div>`,
            icon: 'info',
            confirmButtonText: 'Close',
            width: '600px'
        });
        return false;
    }

    // Handle Restore and Delete button clicks with SweetAlert
    document.addEventListener('DOMContentLoaded', function () {
        attachConfirmationHandlers();
    });

    function attachConfirmationHandlers() {
        // Restore buttons
        const restoreButtons = document.querySelectorAll('[id*="btnRestore"]');
        restoreButtons.forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                const row = this.closest('tr');
                const inquiryNumber = row.querySelector('[id*="hdnInquiryNumber"]')?.value || 'this inquiry';

                Swal.fire({
                    title: 'Restore Inquiry',
                    html: `Are you sure you want to restore:<br><strong>${inquiryNumber}</strong>?<br><br>This inquiry will be moved back to Pending status.`,
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#10b981',
                    cancelButtonColor: '#6b7280',
                    confirmButtonText: '<i class="fas fa-undo mr-2"></i>Yes, Restore It',
                    cancelButtonText: 'Cancel'
                }).then((result) => {
                    if (result.isConfirmed) {
                        const hdnConfirm = row.querySelector('[id*="hdnConfirmAction"]');
                        if (hdnConfirm) {
                            hdnConfirm.value = 'Restore';
                            __doPostBack(this.name, '');
                        }
                    }
                });

                return false;
            });
        });

        // Delete buttons
        const deleteButtons = document.querySelectorAll('[id*="btnDelete"]');
        deleteButtons.forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                const row = this.closest('tr');
                const inquiryNumber = row.querySelector('[id*="hdnInquiryNumber"]')?.value || 'this inquiry';

                Swal.fire({
                    title: 'Permanently Delete?',
                    html: `⚠️ <strong>WARNING:</strong> This action cannot be undone!<br><br>Are you sure you want to permanently delete:<br><strong>${inquiryNumber}</strong>?`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#dc2626',
                    cancelButtonColor: '#6b7280',
                    confirmButtonText: '<i class="fas fa-trash mr-2"></i>Yes, Delete Forever',
                    cancelButtonText: 'Cancel'
                }).then((result) => {
                    if (result.isConfirmed) {
                        const hdnConfirm = row.querySelector('[id*="hdnConfirmAction"]');
                        if (hdnConfirm) {
                            hdnConfirm.value = 'Delete';
                            __doPostBack(this.name, '');
                        }
                    }
                });

                return false;
            });
        });
    }

    // Re-attach handlers after postback
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    if (prm) {
        prm.add_endRequest(function () {
            attachConfirmationHandlers();
        });
    }
</script>
</asp:Content>