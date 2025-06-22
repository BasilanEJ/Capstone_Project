<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageClient.aspx.cs" Inherits="RRCManagementSystem.ManageClient" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- FontAwesome -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
    <!-- SweetAlert2 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <h2 class="text-center mb-4 fw-semibold">👤 Manage Clients</h2>

        <div class="row g-4 justify-content-center">

            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="ClientsProfiles.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-user fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">View Client Profiles</h5>
                        </div>
                    </div>
                </a>
            </div>

            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="ArchivedClients.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-archive fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">Archive Client Accounts</h5>
                        </div>
                    </div>
                </a>
            </div>

            <!-- Uncomment if you want to add these links in future
            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="ClientsHistory.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-history fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">Clients History / Transactions</h5>
                        </div>
                    </div>
                </a>
            </div>

            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="SendNotifications.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-bell fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">Send Notifications</h5>
                        </div>
                    </div>
                </a>
            </div>
            -->

            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="ManageContract.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-file-signature fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">Manage Contracts</h5>
                        </div>
                    </div>
                </a>
            </div>

        </div>
    </div>

    <!-- Bootstrap 5 JS Bundle (includes Popper) -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- SweetAlert2 JS -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js"></script>

    <script>
        // Example SweetAlert2 usage on page load (optional)
        /*
        document.addEventListener('DOMContentLoaded', function () {
            Swal.fire({
                icon: 'info',
                title: 'Welcome to Manage Clients',
                text: 'Use the cards below to navigate client management.',
                timer: 3000,
                showConfirmButton: false
            });
        });
        */
    </script>

    <style>
        /* Optional hover shadow for cards */
        .hover-shadow:hover {
            box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
            transform: translateY(-4px);
            transition: all 0.3s ease;
        }
    </style>
</asp:Content>
