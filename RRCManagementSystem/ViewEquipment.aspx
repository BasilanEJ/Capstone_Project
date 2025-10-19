<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewEquipment.aspx.cs" Inherits="RRCManagementSystem.ViewEquipment" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
  <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css" rel="stylesheet" />
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .container {
            margin-top: 40px;
            padding: 20px;
            background-color: #f9f9f9;
        }

        .card {
            border-radius: 8px;
            border: 1px solid #ddd;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
            background-color: #ffffff;
        }

        .card-header {
            background-color: #007bff;
            color: #ffffff;
            font-size: 20px;
            font-weight: bold;
            padding: 15px 20px;
            border-top-left-radius: 8px;
            border-top-right-radius: 8px;
            border-bottom: 1px solid #ccc;
        }

        .card-body {
            padding: 20px;
        }

        .form-group label {
            font-weight: bold;
            font-size: 16px;
        }

        .form-control {
            width: 300px;
            padding: 8px 12px;
            font-size: 14px;
            border-radius: 4px;
            border: 1px solid #ccc;
            margin-bottom: 20px;
        }

        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

        .table th, .table td {
            padding: 12px 15px;
            text-align: center;
            border: 1px solid #dee2e6;
            font-size: 14px;
            vertical-align: middle;
        }

        .table th {
            background-color: #007bff;
            color: #ffffff;
            font-weight: 600;
        }

        .table-striped tbody tr:nth-child(odd) {
            background-color: #f9f9f9;
        }

        .table-striped tbody tr:hover {
            background-color: #e9ecef;
        }

        .equipment-image {
            width: 70px;
            height: 70px;
            object-fit: cover;
            border-radius: 5px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            cursor: pointer;
            transition: transform 0.2s;
        }

        .equipment-image:hover {
            transform: scale(1.1);
        }

        .filter-btn {
            padding: 8px 16px;
            font-size: 14px;
            margin-right: 10px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-weight: 600;
            transition: background-color 0.3s ease;
        }

        .available {
            background-color: #28a745;
            color: white;
        }

        .available:hover {
            background-color: #218838;
        }

        .gridview-pager {
            margin-top: 10px;
            text-align: center;
        }

        .gridview-pager a, .gridview-pager span {
            display: inline-block;
            margin: 0 5px;
            padding: 5px 10px;
            color: #007bff;
            text-decoration: none;
            border: 1px solid #007bff;
            border-radius: 4px;
        }

        .gridview-pager a:hover {
            background-color: #007bff;
            color: #fff;
        }

        .gridview-pager span {
            background-color: #007bff;
            color: #fff;
        }

        .message-label {
            margin-top: 10px;
            display: block;
            font-weight: 600;
            color: #dc3545;
        }

        /* Image Modal Styles */
        .image-modal-overlay {
            background-color: rgba(0, 0, 0, 0.85);
            transition: opacity 0.3s ease-in-out;
        }

        .image-modal-content {
            position: relative;
            max-width: 90vw;
            max-height: 90vh;
        }

        .image-modal-close-btn {
            position: absolute;
            top: 15px;
            right: 15px;
            background-color: rgba(255, 255, 255, 0.9);
            border: none;
            cursor: pointer;
            color: #1f2937;
            font-size: 1.5rem;
            line-height: 1;
            padding: 0.5rem;
            border-radius: 50%;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1);
            transition: background-color 0.2s, transform 0.2s;
            z-index: 60;
        }

        .image-modal-close-btn:hover {
            background-color: #f87171;
            color: white;
            transform: scale(1.05);
        }
    </style>

    <div class="container mt-4">
        <div class="card">
            <div class="card-header">
                <strong>View Equipment</strong>
            </div>
            <div class="card-body">

                <div class="form-group mb-3">
                    <label for="ddlStatus"><strong>Filter by Status:</strong></label>
                    <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" CssClass="form-control" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                        <asp:ListItem Text="All" Value="" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="Available" Value="Available"></asp:ListItem>
                        <asp:ListItem Text="Unavailable" Value="Unavailable"></asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div class="form-group mb-3">
                    <label for="txtFilterDate"><strong>Filter by Scheduled Date:</strong></label>
                    <asp:TextBox ID="txtFilterDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>

                <div class="form-group mb-3">
                    <asp:Button ID="btnFilter" runat="server" Text="Filter Results" CssClass="filter-btn available" OnClick="btnFilter_Click" />
                </div>

                <asp:GridView ID="gvEquipment" runat="server" AutoGenerateColumns="False"
                    CssClass="table table-bordered table-striped mt-3"
                    AllowPaging="true" PageSize="10"
                    OnPageIndexChanging="gvEquipment_PageIndexChanging"
                    OnRowCommand="gvEquipment_RowCommand">

                    <Columns>
                        <asp:BoundField DataField="FormattedID" HeaderText="Equipment ID" />
                        <asp:BoundField DataField="Name" HeaderText="Equipment Name" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                        
                        <asp:TemplateField HeaderText="Image">
                            <ItemTemplate>
                                <%# !string.IsNullOrEmpty(Eval("ImagePath")?.ToString()) 
                                    ? $"<img src='{ResolveUrl(Eval("ImagePath").ToString())}' alt='Equipment Image' class='equipment-image' onclick='showImageModal(\"{ResolveUrl(Eval("ImagePath").ToString())}\"); return false;' />"
                                    : "<span style='color: #999; font-style: italic;'>No Image</span>" %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="CreatedAt" HeaderText="Date Added" DataFormatString="{0:yyyy-MM-dd HH:mm}" />

                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="d-flex justify-content-center gap-2">
                                    <a class="btn btn-success btn-sm"
                                       href='<%# "EditEquipment.aspx?id=" + EncodeID(Eval("EquipmentID").ToString()) %>'>
                                        <i class="bi bi-pencil-square me-1"></i> Edit
                                    </a>
                                    
                                    <asp:Button ID="btnDelete" runat="server"
                                        Text="Delete"
                                        CssClass="btn btn-danger btn-sm"
                                        CommandName="DeleteEquipment"
                                        CommandArgument='<%# Eval("EquipmentID") %>'
                                        UseSubmitBehavior="false"
                                        OnClientClick="return confirmDelete(this);" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <asp:Label ID="lblMessage" runat="server" CssClass="message-label" />
            </div>
        </div>
    </div>

    <!-- Image Modal -->
    <div id="imageModal" class="fixed-top d-flex align-items-center justify-content-center image-modal-overlay opacity-0 pe-none" 
         style="z-index: 9999; position: fixed; inset: 0;"
         onclick="if (event.target.id === 'imageModal') hideImageModal()">
        
        <div class="image-modal-content" onclick="event.stopPropagation()">
            <button type="button" onclick="hideImageModal()" class="image-modal-close-btn">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
            </button>
            
            <img id="modalImage" class="rounded" style="max-width: 90vw; max-height: 90vh;" alt="Enlarged Equipment Image" />
        </div>
    </div>

    <script>
        // Image Modal Functions
        function showImageModal(src) {
            document.getElementById('modalImage').src = src;
            const modal = document.getElementById('imageModal');
            modal.classList.remove('opacity-0', 'pe-none');
            document.body.style.overflow = 'hidden';
        }

        function hideImageModal() {
            const modal = document.getElementById('imageModal');
            modal.classList.add('opacity-0', 'pe-none');
            document.body.style.overflow = '';
        }

        // Close modal on Escape key
        document.addEventListener('keydown', function (event) {
            const imageModal = document.getElementById('imageModal');
            if (event.key === 'Escape' && !imageModal.classList.contains('opacity-0')) {
                hideImageModal();
            }
        });

        // Delete Confirmation
        function confirmDelete(btn) {
            if (window.event) window.event.preventDefault();
            Swal.fire({
                title: 'Delete this equipment?',
                text: 'This action cannot be undone.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#dc3545'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn.name, '');
                }
            });
            return false;
        }
    </script>
</asp:Content>