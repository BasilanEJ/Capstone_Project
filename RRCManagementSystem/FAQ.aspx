<%@ Page Title="FAQ Management" Language="C#" MasterPageFile="~/SuperAdmin.Master" AutoEventWireup="true" CodeBehind="FAQ.aspx.cs" Inherits="RRCManagementSystem.FAQ" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* ============ FAQ Management Styles ============ */
        .faq-container {
            padding: 30px;
            max-width: 1400px;
            margin: 0 auto;
        }

        .faq-header {
            background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
            color: white;
            padding: 25px;
            border-radius: 12px;
            margin-bottom: 30px;
            box-shadow: 0 4px 12px rgba(37, 99, 235, 0.2);
        }

        .faq-header h1 {
            margin: 0;
            font-size: 28px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .faq-header p {
            margin: 8px 0 0;
            opacity: 0.9;
            font-size: 14px;
        }

        /* ============ Alert Info ============ */
        .alert-info {
            background: #dbeafe;
            border-left: 4px solid #2563eb;
            padding: 15px 20px;
            border-radius: 8px;
            margin-bottom: 25px;
            display: flex;
            align-items: start;
            gap: 12px;
        }

        .alert-info i {
            color: #2563eb;
            font-size: 18px;
            margin-top: 2px;
        }

        .alert-info strong {
            color: #1e40af;
        }

        /* ============ Section Card ============ */
        .section-card {
            background: white;
            border-radius: 12px;
            padding: 30px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            border: 1px solid #e2e8f0;
            margin-bottom: 30px;
        }

        .section-title {
            font-size: 20px;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        /* ============ Add FAQ Section ============ */
        .add-faq-section {
            background: linear-gradient(to bottom, #f8fafc 0%, #ffffff 100%);
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 25px;
            border-left: 4px solid #2563eb;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        }

        .add-faq-section h5 {
            font-size: 16px;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 2px solid #e2e8f0;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        /* ============ Form Styles ============ */
        .form-group {
            margin-bottom: 20px;
        }

        .form-label {
            display: block;
            font-weight: 600;
            color: #334155;
            margin-bottom: 8px;
            font-size: 14px;
        }

        .form-input {
            width: 100%;
            padding: 10px 14px;
            border: 1px solid #cbd5e1;
            border-radius: 8px;
            font-size: 14px;
            transition: all 0.3s ease;
            box-sizing: border-box;
        }

        .form-input:focus {
            outline: none;
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
        }

        textarea.form-input {
            resize: vertical;
            min-height: 100px;
            font-family: inherit;
        }

        .help-text {
            font-size: 12px;
            color: #64748b;
            margin-top: 6px;
        }

        /* ============ Button Styles ============ */
        .btn-update {
            background: linear-gradient(135deg, #10b981, #059669);
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 8px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .btn-update:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
        }

        /* ============ FAQ List Section ============ */
        .faq-list-section {
            margin-top: 30px;
        }

        .faq-list-section h5 {
            font-size: 16px;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        /* ============ GridView Styles ============ */
        .faq-grid {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

        .faq-grid thead tr {
            background: #2563eb !important;
            color: white !important;
        }

        .faq-grid th {
            padding: 14px;
            text-align: left;
            font-weight: 600;
            color: white !important;
            border-bottom: 2px solid #1e40af;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .faq-grid td {
            padding: 16px 14px;
            border-bottom: 1px solid #f1f5f9;
            vertical-align: middle;
        }

        .faq-row {
            background: white;
            transition: all 0.3s ease;
        }

        .faq-row-alt {
            background: #f8fafc;
        }

        .faq-grid tr:hover {
            background: #eff6ff !important;
        }

        /* ============ FAQ Content Styles ============ */
        .faq-question {
            color: #1e293b;
            font-weight: 600;
            font-size: 14px;
            display: block;
            margin-bottom: 5px;
        }

        .faq-answer {
            color: #475569;
            font-size: 13px;
            line-height: 1.6;
        }

        /* ============ Action Buttons ============ */
        .faq-actions {
            display: flex;
            gap: 8px;
            flex-wrap: wrap;
            justify-content: center;
        }

        .faq-action-btn {
            padding: 6px 12px;
            border: none;
            border-radius: 6px;
            font-size: 12px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 6px;
            text-decoration: none;
            color: white;
        }

        .btn-primary.faq-action-btn {
            background: #2563eb;
        }

        .btn-primary.faq-action-btn:hover {
            background: #1e40af;
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(37, 99, 235, 0.3);
        }

        .btn-success.faq-action-btn {
            background: #10b981;
        }

        .btn-success.faq-action-btn:hover {
            background: #059669;
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(16, 185, 129, 0.3);
        }

        .btn-danger.faq-action-btn {
            background: #ef4444;
        }

        .btn-danger.faq-action-btn:hover {
            background: #dc2626;
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(239, 68, 68, 0.3);
        }

        .btn-secondary.faq-action-btn {
            background: #64748b;
        }

        .btn-secondary.faq-action-btn:hover {
            background: #475569;
        }

        /* ============ Move Buttons ============ */
        .faq-move-btn {
            padding: 6px 10px;
            margin: 2px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            background: white;
            color: #2563eb;
            font-size: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            text-decoration: none;
        }

        .faq-move-btn:hover {
            background: #2563eb;
            color: white;
            border-color: #2563eb;
            transform: translateY(-2px);
            box-shadow: 0 2px 6px rgba(37, 99, 235, 0.3);
        }

        /* ============ Status Button ============ */
        .faq-status-btn {
            padding: 6px 16px;
            border: none;
            border-radius: 6px;
            font-size: 12px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            text-decoration: none;
            color: white;
        }

        .btn-success.faq-status-btn {
            background: #10b981;
        }

        .btn-success.faq-status-btn:hover {
            background: #059669;
            transform: translateY(-2px);
        }

        .btn-secondary.faq-status-btn {
            background: #94a3b8;
        }

        .btn-secondary.faq-status-btn:hover {
            background: #64748b;
            transform: translateY(-2px);
        }

        /* ============ Empty State ============ */
        .faq-empty {
            text-align: center;
            padding: 60px 20px;
            color: #64748b;
        }

        .faq-empty i {
            font-size: 48px;
            color: #cbd5e1;
            margin-bottom: 15px;
        }

        .faq-empty p {
            margin: 10px 0 0;
            font-size: 14px;
        }

        /* ============ Form Control Override ============ */
        .form-control {
            width: 100%;
            padding: 8px 12px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            font-size: 13px;
            box-sizing: border-box;
        }

        .form-control:focus {
            outline: none;
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
        }

        /* ============ Responsive Design ============ */
        @media screen and (max-width: 1024px) {
            .faq-grid thead {
                display: none;
            }

            .faq-grid tr {
                display: block;
                margin-bottom: 20px;
                border: 1px solid #e2e8f0;
                border-radius: 8px;
                overflow: hidden;
            }

            .faq-grid td {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 12px 16px;
                border-bottom: 1px solid #f1f5f9;
            }

            .faq-grid td:last-child {
                border-bottom: none;
            }

            .faq-grid td::before {
                content: attr(data-label);
                font-weight: 600;
                color: #475569;
                margin-right: 10px;
                flex-shrink: 0;
            }

            .faq-actions {
                flex-direction: column;
                width: 100%;
            }

            .faq-action-btn {
                width: 100%;
                justify-content: center;
            }
        }

        @media screen and (max-width: 768px) {
            .faq-container {
                padding: 15px;
            }

            .faq-header h1 {
                font-size: 22px;
            }

            .section-card {
                padding: 20px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="faq-container">
        <%-- Header Section --%>
        <div class="faq-header">
            <h1>
                <i class="fas fa-question-circle"></i>
                FAQ Management
            </h1>
            <p>Manage frequently asked questions for the chat assistant</p>
        </div>

        <%-- Main Content Card --%>
        <div class="section-card active" data-section="faq">
            <div class="alert-info">
                <i class="fas fa-info-circle"></i> 
                <div>
                    <strong>Instructions:</strong> 
                    Manage frequently asked questions for the chat assistant. Add, edit, remove, or reorder questions.
                </div>
            </div>

            <asp:UpdatePanel ID="upFaqSection" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <%-- Add New FAQ Section --%>
                    <div class="add-faq-section">
                        <h5>
                            <i class="fas fa-plus-circle"></i> Add New FAQ
                        </h5>

                        <div class="form-group">
                            <label class="form-label">Question *</label>
                            <asp:TextBox ID="txtNewFaqQuestion" runat="server" CssClass="form-input" 
                                placeholder="Enter the question..." MaxLength="500" />
                            <div class="help-text">Keep questions clear and concise</div>
                        </div>

                        <div class="form-group">
                            <label class="form-label">Answer *</label>
                            <asp:TextBox ID="txtNewFaqAnswer" runat="server" TextMode="MultiLine" 
                                Rows="4" CssClass="form-input" placeholder="Enter the answer..." />
                            <div class="help-text">Provide detailed but easy-to-understand answers</div>
                        </div>

                        <asp:Button ID="btnAddFaq" runat="server" Text="Add FAQ" 
                            CssClass="btn-update" OnClick="btnAddFaq_Click" />
                    </div>

                    <%-- FAQ List Section --%>
                    <div class="faq-list-section">
                        <h5>
                            <i class="fas fa-list"></i> Current FAQs
                        </h5>

                        <asp:GridView ID="gvFaqs" runat="server" AutoGenerateColumns="False" 
                            CssClass="table table-hover faq-grid" 
                            OnRowCommand="gvFaqs_RowCommand"
                            OnRowEditing="gvFaqs_RowEditing"
                            OnRowCancelingEdit="gvFaqs_RowCancelingEdit"
                            OnRowUpdating="gvFaqs_RowUpdating"
                            OnRowDeleting="gvFaqs_RowDeleting"
                            DataKeyNames="ID"
                            GridLines="None">
                            
                            <HeaderStyle BackColor="#2563eb" ForeColor="White" Font-Bold="True" CssClass="faq-header" />
                            <RowStyle BackColor="White" CssClass="faq-row" />
                            <AlternatingRowStyle BackColor="#f8fafc" CssClass="faq-row-alt" />

                            <Columns>
                                <%-- Order Column --%>
                                <asp:TemplateField HeaderText="Order" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnMoveUp" runat="server" 
                                            CommandName="MoveUp" 
                                            CommandArgument='<%# Eval("ID") %>'
                                            CssClass="btn btn-sm btn-outline-primary faq-move-btn"
                                            ToolTip="Move Up">
                                            <i class="fas fa-arrow-up"></i>
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnMoveDown" runat="server" 
                                            CommandName="MoveDown" 
                                            CommandArgument='<%# Eval("ID") %>'
                                            CssClass="btn btn-sm btn-outline-primary faq-move-btn"
                                            ToolTip="Move Down">
                                            <i class="fas fa-arrow-down"></i>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <%-- Question Column --%>
                                <asp:TemplateField HeaderText="Question">
                                    <ItemTemplate>
                                        <strong class="faq-question">
                                            <%# Eval("Question") %>
                                        </strong>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtEditQuestion" runat="server" 
                                            Text='<%# Bind("Question") %>'
                                            CssClass="form-control" 
                                            MaxLength="500" />
                                    </EditItemTemplate>
                                </asp:TemplateField>

                                <%-- Answer Column --%>
                                <asp:TemplateField HeaderText="Answer">
                                    <ItemTemplate>
                                        <div class="faq-answer">
                                            <%# Eval("Answer") %>
                                        </div>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtEditAnswer" runat="server" 
                                            Text='<%# Bind("Answer") %>'
                                            TextMode="MultiLine" 
                                            Rows="3"
                                            CssClass="form-control" />
                                    </EditItemTemplate>
                                </asp:TemplateField>

                                <%-- Status Column --%>
                                <asp:TemplateField HeaderText="Status" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center" Visible ="False">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnToggleActive" runat="server"
                                            CommandName="ToggleActive"
                                            CommandArgument='<%# Eval("ID") %>'
                                            CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "btn btn-sm btn-success faq-status-btn" : "btn btn-sm btn-secondary faq-status-btn" %>'>
                                            <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions" ItemStyle-Width="180px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <div class="faq-actions">
                                            <asp:LinkButton ID="btnEdit" runat="server" 
                                                CommandName="Edit"
                                                CssClass="btn btn-sm btn-primary faq-action-btn">
                                                <i class="fas fa-edit"></i> Edit
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server" 
                                                CssClass="btn btn-sm btn-danger faq-action-btn"
                                                OnClientClick='<%# "showDeleteConfirm(" + Container.DataItemIndex + "); return false;" %>'>
                                                <i class="fas fa-trash"></i> Remove
                                            </asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <div class="faq-actions">
                                            <asp:LinkButton ID="btnUpdate" runat="server" 
                                                CommandName="Update"
                                                CssClass="btn btn-sm btn-success faq-action-btn">
                                                <i class="fas fa-check"></i> Save
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnCancel" runat="server" 
                                                CommandName="Cancel"
                                                CssClass="btn btn-sm btn-secondary faq-action-btn">
                                                <i class="fas fa-times"></i> Cancel
                                            </asp:LinkButton>
                                        </div>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                            </Columns>

                            <EmptyDataTemplate>
                                <div class="faq-empty">
                                    <i class="fas fa-inbox"></i>
                                    <p>No FAQs found. Add your first FAQ above!</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
    <asp:HiddenField ID="hfDeleteRowIndex" runat="server" />

    <asp:Button ID="btnConfirmDelete" runat="server" 
    OnClick="btnConfirmDelete_Click" 
    Style="display:none;" />

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

<script>
    function showDeleteConfirm(rowIndex) {
        Swal.fire({
            title: 'Are you sure?',
            text: "Do you want to remove this FAQ? This action will archive the FAQ.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, remove it!',
            cancelButtonText: 'Cancel',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                // Store the row index in hidden field
                document.getElementById('<%= hfDeleteRowIndex.ClientID %>').value = rowIndex;
                // Trigger the server-side delete
                document.getElementById('<%= btnConfirmDelete.ClientID %>').click();
            }
        });
    }

    // Apply responsive table headers
    function applyResponsiveTableHeaders() {
        const grid = document.querySelector('.faq-grid');
        if (!grid) return;

        const headers = [];
        const headerRow = grid.querySelector('thead tr');
        if (!headerRow) return;

        headerRow.querySelectorAll('th').forEach(th => {
            headers.push(th.innerText.trim());
        });

        if (headers.length === 0) return;

        grid.querySelectorAll('tbody tr').forEach(row => {
            row.querySelectorAll('td').forEach((td, index) => {
                if (headers[index]) {
                    td.setAttribute('data-label', headers[index]);
                }
            });
        });
    }

    // Run on page load
    document.addEventListener('DOMContentLoaded', applyResponsiveTableHeaders);

    // Run after UpdatePanel refresh
    if (typeof Sys !== 'undefined') {
        try {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(applyResponsiveTableHeaders);
        } catch (e) {
            console.error("Error attaching to PageRequestManager:", e);
        }
    }
</script>
</asp:Content>
