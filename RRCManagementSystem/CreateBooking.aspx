<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="CreateBooking.aspx.cs" Inherits="RRCManagementSystem.CreateBooking" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />

    <div class="container py-5">
        <div class="card shadow mx-auto" style="max-width: 700px;">
            <div class="card-body">
                <h2 class="text-center text-primary mb-4">🛠️ Create Booking Quotation</h2>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-center d-block fw-semibold text-danger mb-3" />

                <!-- Search Client -->
                <div class="mb-3">
                    <label for="txtClientSearch" class="form-label fw-bold">Search Client:</label>
                    <asp:TextBox ID="txtClientSearch" runat="server" CssClass="form-control" />
                    <ajaxToolkit:AutoCompleteExtender 
                        ID="AutoCompleteExtender1" 
                        runat="server"
                        TargetControlID="txtClientSearch"
                        ServiceMethod="SearchClients"
                        MinimumPrefixLength="1"
                        CompletionSetCount="10"
                        EnableCaching="true"
                        FirstRowSelected="true" />
                    <asp:HiddenField ID="hfClientID" runat="server" />
                </div>

                <!-- Services CheckBoxList -->
                    <div class="mb-3">
                        <label class="form-label fw-bold">Select Services:</label>
                        <asp:CheckBoxList 
                            ID="cblServices" 
                            runat="server" 
                            RepeatLayout="Table"
                            CssClass="form-check-list"
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

    <style>
    .form-check-list table {
        width: 100%;
    }

    .form-check-list td {
        padding: 8px 0;
    }

    .form-check-list input[type="checkbox"] {
        margin-right: 8px;
    }

    .form-check-list label {
        display: inline-block;
        margin-left: 4px;
        font-weight: normal;
    }
</style>


    <!-- JS to handle extracting client ID -->
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var txt = document.getElementById('<%= txtClientSearch.ClientID %>');
            txt.addEventListener('blur', function () {
                var value = txt.value;
                if (value.includes('|')) {
                    var parts = value.split('|');
                    txt.value = parts[0]; // Show just the client name
                    document.getElementById('<%= hfClientID.ClientID %>').value = parts[1]; // Save the ClientID
                }
            });
        });
    </script>
</asp:Content>
