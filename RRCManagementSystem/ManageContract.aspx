<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageContract.aspx.cs" Inherits="RRCManagementSystem.ManageContract" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    </asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-10 px-4 flex justify-center items-start">
        <div class="w-full max-w-xl bg-white p-8 rounded-2xl shadow-xl border border-gray-200">
            <h2 class="text-center text-3xl font-extrabold text-blue-800 mb-6">Upload New Client Contract</h2>

            <div class="mb-4">
                <asp:Label ID="lblMessage" runat="server" CssClass="block text-center text-sm font-medium text-gray-600" />
            </div>

            <div class="mb-5">
                <label for="<%= ddlClients.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Client</label>
                <asp:DropDownList ID="ddlClients" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors" />
            </div>

            <div class="mb-5">
                <label for="<%= fuContract.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Contract File (PDF Only)</label>
                <asp:FileUpload ID="fuContract" runat="server" CssClass="block w-full text-sm text-gray-700 border border-gray-300 rounded-lg cursor-pointer bg-gray-50 focus:outline-none file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-blue-500 file:text-white hover:file:bg-blue-600 transition-colors" accept=".pdf" />
            </div>

            <div class="mb-5">
                <label for="<%= txtStartDate.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Start Date</label>
                <asp:TextBox ID="txtStartDate" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors" TextMode="Date" />
            </div>

            <div class="mb-5">
                <label for="<%= txtEndDate.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">End Date</label>
                <asp:TextBox ID="txtEndDate" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors" TextMode="Date" />
            </div>

            <div class="mb-6">
                <label for="<%= txtRemarks.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Remarks</label>
                <asp:TextBox ID="txtRemarks" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors" TextMode="MultiLine" Rows="3" />
            </div>

            <asp:Button ID="btnUpload" runat="server" Text="Upload Contract"
                CssClass="w-full py-3 px-4 bg-blue-600 text-white font-bold rounded-lg shadow-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                OnClick="btnUpload_Click" OnClientClick="return confirmUpload();" />
        </div>
    </div>
    
    <script type="text/javascript">
        function confirmUpload() {
            event.preventDefault(); // prevent immediate postback
            Swal.fire({
                title: 'Are you sure?',
                text: "Do you want to upload this contract?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, upload it!',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#2563eb', // Tailwind's blue-600
                cancelButtonColor: '#6b7280'   // Tailwind's gray-500
            }).then((result) => {
                if (result.isConfirmed) {
                    // Use a small delay to ensure the event is fully handled before postback
                    setTimeout(function () {
                        __doPostBack('<%= btnUpload.UniqueID %>', '');
                    }, 100);
                }
            });
            return false;
        }
    </script>
</asp:Content>