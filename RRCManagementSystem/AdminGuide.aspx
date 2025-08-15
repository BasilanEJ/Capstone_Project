<%@ Page Title="Administrator Guide"
    Language="C#"
    MasterPageFile="~/Admin.Master"
    AutoEventWireup="true"
    CodeBehind="AdminGuide.aspx.cs"
    Inherits="RRCManagementSystem.AdminGuide" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        :root{
            --ink:#1f2937;          /* text */
            --muted:#6b7280;        /* secondary text */
            --brand:#0d6efd;        /* accents/links */
            --bg:#f8fafc;           /* page bg */
            --card:#ffffff;         /* card bg */
            --border:#e5e7eb;       /* hairlines */
        }
        .guide-wrap{max-width:1100px;margin:0 auto;padding:24px}
        .guide-title{font-weight:800;color:var(--ink);font-size:clamp(1.5rem,2.2vw,2rem);letter-spacing:.2px}
        .guide-meta{color:var(--muted);font-size:.95rem}
        .tools{display:flex;gap:12px;flex-wrap:wrap;margin-top:12px}
        .btn-ghost{
            border:1px solid var(--border);background:var(--card);color:var(--ink);
            border-radius:10px;padding:8px 12px;font-weight:600;cursor:pointer
        }
        .btn-ghost:hover{border-color:#c7cdd4}
        .guide-card{
            background:var(--card);border:1px solid var(--border);border-radius:16px;
            padding:20px;box-shadow:0 6px 18px rgba(0,0,0,.04);margin-top:16px
        }
        .toc h3{margin:0 0 12px 0;font-size:1.05rem;color:var(--muted);font-weight:700;text-transform:uppercase;letter-spacing:.08em}
        .toc a{color:var(--brand);text-decoration:none}
        .toc a:hover{text-decoration:underline}
        .toc ul{columns:2;gap:24px;margin:0;padding-left:18px}
        .searchbar{position:relative;margin-top:14px}
        .searchbar input{
            width:100%;padding:12px 40px 12px 12px;border:1px solid var(--border);border-radius:10px;
            outline:none
        }
        .searchbar .icon{position:absolute;right:12px;top:50%;transform:translateY(-50%);color:var(--muted)}
        .section{margin-top:22px}
        .section h2{font-size:1.25rem;margin:0 0 6px 0;color:var(--ink)}
        .subtle{color:var(--muted)}
        details{
            border:1px solid var(--border);border-radius:14px;background:var(--card);
            padding:0;margin-top:14px;overflow:hidden
        }
        summary{
            list-style:none;padding:16px 18px;font-weight:700;cursor:pointer;
            display:flex;align-items:center;justify-content:space-between
        }
        summary::-webkit-details-marker{display:none}
        .chev{transition:transform .2s ease}
        details[open] .chev{transform:rotate(90deg)}
        .panel{padding:0 18px 18px 18px;border-top:1px solid var(--border)}
        .panel ul{margin:10px 0 0 18px}
        .divider{height:1px;background:var(--border);margin:24px 0}
        .footer-note{font-size:.95rem;color:var(--muted)}
        .back-top{position:fixed;right:22px;bottom:22px}
        .back-top a{display:inline-block;padding:10px 12px;border-radius:10px;border:1px solid var(--border);background:var(--card);text-decoration:none}
        @media (max-width:740px){ .toc ul{columns:1} }
        /* print */
        @media print{
            .tools,.searchbar,.back-top{display:none !important}
            .guide-wrap{padding:0}
            a{color:inherit;text-decoration:none}
            details{border:none;padding:0}
            summary{padding:0 0 6px 0}
            .panel{padding:0;border:none}
            .guide-card{box-shadow:none;border:none}
        }
    </style>
    <script>
        // Simple client-side filter for <details> sections
        document.addEventListener('DOMContentLoaded', function () {
            var input = document.getElementById('filterBox');
            var groups = Array.from(document.querySelectorAll('[data-filter-item]'));

            function normalize(s){ return (s||'').toLowerCase().trim(); }

            function applyFilter() {
                var q = normalize(input.value);
                groups.forEach(function(el){
                    var hay = normalize(el.getAttribute('data-filter-item'));
                    var body = el.querySelector('.panel');
                    if(!q || hay.includes(q)){
                        el.style.display = '';
                        // auto-open when matched
                        if(q){ el.setAttribute('open','open'); }
                    } else {
                        el.style.display = 'none';
                        el.removeAttribute('open');
                    }
                });
            }
            input.addEventListener('input', applyFilter);
        });

        function copyTocLink(anchorId){
            const url = new URL(window.location);
            url.hash = anchorId;
            navigator.clipboard.writeText(url.toString()).then(()=> {
                const btn = document.getElementById('btnCopy');
                if(btn){ btn.textContent = 'Link copied!'; setTimeout(()=>btn.textContent='Copy link',1200); }
            });
        }
        function printGuide(){ window.print(); }
    </script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="guide-wrap">
        <div class="guide-header">
            <div class="guide-title">Administrator Guide</div>
            <div class="guide-meta">
                RRC Termite &amp; Pest Control Management System &middot;
                Last updated: <asp:Label ID="lblUpdated" runat="server" />
            </div>

      <!-- Introduction -->
<div id="top" class="guide-card section">
    <h2>1. Introduction</h2>
    <p>
        Welcome to the RRC Termite &amp; Pest Control Management Business Administrator Guide.
        This manual explains how to use each module to manage clients, employees, equipment,
        bookings, inventory, suppliers, sales, and reports.
    </p>
</div>

<!-- 2. Module Descriptions (Accordion using <details>) -->
<div class="section">
    <h2 class="subtle">2. Module Descriptions</h2>

    <details id="dashboard" data-filter-item="dashboard overview weekly calendar sales blockchain graphs">
        <summary>
            2.1 Dashboard
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <p>The Dashboard provides a quick overview of company performance and upcoming activities. Features include:</p>
            <ul>
                <li><b>Total Clients</b> – Displays the total registered customers.</li>
                <li><b>Total Employees</b> – Shows active employees.</li>
                <li><b>Today's Sales</b> – Displays daily total sales.</li>
                <li><b>This Month's Sales</b> – Summarizes monthly sales.</li>
                <li><b>Weekly Booking Calendar</b> – View booked customers for the current week.</li>
                <li><b>Sales Overview</b> – Switch between Daily, Weekly, Monthly, and Yearly views; choose Bar or Line graph formats.</li>
                <li><b>Blockchain Sales Transparency</b> – Select a date range to view blockchain-verified sales, including Log ID, Transaction ID, Sale Hash, and Timestamp.</li>
            </ul>
        </div>
    </details>

    <details id="manage-inquiry" data-filter-item="manage inquiry inspectors assignments">
        <summary>
            2.2 Manage Inquiry
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <ul>
                <li>Review inquiries submitted via the Inquiry page.</li>
                <li>Assign Inspectors to potential client properties.</li>
            </ul>
        </div>
    </details>

    <details id="create-customer" data-filter-item="create customer account accounts after inspection">
        <summary>
            2.3 Create Customer Account
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <ul>
                <li>Create accounts for customers after a successful property inspection.</li>
            </ul>
        </div>
    </details>

    <details id="manage-employees" data-filter-item="manage employees add archive teams team details">
        <summary>
            2.4 Manage Employees
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <ul>
                <li>View, add, and archive employees.</li>
                <li>Assign employees to service teams and view team details.</li>
            </ul>
        </div>
    </details>

    <details id="manage-items" data-filter-item="manage items chemicals sachet safety gear daily total stocks inventory">
        <summary>
            2.5 Manage Items
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <ul>
                <li>Add and view items such as chemicals, sacheted chemicals, and safety gear.</li>
                <li>Monitor <b>Daily Total Stocks</b> for detailed daily inventory updates.</li>
            </ul>
        </div>
    </details>

    <details id="manage-equipment" data-filter-item="manage equipment sprayers cords wood injectors availability booked days">
        <summary>
            2.6 Manage Equipment
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <ul>
                <li>Add and view service equipment (e.g., sprayers, extension cords, wood injectors).</li>
                <li>Check equipment availability for booked service days.</li>
            </ul>
        </div>
    </details>

    <details id="clients" data-filter-item="clients register archive contracts upload">
        <summary>
            2.7 Clients
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <ul>
                <li>View all registered clients.</li>
                <li>Archive inactive or non-using accounts.</li>
                <li>Upload contracts for customers availing RRC’s services.</li>
            </ul>
        </div>
    </details>

    <details id="bookings" data-filter-item="bookings approve reschedule history contractual">
        <summary>
            2.8 Bookings
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <ul>
                <li>View and approve customer bookings.</li>
                <li>Approve reschedule requests for contractual services.</li>
                <li>View booking history.</li>
            </ul>
        </div>
    </details>

    <details id="sales" data-filter-item="sales transactions history payment methods amounts manual adjust">
        <summary>
            2.9 Sales &amp; Transactions
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <ul>
                <li>View transaction history with payment methods and amounts.</li>
                <li>Manually adjust client payments if needed.</li>
            </ul>
        </div>
    </details>

    <details id="supplier" data-filter-item="supplier add view edit archive">
        <summary>
            2.10 Manage Supplier
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <ul>
                <li>Add and view supplier records.</li>
                <li>Edit supplier details and archive as needed.</li>
            </ul>
        </div>
    </details>

    <details id="services" data-filter-item="services add view edit delete">
        <summary>
            2.11 Manage Services
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <ul>
                <li>Add and view services offered.</li>
                <li>Edit or delete services.</li>
            </ul>
        </div>
    </details>

    <details id="reports" data-filter-item="reports pdf totals inquiries clients equipment bookings user accounts inventory sales inspection team">
        <summary>
            2.12 Reports
            <span class="chev">›</span>
        </summary>
        <div class="panel">
            <p>Generate PDF reports filtered by specific dates. Available reports include:</p>
            <ul>
                <li>User Accounts (excluding admins)</li>
                <li>Inquiries</li>
                <li>Approved Clients</li>
                <li>Total Stocks Snapshot (Daily)</li>
                <li>Inventory Details</li>
                <li>Sales</li>
                <li>Equipment Status</li>
                <li>Booking Details</li>
                <li>Inspection Details</li>
                <li>Team Reports (by date/range)</li>
                <li>Team Members (per roster)</li>
            </ul>
        </div>
    </details>
</div>

<div class="divider"></div>

<!-- 3. Best Practices -->
<div class="guide-card section" id="best">
    <h2>3. Best Practices &amp; Security Notes</h2>
    <ul>
        <li>Always log out after each session.</li>
        <li>Keep all records updated regularly.</li>
        <li>Use strong, unique passwords.</li>
        <li>Back up sales, bookings, and inventory data.</li>
        <li>Retain blockchain logs for transparency and auditing.</li>
    </ul>
</div>

<div class="guide-card section" id="contact"> <h2>4. Contact Information</h2> <p class="footer-note"> For support or inquiries: <b>rrctermiteandpestcontrol@gmail.com</b> </p> </div>

</div>
</asp:Content>
