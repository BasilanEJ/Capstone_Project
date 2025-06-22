<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ManageEquipment.aspx.cs" Inherits="RRCManagementSystem.ManageEquipment" %>

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
        <h2 class="text-center mb-4 fw-semibold">🔧 Manage Equipment</h2>

        <div class="row g-4 justify-content-center">

            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
    <a href="AddEquipment.aspx" class="text-decoration-none">
        <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
            <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                <i class="fas fa-plus fa-2x text-primary mb-3"></i>
                <h5 class="card-title fw-semibold text-dark">Add Equipment</h5>
            </div>
        </div>
    </a>
</div>
            <div class="col-12 col-sm-6 col-md-4 col-lg-3">
                <a href="ViewEquipment.aspx" class="text-decoration-none">
                    <div class="card shadow-sm h-100 text-center border-0 rounded-3 hover-shadow">
                        <div class="card-body d-flex flex-column justify-content-center align-items-center p-4">
                            <i class="fas fa-list fa-2x text-primary mb-3"></i>
                            <h5 class="card-title fw-semibold text-dark">View Equipment</h5>
                        </div>
                    </div>
                </a>
            </div>

            

        </div>
    </div>

    <!-- Bootstrap 5 JS Bundle -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- SweetAlert2 JS -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js"></script>

    <script>
        // Example SweetAlert2 alert on page load (optional)
        /*
        document.addEventListener('DOMContentLoaded', function () {
            Swal.fire({
                icon: 'info',
                title: 'Manage Equipment',
                text: 'Use the cards to view or add equipment.',
                timer: 2500,
                showConfirmButton: false
            });
        });
        */
    </script>

    <style>
        /* Optional hover effect for cards */
        .hover-shadow:hover {
            box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
            transform: translateY(-4px);
            transition: all 0.3s ease;
        }
    </style>
</asp:Content>
