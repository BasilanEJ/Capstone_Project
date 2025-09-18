<%@ Page Title="Our Contract" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="OurContract.aspx.cs" Inherits="RRCManagementSystem.OurContract" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tailwind CSS -->
    <script src="https://cdn.tailwindcss.com"></script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="max-w-4xl mx-auto px-4 py-8">
        <!-- Page Title -->
        <h2 class="text-2xl md:text-3xl font-bold text-blue-600 text-center mb-6">
            Your Contract
        </h2>

        <!-- Message -->
        <asp:Label ID="lblMessage" runat="server"
            CssClass="block text-center text-red-600 font-medium mb-4" />

        <!-- Contract Details Section -->
        <asp:Panel ID="pnlContract" runat="server" Visible="false">
            <div class="bg-white shadow-lg rounded-xl p-6 mb-6">
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
                    <div>
                        <span class="font-semibold text-gray-700">Start Date:</span>
                        <asp:Label ID="lblStartDate" runat="server"
                            CssClass="ml-2 text-gray-600" />
                    </div>
                    <div>
                        <span class="font-semibold text-gray-700">End Date:</span>
                        <asp:Label ID="lblEndDate" runat="server"
                            CssClass="ml-2 text-gray-600" />
                    </div>
                    <div>
                        <span class="font-semibold text-gray-700">Uploaded:</span>
                        <asp:Label ID="lblUploaded" runat="server"
                            CssClass="ml-2 text-gray-600" />
                    </div>
                    <div class="sm:col-span-2">
                        <span class="font-semibold text-gray-700">Remarks:</span>
                        <asp:Label ID="lblRemarks" runat="server"
                            CssClass="ml-2 text-gray-600 break-words" />
                    </div>
                </div>

                <!-- Action Buttons -->
                <div class="flex flex-col sm:flex-row gap-3">
                    <asp:Button ID="btnDownload" runat="server"
                        Text="Download Contract"
                        CssClass="bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg shadow transition w-full sm:w-auto text-center"
                        OnClick="btnDownload_Click"
                        UseSubmitBehavior="false"
                        CausesValidation="false" />

                    <asp:Button ID="btnPreview" runat="server"
                        Text="Preview"
                        CssClass="bg-gray-100 hover:bg-gray-200 text-gray-800 font-semibold py-2 px-4 rounded-lg border border-gray-300 shadow-sm transition w-full sm:w-auto text-center"
                        OnClick="btnPreview_Click"
                        UseSubmitBehavior="false"
                        CausesValidation="false" />

                    <asp:HyperLink ID="hlOpenNewTab" runat="server"
                        CssClass="bg-gray-100 hover:bg-gray-200 text-gray-800 font-semibold py-2 px-4 rounded-lg border border-gray-300 shadow-sm transition w-full sm:w-auto text-center hidden"
                        Target="_blank"
                        Text="Open in New Tab" />
                </div>
            </div>
        </asp:Panel>

        <!-- Contract Preview Section -->
        <asp:Panel ID="pnlPreview" runat="server" Visible="false">
            <h3 class="text-lg md:text-xl font-semibold text-green-600 mb-3">
                📄 Contract Preview
            </h3>

            <!-- PDF Viewer Container -->
            <div class="relative w-full h-[360px] md:h-[500px] lg:h-[650px] bg-gray-50 rounded-lg overflow-hidden shadow-inner">
                <iframe id="pdfViewer" runat="server"
                    class="w-full h-full bg-white"
                    onerror="handlePDFError()"></iframe>

                <!-- Fallback Message -->
                <div id="fallbackMessage"
                    class="absolute inset-0 hidden flex flex-col items-center justify-center border-2 border-dashed border-gray-300 bg-gray-50 p-4 text-center">
                    <h4 class="text-red-600 font-semibold mb-2">⚠️ Unable to load PDF preview</h4>
                    <p class="text-gray-700 mb-3">
                        Please use the <strong>Download Contract</strong> button above to view your file.
                    </p>
                    <small class="text-gray-500 block">
                        Tip: Some mobile browsers block inline PDF preview.
                    </small>
                </div>
            </div>
        </asp:Panel>
    </div>

    <!-- JavaScript for PDF Error Handling -->
    <script type="text/javascript">
        function handlePDFError() {
            const frame = document.getElementById("pdfViewer");
            const fallback = document.getElementById("fallbackMessage");
            if (frame) frame.style.display = "none";
            if (fallback) fallback.classList.remove("hidden");
        }
    </script>
</asp:Content>
