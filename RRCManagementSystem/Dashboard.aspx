<%@ Page Title="Dashboard"
    Language="C#"
    MasterPageFile="~/Admin.Master"
    AutoEventWireup="true"
    MaintainScrollPositionOnPostBack="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="RRCManagementSystem.Dashboard" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* ===== Modern Dashboard Styles ===== */
        
        /* Page Title with Gradient */
        .page-title { 
            font-size: 1.875rem;
            line-height: 1.2;
            background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }
        @media (min-width: 640px) {
            .page-title { font-size: 2.5rem; }
        }
        
        /* Modern Metric Cards */
        .metric-card {
            position: relative;
            background: white;
            border-radius: 16px;
            padding: 1.5rem;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
            overflow: hidden;
        }
        
        .metric-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 4px;
            background: linear-gradient(90deg, var(--card-color-start), var(--card-color-end));
            transform: scaleX(0);
            transform-origin: left;
            transition: transform 0.4s ease;
        }
        
        .metric-card:hover::before {
            transform: scaleX(1);
        }
        
        @media (hover: hover) {
            .metric-card:hover { 
                transform: translateY(-8px); 
                box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
            }
        }
        
        .metric-icon {
            width: 3rem;
            height: 3rem;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.5rem;
            margin: 0 auto 1rem;
            background: linear-gradient(135deg, var(--card-color-start), var(--card-color-end));
            color: white;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }
        
        @media (min-width: 640px) {
            .metric-icon {
                width: 3.5rem;
                height: 3.5rem;
                font-size: 1.75rem;
            }
        }
        
        .metric-card.card-blue {
            --card-color-start: #3b82f6;
            --card-color-end: #2563eb;
        }
        
        .metric-card.card-purple {
            --card-color-start: #8b5cf6;
            --card-color-end: #7c3aed;
        }
        
        .metric-card.card-green {
            --card-color-start: #10b981;
            --card-color-end: #059669;
        }
        
        .metric-card.card-orange {
            --card-color-start: #f59e0b;
            --card-color-end: #d97706;
        }
        
        .metric-label {
            font-size: 0.875rem;
            font-weight: 600;
            color: #6b7280;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            margin-bottom: 0.5rem;
        }
        
       .fullpage-loader {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(255, 255, 255, 0.98);
    backdrop-filter: blur(8px);
    z-index: 9999;
    display: none;
    align-items: center;
    justify-content: center;
    animation: fadeIn 0.3s ease;
}

.fullpage-loader.active {
    display: flex;
}

@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

.loader-content {
    text-align: center;
    max-width: 500px;
    padding: 2rem;
}
.loader-insect {
    position: relative;
    width: 100%;
    height: 180px;
    margin-bottom: 2rem;
    display: flex;
    align-items: center;
    justify-content: center;
}

/* === Magnifying Glass Container === */
.loader-insect .magnifying-glass {
    position: relative;
    width: 150px;
    height: 150px;
    animation: searchFloat 3s ease-in-out infinite;
}

/* === Lens (Main Circle) === */
.loader-insect .glass-lens {
    position: absolute;
    top: 10px;
    left: 10px;
    width: 80px;
    height: 80px;
    border: 5px solid #3b82f6;
    border-radius: 50%;
    background: linear-gradient(135deg, rgba(255, 255, 255, 0.9) 0%, rgba(219, 234, 254, 0.8) 100%);
    box-shadow: 
        inset 0 4px 12px rgba(59, 130, 246, 0.2),
        inset 0 -4px 8px rgba(37, 99, 235, 0.1),
        0 8px 24px rgba(59, 130, 246, 0.3),
        0 0 40px rgba(59, 130, 246, 0.15);
    animation: lensGlow 2s ease-in-out infinite;
    overflow: hidden;
    z-index: 2;
}

/* === Lens Reflection Effect === */
.loader-insect .glass-lens::before {
    content: '';
    position: absolute;
    top: -50%;
    left: -50%;
    width: 200%;
    height: 200%;
    background: linear-gradient(
        45deg,
        transparent 30%,
        rgba(255, 255, 255, 0.8) 50%,
        transparent 70%
    );
    animation: lensReflection 2.5s linear infinite;
}

/* === Rim (Outer Glow) === */
.loader-insect .glass-rim {
    position: absolute;
    top: 7px;
    left: 7px;
    width: 86px;
    height: 86px;
    border-radius: 50%;
    background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 50%, #2563eb 100%);
    z-index: 1;
    animation: rimPulse 2s ease-in-out infinite;
}

/* === Handle === */
.loader-insect .glass-handle {
    position: absolute;
    width: 12px;
    height: 65px;
    background: linear-gradient(180deg, #3b82f6 0%, #2563eb 50%, #1e40af 100%);
    border-radius: 6px;
    top: 70px;
    left: 70px;
    transform: rotate(-45deg);
    transform-origin: top left;
    box-shadow: 
        0 4px 12px rgba(37, 99, 235, 0.4),
        inset 2px 0 4px rgba(255, 255, 255, 0.3);
    z-index: 0;
}

/* === Handle Highlight === */
.loader-insect .glass-handle::after {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: linear-gradient(90deg, transparent 0%, rgba(255, 255, 255, 0.4) 50%, transparent 100%);
    border-radius: 6px;
}

/* === Shine Effect on Lens === */
.loader-insect .glass-shine {
    position: absolute;
    width: 30px;
    height: 30px;
    background: radial-gradient(circle, rgba(255, 255, 255, 0.95) 0%, rgba(255, 255, 255, 0.6) 50%, transparent 100%);
    border-radius: 50%;
    top: 18px;
    left: 18px;
    filter: blur(1px);
    animation: shineFloat 2s ease-in-out infinite;
    z-index: 3;
}

.loader-insect .search-sparkles {
    position: absolute;
    width: 100%;
    height: 100%;
}

.loader-insect .sparkle {
    position: absolute;
    width: 6px;
    height: 6px;
    background: #3b82f6;
    border-radius: 50%;
    animation: sparkleFloat 2s ease-in-out infinite;
    box-shadow: 0 0 8px #3b82f6;
}

.loader-insect .sparkle:nth-child(1) {
    top: 10%;
    left: 20%;
    animation-delay: 0s;
}

.loader-insect .sparkle:nth-child(2) {
    top: 15%;
    right: 15%;
    animation-delay: 0.3s;
}

.loader-insect .sparkle:nth-child(3) {
    bottom: 20%;
    left: 10%;
    animation-delay: 0.6s;
}

.loader-insect .sparkle:nth-child(4) {
    bottom: 15%;
    right: 20%;
    animation-delay: 0.9s;
}

@keyframes searchFloat {
    0%, 100% {
        transform: translate(0, 0) rotate(0deg);
    }
    25% {
        transform: translate(10px, -10px) rotate(5deg);
    }
    50% {
        transform: translate(-5px, 5px) rotate(-3deg);
    }
    75% {
        transform: translate(8px, 8px) rotate(4deg);
    }
}

@keyframes lensGlow {
    0%, 100% {
        box-shadow: 
            inset 0 4px 12px rgba(59, 130, 246, 0.2),
            inset 0 -4px 8px rgba(37, 99, 235, 0.1),
            0 8px 24px rgba(59, 130, 246, 0.3),
            0 0 40px rgba(59, 130, 246, 0.15);
    }
    50% {
        box-shadow: 
            inset 0 4px 16px rgba(59, 130, 246, 0.3),
            inset 0 -4px 12px rgba(37, 99, 235, 0.2),
            0 12px 32px rgba(59, 130, 246, 0.4),
            0 0 60px rgba(59, 130, 246, 0.25);
    }
}

@keyframes lensReflection {
    0% {
        transform: translate(-100%, -100%) rotate(0deg);
    }
    100% {
        transform: translate(100%, 100%) rotate(0deg);
    }
}

@keyframes rimPulse {
    0%, 100% {
        transform: scale(1);
        opacity: 0.8;
    }
    50% {
        transform: scale(1.05);
        opacity: 1;
    }
}

@keyframes shineFloat {
    0%, 100% {
        opacity: 0.8;
        transform: translate(0, 0) scale(1);
    }
    50% {
        opacity: 1;
        transform: translate(3px, 3px) scale(1.1);
    }
}

@keyframes sparkleFloat {
    0%, 100% {
        opacity: 0;
        transform: scale(0) translateY(0);
    }
    50% {
        opacity: 1;
        transform: scale(1) translateY(-10px);
    }
}
.loader-text {
    font-size: 1.5rem;
    font-weight: 700;
    background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    margin-bottom: 1rem;
    animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.6; }
}

.loader-subtext {
    font-size: 0.875rem;
    color: #6b7280;
    margin-bottom: 1.5rem;
}

.loader-progress {
    width: 100%;
    height: 4px;
    background: #e5e7eb;
    border-radius: 2px;
    overflow: hidden;
    position: relative;
}

.loader-progress-bar {
    height: 100%;
    background: linear-gradient(90deg, #3b82f6, #2563eb, #3b82f6);
    background-size: 200% 100%;
    animation: progressSlide 1.5s ease-in-out infinite;
}

@keyframes progressSlide {
    0% { transform: translateX(-100%); }
    100% { transform: translateX(100%); }
}

/* Skeleton loading shimmer effect */
@keyframes shimmer {
    0% { background-position: -468px 0; }
    100% { background-position: 468px 0; }
}

.skeleton {
    animation: shimmer 1.5s infinite;
    background: linear-gradient(to right, #f0f0f0 8%, #e0e0e0 18%, #f0f0f0 33%);
    background-size: 800px 100px;
}

        @media (min-width: 640px) {
            .metric-label { font-size: 0.9375rem; }
        }
        
       .metric-value { 
    /* 1. Start with a smaller font size for mobile */
    font-size: 1.75rem; 
    font-weight: 800;
    
    /* 2. Add a bit of line-height in case the number needs to wrap */
    line-height: 1.2; 


    overflow-wrap: break-word;

    background: linear-gradient(135deg, var(--card-color-start), var(--card-color-end));
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
}

@media (min-width: 640px) {
    /* 4. Now, increase the size for tablets and desktops */
    .metric-value { font-size: 2.75rem; }
}
        
        /* Modern Section Card */
        .section-card {
            background: white;
            border-radius: 20px;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
            overflow: hidden;
            transition: box-shadow 0.3s ease;
        }
        
        .section-card:hover {
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
        }
        
        .section-header {
            background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);
            color: white;
            padding: 1.25rem 1.5rem;
            font-weight: 700;
            font-size: 1.125rem;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
        }
        
        @media (min-width: 640px) {
            .section-header {
                font-size: 1.25rem;
                padding: 1.5rem 2rem;
            }
        }
        
        /* Calendar Styles - MODIFIED FOR RESPONSIVENESS */
        .calendar-wrapper {
            overflow-x: auto;
            -webkit-overflow-scrolling: touch;
            scrollbar-width: thin;
            scrollbar-color: #cbd5e1 #f1f5f9;
        }
        
        .calendar-wrapper::-webkit-scrollbar {
            height: 8px;
        }
        
        .calendar-wrapper::-webkit-scrollbar-track {
            background: #f1f5f9;
        }
        
        .calendar-wrapper::-webkit-scrollbar-thumb {
            background: #cbd5e1;
            border-radius: 4px;
        }
        
        .custom-calendar-table {
            min-width: 100%;
            width: max-content;
            border-collapse: separate;
            border-spacing: 0;
        }
        
        @media (min-width: 1024px) {
            .calendar-wrapper {
                overflow-x: hidden;
            }
            .custom-calendar-table {
                width: 100%;
            }
            .custom-calendar-table th, .custom-calendar-table td {
                min-width: 1px;
                width: calc(100% / 7);
            }
        }

        .custom-calendar-table th {
            background: linear-gradient(135deg, #f1f5f9 0%, #e2e8f0 100%);
            color: #1e293b;
            font-weight: 700;
            text-transform: uppercase;
            font-size: 0.75rem;
            letter-spacing: 0.05em;
            padding: 1rem;
            text-align: center;
            border-bottom: 2px solid #cbd5e1;
            min-width: 100px;
        }
        
        @media (min-width: 640px) {
            .custom-calendar-table th {
                min-width: 120px;
                font-size: 0.875rem;
            }
        }
        
        .custom-calendar-table td {
            min-width: 100px;
            vertical-align: top;
            border: 1px solid #e5e7eb;
            padding: 0.75rem;
            min-height: 100px;
            font-size: 0.875rem;
            background: white;
            transition: all 0.2s ease;
            position: relative;
        }
        
        @media (min-width: 640px) {
            .custom-calendar-table td {
                min-width: 120px;
                padding: 1rem;
                min-height: 120px;
            }
        }
        
        @media (min-width: 1024px) {
            .custom-calendar-table td {
                height: 140px;
            }
        }
        
        .custom-calendar-table td:hover {
            background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
            box-shadow: inset 0 0 0 2px #3b82f6;
            cursor: pointer;
        }
        
        /* Modern Button Styles */
        .btn-primary {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            padding: 0.75rem 1.5rem;
            border-radius: 12px;
            font-weight: 600;
            font-size: 0.9375rem;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
            border: none;
            cursor: pointer;
        }
        
        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(59, 130, 246, 0.4);
        }
        
        .btn-primary:active {
            transform: translateY(0);
        }
        
        .btn-secondary {
            background: white;
            color: #3b82f6;
            padding: 0.625rem 1.25rem;
            border-radius: 12px;
            font-weight: 600;
            font-size: 0.875rem;
            transition: all 0.3s ease;
            border: 2px solid #3b82f6;
            cursor: pointer;
        }
        
        .btn-secondary:hover {
            background: #3b82f6;
            color: white;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
        }
        
        .btn-secondary.active {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
        }
        
        .btn-success {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            padding: 0.875rem 2rem;
            border-radius: 12px;
            font-weight: 700;
            font-size: 1rem;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
            border: none;
            cursor: pointer;
        }
        
        .btn-success:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(16, 185, 129, 0.4);
        }
        
        .btn-success.loading {
            pointer-events: none;
            opacity: 0.8;
            position: relative;
        }
        
        .btn-success.loading::after {
            content: "";
            display: inline-block;
            width: 1.25rem;
            height: 1.25rem;
            border: 3px solid rgba(255, 255, 255, 0.4);
            border-top-color: #ffffff;
            border-radius: 50%;
            animation: btn-spin 0.8s linear infinite;
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
        }
        
        @keyframes btn-spin {
            to { transform: translate(-50%, -50%) rotate(360deg); }
        }
        
        .btn-success:active {
            transform: translateY(0);
        }
        
        .btn-toggle {
            background: #e5e7eb;
            color: #374151;
            padding: 0.625rem 1.25rem;
            border-radius: 12px;
            font-weight: 600;
            font-size: 0.875rem;
            transition: all 0.3s ease;
            border: none;
            cursor: pointer;
        }
        
        .btn-toggle.active {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
        }
        
        .btn-toggle:hover:not(.active) {
            background: #d1d5db;
        }
        
        /* Chart Container */
        .chart-container {
            height: 300px;
            position: relative;
            padding: 1rem;
        }
        @media (min-width: 640px) {
            .chart-container { 
                height: 350px;
                padding: 1.5rem;
            }
        }
        @media (min-width: 1024px) {
            .chart-container { 
                height: 450px;
                padding: 2rem;
            }
        }
        
        /* Button Groups */
        .button-group {
            display: flex;
            flex-wrap: wrap;
            gap: 0.75rem;
            justify-content: center;
        }
        @media (min-width: 768px) {
            .button-group { 
                justify-content: flex-start;
                gap: 1rem;
            }
        }

        /* Modern Modal */
        .modal-backdrop {
            backdrop-filter: blur(4px);
            background: rgba(0, 0, 0, 0.6);
        }
        
        .modal-content {
            max-height: 90vh;
            overflow-y: auto;
            animation: modalSlideIn 0.3s ease;
        }
        
        @keyframes modalSlideIn {
            from {
                opacity: 0;
                transform: translateY(-20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        .modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding-bottom: 1rem;
            border-bottom: 2px solid #e5e7eb;
            margin-bottom: 1.5rem;
        }
        
        .modal-title {
            font-size: 1.25rem;
            font-weight: 700;
            color: #1e293b;
            background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }
        
        @media (min-width: 640px) {
            .modal-title { font-size: 1.5rem; }
        }
        
        .modal-close {
            width: 36px;
            height: 36px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 8px;
            color: #9ca3af;
            transition: all 0.2s ease;
            cursor: pointer;
        }
        
        .modal-close:hover {
            background: #f3f4f6;
            color: #374151;
        }
        
        /* Code Block Styling */
        .code-block {
            background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
            color: #e2e8f0;
            padding: 1.5rem;
            border-radius: 12px;
            overflow-x: auto;
            font-family: 'Monaco', 'Consolas', monospace;
            font-size: 0.8125rem;
            line-height: 1.6;
            box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.3);
        }
        
        @media (min-width: 640px) {
            .code-block { 
                font-size: 0.875rem;
                padding: 2rem;
            }
        }
        
        /* Welcome Message */
        .welcome-message {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
            border-radius: 12px;
            color: #1e40af;
            font-weight: 600;
            box-shadow: 0 2px 8px rgba(59, 130, 246, 0.15);
        }
        
        /* Divider */
        .divider {
            height: 2px;
            background: linear-gradient(90deg, transparent, #e5e7eb, transparent);
            margin: 3rem 0;
        }
        
        @media (min-width: 640px) {
            .divider { margin: 4rem 0; }
        }
        
        /* Animations */
        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        .animate-fade-in {
            animation: fadeInUp 0.6s ease forwards;
        }
        
        .animate-delay-1 { animation-delay: 0.1s; opacity: 0; }
        .animate-delay-2 { animation-delay: 0.2s; opacity: 0; }
        .animate-delay-3 { animation-delay: 0.3s; opacity: 0; }
        .animate-delay-4 { animation-delay: 0.4s; opacity: 0; }


        /* ===== vvv ADD THESE NEW STYLES vvv ===== */

/* New GridView Table Styles */
.grid-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.875rem; /* 14px */
}

.grid-table th {
    background: linear-gradient(135deg, #f1f5f9 0%, #e2e8f0 100%);
    color: #1e293b;
    font-weight: 700;
    text-transform: uppercase;
    font-size: 0.75rem; /* 12px */
    letter-spacing: 0.05em;
    padding: 1rem 1.5rem; /* 16px 24px */
    text-align: left;
    border-bottom: 2px solid #cbd5e1;
}

.grid-table td {
    padding: 1rem 1.5rem; /* 16px 24px */
    border-bottom: 1px solid #e5e7eb;
    color: #374151;
    vertical-align: middle;
}

.grid-table tr:last-child td {
    border-bottom: none;
}

.grid-table tr:hover td {
    background: #f8fafc;
}

/* "In Stock" Label Style */
.status-in-stock {
    color: #059669; /* Green */
    font-weight: 700;
    font-size: 0.875rem; /* 14px */
}

/* "Restock Now!" Flag/Label Style */
.status-restock-now {
    color: #dc2626; /* Red */
    font-weight: 700;
    font-size: 0.875rem; /* 14px */
    animation: pulse-red 1.5s ease-in-out infinite;
}

/* Optional: Animation for the restock flag */
@keyframes pulse-red {
    0%, 100% {
        opacity: 1;
    }
    50% {
        opacity: 0.7;
    }
}

/* vvv ADD THIS STYLE FOR THE DROPDOWN vvv */
.modern-dropdown {
    width: 100%;
    max-width: 300px; /* Or leave at 100% */
    padding: 0.75rem 1rem;
    border: 1px solid #d1d5db; /* gray-300 */
    border-radius: 12px;
    background-color: white;
    font-size: 0.875rem; /* 14px */
    color: #374151; /* gray-700 */
    box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
}
.modern-dropdown:focus {
    border-color: #3b82f6; 
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2);
    outline: none;
}


    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-6 sm:px-6 sm:py-8 lg:px-8 lg:py-10">
        
        <header class="text-center mb-8 sm:mb-10 lg:mb-12 animate-fade-in">
            <h1 class="page-title font-black mb-3">Admin Dashboard</h1>
            <div class="welcome-message">
                <i class="fas fa-user-shield"></i>
                <asp:Label ID="lblWelcome" runat="server" />
            </div>
        </header>

        <section class="mb-8 sm:mb-10 lg:mb-12">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6 lg:gap-8">
                
                <div class="metric-card card-blue animate-fade-in animate-delay-1">
                    <div class="metric-icon">
                        <i class="fas fa-users"></i>
                    </div>
                    <div class="metric-label text-center">Total Clients</div>
                    <div class="text-center">
                        <asp:Label ID="lblTotalClients" runat="server" CssClass="metric-value"></asp:Label>
                    </div>
                </div>
                
                <div class="metric-card card-purple animate-fade-in animate-delay-2">
                    <div class="metric-icon">
                        <i class="fas fa-user-tie"></i>
                    </div>
                    <div class="metric-label text-center">Total Employees</div>
                    <div class="text-center">
                        <asp:Label ID="lblTotalWorkers" runat="server" CssClass="metric-value"></asp:Label>
                    </div>
                </div>
                
                <div class="metric-card card-green animate-fade-in animate-delay-3">
                    <div class="metric-icon">
                        <i class="fas fa-dollar-sign"></i>
                    </div>
                    <div class="metric-label text-center">Today's Sales</div>
                    <div class="text-center">
                        <asp:Label ID="lblTodaySales" runat="server" CssClass="metric-value"></asp:Label>
                    </div>
                </div>
                
                <div class="metric-card card-orange animate-fade-in animate-delay-4">
                    <div class="metric-icon">
                        <i class="fas fa-chart-line"></i>
                    </div>
                    <div class="metric-label text-center">This Month's Sales</div>
                    <div class="text-center">
                        <asp:Label ID="lblMonthSales" runat="server" CssClass="metric-value"></asp:Label>
                    </div>
                </div>
                
            </div>
        </section>

        <section class="mb-8 sm:mb-10 lg:mb-12">
            <div class="section-card">
                <div class="section-header">
                    <i class="fas fa-calendar-alt text-xl"></i>
                    <span>Weekly Booking Calendar</span>
                </div>
                <div class="calendar-wrapper p-4 sm:p-6">
                    <asp:Table ID="tblCalendar" runat="server" CssClass="custom-calendar-table" />
                </div>
            </div>
        </section>

        <div class="fixed inset-0 modal-backdrop hidden items-center justify-center z-[1000] p-4" id="bookingDetailsModal">
            <div class="bg-white rounded-2xl shadow-2xl w-full max-w-2xl modal-content p-6 sm:p-8">
                <div class="modal-header">
                    <h5 class="modal-title">Booking Details</h5>
                    <button type="button" class="modal-close" onclick="hideBookingModal()">
                        <i class="fas fa-times text-xl"></i>
                    </button>
                </div>
                <div class="text-gray-700 text-sm sm:text-base" id="bookingDetailsContent"></div>
            </div>
        </div>
        
        <script>
            const bookingModal = document.getElementById('bookingDetailsModal');
            function showBookingModal(details) {
                document.getElementById('bookingDetailsContent').innerHTML = details;
                bookingModal.classList.remove('hidden');
                bookingModal.classList.add('flex');
                document.body.style.overflow = 'hidden';
            }
            function hideBookingModal() {
                bookingModal.classList.add('hidden');
                bookingModal.classList.remove('flex');
                document.body.style.overflow = '';
            }
            bookingModal.addEventListener('click', (e) => {
                if (e.target === bookingModal) hideBookingModal();
            });
        </script>


<section class="mb-8 sm:mb-10 lg:mb-12">
    <div class="section-card">
        <div class="section-header">
            <i class="fas fa-boxes text-xl"></i>
            <span>Live Inventory Snapshot</span>
        </div>

        <%-- UpdatePanel to hold the grid --%>
        <asp:UpdatePanel ID="upInventory" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                
                <div class="p-4 sm:p-6 border-b border-gray-200">
                    <label for="<%= ddlCategoryFilter.ClientID %>" class="block text-sm font-medium text-gray-700 mb-2">
                        Filter by Category:
                    </label>
                    <asp:DropDownList ID="ddlCategoryFilter" runat="server" 
                        CssClass="modern-dropdown" 
                        AutoPostBack="true" 
                        OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged" 
                        AppendDataBoundItems="true">
                    </asp:DropDownList>
                </div>
                <div class="overflow-x-auto">
                    <%-- GridView to display inventory --%>
                    <asp:GridView ID="gvInventory" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="grid-table"
                        GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Item Name" />
                           <asp:TemplateField HeaderText="Category">
    <ItemTemplate>
        <asp:Label ID="lblCategory" runat="server" 
            Text='<%# Eval("Type").ToString() == "Safety Gear" ? "Gear" : Eval("Type") %>' />
    </ItemTemplate>
</asp:TemplateField>
                            <asp:BoundField DataField="LiveQuantity" HeaderText="Live Stock" 
                                ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="SnapshotQuantity" HeaderText="Starting Snapshot" 
                                ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" 
                                NullDisplayText="N/A" Visible="false" />
                            <asp:TemplateField HeaderText="Status" HeaderStyle-HorizontalAlign="Center" 
                                ItemStyle-HorizontalAlign="Center" ItemStyle-Width="200px">
                                <ItemTemplate>
                                    <asp:Label ID="lblStatus" runat="server"
                                        Text="✓ In Stock"
                                        CssClass="status-in-stock"
                                        Visible='<%# !Convert.ToBoolean(Eval("RestockFlag")) %>' />
                                    <asp:Label ID="lblRestockFlag" runat="server"
                                        Text="Restock Now!"
                                        CssClass="status-restock-now"
                                        Visible='<%# Convert.ToBoolean(Eval("RestockFlag")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="p-6 text-center text-gray-500">
                                No inventory items found.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</section>

        <section class="mb-8 sm:mb-10 lg:mb-12">
            <div class="section-card">
                <div class="p-6 sm:p-8">
                    
                    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
                        <h3 class="text-2xl sm:text-3xl font-bold text-gray-800">
                            <i class="fas fa-chart-line text-blue-600 mr-2"></i>
                            Sales Overview
                        </h3>
                        <div class="flex gap-2">
                             <button type="button" onclick="setChartType('line')" class="btn-toggle  active">
                                 <i class="fas fa-chart-line mr-1"></i> Line
                             </button>
                            <button type="button" onclick="setChartType('bar')" class="btn-toggle">
                                <i class="fas fa-chart-bar mr-1"></i> Bar
                            </button>
                        </div>
                    </div>
                    
                    <div class="button-group mb-6">
                        <button type="button" onclick="loadSalesData('daily')" class="btn-secondary">
                            <i class="fas fa-calendar-day mr-1"></i> Daily
                        </button>
                        <button type="button" onclick="loadSalesData('weekly')" class="btn-secondary">
                            <i class="fas fa-calendar-week mr-1"></i> Weekly
                        </button>
                        <button type="button" onclick="loadSalesData('monthly')" class="btn-secondary active">
                            <i class="fas fa-calendar-alt mr-1"></i> Monthly
                        </button>
                        <button type="button" onclick="loadSalesData('yearly')" class="btn-secondary">
                            <i class="fas fa-calendar mr-1"></i> Yearly
                        </button>
                    </div>
                    
                    <div class="chart-container">
                        <canvas id="salesChart"></canvas>
                    </div>
                    
                </div>
            </div>
        </section>

        <div class="divider"></div>
        
<span id="blockchainSection"></span>
<section class="min-h-[200px]">
    <div class="section-card">
        <div class="p-6 sm:p-8 text-center">
            <asp:UpdatePanel ID="upBlockchain" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="mb-6">
                        <i class="fas fa-lock text-6xl text-blue-600 mb-4"></i>
                        <h3 class="text-2xl sm:text-3xl font-bold text-gray-800 mb-2">
                            Blockchain Sales Transparency
                        </h3>
                        <p class="text-gray-600 text-sm sm:text-base max-w-2xl mx-auto">
                            Verify the integrity and authenticity of sales records using blockchain technology
                        </p>
                    </div>
                    
                    <div class="my-6">
                        <button type="button" class="btn-success" onclick="triggerVerification(); return false;">
                            🔍 Verify Blockchain Integrity
                        </button>
                        
                        <asp:Button ID="btnVerifyBlockchain" runat="server"
                            OnClick="btnVerifyBlockchain_Click"
                            CausesValidation="false"
                            Style="display:none;" />
                    </div>
                    
                    <asp:Label ID="lblVerificationResult" runat="server"
                        CssClass="text-lg sm:text-xl font-bold mt-6 block" />
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnVerifyBlockchain" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
</section>

<script type="text/javascript">
    var isProcessing = false;

    function triggerVerification() {
        console.log('triggerVerification called');

        if (isProcessing) {
            console.log('Already processing, ignoring click');
            return false;
        }

        isProcessing = true;
        showFullPageLoader();

        // Small delay to ensure loader is visible before postback
        setTimeout(function () {
            var btn = document.getElementById('<%= btnVerifyBlockchain.ClientID %>');
            if (btn) {
                console.log('Clicking hidden button');
                btn.click();
            } else {
                console.error('Hidden button not found');
                hideFullPageLoader();
            }
        }, 50);

        return false;
    }

    function showFullPageLoader() {
        console.log('showFullPageLoader called');
        var loader = document.getElementById('fullpageLoader');
        if (loader) {
            loader.classList.add('active');
            document.body.style.overflow = 'hidden';
            console.log('Full page loader activated');
        } else {
            console.error('Loader element not found!');
        }
    }

    function hideFullPageLoader() {
        console.log('hideFullPageLoader called');
        var loader = document.getElementById('fullpageLoader');
        if (loader) {
            loader.classList.remove('active');
            document.body.style.overflow = '';
            console.log('Full page loader deactivated');
        }
        isProcessing = false;
    }

    // Initialize PageRequestManager event handlers
    function initBlockchainHandlers() {
        console.log('Initializing blockchain handlers...');

        if (typeof Sys === 'undefined' || !Sys.WebForms || !Sys.WebForms.PageRequestManager) {
            console.log('Sys not ready, retrying...');
            setTimeout(initBlockchainHandlers, 100);
            return;
        }

        try {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            console.log('PageRequestManager instance obtained');

            prm.add_initializeRequest(function (sender, args) {
                console.log('initializeRequest event fired');
                var postBackElement = args.get_postBackElement();
                console.log('PostBack element:', postBackElement ? postBackElement.id : 'unknown');
                
                // Don't cancel if it's our verification button
                if (postBackElement && postBackElement.id.indexOf('btnVerifyBlockchain') === -1 && isProcessing) {
                    console.log('Different request while processing, canceling');
                    args.set_cancel(true);
                }
            });

            prm.add_beginRequest(function (sender, args) {
                console.log('beginRequest event fired');
            });

            prm.add_endRequest(function (sender, args) {
                console.log('endRequest event fired');

                // Show result for 2 seconds before hiding loader
                setTimeout(function () {
                    hideFullPageLoader();
                    
                    // Scroll to result if it exists
                    var resultLabel = document.getElementById('<%= lblVerificationResult.ClientID %>');
                    if (resultLabel && resultLabel.innerText.trim() !== '') {
                        resultLabel.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    }
                }, 2000);

                if (args.get_error()) {
                    var errorMessage = args.get_error().message;
                    console.error('Error occurred:', errorMessage);
                    alert('An error occurred: ' + errorMessage);
                    args.set_errorHandled(true);
                } else {
                    console.log('Request completed successfully');
                }
            });

            console.log('Blockchain handlers initialized successfully');
        } catch (e) {
            console.error('Error initializing handlers:', e);
            hideFullPageLoader();
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initBlockchainHandlers);
    } else {
        initBlockchainHandlers();
    }

    // Failsafe - force hide loader after 15 seconds
    window.addEventListener('load', function () {
        setTimeout(function () {
            if (isProcessing) {
                console.warn('Failsafe triggered - forcing loader hide after 15s timeout');
                hideFullPageLoader();
            }
        }, 15000);
    });
</script>


        <div class="fixed inset-0 modal-backdrop hidden items-center justify-center z-[1000] p-4" id="jsonModal">
            <div class="bg-white rounded-2xl shadow-2xl w-full max-w-4xl modal-content p-6 sm:p-8">
                <div class="modal-header">
                    <h5 class="modal-title">Transaction Details</h5>
                    <button type="button" class="modal-close" onclick="closeJsonModal()">
                        <i class="fas fa-times text-xl"></i>
                    </button>
                </div>
                <div class="text-gray-700">
                    <pre id="jsonModalBody" class="code-block"></pre>
                </div>
            </div>
        </div>
        
        <script>
            const jsonModal = document.getElementById('jsonModal');
            function openJsonModal(jsonStr) {
                try {
                    const parsedJson = JSON.parse(jsonStr);
                    document.getElementById('jsonModalBody').textContent = JSON.stringify(parsedJson, null, 2);
                } catch (e) {
                    document.getElementById('jsonModalBody').textContent = 'Invalid JSON data.';
                }
                jsonModal.classList.remove('hidden');
                jsonModal.classList.add('flex');
                document.body.style.overflow = 'hidden';
            }
            function closeJsonModal() {
                jsonModal.classList.add('hidden');
                jsonModal.classList.remove('flex');
                document.body.style.overflow = '';
            }
            jsonModal.addEventListener('click', (e) => {
                if (e.target === jsonModal) closeJsonModal();
            });
        </script>

        <input type="hidden" id="salesDataJson" value='<%= salesDataJson %>' />

        <script src="https://cdn.jsdelivr.net/npm/chart.js@4"></script>
        <script>
            let chart, currentChartType = 'line';

            // Set Chart Type Function
            function setChartType(type) {
                currentChartType = type;
                const buttons = document.querySelectorAll('[onclick^="setChartType"]');
                buttons.forEach(btn => {
                    if (btn.textContent.toLowerCase().includes(type)) {
                        btn.classList.add('active');
                    } else {
                        btn.classList.remove('active');
                    }
                });
                if (window._lastRange) {
                    loadSalesData(window._lastRange);
                }
            }

            // Load Sales Data Function
            function loadSalesData(type) {
                window._lastRange = type;

                // Update button states
                const rangeButtons = document.querySelectorAll('[onclick^="loadSalesData"]');
                rangeButtons.forEach(btn => {
                    if (btn.textContent.toLowerCase().includes(type)) {
                        btn.classList.add('active');
                    } else {
                        btn.classList.remove('active');
                    }
                });

                fetch('Dashboard.aspx/GetSalesData', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json; charset=utf-8' },
                    body: JSON.stringify({ type })
                })
                    .then(response => response.json())
                    .then(result => updateChart(result.d))
                    .catch(error => console.error('Error loading sales data:', error));
            }

            // Update Chart Function
            function updateChart(salesData) {
                if (!salesData || !salesData.labels || !salesData.data) return;

                const ctx = document.getElementById('salesChart').getContext('2d');

                // Destroy existing chart
                if (chart) chart.destroy();

                // Create gradient
                const gradient = ctx.createLinearGradient(0, 0, 0, 400);
                gradient.addColorStop(0, 'rgba(59, 130, 246, 0.8)');
                gradient.addColorStop(1, 'rgba(37, 99, 235, 0.4)');

                // Create new chart
                chart = new Chart(ctx, {
                    type: currentChartType,
                    data: {
                        labels: salesData.labels,
                        datasets: [{
                            label: 'Sales (₱)',
                            data: salesData.data,
                            backgroundColor: currentChartType === 'line' ? 'rgba(59, 130, 246, 0.1)' : gradient,
                            borderColor: 'rgb(59, 130, 246)',
                            borderWidth: 3,
                            tension: currentChartType === 'line' ? 0.4 : 0,
                            fill: currentChartType === 'line',
                            pointRadius: currentChartType === 'line' ? 4 : 0,
                            pointHoverRadius: currentChartType === 'line' ? 6 : 0,
                            pointBackgroundColor: 'rgb(59, 130, 246)',
                            pointBorderColor: '#fff',
                            pointBorderWidth: 2,
                            pointHoverBackgroundColor: '#fff',
                            pointHoverBorderColor: 'rgb(59, 130, 246)',
                            pointHoverBorderWidth: 3
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        interaction: {
                            intersect: false,
                            mode: 'index'
                        },
                        plugins: {
                            legend: {
                                display: true,
                                position: 'top',
                                align: 'end',
                                labels: {
                                    font: {
                                        size: window.innerWidth < 640 ? 11 : 13,
                                        weight: '600'
                                    },
                                    padding: 15,
                                    usePointStyle: true,
                                    pointStyle: 'circle'
                                }
                            },
                            tooltip: {
                                enabled: true,
                                backgroundColor: 'rgba(15, 23, 42, 0.95)',
                                titleColor: '#fff',
                                bodyColor: '#e2e8f0',
                                borderColor: 'rgba(59, 130, 246, 0.5)',
                                borderWidth: 1,
                                padding: 12,
                                displayColors: true,
                                callbacks: {
                                    label: (context) => {
                                        return 'Sales: ₱' + Number(context.parsed.y).toLocaleString('en-PH', {
                                            minimumFractionDigits: 2,
                                            maximumFractionDigits: 2
                                        });
                                    }
                                }
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                grid: {
                                    color: 'rgba(0, 0, 0, 0.05)',
                                    drawBorder: false
                                },
                                ticks: {
                                    callback: (value) => '₱' + Number(value).toLocaleString('en-PH', {
                                        minimumFractionDigits: 0,
                                        maximumFractionDigits: 0
                                    }),
                                    font: {
                                        size: window.innerWidth < 640 ? 10 : 12,
                                        weight: '500'
                                    },
                                    color: '#64748b',
                                    padding: 8
                                }
                            },
                            x: {
                                grid: {
                                    display: false,
                                    drawBorder: false
                                },
                                ticks: {
                                    font: {
                                        size: window.innerWidth < 640 ? 10 : 12,
                                        weight: '500'
                                    },
                                    color: '#64748b',
                                    maxRotation: window.innerWidth < 640 ? 45 : 0,
                                    minRotation: window.innerWidth < 640 ? 45 : 0,
                                    padding: 8
                                }
                            }
                        }
                    }
                });
            }

            // Initialize chart on page load
            document.addEventListener('DOMContentLoaded', () => {
                loadSalesData('weekly');
            });

            // Handle window resize
            let resizeTimer;
            window.addEventListener('resize', function () {
                clearTimeout(resizeTimer);
                resizeTimer = setTimeout(function () {
                    if (chart && window._lastRange) {
                        loadSalesData(window._lastRange);
                    }
                }, 250);
            });
        </script>
        
    </div>

    <div id="fullpageLoader" class="fullpage-loader">
    <div class="loader-content">
    <div class="loader-insect">
    <div class="search-sparkles">
        <div class="sparkle"></div>
        <div class="sparkle"></div>
        <div class="sparkle"></div>
        <div class="sparkle"></div>
    </div>
    <div class="magnifying-glass">
        <div class="glass-lens">
            <div class="glass-rim"></div>
            <div class="glass-shine"></div>
        </div>
        <div class="glass-handle"></div>
    </div>
</div>
        <h2 class="loader-text">🔐 Verifying Blockchain...</h2>
        <p class="loader-subtext">Checking integrity of sales records</p>
        <div class="loader-progress">
            <div class="loader-progress-bar"></div>
        </div>
    </div>
</div>

</asp:Content>