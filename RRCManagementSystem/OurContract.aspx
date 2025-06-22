<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="OurContract.aspx.cs" Inherits="RRCManagementSystem.OurContract" %>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container my-5">
        <div class="card shadow mx-auto" style="max-width: 800px;">
            <div class="card-body">
                <h2 class="text-center text-primary mb-4">Your Contract</h2>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger text-center d-block mb-3" />

                <asp:Panel ID="pnlContract" runat="server" Visible="false">
                    <div class="mb-2">
                        <strong>Start Date:</strong>
                        <asp:Label ID="lblStartDate" runat="server" CssClass="ms-2 text-secondary" />
                    </div>
                    <div class="mb-2">
                        <strong>End Date:</strong>
                        <asp:Label ID="lblEndDate" runat="server" CssClass="ms-2 text-secondary" />
                    </div>
                    <div class="mb-2">
                        <strong>Uploaded:</strong>
                        <asp:Label ID="lblUploaded" runat="server" CssClass="ms-2 text-secondary" />
                    </div>
                    <div class="mb-4">
                        <strong>Remarks:</strong>
                        <asp:Label ID="lblRemarks" runat="server" CssClass="ms-2 text-secondary" />
                    </div>

                    <asp:Button ID="btnDownload" runat="server" Text="Download Contract" CssClass="btn btn-primary w-100 fw-bold mb-4" OnClick="btnDownload_Click" />
                </asp:Panel>


<asp:Panel ID="pnlPreview" runat="server" Visible="false" CssClass="mt-4">
    <h5 class="text-success fw-bold mb-2">📄 Contract Preview</h5>
    <div style="position: relative; width: 100%; height: 500px;">
        <iframe id="pdfViewer" runat="server" style="width:100%; height:100%; border:2px solid #ccc;" frameborder="0"
                onerror="handlePDFError()"></iframe>

        <div id="fallbackMessage" style="display: none; position: absolute; top: 0; left: 0;
             width: 100%; height: 100%; background-color: #f8f9fa; border: 2px solid #ccc;
             padding: 2rem; text-align: center;">
            <h5 class="text-danger">⚠️ Unable to load PDF preview.</h5>
            <p>Please click the download button above to view your contract.</p>
        </div>
    </div>
</asp:Panel>
    

            </div>
        </div>
    </div>

    <script type="text/javascript">
        function handlePDFError() {
            document.getElementById("pdfViewer").style.display = "none";
            document.getElementById("fallbackMessage").style.display = "block";
        }
    </script>
</asp:Content>
