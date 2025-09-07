<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Maintenance.aspx.cs" Inherits="RRCManagementSystem.Maintenance" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>System Under Maintenance</title>
    <meta http-equiv="refresh" content="30;url=Login.aspx" /> <!-- Auto redirect after 30 sec -->

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />

    <style>
        body {
            background-color: #f8f9fa;
            font-family: 'Segoe UI', sans-serif;
        }
        .maintenance-container {
            height: 100vh;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            text-align: center;
        }
        .maintenance-icon {
            font-size: 80px;
            color: #dc3545;
            margin-bottom: 20px;
        }
        .maintenance-text h1 {
            font-size: 36px;
            margin-bottom: 15px;
        }
        .maintenance-text p {
            font-size: 18px;
            color: #6c757d;
        }
        .btn-home {
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="maintenance-container">
            <i class="fas fa-tools maintenance-icon"></i>
            <div class="maintenance-text">
                <h1>System Under Maintenance</h1>
                <p>We are currently performing system upgrades. Please check back later.</p>
                <asp:Button ID="btnHome" runat="server" Text="Go to Login" CssClass="btn btn-primary btn-home" OnClick="btnHome_Click" />
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
