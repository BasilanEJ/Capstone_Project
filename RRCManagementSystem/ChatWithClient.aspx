<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ChatWithClient.aspx.cs" Inherits="RRCManagementSystem.ChatWithClient" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white text-center fw-bold">
                Chat with Client
            </div>
            <div class="card-body d-flex flex-column" style="height: 80vh">
                <asp:Label ID="lblInfo" runat="server" CssClass="text-danger mb-2" />

                <asp:DropDownList ID="ddlClients" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlClients_SelectedIndexChanged" CssClass="form-select mb-3" />

                <div id="chatBox" class="border rounded bg-light p-3 mb-3 overflow-auto flex-grow-1" style="min-height: 200px;">
                    <asp:Repeater ID="rptMessages" runat="server">
                        <ItemTemplate>
                            <div class="mb-3">
                                <div class='<%# Eval("SenderType").ToString() == "Admin" ? "bg-primary text-white text-start rounded p-2" : "bg-success text-white text-end rounded p-2 ms-auto" %>'>
                                    <%# Eval("MessageText") %>

                                    <%#
                                        (Eval("AttachmentName") != DBNull.Value && Eval("AttachmentType") != DBNull.Value && Eval("AttachmentType").ToString().ToLower().StartsWith("image/"))
                                        ? "<br/><a href='DownloadAttachment.aspx?ID=" + Eval("MessageID") + "' target='_blank'>" +
                                          "<img src='DownloadAttachment.aspx?ID=" + Eval("MessageID") + "' class='img-thumbnail mt-2' style='max-width: 200px;' alt='" + Eval("AttachmentName") + "' onerror=\"this.style.display='none';\" /></a>"
                                        : (Eval("AttachmentName") != DBNull.Value
                                          ? "<br/><a href='DownloadAttachment.aspx?ID=" + Eval("MessageID") + "' target='_blank' class='d-block mt-2 text-decoration-underline text-info'>📎 " + Eval("AttachmentName") + "</a>"
                                          : "")
                                    %>
                                    <div class="text-end small text-muted mt-1">
                                        <%# Convert.ToDateTime(Eval("SentAt")).ToString("MMM dd, yyyy hh:mm tt") %>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <div class="d-flex align-items-center gap-2">
                    <asp:TextBox ID="txtMessage" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" placeholder="Type your message..."></asp:TextBox>

                    <label class="btn btn-outline-primary mb-0" onclick="document.getElementById('<%= fileAttachment.ClientID %>').click();">
                        <i class="fas fa-paperclip"></i>
                    </label>
                    <asp:FileUpload ID="fileAttachment" runat="server" CssClass="d-none" />

                    <asp:Button ID="btnSend" runat="server" Text="Send" CssClass="btn btn-primary" OnClick="btnSend_Click" />
                </div>
            </div>
        </div>
    </div>

    <script>
        window.onload = function () {
            var chatBox = document.getElementById('chatBox');
            if (chatBox) chatBox.scrollTop = chatBox.scrollHeight;
        };
    </script>
</asp:Content>
