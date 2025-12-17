RRC Management System
Overview

The RRC Management System is an enterprise-grade Inspection, Booking, and Operations Management Platform developed as a Capstone Project. The system automates inspector and technician assignment, enforces real-time availability validation, supports dynamic service quotations, and ensures secure, transparent, and auditable business operations.

Built using ASP.NET Web Forms (C#) and Microsoft SQL Server, the system integrates advanced scheduling logic, role-based access control, cryptographic security mechanisms, and blockchain-backed sales transparency. It is designed to support end-to-end workflows—from public inquiry and inspection booking to service execution, reporting, payments, and audit tracking.

Instructions for Use

This system is provided strictly for academic demonstration, evaluation, and research purposes as part of a Capstone Project.

Authorized Use

The following are permitted:

System execution for capstone evaluation, grading, and defense

Code inspection for academic review and technical validation

Local deployment for testing and assessment purposes only

Intended users include:

Capstone advisers

Thesis panelists

Authorized academic evaluators

Restrictions

The following actions are strictly prohibited without explicit written consent from the author(s):

Reuse or integration of any system components into other academic or commercial projects

Redistribution of source code or compiled system artifacts

Modification and redeployment for institutional or commercial use

Claiming authorship or partial ownership of the system

Any usage beyond academic evaluation constitutes unauthorized use and may result in academic, institutional, and legal consequences.

Core System Logic
Automated Assignment & Scheduling

Inspector Assignment

Automatic Round-Robin allocation

Filtered by:

Inspector availability

Geographic service location

Existing workload

Booking is blocked if no inspector is available for the selected date, time slot, and location

Technician Assignment

Supports Morning and Evening shifts

Availability-based scheduling (no location constraint)

Prevents overbooking per shift

Head Technician Routing

Once approved, bookings are forwarded to the Head Technician

Includes assigned:

Inspector report

Equipment

Chemicals (service-specific only)

Booking & Quotation System

Dynamic quotation based on:

Square meters (SQM) per selected service

Automatic inclusion of:

Travel expenses (if applicable)

Miscellaneous fees (configurable)

Real-time validation of:

Inspector availability

Technician availability

Time slot conflicts

Inspection & Reporting

Integrated Inspector Report Module

Inspectors submit:

Findings

Recommendations

Service insights

Reports are automatically distributed to:

Admin

Client

Head Technician (post-assignment)

Inventory & Equipment Management

Service-aware item assignment:

Only equipment and chemicals relevant to the selected service are displayed

Prevents incorrect item usage

Inventory tracking includes:

Chemical usage

Safety gear

Equipment availability

Automatic deduction upon assignment

Security Architecture

The system implements enterprise-level security controls:

Authentication & Access

Single Sign-On (SSO)

One active session per account

Automatic logout on previously logged-in devices

Time-based One-Time Password (TOTP)

OTP verification

Google reCAPTCHA protection

Device & Network Security

Device fingerprinting

Browser fingerprinting

Device lockout

IP lockout

Account lockout after repeated failed attempts

Cryptography

Password hashing using Argon2id

Sensitive data encryption using AES-256

Blockchain Integration

Sales transactions are recorded using a blockchain-based ledger

Ensures:

Transparency

Immutability

Tamper-resistant sales records

Supports audit and financial verification

Audit & Reporting

Comprehensive Audit Logs across all modules

Role-based reporting access

Reports available for:

System Admin

Business Admin

Exportable summaries for:

Bookings

Inspections

Inventory usage

Payments

User activity

Content Management System (CMS)

Public-facing Inquiry Page CMS

Managed by System/Super Admin

Supports:

Announcements

Service content

Informational updates

User Roles & Dashboards
Role	Description
ROOT Admin	Manages system administrators and controls maintenance mode
System Admin / Super Admin	System-wide control, CMS management, reports, audit logs
Admin	Operations management, approvals, monitoring
Head Technician	Assignment oversight, inventory & equipment supervision
Inspector	Inspection execution, quotation generation, booking creation
Client	Booking confirmation, payments, contract access
Public User	Inquiry submission (no login required)
Technologies Used

Backend: ASP.NET Web Forms (C#)

Database: Microsoft SQL Server

Frontend: HTML5, CSS3, Tailwind

Client Scripts: JavaScript, jQuery, SweetAlert

Security: Argon2id, AES-256, TOTP, OTP, reCAPTCHA

Blockchain: Custom sales ledger implementation

Development Tools: Visual Studio, Git, GitHub

Credits

Developed as a Capstone Project by:

[Basilan, Edgar Joseph] – Lead Developer / System Architect

[Austria, Dan Adrian] – Project Manager

[Pagarigan, Zeth Nikolai] – System Analyst

[Dela Cruz, Dan Fredricck] – Quality Assurance

Ownership, Rights, and Legal Notice

This repository and all associated materials are the original intellectual property of the author(s).

© 2025 [Basilan, Edgar Joseph]. All rights reserved.

Unauthorized reproduction, modification, redistribution, or use beyond academic evaluation constitutes a violation of intellectual property rights and academic integrity policies.
