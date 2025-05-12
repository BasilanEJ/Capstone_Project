<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ChatWithClient.aspx.cs" Inherits="RRCManagementSystem.ChatWithClient" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .chat-container {
            max-width: 800px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
            padding: 25px;
            font-family: Arial, sans-serif;
            height: 85vh;
            display: flex;
            flex-direction: column;
        }

        .chat-header {
            font-size: 24px;
            color: #004085;
            font-weight: bold;
            margin-bottom: 20px;
            text-align: center;
        }

        .form-control {
            width: 100%;
            padding: 10px 15px;
            margin-bottom: 10px;
            font-size: 14px;
            border: 1px solid #ccc;
            border-radius: 5px;
            box-sizing: border-box;
        }

        .chat-box {
            flex-grow: 1;
            overflow-y: auto;
            border: 1px solid #ccc;
            padding: 15px;
            background-color: #f9f9f9;
            margin-bottom: 10px;
            border-radius: 5px;
            display: flex;
            flex-direction: column;
        }

        .message {
            margin-bottom: 15px;
            padding: 10px 15px;
            border-radius: 12px;
            max-width: 70%;
            word-wrap: break-word;
        }

        .admin-message {
            background-color: #004085;
            color: #ffffff;
            align-self: flex-start;
            margin-right: auto;
        }

        .client-message {
            background-color: #28a745;
            color: #ffffff;
            align-self: flex-end;
            margin-left: auto;
        }

        .time-stamp {
            font-size: 10px;
            color: gray;
            margin-top: 5px;
            text-align: right;
        }

        .attachment-link {
            font-size: 13px;
            color: #1877f2;
            text-decoration: underline;
            margin-top: 5px;
            display: block;
        }

        .img-preview {
            max-width: 200px;
            max-height: 200px;
            margin-top: 8px;
            border-radius: 6px;
            object-fit: cover;
            border: 1px solid #ccc;
        }

        .chat-input {
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .upload-icon-label {
            cursor: pointer;
            color: #004085;
            font-size: 24px;
            transition: color 0.3s ease;
        }

        .upload-icon-label:hover {
            color: #0084ff;
        }

        .file-upload {
            display: none;
        }

        .btn-send {
            background-color: #004085;
            color: #ffffff;
            border: none;
            padding: 10px 20px;
            border-radius: 5px;
            font-size: 14px;
            cursor: pointer;
        }

        .btn-send:hover {
            background-color: #003366;
        }
    </style>

    <div class="chat-container">
        <div class="chat-header">Chat with Client</div>

        <asp:Label ID="lblInfo" runat="server" ForeColor="Red" />

        <asp:DropDownList ID="ddlClients" runat="server" AutoPostBack="true"
            OnSelectedIndexChanged="ddlClients_SelectedIndexChanged" CssClass="form-control" />

        <div class="chat-box" id="chatBox">
            <asp:Repeater ID="rptMessages" runat="server">
                <ItemTemplate>
                    <div class="message <%# Eval("SenderType").ToString() == "Admin" ? "admin-message" : "client-message" %>">
                        <%# Eval("MessageText") %>

                        <%# 
                            (Eval("AttachmentName") != DBNull.Value && 
                             Eval("AttachmentType") != DBNull.Value && 
                             Eval("AttachmentType").ToString().ToLower().StartsWith("image/")) 
                             ? "<br/><a href='DownloadAttachment.aspx?ID=" + Eval("MessageID") + "' target='_blank'>" +
                               "<img src='DownloadAttachment.aspx?ID=" + Eval("MessageID") + "' class='img-preview' alt='" + Eval("AttachmentName") + "' onerror=\"this.style.display='none';\" /></a>" 
                             : (Eval("AttachmentName") != DBNull.Value 
                               ? "<br/><a href='DownloadAttachment.aspx?ID=" + Eval("MessageID") + "' target='_blank' class='attachment-link'>📎 " + Eval("AttachmentName") + "</a>" 
                               : "") 
                        %>

                        <div class="time-stamp">
                            <%# Convert.ToDateTime(Eval("SentAt")).ToString("MMM dd, yyyy hh:mm tt") %>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <div class="chat-input">
            <asp:TextBox ID="txtMessage" runat="server" CssClass="form-control"
                TextMode="MultiLine" Rows="2" placeholder="Type your message..."></asp:TextBox>

            <label class="upload-icon-label" onclick="document.getElementById('<%= fileAttachment.ClientID %>').click();">
                <i class="fas fa-paperclip"></i>
            </label>
            <asp:FileUpload ID="fileAttachment" runat="server" CssClass="file-upload" />

            <asp:Button ID="btnSend" runat="server" Text="Send" CssClass="btn-send"
                OnClick="btnSend_Click" />
        </div>
    </div>

    <script>
        window.onload = function () {
            var chatBox = document.getElementById('chatBox');
            if (chatBox) chatBox.scrollTop = chatBox.scrollHeight;
        };
    </script>
</asp:Content>