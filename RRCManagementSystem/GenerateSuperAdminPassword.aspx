<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GenerateSuperAdminPassword.aspx.cs" Inherits="RRCManagementSystem.GenerateSuperAdminPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SuperAdmin Password Generator</title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="margin: 50px; font-family: Arial;">
            <h2>Generate SuperAdmin Password Hash & Salt</h2>
            
            <asp:TextBox ID="txtPlainPassword" runat="server" Placeholder="Enter Password to Encrypt" Width="300px" /><br /><br />
            <asp:Button ID="btnGenerate" runat="server" Text="Generate Hash & Salt" OnClick="btnGenerate_Click" /><br /><br />
            
            <asp:Label ID="lblResult" runat="server" Text=""></asp:Label>
        </div>
    </form>
</body>
</html>