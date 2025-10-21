-- =======================================================================
-- Script: Database Setup for Jours Fériés (Public Holidays & Special Days)
-- Purpose: Create table to manage holidays counted as rest days (repos)
-- Database: ERP_RH_Pointag_V2
-- Author: ERP-RH System
-- Date: 2025-01-15
-- =======================================================================

USE [ERP_RH_Pointag_V2]
GO

-- =======================================================================
-- 1. CREATE TABLE: JOURS_FERIES
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

-- =======================================================================
-- 2. CREATE INDEXES
-- =======================================================================

-- Index on DateJour for fast date-based queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JOURS_FERIES_DateJour' AND object_id = OBJECT_ID('JOURS_FERIES'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_JOURS_FERIES_DateJour
    ON [dbo].[JOURS_FERIES] ([DateJour])
    INCLUDE ([TypeJour], [IsPaid], [is_deleted]);

    PRINT 'Index IX_JOURS_FERIES_DateJour created successfully.';
END
GO

-- Index on TypeJour for filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JOURS_FERIES_TypeJour' AND object_id = OBJECT_ID('JOURS_FERIES'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_JOURS_FERIES_TypeJour
    ON [dbo].[JOURS_FERIES] ([TypeJour])
    WHERE is_deleted = 0;

    PRINT 'Index IX_JOURS_FERIES_TypeJour created successfully.';
END
GO

-- =======================================================================
-- 3. INSERT DEFAULT DATA (Morocco Public Holidays for 2025)
-- =======================================================================

-- Check if data already exists
IF NOT EXISTS (SELECT 1 FROM JOURS_FERIES WHERE YEAR(DateJour) = 2025 AND is_deleted = 0)
BEGIN
    INSERT INTO [dbo].[JOURS_FERIES]
    ([Nom], [DateJour], [TypeJour], [IsPaid], [IsRecurrent], [RecurringType], [Description], [CreatedDate])
    VALUES
    -- Fixed Public Holidays (Civil Calendar)
    ('Nouvel An', '2025-01-01', 'FERIE', 1, 1, 'YEARLY', 'Jour de l''An - Calendrier Grégorien', GETDATE()),
    ('Manifeste de l''Indépendance', '2025-01-11', 'FERIE', 1, 1, 'YEARLY', 'Commémoration du Manifeste de l''Indépendance (1944)', GETDATE()),
    ('Fête du Travail', '2025-05-01', 'FERIE', 1, 1, 'YEARLY', 'Journée Internationale des Travailleurs', GETDATE()),
    ('Fête du Trône', '2025-07-30', 'FERIE', 1, 1, 'YEARLY', 'Anniversaire de l''intronisation de Sa Majesté le Roi Mohammed VI', GETDATE()),
    ('Journée de Oued Ed-Dahab', '2025-08-14', 'FERIE', 1, 1, 'YEARLY', 'Commémoration de la récupération de Oued Ed-Dahab (1979)', GETDATE()),
    ('Révolution du Roi et du Peuple', '2025-08-20', 'FERIE', 1, 1, 'YEARLY', 'Commémoration de la Révolution du Roi Mohammed V et du Peuple', GETDATE()),
    ('Fête de la Jeunesse', '2025-08-21', 'FERIE', 1, 1, 'YEARLY', 'Anniversaire de Sa Majesté le Roi Mohammed VI', GETDATE()),
    ('Marche Verte', '2025-11-06', 'FERIE', 1, 1, 'YEARLY', 'Commémoration de la Marche Verte (1975)', GETDATE()),
    ('Fête de l''Indépendance', '2025-11-18', 'FERIE', 1, 1, 'YEARLY', 'Commémoration de l''Indépendance du Maroc (1956)', GETDATE());

    PRINT 'Default public holidays for 2025 inserted successfully.';

    -- Note: Islamic calendar holidays (Aid Al Fitr, Aid Al Adha, etc.) should be added manually
    -- each year as their dates vary based on lunar calendar

    PRINT '';
    PRINT 'IMPORTANT: Islamic holidays (Aid Al Fitr, Aid Al Adha, Hijri New Year, Achoura, Mawlid) must be added manually.';
    PRINT 'These dates vary each year based on the Hijri (lunar) calendar.';
END
ELSE
BEGIN
    PRINT 'Public holidays for 2025 already exist in the database.';
END
GO

-- =======================================================================
-- 4. CREATE HELPER VIEWS
-- =======================================================================

-- View for upcoming holidays
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_JOURS_FERIES_UPCOMING')
    DROP VIEW VW_JOURS_FERIES_UPCOMING;
GO

CREATE VIEW VW_JOURS_FERIES_UPCOMING
AS
SELECT
    IdJourFerie,
    Nom,
    DateJour,
    TypeJour,
    IsPaid,
    IsRecurrent,
    DATEDIFF(DAY, GETDATE(), DateJour) AS DaysUntil,
    DATENAME(WEEKDAY, DateJour) AS DayOfWeek,
    CASE
        WHEN TypeJour = 'FERIE' THEN 'Jour Férié'
        WHEN TypeJour = 'SPECIAL' THEN 'Jour Spécial'
        ELSE 'Autre'
    END AS TypeDescription
FROM JOURS_FERIES
WHERE is_deleted = 0
    AND DateJour >= CAST(GETDATE() AS DATE)
    AND YEAR(DateJour) = YEAR(GETDATE());
GO

PRINT 'View VW_JOURS_FERIES_UPCOMING created successfully.';
GO

-- View for current year holidays
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_JOURS_FERIES_CURRENT_YEAR')
    DROP VIEW VW_JOURS_FERIES_CURRENT_YEAR;
GO

CREATE VIEW VW_JOURS_FERIES_CURRENT_YEAR
AS
SELECT
    IdJourFerie,
    Nom,
    DateJour,
    TypeJour,
    IsPaid,
    IsRecurrent,
    Description,
    MONTH(DateJour) AS Mois,
    DAY(DateJour) AS Jour,
    DATENAME(WEEKDAY, DateJour) AS JourSemaine
FROM JOURS_FERIES
WHERE is_deleted = 0
    AND YEAR(DateJour) = YEAR(GETDATE());
GO

PRINT 'View VW_JOURS_FERIES_CURRENT_YEAR created successfully.';
GO

-- =======================================================================
-- 5. CREATE STORED PROCEDURES
-- =======================================================================

-- Procedure to check if a date is a holiday
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_IsHoliday')
    DROP PROCEDURE SP_IsHoliday;
GO

CREATE PROCEDURE SP_IsHoliday
    @CheckDate DATE,
    @IsHoliday BIT OUTPUT,
    @HolidayName NVARCHAR(200) OUTPUT,
    @IsPaid BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        @IsHoliday = 1,
        @HolidayName = Nom,
        @IsPaid = IsPaid
    FROM JOURS_FERIES
    WHERE DateJour = @CheckDate
        AND is_deleted = 0;

    IF @@ROWCOUNT = 0
    BEGIN
        SET @IsHoliday = 0;
        SET @HolidayName = NULL;
        SET @IsPaid = 0;
    END
END
GO

PRINT 'Stored Procedure SP_IsHoliday created successfully.';
GO

-- Procedure to get all holidays for a date range
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetHolidaysByDateRange')
    DROP PROCEDURE SP_GetHolidaysByDateRange;
GO

CREATE PROCEDURE SP_GetHolidaysByDateRange
    @StartDate DATE,
    @EndDate DATE,
    @TypeJour NVARCHAR(20) = NULL -- Optional filter by type
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdJourFerie,
        Nom,
        DateJour,
        TypeJour,
        IsPaid,
        IsRecurrent,
        RecurringType,
        Description,
        DATENAME(WEEKDAY, DateJour) AS DayOfWeek
    FROM JOURS_FERIES
    WHERE is_deleted = 0
        AND DateJour BETWEEN @StartDate AND @EndDate
        AND (@TypeJour IS NULL OR TypeJour = @TypeJour)
    ORDER BY DateJour ASC;
END
GO

PRINT 'Stored Procedure SP_GetHolidaysByDateRange created successfully.';
GO

-- Procedure to count working days (excluding holidays)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CountWorkingDays')
    DROP PROCEDURE SP_CountWorkingDays;
GO

CREATE PROCEDURE SP_CountWorkingDays
    @StartDate DATE,
    @EndDate DATE,
    @WorkingDays INT OUTPUT,
    @HolidayCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalDays INT;
    DECLARE @CurrentDate DATE = @StartDate;
    SET @WorkingDays = 0;
    SET @HolidayCount = 0;

    WHILE @CurrentDate <= @EndDate
    BEGIN
        -- Check if it's a holiday
        IF EXISTS (SELECT 1 FROM JOURS_FERIES WHERE DateJour = @CurrentDate AND is_deleted = 0)
        BEGIN
            SET @HolidayCount = @HolidayCount + 1;
        END
        -- Check if it's a weekend (Saturday or Sunday)
        ELSE IF DATEPART(WEEKDAY, @CurrentDate) NOT IN (1, 7) -- 1=Sunday, 7=Saturday
        BEGIN
            SET @WorkingDays = @WorkingDays + 1;
        END

        SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
    END
END
GO

PRINT 'Stored Procedure SP_CountWorkingDays created successfully.';
GO

-- =======================================================================
-- 6. SAMPLE QUERIES
-- =======================================================================

PRINT '';
PRINT '========================================';
PRINT 'SAMPLE QUERIES:';
PRINT '========================================';
PRINT '';
PRINT '-- Get all upcoming holidays:';
PRINT 'SELECT * FROM VW_JOURS_FERIES_UPCOMING ORDER BY DateJour;';
PRINT '';
PRINT '-- Check if a specific date is a holiday:';
PRINT 'DECLARE @IsHol BIT, @Name NVARCHAR(200), @Paid BIT;';
PRINT 'EXEC SP_IsHoliday ''2025-05-01'', @IsHol OUTPUT, @Name OUTPUT, @Paid OUTPUT;';
PRINT 'SELECT @IsHol AS IsHoliday, @Name AS HolidayName, @Paid AS IsPaid;';
PRINT '';
PRINT '-- Get holidays in a date range:';
PRINT 'EXEC SP_GetHolidaysByDateRange ''2025-01-01'', ''2025-12-31'', NULL;';
PRINT '';
PRINT '-- Count working days between two dates:';
PRINT 'DECLARE @WorkDays INT, @HolDays INT;';
PRINT 'EXEC SP_CountWorkingDays ''2025-01-01'', ''2025-12-31'', @WorkDays OUTPUT, @HolDays OUTPUT;';
PRINT 'SELECT @WorkDays AS WorkingDays, @HolDays AS Holidays;';
PRINT '';

-- =======================================================================
-- 7. GRANT PERMISSIONS (Optional - adjust as needed)
-- =======================================================================

-- Grant permissions to application user (adjust username as needed)
-- GRANT SELECT, INSERT, UPDATE, DELETE ON JOURS_FERIES TO [YourAppUser];
-- GRANT EXECUTE ON SP_IsHoliday TO [YourAppUser];
-- GRANT EXECUTE ON SP_GetHolidaysByDateRange TO [YourAppUser];
-- GRANT EXECUTE ON SP_CountWorkingDays TO [YourAppUser];

-- =======================================================================
-- SCRIPT COMPLETE
-- =======================================================================

PRINT '';
PRINT '========================================';
PRINT 'Database setup completed successfully!';
PRINT '========================================';
PRINT '';
PRINT 'Table created: JOURS_FERIES';
PRINT 'Indexes created: 2';
PRINT 'Views created: 2';
PRINT 'Stored Procedures created: 3';
PRINT 'Default holidays inserted: 9 (Civil calendar only)';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Navigate to /conge/GestionJoursFeries.aspx to manage holidays';
PRINT '2. Add Islamic calendar holidays manually for each year';
PRINT '3. Configure permissions as needed';
PRINT '';
GO
