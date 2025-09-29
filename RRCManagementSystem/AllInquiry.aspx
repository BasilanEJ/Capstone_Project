<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AllInquiry.aspx.cs" Inherits="RRCManagementSystem.AllInquiry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    
    <style>
        .table-rounded-corners {
            border-collapse: separate;
            border-spacing: 0;
            border-radius: 0.5rem;
            overflow: hidden;
        }
        .table-rounded-corners thead tr:first-child th:first-child {
            border-top-left-radius: 0.5rem;
        }
        .table-rounded-corners thead tr:first-child th:last-child {
            border-top-right-radius: 0.5rem;
        }
        #inspectorScheduleContainer {
    scroll-behavior: smooth;
}
        @media (max-width: 640px) {
    .swal2-popup {
        width: 95% !important;
        max-height: 90vh !important;
    }
}



    </style>

    <div class="container mx-auto p-4 md:p-8 bg-white rounded-lg shadow-lg mt-8">
        <h2 class="text-2xl font-bold mb-6 text-gray-800 text-center">📋 Manage Inquiries</h2>
        <asp:Label ID="lblPermission" runat="server" CssClass="text-red-500 font-semibold text-center block mb-4" />

        <asp:HiddenField ID="hfSelectedInquiryID" runat="server" />
        <asp:HiddenField ID="hfAssignData" runat="server" />
        <asp:Button ID="btnAssignHidden" runat="server" OnClick="btnAssignHidden_Click"
            Style="display:none;" UseSubmitBehavior="false" />

        <asp:HiddenField ID="hfDeleteInquiryID" runat="server" />
        <asp:Button ID="btnDeleteHidden" runat="server" OnClick="btnDeleteHidden_Click" Style="display:none;" />
        <asp:DropDownList ID="ddlInspectorSource" runat="server" Style="display:none;"></asp:DropDownList>

        <div class="overflow-x-auto shadow-lg rounded-lg">
            <asp:GridView ID="gvInquiries" runat="server" AutoGenerateColumns="False"
                CssClass="min-w-full bg-white table-rounded-corners"
                DataKeyNames="InquiryID,InquiryCode"
                OnRowCommand="gvInquiries_RowCommand"
                OnRowDataBound="gvInquiries_RowDataBound"
                HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-sm leading-normal"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors"
                AlternatingRowStyle-CssClass="bg-gray-50 hover:bg-gray-100 transition-colors">

                <Columns>
                    <asp:BoundField DataField="InquiryCode" HeaderText="Reference Code"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />

                    <asp:BoundField DataField="Email" HeaderText="Client Email"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />

                    <asp:BoundField DataField="ContactNumber" HeaderText="Contact Number"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />

                    <asp:TemplateField HeaderText="Message"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 border-r border-gray-200 max-w-xs overflow-hidden text-ellipsis whitespace-nowrap">
                        <ItemTemplate>
                            <span class="block max-w-xs overflow-hidden text-ellipsis whitespace-nowrap">
                                <%# Eval("Message") %>
                            </span>
                           <asp:LinkButton ID="lnkViewMessage" runat="server"
                                CssClass="text-blue-500 hover:underline ml-1 text-sm"
                                OnClientClick='<%# "return showFullMessage(\"" + HttpUtility.JavaScriptStringEncode(Eval("Message").ToString()) + "\");" %>'
                                Visible='<%# Eval("Message") != null && Eval("Message").ToString().Length > 50 %>'>
                                See more
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Photo"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200">
                        <ItemTemplate>
                            <asp:Image ID="imgPhoto" runat="server"
                                Width="60" Height="60" CssClass="rounded-md"
                                ImageUrl='<%# string.IsNullOrEmpty(Eval("PhotoPath") as string) 
                                        ? ResolveUrl("~/Images/no-image.png") 
                                        : ResolveUrl(Eval("PhotoPath").ToString()) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="SubmittedAt" HeaderText="Submitted At" DataFormatString="{0:g}"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center border-r border-gray-200" />

                    <asp:TemplateField HeaderText="Action"
                        HeaderStyle-CssClass="py-3 px-6 text-center border-r border-gray-200"
                        ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <asp:Button ID="btnAssign" runat="server" Text="Assign Inspector"
                                CssClass="px-4 py-2 bg-blue-500 text-white font-bold rounded-md hover:bg-blue-600 transition-colors text-sm"
                                CommandName="Assign"
                                CommandArgument='<%# Eval("InquiryID") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Archive"
                        HeaderStyle-CssClass="py-3 px-6 text-center"
                        ItemStyle-CssClass="py-3 px-6 text-center">
                        <ItemTemplate>
                            <button type="button"
                                class="px-4 py-2 bg-yellow-500 text-white font-bold rounded-md hover:bg-yellow-600 transition-colors text-sm"
                                onclick='confirmArchive(<%# Eval("InquiryID") %>)'>
                                Archive
                            </button>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

<script type="text/javascript">
    const regionCities = {
        "NCR": ["Quezon City", "Manila", "Makati", "Caloocan", "Las Piñas", "Pasig",
            "Taguig", "Valenzuela", "Pasay", "Malabon", "Mandaluyong", "Marikina", "Muntinlupa", "Navotas", "San Juan", "Pateros", "Parañaque"],
        "Region I": ["Alaminos", "Batac", "Candon", "Laoag", "Vigan", "San Fernando", "San Carlos", "Dagupan", "Urdaneta"],
        "Region II": ["Cauayan", "Tuguegarao", "Ilagan", "Santiago"],
        "Region III": ["San Fernando", "Angeles", "Olongapo", "Balanga", "Baliwag", "Cabanatuan", "Gapan", "Mabalacat", "Malolos", "Meycauayan", "Muñoz", "Palayan", "San Jose", "San Jose del Monte", "Tarlac City"],
        "Region IV-A": ["Cavite", "Batangas", "Lucena", "Antipolo", "Bacoor", "Biñan", "Cabuyao", "Calaca", "Calamba", "Carmona", "Dasmariñas", "General Trias", "Imus", "Lipa", "San Pablo", "San Pedro", "Santa Rosa", "Santo Tomas", "Tagaytay", "Tanauan", "Tayabas", "Trece Martires"],
        "Region IV-B": ["Puerto Princesa", "Calapan"],
        "Region V": ["Legazpi", "Naga", "Iriga", "Ligao", "Masbate City", "Sorsogon City", "Tabaco"],
        "Region VI": ["Iloilo City", "Passi", "Bacolod", "Roxas City"],
        "Region VII": ["Cebu City", "Dumaguete", "Lapu-Lapu City", "Mandaue", "Bogo", "Carcar", "Danao", "Naga", "Tagbilaran", "Talisay", "Toledo"],
    };

    function loadCitiesForRegion(regionValue) {
        const cityDropdown = document.getElementById('swalCity');
        cityDropdown.innerHTML = '<option value="">-- Select City --</option>';
        if (regionValue && regionCities[regionValue]) {
            regionCities[regionValue].forEach(city => {
                const option = document.createElement('option');
                option.value = city;
                option.textContent = city;
                cityDropdown.appendChild(option);
            });
        } else {
            const option = document.createElement('option');
            option.value = "";
            option.textContent = "No Cities Available";
            cityDropdown.appendChild(option);
        }
    }

    function showAssignModal(el, inquiryId) {
        document.getElementById('<%= hfSelectedInquiryID.ClientID %>').value = inquiryId;

        const d = el && el.dataset ? el.dataset : {};
        const val = (s) => (s || '').replace(/"/g, '&quot;');

        const now = new Date();
        const pad = (n) => String(n).padStart(2, '0');
        const minDateTime = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:${pad(now.getMinutes())}`;

        Swal.fire({
            title: '<span class="text-xl font-bold text-gray-800">🛠️ Assign Inspector</span>',
            width: '100%',
            customClass: {
                container: '!w-full !m-0 !p-0 !max-w-none flex justify-center items-center',
                htmlContainer: '!my-0',
                popup: '!p-4 md:!p-8 !w-11/12 md:!w-2/3 lg:!w-1/2 max-h-[90vh] overflow-y-auto rounded-lg'
            },
            html: `
                <div class="space-y-2">
                    <div class="flex flex-col md:flex-row md:space-x-4 space-y-2 md:space-y-0">
                        <div class="flex-1 flex flex-col gap-1">
                            <label class="text-sm font-medium text-gray-700">First Name</label>
                            <input id="swalFirstName" class="w-full px-3 py-2 border rounded-md" placeholder="First Name" value="${val(d.fn)}">
                        </div>
                        <div class="flex-1 flex flex-col gap-1">
                            <label class="text-sm font-medium text-gray-700">Last Name</label>
                            <input id="swalLastName" class="w-full px-3 py-2 border rounded-md" placeholder="Last Name" value="${val(d.ln)}">
                        </div>
                    </div>
                    
                    <div class="flex flex-col md:flex-row md:space-x-4 space-y-2 md:space-y-0">
                        <div class="flex-1 flex flex-col gap-1">
                            <label class="text-sm font-medium text-gray-700">Middle Name</label>
                            <input id="swalMiddleName" class="w-full px-3 py-2 border rounded-md" placeholder="Middle Name" value="${val(d.mn)}">
                        </div>
                        <div class="flex-1 flex flex-col gap-1">
                            <label class="text-sm font-medium text-gray-700">Country</label>
                            <input id="swalCountry" class="w-full px-3 py-2 border rounded-md" value="${val(d.ctry || 'Philippines')}">
                        </div>
                    </div>

                    <div class="flex flex-col md:flex-row md:space-x-4 space-y-2 md:space-y-0">
                        <div class="flex-1 flex flex-col gap-1">
                            <label class="text-sm font-medium text-gray-700">Region</label>
                            <select id="swalRegion" class="w-full px-3 py-2 border rounded-md">
                                <option value="">-- Select Region --</option>
                                <option value="NCR">NCR - National Capital Region</option>
                                <option value="Region I">Region I - Ilocos Region</option>
                                <option value="Region II">Region II - Cagayan Valley</option>
                                <option value="Region III">Region III - Central Luzon</option>
                                <option value="Region IV-A">Region IV-A - CALABARZON</option>
                                <option value="Region IV-B">Region IV-B - MIMAROPA</option>
                                <option value="Region V">Region V - Bicol Region</option>
                                <option value="Region VI">Region VI - Western Visayas</option>
                                <option value="Region VII">Region VII - Central Visayas</option>
                            </select>
                        </div>
                        <div class="flex-1 flex flex-col gap-1">
                            <label class="text-sm font-medium text-gray-700">City</label>
                            <select id="swalCity" class="w-full px-3 py-2 border rounded-md">
                                <option value="">-- Select City --</option>
                            </select>
                        </div>
                    </div>

                    <div class="flex flex-col md:flex-row md:space-x-4 space-y-2 md:space-y-0">
                        <div class="flex-1 flex flex-col gap-1">
                            <label class="text-sm font-medium text-gray-700">Barangay</label>
                            <input id="swalBarangay" class="w-full px-3 py-2 border rounded-md" placeholder="Barangay" value="${val(d.brgy)}">
                        </div>
                        <div class="flex-1 flex flex-col gap-1">
                            <label class="text-sm font-medium text-gray-700">Street & Unit</label>
                            <input id="swalStreet" class="w-full px-3 py-2 border rounded-md" placeholder="Street & Unit" value="${val(d.str)}">
                        </div>
                    </div>

                    <div class="flex flex-col md:flex-row md:space-x-4 space-y-2 md:space-y-0">
                        <div class="flex-1 flex flex-col gap-1">
                            <label class="text-sm font-medium text-gray-700">Landmark</label>
                            <input id="swalLandmark" class="w-full px-3 py-2 border rounded-md" placeholder="Landmark" value="${val(d.lmk)}">
                        </div>
                        <div class="flex-1 flex flex-col gap-1">
                            <label class="text-sm font-medium text-gray-700">Schedule</label>
                            <input type="datetime-local" id="swalSchedule" class="w-full px-3 py-2 border rounded-md" min="${minDateTime}">
                        </div>
                    </div>

                    <div class="w-full flex flex-col gap-1">
                        <label class="text-sm font-medium text-gray-700">Inspector</label>
                        <select id="swalInspector" class="w-full px-3 py-2 border rounded-md"></select>
                    </div>
                  <div id="inspectorScheduleContainer"
     class="hidden border border-gray-300 rounded-md max-h-[200px] md:max-h-[300px] overflow-y-auto p-2 bg-gray-50">
</div>

                    <div class="w-full flex flex-col gap-1">
                        <label class="text-sm font-medium text-gray-700">Remarks</label>
                        <textarea id="swalRemarks" class="w-full px-3 py-2 border rounded-md" placeholder="Remarks (optional)"></textarea>
                    </div>
                </div>
            `,
            didOpen: () => {
                const regionDropdown = document.getElementById('swalRegion');
                const cityDropdown = document.getElementById('swalCity');
                const inspectorDropdown = document.getElementById('swalInspector');
                const scheduleInput = document.getElementById('swalSchedule');
                const scheduleContainer = document.getElementById('inspectorScheduleContainer');

                // Populate visible dropdown with hidden ASP.NET dropdown values
                const source = document.getElementById('<%= ddlInspectorSource.ClientID %>');
                if (source && inspectorDropdown) {
                    inspectorDropdown.innerHTML = source.innerHTML;
                }

                // Load cities dynamically when region changes
                regionDropdown.addEventListener('change', function () {
                    loadCitiesForRegion(this.value);
                });

                // Fetch inspector schedule when inspector OR date changes
                const updateSchedule = () => {
                    const inspectorId = inspectorDropdown.value;
                    const selectedDateTime = scheduleInput.value;

                    if (inspectorId && selectedDateTime) {
                        const selectedDate = selectedDateTime.split('T')[0];
                        fetchInspectorSchedule(inspectorId, selectedDate, scheduleContainer);
                    } else {
                        scheduleContainer.innerHTML = '';
                        scheduleContainer.classList.add('hidden');
                    }
                };

                inspectorDropdown.addEventListener('change', updateSchedule);
                scheduleInput.addEventListener('change', updateSchedule);
            },
            showCancelButton: true,
            confirmButtonText: 'Assign',
            cancelButtonText: 'Cancel',
            confirmButtonColor: '#2563eb',
            cancelButtonColor: '#6b7280',
            preConfirm: () => {
                const firstName = document.getElementById('swalFirstName').value.trim();
                const middleName = document.getElementById('swalMiddleName').value.trim();
                const lastName = document.getElementById('swalLastName').value.trim();
                const street = document.getElementById('swalStreet').value.trim();
                const barangay = document.getElementById('swalBarangay').value.trim();
                const city = document.getElementById('swalCity').value;
                const region = document.getElementById('swalRegion').value;
                const country = document.getElementById('swalCountry').value.trim();
                const landmark = document.getElementById('swalLandmark').value.trim();
                const inspector = document.getElementById('swalInspector').value;
                const schedule = document.getElementById('swalSchedule').value;
                const remarks = document.getElementById('swalRemarks').value;

                // Validation
                if (!firstName || !lastName || !street || !barangay || !city || !region || !country || !inspector || !schedule) {
                    Swal.showValidationMessage("All fields are required except middle name, landmark, and remarks");
                    return false;
                }

                // Send data to hidden field
                document.getElementById('<%= hfAssignData.ClientID %>').value =
                    `${inspector}|${schedule}|${remarks}|${firstName}|${middleName}|${lastName}|${street}|${barangay}|${city}|${region}|${country}|${landmark}`;

                // Trigger ASP.NET postback
                __doPostBack('<%= btnAssignHidden.UniqueID %>', '');
                return true;
            }
        });
    }

    function confirmArchive(inquiryId) {
        Swal.fire({
            title: 'Archive this inquiry?',
            text: "You can restore this later from the Archived Inquiries page.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, archive it',
            cancelButtonText: 'Cancel',
            confirmButtonColor: '#f59e0b',
            cancelButtonColor: '#6c757d'
        }).then((result) => {
            if (result.isConfirmed) {
                document.getElementById('<%= hfDeleteInquiryID.ClientID %>').value = inquiryId;
                document.getElementById('<%= btnDeleteHidden.ClientID %>').click();
            }
        });
    }

    function showFullMessage(message) {
        Swal.fire({
            title: 'Full Message',
            text: message,
            icon: 'info',
            confirmButtonText: 'Close'
        });
        return false;
    }
    // New/Revised function to fetch schedule
    function fetchInspectorSchedule(inspectorId, selectedDate, container) {
        container.innerHTML = '<div class="text-center text-gray-500 py-4">Loading schedule...</div>';
        container.classList.remove('hidden');

        fetch(`AllInquiry.aspx?getInspectorSchedule=1&inspectorId=${inspectorId}&date=${selectedDate}`)
            .then(response => response.json())
            .then(data => {
                if (data.length === 0) {
                    container.innerHTML = '<div class="text-center text-gray-500 py-4">No scheduled inspections for this day.</div>';
                    return;
                }

                const html = data.map(item => {
                    // Correctly parse the ISO 8601 string.
                    // It's a good practice to explicitly check for a valid date.
                    const scheduledDate = new Date(item.ScheduledDate);
                    const formattedDate = scheduledDate.toString() === 'Invalid Date'
                        ? 'Invalid Date'
                        : scheduledDate.toLocaleString();

                    return `
    <div class="border p-3 rounded-md bg-gray-50 mb-3 text-left">
        <p><strong>Date & Time:</strong> ${formattedDate}</p>
        <p><strong>Status:</strong> ${item.InspectionStatus}</p>
        <hr class="my-2"/>
        <p><strong>Location:</strong> ${item.StreetEnc}, ${item.BarangayEnc}, ${item.CityEnc}</p>
        <p><em>Landmark: ${item.LandmarkEnc || 'N/A'}</em></p>
    </div>
`;
                }).join('');

                container.innerHTML = `<div class="p-2 space-y-2">${html}</div>`;
            })
            .catch(err => {
                container.innerHTML = '<div class="text-center text-red-500 py-4">Error loading schedule.</div>';
                console.error(err);
            });
    }

</script>
</asp:Content>