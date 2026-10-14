-- =====================================================================
-- ERP_RH_Pointag_V2 - full schema + reference data
-- Rebuilt from SQLQuery3.sql, Database_*.sql and every query in the
-- APPLICATION RH code-behind (2026-10-14).
-- Run with:  sqlcmd -S <server> -U <user> -P <pwd> -f 65001 -i ERP_RH_Pointag_V2_Full.sql
-- =====================================================================
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;   -- required for filtered indexes
SET ANSI_NULLS ON;
GO
IF DB_ID(N'ERP_RH_Pointag_V2') IS NULL
    CREATE DATABASE ERP_RH_Pointag_V2;
GO
USE ERP_RH_Pointag_V2;
GO

-- =====================================================================
-- 1. ORGANISATION
-- =====================================================================
CREATE TABLE SOCIETE (
    IdSociete        INT IDENTITY(1,1) PRIMARY KEY,
    NomSociete       NVARCHAR(150) NOT NULL,
    CodeSociete      NVARCHAR(10)  NOT NULL UNIQUE,
    Adresse          NVARCHAR(250),
    Telephone        NVARCHAR(20),
    Email            NVARCHAR(100),
    RegistreCommerce NVARCHAR(50),
    ICE              NVARCHAR(20),
    DateCreation     DATETIME DEFAULT GETDATE(),
    Actif            BIT DEFAULT 1
);

CREATE TABLE DEPARTEMENT (
    IdDepartement   INT IDENTITY(1,1) PRIMARY KEY,
    NomDepartement  NVARCHAR(100) NOT NULL,
    CodeDepartement NVARCHAR(10) UNIQUE,
    Description     NVARCHAR(250),
    DateCreation    DATETIME DEFAULT GETDATE(),
    Actif           BIT DEFAULT 1,
    IdSociete       INT NOT NULL REFERENCES SOCIETE(IdSociete)
);

CREATE TABLE SERVICE (
    IdService          INT IDENTITY(1,1) PRIMARY KEY,
    IdDepartement      INT NOT NULL REFERENCES DEPARTEMENT(IdDepartement),
    NomService         NVARCHAR(100) NOT NULL,
    CodeService        NVARCHAR(10),
    ResponsableService NVARCHAR(100),
    DateCreation       DATETIME DEFAULT GETDATE(),
    Actif              BIT DEFAULT 1
);

CREATE TABLE CELLULE (
    IdCellule    INT IDENTITY(1,1) PRIMARY KEY,
    IdService    INT NOT NULL REFERENCES SERVICE(IdService),
    NomCellule   NVARCHAR(100) NOT NULL,
    CodeCellule  NVARCHAR(10),
    Description  NVARCHAR(250),
    DateCreation DATETIME DEFAULT GETDATE(),
    Actif        BIT DEFAULT 1
);

CREATE TABLE FONCTION (
    IdFonction           INT IDENTITY(1,1) PRIMARY KEY,
    IdService            INT NOT NULL REFERENCES SERVICE(IdService),
    IdCellule            INT NOT NULL REFERENCES CELLULE(IdCellule),
    NomFonction          NVARCHAR(100) NOT NULL,
    CodeFonction         NVARCHAR(10),
    Description          NVARCHAR(250),
    NiveauResponsabilite INT DEFAULT 1, -- 1=Ouvrier, 2=Superviseur, 3=Chef
    DateCreation         DATETIME DEFAULT GETDATE(),
    Actif                BIT DEFAULT 1
);

CREATE TABLE EMPLACEMENT (
    IdEmplacement   INT IDENTITY(1,1) PRIMARY KEY,
    IdSociete       INT NOT NULL REFERENCES SOCIETE(IdSociete),
    NomEmplacement  NVARCHAR(100) NOT NULL,
    TypeEmplacement NVARCHAR(50), -- Usine, Bureau, Entrepôt
    Adresse         NVARCHAR(250),
    Superficie      DECIMAL(10,2),
    DateCreation    DATETIME DEFAULT GETDATE(),
    Actif           BIT DEFAULT 1
);

CREATE TABLE TYPEOUVRIER (
    IdTypeOuv    INT IDENTITY(1,1) PRIMARY KEY,
    NomTypeOuv   NVARCHAR(50) NOT NULL UNIQUE,
    Description  NVARCHAR(250),
    Actif        BIT DEFAULT 1,
    DateCreation DATETIME DEFAULT GETDATE()
);
GO

-- =====================================================================
-- 2. EMPLOYE
-- =====================================================================
CREATE TABLE EMPLOYE (
    IdEmploye          INT IDENTITY(1,1) PRIMARY KEY,
    IdSociete          INT NOT NULL REFERENCES SOCIETE(IdSociete),
    Matricule          NVARCHAR(20)  NOT NULL,
    Nom                NVARCHAR(150) NOT NULL,
    Prenom             NVARCHAR(150) NOT NULL,
    DateNaissance      DATE,
    LieuNaissance      NVARCHAR(100),
    CinPassport        NVARCHAR(50), -- no UNIQUE (dropped in SQLQuery3.sql)
    Sexe               NVARCHAR(10) CHECK (Sexe IN ('Homme', 'Femme')),
    SituationFamiliale NVARCHAR(50),
    NombreEnfants      INT DEFAULT 0,
    Adresse            NVARCHAR(250),
    Telephone          NVARCHAR(20),
    Email              NVARCHAR(100),
    Nationalite        NVARCHAR(100),
    NiveauScolaire     NVARCHAR(100),
    Diplome            NVARCHAR(150),
    DateEmbauche       DATE NOT NULL,
    DateSortie         DATE NULL,
    TypeContrat        NVARCHAR(50),
    TypeEmploye        NVARCHAR(50) NOT NULL,
    IdTypeOuv          INT NULL REFERENCES TYPEOUVRIER(IdTypeOuv),
    Rolle              NVARCHAR(50) NULL,   -- matches ROLES.RoleName
    PasswordHash       NVARCHAR(255) NULL,  -- app compares plain text today
    IdDepartement      INT REFERENCES DEPARTEMENT(IdDepartement),
    IdService          INT REFERENCES SERVICE(IdService),
    IdCellule          INT REFERENCES CELLULE(IdCellule),
    IdFonction         INT REFERENCES FONCTION(IdFonction),
    IdEmplacement      INT REFERENCES EMPLACEMENT(IdEmplacement),
    Shift              NVARCHAR(10) CHECK (Shift IN ('JOUR', 'NUIT')),
    SituationCNSS      NVARCHAR(5)  CHECK (SituationCNSS IN ('D', 'ND')),
    NumeroCNSS         NVARCHAR(50),
    Actif              BIT DEFAULT 1,       -- used by ListeEmployes export
    is_deleted         BIT DEFAULT 0,
    DateCreation       DATETIME DEFAULT GETDATE(),
    DateModification   DATETIME,
    ModifiePar         NVARCHAR(100),
    CONSTRAINT UK_Employe_Matricule_Societe UNIQUE (Matricule, IdSociete),
    CONSTRAINT CK_DateSortie CHECK (DateSortie IS NULL OR DateSortie >= DateEmbauche)
);

CREATE TABLE AUDIT_EMPLOYE (
    IdAudit          INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye        INT NOT NULL REFERENCES EMPLOYE(IdEmploye),
    TypeOperation    NVARCHAR(20) NOT NULL,
    ChampModifie     NVARCHAR(100),
    AncienneValeur   NVARCHAR(500),
    NouvelleValeur   NVARCHAR(500),
    ModifiePar       NVARCHAR(100) NOT NULL,
    DateModification DATETIME DEFAULT GETDATE()
);

-- EditEmployee.aspx.cs
CREATE TABLE LOG_EMPLOYE (
    IdLog             INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye         INT NOT NULL REFERENCES EMPLOYE(IdEmploye),
    Action            NVARCHAR(50) NOT NULL,
    DateAction        DATETIME NOT NULL DEFAULT GETDATE(),
    UtilisateurAction NVARCHAR(100),
    Details           NVARCHAR(MAX)
);
GO

-- =====================================================================
-- 3. ROLES / PAGES / PERMISSIONS
-- =====================================================================
CREATE TABLE ROLES (
    IdRole       INT IDENTITY(1,1) PRIMARY KEY,
    RoleName     NVARCHAR(50) NOT NULL UNIQUE,
    Description  NVARCHAR(250),
    IsActive     BIT NOT NULL DEFAULT 1,
    DateCreation DATETIME DEFAULT GETDATE()
);

CREATE TABLE PAGES (
    IdPage       INT IDENTITY(1,1) PRIMARY KEY,
    PageName     NVARCHAR(100) NOT NULL,
    PageURL      NVARCHAR(250) NOT NULL UNIQUE,
    MenuGroup    NVARCHAR(50),
    IsActive     BIT NOT NULL DEFAULT 1,
    DateCreation DATETIME DEFAULT GETDATE()
);

CREATE TABLE ROLE_PERMISSIONS (
    IdRolePermission INT IDENTITY(1,1) PRIMARY KEY,
    IdRole           INT NOT NULL REFERENCES ROLES(IdRole),
    IdPage           INT NOT NULL REFERENCES PAGES(IdPage),
    HasAccess        BIT NOT NULL DEFAULT 1,
    CONSTRAINT UK_RolePermission UNIQUE (IdRole, IdPage)
);
GO

-- MenuHelper.cs queries EMPLOYEES.IdRole; this view maps it onto EMPLOYE.Rolle
CREATE VIEW EMPLOYEES AS
SELECT e.*, r.IdRole
FROM EMPLOYE e
LEFT JOIN ROLES r ON r.RoleName = e.Rolle;
GO

CREATE TABLE PASSWORD_AUDIT (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId  INT NOT NULL,
    Action      VARCHAR(100) NOT NULL,
    PerformedBy INT NOT NULL,
    ActionDate  DATETIME NOT NULL,
    IPAddress   VARCHAR(45)
);

-- Site.Master.cs
CREATE TABLE USER_LOGS (
    IdLog      INT IDENTITY(1,1) PRIMARY KEY,
    EmployeId  INT NOT NULL,
    Action     NVARCHAR(100) NOT NULL,
    ActionDate DATETIME NOT NULL DEFAULT GETDATE(),
    IPAddress  VARCHAR(45)
);
GO

-- =====================================================================
-- 4. ÉQUIPES & POINTAGE PRODUCTION
-- =====================================================================
CREATE TABLE CHEF_EQUIPE (
    IdChefEquipe    INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye       INT NOT NULL REFERENCES EMPLOYE(IdEmploye),
    NomEquipe       NVARCHAR(100) NOT NULL,
    AssignePar      INT NOT NULL REFERENCES EMPLOYE(IdEmploye),
    IdSociete       INT NOT NULL REFERENCES SOCIETE(IdSociete),
    IdDepartement   INT NULL REFERENCES DEPARTEMENT(IdDepartement),
    is_deleted      BIT DEFAULT 0,
    DateAssignation DATE DEFAULT GETDATE(),
    DateCreation    DATETIME DEFAULT GETDATE()
);
-- one active chef role per employee (soft-deleted rows don't block re-creation)
CREATE UNIQUE INDEX UX_ChefEquipe_Employe_Active ON CHEF_EQUIPE(IdEmploye) WHERE is_deleted = 0;

CREATE TABLE EMPLOYE_CHEF_EQUIPE (
    IdEmployeChefEquipe INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye           INT NOT NULL REFERENCES EMPLOYE(IdEmploye),
    IdChefEquipe        INT NOT NULL REFERENCES CHEF_EQUIPE(IdChefEquipe),
    -- NVARCHAR: TransfertAssignation stores 'TRANSFERT - <id>' and filters LIKE 'TRANSFERT%'
    AssignePar          NVARCHAR(100) NOT NULL,
    DateAssignation     DATE DEFAULT GETDATE(),
    DateFinAssignation  DATE NULL,
    is_deleted          BIT DEFAULT 0,
    DateCreation        DATETIME DEFAULT GETDATE(),
    CONSTRAINT CK_EmployeChefEquipe_DateFin CHECK (DateFinAssignation IS NULL OR DateFinAssignation >= DateAssignation)
);
CREATE UNIQUE INDEX UX_EmployeChefEquipe_Active ON EMPLOYE_CHEF_EQUIPE(IdEmploye)
    WHERE DateFinAssignation IS NULL AND is_deleted = 0;

CREATE TABLE POINTAGE (
    IdPointage            INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye             INT NOT NULL REFERENCES EMPLOYE(IdEmploye),
    DatePointage          DATE NOT NULL,
    Shift                 NVARCHAR(10) CHECK (Shift IN ('JOUR', 'NUIT')),
    StatutPresence        NVARCHAR(10) CHECK (StatutPresence IN ('P', 'A', 'P/2')),
    Coefficient           DECIMAL(3,2) DEFAULT 1.00,
    HeureEntree           TIME NULL,
    HeureSortie           TIME NULL,
    IdDepartement         INT NULL REFERENCES DEPARTEMENT(IdDepartement),
    IdService             INT NULL REFERENCES SERVICE(IdService),
    IdCellule             INT NULL REFERENCES CELLULE(IdCellule),
    IdFonction            INT NULL REFERENCES FONCTION(IdFonction),
    IdEmplacement         INT NULL REFERENCES EMPLACEMENT(IdEmplacement),
    IdChefEquipe          INT NULL REFERENCES CHEF_EQUIPE(IdChefEquipe),
    MotifAbsence          NVARCHAR(100) NULL,
    DateCreationPointage  DATETIME DEFAULT GETDATE(),
    HeureCreationPointage TIME DEFAULT CAST(GETDATE() AS TIME),
    PointePar             NVARCHAR(100) NOT NULL,
    CONSTRAINT UK_Pointage_Employe_Date UNIQUE (IdEmploye, DatePointage, Shift),
    CONSTRAINT CK_Horaires CHECK (
        (StatutPresence = 'A' AND HeureEntree IS NULL AND HeureSortie IS NULL) OR
        (StatutPresence IN ('P', 'P/2') AND HeureEntree IS NOT NULL)
    )
);

CREATE TABLE POINTAGE_HEURE_SUPP (
    IdHeureSupp      INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye        INT NOT NULL,
    DateHeureSupp    DATE NOT NULL,
    HeureDebut       TIME NULL,
    HeureFin         TIME NULL,
    NombreHeures     DECIMAL(5,2) NOT NULL,
    Coefficient      DECIMAL(3,2) NOT NULL DEFAULT 1.00,
    Justification    NVARCHAR(500) NULL,
    IdChefEquipe     INT NOT NULL,
    SaisiPar         NVARCHAR(100) NOT NULL,
    DateSaisie       DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiePar       NVARCHAR(100) NULL,
    DateModification DATETIME NULL,
    CONSTRAINT FK_POINTAGE_HEURE_SUPP_EMPLOYE     FOREIGN KEY (IdEmploye)    REFERENCES EMPLOYE(IdEmploye),
    CONSTRAINT FK_POINTAGE_HEURE_SUPP_CHEF_EQUIPE FOREIGN KEY (IdChefEquipe) REFERENCES CHEF_EQUIPE(IdChefEquipe),
    CONSTRAINT CK_POINTAGE_HEURE_SUPP_COEFFICIENT    CHECK (Coefficient IN (1.00, 1.25, 1.50, 2.00)),
    CONSTRAINT CK_POINTAGE_HEURE_SUPP_NOMBRE_HEURES  CHECK (NombreHeures > 0 AND NombreHeures <= 24),
    CONSTRAINT UQ_POINTAGE_HEURE_SUPP_EMPLOYE_DATE   UNIQUE (IdEmploye, DateHeureSupp)
);

-- Mobile/PointageMobile.aspx.cs (rows are written by the mobile app)
CREATE TABLE POINTAGE_MOBILE (
    IdPointageMobile    INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye           INT NULL REFERENCES EMPLOYE(IdEmploye),
    Matricule           NVARCHAR(20) NOT NULL,
    NomComplet          NVARCHAR(300),
    Statut              NVARCHAR(20) NOT NULL CHECK (Statut IN ('present', 'absent', 'demi_journee')),
    DatePointage        DATETIME NOT NULL,
    DeviceInfo          NVARCHAR(500),
    DateCreation        DATETIME NOT NULL DEFAULT GETDATE(),
    DateExport          DATETIME NULL,
    DateReception       DATETIME NULL,
    IdUtilisateurExport INT NULL,
    updated_at          DATETIME NULL,
    is_deleted          BIT NOT NULL DEFAULT 0
);
GO

-- =====================================================================
-- 5. POINTAGE PORTES
-- =====================================================================
CREATE TABLE PORTEUSINE (
    idPorte INT IDENTITY(1,1) PRIMARY KEY,
    Porte   NVARCHAR(100) NOT NULL UNIQUE -- must stay 2nd column: read as reader[1]
);

-- which gates a guard account may scan at
CREATE TABLE AccessLogine (
    IdAccess       INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye      INT NOT NULL REFERENCES EMPLOYE(IdEmploye),
    IDPorteAssigne INT NOT NULL REFERENCES PORTEUSINE(idPorte),
    DateCreation   DATETIME DEFAULT GETDATE(),
    CONSTRAINT UK_AccessLogine UNIQUE (IdEmploye, IDPorteAssigne)
);

CREATE TABLE EMPLOYE_PORT (
    IdEmployePort    INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye        INT NOT NULL,
    NomPort          NVARCHAR(100) NOT NULL,
    AssignePar       INT NULL,
    DateCreation     DATETIME NOT NULL DEFAULT GETDATE(),
    DateModification DATETIME NULL,
    is_deleted       BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_EMPLOYE_PORT_EMPLOYE   FOREIGN KEY (IdEmploye)  REFERENCES EMPLOYE(IdEmploye),
    CONSTRAINT FK_EMPLOYE_PORT_ASSIGNEUR FOREIGN KEY (AssignePar) REFERENCES EMPLOYE(IdEmploye)
);

CREATE TABLE EMPLOYE_POINTAGE (
    IdPointage       INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye        INT NOT NULL,
    NomPort          NVARCHAR(100) NOT NULL,
    TypePointage     VARCHAR(10) NOT NULL,
    DateHeure        DATETIME NOT NULL,
    AssignePar       INT NULL,
    DateCreation     DATETIME NOT NULL DEFAULT GETDATE(),
    DateModification DATETIME NULL,
    ModifiePar       NVARCHAR(100) NULL, -- used by EditPointagePort.aspx.cs
    is_deleted       BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_EMPLOYE_POINTAGE_EMPLOYE   FOREIGN KEY (IdEmploye)  REFERENCES EMPLOYE(IdEmploye),
    CONSTRAINT FK_EMPLOYE_POINTAGE_ASSIGNEUR FOREIGN KEY (AssignePar) REFERENCES EMPLOYE(IdEmploye),
    CONSTRAINT CK_EMPLOYE_POINTAGE_TYPE CHECK (TypePointage IN ('ENTREE', 'SORTIE'))
);
CREATE INDEX IX_EMPLOYE_POINTAGE_Emp_Date ON EMPLOYE_POINTAGE(IdEmploye, DateHeure) INCLUDE (TypePointage, is_deleted);
GO

CREATE FUNCTION dbo.fn_CalculateWorkingHours(@IdEmploye INT, @Date DATE)
RETURNS VARCHAR(10)
AS
BEGIN
    DECLARE @TotalMinutes INT = 0;
    DECLARE @Entrees TABLE (DateHeure DATETIME);
    DECLARE @Sorties TABLE (DateHeure DATETIME);

    INSERT INTO @Entrees SELECT DateHeure FROM EMPLOYE_POINTAGE
    WHERE IdEmploye = @IdEmploye AND CAST(DateHeure AS DATE) = @Date AND TypePointage = 'ENTREE' AND is_deleted = 0;
    INSERT INTO @Sorties SELECT DateHeure FROM EMPLOYE_POINTAGE
    WHERE IdEmploye = @IdEmploye AND CAST(DateHeure AS DATE) = @Date AND TypePointage = 'SORTIE' AND is_deleted = 0;

    DECLARE @EntreeHeure DATETIME, @SortieHeure DATETIME;
    DECLARE entree_cursor CURSOR FOR SELECT DateHeure FROM @Entrees ORDER BY DateHeure;
    DECLARE sortie_cursor CURSOR FOR SELECT DateHeure FROM @Sorties ORDER BY DateHeure;
    OPEN entree_cursor;
    OPEN sortie_cursor;
    FETCH NEXT FROM entree_cursor INTO @EntreeHeure;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        FETCH NEXT FROM sortie_cursor INTO @SortieHeure;
        IF @@FETCH_STATUS = 0 AND @SortieHeure > @EntreeHeure
            SET @TotalMinutes = @TotalMinutes + DATEDIFF(MINUTE, @EntreeHeure, @SortieHeure);
        FETCH NEXT FROM entree_cursor INTO @EntreeHeure;
    END
    CLOSE entree_cursor;  DEALLOCATE entree_cursor;
    CLOSE sortie_cursor;  DEALLOCATE sortie_cursor;

    RETURN CAST(@TotalMinutes / 60 AS VARCHAR(4)) + 'h' + RIGHT('00' + CAST(@TotalMinutes % 60 AS VARCHAR(2)), 2);
END;
GO

-- =====================================================================
-- 6. CONGÉS / ABSENCES / JOURS FÉRIÉS
-- =====================================================================
CREATE TABLE MOTIF_ABSENCE (
    IdMotif      INT IDENTITY(1,1) PRIMARY KEY,
    NomMotif     NVARCHAR(100) NOT NULL UNIQUE,
    CodeMotif    NVARCHAR(20)  NOT NULL UNIQUE,
    CouleurBadge NVARCHAR(20) DEFAULT '#6c757d',
    IsActive     BIT DEFAULT 1,
    DateCreation DATETIME DEFAULT GETDATE(),
    is_deleted   BIT DEFAULT 0
);

CREATE TABLE ABSENCE_EMPLOYE (
    IdAbsence    INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye    INT NOT NULL REFERENCES EMPLOYE(IdEmploye),
    IdMotif      INT NOT NULL REFERENCES MOTIF_ABSENCE(IdMotif),
    DateDebut    DATE NOT NULL,
    DateFin      DATE NOT NULL,
    Description  NVARCHAR(500),
    CreePar      INT NOT NULL,
    DateCreation DATETIME DEFAULT GETDATE(),
    is_deleted   BIT DEFAULT 0,
    CONSTRAINT CK_DateFin_Absence CHECK (DateFin >= DateDebut)
);

-- conge/absence-permissions.aspx.cs
CREATE TABLE PERMISSION_ABSENCE (
    IdPermissionAbsence INT IDENTITY(1,1) PRIMARY KEY,
    TypeAbsence         NVARCHAR(100) NOT NULL UNIQUE,
    Description         NVARCHAR(500),
    Actif               BIT NOT NULL DEFAULT 1,
    DateCreation        DATETIME DEFAULT GETDATE(),
    is_deleted          BIT NOT NULL DEFAULT 0
);

CREATE TABLE PERMISSION_ABSENCE_EMPLOYE (
    IdDemandeAbsence    INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye           INT NOT NULL REFERENCES EMPLOYE(IdEmploye),
    IdPermissionAbsence INT NOT NULL REFERENCES PERMISSION_ABSENCE(IdPermissionAbsence),
    DateDebut           DATETIME NOT NULL,
    DateFin             DATETIME NOT NULL,
    EstApprouvee        BIT NOT NULL DEFAULT 0,
    ApprouveePar        INT NULL REFERENCES EMPLOYE(IdEmploye),
    DateCreation        DATETIME NOT NULL DEFAULT GETDATE(),
    DateModification    DATETIME NULL,
    is_deleted          BIT NOT NULL DEFAULT 0,
    CONSTRAINT CK_PermissionAbsence_Dates CHECK (DateFin >= DateDebut)
);

CREATE TABLE JOURS_FERIES (
    IdJourFerie   INT IDENTITY(1,1) PRIMARY KEY,
    Nom           NVARCHAR(200) NOT NULL,
    DateJour      DATE NOT NULL,
    TypeJour      NVARCHAR(20) NOT NULL DEFAULT 'FERIE',
    IsPaid        BIT NOT NULL DEFAULT 1,
    IsRecurrent   BIT NOT NULL DEFAULT 0,
    RecurringType NVARCHAR(20) NULL,
    Description   NVARCHAR(500) NULL,
    CreatedBy     INT NULL,
    CreatedDate   DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedBy    INT NULL,
    ModifiedDate  DATETIME NULL,
    is_deleted    BIT NOT NULL DEFAULT 0,
    CONSTRAINT CK_JOURS_FERIES_TypeJour CHECK (TypeJour IN ('FERIE', 'SPECIAL')),
    CONSTRAINT CK_JOURS_FERIES_RecurringType CHECK (RecurringType IS NULL OR RecurringType IN ('YEARLY', 'MONTHLY', 'CUSTOM'))
);
CREATE NONCLUSTERED INDEX IX_JOURS_FERIES_DateJour ON JOURS_FERIES(DateJour) INCLUDE (TypeJour, IsPaid, is_deleted);
CREATE NONCLUSTERED INDEX IX_JOURS_FERIES_TypeJour ON JOURS_FERIES(TypeJour) WHERE is_deleted = 0;
GO

CREATE VIEW VW_JOURS_FERIES_UPCOMING AS
SELECT IdJourFerie, Nom, DateJour, TypeJour, IsPaid, IsRecurrent,
       DATEDIFF(DAY, GETDATE(), DateJour) AS DaysUntil,
       DATENAME(WEEKDAY, DateJour) AS DayOfWeek,
       CASE WHEN TypeJour = 'FERIE' THEN N'Jour Férié' WHEN TypeJour = 'SPECIAL' THEN N'Jour Spécial' ELSE 'Autre' END AS TypeDescription
FROM JOURS_FERIES
WHERE is_deleted = 0 AND DateJour >= CAST(GETDATE() AS DATE) AND YEAR(DateJour) = YEAR(GETDATE());
GO
CREATE VIEW VW_JOURS_FERIES_CURRENT_YEAR AS
SELECT IdJourFerie, Nom, DateJour, TypeJour, IsPaid, IsRecurrent, Description,
       MONTH(DateJour) AS Mois, DAY(DateJour) AS Jour, DATENAME(WEEKDAY, DateJour) AS JourSemaine
FROM JOURS_FERIES
WHERE is_deleted = 0 AND YEAR(DateJour) = YEAR(GETDATE());
GO
CREATE PROCEDURE SP_IsHoliday
    @CheckDate DATE, @IsHoliday BIT OUTPUT, @HolidayName NVARCHAR(200) OUTPUT, @IsPaid BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 @IsHoliday = 1, @HolidayName = Nom, @IsPaid = IsPaid
    FROM JOURS_FERIES WHERE DateJour = @CheckDate AND is_deleted = 0;
    IF @@ROWCOUNT = 0
        SELECT @IsHoliday = 0, @HolidayName = NULL, @IsPaid = 0;
END
GO
CREATE PROCEDURE SP_GetHolidaysByDateRange
    @StartDate DATE, @EndDate DATE, @TypeJour NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdJourFerie, Nom, DateJour, TypeJour, IsPaid, IsRecurrent, RecurringType, Description,
           DATENAME(WEEKDAY, DateJour) AS DayOfWeek
    FROM JOURS_FERIES
    WHERE is_deleted = 0 AND DateJour BETWEEN @StartDate AND @EndDate
      AND (@TypeJour IS NULL OR TypeJour = @TypeJour)
    ORDER BY DateJour;
END
GO
CREATE PROCEDURE SP_CountWorkingDays
    @StartDate DATE, @EndDate DATE, @WorkingDays INT OUTPUT, @HolidayCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CurrentDate DATE = @StartDate;
    SELECT @WorkingDays = 0, @HolidayCount = 0;
    WHILE @CurrentDate <= @EndDate
    BEGIN
        IF EXISTS (SELECT 1 FROM JOURS_FERIES WHERE DateJour = @CurrentDate AND is_deleted = 0)
            SET @HolidayCount += 1;
        ELSE IF DATEPART(WEEKDAY, @CurrentDate) NOT IN (1, 7)
            SET @WorkingDays += 1;
        SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
    END
END
GO

-- =====================================================================
-- 7. REFERENCE DATA
-- =====================================================================
INSERT INTO TYPEOUVRIER (NomTypeOuv, Description) VALUES
(N'MOD', N'Main d''œuvre directe'),
(N'MOI', N'Main d''œuvre indirecte'),
(N'Productif', N'Ouvrier productif'),
(N'Structure', N'Ouvrier de structure');

INSERT INTO SOCIETE (NomSociete, CodeSociete, Adresse, Telephone, Email, RegistreCommerce, ICE) VALUES
(N'GOLD SEA FISH', 'GSF', N'Zone Industrielle Dakhla', '+212-528-123456', 'contact@goldseafish.ma', 'RC12345', 'ICE001'),
(N'ERG CONSERVE', 'ERG', N'Zone Portuaire Dakhla', '+212-528-654321', 'info@ergconserve.ma', 'RC67890', 'ICE002'),
(N'PROTEIN AND OIL', 'PRO', N'Zone Industrielle Agadir', '+212-528-987654', 'contact@proteinoil.ma', 'RC54321', 'ICE003'),
(N'BENIS PELAGIQUE', 'BPG', N'Zone Portuaire Dakhla', '+212-528-111333', 'contact@benispelagique.ma', 'RC11111', 'ICE004'),
(N'ERG PACKAGING', 'EPK', N'Zone Industrielle Dakhla', '+212-528-222444', 'info@ergpackaging.ma', 'RC22222', 'ICE005'),
(N'LOGISTIQUE & TRAITEMENT', 'LGT', N'Zone Logistique Dakhla', '+212-528-333555', 'contact@logistiquetraitement.ma', 'RC33333', 'ICE006'),
(N'BENIS PECHE', 'BPC', N'Port de Pêche Dakhla', '+212-528-444666', 'info@benispeche.ma', 'RC44444', 'ICE007'),
(N'ERG DELICE', 'EDL', N'Zone Commerciale Dakhla', '+212-528-555777', 'contact@ergdelice.ma', 'RC55555', 'ICE008'),
(N'PAOI', 'PAO', N'Zone Industrielle Agadir', '+212-528-666888', 'info@paoi.ma', 'RC66666', 'ICE009'),
(N'PROTEIN PELAGIC', 'PPG', N'Zone Portuaire Agadir', '+212-528-777999', 'contact@proteinpelagic.ma', 'RC77777', 'ICE010');

-- Departments + services for GSF (1), ERG (2), PRO (3) — same structure for each
DECLARE @soc TABLE (IdSociete INT, Code NVARCHAR(5), Nom NVARCHAR(50));
INSERT INTO @soc VALUES (1, 'GSF', N'Gold Sea Fish'), (2, 'ERG', N'ERG Conserve'), (3, 'PRO', N'Protein and Oil');

INSERT INTO DEPARTEMENT (NomDepartement, CodeDepartement, Description, IdSociete)
SELECT d.Nom, d.Prefix + '-' + s.Code, N'Département ' + d.Label + N' - ' + s.Nom, s.IdSociete
FROM @soc s
CROSS JOIN (VALUES (1, N'PRODUCTION', 'PROD', N'de production'),
                   (2, N'QUALITE', 'QUAL', N'qualité'),
                   (3, N'RESSOURCES HUMAINES', 'RH', N'ressources humaines'),
                   (4, N'TECHNIQUE', 'TECH', N'technique'),
                   (5, N'STOCK', 'STOCK', N'stock')) d(Ord, Nom, Prefix, Label)
ORDER BY s.IdSociete, d.Ord;

INSERT INTO SERVICE (IdDepartement, NomService, CodeService)
SELECT dep.IdDepartement, sv.Nom, sv.Prefix + '-' + s.Code
FROM @soc s
CROSS JOIN (VALUES (1, N'PRODUCTION', N'CONTRÔLE', 'CTRL'), (2, N'PRODUCTION', N'DEMOULAGE', 'DMOU'),
                   (3, N'PRODUCTION', N'EMBALLAGE', 'EMBL'), (4, N'PRODUCTION', N'MANUTENTION', 'MANU'),
                   (5, N'PRODUCTION', N'RECEPTION', 'RECP'), (6, N'PRODUCTION', N'SUPPORT', 'SUPP'),
                   (7, N'PRODUCTION', N'TRAITEMENT', 'TRAI'), (8, N'PRODUCTION', N'TRIAGE', 'TRIG'),
                   (9, N'QUALITE', N'HYGIENE', 'HYG'),
                   (10, N'RESSOURCES HUMAINES', N'BIANDRIE', 'BIAN'), (11, N'RESSOURCES HUMAINES', N'MOYENS GENERAUX', 'MGEN'),
                   (12, N'RESSOURCES HUMAINES', N'RESTAURATION', 'REST'),
                   (13, N'TECHNIQUE', N'MAINTENANCE MACHINE', 'MACH'),
                   (14, N'STOCK', N'STOCK', 'STCK')) sv(Ord, Dept, Nom, Prefix)
JOIN DEPARTEMENT dep ON dep.IdSociete = s.IdSociete AND dep.NomDepartement = sv.Dept
ORDER BY s.IdSociete, sv.Ord;
-- => GSF services are IdService 1..14 (used by CELLULE / FONCTION below)

INSERT INTO CELLULE (NomCellule, IdService) VALUES
(N'CONTRÔLE GUANO', 1), (N'CONTRÔLE MACHINE', 1), (N'DEMOULAGE', 2), (N'EMBALLAGE', 3),
(N'MENUISERIE', 4), (N'TRANSPALETTE', 4), (N'COUPAGE', 5), (N'DECHARGE BASSIN', 5),
(N'LIVRAISON GLACE', 5), (N'LIVRAISON SLURY', 5), (N'PECHE COTIERE', 5), (N'RECEPTION MATIERE PREMIERE', 5),
(N'collecte des informations de la prod', 6), (N'SUIVI', 6), (N'MACHINE', 7), (N'CALIBREUSE', 8),
(N'TRIAGE', 8), (N'HYGIENE', 9), (N'BIANDRIE', 10), (N'MOYENS GENERAUX', 11),
(N'RESTAURATION', 12), (N'MAINTEANCE MACHINE', 13), (N'STOCK', 14);

INSERT INTO FONCTION (IdService, IdCellule, NomFonction, NiveauResponsabilite) VALUES
(1, 1, N'ouvrier(e) de contrôle guano', 1),
(1, 2, N'ouvrier(e) de contrôle M', 1),
(1, 2, N'chef d equipe Controle', 1),
(2, 3, N'ouvrier(e) de demoulage', 1),
(3, 4, N'chargé(e) de collecte des informations de la prod', 1),
(3, 4, N'ouvrier(e) de coupage', 1),
(3, 4, N'ouvrier(e) d emballage', 1),
(3, 4, N'chef d equipe emballage', 1),
(4, 5, N'ouvrier(e) de menuiserie', 1),
(4, 6, N'ouvrier(e) de transpalette emballage', 1),
(4, 6, N'ouvrier(e) de traspalette demoulage', 1),
(4, 6, N'ouvrier(e) de traspalette salle', 1),
(5, 7, N'ouvrier(e) de coupage', 1),
(5, 8, N'ouvrier(e) de décharge bassins', 1),
(5, 9, N'ouvrier(e) de glace', 1),
(5, 10, N'ouvrier(e) de slaurry', 1),
(5, 11, N'AGENT DE PECHE COTIERE', 1),
(5, 12, N'agent de réception MP', 1),
(6, 13, N'chargé(e) de collecte des informations de la prod', 1),
(6, 14, N'AGENT SUIVI DES OUVRIERE', 1),
(7, 15, N'chef d equipe contrôle', 1),
(7, 15, N'ouvrier(e) de machine', 1),
(8, 16, N'ouvrier(e) de calibreuse', 1),
(8, 17, N'ouvrier(e) de triage', 1),
(9, 18, N'agent d hygiene', 1),
(9, 18, N'ouvrier(e) de menage', 1),
(10, 19, N'AGENT DE BIANDRIE', 1),
(11, 20, N'ouvrier(e) de menage RH', 1),
(11, 20, N'ouvrier(e) de menage externe', 1),
(12, 21, N'agent de cantine', 1),
(13, 22, N'TECHNICIEN DE MACHINES', 1),
(14, 23, N'agent de fermeture porte chambre froide', 1);

INSERT INTO EMPLACEMENT (NomEmplacement, IdSociete) VALUES
(N'GSF', 3), (N'H E', 3), (N'KT', 3), (N'RH', 3), (N'SALLE 03', 3), (N'STOCK', 3), (N'TECH', 3);

INSERT INTO MOTIF_ABSENCE (NomMotif, CodeMotif, CouleurBadge) VALUES
(N'Maladie', 'MALADIE', '#dc3545'),
(N'Mission', 'MISSION', '#007bff'),
(N'Congé', 'CONGE', '#28a745'),
(N'Formation', 'FORMATION', '#17a2b8'),
(N'Autre', 'AUTRE', '#6c757d');

INSERT INTO PERMISSION_ABSENCE (TypeAbsence, Description) VALUES
(N'Permission de sortie', N'Sortie pendant les heures de travail'),
(N'Retard justifié', N'Arrivée tardive autorisée'),
(N'Rendez-vous médical', N'Absence pour rendez-vous médical'),
(N'Raison familiale', N'Absence pour événement familial'),
(N'Démarche administrative', N'Absence pour démarche administrative');

INSERT INTO PORTEUSINE (Porte) VALUES
(N'Porte A - Entrée Principal'),
(N'Porte B - Entrée GSF'),
(N'Porte C - Entrée POI'),
(N'Porte D - Entrée Technique'),
(N'Porte E - Entrée ERG PACKAGING');

INSERT INTO JOURS_FERIES (Nom, DateJour, TypeJour, IsPaid, IsRecurrent, RecurringType, Description)
SELECT h.Nom, DATEFROMPARTS(y.Annee, h.M, h.D), 'FERIE', 1, 1, 'YEARLY', h.Descr
FROM (VALUES (2025), (2026)) y(Annee)
CROSS JOIN (VALUES
    (N'Nouvel An', 1, 1, N'Jour de l''An - Calendrier Grégorien'),
    (N'Manifeste de l''Indépendance', 1, 11, N'Commémoration du Manifeste de l''Indépendance (1944)'),
    (N'Fête du Travail', 5, 1, N'Journée Internationale des Travailleurs'),
    (N'Fête du Trône', 7, 30, N'Anniversaire de l''intronisation de Sa Majesté le Roi Mohammed VI'),
    (N'Journée de Oued Ed-Dahab', 8, 14, N'Commémoration de la récupération de Oued Ed-Dahab (1979)'),
    (N'Révolution du Roi et du Peuple', 8, 20, N'Commémoration de la Révolution du Roi Mohammed V et du Peuple'),
    (N'Fête de la Jeunesse', 8, 21, N'Anniversaire de Sa Majesté le Roi Mohammed VI'),
    (N'Marche Verte', 11, 6, N'Commémoration de la Marche Verte (1975)'),
    (N'Fête de l''Indépendance', 11, 18, N'Commémoration de l''Indépendance du Maroc (1956)')
) h(Nom, M, D, Descr);
GO

-- =====================================================================
-- 8. ROLES, PAGES, PERMISSIONS
-- =====================================================================
INSERT INTO ROLES (RoleName, Description) VALUES
(N'Admin', N'Administrateur système - Accès complet'),
(N'Manager', N'Gestionnaire - Accès aux fonctions de gestion'),
(N'HR', N'Ressources Humaines - Gestion du personnel'),
(N'Chef_Equipe', N'Chef d''équipe - Gestion des pointages'),
(N'Employe', N'Employé standard - Accès limité'),
(N'Pointage Porte', N'Agent de sécurité - Pointage aux portes'); -- name checked in connexion.aspx.cs

INSERT INTO PAGES (PageName, PageURL, MenuGroup) VALUES
(N'Dashboard HRM', '~/Default.aspx', 'Dashboard'),
(N'Rapport Pointage Port', '~/reporting/RaportPointagePort.aspx', 'Dashboard'),
(N'Ajouter Employé', '~/personnel/AjouterEmploye.aspx', 'Personnel'),
(N'Liste des Employés', '~/personnel/ListeEmployes.aspx', 'Personnel'),
(N'Editer Employé', '~/personnel/EditEmployee.aspx', 'Personnel'),
(N'Assigner Employés', '~/pointage/AssignerEmployes.aspx', 'Pointage'),
(N'Créer Chef d''Équipe', '~/pointage/CreerChefEquipe.aspx', 'Pointage'),
(N'Assignation de Groupe', '~/pointage/TransfertAssignation.aspx', 'Pointage'),
(N'Mon Équipe', '~/pointage/MonEquipe.aspx', 'Pointage'),
(N'Pointage Production', '~/pointage/PointageProduction.aspx', 'Pointage'),
(N'Pointage Équipe', '~/pointage/PointageEquipe.aspx', 'Pointage'),
(N'Contrôle Pointage', '~/pointage/SupervisionPointage.aspx', 'Pointage'),
(N'Heures Supplémentaires', '~/pointage/PointageHeuresSupp.aspx', 'Pointage'),
(N'Assigner Employés Ports', '~/pointagfix/AssignerEmployesPorts.aspx', 'Pointage Fix'),
(N'Pointage Équipe Ports', '~/pointagfix/PointageEmployesPorts.aspx', 'Pointage Fix'),
(N'Editer Pointage Port', '~/pointagfix/EditPointagePort.aspx', 'Pointage Fix'),
(N'Pointage Mobile', '~/PointageMobile/PointageMobile.aspx', 'Pointage Fix'),
(N'Congés Employés', '~/conge/conge-employes.aspx', 'Congés'),
(N'Permissions d''Absence', '~/conge/absence-permissions.aspx', 'Congés'),
(N'Jours Fériés', '~/conge/GestionJoursFeries.aspx', 'Congés'),
(N'Rapport Employé Pointage', '~/reporting/RaportEmploiyerPointage.aspx', 'Rapports'),
(N'Impression Rapport Employé', '~/reporting/EmployeePrintReport.aspx', 'Rapports'),
(N'Gestion des Rôles', '~/permissions/RollesManagment.aspx', 'Permissions'),
(N'Gestion des Pages', '~/permissions/PagesManagment.aspx', 'Permissions'),
(N'Permissions Rôle-Pages', '~/permissions/RollePage.aspx', 'Permissions'),
(N'Attribution des Rôles', '~/permissions/RolleEmployer.aspx', 'Permissions'),
(N'Gestion Mots de Passe', '~/permissions/GestionMotsDePasse.aspx', 'Permissions'),
(N'Mon Profil', '~/permissions/Profile.aspx', 'Permissions');

INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r CROSS JOIN PAGES p
WHERE r.RoleName = N'Admin'
   OR p.PageURL IN ('~/Default.aspx', '~/permissions/Profile.aspx')
   OR (r.RoleName = N'Manager' AND (p.MenuGroup IN ('Dashboard', 'Personnel', 'Pointage', 'Congés', 'Rapports')
                                    OR p.PageURL = '~/permissions/RolleEmployer.aspx'))
   OR (r.RoleName = N'HR' AND p.MenuGroup IN ('Dashboard', 'Personnel', 'Congés', 'Rapports'))
   OR (r.RoleName = N'Chef_Equipe' AND p.PageURL IN ('~/pointage/MonEquipe.aspx', '~/pointage/PointageEquipe.aspx',
                                                     '~/pointage/PointageHeuresSupp.aspx'))
   OR (r.RoleName = N'Pointage Porte' AND p.MenuGroup = 'Pointage Fix');
GO

-- =====================================================================
-- 9. FIRST ADMIN ACCOUNT  (login = Email, password compared as-is)
--    >>> change the password after first login <<<
-- =====================================================================
INSERT INTO EMPLOYE (IdSociete, Matricule, Nom, Prenom, Email, DateEmbauche, TypeEmploye,
                     Rolle, PasswordHash, ModifiePar)
VALUES (1, 'ADMIN', N'Administrateur', N'Système', 'admin@gmail.com', CAST(GETDATE() AS DATE), N'ADMINISTRATIF',
        N'Admin', 'ChangeMe@2026', 'System');

INSERT INTO AccessLogine (IdEmploye, IDPorteAssigne)
SELECT e.IdEmploye, p.idPorte FROM EMPLOYE e CROSS JOIN PORTEUSINE p WHERE e.Matricule = 'ADMIN';
GO

PRINT 'ERP_RH_Pointag_V2 created.';
GO
