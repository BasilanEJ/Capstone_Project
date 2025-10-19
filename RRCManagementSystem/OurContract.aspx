<%@ Page Title="Our Contract" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="OurContract.aspx.cs" Inherits="RRCManagementSystem.OurContract" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Responsive PDF viewer */
        .pdf-container {
            position: relative;
            width: 100%;
            padding-bottom: 75%; /* 4:3 aspect ratio */
            height: 0;
            overflow: hidden;
        }

        .pdf-container iframe {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
        }

        @media (min-width: 768px) {
            .pdf-container {
                padding-bottom: 141.42%; /* A4 aspect ratio */
            }
        }

        /* Card hover effect */
        .contract-card {
            transition: all 0.3s ease;
        }

        .contract-card:hover {
            box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1);
        }

        /* Info row styling */
        .info-row {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
            padding: 0.75rem 0;
            border-bottom: 1px solid #e5e7eb;
        }

        .info-row:last-child {
            border-bottom: none;
        }

        .info-label {
            font-size: 0.75rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            color: #6b7280;
        }

        .info-value {
            font-size: 0.875rem;
            color: #1f2937;
        }

        @media (min-width: 640px) {
            .info-row {
                flex-direction: row;
                align-items: center;
                justify-content: space-between;
            }

            .info-value {
                text-align: right;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="max-w-5xl mx-auto">
        <!-- Page Header -->
        <div class="mb-8">
            <div class="flex items-center gap-4 mb-3">
                <div class="w-12 h-12 bg-blue-600 rounded-lg flex items-center justify-center">
                    <i class="fas fa-file-contract text-white text-xl"></i>
                </div>
                <div>
                    <h1 class="text-3xl md:text-4xl font-bold text-gray-900">Your Contract</h1>
                    <p class="text-gray-600 mt-1">View and download your service contract</p>
                </div>
            </div>
        </div>

        <!-- Message Alert -->
        <asp:Label ID="lblMessage" runat="server" CssClass="hidden" />
        <div id="messageContainer" class="mb-6"></div>

        <!-- Contract Details Section -->
        <asp:Panel ID="pnlContract" runat="server" Visible="false">
            <div class="contract-card bg-white shadow-md rounded-lg overflow-hidden border border-gray-200 mb-8">
                <!-- Card Header -->
                <div class="bg-blue-600 px-6 py-4 border-b border-blue-700">
                    <h2 class="text-lg font-semibold text-white flex items-center">
                        <i class="fas fa-info-circle mr-3"></i>
                        Contract Information
                    </h2>
                </div>

                <!-- Card Body -->
                <div class="p-6">
                    <!-- Contract Details Grid -->
                    <div class="space-y-0 mb-6">
                        <div class="info-row">
                            <span class="info-label">
                                <i class="far fa-calendar-check text-blue-600 mr-2"></i>
                                Start Date
                            </span>
                            <asp:Label ID="lblStartDate" runat="server" CssClass="info-value font-medium" />
                        </div>

                        <div class="info-row">
                            <span class="info-label">
                                <i class="far fa-calendar-times text-gray-600 mr-2"></i>
                                End Date
                            </span>
                            <asp:Label ID="lblEndDate" runat="server" CssClass="info-value font-medium" />
                        </div>

                        <div class="info-row">
                            <span class="info-label">
                                <i class="far fa-clock text-blue-600 mr-2"></i>
                                Uploaded
                            </span>
                            <asp:Label ID="lblUploaded" runat="server" CssClass="info-value" />
                        </div>

                        <div class="info-row">
                            <span class="info-label">
                                <i class="fas fa-comment-dots text-gray-600 mr-2"></i>
                                Remarks
                            </span>
                            <asp:Label ID="lblRemarks" runat="server" CssClass="info-value break-words" />
                        </div>
                    </div>

                    <!-- Action Buttons -->
                    <div class="flex flex-col sm:flex-row gap-3 pt-4 border-t border-gray-200">
                        <asp:Button ID="btnDownload" runat="server"
                            Text="Download Contract"
                            CssClass="inline-flex items-center justify-center bg-blue-600 hover:bg-blue-700 text-white font-medium py-3 px-6 rounded-lg transition w-full sm:w-auto"
                            OnClick="btnDownload_Click"
                            UseSubmitBehavior="false"
                            CausesValidation="false">
                        </asp:Button>

                        <asp:Button ID="btnPreview" runat="server"
                            Text="Preview Contract"
                            CssClass="inline-flex items-center justify-center bg-gray-600 hover:bg-gray-700 text-white font-medium py-3 px-6 rounded-lg transition w-full sm:w-auto"
                            OnClick="btnPreview_Click"
                            UseSubmitBehavior="false"
                            CausesValidation="false">
                        </asp:Button>

                        <asp:HyperLink ID="hlOpenNewTab" runat="server"
                            CssClass="inline-flex items-center justify-center bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium py-3 px-6 rounded-lg border border-gray-300 transition w-full sm:w-auto hidden"
                            Target="_blank"
                            Text="Open in New Tab">
                        </asp:HyperLink>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Contract Preview Section -->
        <asp:Panel ID="pnlPreview" runat="server" Visible="false">
            <div class="contract-card bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
                <!-- Preview Header -->
                <div class="bg-gray-700 px-6 py-4 border-b border-gray-800">
                    <h2 class="text-lg font-semibold text-white flex items-center">
                        <i class="far fa-file-pdf mr-3"></i>
                        Contract Preview
                    </h2>
                </div>

                <!-- Preview Body -->
                <div class="p-6 bg-gray-50">
                    <!-- PDF Viewer Container -->
                    <div class="relative w-full bg-white rounded-lg shadow-inner overflow-hidden border border-gray-200" style="height: 600px;">
                        <iframe id="pdfViewer" runat="server"
                            class="w-full h-full"
                            style="border: none;"
                            onerror="handlePDFError()">
                        </iframe>

                        <!-- Fallback Message -->
                        <div id="fallbackMessage"
                            class="absolute inset-0 hidden flex flex-col items-center justify-center p-6 bg-gray-50">
                            <div class="text-center max-w-md">
                                <div class="w-16 h-16 bg-gray-200 rounded-full flex items-center justify-center mx-auto mb-4">
                                    <i class="fas fa-exclamation-triangle text-gray-600 text-2xl"></i>
                                </div>
                                <h4 class="text-gray-800 font-semibold text-lg mb-2">Unable to Load PDF Preview</h4>
                                <p class="text-gray-600 mb-4">
                                    Your browser may not support inline PDF viewing.
                                </p>
                                <div class="bg-blue-50 border border-blue-200 rounded-lg p-4 text-sm text-gray-700">
                                    <p class="mb-2">
                                        <i class="fas fa-info-circle text-blue-600 mr-2"></i>
                                        Please use the <strong>Download Contract</strong> button above to view your file.
                                    </p>
                                    <p class="text-xs text-gray-500">
                                        Note: Some mobile browsers block inline PDF previews for security reasons.
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Mobile Helper Text -->
                    <div class="mt-4 text-center text-sm text-gray-600 md:hidden">
                        <i class="fas fa-mobile-alt mr-2"></i>
                        If preview doesn't work, tap <strong>Download Contract</strong> above
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>

    <!-- JavaScript for PDF Error Handling and Message Display -->
    <script type="text/javascript">
        // Handle PDF loading errors
        function handlePDFError() {
            const frame = document.getElementById("pdfViewer");
            const fallback = document.getElementById("fallbackMessage");
            if (frame) frame.style.display = "none";
            if (fallback) fallback.classList.remove("hidden");
        }

        // Display message if exists
        document.addEventListener('DOMContentLoaded', function () {
            const lblMessage = document.getElementById('<%= lblMessage.ClientID %>');
            const messageContainer = document.getElementById('messageContainer');

            if (lblMessage && lblMessage.innerText.trim()) {
                const messageText = lblMessage.innerText.trim();
                const isError = messageText.toLowerCase().includes('no contract') ||
                    messageText.toLowerCase().includes('not found') ||
                    messageText.toLowerCase().includes('error');

                messageContainer.innerHTML = `
                    <div class="flex items-start gap-3 p-4 rounded-lg border ${isError ? 'bg-gray-50 border-gray-300' : 'bg-blue-50 border-blue-200'}">
                        <i class="fas ${isError ? 'fa-info-circle text-gray-600' : 'fa-check-circle text-blue-600'} text-lg mt-0.5"></i>
                        <div class="flex-1">
                            <p class="${isError ? 'text-gray-800' : 'text-blue-900'} font-medium">${messageText}</p>
                        </div>
                    </div>
                `;
            }
        });

        // Add icons to buttons dynamically
        window.addEventListener('load', function () {
            const btnDownload = document.getElementById('<%= btnDownload.ClientID %>');
            const btnPreview = document.getElementById('<%= btnPreview.ClientID %>');

            if (btnDownload && !btnDownload.querySelector('i')) {
                btnDownload.innerHTML = '<i class="fas fa-download mr-2"></i>' + btnDownload.innerText;
            }

            if (btnPreview && !btnPreview.querySelector('i')) {
                btnPreview.innerHTML = '<i class="far fa-eye mr-2"></i>' + btnPreview.innerText;
            }
        });
    </script>
</asp:Content>