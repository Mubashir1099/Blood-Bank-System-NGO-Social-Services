# 🩸 Blood Bank Management System

> *"Saving Lives, One Drop at a Time"*

A complete, full‑stack **desktop application** for managing the end‑to‑end operations of a blood bank — donor and recipient records, live blood inventory, donation sessions, blood requests with approval workflow, NGO donation camps, volunteers, printable reports and visual analytics. Built with **C# (.NET 10) Windows Forms** on top of a **SQL Server** database, following a clean two‑layer architecture.

---

## 📑 Table of Contents

1. [Overview](#-overview)
2. [Features](#-features)
3. [Architecture](#️-architecture)
4. [Tech Stack](#️-tech-stack)
5. [Project Structure](#-project-structure)
6. [Database Schema](#️-database-schema)
7. [Prerequisites](#-prerequisites)
8. [Setup & Installation](#-setup--installation)
9. [Default Login](#-default-login)
10. [How to Use](#-how-to-use)
11. [Core Business Rules & Validations](#-core-business-rules--validations)
12. [Known Limitations & Future Enhancements](#️-known-limitations--future-enhancements)
13. [Project Info](#-project-info)

---

## 📋 Overview

The **Blood Bank Management System** digitizes every core process of a blood bank:

- Registering **donors** and **recipients**
- Recording **donations** and automatically updating **inventory**
- Tracking **stock levels per blood group** with expiry monitoring
- Handling **blood requests** from hospitals/recipients with an approve/reject workflow that automatically deducts stock
- Organizing **NGO blood donation camps** and assigning **volunteers**
- Generating **printable reports** and **visual charts** for decision‑making

The system is built around a secure login, a live dashboard with key statistics, and a sidebar‑driven navigation model — all wrapped in a custom red‑and‑white themed UI that matches the blood bank identity.

---

## ✨ Features

### 🔐 Authentication & Security
- Dedicated split‑panel **Login screen** with show/hide password toggle
- Session tied to the logged‑in user (name + role shown in the header)
- **Change Password** dialog (verifies current password, enforces a 6‑character minimum, confirms match)
- Logout with confirmation prompt

### 📊 Dashboard
- Personalized greeting (Good Morning / Afternoon / Evening) with live clock
- Clickable summary cards: Total Donors, Recipients, Blood Units, Pending Requests, Approved Requests, Active Camps, Volunteers
- Per‑blood‑group stock cards (A+, A‑, B+, B‑, O+, O‑, AB+, AB‑) color‑coded by stock level
- **Quick Action** buttons: Add Donor, Record Donation, New Blood Request, View Reports, View Charts
- Automatic **low‑stock warning banner** when any blood group falls below the safety threshold

### 🩸 Donor Management
- Add / Edit / Delete (soft delete) donor records
- Fields: Full Name, CNIC, Blood Group, Date of Birth (auto‑computed Age), Gender, Phone, Email, Address, Last Donation Date
- Live **search** (name, CNIC, phone) and **filter by blood group**
- Sortable, color‑coded data grid
- **Duplicate CNIC prevention**

### 💉 Blood Donation Recording
- Dedicated "Record Donation" form (donor, date, units, blood pressure, hemoglobin, notes)
- On save, the system automatically:
  1. Logs the session in the **Donations** history table
  2. Updates the donor's **Last Donation Date**
  3. Adds the donated units into **Blood Inventory** with a **42‑day expiry** calculated from the collection date

### 🏥 Blood Inventory Management
- Add / Edit / Delete inventory batches, each linked to its source donor
- Filter by blood group; columns show units, collection/expiry dates, days remaining, and status
- Expired batches are visually flagged
- Live **available‑units summary** per blood group, used everywhere else in the system (dashboard, requests, charts)

### 👥 Recipient Management
- Add / Edit / Delete recipient records
- Fields: Full Name, CNIC, Blood Group, DOB, Gender, Phone, Email, Address, Hospital Name, Medical Condition
- Search and **duplicate CNIC prevention**

### 📋 Blood Request Management
- Submit a request on behalf of a recipient: Blood Group, Units Required, Urgency (**Normal / High / Critical**), optional Required‑By date, Notes
- Filter the request list by status: All / Pending / Approved / Rejected
- **Approve** — automatically checks available stock, blocks approval if insufficient, deducts inventory (oldest stock first) and records who approved it and when
- **Reject** — single click with confirmation
- **Edit / Delete** allowed only while a request is still **Pending**
- Color‑coded urgency and status columns

### ⛺ NGO Camp Management
- Add / Edit / Delete (soft delete) blood donation camps: Name, Location, Date, Target Units, Organizer
- Search camps by name, location, or organizer
- Master/detail layout: select a camp to see its assigned volunteers
- **Assign / Remove Volunteers** to a camp via a check‑list dialog

### 🤝 Volunteer Management
- Add / Edit / Delete (soft delete) volunteers: Name, Phone, Email, Address, Skills
- Search volunteers, reusable across multiple camps

### 📊 Reports
- Tabbed report viewer covering: **Donors, Blood Inventory, Recipients, Blood Requests, Donation History**
- Each tab shows live record counts and is independently refreshable
- Built‑in **Print Preview / Print** for any tab (formatted as a fixed‑width text report)

### 📈 Charts & Analytics
- Hand‑drawn (GDI+, no third‑party charting library) **pie chart** of blood inventory distribution by group
- **Bar chart** of blood request status (Pending / Approved / Rejected)
- **Bar chart** of donor count per blood group
- One‑click refresh to pull the latest data

---

## 🏗️ Architecture

The solution follows a clean **two‑project, layered architecture**:

```
┌─────────────────────────────┐
│        App.WindowsApp       │   Presentation layer
│  (WinForms, Forms/Controls) │   — UI, validation, user interaction
└──────────────┬───────────────┘
               │ references
┌──────────────▼───────────────┐
│           App.Core            │   Business + Data Access layer
│  Models / Interfaces / Services│  — Domain models, service contracts,
│      (ADO.NET + SqlClient)     │    CRUD logic, SQL queries
└──────────────┬───────────────┘
               │ talks to
┌──────────────▼───────────────┐
│         SQL Server            │   BloodBankDB
│   (Database/BloodBankDB.sql)  │
└───────────────────────────────┘
```

- **App.Core** has no UI dependency — every service implements a corresponding interface (`IDonorService`, `IBloodInventoryService`, etc.), which makes the project unit‑test‑ and mock‑friendly.
- **App.WindowsApp** never talks to the database directly; it only calls into `App.Core` services.
- Each major module (Donors, Inventory, Recipients, Requests, Camps, Volunteers) is built as a `UserControl` that is swapped into the dashboard's content panel, while dialogs (Add/Edit forms, Reports, Charts) open as modal `Form`s.
- Several services expose an **async** variant (`GetAllDonorsAsync`, `GetAllCampsAsync`, `GetAllVolunteersAsync`) using `Task.Run`, demonstrating asynchronous data loading.

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Language | C# |
| Framework | .NET 10 (`net10.0-windows`) |
| UI | Windows Forms (WinForms) |
| Data Access | ADO.NET via `Microsoft.Data.SqlClient` 5.2.2 |
| Database | Microsoft SQL Server |
| Charts | Custom GDI+ drawing (`System.Drawing`) |
| Printing | `System.Drawing.Printing` (Print Preview / Print) |
| Async | `Task` / `async`‑`await` for non‑blocking data loads |

---

## 📂 Project Structure

```
BloodBankSystem/
└── FinalProject/
    ├── BloodBankSystem.slnx          # Solution file
    ├── App.Core/                     # Class library — business logic & data access
    │   ├── Models/                   # Donor, Recipient, BloodInventory, BloodRequest,
    │   │                              # Camp, Volunteer, User
    │   ├── Interfaces/                # Service contracts (IDonorService, etc.)
    │   └── Services/                  # Service implementations + DatabaseHelper
    ├── App.WindowsApp/                # WinForms executable — UI layer
    │   ├── Program.cs                 # Entry point (tests DB connection → Login → Dashboard)
    │   ├── App.config                 # Connection string configuration
    │   └── Forms/                     # LoginForm, MainDashboard, *ManagementControl,
    │                                   # *Form dialogs, ReportForm, ChartForm, etc.
    └── Database/
        └── BloodBankDB.sql            # Full schema + seed data script
```

---

## 🗄️ Database Schema

The script in `Database/BloodBankDB.sql` creates the `BloodBankDB` database with the following tables:

| Table | Key Columns | Purpose |
|---|---|---|
| **Donors** | `DonorID` (PK), CNIC (unique), BloodGroup, DOB, Gender, Phone, LastDonationDate, IsActive | Master list of blood donors |
| **BloodInventory** | `InventoryID` (PK), BloodGroup, Units, CollectionDate, ExpiryDate, `DonorID` (FK), Status | Current blood stock, batch by batch |
| **Recipients** | `RecipientID` (PK), CNIC (unique), BloodGroup, HospitalName, MedicalCondition | Patients/recipients who need blood |
| **BloodRequests** | `RequestID` (PK), `RecipientID` (FK), BloodGroup, UnitsRequired, UrgencyLevel, Status, ApprovedBy, ApprovalDate | Requests raised against recipients, with approval workflow |
| **Donations** | `DonationID` (PK), `DonorID` (FK), DonationDate, UnitsDonated, BloodPressure, Hemoglobin | Historical log of every donation session |
| **Users** | `UserID` (PK), Username (unique), PasswordHash, Role, IsActive | Application login accounts |
| **Camps** | `CampID` (PK), CampName, Location, CampDate, TargetUnits, OrganizerName, IsActive | NGO blood donation camps |
| **Volunteers** | `VolunteerID` (PK), FullName, Phone, Email, Skills, IsActive | Volunteers available for camps |
| **CampVolunteers** | `CampID` (FK), `VolunteerID` (FK) — composite PK | Many‑to‑many link between camps and volunteers |

The script also seeds a default **admin** user and a handful of sample donors/inventory rows so the app has data to show on first run.

---

## ⚙️ Prerequisites

Before running the project, make sure you have:

- **Windows 10/11** (WinForms only runs on Windows)
- **Visual Studio 2022/2026** with the *.NET Desktop Development* workload, **or** the **.NET 10 SDK** + any editor
- **SQL Server** — Express, Developer, Standard edition, or LocalDB (any edition that supports `CREATE DATABASE`)
- **SQL Server Management Studio (SSMS)** — recommended for running the setup script

---

## 🚀 Setup & Installation

**1. Extract / clone the project**

```bash
unzip BloodBankSystem.zip
cd BloodBankSystem/FinalProject
```

**2. Create the database**

Open `Database/BloodBankDB.sql` in SQL Server Management Studio and execute it. This will:
- Create the `BloodBankDB` database and all 9 tables
- Insert a default admin user and sample donor/inventory data

**3. Point the app at your SQL Server instance**

The connection string is currently hardcoded for a local default instance. Update it in **both** of these places to match your environment:

- `App.Core/Services/DatabaseHelper.cs`
  ```csharp
  public static string ConnectionString =
      @"Data Source=localhost;Initial Catalog=BloodBankDB;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;";
  ```
- `App.WindowsApp/App.config`
  ```xml
  <add name="BloodBankDB"
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=BloodBankDB;Integrated Security=True;Connect Timeout=30;" />
  ```

  Common `Data Source` values: `.\SQLEXPRESS`, `(local)`, `localhost`, or `YOUR-PC-NAME\SQLEXPRESS`.

**4. Restore & build**

```bash
dotnet restore
dotnet build
```
(or simply open `BloodBankSystem.slnx` in Visual Studio and build the solution)

**5. Run the application**

```bash
dotnet run --project App.WindowsApp
```
(or press **F5** in Visual Studio with `App.WindowsApp` set as the startup project)

On launch, the app first tests the database connection (`Program.cs`); if it fails, you'll get a clear error dialog instead of a crash.

---

## 🔑 Default Login

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin123` | Administrator |

> 💡 It's recommended to log in and use **Change Password** right after the first run.

---

## 🧭 How to Use

1. **Log in** with the default admin credentials.
2. Check the **Dashboard** for a system‑wide snapshot and low‑stock warnings.
3. Go to **Donors → Add Donor** to register donors, then use **Record Donation** to log a donation — inventory updates automatically.
4. Go to **Recipients → Add Recipient** to register a patient/recipient.
5. Go to **Blood Requests → New Request** to raise a request for a recipient; **Approve** it once stock is confirmed (this deducts inventory) or **Reject** it.
6. Use **NGO Camps** to plan a donation drive and **Assign Volunteers** to it from the **Volunteers** pool.
7. Open **Reports** for tabular, printable summaries, or **Charts** for a visual breakdown.
8. Use **Change Password** / **Logout** from the bottom of the sidebar when done.

---

## 📐 Core Business Rules & Validations

- **Blood shelf life:** every donated unit is given a **42‑day expiry** from its collection date.
- **FIFO stock usage:** when a request is approved, units are deducted from the **oldest non‑expired available batch first**.
- **Stock guard:** a request **cannot be approved** if the available units for that blood group are less than the units requested.
- **Low‑stock alert:** the dashboard flags any blood group with fewer than **3** available units.
- **Workflow lock:** Blood Requests can only be **edited, approved, or rejected while `Pending`**; once Approved/Rejected they're locked.
- **Duplicate prevention:** Donor and Recipient CNIC numbers must be unique.
- **Soft delete:** Donors, Camps, and Volunteers are **deactivated** (`IsActive = 0`) rather than physically removed, preserving history and foreign‑key integrity.
- **Password rules:** changing a password requires the correct current password and a new password of at least 6 characters that matches its confirmation.

---

## ⚠️ Known Limitations & Future Enhancements

- Passwords are currently stored and compared as **plain text** in the `Users` table (the `PasswordHash` column name anticipates this). A production version should hash passwords (e.g., with BCrypt/PBKDF2).
- The connection string is hardcoded for local development; a deployable version should read it from configuration/environment variables only.
- No role‑based access control yet — every logged‑in user currently has full access regardless of `Role`.
- Reports currently export via the Windows **Print/Print‑Preview** dialog only; PDF/Excel export could be added.
- Charts are custom‑drawn with GDI+; integrating a charting library (e.g., `System.Windows.Forms.DataVisualization` or a NuGet chart package) could add interactivity (tooltips, zoom).

---

## 🎓 Project Info

- **Course:** Advanced Programming (COSC‑5136)
- **Term:** Spring 2026
- **Type:** Desktop Application (C# / .NET / WinForms / SQL Server)
- **Developed by:** Mubashir

---

<p align="center">Made with care for a cause that saves lives. 🩸</p>
