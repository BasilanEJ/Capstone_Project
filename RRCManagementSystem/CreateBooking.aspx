<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="CreateBooking.aspx.cs" Inherits="RRCManagementSystem.CreateBooking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <div class="card shadow mx-auto" style="max-width: 700px;">
            <div class="card-body">
                <h2 class="text-center text-primary mb-4">🛠️ Create Booking Quotation</h2>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-center d-block fw-semibold text-danger mb-3" />

                <!-- Client Dropdown -->
                <div class="mb-3">
                    <label for="ddlClients" class="form-label fw-bold">Select Client:</label>
                    <asp:DropDownList ID="ddlClients" runat="server" CssClass="form-select" />
                </div>

                <!-- Services CheckBoxList -->
                <div class="mb-3">
                    <label class="form-label fw-bold">Select Services:</label>
                    <asp:CheckBoxList 
                        ID="cblServices" 
                        runat="server" 
                        RepeatLayout="Flow" 
                        CssClass="d-flex flex-column gap-2"
                        DataTextField="Name"
                        DataValueField="ServiceID">
                    </asp:CheckBoxList>
                </div>

                <!-- SQM Input -->
                <div class="mb-3">
                    <label for="txtSQM" class="form-label fw-bold">Square Meters (SQM):</label>
                    <asp:TextBox ID="txtSQM" runat="server" CssClass="form-control" TextMode="Number" />
                </div>

                <!-- Total Label -->
                <asp:Label ID="lblTotal" runat="server" CssClass="d-block text-center fw-bold text-success mb-3" />

                <!-- Action Buttons -->
                <div class="d-flex justify-content-center gap-3">
                    <asp:Button ID="btnCalculate" runat="server" Text="Calculate Price" CssClass="btn btn-primary px-4" OnClick="btnCalculate_Click" />
                    <asp:Button ID="btnSubmit" runat="server" Text="Submit Quotation" CssClass="btn btn-success px-4" OnClick="btnSubmit_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>
