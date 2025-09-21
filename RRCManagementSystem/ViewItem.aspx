<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewItem.aspx.cs" Inherits="RRCManagementSystem.ViewItem" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

        body {
            font-family: 'Inter', sans-serif;
            background-color: #f3f4f6;
        }

        .hidden-btn {
            visibility: hidden;
            position: absolute;
            top: -9999px;
        }

        .link-button-disabled {
            pointer-events: none;
            cursor: not-allowed;
            opacity: 0.5;
        }

        /* Modal */
        .modal-hidden {
            opacity: 0;
            pointer-events: none;
        }

        .modal-overlay {
            background-color: rgba(0, 0, 0, 0.75);
            transition: opacity 0.3s ease-in-out;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container mx-auto py-10 px-4">
        <div class="bg-white p-8 rounded-2xl shadow-xl border border-gray-200">

            <div class="text-center mb-8">
                <h2 class="text-4xl font-extrabold text-blue-800">View Items</h2>
            </div>

            <!-- Restock Notice -->
           <div id="divRestockNotice" runat="server" class="mb-6">
    <asp:Label ID="lblRestockNotice" runat="server"
        CssClass="bg-yellow-50 text-yellow-700 font-medium px-6 py-4 rounded-lg border-l-4 border-yellow-400 flex items-center shadow-sm">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 mr-3 text-yellow-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
        </svg>
        <span>
            <strong class="font-bold">⚠️ Warning:</strong> Some items are below restock threshold.
        </span>
    </asp:Label>
</div>

            <!-- Filter -->
            <div class="mb-6 max-w-sm mx-auto">
                <label for="ddlType" class="block text-sm font-semibold text-gray-700 mb-2">Filter by Item Type:</label>
                <asp:DropDownList ID="ddlType" runat="server"
                    CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:ring-1 focus:ring-blue-500"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlType_SelectedIndexChanged">
                    <asp:ListItem Text="All" Value="All" Selected="True" />
                    <asp:ListItem Text="Bottled Chemical" Value="Bottled Chemical" />
                    <asp:ListItem Text="Sachet Pack Chemical" Value="Sachet Pack Chemical" />
                    <asp:ListItem Text="Safety Gear" Value="Safety Gear" />
                </asp:DropDownList>
            </div>

            <!-- GridView -->
            <div class="overflow-x-auto border border-gray-200 rounded-lg shadow-sm">
                <asp:GridView ID="gvItems" runat="server"
                    CssClass="w-full text-left border-collapse"
                    GridLines="None"
                    AutoGenerateColumns="False"
                    AllowPaging="True"
                    PageSize="10"
                    DataKeyNames="ItemID"
                    OnPageIndexChanging="gvItems_PageIndexChanging"
                    OnRowCommand="gvItems_RowCommand"
                    OnRowDataBound="gvItems_RowDataBound">

                    <HeaderStyle CssClass="bg-blue-600 text-white text-sm font-semibold uppercase tracking-wider" />
                    <RowStyle CssClass="bg-white hover:bg-gray-50 border-b border-gray-300" />
                    <AlternatingRowStyle CssClass="bg-gray-50 hover:bg-gray-100 border-b border-gray-300" />

                    <Columns>
                        <asp:TemplateField HeaderText="Item ID">
                            <ItemTemplate>
                                <div class="px-4 py-3">
                                    <%# "Item" + Convert.ToInt32(Eval("ItemID")).ToString("D3") %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="Name" HeaderText="Item Name"
                            HeaderStyle-CssClass="px-4 py-3"
                            ItemStyle-CssClass="px-4 py-3 text-gray-900" />

                        <asp:BoundField DataField="Type" HeaderText="Type"
                            HeaderStyle-CssClass="px-4 py-3"
                            ItemStyle-CssClass="px-4 py-3 text-gray-700" />

                        <asp:TemplateField HeaderText="Quantity">
                            <ItemTemplate>
                                <asp:Label ID="lblQuantity" runat="server"
                                    Text='<%# Eval("Type").ToString() == "Bottled Chemical" ? Eval("Quantity") + " bottles" :
                                           Eval("Type").ToString() == "Sachet Pack Chemical" ? Eval("Quantity") + " packs" :
                                           Eval("Type").ToString() == "Safety Gear" ? Eval("Quantity") + " pcs" :
                                           Eval("Quantity") + " unit(s)" %>' />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Excess (mL)">
                            <ItemTemplate>
                                <%# string.IsNullOrEmpty(Eval("ExcessML").ToString()) ? "-" : Eval("ExcessML") + " mL" %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="ExpirationDate" HeaderText="Expiration Date" DataFormatString="{0:yyyy-MM-dd}" />
                        <asp:BoundField DataField="CreatedAt" HeaderText="Created At" DataFormatString="{0:yyyy-MM-dd HH:mm}" />

                        <asp:TemplateField HeaderText="Image">
                            <ItemTemplate>
                                <asp:Image ID="imgItem" runat="server"
                                    ImageUrl='<%# Eval("ImagePath") %>'
                                    Width="70px" Height="70px"
                                    CssClass="rounded shadow cursor-pointer hover:scale-110 transition"
                                    onclick="showImageModal(this.src)" />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="flex flex-col items-center gap-2">
                                    <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditItem" CommandArgument='<%# Eval("ItemID") %>'
                                        CssClass="w-full px-4 py-2 text-sm font-semibold rounded-lg text-white bg-green-500 hover:bg-green-600">
                                        EDIT
                                    </asp:LinkButton>

                                    <asp:LinkButton ID="btnAddStocks" runat="server" CommandName="AddStocks" CommandArgument='<%# Eval("ItemID") %>'
                                        CssClass="w-full px-4 py-2 text-sm font-semibold rounded-lg text-white bg-cyan-600 hover:bg-cyan-700">
                                        ADD STOCKS
                                    </asp:LinkButton>

                                    <!-- Delete triggers SweetAlert -->
                                    <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteItem" CommandArgument='<%# Eval("ItemID") %>'
                                        CssClass="w-full px-4 py-2 text-sm font-semibold rounded-lg text-white bg-red-600 hover:bg-red-700"
                                        OnClientClick='<%# "confirmDelete(\"" + hiddenItemId.ClientID + "\", \"" + Eval("ItemID") + "\"); return false;" %>'>
                                        DELETE
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <div class="py-4 text-center">
                    <asp:Label ID="lblMessage" runat="server" CssClass="text-lg font-medium text-gray-500" Visible="false" />
                </div>
            </div>
        </div>
    </div>

    <!-- Hidden Fields -->
    <asp:HiddenField ID="hiddenItemId" runat="server" />
    <asp:Button ID="btnConfirmDelete" runat="server" CssClass="hidden-btn" OnClick="btnConfirmDelete_Click" />

    <!-- Add Stocks Modal -->
    <div id="addStockModal" class="fixed inset-0 z-50 flex items-center justify-center modal-hidden modal-overlay">
        <div class="bg-white p-8 rounded-xl shadow-2xl modal-content transform transition-all max-w-sm w-full">
            <h3 class="text-xl font-bold text-gray-900 mb-4">Add Stocks</h3>
            <p class="text-gray-700 mb-4">Please enter the quantity of stocks to add.</p>

            <!-- Quantity TextBox -->
            <div class="mb-4">
                <label for="txtAddQuantity" class="block text-sm font-semibold text-gray-700 mb-2">Quantity:</label>
                <asp:TextBox ID="txtAddQuantity" runat="server" TextMode="Number"
                    CssClass="block w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors" />
            </div>

            <div class="flex justify-end space-x-4">
                <button type="button" onclick="hideAddStockModal()" 
                    class="px-4 py-2 text-sm font-semibold rounded-lg bg-gray-200 text-gray-800 hover:bg-gray-300">
                    Cancel
                </button>
                <asp:Button ID="btnConfirmAddStock" runat="server" Text="Add Stocks" 
                    OnClick="btnConfirmAddStock_Click"
                    CssClass="px-4 py-2 text-sm font-semibold rounded-lg text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
        </div>
    </div>

    <!-- Image Modal -->
    <div id="imageModal" class="fixed inset-0 z-50 hidden">
        <div class="flex items-center justify-center min-h-screen">
            <div class="bg-white rounded-lg shadow-lg max-w-lg w-full">
                <div class="p-4">
                    <img id="modalImage" class="max-h-[80vh] mx-auto" />
                </div>
                <div class="p-4 text-center">
                    <button type="button" onclick="hideImageModal()" class="bg-blue-600 text-white px-4 py-2 rounded">
                        Close
                    </button>
                </div>
            </div>
        </div>
    </div>

    <script>
        // Image Modal
        function showImageModal(src) {
            document.getElementById('modalImage').src = src;
            document.getElementById('imageModal').classList.remove('hidden');
        }
        function hideImageModal() {
            document.getElementById('imageModal').classList.add('hidden');
        }

        // SweetAlert Delete
        function confirmDelete(hiddenFieldId, itemId) {
            Swal.fire({
                title: 'Are you sure?',
                text: "This action cannot be undone!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    console.log("Deleting Item ID: " + itemId); // Debug
                    document.getElementById(hiddenFieldId).value = itemId;

                    var btn = document.getElementById('<%= btnConfirmDelete.ClientID %>');
                    if (btn) {
                        console.log("Triggering hidden button click."); // Debug
                        btn.click();
                    } else {
                        console.error("Hidden button not found!");
                    }
                }
            });
        }

        // Show Add Stock Modal
        function showAddStockModal(itemId) {
            document.getElementById('<%= hiddenItemId.ClientID %>').value = itemId;
            document.getElementById('addStockModal').classList.remove('modal-hidden');
        }

        // Hide Add Stock Modal
        function hideAddStockModal() {
            document.getElementById('addStockModal').classList.add('modal-hidden');
        }
    </script>
</asp:Content>
