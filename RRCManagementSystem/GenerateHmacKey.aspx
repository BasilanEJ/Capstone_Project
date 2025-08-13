<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GenerateHmacKey.aspx.cs" Inherits="RRCManagementSystem.GenerateHmacKey" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Generate Blockchain HMAC Key</title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="margin: 50px; font-family: Arial;">
            <h2>Generate Blockchain HMAC Key</h2>
            <asp:Button ID="btnGenerate" runat="server" Text="Generate Key" OnClick="btnGenerate_Click" /><br /><br />
            <asp:Label ID="lblResult" runat="server" Text=""></asp:Label>
        </div>
    </form>
</body>
</html>

