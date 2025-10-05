<%@ Page Title="System Settings" Language="C#" MasterPageFile="~/RootAdmin.Master"
    AutoEventWireup="true" CodeBehind="SystemSettings.aspx.cs" Inherits="RRCManagementSystem.SystemSettings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />

    <style>
        .container { max-width: 800px; margin: auto; padding-top: 50px; }
        .card { border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); padding: 20px; }

        /* Switch Styles */
        .switch-wrapper { position: relative; display: inline-block; width: 60px; height: 30px; cursor: pointer; }
        .switch { position: absolute; top: 0; left: 0; right: 0; bottom: 0;
                  background-color: #ccc; border-radius: 50px; transition: background-color 0.3s; }
        .switch:before { content: ""; position: absolute; height: 24px; width: 24px; left: 3px; top: 3px;
                         background-color: white; border-radius: 50%; transition: transform 0.3s; }
        .switch.on { background-color: #4CAF50; }
        .switch.on:before { transform: translateX(30px); }

        .fw-bold { font-weight: 700; }
        .text-danger { color: red; }
        .text-success { color: green; }
    </style>

    <div class="container">
        <h3 class="mb-4">System Settings</h3>

        <div class="card shadow">
            <h5 class="mb-3">System Maintenance Mode</h5>

            <!-- Toggle Switch (indicator only) -->
            <div id="maintenanceSwitch" class="switch-wrapper mb-3">
                <div id="switchToggle" class="switch"></div>
            </div>

            <!-- Messages -->
            <asp:Label ID="lblMessage" runat="server" CssClass="text-danger mt-2"></asp:Label>

            <!-- Hidden button for postback -->
            <asp:Button ID="btnToggle" runat="server" OnClick="btnToggle_Click" Style="display:none;" />
        </div>
    </div>

    <!-- Confirmation Modal -->
    <div class="modal fade" id="confirmModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Confirm Action</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <p>Type <strong>CONFIRM</strong> to apply the change.</p>
                    <input type="text" id="confirmInput" class="form-control" placeholder="Type CONFIRM here" />
                </div>
                <div class="modal-footer">
                    <button type="button" id="modalCancel" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="button" id="modalConfirm" class="btn btn-primary">Confirm</button>
                </div>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>

  <script>
      document.addEventListener('DOMContentLoaded', function () {
          var switchToggle = document.getElementById('switchToggle');
          var btn = document.getElementById('<%= btnToggle.ClientID %>');

        var modal = new bootstrap.Modal(document.getElementById('confirmModal'));
        var input = document.getElementById('confirmInput');
        var confirmBtn = document.getElementById('modalConfirm');

        // Set initial state from server (server renders class "on" if maintenance enabled)
        var isOn = switchToggle.classList.contains('on');

        switchToggle.addEventListener('click', function () {
            console.log("Switch clicked, opening modal...");
            modal.show();
            input.value = "";
            input.focus();
        });

        confirmBtn.addEventListener('click', function () {
            console.log("Confirm button clicked.");
            if (input.value.trim().toUpperCase() === "CONFIRM") {
                console.log("CONFIRM matched. Triggering postback...");

                // Toggle visual state
                isOn = !switchToggle.classList.contains('on');
                switchToggle.className = isOn ? "switch on" : "switch";

                // Trigger server-side postback
                modal.hide();
                btn.click();
            } else {
                alert('You must type CONFIRM to proceed.');
                input.focus();
            }
        });
    });
  </script>



</asp:Content>
