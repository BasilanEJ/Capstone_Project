<%@ Page Title="" Language="C#" MasterPageFile="~/Inspector.master" AutoEventWireup="true" CodeBehind="MyInspections.aspx.cs" Inherits="RRCManagementSystem.MyInspections" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="px-4 py-8 lg:px-8">
        <h2 class="text-3xl lg:text-4xl font-extrabold text-blue-600 mb-8 flex items-center">
            <i class="fas fa-search-location mr-2"></i> My Inspections
        </h2>

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

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            <asp:Repeater ID="rptInspections" runat="server">
                <ItemTemplate>
                    <div class="bg-white rounded-xl shadow-lg hover:shadow-xl transition-shadow duration-300 overflow-hidden border-l-4 <%# Eval("InspectionStatus").ToString() == "Pending" ? "border-yellow-500" : "border-green-500" %>">
                        <div class="p-6 flex flex-col h-full">
                            <h5 class="text-xl font-bold text-blue-600 mb-2">
                                <i class="fas fa-file-invoice mr-2"></i> Inspection #<%# Eval("InspectionID") %>
                            </h5>
                            
                            <div class="text-gray-700 space-y-2 text-sm flex-grow">
                                <p><strong class="font-semibold">Inquiry Code:</strong> <span class="break-words"><%# Eval("InquiryCode") %></span></p>
                                <p><strong class="font-semibold">Name:</strong> <span class="break-words"><%# Eval("FullName") %></span></p>
                                <p class="break-words">
                                    <strong class="font-semibold">Address:</strong>
                                    <span class="block">
                                        <%# Eval("StreetAndUnit") %>, <%# Eval("Barangay") %>, <%# Eval("City") %>, <%# Eval("Region") %>, <%# Eval("Country") %>
                                    </span>
                                    <em><%# !string.IsNullOrEmpty(Eval("Landmark")?.ToString()) ? "(Landmark: " + Eval("Landmark") + ")" : "" %></em>
                                </p>
                                <p><strong class="font-semibold">Scheduled:</strong> <%# Eval("ScheduledDate", "{0:yyyy-MM-dd hh:mm tt}") %></p>
                                <p><strong class="font-semibold">Status:</strong> 
                                    <span class="inline-block px-2 py-1 rounded-full text-xs font-bold text-white <%# Eval("InspectionStatus").ToString() == "Pending" ? "bg-yellow-500" : "bg-green-500" %>">
                                        <%# Eval("InspectionStatus") %>
                                    </span>
                                </p>
                                <p><strong class="font-semibold">Remarks:</strong> <span class="break-words"><%# Eval("Remarks") %></span></p>
                                <p><strong class="font-semibold">Findings:</strong> <span class="break-words"><%#: Eval("Findings") %></span></p>
                            </div>
                            
                            <div class="mt-4 pt-4 border-t border-gray-200">
                                <%# Eval("InspectionStatus").ToString() == "Pending"
                                    ? "<button type=\"button\" class=\"bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded-md text-sm transition-colors\" onclick=\"markDoneWithFindings('" + Eval("InspectionID") + "')\">Mark as Done</button>"
                                    : "" %>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function markDoneWithFindings(inspectionId) {
            // Fetch the services list dynamically
            fetch('MyInspections.aspx?getServices=1')
                .then(response => response.json())
                .then(services => {
                    // Build dropdown options grouped by ServiceType
                    let optionsHtml = '<select id="serviceDropdown" class="swal2-select" style="width:100%;">';
                    optionsHtml += '<option value="">-- Select a service --</option>';

                    let currentGroup = '';
                    services.forEach(service => {
                        if (service.ServiceType !== currentGroup) {
                            if (currentGroup !== '') {
                                optionsHtml += '</optgroup>'; // close previous group
                            }
                            currentGroup = service.ServiceType;
                            optionsHtml += `<optgroup label="${currentGroup}">`;
                        }
                        optionsHtml += `<option value="${service.Name}">${service.Name}</option>`;
                    });
                    if (currentGroup !== '') {
                        optionsHtml += '</optgroup>'; // close last group
                    }
                    optionsHtml += '</select>';

                    Swal.fire({
                        title: 'Mark as Done?',
                        html: `
                        <label style="font-weight:bold;">Describe your findings:</label>
                        <textarea id="findingsInput" class="swal2-textarea" placeholder="Describe your findings..."></textarea>
                        <br>
                        <label style="font-weight:bold;">Select Service:</label>
                        ${optionsHtml}
                    `,
                        focusConfirm: false,
                        showCancelButton: true,
                        confirmButtonColor: '#198754',
                        cancelButtonColor: '#d33',
                        confirmButtonText: 'Yes, Mark Done',
                        preConfirm: () => {
                            const findings = document.getElementById('findingsInput').value.trim();
                            const serviceName = document.getElementById('serviceDropdown').value;

                            if (!findings) {
                                Swal.showValidationMessage('Findings are required.');
                                return false;
                            }
                            if (!serviceName) {
                                Swal.showValidationMessage('Please select a service.');
                                return false;
                            }

                            return { findings, serviceName };
                        }
                    }).then((result) => {
                        if (result.isConfirmed) {
                            const combinedText = `[Service: ${result.value.serviceName}] - ${result.value.findings}`;
                            const encoded = encodeURIComponent(combinedText);
                            window.location.href = `MyInspections.aspx?done=${inspectionId}&findings=${encoded}`;
                        }
                    });
                });
        }
    </script>
</asp:Content>