<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DecryptPDF.aspx.cs" Inherits="RRCManagementSystem.DecryptPDF" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Decrypt and View Encrypted PDF</title>
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
        <div style="text-align:center; margin-top:50px;">
            <h2>🔓 Decrypt Encrypted PDF</h2>
            <asp:FileUpload ID="fileUpload" runat="server" />
            <br /><br />
            <asp:Button ID="btnDecrypt" runat="server" Text="Decrypt and View" OnClick="btnDecrypt_Click" />
        </div>
    </form>
</body>
</html>