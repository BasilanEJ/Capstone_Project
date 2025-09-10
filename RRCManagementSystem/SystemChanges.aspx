<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="SystemChanges.aspx.cs" Inherits="RRCManagementSystem.SystemChanges" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <div class="container my-5">

        <!-- Hidden field for SweetAlert messages -->
        <asp:HiddenField ID="hfAlertMessage" runat="server" />

        <!-- 🧪 Chemical Usage Settings -->
        <div class="card shadow mb-4 mx-auto" style="max-width: 700px;">
            <div class="card-body">
                <h4 class="text-primary fw-bold border-bottom pb-2 mb-4">🧪 Chemical Usage Based on SQM</h4>

                <div class="mb-3">
                    <label class="form-label">Usage for 0–100 sqm (mL)</label>
                    <asp:TextBox ID="txtUsage_0_100" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Usage for 101–250 sqm (mL)</label>
                    <asp:TextBox ID="txtUsage_101_250" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Usage for 251–400 sqm (mL)</label>
                    <asp:TextBox ID="txtUsage_251_400" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Usage for 401–600 sqm (mL)</label>
                    <asp:TextBox ID="txtUsage_401_600" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Usage for 601–800 sqm (mL)</label>
                    <asp:TextBox ID="txtUsage_601_800" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Usage for 801–1000 sqm (mL)</label>
                    <asp:TextBox ID="txtUsage_801_1000" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Usage for 1000+ sqm (mL)</label>
                    <asp:TextBox ID="txtUsage_1000plus" runat="server" CssClass="form-control" TextMode="Number" />
                </div>
            </div>
        </div>

        <!-- 📅 Max Inspections Settings -->
        <div class="card shadow mx-auto" style="max-width: 700px;">
            <div class="card-body">
                <h4 class="text-primary fw-bold border-bottom pb-2 mb-4">📅 Max Inspections Per Day</h4>

                <div class="mb-3">
                    <label class="form-label">Max Inspections per Day</label>
                    <asp:TextBox ID="txtMaxInspections" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <asp:Button ID="btnSave" runat="server" Text="Save Changes"
                    CssClass="btn btn-primary w-100 fw-bold mb-3" OnClick="btnSave_Click" />
            </div>
        </div>
    </div>

    <!-- SweetAlert Script -->
    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {
            var alertMessage = document.getElementById("<%= hfAlertMessage.ClientID %>").value;
            if (alertMessage) {
                var parts = alertMessage.split('|');
                var type = parts[0];
                var message = parts[1];

                Swal.fire({
                    icon: type,
                    title: type === 'success' ? 'Success' : 'Error',
                    text: message,
                    confirmButtonColor: '#0b3f7a'
                });
            }
        });
    </script>
</asp:Content>
