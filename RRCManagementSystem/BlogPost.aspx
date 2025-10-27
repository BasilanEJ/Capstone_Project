<%@ Page Title="Blog Post" Language="C#" MasterPageFile="~/Inquiry.Master" AutoEventWireup="true" CodeBehind="BlogPost.aspx.cs" Inherits="RRCManagementSystem.BlogPost" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Styles from your example and DIY.aspx */
        .blog-post-content {
            font-size: 18px;
            line-height: 1.6;
            color: #2d3748;
        }

        /* Styles for content from the Rich Text Editor */
        .blog-post-content h2 {
            font-size: 28px; 
            color: #121481; 
            margin-top: 40px;
            margin-bottom: 20px;
        }

        .blog-post-content h3 {
            font-size: 22px; 
            color: #333;
            margin-top: 30px;
            margin-bottom: 15px;
            font-weight: 600;
        }

        .blog-post-content p {
            margin-bottom: 20px;
        }

        .blog-post-content ul,
        .blog-post-content ol {
            margin-bottom: 20px;
            margin-left: 20px; 
        }
        
        .blog-post-content li {
            margin-bottom: 10px;
            line-height: 1.6;
        }
        
        .blog-post-content li > strong {
             color: #121481; /* Make list headers stand out */
        }

        .blog-post-content img {
            max-width: 100%;
            height: auto;
            border-radius: 8px;
            margin: 20px 0;
        }
        
        .blog-post-meta {
            font-size: 14px;
            color: #718096;
            margin-top: 15px;
        }

        .blog-post-meta-item {
            display: inline-block;
            margin-right: 20px;
        }

        .blog-not-found {
            text-align: center;
            padding: 80px 20px;
            color: #718096;
        }
        
        .blog-not-found h2 {
            font-size: 30px;
            color: #121481;
        }
        
        /* "More Blogs" section styles from your example */
        .more-blogs-container {
            text-align: center;
            margin-top: 60px;
            padding-top: 40px;
            border-top: 1px solid #e2e8f0;
        }
        
        .more-blogs-container h2 {
            font-size: 24px;
            color: #121481;
            margin-bottom: 20px;
        }

        .more-blogs-grid {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: 20px;
        }

        .blog-card {
            text-decoration: none;
            color: inherit;
            width: 300px;
            border: 1px solid #ddd;
            border-radius: 10px;
            overflow: hidden;
            transition: transform 0.3s, box-shadow 0.3s;
            text-align: left;
            box-shadow: 0 2px 5px rgba(0,0,0,0.05);
        }
        .blog-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }

        .blog-card img {
            width: 100%;
            height: 200px;
            object-fit: cover;
            margin: 0;
        }

        .blog-card-content {
            padding: 15px;
        }

        .blog-card-content h3 {
            font-size: 18px;
            font-weight: bold;
            color: #121481; /* Match heading color */
            margin: 0;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <asp:Panel ID="pnlBlogPost" runat="server">

        <div class="container" style="padding:40px 20px; max-width:1200px; margin:0 auto;">

            <div style="display:flex; flex-wrap:wrap; align-items:center; gap:20px; margin-bottom:40px;">
                
                <div style="flex:1 1 500px;">
                    <h1 style="font-size:32px; color:#121481; margin-bottom:20px;">
                        <asp:Literal ID="litBlogTitle" runat="server" />
                    </h1>
                    <p style="font-size:18px; line-height:1.6;">

                        <asp:Literal ID="litBlogDescription" runat="server" />
                    </p>
                    
                    <div class="blog-post-meta">
                        <span class="blog-post-meta-item">
                            👤 By <asp:Literal ID="litAuthor" runat="server" Text="RRC Team" />
                        </span>
                        <span class="blog-post-meta-item">
                            ⏱️ <asp:Literal ID="litReadTime" runat="server" Text="5" /> min read
                        </span>
                    </div>
                </div>
                
                <div style="flex:1 1 400px;">

                    <asp:Image ID="imgFeatured" runat="server" 
                        style="width:100%; border-radius:10px; object-fit:cover;" 
                        alt="Blog Featured Image" />
                </div>
            </div>

            <hr style="border-top: 1px solid #eee; margin: 40px 0;" />

            <div class="blog-post-content">

                <asp:Literal ID="litBlogContent" runat="server" />
            </div>
            
        </div>
    </asp:Panel>
    
    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="container blog-not-found" style="max-width: 1200px; margin: 0 auto;">
            <h2>🙁 Blog Post Not Found</h2>
            <p>The post you are looking for does not exist or is no longer active.</p>
            <a href="Default.aspx" style="text-decoration: none; color: #667eea; font-weight: 600;">
                &larr; Back to Home
            </a>
        </div>
    </asp:Panel>
    

    <asp:Panel ID="pnlMoreBlogs" runat="server" Visible="false">
        <div class="container more-blogs-container" style="max-width: 1200px; margin: 0 auto;">
            <h2>More Blogs</h2>
            <div class="more-blogs-grid">
                <asp:Repeater ID="rptMoreBlogs" runat="server">
                    <ItemTemplate>
                        <a href='BlogPost.aspx?id=<%# Eval("BlogID") %>' class="blog-card">
                            <asp:Image ID="imgBlogCard" runat="server" 
                                ImageUrl='<%# Eval("BlogImagePath") %>' 
                                alt='<%# Eval("BlogTitle") %>' 
                                onerror="this.src='/images/placeholder.png'" />
                            <div class="blog-card-content">
                                <h3><%# Eval("BlogTitle") %></h3>
                            </div>
                        </a>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </asp:Panel>

</asp:Content>