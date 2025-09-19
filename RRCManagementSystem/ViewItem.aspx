<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewItem.aspx.cs" Inherits="RRCManagementSystem.ViewItem" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    </asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container mx-auto py-10 px-4">
        <div class="bg-white p-8 rounded-2xl shadow-xl border border-gray-200">

            <div class="text-center mb-8">
                <h2 class="text-4xl font-extrabold text-blue-800">View Items</h2>
            </div>

            <div id="divRestockNotice" runat="server" class="mb-6">
                <asp:Label ID="lblRestockNotice" runat="server" 
                    CssClass="bg-yellow-50 text-yellow-700 font-medium px-6 py-4 rounded-lg border-l-4 border-yellow-400 flex items-center shadow-sm">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 mr-3 text-yellow-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                    </svg>
                    <span>
                        <strong class="font-bold">⚠️ Warning:</strong> There are items with quantities below the restock threshold.
                    </span>
                </asp:Label>
            </div>

            <div class="mb-6 max-w-sm mx-auto">
                <label for="ddlType" class="block text-sm font-semibold text-gray-700 mb-2">Filter by Item Type:</label>
                <asp:DropDownList ID="ddlType" runat="server" CssClass="block w-full px-4 py-2 text-gray-700 bg-gray-50 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlType_SelectedIndexChanged">
                    <asp:ListItem Text="All" Value="All" Selected="True" />
                    <asp:ListItem Text="Bottled Chemical" Value="Bottled Chemical" />
                    <asp:ListItem Text="Sachet Pack Chemical" Value="Sachet Pack Chemical" />
                    <asp:ListItem Text="Safety Gear" Value="Safety Gear" />
                </asp:DropDownList>
            </div>

            <div class="overflow-x-auto border border-gray-200 rounded-lg shadow-sm">
                <asp:GridView ID="gvItems" runat="server"
                    CssClass="w-full text-left border-collapse"
                    GridLines="None"
                    AutoGenerateColumns="False" AllowPaging="True" PageSize="10"
                    OnPageIndexChanging="gvItems_PageIndexChanging" OnRowCommand="gvItems_RowCommand" OnRowDataBound="gvItems_RowDataBound">
                    
                    <HeaderStyle CssClass="bg-blue-600 text-white text-sm font-semibold uppercase tracking-wider border-r border-gray-300" />
                    <RowStyle CssClass="bg-white hover:bg-gray-50 border-b border-gray-300" />
                    <AlternatingRowStyle CssClass="bg-gray-50 hover:bg-gray-100 border-b border-gray-300" />
                    
                    <Columns>
                        
                        <asp:TemplateField HeaderText="Item ID">
                            <HeaderStyle CssClass="px-4 py-3 border-r border-gray-300" />
                            <ItemTemplate>
                                <div class="px-4 py-3 border-r border-gray-200">
                                    <span class="font-mono text-gray-700"><%# "Item" + Convert.ToInt32(Eval("ItemID")).ToString("D3") %></span>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="Name" HeaderText="Item Name" 
                            HeaderStyle-CssClass="px-4 py-3 border-r border-gray-300"
                            ItemStyle-CssClass="px-4 py-3 border-r border-gray-200 text-gray-900" />
                        
                        <asp:BoundField DataField="Type" HeaderText="Type" 
                            HeaderStyle-CssClass="px-4 py-3 border-r border-gray-300"
                            ItemStyle-CssClass="px-4 py-3 border-r border-gray-200 text-gray-700" />

                        <asp:TemplateField HeaderText="Quantity">
                            <HeaderStyle CssClass="px-4 py-3 border-r border-gray-300" />
                            <ItemTemplate>
                                <div class="px-4 py-3 border-r border-gray-200 text-gray-700">
                                    <asp:Label ID="lblQuantity" runat="server"
                                        Text='<%# Eval("Type").ToString() == "Bottled Chemical" ? Eval("Quantity") + " bottles" :
                                                Eval("Type").ToString() == "Sachet Pack Chemical" ? Eval("Quantity") + " packs" :
                                                Eval("Type").ToString() == "Safety Gear" ? Eval("Quantity") + " pcs" :
                                                Eval("Quantity") + " unit(s)" %>'></asp:Label>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Excess (mL)">
                            <HeaderStyle CssClass="px-4 py-3 border-r border-gray-300" />
                            <ItemTemplate>
                                <div class="px-4 py-3 border-r border-gray-200 text-gray-700">
                                    <%# string.IsNullOrEmpty(Eval("ExcessML").ToString()) ? "-" : Eval("ExcessML") + " mL" %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="ExpirationDate" HeaderText="Expiration Date" DataFormatString="{0:yyyy-MM-dd}" 
                            HeaderStyle-CssClass="px-4 py-3 border-r border-gray-300"
                            ItemStyle-CssClass="px-4 py-3 border-r border-gray-200 text-gray-700" />
                        
                        <asp:BoundField DataField="CreatedAt" HeaderText="Created At" DataFormatString="{0:yyyy-MM-dd HH:mm}" 
                            HeaderStyle-CssClass="px-4 py-3 border-r border-gray-300"
                            ItemStyle-CssClass="px-4 py-3 border-r border-gray-200 text-gray-700" />

                        <asp:TemplateField HeaderText="Image">
                            <HeaderStyle CssClass="px-4 py-3 border-r border-gray-300" />
                            <ItemTemplate>
                                <div class="px-4 py-3 border-r border-gray-200 flex justify-center">
                                    <asp:Image ID="imgItem" runat="server" 
                                        ImageUrl='<%# Eval("ImagePath") %>' 
                                        Width="70px" 
                                        Height="70px" 
                                        CssClass="rounded-lg shadow-md cursor-pointer transition-transform duration-200 hover:scale-110" 
                                        onclick="showImageModal(this.src)" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions">
                            <HeaderStyle CssClass="px-4 py-3" />
                            <ItemTemplate>
                                <div id="ActionsDiv" runat="server" class="px-4 py-3 flex flex-col items-center justify-center space-y-2">
                                    <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditItem" CommandArgument='<%# Eval("ItemID") %>'
                                        CssClass="w-full px-4 py-2 text-sm font-semibold rounded-lg text-white bg-green-500 hover:bg-green-600 focus:outline-none focus:ring-2 focus:ring-green-400 transition-colors">
                                        EDIT
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnAddStocks" runat="server" CommandName="AddStocks" CommandArgument='<%# Eval("ItemID") %>'
                                        CssClass="w-full px-4 py-2 text-sm font-semibold rounded-lg text-white bg-cyan-600 hover:bg-cyan-700 focus:outline-none focus:ring-2 focus:ring-cyan-400 transition-colors">
                                        ADD STOCKS
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteItem" CommandArgument='<%# Eval("ItemID") %>'
                                        OnClientClick='return confirm("Are you sure you want to delete this item?");'
                                        CssClass="w-full px-4 py-2 text-sm font-semibold rounded-lg text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-400 transition-colors">
                                        DELETE
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>
                    <PagerStyle CssClass="bg-gray-100 text-center px-4 py-3" />
                </asp:GridView>
                <div class="py-4 text-center">
                    <asp:Label ID="lblMessage" runat="server" CssClass="text-lg font-medium text-gray-500" Visible="false" />
                </div>
            </div>
        </div>
    </div>

    <div id="imageModal" class="fixed inset-0 z-50 hidden overflow-y-auto">
        <div class="flex items-center justify-center min-h-screen px-4 text-center">
            <div class="fixed inset-0 transition-opacity" aria-hidden="true">
                <div class="absolute inset-0 bg-gray-900 opacity-75"></div>
            </div>
            
            <div class="bg-white rounded-lg overflow-hidden shadow-xl transform transition-all max-w-lg w-full">
                <div class="p-4">
                    <img id="modalImage" src="" alt="Full-sized item image" class="max-h-[80vh] w-full object-contain mx-auto" />
                </div>
                <div class="px-4 py-3 bg-gray-50 text-center">
                    <button type="button" onclick="hideImageModal()" 
                        class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:w-auto sm:text-sm">
                        Close
                    </button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function showImageModal(imageUrl) {
            const modal = document.getElementById('imageModal');
            const modalImage = document.getElementById('modalImage');
            modalImage.src = imageUrl;
            modal.classList.remove('hidden');
        }

        function hideImageModal() {
            const modal = document.getElementById('imageModal');
            modal.classList.add('hidden');
        }
    </script>
</asp:Content>