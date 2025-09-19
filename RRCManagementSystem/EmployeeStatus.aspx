<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="EmployeeStatus.aspx.cs" Inherits="RRCManagementSystem.EmployeeStatus" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    </asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto py-10 px-4 flex justify-center items-start">
        <div class="w-full max-w-5xl bg-white p-8 rounded-2xl shadow-xl border border-gray-200">
            <h2 class="text-center text-3xl font-extrabold text-blue-800 mb-6">Employee Status</h2>

            <div class="mb-6 max-w-xs mx-auto md:max-w-md">
                <label for="<%= ddlStatus.ClientID %>" class="block text-sm font-semibold text-gray-700 mb-2">Filter by Status:</label>
                <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true"
                    CssClass="block w-full px-4 py-2 bg-white text-gray-900 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
                    OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged" />
            </div>

            <div class="overflow-x-auto">
                <asp:GridView ID="gvEmployees" runat="server"
                    CssClass="w-full text-left border-collapse border-separate border-spacing-0.5"
                    GridLines="None"
                    AutoGenerateColumns="False" AllowPaging="True" PageSize="10"
                    OnPageIndexChanging="gvEmployees_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="EmployeeID" HeaderText="ID" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                        <asp:BoundField DataField="FullName" HeaderText="Full Name" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                        <asp:BoundField DataField="Email" HeaderText="Email" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                        <asp:BoundField DataField="Phone" HeaderText="Phone" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                        <asp:BoundField DataField="Position" HeaderText="Position" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                        <asp:BoundField DataField="Status" HeaderText="Status" HeaderStyle-CssClass="px-4 py-2 text-sm font-semibold text-gray-700 uppercase bg-gray-50 border border-gray-200" ItemStyle-CssClass="px-4 py-2 text-sm text-gray-900 border border-gray-200" />
                    </Columns>
                    <HeaderStyle CssClass="text-center" />
                </asp:GridView>
            </div>
            
            <asp:Label ID="lblNoData" runat="server" Text="No employees found for this status." Visible="false" CssClass="block text-center text-lg font-medium text-gray-500 mt-8 mb-4" />
        </div>
    </div>
</asp:Content>