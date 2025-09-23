<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="CreateBooking.aspx.cs" Inherits="RRCManagementSystem.CreateBooking" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

    <asp:HiddenField ID="hfInquiryVisible" runat="server" Value="false" />
    <asp:HiddenField ID="hfInquiryData" runat="server" Value="" />

    <div class="px-4 py-8 lg:px-8">
        <div class="bg-white rounded-xl shadow-lg mx-auto max-w-2xl overflow-hidden">
            <div class="p-6 md:p-8">
                <h2 class="text-3xl lg:text-4xl font-extrabold text-center text-blue-600 mb-8">
                    <i class="fas fa-tools mr-2"></i> Create Booking Quotation
                </h2>

                <asp:Label ID="lblMessage" runat="server" CssClass="block text-center font-semibold text-red-600 mb-4" />

                <asp:UpdatePanel ID="UpdatePanelMain" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="mb-5">
                            <label for="<%= txtClientSearch.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-1">Search Client</label>
                            <asp:TextBox ID="txtClientSearch" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-blue-500 focus:border-blue-500 transition-colors" placeholder="Type name or email..." />
                            <div class="text-sm text-gray-500 mt-1">Start typing to search a name, then select the desired client to create a quotation on.</div>
                            <ajaxToolkit:AutoCompleteExtender
                                ID="AutoCompleteExtender1" runat="server"
                                TargetControlID="txtClientSearch"
                                ServiceMethod="SearchClients"
                                MinimumPrefixLength="1"
                                CompletionSetCount="10"
                                EnableCaching="true"
                                FirstRowSelected="true"
                                OnClientItemSelected="setClientID" />
                            <asp:HiddenField ID="hfClientID" runat="server" />
                        </div>

                        <div id="inquiryBackground" class="mb-6 p-4 border border-gray-200 rounded-lg bg-gray-50" style="display:none;">
                            <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-3">
                                <h5 class="text-lg font-bold text-gray-800 mb-2 sm:mb-0">Inquiry Background</h5>
                                <small class="text-gray-400 text-sm" id="ibUpdated"></small>
                            </div>

                            <div class="mb-3">
                                <span class="font-semibold text-gray-700">Inquiry Code:</span>
                                <span id="ibCode" class="text-blue-600 font-semibold">—</span>
                            </div>

                            <div>
                                <span class="font-semibold text-gray-700">Recent Findings:</span>
                                <ul id="ibFindings" class="mb-0 mt-2 pl-5 list-disc list-inside text-sm text-gray-600"></ul>
                                <div id="ibEmpty" class="text-gray-400 text-sm mt-2">No findings found for this client yet.</div>
                            </div>
                        </div>

                        <div class="mb-5">
                            <label for="<%= ddlServices.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-1">Select Service</label>
                            <asp:DropDownList ID="ddlServices" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-blue-500 focus:border-blue-500 transition-colors"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlServices_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>

                        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-5">
                            <div>
                                <label for="<%= txtSQM.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-1">Square Meters (SQM)</label>
                                <asp:TextBox ID="txtSQM" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-blue-500 focus:border-blue-500 transition-colors" TextMode="Number"
                                    AutoPostBack="true" OnTextChanged="RecalculateTotal" />
                            </div>
                            <div>
                                <label for="<%= txtTravelExpense.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-1">Travel Expense (₱)</label>
                                <asp:TextBox ID="txtTravelExpense" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-blue-500 focus:border-blue-500 transition-colors" TextMode="Number"
                                    AutoPostBack="true" OnTextChanged="RecalculateTotal" />
                            </div>
                            <div>
                                <label for="<%= txtMiscellaneous.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-1">Miscellaneous (₱)</label>
                                <asp:TextBox ID="txtMiscellaneous" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-blue-500 focus:border-blue-500 transition-colors" TextMode="Number"
                                    AutoPostBack="true" OnTextChanged="RecalculateTotal" />
                            </div>
                        </div>

                        <div class="mb-6">
                            <label for="<%= txtTotalPrice.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-1">Total Price (₱)</label>
                            <asp:TextBox ID="txtTotalPrice" runat="server" CssClass="w-full px-4 py-2 bg-gray-100 text-gray-600 font-bold rounded-lg cursor-not-allowed" ReadOnly="true" />
                        </div>

                        <div class="flex justify-center">
                            <asp:Button ID="btnSubmit" runat="server" Text="Submit Quotation"
                                CssClass="w-full sm:w-auto px-8 py-3 bg-blue-600 text-white font-bold rounded-lg shadow-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
                                OnClick="btnSubmit_Click" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script type="text/javascript">
        function setClientID(source, eventArgs) {
            var clientName = eventArgs.get_text();
            var clientID = eventArgs.get_value();
            document.getElementById('<%= txtClientSearch.ClientID %>').value = clientName;
            document.getElementById('<%= hfClientID.ClientID %>').value = clientID;

            // Show Inquiry panel
            document.getElementById('<%= hfInquiryVisible.ClientID %>').value = "true";
            document.getElementById('inquiryBackground').style.display = "block";

            // Fetch Inquiry Data
            PageMethods.GetClientInquirySummary(parseInt(clientID, 10),
                function (res) {
                    var jsonString = JSON.stringify(res);
                    document.getElementById('<%= hfInquiryData.ClientID %>').value = jsonString; // Persist data
                    renderInquiry(res);
                },
                function () {
                    document.getElementById('ibCode').textContent = "—";
                    document.getElementById('ibFindings').innerHTML = "";
                    document.getElementById('ibEmpty').style.display = "";
                    document.getElementById('ibUpdated').textContent = "";
                    document.getElementById('inquiryBackground').style.display = "block";
                });
        }

        // Renders Inquiry Background from object
        function renderInquiry(data) {
            var box = document.getElementById('inquiryBackground');
            var code = document.getElementById('ibCode');
            var list = document.getElementById('ibFindings');
            var empty = document.getElementById('ibEmpty');
            var upd = document.getElementById('ibUpdated');

            list.innerHTML = "";
            empty.style.display = "none";
            upd.textContent = "";

            if (!data) {
                code.textContent = "—";
                empty.style.display = "";
                box.style.display = "block";
                return;
            }

            code.textContent = data.InquiryCode || "—";

            if (data.Findings && data.Findings.length > 0) {
                data.Findings.forEach(function (f) {
                    var li = document.createElement("li");
                    li.className = "text-gray-600 text-sm";
                    var when = f.When ? (" (" + f.When + ")") : "";
                    li.textContent = f.Text + when;
                    list.appendChild(li);
                });
                if (data.LastUpdated) upd.textContent = "Updated: " + data.LastUpdated;
            } else {
                empty.style.display = "";
            }

            box.style.display = "block";
        }

        // Reload Inquiry after postback (service selection)
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            var json = document.getElementById('<%= hfInquiryData.ClientID %>').value;
            if (json) {
                renderInquiry(JSON.parse(json));
            }
        });
    </script>
</asp:Content>