
University Correspondence Portal

Author: Rohit Mahajan and Vishal Sonone

Project Overview

University Correspondence Portal is a web-based system built with .NET Framework 4.7.2 (ASP.NET MVC) and SQL Server to manage inward and outward correspondence for a university. The portal supports role-based access (Admin, Clerk, Staff), many-to-many staff assignment for letters, reporting, and email notifications.

Key Features

Manage Inward and Outward letters with full CRUD operations.

Role-based access control: Admin, Clerk, Staff — each role has relevant views and permissions.

Many-to-many assignment of staff to incoming letters using a LetterStaff (junction) table; aggregated display of StaffNames in views.

Department and Staff management (including IsActive flag for staff).

Modal forms for Create/Edit/View operations across letters and staff; many modals are popup-styled.

Outward letter number generation with tracking using OutwardLetterSerialTracker. On create, the previous tracker entry is deleted and replaced with the new tracker row to update serials.

Email notifications: Outward letter creation triggers emails to receiver staff.

Advanced filters & search on letter listing pages (FromDate, ToDate, Sender/Receiver Department, DeliveryMode, Priority, etc.) with Apply / Clear controls.

Staff-level letter view: staff see only letters assigned to them (Inward via LetterStaff) or sent by them (Outward via OutwardLetter.StaffID).

Reports page (Report.cshtml) for statistics, data analysis, and staff-assigned letter reporting.

Test data generation capability to insert large volumes of test rows for tables like InwardLetter, OutwardLetter, Staff, Department, LetterStaff, etc.

Database / Models (summary)

Important domain entities and relationships (high-level):

Department (PK: DepartmentID)

Staff (PK: StaffID) — fields include Name, IsActive, contact details, and navigation properties to letters/departments.

InwardLetter — fields: InwardNumber, OutwardNumber (if any), DeliveryMode, SenderName, SenderDepartment, ReceiverDepartment, Priority, ReceivedDate, and others. Assigned staff are linked via LetterStaff.

OutwardLetter — fields: OutwardNumber, ReceiverDepartment, DeliveryMode, Priority, StaffID (sender), etc. Sender name is obtained via OutwardLetter.Staff.Name.

LetterStaff (junction) — many-to-many between InwardLetter and Staff.

OutwardLetterSerialTracker — keeps the latest serial; on creation the previous entry is deleted and the new one inserted.

Note: Views and controller logic were updated to aggregate assigned staff into a StaffNames string using LINQ queries over LetterStaffs.

Important Views / UI Notes

InwardLetter and OutwardLetter pages use modal popup forms for Add/Edit/View. The InwardLetter modal includes department and staff dropdowns (with an Other option to allow custom input) and support for selecting multiple staff.

Staff.cshtml displays IsActive column and action buttons to set Active/Inactive (instead of an editable Status). The Status filter should not affect the search/display logic (filter UI only).

Letter.cshtml (Staff panel) shows both inward and outward letters filtered for the logged-in staff (assigned or sender). The table supports search and filters (SenderDepartment, ReceiverDepartment, FromDate, ToDate, Priority) with Apply/Clear.

Controllers / Important Actions (high level)

ClerkController (or relevant clerk-facing controllers):

Logic was updated to fetch assigned staff via LetterStaffs and aggregate names into a StaffNames string using LINQ.

Provides GetOutwardLetter action to fetch outward letter details for modal viewing.

Create/Edit actions for OutwardLetter ensure outward numbers are generated and stored both in OutwardLetter and OutwardLetterSerialTracker.

Setup & Installation (local development)

Prerequisites

Windows OS (recommended) or any environment that can run .NET Framework 4.7.2 apps.

Visual Studio 2019 / 2022 (with ASP.NET workload)

SQL Server (Express or full)

SMTP details for sending emails (or use a local SMTP server for development)

Database

Create a new SQL Server database (e.g., UniversityCorrespondencePortalDb).

Run EF migrations or execute the provided SQL schema (if included in Database folder).

If you prefer test data, use the provided insert scripts (or the test data generator module) to populate Staff, Department, InwardLetter, OutwardLetter, LetterStaff, etc.

Configuration

Update web.config connection string under ConnectionStrings:DefaultConnection to point to your SQL Server database.

Set SMTP settings in web.config or a secure secrets mechanism for email notifications.

Run

Open solution in Visual Studio, build, and run.

Login using seeded admin/clerk/staff accounts (or create new users via the Admin panel).

Suggested Folder Structure

/Controllers — ClerkController, StaffController, AdminController, LetterController, etc.

/Models — InwardLetter, OutwardLetter, Staff, Department, LetterStaff, OutwardLetterSerialTracker, etc.

/Views — InwardLetter, OutwardLetter, Staff, Report folders with corresponding .cshtml files.

/Scripts — JS for modal handling, date pickers, and filter logic.

/Migrations or /Database — EF migrations or SQL schema + seed/test data scripts.

Test Data & Sample SQL Inserts

This repository includes (or should include) a Database/seed.sql file with full INSERT statements aligned to the EF model naming conventions. If you want, I can generate bulk insert scripts for the following tables:

Department (sample departments)

Staff (sample staff with IsActive = true for newly created staff)

InwardLetter / OutwardLetter (realistic sample rows)

LetterStaff (assignments linking staff to inward letters)

Reporting & Analytics

Report.cshtml (Staff panel) offers statistics and reporting related to staff-assigned letters. It can be extended to export CSV/PDF and visual dashboards.

Known Implementation Details / Design Decisions

StaffNames: For quick display in lists and modals, assigned staff names are aggregated into a single StaffNames string using LINQ over the LetterStaff junction table.

IsActive default: New staff entries should default to IsActive = true.

Status filter design: Status filter is purely a UI control and should not alter core search logic unless explicitly applied.

TODO / Enhancements

Add unit tests for controllers and services.

Add integration tests for email sending (with mock SMTP) and outward number generation.

Improve reporting — add charts, export features, and scheduled report emails.

Add role-based login seeders and a simple admin onboarding guide.
