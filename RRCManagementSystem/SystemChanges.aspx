<%@ Page Title="" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="SystemChanges.aspx.cs" Inherits="RRCManagementSystem.SystemChanges" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <div class="container my-5">
        <!-- 🧪 Chemical Usage Settings -->
        <div class="card shadow mb-4 mx-auto" style="max-width: 700px;">
            <div class="card-body">
                <h4 class="text-primary fw-bold border-bottom pb-2 mb-4">🧪 Chemical Usage Based on SQM</h4>

                <div class="mb-3">
                    <label class="form-label">Usage for 0–100 sqm (mL)</label>
                    <asp:TextBox ID="txtUsage100" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Usage for 101–200 sqm (mL)</label>
                    <asp:TextBox ID="txtUsage200" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Usage for 201+ sqm (mL)</label>
                    <asp:TextBox ID="txtUsage200Plus" runat="server" CssClass="form-control" TextMode="Number" />
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

                <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-primary w-100 fw-bold mb-3" OnClick="btnSave_Click" />
                <asp:Label ID="lblMessage" runat="server" CssClass="text-success fw-semibold d-block text-center" />
            </div>
        </div>
    </div>
</asp:Content>
