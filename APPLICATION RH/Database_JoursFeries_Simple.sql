-- =======================================================================
-- Script: Simple Database Setup for Jours Fériés (Public Holidays)
-- Purpose: Create only the JOURS_FERIES table
-- Database: ERP_RH_Pointag_V2
-- Author: ERP-RH System
-- Date: 2025-01-15
-- =======================================================================

USE [ERP_RH_Pointag_V2]
GO

-- =======================================================================
-- CREATE TABLE: JOURS_FERIES
-- =======================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JOURS_FERIES]') AND type in (N'U'))
BEGIN
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

    PRINT 'Table JOURS_FERIES created successfully.';
END
ELSE
BEGIN
    PRINT 'Table JOURS_FERIES already exists.';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Database setup completed!';
PRINT '========================================';
PRINT '';
PRINT 'Table created: JOURS_FERIES';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Navigate to /conge/GestionJoursFeries.aspx';
PRINT '2. Use the interface to add holidays';
PRINT '3. Click "Ajouter Jours Fériés Officiels" button to auto-add Morocco holidays';
PRINT '';
GO
