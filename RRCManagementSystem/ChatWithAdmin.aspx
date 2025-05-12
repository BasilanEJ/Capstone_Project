<%@ Page Title="" Language="C#" MasterPageFile="~/Client.master" AutoEventWireup="true" CodeBehind="ChatWithAdmin.aspx.cs" Inherits="RRCManagementSystem.ChatWithAdmin" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .chat-container {
            max-width: 700px;
            margin: 30px auto;
            background: #ffffff;
            padding: 20px;
            border-radius: 20px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            display: flex;
            flex-direction: column;
            height: 85vh;
        }

        .chat-header {
            font-size: 22px;
            font-weight: bold;
            color: #004085;
            margin-bottom: 10px;
        }

        .chat-box {
            flex-grow: 1;
            overflow-y: auto;
            padding: 15px;
            background-color: #f0f2f5;
            border-radius: 12px;
            margin-bottom: 15px;
        }

        .message {
            display: flex;
            flex-direction: column;
            margin-bottom: 15px;
            max-width: 70%;
        }

        .admin-message {
            align-self: flex-start;
            background-color: #e4e6eb;
            color: #050505;
            border-radius: 18px 18px 18px 0;
            padding: 10px 15px;
        }

        .client-message {
            align-self: flex-end;
            background-color: #0084ff;
            color: #ffffff;
            border-radius: 18px 18px 0 18px;
            padding: 10px 15px;
        }

        .timestamp {
            font-size: 10px;
            color: #777;
            margin-top: 5px;
            text-align: right;
        }

        .attachment-link {
            margin-top: 5px;
            font-size: 13px;
            color: #1877f2;
            text-decoration: underline;
            cursor: pointer;
        }

        .img-preview {
            max-width: 200px;
            max-height: 200px;
            margin-top: 8px;
            border-radius: 6px;
            object-fit: cover;
            border: 1px solid #ccc;
        }

        .chat-input-area {
            display: flex;
            align-items: center;
            gap: 10px;
            margin-top: 10px;
        }

        .chat-input-area textarea {
            flex-grow: 1;
            resize: none;
            border-radius: 20px;
            padding: 12px;
            border: 1px solid #ccc;
            font-size: 14px;
        }

        .upload-icon-label {
            cursor: pointer;
            color: #004085;
            font-size: 24px;
            margin: 0 5px;
            transition: color 0.3s ease;
        }

        .upload-icon-label:hover {
            color: #0084ff;
        }

        .file-upload {
            display: none;
        }

        .btn-send {
            padding: 12px 20px;
            background-color: #004085;
            color: white;
            border: none;
            border-radius: 20px;
            cursor: pointer;
        }

        .btn-send:hover {
            background-color: #002752;
        }

        .error-message {
            color: red;
            font-size: 13px;
            margin-bottom: 8px;
        }
    </style>

    <div class="chat-container">
        <div class="chat-header">Chat with Admin</div>

        <asp:Label ID="lblInfo" runat="server" CssClass="error-message" />

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

                        <div class="timestamp">
                            <%# Convert.ToDateTime(Eval("SentAt")).ToString("MMM dd, yyyy hh:mm tt") %>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <div class="chat-input-area">
            <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control" placeholder="Type a message..."></asp:TextBox>
            <label class="upload-icon-label" onclick="document.getElementById('<%= fileAttachment.ClientID %>').click();">
                <i class="fas fa-paperclip"></i>
            </label>
            <asp:FileUpload ID="fileAttachment" runat="server" CssClass="file-upload" />
            <asp:Button ID="btnSend" runat="server" Text="Send" CssClass="btn-send" OnClick="btnSend_Click" />
        </div>
    </div>

    <script>
        window.onload = function () {
            var chatBox = document.getElementById('chatBox');
            chatBox.scrollTop = chatBox.scrollHeight;
        };
    </script>
</asp:Content>