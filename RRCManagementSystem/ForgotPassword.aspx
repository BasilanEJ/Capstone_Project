<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="RRCManagementSystem.ForgotPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <meta charset="UTF-8" />
    <link rel="icon" type="image/png" href="~/Images/rrc-logo.jpg" />
    <title>Forgot Password - RRC Management System</title>

    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />

    <style>
        /*
         * INLINE STYLES FROM Login.aspx - Ensures visual consistency
         */
        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        body, html {
            height: 100%;
            font-family: 'Poppins', sans-serif;
            color: #333;
            -webkit-font-smoothing: antialiased;
            -moz-osx-font-smoothing: grayscale;
        }

        body {
            background: linear-gradient(135deg, rgba(0, 123, 255, 0.1) 0%, rgba(0, 86, 179, 0.2) 100%),
                        url('images/logo.jpg') no-repeat center center fixed;
            background-size: cover;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            padding: 15px;
            overflow-x: hidden;
        }

        .login-container {
            background: rgba(255, 255, 255, 0.98);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            width: 100%;
            max-width: 480px;
            padding: 50px 40px;
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
            border: 1px solid rgba(255, 255, 255, 0.3);
            transition: all 0.4s ease;
            animation: fadeInUp 0.6s ease;
            margin: 0 auto;
        }

        @keyframes fadeInUp {
            from { opacity: 0; transform: translateY(30px); }
            to { opacity: 1; transform: translateY(0); }
        }

        .login-container:hover {
            transform: translateY(-8px);
            box-shadow: 0 25px 70px rgba(0, 0, 0, 0.35);
        }

        .logo {
            width: 100%;
            max-width: 200px;
            height: auto;
            margin-bottom: 20px;
            filter: drop-shadow(0 4px 8px rgba(0, 0, 0, 0.1));
            transition: transform 0.3s ease;
        }

        .logo:hover {
            transform: scale(1.05);
        }

        h2 {
            color: #1a1a1a;
            font-size: clamp(26px, 6vw, 32px);
            margin-bottom: 10px;
            font-weight: 700;
            letter-spacing: -0.5px;
        }

        .subtitle {
            color: #666;
            font-size: clamp(13px, 3.5vw, 15px);
            margin-bottom: 30px;
            font-weight: 400;
            line-height: 1.6;
        }

        .input-group {
            margin-bottom: 20px;
            width: 100%;
        }

        .input {
            width: 100%;
            padding: 14px 18px;
            font-size: clamp(14px, 3.5vw, 16px);
            border: 2px solid #e0e0e0;
            border-radius: 12px;
            background: #f8f9fa;
            color: #333;
            transition: all 0.3s ease;
            font-family: 'Poppins', sans-serif;
            outline: none;
        }

        .input:focus {
            border-color: #007bff;
            background: #ffffff;
            box-shadow: 0 0 0 4px rgba(0, 123, 255, 0.1);
            transform: translateY(-2px);
        }

        .input::placeholder {
            color: #999;
        }

        /* UPDATED: Matches the blue gradient of the Login.aspx Sign In button */
        .btn-login {
            width: 100%;
            padding: 15px;
            font-size: clamp(15px, 4vw, 17px);
            font-weight: 600;
            color: #ffffff;
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            border: none;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(0, 123, 255, 0.3);
            margin-top: 10px;
        }

        .btn-login:hover {
            background: linear-gradient(135deg, #0056b3 0%, #004494 100%);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0, 123, 255, 0.4);
        }

        .btn-login:active {
            transform: translateY(0);
        }

        .links-container {
            margin-top: 25px;
            width: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 0 10px;
        }

        .link {
            color: #007bff;
            text-decoration: none;
            font-size: clamp(13px, 3.5vw, 14px);
            font-weight: 500;
            transition: all 0.3s ease;
            padding: 4px 8px;
            border-radius: 6px;
        }

        .link:hover {
            color: #0056b3;
            background: rgba(0, 123, 255, 0.1);
        }

        #messageContainer {
            margin-top: 20px;
            width: 100%;
            transition: all 0.3s ease;
            opacity: 0;
            transform: translateY(-10px);
        }

        #messageContainer.show {
            opacity: 1;
            transform: translateY(0);
        }

        .message {
            font-size: clamp(12px, 3.5vw, 13px);
            color: #dc3545;
            line-height: 1.6;
            padding: 14px 16px;
            background: none;
            border-radius: 8px;
            text-align: left;
            word-wrap: break-word;
            overflow-wrap: break-word;
            max-width: 100%;
            box-sizing: border-box;
        }
        
        .message strong {
            font-weight: 600;
            margin-right: 5px;
            white-space: nowrap;
        }

        /* Responsive adjustments */
        @media screen and (max-width: 768px) {
            .login-container {
                padding: 40px 30px;
            }
        }

        @media screen and (max-width: 480px) {
            .login-container {
                padding: 30px 20px;
                border-radius: 16px;
                transform: none !important;
                max-width: 100%;
            }

            .login-container:hover {
                transform: none !important;
                box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            }

            .logo {
                max-width: 150px;
                margin-bottom: 16px;
            }

            .subtitle {
                margin-bottom: 25px;
            }

            .input-group {
                margin-bottom: 16px;
            }

            .input {
                padding: 13px 16px;
            }

            .btn-login {
                padding: 14px;
            }

            .links-container {
                margin-top: 20px;
            }
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <img src="images/logorrc.png" alt="RRC Logo" class="logo" />
            <h2>Forgot Password</h2>
            <p class="subtitle">
                Enter your registered email address to receive a **One-Time Password (OTP)** for password reset.
            </p>

            <div class="input-group">
                <asp:TextBox
                    ID="txtEmail"
                    runat="server"
                    CssClass="input"
                    placeholder="Enter your email"
                    TextMode="Email"
                    MaxLength="100" />
            </div>

            <asp:Button
                ID="btnSubmit"
                runat="server"
                Text="Send OTP"
                CssClass="btn-login"
                OnClick="btnSubmit_Click"
                UseSubmitBehavior="false" />

            <div id="messageContainer">
                <asp:Label
                    ID="lblMessage"
                    runat="server"
                    CssClass="message" />
            </div>

            <div class="links-container">
                <a href="Login.aspx" class="link">
                    <i class="fa-solid fa-arrow-left" style="margin-right: 5px;"></i>Back to Login
                </a>
            </div>
        </div>

        <script>
            // Utility function for showing and formatting the message, adapted from Login.aspx
            function initScripts() {
                const msgContainer = document.getElementById('messageContainer');
                const msgLabel = msgContainer ? document.getElementById('<%= lblMessage.ClientID %>') : null;

                if (msgContainer && msgLabel) {
                    const text = msgLabel.innerText.trim();
                    if (text !== "") {
                        // Attempt to format the standard error/success message
                        const errorRegex = /^(.*?\.?)\s*(.*)$/;
                        const match = text.match(errorRegex);

                        let formattedText = text;

                        if (match && match[1].length > 0) {
                            // Reconstruct the message with <strong> for the first part
                            const boldPart = match[1].trim();
                            const restOfMessage = match[2].trim();
                            formattedText = `<strong>${boldPart}</strong> ${restOfMessage}`;
                        } else {
                            // Fallback for custom messages using **Markdown**
                            formattedText = text.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
                        }

                        msgLabel.innerHTML = formattedText;
                        msgContainer.classList.add('show');
                    } else {
                        msgContainer.classList.remove('show');
                    }
                }

                // Prevent zoom on iOS (copied from Login.aspx)
                const el = document.querySelector('meta[name=viewport]');
                if (el !== null) {
                    let content = el.getAttribute('content');
                    let re = /maximum\-scale=[0-9\.]+/g;
                    if (re.test(content)) {
                        content = content.replace(re, 'maximum-scale=1.0');
                    } else {
                        content = [content, 'maximum-scale=1.0'].join(', ')
                    }
                    el.setAttribute('content', content);
                }
            }

            // Initialize scripts after DOM is ready
            document.addEventListener("DOMContentLoaded", initScripts);

            // Re-initialize after AJAX postback 
            if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initScripts);
            }
        </script>
    </form>
</body>
</html>