# RRC Management System
#### An enterprise-grade Inspection, Booking, and Operations Management Platform for Pest Control Services

## Overview
#### The RRC Management System is a comprehensive digital solution developed for R.R.C Termite and Pest Control to revolutionize their service delivery and operational efficiency. Built as a Capstone Project, this platform automates the complete business workflow—from customer inquiry and booking through inspection, service execution, payment processing, and audit tracking—while implementing enterprise-level security, intelligent resource allocation, and transparent record-keeping to support sustainable business growth.
## Key Highlights

- Intelligent Assignment - Automated round-robin inspector and technician scheduling
- Dynamic Quotations - Real-time pricing based on service area and requirements
- Enterprise Security - Multi-layered authentication with blockchain-backed transparency
- Smart Inventory - Service-aware equipment and chemical management
- Comprehensive Auditing - Full activity logging and reporting capabilities


## Features
### Automated Assignment and Scheduling
### Inspector Assignment

- Round-robin allocation system
- Availability-based filtering
- Geographic service location matching
- Workload balancing
- Real-time booking validation

### Technician Scheduling

- Morning and evening shift support
- Availability-based assignment
- Overbooking prevention
- Automatic workload distribution

### Booking and Quotation System

- Dynamic pricing calculation based on square meters
- Automatic travel expense inclusion
- Configurable miscellaneous fees
- Real-time availability validation
- Time slot conflict prevention

### Inspection and Reporting

- Integrated Inspector Report Module
- Findings and recommendations documentation
- Automatic report distribution to stakeholders
- Service insights tracking
- Post-assignment routing to Head Technician

### Inventory and Equipment Management

- Service-specific item filtering
- Automatic inventory deduction
- Chemical usage tracking
- Safety gear monitoring
- Equipment availability management


## Security Architecture
### Authentication and Access Control

- Single Sign-On - One active session per account
- Time-based OTP - Enhanced verification
- Google reCAPTCHA - Bot protection
- Session Management - Automatic logout on concurrent logins

### Device and Network Security

- Device Fingerprinting - Hardware identification
- Browser Fingerprinting - Client validation
- Smart Lockout - IP and device-based protection
- Account Security - Failed attempt monitoring

### Cryptography

- Password Hashing - Argon2id algorithm
- Data Encryption - AES-256 for sensitive data
- Blockchain Ledger - Immutable sales records


## User Roles
### ROOT ADMIN

- System administration and maintenance mode control

### SYSTEM ADMIN / SUPER ADMIN

- Full system control, CMS management, reports, and audit logs

### ADMIN

- Operations management, approvals, and monitoring

### HEAD TECHNICIAN

- Assignment oversight and inventory supervision

### INSPECTOR

- Inspection execution, quotation generation, and booking creation

### CLIENT

- Booking confirmation, payments, and contract access

### PUBLIC USER

- Inquiry submission without login requirement


## Technology Stack
### Backend

- Framework: ASP.NET Web Forms (C#)
- Database: Microsoft SQL Server
- Security: Argon2id, AES-256, TOTP (Google Authenticator, Microsoft Authenticator)

### Frontend

- Markup: HTML5, CSS3
- Styling: Tailwind CSS
- Scripts: JavaScript, jQuery, SweetAlert

### Additional Technologies

- Blockchain: Custom sales ledger implementation
- Authentication: reCAPTCHA, OTP verification
- Development Tools: Visual Studio, Git

## Installation and Setup
### Prerequisites

- Visual Studio 2019 or later
- .NET Framework 4.8
- Microsoft SQL Server 2016 or later
- IIS 10.0 or later (for deployment)

### Configuration Steps
1. Clone the repository
bashgit clone https://github.com/BasilanEJ/Capstone_Project.git
2. Set up configuration

- Create Web.config.secrets file
- Configure database connection strings
- Add API keys and encryption keys

3. Database setup

- Restore database backup
- Run migration scripts if provided
- Update connection string in Web.config

4. Build and run

- Open solution in Visual Studio
- Restore NuGet packages
- Build the solution
- Run on IIS Express or local IIS


## Usage Instructions
### For Academic Evaluation
#### This system is provided strictly for academic demonstration, evaluation, and research purposes.
### Authorized Use

- System execution for capstone evaluation, grading, and defense
- Code inspection for academic review and technical validation
- Local deployment for testing and assessment purposes

### Restrictions
#### The following actions are strictly prohibited without explicit written consent:

- Reuse or integration into other academic or commercial projects
- Redistribution of source code or compiled artifacts
- Modification and redeployment for institutional or commercial use
- Claiming authorship or partial ownership


#### Development Team
| Role | Name |
|------|------|
| Lead Programmer & System Architect | Edgar Joseph Basilan |
| Project Manager | Dan Adrian Austria |
| System Analyst | Zeth Nikolai Pagarigan |
| Quality Assurance | Dan Fredricck Dela Cruz |

### License and Copyright
© 2025 Edgar Joseph Basilan. All rights reserved.
This repository and all associated materials are the original intellectual property of the author(s).
Unauthorized reproduction, modification, redistribution, or use beyond academic evaluation constitutes a violation of intellectual property rights and academic integrity policies.

### Contact
For academic inquiries or evaluation purposes, please contact:


Email: [edgarjosephbasilan@gmail.com]


### Acknowledgments
Special thanks to our capstone advisers, thesis panelists, capstone coordinator, and academic evaluators for their guidance and support throughout this project.

Made with dedication for Academic Excellence
