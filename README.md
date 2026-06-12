# 🩸 Blood Bank Management System
### Final Year Project — Advanced Programming (.NET Framework)

---

## 📋 Project Overview

A complete Windows Forms application for managing a Blood Bank System.
Built with **C# .NET Framework 4.7.2** + **SQL Server** (ADO.NET).

---

## 🏗️ Project Structure

```
BloodBankSystem/
├── BloodBankSystem.sln              ← Open this in Visual Studio
│
├── App.Core/                        ← Business Logic Layer (Class Library)
│   ├── Models/
│   │   ├── Donor.cs
│   │   ├── BloodInventory.cs
│   │   ├── Recipient.cs
│   │   ├── BloodRequest.cs
│   │   └── User.cs
│   ├── Interfaces/
│   │   ├── IDonorService.cs
│   │   ├── IBloodInventoryService.cs
│   │   ├── IRecipientService.cs
│   │   ├── IBloodRequestService.cs
│   │   └── IUserService.cs
│   └── Services/
│       ├── DatabaseHelper.cs        ← Change connection string here
│       ├── DonorService.cs
│       ├── BloodInventoryService.cs
│       ├── RecipientService.cs
│       ├── BloodRequestService.cs
│       └── UserService.cs
│
├── App.WindowsApp/                  ← UI Layer (Windows Forms)
│   ├── Forms/
│   │   ├── LoginForm.cs             ← Login screen
│   │   ├── MainDashboard.cs         ← Main dashboard with sidebar
│   │   ├── DonorManagementControl.cs
│   │   ├── DonorForm.cs             ← Add/Edit donor popup
│   │   ├── BloodInventoryControl.cs
│   │   ├── BloodInventoryForm.cs    ← Add/Edit inventory popup
│   │   ├── RecipientManagementControl.cs
│   │   ├── RecipientForm.cs         ← Add/Edit recipient popup
│   │   ├── BloodRequestControl.cs
│   │   ├── BloodRequestForm.cs      ← Add/Edit request popup
│   │   ├── DonationForm.cs          ← Record donation popup
│   │   ├── ReportForm.cs            ← Reports with print support
│   │   └── ChangePasswordForm.cs
│   ├── Program.cs                   ← Entry point
│   └── App.config
│
└── Database/
    └── BloodBankDB.sql              ← Run this in SQL Server first!
```

---

## ⚙️ SETUP INSTRUCTIONS

### Step 1 — Database Setup

1. Open **SQL Server Management Studio (SSMS)**
2. Open `Database/BloodBankDB.sql`
3. Click **Execute (F5)**
4. This creates the database with all tables and sample data

### Step 2 — Update Connection String

Open `App.Core/Services/DatabaseHelper.cs` and change:

```csharp
public static string ConnectionString =
    @"Data Source=.\SQLEXPRESS;Initial Catalog=BloodBankDB;Integrated Security=True;";
```

**Common SQL Server instance names:**

| Your SQL Server | Change `Data Source` to |
|----------------|-------------------------|
| SQL Server Express (default) | `.\SQLEXPRESS` |
| SQL Server Developer | `(local)` or `localhost` |
| Named instance | `DESKTOP-ABC\SQLEXPRESS` |

### Step 3 — Open in Visual Studio

1. Open `BloodBankSystem.sln` in **Visual Studio 2019 or 2022**
2. Set `App.WindowsApp` as **Startup Project**
   - Right-click `App.WindowsApp` → Set as Startup Project
3. Press **F5** to build and run

---

## 🔑 Default Login

| Username | Password |
|----------|----------|
| `admin`  | `Admin123` |

---

## 📱 Features

| Module | Features |
|--------|----------|
| **Dashboard** | Live stats, blood group summary, low-stock alerts, quick actions |
| **Donors** | Add, Edit, Delete, Search, Filter by blood group |
| **Record Donation** | Records donation + automatically updates inventory |
| **Blood Inventory** | Add, Edit, Delete, expiry color coding |
| **Recipients** | Add, Edit, Delete, Search |
| **Blood Requests** | Create, Approve (with stock check), Reject, Delete |
| **Reports** | Tabbed view of all data + Print support |
| **Change Password** | Secure password update |

---

## 🗃️ Database Tables

- **Donors** — Donor personal and blood info
- **BloodInventory** — Blood stock with expiry tracking
- **Recipients** — Patient/recipient records
- **BloodRequests** — Request management with approval workflow
- **Donations** — Donation history log
- **Users** — Login credentials

---

## 💻 Technologies Used

- Language: **C# (.NET Framework 4.7.2)**
- UI: **Windows Forms (WinForms)**
- Database: **SQL Server / SQL Server Express**
- Data Access: **ADO.NET (SqlConnection, SqlCommand, SqlDataAdapter)**
- Architecture: **2-Layer (Core + UI)**

---

## ⚠️ Troubleshooting

**"Cannot connect to database"**
→ Check that SQL Server service is running
→ Verify the `Data Source` in `DatabaseHelper.cs`

**"Database 'BloodBankDB' does not exist"**
→ Run `BloodBankDB.sql` in SSMS first

**Build errors**
→ Make sure `App.WindowsApp` references `App.Core`
→ Right-click Solution → Restore NuGet Packages (if any)
