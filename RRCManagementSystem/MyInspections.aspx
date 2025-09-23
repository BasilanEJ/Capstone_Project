<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="MyInspections.aspx.cs" Inherits="RRCManagementSystem.MyInspections" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="px-4 py-8 lg:px-8">
        <!-- Page Title -->
        <h2 class="text-3xl lg:text-4xl font-extrabold text-blue-600 mb-8 flex items-center">
            <i class="fas fa-search-location mr-2"></i> My Inspections
        </h2>

        <!-- Filter Dropdown -->
        <div class="mb-6 max-w-sm">
            <label for="ddlStatusFilter" class="block text-sm font-medium text-gray-700 mb-2">Filter by Status</label>
            <asp:DropDownList ID="ddlStatusFilter" runat="server" AutoPostBack="true"
                OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged"
                CssClass="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 transition duration-150 ease-in-out">
                <asp:ListItem Text="All" Value="All" />
                <asp:ListItem Text="Pending" Value="Pending" />
                <asp:ListItem Text="Completed" Value="Completed" />
            </asp:DropDownList>
        </div>

        <!-- Inspections Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            <asp:Repeater ID="rptInspections" runat="server">
                <ItemTemplate>
                    <div class='bg-white rounded-xl shadow-lg hover:shadow-xl transition-shadow duration-300 overflow-hidden border-l-4 <%# Eval("InspectionStatus").ToString() == "Pending" ? "border-yellow-500" : "border-green-500" %>'>
                        <div class="p-6 flex flex-col h-full">
                            <!-- Header -->
                            <h5 class="text-xl font-bold text-blue-600 mb-2">
                                <i class="fas fa-file-invoice mr-2"></i> Inspection #<%# Eval("InspectionID") %>
                            </h5>
                            
                            <!-- Details -->
                            <div class="text-gray-700 space-y-2 text-sm flex-grow">
                                <p>
                                    <strong class="font-semibold">Inquiry Code:</strong>
                                    <span class="break-words"><%# Eval("InquiryCode") %></span>
                                </p>
                                <p>
                                    <strong class="font-semibold">Name:</strong>
                                    <span class="break-words"><%# Eval("FullName") %></span>
                                </p>
                                <p class="break-words">
                                    <strong class="font-semibold">Address:</strong>
                                    <span class="block">
                                        <%# Eval("StreetAndUnit") %>, <%# Eval("Barangay") %>, <%# Eval("City") %>, <%# Eval("Region") %>, <%# Eval("Country") %>
                                    </span>
                                    <em><%# !string.IsNullOrEmpty(Eval("Landmark")?.ToString()) ? "(Landmark: " + Eval("Landmark") + ")" : "" %></em>
                                </p>
                                <p>
                                    <strong class="font-semibold">Scheduled:</strong> 
                                    <%# Eval("ScheduledDate", "{0:yyyy-MM-dd hh:mm tt}") %>
                                </p>
                                <p>
                                    <strong class="font-semibold">Status:</strong> 
                                    <span class='inline-block px-2 py-1 rounded-full text-xs font-bold text-white <%# Eval("InspectionStatus").ToString() == "Pending" ? "bg-yellow-500" : "bg-green-500" %>'>
                                        <%# Eval("InspectionStatus") %>
                                    </span>
                                </p>
                                <p>
                                    <strong class="font-semibold">Remarks:</strong> 
                                    <span class="break-words"><%# Eval("Remarks") %></span>
                                </p>
                                <p>
                                    <strong class="font-semibold">Findings:</strong> 
                                    <span class="break-words"><%#: Eval("Findings") %></span>
                                </p>
                            </div>
                            
                            <!-- Action Button -->
                            <div class="mt-4 pt-4 border-t border-gray-200">
                                <%# GetActionButton(Eval("ScheduledDate"), Eval("InspectionStatus"), Eval("InspectionID")) %>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </div>

    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        // Mark inspection as done with service, SQM, and findings
        function markDoneWithFindings(inspectionId) {
            fetch('MyInspections.aspx?getServices=1')
                .then(response => response.json())
                .then(services => {
                    // Build grouped dropdown for services
                    let optionsHtml = `
                <select id="serviceDropdown" class="w-full p-2 rounded-md border border-gray-300 focus:border-blue-500 focus:ring focus:ring-blue-200 text-gray-700">
                    <option value="">-- Select a service --</option>
            `;

                    let currentGroup = '';
                    services.forEach(service => {
                        if (service.ServiceType !== currentGroup) {
                            if (currentGroup !== '') {
                                optionsHtml += '</optgroup>';
                            }
                            currentGroup = service.ServiceType;
                            optionsHtml += `<optgroup label="${currentGroup}">`;
                        }
                        optionsHtml += `<option value="${service.Name}">${service.Name}</option>`;
                    });
                    if (currentGroup !== '') {
                        optionsHtml += '</optgroup>';
                    }
                    optionsHtml += '</select>';

                    // SweetAlert modal
                    Swal.fire({
                        title: '<span class="text-xl md:text-2xl font-bold text-gray-800">Mark as Done</span>',
                        html: `
                    <div class="space-y-4 text-left">

                        <!-- Step 1: Select Service -->
                        <div>
                            <label for="serviceDropdown" class="block text-sm font-semibold text-gray-700 mb-1">
                                Select Service:
                            </label>
                            ${optionsHtml}
                        </div>

                        <!-- Step 2: SQM -->
                        <div>
                            <label for="sqmInput" class="block text-sm font-semibold text-gray-700 mb-1">
                                SQM (Square Meters):
                            </label>
                            <input 
                                type="number" 
                                id="sqmInput" 
                                class="w-full p-2 border border-gray-300 rounded-md focus:border-blue-500 focus:ring focus:ring-blue-200" 
                                placeholder="Enter area size in SQM" 
                                min="1" />
                        </div>

                        <!-- Step 3: Findings -->
                        <div>
                            <label for="findingsInput" class="block text-sm font-semibold text-gray-700 mb-1">
                                Findings / Remarks:
                            </label>
                            <textarea 
                                id="findingsInput" 
                                class="w-full p-2 border border-gray-300 rounded-md focus:border-blue-500 focus:ring focus:ring-blue-200 resize-none" 
                                placeholder="Describe your findings..." 
                                rows="4"></textarea>
                        </div>
                    </div>
                `,
                        focusConfirm: false,
                        showCancelButton: true,
                        width: '95%',
                        customClass: {
                            popup: 'rounded-xl shadow-lg bg-white max-w-md w-full md:w-[500px]',
                            confirmButton: 'px-6 py-2 text-white font-semibold rounded-md bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-300',
                            cancelButton: 'px-6 py-2 text-white font-semibold rounded-md bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-300'
                        },
                        confirmButtonText: '<i class="fas fa-check-circle mr-2"></i> Yes, Mark Done',
                        cancelButtonText: '<i class="fas fa-times mr-2"></i> Cancel',

                        preConfirm: () => {
                            const serviceName = document.getElementById('serviceDropdown').value;
                            const sqmValue = document.getElementById('sqmInput').value.trim();
                            const findings = document.getElementById('findingsInput').value.trim();

                            if (!serviceName) {
                                Swal.showValidationMessage('Please select a service.');
                                return false;
                            }
                            if (!sqmValue || parseFloat(sqmValue) <= 0) {
                                Swal.showValidationMessage('Please enter a valid SQM greater than 0.');
                                return false;
                            }
                            if (!findings) {
                                Swal.showValidationMessage('Please provide your findings.');
                                return false;
                            }

                            return { serviceName, sqmValue, findings };
                        }
                    }).then((result) => {
                        if (result.isConfirmed) {
                            // Combine into a single text block
                            const combinedText =
                                `Service: ${result.value.serviceName}\n` +
                                `SQM: ${result.value.sqmValue}\n` +
                                `Remarks: ${result.value.findings}`;

                            const encodedFindings = encodeURIComponent(combinedText);

                            // Send to backend as just "findings"
                            window.location.href = `MyInspections.aspx?done=${inspectionId}&findings=${encodedFindings}`;
                        }
                    });
                });
        }

    </script>
</asp:Content>
