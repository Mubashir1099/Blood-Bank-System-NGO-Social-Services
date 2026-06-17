-- ============================================
-- Blood Bank Management System - Database Setup
-- Run this script in SQL Server Management Studio
-- ============================================

CREATE DATABASE BloodBankDB;
GO

USE BloodBankDB;
GO

-- Donors Table
CREATE TABLE Donors (
    DonorID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    CNIC NVARCHAR(15) UNIQUE NOT NULL,
    BloodGroup NVARCHAR(5) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    PhoneNumber NVARCHAR(15) NOT NULL,
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    LastDonationDate DATE,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Blood Inventory Table
CREATE TABLE BloodInventory (
    InventoryID INT PRIMARY KEY IDENTITY(1,1),
    BloodGroup NVARCHAR(5) NOT NULL,
    Units INT NOT NULL DEFAULT 0,
    CollectionDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    DonorID INT FOREIGN KEY REFERENCES Donors(DonorID),
    Status NVARCHAR(20) DEFAULT 'Available',
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Recipients Table
CREATE TABLE Recipients (
    RecipientID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    CNIC NVARCHAR(15) UNIQUE NOT NULL,
    BloodGroup NVARCHAR(5) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    PhoneNumber NVARCHAR(15) NOT NULL,
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    HospitalName NVARCHAR(150),
    MedicalCondition NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Blood Requests Table
CREATE TABLE BloodRequests (
    RequestID INT PRIMARY KEY IDENTITY(1,1),
    RecipientID INT FOREIGN KEY REFERENCES Recipients(RecipientID),
    BloodGroup NVARCHAR(5) NOT NULL,
    UnitsRequired INT NOT NULL,
    UrgencyLevel NVARCHAR(20) NOT NULL,
    RequestDate DATE DEFAULT GETDATE(),
    RequiredByDate DATE,
    Status NVARCHAR(20) DEFAULT 'Pending',
    Notes NVARCHAR(500),
    ApprovedBy NVARCHAR(100),
    ApprovalDate DATE,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Donations Table (tracks actual donations)
CREATE TABLE Donations (
    DonationID INT PRIMARY KEY IDENTITY(1,1),
    DonorID INT FOREIGN KEY REFERENCES Donors(DonorID),
    DonationDate DATE NOT NULL,
    BloodGroup NVARCHAR(5) NOT NULL,
    UnitsDonated INT NOT NULL DEFAULT 1,
    BloodPressure NVARCHAR(20),
    Hemoglobin NVARCHAR(10),
    Notes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Users Table (for login)
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) DEFAULT 'Staff',
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Insert default admin user (Password: Admin123)
INSERT INTO Users (Username, PasswordHash, FullName, Role)
VALUES ('admin', 'Admin123', 'System Administrator', 'Admin');

-- Insert sample blood inventory
INSERT INTO Donors (FullName, CNIC, BloodGroup, DateOfBirth, Gender, PhoneNumber, Email, Address, LastDonationDate)
VALUES 
('Ali Hassan', '42101-1234567-1', 'A+', '1990-05-15', 'Male', '0300-1234567', 'ali@email.com', 'Karachi', '2025-01-10'),
('Sara Ahmed', '42201-2345678-2', 'B+', '1988-08-22', 'Female', '0311-2345678', 'sara@email.com', 'Lahore', '2025-02-15'),
('Usman Khan', '42301-3456789-3', 'O+', '1995-03-10', 'Male', '0321-3456789', 'usman@email.com', 'Islamabad', '2025-01-20'),
('Ayesha Malik', '42401-4567890-4', 'AB+', '1992-11-05', 'Female', '0333-4567890', 'ayesha@email.com', 'Faisalabad', NULL),
('Bilal Raza', '42501-5678901-5', 'O-', '1985-07-18', 'Male', '0345-5678901', 'bilal@email.com', 'Multan', '2025-03-05');

INSERT INTO BloodInventory (BloodGroup, Units, CollectionDate, ExpiryDate, DonorID, Status)
VALUES 
('A+', 5, '2025-04-01', '2025-05-31', 1, 'Available'),
('B+', 3, '2025-04-05', '2025-06-04', 2, 'Available'),
('O+', 8, '2025-04-10', '2025-06-09', 3, 'Available'),
('AB+', 2, '2025-04-15', '2025-06-14', 4, 'Available'),
('O-', 4, '2025-04-20', '2025-06-19', 5, 'Available');

GO

-- ============ ALTER FOR EXISTING DATABASES ============
-- Run these if you already created the DB before:
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Donations') AND name = 'BloodPressure')
    ALTER TABLE Donations ADD BloodPressure NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Donations') AND name = 'Hemoglobin')
    ALTER TABLE Donations ADD Hemoglobin NVARCHAR(20) NULL;
GO

-- NGO & Services Tables
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Camps]') AND type in (N'U'))
BEGIN
    CREATE TABLE Camps (
        CampID INT PRIMARY KEY IDENTITY(1,1),
        CampName NVARCHAR(150) NOT NULL,
        Location NVARCHAR(255) NOT NULL,
        CampDate DATE NOT NULL,
        TargetUnits INT NOT NULL DEFAULT 0,
        OrganizerName NVARCHAR(100) NOT NULL,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Volunteers]') AND type in (N'U'))
BEGIN
    CREATE TABLE Volunteers (
        VolunteerID INT PRIMARY KEY IDENTITY(1,1),
        FullName NVARCHAR(100) NOT NULL,
        PhoneNumber NVARCHAR(15) NOT NULL,
        Email NVARCHAR(100),
        Address NVARCHAR(255),
        Skills NVARCHAR(100),
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CampVolunteers]') AND type in (N'U'))
BEGIN
    CREATE TABLE CampVolunteers (
        CampID INT FOREIGN KEY REFERENCES Camps(CampID),
        VolunteerID INT FOREIGN KEY REFERENCES Volunteers(VolunteerID),
        PRIMARY KEY (CampID, VolunteerID)
    );
END
GO

PRINT 'Database setup complete!';
GO
