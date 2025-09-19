<%@ Page Title="View Quotations" Language="C#" MasterPageFile="~/Admin.Master"
    AutoEventWireup="true" CodeBehind="ViewQuotation.aspx.cs"
    Inherits="RRCManagementSystem.ViewQuotation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://cdn.tailwindcss.com"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

    <style>
        .table-grid {
            border-collapse: collapse;
        }

        .table-grid th, .table-grid td {
            border: 1px solid #e2e8f0; /* slato-200 */
        }
    </style>

    <div class="container mx-auto p-4 md:p-8">
        <h2 class="text-center text-3xl font-bold text-slate-900 mb-6">📑 View Quotations</h2>

        <!-- Filters Bar -->
        <div class="flex flex-col md:flex-row justify-center items-center gap-4 md:gap-6 mb-8 p-4 bg-gray-50 rounded-lg shadow-sm">
            <div class="flex flex-col flex-grow">
                <label for="<%= txtDateFrom.ClientID %>" class="font-semibold text-slate-700 mb-1 text-sm">Date From</label>
                <asp:TextBox ID="txtDateFrom" runat="server" TextMode="Date" CssClass="w-full px-4 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div class="flex flex-col flex-grow">
                <label for="<%= txtDateTo.ClientID %>" class="font-semibold text-slate-700 mb-1 text-sm">Date To</label>
                <asp:TextBox ID="txtDateTo" runat="server" TextMode="Date" CssClass="w-full px-4 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div class="flex flex-col flex-grow">
                <label for="<%= ddlInspector.ClientID %>" class="font-semibold text-slate-700 mb-1 text-sm">Inspector</label>
                <asp:DropDownList ID="ddlInspector" runat="server" CssClass="w-full px-4 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div class="flex flex-row gap-2 mt-auto">
                <asp:Button ID="btnSearch" runat="server" CssClass="bg-blue-600 text-white px-4 py-2 rounded-lg font-bold text-sm cursor-pointer hover:bg-blue-700 transition-colors" Text="Search" OnClick="btnSearch_Click" />
                <asp:Button ID="btnReset" runat="server" CssClass="bg-white text-blue-600 border border-blue-600 px-4 py-2 rounded-lg font-bold text-sm cursor-pointer hover:bg-gray-100 transition-colors" Text="Reset" OnClick="btnReset_Click" />
            </div>
            <asp:Label ID="lblMessage" runat="server" CssClass="text-slate-500 text-sm mt-2 md:mt-0" />
        </div>

        <!-- Quotation Data -->
        <div class="overflow-x-auto shadow-lg rounded-lg">
            <asp:GridView ID="gvQuotations" runat="server"
                AutoGenerateColumns="False"
                CssClass="min-w-full bg-white table-auto rounded-lg table-grid"
                DataKeyNames="PendingQuotationID"
                AllowPaging="true" PageSize="10"
                OnPageIndexChanging="gvQuotations_PageIndexChanging"
                HeaderStyle-CssClass="bg-blue-600 text-white uppercase text-xs leading-normal font-bold"
                RowStyle-CssClass="border-b border-gray-200 hover:bg-gray-100 transition-colors">
                <Columns>
                    <asp:BoundField DataField="PendingQuotationID" HeaderText="Quote #" Visible="False" />
                    <asp:BoundField DataField="QuotationCode" HeaderText="Quotation Code" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center" />
                    <asp:BoundField DataField="CreatedAt" HeaderText="Created" DataFormatString="{0:yyyy-MM-dd HH:mm}" HtmlEncode="false" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" HeaderStyle-CssClass="py-3 px-6 text-left" ItemStyle-CssClass="py-3 px-6 text-left" />
                    <asp:BoundField DataField="InspectorName" HeaderText="Inspector" HeaderStyle-CssClass="py-3 px-6 text-left" ItemStyle-CssClass="py-3 px-6 text-left" />
                    <asp:BoundField DataField="ServiceNames" HeaderText="Services" HeaderStyle-CssClass="py-3 px-6 text-left" ItemStyle-CssClass="py-3 px-6 text-left" />
                    <asp:BoundField DataField="SQM" HeaderText="SQM" HeaderStyle-CssClass="py-3 px-6 text-center" ItemStyle-CssClass="py-3 px-6 text-center" />
                    <asp:BoundField DataField="BasePrice" HeaderText="Base Price (₱)" DataFormatString="{0:N2}" HtmlEncode="false" HeaderStyle-CssClass="py-3 px-6 text-right" ItemStyle-CssClass="py-3 px-6 text-right" />
                    <asp:BoundField DataField="TravelExpense" HeaderText="Travel Expense (₱)" DataFormatString="{0:N2}" HtmlEncode="false" HeaderStyle-CssClass="py-3 px-6 text-right" ItemStyle-CssClass="py-3 px-6 text-right" />
                    <asp:BoundField DataField="Miscellaneous" HeaderText="Miscellaneous (₱)" DataFormatString="{0:N2}" HtmlEncode="false" HeaderStyle-CssClass="py-3 px-6 text-right" ItemStyle-CssClass="py-3 px-6 text-right" />
                    <asp:BoundField DataField="Price" HeaderText="Total Price (₱)" DataFormatString="{0:N2}" HtmlEncode="false" HeaderStyle-CssClass="py-3 px-6 text-right" ItemStyle-CssClass="py-3 px-6 text-right font-bold text-green-700" />

                    <asp:TemplateField HeaderText="Contract?">
                        <ItemTemplate>
                            <span class="inline-block px-2 py-1 text-xs font-semibold rounded-full border border-gray-300 text-slate-600">
                                <%# Convert.ToBoolean(Eval("IsContract")) ? "Yes" : "No" %>
                            </span>
                        </ItemTemplate>
                        <HeaderStyle CssClass="py-3 px-6 text-center" />
                        <ItemStyle CssClass="py-3 px-6 text-center" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class="inline-block px-2 py-1 text-xs font-semibold rounded-full border border-gray-300
                                <%# Eval("Status").ToString() == "Converted" ? "bg-green-100 text-green-700" : "bg-amber-100 text-amber-700" %>">
                                <%# Eval("Status") %>
                            </span>
                        </ItemTemplate>
                        <HeaderStyle CssClass="py-3 px-6 text-center" />
                        <ItemStyle CssClass="py-3 px-6 text-center" />
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center text-slate-500 py-6">No quotations found for the selected filters.</div>
                </EmptyDataTemplate>
                <PagerStyle CssClass="bg-gray-100 text-blue-600 font-bold text-lg p-2" />
            </asp:GridView>
        </div>
    </div>
</asp:Content>
