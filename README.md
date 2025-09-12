# 📚 University Correspondence Portal  

**Authors:** Rohit Mahajan & Vishal Sonone  

A **web-based correspondence management system** for universities, built with **ASP.NET MVC (.NET Framework 4.7.2)** and **SQL Server**.  
The portal supports **role-based access (Admin, Clerk, Staff)**, **inward/outward letter management**, **reporting**, **staff assignment**, and **email notifications**.  

---

## 🚀 Project Overview  

- Manage **Inward & Outward Letters** with full CRUD operations.  
- **Role-based Access Control** → Admin, Clerk, Staff.  
- **Many-to-many staff assignment** for inward letters via a `LetterStaff` junction table.  
- **Department & Staff Management** (with `IsActive` flag).  
- **Modal Popup Forms** for create/edit/view operations.  
- **Outward Letter Number Tracking** via `OutwardLetterSerialTracker`.  
- **Email Notifications** on outward letter creation.  
- **Advanced Filters & Search** for letter listings.  
- **Staff-specific Letter View** (only see assigned/sent letters).  
- **Reports & Analytics** with filtering and export capability.  
- **Bulk Test Data Generation** for departments, staff, and letters.  

---

## 🖥️ UI Design Overview  

The portal follows a **clean, modern UI design** with:  

- 🎨 **Bootstrap + jQuery** for responsive UI.  
- 📋 **Tables with search & filters** (From Date, To Date, Priority, Departments, etc.).  
- 🪟 **Popup Modals** for **Add/Edit/View** (instead of redirect pages).  
- ✅ **Buttons & Badges** for quick actions (Active/Inactive, Priority, Delivery Mode).  
- 📊 **Report Page** (`Report.cshtml`) with statistics, charts, and staff-wise analytics.  
- 🔎 **Staff Panel Letters** (`Letter.cshtml`) showing inward + outward letters with filters.  

### Example UI Elements  

| Feature | UI Design |
|---------|-----------|
| **Inward/Outward Letter List** | Paginated table with search, filter panel, action buttons (View/Edit). |
| **Letter Modals** | Popup modal forms with dropdowns (Department, Staff, DeliveryMode, Priority). |
| **Staff Management** | Table with `IsActive` column + buttons to toggle Active/Inactive. |
| **Reports Page** | Tabular stats + charts (future enhancement with exports). |
| **Filters** | Apply & Clear buttons, dropdowns, and date pickers for range filters. |

---

## 🗄️ Database / Models  

### Key Entities  

- **Department** → `DepartmentID (PK)`, Name.  
- **Staff** → `StaffID (PK)`, Name, Contact, `IsActive`.  
- **InwardLetter** → InwardNumber, OutwardNumber, DeliveryMode, Sender/Receiver Dept, Priority, ReceivedDate.  
- **OutwardLetter** → OutwardNumber, ReceiverDept, DeliveryMode, Priority, `StaffID (FK)` (sender).  
- **LetterStaff** → Junction table for many-to-many relation (InwardLetter ↔ Staff).  
- **OutwardLetterSerialTracker** → Maintains last generated serial.  

---

## 📂 Suggested Folder Structure  

/Controllers
ClerkController.cs
StaffController.cs
AdminController.cs
LetterController.cs

/Models
InwardLetter.cs
OutwardLetter.cs
Staff.cs
Department.cs
LetterStaff.cs
OutwardLetterSerialTracker.cs

/Views
/InwardLetter
/OutwardLetter
/Staff
/Report

/Scripts
filters.js
modals.js
datepickers.js

/Database
seed.sql
migrations/



## ⚙️ Setup & Installation  

### ✅ Prerequisites  
- Windows OS  
- Visual Studio 2019 / 2022 (ASP.NET workload)  
- SQL Server (Express or Full)  
- SMTP server (for email notifications)  

### 🛠️ Database Setup  
1. Create a new DB → `UniversityCorrespondencePortalDb`.  
2. Run EF migrations OR import provided schema (`/Database/seed.sql`).  
3. Insert test data using **insert scripts** or generator module.  

### 🔧 Configuration  
- Update **connection string** in `web.config`:  
  ```xml
  <connectionStrings>
    <add name="DefaultConnection" connectionString="Data Source=.;Initial Catalog=UniversityCorrespondencePortalDb;Integrated Security=True;" providerName="System.Data.SqlClient" />
  </connectionStrings>
Add SMTP settings for email sending.

▶️ Run
Open solution in Visual Studio.

Build & run.

Login with seeded Admin/Clerk/Staff accounts.

📊 Reporting & Analytics
Staff-wise assigned letters

Filters by Department, Priority, Date Range

Charts & Export (CSV/PDF planned)

Future Enhancements: Scheduled email reports

📝 Known Design Decisions
StaffNames aggregated into single string (via LINQ).

New staff default → IsActive = true.

Status filter is UI-only (does not affect core logic unless applied).

🛠️ TODO / Enhancements
✅ Add unit & integration tests

📈 Enhance reporting with charts & exports

✉️ Add scheduled report emails

👤 Role-based login seeders for quick onboarding
