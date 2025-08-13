<%@ Page Title="Our Contract" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="OurContract.aspx.cs" Inherits="RRCManagementSystem.OurContract" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .page-title{
            color:#0d6efd;
            font-weight:700;
            font-size:clamp(1.25rem,3.2vw,1.75rem);
            line-height:1.2;
        }
        .page-wrap{
            padding-top:clamp(.75rem,2vw,1.25rem);
            padding-bottom:clamp(.75rem,2vw,1.5rem);
        }
        .card-shell{
            border:none;border-radius:1rem;box-shadow:0 8px 20px rgba(0,0,0,.06);
        }
        .info-label{
            color:#6c757d;
        }
        /* Responsive PDF area */
        .pdf-wrap{
            position:relative;
            width:100%;
            height:clamp(360px, 65vh, 720px); /* good on phones and desktops */
            border-radius:.75rem;
            overflow:hidden;
        }
        .pdf-frame{
            width:100%;
            height:100%;
            border:0;
            display:block;
            background:#fff;
        }
        .pdf-fallback{
            position:absolute; inset:0;
            background-color:#f8f9fa;
            border:2px dashed #ced4da;
            display:none;
            padding:1.25rem;
            text-align:center;
            align-items:center; justify-content:center;
        }
        .btn{ min-height:44px } /* comfy tap targets on mobile */

        @media (prefers-reduced-motion: reduce){
            .fade, .collapse { transition:none !important; }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container page-wrap">
        <div class="card card-shell mx-auto p-3 p-sm-4" style="max-width:880px;">
            <h2 class="page-title text-center mb-3">Your Contract</h2>

            <asp:Label ID="lblMessage" runat="server" CssClass="text-danger text-center d-block mb-3" />

            <!-- Contract Details -->
            <asp:Panel ID="pnlContract" runat="server" Visible="false">
                <div class="row g-3 mb-3">
                    <div class="col-12 col-sm-6">
                        <strong>Start Date:</strong>
                        <asp:Label ID="lblStartDate" runat="server" CssClass="ms-2 info-label d-inline-block" />
                    </div>
                    <div class="col-12 col-sm-6">
                        <strong>End Date:</strong>
                        <asp:Label ID="lblEndDate" runat="server" CssClass="ms-2 info-label d-inline-block" />
                    </div>
                    <div class="col-12 col-sm-6">
                        <strong>Uploaded:</strong>
                        <asp:Label ID="lblUploaded" runat="server" CssClass="ms-2 info-label d-inline-block" />
                    </div>
                    <div class="col-12">
                        <strong>Remarks:</strong>
                        <asp:Label ID="lblRemarks" runat="server" CssClass="ms-2 info-label d-inline-block" />
                    </div>
                </div>

                <div class="d-grid gap-2 d-sm-flex mb-3">
                    <asp:Button ID="btnDownload" runat="server"
                        Text="Download Contract"
                        CssClass="btn btn-primary fw-bold flex-fill flex-sm-grow-0"
                        OnClick="btnDownload_Click"
                        UseSubmitBehavior="false"
                        CausesValidation="false" />
                    
                    <asp:Button ID="btnPreview" runat="server"
                        Text="Preview"
                        CssClass="btn btn-outline-secondary fw-semibold flex-fill flex-sm-grow-0"
                        OnClick="btnPreview_Click"
                        UseSubmitBehavior="false"
                        CausesValidation="false" />

                    <!-- Optional: set NavigateUrl in code-behind if you want a direct open link -->
                    <asp:HyperLink ID="hlOpenNewTab" runat="server"
                        CssClass="btn btn-outline-secondary fw-semibold flex-fill flex-sm-grow-0"
                        Target="_blank" Visible="false" Text="Open in New Tab" />
                </div>
            </asp:Panel>

            <!-- Preview -->
            <asp:Panel ID="pnlPreview" runat="server" Visible="false" CssClass="mt-2">
                <h5 class="text-success fw-semibold mb-2">📄 Contract Preview</h5>

                <div class="pdf-wrap">
                    <iframe id="pdfViewer" runat="server" class="pdf-frame" onerror="handlePDFError()"></iframe>

                    <div id="fallbackMessage" class="pdf-fallback">
                        <div>
                            <h6 class="text-danger mb-2">⚠️ Unable to load PDF preview.</h6>
                            <p class="mb-3">Please use the <strong>Download Contract</strong> button above to view your file.</p>
                            <small class="text-muted d-block">Tip: Some mobile browsers block inline PDF preview.</small>
                        </div>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </div>

    <script type="text/javascript">
        function handlePDFError() {
            var frame = document.getElementById("pdfViewer");
            var fallback = document.getElementById("fallbackMessage");
            if (frame) frame.style.display = "none";
            if (fallback) fallback.style.display = "flex";
        }
    </script>
</asp:Content>
    