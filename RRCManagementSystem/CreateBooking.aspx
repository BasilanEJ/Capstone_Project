<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="CreateBooking.aspx.cs" Inherits="RRCManagementSystem.CreateBooking" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

    <asp:HiddenField ID="hfInquiryVisible" runat="server" Value="false" />
    <asp:HiddenField ID="hfInquiryData" runat="server" Value="" />
    <asp:HiddenField ID="hfTravelExpense" runat="server" Value="0" />
     <asp:HiddenField ID="hfMiscellaneousItems" runat="server" Value="[]" />
    <asp:HiddenField ID="hfTotalPrice" runat="server" Value="0" />

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

                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-5">
                            <div>
                                <label for="<%= txtSQM.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-1">Square Meters (SQM)</label>
                                <asp:TextBox ID="txtSQM" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-blue-500 focus:border-blue-500 transition-colors" TextMode="Number" />
                            </div>
                            <div>
                                <label for="<%= txtTravelExpense.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-1">
                                    Travel Expense (₱) 
                                    <span class="text-xs text-gray-500">(Auto)</span>
                                </label>
                                <asp:TextBox ID="txtTravelExpense" runat="server" 
                                    CssClass="w-full px-4 py-2 bg-gray-100 border border-gray-300 rounded-lg cursor-not-allowed text-gray-700" 
                                    TextMode="Number" ReadOnly="true" />
                            </div>
                        </div>

                        <!-- NEW: Miscellaneous Items Section -->
                        <div class="mb-5">
                            <label class="block text-sm font-semibold text-gray-700 mb-2">
                                Miscellaneous Expenses 
                                <span class="text-xs text-gray-500">(Optional)</span>
                            </label>
                            
                            <!-- Add New Expense Item -->
                            <div class="border border-gray-200 rounded-lg p-4 bg-gray-50 mb-3">
                                <div class="grid grid-cols-1 md:grid-cols-12 gap-3">
                                    <div class="md:col-span-7">
                                        <input type="text" id="txtExpenseDescription" 
                                            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-blue-500 focus:border-blue-500" 
                                            placeholder="e.g., Food Allowance, Parking Fee" />
                                    </div>
                                    <div class="md:col-span-3">
                                        <input type="number" id="txtExpenseAmount" 
                                            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-blue-500 focus:border-blue-500" 
                                            placeholder="Amount (₱)" step="0.01" min="0" />
                                    </div>
                                    <div class="md:col-span-2">
                                        <button type="button" id="btnAddExpense" 
                                            class="w-full px-4 py-2 bg-green-600 text-white font-semibold rounded-lg hover:bg-green-700 transition-colors">
                                            <i class="fas fa-plus mr-1"></i> Add
                                        </button>
                                    </div>
                                </div>
                            </div>

                            <!-- Expense Items List -->
                            <div id="expenseItemsContainer" class="space-y-2 mb-3">
                                <!-- Items will be added here dynamically -->
                            </div>

                            <!-- Total Miscellaneous -->
                            <div class="flex justify-between items-center p-3 bg-blue-50 border border-blue-200 rounded-lg">
                                <span class="font-semibold text-gray-700">Total Miscellaneous:</span>
                                <span id="lblMiscTotal" class="text-lg font-bold text-blue-600">₱0.00</span>
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
    // Store miscellaneous expense items
    var miscExpenseItems = [];

    function setClientID(source, eventArgs) {
        var clientName = eventArgs.get_text();
        var clientID = eventArgs.get_value();
        document.getElementById('<%= txtClientSearch.ClientID %>').value = clientName;
        document.getElementById('<%= hfClientID.ClientID %>').value = clientID;

        document.getElementById('<%= hfInquiryVisible.ClientID %>').value = "true";
        document.getElementById('inquiryBackground').style.display = "block";

        document.getElementById('<%= txtTravelExpense.ClientID %>').value = "0.00";
        document.getElementById('<%= hfTravelExpense.ClientID %>').value = "0.00";

        PageMethods.GetClientInquirySummary(parseInt(clientID, 10),
            function (res) {
                var jsonString = JSON.stringify(res);
                document.getElementById('<%= hfInquiryData.ClientID %>').value = jsonString;
                renderInquiry(res);
            },
            function () {
                document.getElementById('ibCode').textContent = "—";
                document.getElementById('ibFindings').innerHTML = "";
                document.getElementById('ibEmpty').style.display = "";
                document.getElementById('ibUpdated').textContent = "";
                document.getElementById('inquiryBackground').style.display = "block";
            });

        PageMethods.GetClientTravelExpense(parseInt(clientID, 10),
            function (travelPrice) {
                var formattedPrice = (travelPrice && travelPrice > 0) ? travelPrice.toFixed(2) : "0.00";
                document.getElementById('<%= txtTravelExpense.ClientID %>').value = formattedPrice;
                document.getElementById('<%= hfTravelExpense.ClientID %>').value = formattedPrice;

                console.log("Travel expense set to:", formattedPrice);
                calculateTotalClientSide();
            },
            function (error) {
                console.error("Failed to fetch travel expense:", error);
                document.getElementById('<%= txtTravelExpense.ClientID %>').value = "0.00";
                document.getElementById('<%= hfTravelExpense.ClientID %>').value = "0.00";
                calculateTotalClientSide();
            });
    }

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

    function addExpenseItem() {
        var descInput = document.getElementById('txtExpenseDescription');
        var amountInput = document.getElementById('txtExpenseAmount');

        if (!descInput || !amountInput) {
            console.error("Expense input fields not found");
            return;
        }

        var description = descInput.value.trim();
        var amount = parseFloat(amountInput.value) || 0;

        if (description === "") {
            Swal.fire('Missing', 'Please enter an expense description.', 'warning');
            return;
        }

        if (amount <= 0) {
            Swal.fire('Invalid Amount', 'Please enter a valid amount greater than 0.', 'warning');
            return;
        }

        var item = {
            description: description,
            amount: amount
        };
        miscExpenseItems.push(item);

        document.getElementById('<%= hfMiscellaneousItems.ClientID %>').value = JSON.stringify(miscExpenseItems);
        renderExpenseItems();
        descInput.value = "";
        amountInput.value = "";
        calculateTotalClientSide();
    }

    function renderExpenseItems() {
        var container = document.getElementById('expenseItemsContainer');
        if (!container) return;

        container.innerHTML = "";

        if (miscExpenseItems.length === 0) {
            container.innerHTML = '<div class="text-gray-400 text-sm text-center py-2">No expenses added yet.</div>';
            document.getElementById('lblMiscTotal').textContent = "₱0.00";
            return;
        }

        var total = 0;
        miscExpenseItems.forEach(function (item, index) {
            total += item.amount;

            var itemDiv = document.createElement('div');
            itemDiv.className = "flex justify-between items-center p-3 bg-white border border-gray-200 rounded-lg";
            itemDiv.innerHTML = 
                '<div class="flex-1">' +
                    '<span class="font-medium text-gray-700">' + escapeHtml(item.description) + '</span>' +
                '</div>' +
                '<div class="flex items-center gap-3">' +
                    '<span class="font-semibold text-gray-900">₱' + item.amount.toFixed(2) + '</span>' +
                    '<button type="button" onclick="removeExpenseItem(' + index + ')" ' +
                        'class="text-red-600 hover:text-red-800 transition-colors">' +
                        '<i class="fas fa-trash-alt"></i>' +
                    '</button>' +
                '</div>';
            container.appendChild(itemDiv);
        });

        document.getElementById('lblMiscTotal').textContent = "₱" + total.toFixed(2);
    }

    function escapeHtml(text) {
        var map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, function(m) { return map[m]; });
    }

    function removeExpenseItem(index) {
        miscExpenseItems.splice(index, 1);
        document.getElementById('<%= hfMiscellaneousItems.ClientID %>').value = JSON.stringify(miscExpenseItems);
        renderExpenseItems();
        calculateTotalClientSide();
    }

    function getMiscellaneousTotal() {
        var total = 0;
        miscExpenseItems.forEach(function(item) {
            total += item.amount;
        });
        return total;
    }

    function calculateTotalClientSide() {
        var sqmInput = document.getElementById('<%= txtSQM.ClientID %>');
        var travelHidden = document.getElementById('<%= hfTravelExpense.ClientID %>');
        var serviceSelect = document.getElementById('<%= ddlServices.ClientID %>');
        var totalInput = document.getElementById('<%= txtTotalPrice.ClientID %>');
        var totalHidden = document.getElementById('<%= hfTotalPrice.ClientID %>');

        if (!sqmInput || !travelHidden || !serviceSelect || !totalInput || !totalHidden) {
            console.error("Required fields not found");
            return;
        }

        var sqm = parseInt(sqmInput.value) || 0;
        var travel = parseFloat(travelHidden.value) || 0;
        var misc = getMiscellaneousTotal();
        var serviceId = serviceSelect.value;

        console.log("=== CALCULATION START ===");
        console.log("SQM:", sqm);
        console.log("Travel (from hidden):", travel);
        console.log("Misc:", misc);
        console.log("ServiceID:", serviceId);

        if (serviceId && serviceId !== "" && sqm > 0) {
            PageMethods.GetServicePrice(parseInt(serviceId), sqm,
                function(servicePrice) {
                    console.log("Service Price returned:", servicePrice);
                    var total = (servicePrice || 0) + travel + misc;
                    console.log("Final Total:", total, "=", servicePrice, "+", travel, "+", misc);
                    
                    totalInput.value = total.toFixed(2);
                    totalHidden.value = total.toFixed(2);
                },
                function(error) {
                    console.error("Error fetching service price:", error);
                    var total = travel + misc;
                    totalInput.value = total.toFixed(2);
                    totalHidden.value = total.toFixed(2);
                }
            );
        } else {
            var total = travel + misc;
            console.log("No service/SQM - Total:", total);
            totalInput.value = total.toFixed(2);
            totalHidden.value = total.toFixed(2);
        }
    }

    function attachEventListeners() {
        var txtSQM = document.getElementById('<%= txtSQM.ClientID %>');
        var ddlServices = document.getElementById('<%= ddlServices.ClientID %>');
        var btnAddExpense = document.getElementById('btnAddExpense');
        var txtExpenseAmount = document.getElementById('txtExpenseAmount');
        var txtExpenseDescription = document.getElementById('txtExpenseDescription');

        if (txtSQM) {
            // Remove old listeners
            var newSQM = txtSQM.cloneNode(true);
            txtSQM.parentNode.replaceChild(newSQM, txtSQM);
            newSQM.addEventListener('input', calculateTotalClientSide);
            console.log("✓ SQM input listener attached");
        }

        if (ddlServices) {
            var newDdl = ddlServices.cloneNode(true);
            ddlServices.parentNode.replaceChild(newDdl, ddlServices);
            newDdl.addEventListener('change', calculateTotalClientSide);
            console.log("✓ Service dropdown listener attached");
        }

        if (btnAddExpense) {
            btnAddExpense.onclick = addExpenseItem;
            console.log("✓ Add expense button listener attached");
        }

        if (txtExpenseAmount) {
            txtExpenseAmount.addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    addExpenseItem();
                }
            });
        }

        if (txtExpenseDescription) {
            txtExpenseDescription.addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    addExpenseItem();
                }
            });
        }
    }

    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
        console.log("=== UpdatePanel Postback Completed ===");

        var json = document.getElementById('<%= hfInquiryData.ClientID %>').value;
        if (json) {
            renderInquiry(JSON.parse(json));
        }
        
        var travelExpense = document.getElementById('<%= hfTravelExpense.ClientID %>').value;
        if (travelExpense && travelExpense !== "0") {
            document.getElementById('<%= txtTravelExpense.ClientID %>').value = travelExpense;
            console.log("Restored travel expense:", travelExpense);
        }

        var miscItems = document.getElementById('<%= hfMiscellaneousItems.ClientID %>').value;
        if (miscItems && miscItems !== "[]" && miscItems !== "") {
            try {
                miscExpenseItems = JSON.parse(miscItems);
                renderExpenseItems();
            } catch (e) {
                console.error("Error parsing misc items:", e);
                miscExpenseItems = [];
            }
        }

        attachEventListeners();
        calculateTotalClientSide();
    });

    document.addEventListener('DOMContentLoaded', function () {
        console.log("=== DOM Content Loaded ===");
        attachEventListeners();
        renderExpenseItems();
    });

    window.addEventListener('load', function () {
        console.log("=== Window Loaded ===");
        attachEventListeners();
    });
</script>

</asp:Content>