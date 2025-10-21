-- Simple Absence Management Tables
USE Gestion_Stock_QRCodee;
GO
select * from Logine where Username like '%abd%'
-- Create MOTIF_ABSENCE table (reference table for absence reasons)
CREATE TABLE MOTIF_ABSENCE (
    IdMotif INT IDENTITY(1,1) PRIMARY KEY,
    NomMotif NVARCHAR(100) NOT NULL UNIQUE,
    CodeMotif NVARCHAR(20) NOT NULL UNIQUE,
    CouleurBadge NVARCHAR(20) DEFAULT '#6c757d',
    IsActive BIT DEFAULT 1,
    DateCreation DATETIME DEFAULT GETDATE(),
    is_deleted BIT DEFAULT 0
);


-- Create ABSENCE_EMPLOYE table with foreign key to MOTIF_ABSENCE
CREATE TABLE ABSENCE_EMPLOYE (
    IdAbsence INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye INT NOT NULL,
    IdMotif INT NOT NULL,
    DateDebut DATE NOT NULL,
    DateFin DATE NOT NULL,
    Description NVARCHAR(500),
    CreePar INT NOT NULL,
    DateCreation DATETIME DEFAULT GETDATE(),
    is_deleted BIT DEFAULT 0,

    FOREIGN KEY (IdEmploye) REFERENCES EMPLOYE(IdEmploye),
    FOREIGN KEY (IdMotif) REFERENCES MOTIF_ABSENCE(IdMotif),
    CONSTRAINT CK_DateFin_Absence CHECK (DateFin >= DateDebut)
);

PRINT 'MOTIF_ABSENCE and ABSENCE_EMPLOYE tables created successfully';

  CREATE TABLE [dbo].[JOURS_FERIES] (
        [IdJourFerie] INT IDENTITY(1,1) PRIMARY KEY,
        [Nom] NVARCHAR(200) NOT NULL,                      -- Holiday name (e.g., "Aid Al Fitr", "Fête du Travail")
        [DateJour] DATE NOT NULL,                           -- Holiday date
        [TypeJour] NVARCHAR(20) NOT NULL DEFAULT 'FERIE',  -- Type: 'FERIE' (Public Holiday) or 'SPECIAL' (Special Day)
        [IsPaid] BIT NOT NULL DEFAULT 1,                    -- Is it a paid holiday?
        [IsRecurrent] BIT NOT NULL DEFAULT 0,               -- Does it recur annually?
        [RecurringType] NVARCHAR(20) NULL,                  -- Recurrence type: 'YEARLY', 'MONTHLY', 'CUSTOM'
        [Description] NVARCHAR(500) NULL,                   -- Additional description
        [CreatedBy] INT NULL,                               -- User who created this record
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [ModifiedBy] INT NULL,                              -- User who last modified
        [ModifiedDate] DATETIME NULL,
        [is_deleted] BIT NOT NULL DEFAULT 0,                -- Soft delete flag

        -- Constraints
        CONSTRAINT CK_JOURS_FERIES_TypeJour CHECK (TypeJour IN ('FERIE', 'SPECIAL')),
        CONSTRAINT CK_JOURS_FERIES_RecurringType CHECK (RecurringType IS NULL OR RecurringType IN ('YEARLY', 'MONTHLY', 'CUSTOM'))
    );

-- Insert default motifs
INSERT INTO MOTIF_ABSENCE (NomMotif, CodeMotif, CouleurBadge) VALUES
('Maladie', 'MALADIE', '#dc3545'),
('Mission', 'MISSION', '#007bff'),
('Conge', 'CONGE', '#28a745'),
('Formation', 'FORMATION', '#17a2b8'),
('Autre', 'AUTRE', '#6c757d');
