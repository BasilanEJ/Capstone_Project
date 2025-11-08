<%@ Page Title="Set Service Pricing" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true"
    CodeBehind="SetServicePricing.aspx.cs" Inherits="RRCManagementSystem.SetServicePricing" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- TailwindCSS -->
    <script src="https://cdn.tailwindcss.com"></script>
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="min-h-screen bg-gray-100 p-6">
        <div class="max-w-4xl mx-auto bg-white p-8 shadow-md rounded-xl">
            <h2 class="text-2xl font-bold text-center text-blue-700 mb-6">
                <i class="fas fa-tags mr-2"></i> Set Service Pricing
            </h2>

            <asp:Label ID="lblMessage" runat="server" CssClass="block text-center mb-4 font-semibold"></asp:Label>

            <!-- Select Service -->
            <div class="mb-6">
                <label for="ddlServices" class="block font-semibold text-gray-700 mb-2">Select Service *</label>
                <asp:DropDownList ID="ddlServices" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlServices_SelectedIndexChanged"
                    CssClass="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none">
                </asp:DropDownList>
            </div>

            <!-- Add Pricing Tier -->
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-4">
                <div>
                    <label class="block text-gray-700 font-semibold mb-1">Min SQM</label>
                    <asp:TextBox ID="txtMinSQM" runat="server" TextMode="Number" CssClass="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none" />
                </div>

                <div>
                    <label class="block text-gray-700 font-semibold mb-1">Max SQM</label>
                    <asp:TextBox ID="txtMaxSQM" runat="server" TextMode="Number" CssClass="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none" />
                </div>

                <div>
                    <label class="block text-gray-700 font-semibold mb-1">Flat Price (₱)</label>
                    <asp:TextBox ID="txtFlatPrice" runat="server" TextMode="Number" CssClass="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none" />
                </div>
            </div>

            <div class="text-right mb-6">
                <asp:Button ID="btnAddTier" runat="server" Text="➕ Add Pricing Tier"
                    CssClass="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-6 rounded-lg"
                    OnClick="btnAddTier_Click" />
            </div>

            <!-- GridView for Existing Pricing -->
          <!-- GridView for Existing Pricing -->
<asp:GridView ID="gvPricing" runat="server" AutoGenerateColumns="False" CssClass="min-w-full border rounded-lg"
    DataKeyNames="TierID" OnRowEditing="gvPricing_RowEditing" OnRowUpdating="gvPricing_RowUpdating"
    OnRowCancelingEdit="gvPricing_RowCancelingEdit" OnRowDeleting="gvPricing_RowDeleting"
    HeaderStyle-CssClass="bg-blue-600 text-white font-semibold">

    <Columns>
        <asp:BoundField DataField="TierID" HeaderText="Tier ID" Visible="False" ReadOnly="True" />

        <asp:BoundField DataField="ServiceName" HeaderText="Service" ReadOnly="True" />

        <asp:BoundField DataField="MinSQM" HeaderText="Min SQM" />
        <asp:BoundField DataField="MaxSQM" HeaderText="Max SQM" />
        <asp:BoundField DataField="FlatPrice" HeaderText="Flat Price (₱)" DataFormatString="{0:N2}" />

        <asp:CommandField ShowEditButton="True" EditText="✏️ Edit" />
        <asp:CommandField ShowDeleteButton="True" DeleteText="🗑️ Delete" />
    </Columns>
</asp:GridView>

        </div>
    </div>
</asp:Content>
