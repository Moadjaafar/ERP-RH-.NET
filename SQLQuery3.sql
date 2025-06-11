CREATE DATABASE ERP_RH_Pointag_V2;
use ERP_RH_Pointag_V2

-- Sociétés du groupe
CREATE TABLE SOCIETE (
    IdSociete INT IDENTITY(1,1) PRIMARY KEY,
    NomSociete NVARCHAR(150) NOT NULL,
    CodeSociete NVARCHAR(10) UNIQUE NOT NULL,
    Adresse NVARCHAR(250),
    Telephone NVARCHAR(20),
    Email NVARCHAR(100),
    RegistreCommerce NVARCHAR(50),
    ICE NVARCHAR(20),
    DateCreation DATETIME DEFAULT GETDATE(),
    Actif BIT DEFAULT 1
);

-- Départements (communs à toutes les sociétés)
CREATE TABLE DEPARTEMENT (
    IdDepartement INT IDENTITY(1,1) PRIMARY KEY,
    NomDepartement NVARCHAR(100) NOT NULL,
    CodeDepartement NVARCHAR(10) UNIQUE,
    Description NVARCHAR(250),
    DateCreation DATETIME DEFAULT GETDATE(),
    Actif BIT DEFAULT 1,
	IdSociete INT NOT NULL,
    FOREIGN KEY (IdSociete) REFERENCES SOCIETE(IdSociete)
);

-- Services par département
CREATE TABLE SERVICE (
    IdService INT IDENTITY(1,1) PRIMARY KEY,
    IdDepartement INT NOT NULL,
    NomService NVARCHAR(100) NOT NULL,
    CodeService NVARCHAR(10),
    ResponsableService NVARCHAR(100),
    DateCreation DATETIME DEFAULT GETDATE(),
    Actif BIT DEFAULT 1,
    FOREIGN KEY (IdDepartement) REFERENCES DEPARTEMENT(IdDepartement)
);

-- Cellules par service
CREATE TABLE CELLULE (
    IdCellule INT IDENTITY(1,1) PRIMARY KEY,
    IdService INT NOT NULL,
    NomCellule NVARCHAR(100) NOT NULL,
    CodeCellule NVARCHAR(10),
    Description NVARCHAR(250),
    DateCreation DATETIME DEFAULT GETDATE(),
    Actif BIT DEFAULT 1,
    FOREIGN KEY (IdService) REFERENCES SERVICE(IdService)
);

-- Fonctions par service et cellule
CREATE TABLE FONCTION (
    IdFonction INT IDENTITY(1,1) PRIMARY KEY,
    IdService INT NOT NULL,
    IdCellule INT NOT NULL,
    NomFonction NVARCHAR(100) NOT NULL,
    CodeFonction NVARCHAR(10),
    Description NVARCHAR(250),
    NiveauResponsabilite INT DEFAULT 1, -- 1=Ouvrier, 2=Superviseur, 3=Chef
    DateCreation DATETIME DEFAULT GETDATE(),
    Actif BIT DEFAULT 1,
    FOREIGN KEY (IdService) REFERENCES SERVICE(IdService),
    FOREIGN KEY (IdCellule) REFERENCES CELLULE(IdCellule)
);


select * from EMPLACEMENT
-- Emplacements par société
CREATE TABLE EMPLACEMENT (
    IdEmplacement INT IDENTITY(1,1) PRIMARY KEY,
    IdSociete INT NOT NULL,
    NomEmplacement NVARCHAR(100) NOT NULL,
    TypeEmplacement NVARCHAR(50), -- Usine, Bureau, Entrepôt
    Adresse NVARCHAR(250),
    Superficie DECIMAL(10,2),
    DateCreation DATETIME DEFAULT GETDATE(),
    Actif BIT DEFAULT 1,
    FOREIGN KEY (IdSociete) REFERENCES SOCIETE(IdSociete)
);

-- ============================================
-- 2. TABLE EMPLOYÉ UNIQUE 
-- ============================================
 
-- Drop existing EMPLOYE table and its constraints

select * from EMPLOYE

-- Create EMPLOYE table with all latest updates
CREATE TABLE EMPLOYE (
    IdEmploye INT IDENTITY(1,1) PRIMARY KEY,
    
    -- LIAISON SOCIÉTÉ (Clé étrangère)
    IdSociete INT NOT NULL,
    
    -- INFORMATIONS PERSONNELLES
    Matricule NVARCHAR(20) NOT NULL, -- Peut être généré par société
    Nom NVARCHAR(150) NOT NULL,
    Prenom NVARCHAR(150) NOT NULL,
    DateNaissance DATE,
    LieuNaissance NVARCHAR(100),
    CinPassport NVARCHAR(50) UNIQUE,
    Sexe NVARCHAR(10) CHECK (Sexe IN ('Homme', 'Femme')),
    SituationFamiliale NVARCHAR(50),
    NombreEnfants INT DEFAULT 0,
    Adresse NVARCHAR(250),
    Telephone NVARCHAR(20),
    Email NVARCHAR(100),
    Nationalite NVARCHAR(100),
    
    -- FORMATION
    NiveauScolaire NVARCHAR(100),
    Diplome NVARCHAR(150),
    
    -- INFORMATIONS PROFESSIONNELLES
    DateEmbauche DATE NOT NULL,
    DateSortie DATE NULL, -- Null si encore actif
    TypeContrat NVARCHAR(50),
    TypeEmploye NVARCHAR(50) NOT NULL,
    IdTypeOuv INT NULL, -- Foreign key to TYPEOUVRIER table
    
    -- AUTHENTIFICATION
    Rolle NVARCHAR(50) NULL,
    PasswordHash NVARCHAR(255) NULL, -- Hash sécurisé
    
    -- AFFECTATION ORGANISATIONNELLE (Clés étrangères)
    IdDepartement INT,
    IdService INT,
    IdCellule INT,
    IdFonction INT,
    IdEmplacement INT,
    
    -- PLANIFICATION
    Shift NVARCHAR(10) CHECK (Shift IN ('JOUR', 'NUIT')),
    
    -- CNSS
    SituationCNSS NVARCHAR(5) CHECK (SituationCNSS IN ('D', 'ND')), -- Déclaré/Non Déclaré
    NumeroCNSS NVARCHAR(50),
    
    -- STATUT
    is_deleted BIT DEFAULT 0,
    DateCreation DATETIME DEFAULT GETDATE(),
    DateModification DATETIME,
    ModifiePar NVARCHAR(100),
    
    -- CONTRAINTES DE CLÉS ÉTRANGÈRES
    FOREIGN KEY (IdSociete) REFERENCES SOCIETE(IdSociete),
    FOREIGN KEY (IdDepartement) REFERENCES DEPARTEMENT(IdDepartement),
    FOREIGN KEY (IdService) REFERENCES SERVICE(IdService),
    FOREIGN KEY (IdCellule) REFERENCES CELLULE(IdCellule),
    FOREIGN KEY (IdFonction) REFERENCES FONCTION(IdFonction),
    FOREIGN KEY (IdEmplacement) REFERENCES EMPLACEMENT(IdEmplacement),
    FOREIGN KEY (IdTypeOuv) REFERENCES TYPEOUVRIER(IdTypeOuv),
    
    -- CONTRAINTES MÉTIER
    CONSTRAINT UK_Employe_Matricule_Societe UNIQUE (Matricule, IdSociete),
    CONSTRAINT CK_DateSortie CHECK (DateSortie IS NULL OR DateSortie >= DateEmbauche)
);

-- Create TypeOuvrier table
CREATE TABLE TYPEOUVRIER (
    IdTypeOuv INT IDENTITY(1,1) PRIMARY KEY,
    NomTypeOuv NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(250),
    Actif BIT DEFAULT 1,
    DateCreation DATETIME DEFAULT GETDATE()
);

-- Insert the type ouvrier data
INSERT INTO TYPEOUVRIER (NomTypeOuv, Description) VALUES
('MOD', 'Main d''œuvre directe'),
('MOI', 'Main d''œuvre indirecte'),
('Productif', 'Ouvrier productif'),
('Structure', 'Ouvrier de structure');
-- Historique des modifications employés
-- Insertion des sociétés
INSERT INTO SOCIETE (NomSociete, CodeSociete, Adresse, Telephone, Email, RegistreCommerce, ICE) VALUES
('GOLD SEA FISH', 'GSF', 'Zone Industrielle Dakhla', '+212-528-123456', 'contact@goldseafish.ma', 'RC12345', 'ICE001'),
('ERG CONSERVE', 'ERG', 'Zone Portuaire Dakhla', '+212-528-654321', 'info@ergconserve.ma', 'RC67890', 'ICE002'),
('PROTEIN AND OIL', 'PRO', 'Zone Industrielle Agadir', '+212-528-987654', 'contact@proteinoil.ma', 'RC54321', 'ICE003');

-- Insert departments for GOLD SEA FISH (IdSociete = 1)
INSERT INTO DEPARTEMENT (NomDepartement, CodeDepartement, Description, IdSociete) VALUES
('PRODUCTION', 'PROD-GSF', 'Département de production - Gold Sea Fish', 1),
('QUALITE', 'QUAL-GSF', 'Département qualité - Gold Sea Fish', 1),
('RESSOURCES HUMAINES', 'RH-GSF', 'Département ressources humaines - Gold Sea Fish', 1),
('TECHNIQUE', 'TECH-GSF', 'Département technique - Gold Sea Fish', 1),
('STOCK', 'STOCK-GSF', 'Département stock - Gold Sea Fish', 1);

-- Insert departments for ERG CONSERVE (IdSociete = 2)
INSERT INTO DEPARTEMENT (NomDepartement, CodeDepartement, Description, IdSociete) VALUES
('PRODUCTION', 'PROD-ERG', 'Département de production - ERG Conserve', 2),
('QUALITE', 'QUAL-ERG', 'Département qualité - ERG Conserve', 2),
('RESSOURCES HUMAINES', 'RH-ERG', 'Département ressources humaines - ERG Conserve', 2),
('TECHNIQUE', 'TECH-ERG', 'Département technique - ERG Conserve', 2),
('STOCK', 'STOCK-ERG', 'Département stock - ERG Conserve', 2);

-- Insert departments for PROTEIN AND OIL (IdSociete = 3)
INSERT INTO DEPARTEMENT (NomDepartement, CodeDepartement, Description, IdSociete) VALUES
('PRODUCTION', 'PROD-PRO', 'Département de production - Protein and Oil', 3),
('QUALITE', 'QUAL-PRO', 'Département qualité - Protein and Oil', 3),
('RESSOURCES HUMAINES', 'RH-PRO', 'Département ressources humaines - Protein and Oil', 3),
('TECHNIQUE', 'TECH-PRO', 'Département technique - Protein and Oil', 3),
('STOCK', 'STOCK-PRO', 'Département stock - Protein and Oil', 3);


CREATE TABLE AUDIT_EMPLOYE (
    IdAudit INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye INT NOT NULL,
    TypeOperation NVARCHAR(20) NOT NULL, -- INSERT, UPDATE, DELETE
    ChampModifie NVARCHAR(100),
    AncienneValeur NVARCHAR(500),
    NouvelleValeur NVARCHAR(500),
    ModifiePar NVARCHAR(100) NOT NULL,
    DateModification DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (IdEmploye) REFERENCES EMPLOYE(IdEmploye)
);




-- First, let's create a mapping of services to department names

DECLARE @IdDeptProduction_GSF INT, @IdDeptQualite_GSF INT, @IdDeptRH_GSF INT, @IdDeptTechnique_GSF INT, @IdDeptStock_GSF INT;

SELECT @IdDeptProduction_GSF = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'PRODUCTION' AND IdSociete = 1;
SELECT @IdDeptQualite_GSF = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'QUALITE' AND IdSociete = 1;
SELECT @IdDeptRH_GSF = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'RESSOURCES HUMAINES' AND IdSociete = 1;
SELECT @IdDeptTechnique_GSF = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'TECHNIQUE' AND IdSociete = 1;
SELECT @IdDeptStock_GSF = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'STOCK' AND IdSociete = 1;

-- Insert Production services for GSF
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptProduction_GSF, 'CONTRÔLE', 'CTRL-GSF'),
(@IdDeptProduction_GSF, 'DEMOULAGE', 'DMOU-GSF'),
(@IdDeptProduction_GSF, 'EMBALLAGE', 'EMBL-GSF'),
(@IdDeptProduction_GSF, 'MANUTENTION', 'MANU-GSF'),
(@IdDeptProduction_GSF, 'RECEPTION', 'RECP-GSF'),
(@IdDeptProduction_GSF, 'SUPPORT', 'SUPP-GSF'),
(@IdDeptProduction_GSF, 'TRAITEMENT', 'TRAI-GSF'),
(@IdDeptProduction_GSF, 'TRIAGE', 'TRIG-GSF');

-- Insert Qualite services for GSF
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptQualite_GSF, 'HYGIENE', 'HYG-GSF');

-- Insert RH services for GSF
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptRH_GSF, 'BIANDRIE', 'BIAN-GSF'),
(@IdDeptRH_GSF, 'MOYENS GENERAUX', 'MGEN-GSF'),
(@IdDeptRH_GSF, 'RESTAURATION', 'REST-GSF');

-- Insert Technique services for GSF
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptTechnique_GSF, 'MAINTENANCE MACHINE', 'MACH-GSF');

-- Insert Stock services for GSF
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptStock_GSF, 'STOCK', 'STCK-GSF');

-- For ERG CONSERVE (assuming IdSociete = 2)
DECLARE @IdDeptProduction_ERG INT, @IdDeptQualite_ERG INT, @IdDeptRH_ERG INT, @IdDeptTechnique_ERG INT, @IdDeptStock_ERG INT;

SELECT @IdDeptProduction_ERG = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'PRODUCTION' AND IdSociete = 2;
SELECT @IdDeptQualite_ERG = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'QUALITE' AND IdSociete = 2;
SELECT @IdDeptRH_ERG = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'RESSOURCES HUMAINES' AND IdSociete = 2;
SELECT @IdDeptTechnique_ERG = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'TECHNIQUE' AND IdSociete = 2;
SELECT @IdDeptStock_ERG = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'STOCK' AND IdSociete = 2;

-- Insert Production services for ERG
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptProduction_ERG, 'CONTRÔLE', 'CTRL-ERG'),
(@IdDeptProduction_ERG, 'DEMOULAGE', 'DMOU-ERG'),
(@IdDeptProduction_ERG, 'EMBALLAGE', 'EMBL-ERG'),
(@IdDeptProduction_ERG, 'MANUTENTION', 'MANU-ERG'),
(@IdDeptProduction_ERG, 'RECEPTION', 'RECP-ERG'),
(@IdDeptProduction_ERG, 'SUPPORT', 'SUPP-ERG'),
(@IdDeptProduction_ERG, 'TRAITEMENT', 'TRAI-ERG'),
(@IdDeptProduction_ERG, 'TRIAGE', 'TRIG-ERG');

-- Insert Qualite services for ERG
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptQualite_ERG, 'HYGIENE', 'HYG-ERG');

-- Insert RH services for ERG
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptRH_ERG, 'BIANDRIE', 'BIAN-ERG'),
(@IdDeptRH_ERG, 'MOYENS GENERAUX', 'MGEN-ERG'),
(@IdDeptRH_ERG, 'RESTAURATION', 'REST-ERG');

-- Insert Technique services for ERG
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptTechnique_ERG, 'MAINTENANCE MACHINE', 'MACH-ERG');

-- Insert Stock services for ERG
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptStock_ERG, 'STOCK', 'STCK-ERG');

-- For PROTEIN AND OIL (assuming IdSociete = 3)
DECLARE @IdDeptProduction_PRO INT, @IdDeptQualite_PRO INT, @IdDeptRH_PRO INT, @IdDeptTechnique_PRO INT, @IdDeptStock_PRO INT;

SELECT @IdDeptProduction_PRO = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'PRODUCTION' AND IdSociete = 3;
SELECT @IdDeptQualite_PRO = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'QUALITE' AND IdSociete = 3;
SELECT @IdDeptRH_PRO = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'RESSOURCES HUMAINES' AND IdSociete = 3;
SELECT @IdDeptTechnique_PRO = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'TECHNIQUE' AND IdSociete = 3;
SELECT @IdDeptStock_PRO = IdDepartement FROM DEPARTEMENT WHERE NomDepartement = 'STOCK' AND IdSociete = 3;

-- Insert Production services for PRO
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptProduction_PRO, 'CONTRÔLE', 'CTRL-PRO'),
(@IdDeptProduction_PRO, 'DEMOULAGE', 'DMOU-PRO'),
(@IdDeptProduction_PRO, 'EMBALLAGE', 'EMBL-PRO'),
(@IdDeptProduction_PRO, 'MANUTENTION', 'MANU-PRO'),
(@IdDeptProduction_PRO, 'RECEPTION', 'RECP-PRO'),
(@IdDeptProduction_PRO, 'SUPPORT', 'SUPP-PRO'),
(@IdDeptProduction_PRO, 'TRAITEMENT', 'TRAI-PRO'),
(@IdDeptProduction_PRO, 'TRIAGE', 'TRIG-PRO');

-- Insert Qualite services for PRO
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptQualite_PRO, 'HYGIENE', 'HYG-PRO');

-- Insert RH services for PRO
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptRH_PRO, 'BIANDRIE', 'BIAN-PRO'),
(@IdDeptRH_PRO, 'MOYENS GENERAUX', 'MGEN-PRO'),
(@IdDeptRH_PRO, 'RESTAURATION', 'REST-PRO');

-- Insert Technique services for PRO
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptTechnique_PRO, 'MAINTENANCE MACHINE', 'MACH-PRO');

-- Insert Stock services for PRO
INSERT INTO SERVICE (IdDepartement, NomService, CodeService) VALUES
(@IdDeptStock_PRO, 'STOCK', 'STCK-PRO');

-- Verify the inserted data
SELECT * FROM EMPLOYE 
UPDATE EMPLOYE 
SET Email = 'admin@gmail.com'
WHERE IdEmploye = 1;

INSERT INTO CELLULE (NomCellule, IdService) VALUES 
(N'CONTRÔLE GUANO', 1),
(N'CONTRÔLE MACHINE', 1),
(N'DEMOULAGE', 2),
(N'EMBALLAGE', 3),
(N'MENUISERIE', 4),
(N'TRANSPALETTE', 4),
(N'COUPAGE', 5),
(N'DECHARGE BASSIN', 5),
(N'LIVRAISON GLACE', 5),
(N'LIVRAISON SLURY', 5),
(N'PECHE COTIERE', 5),
(N'RECEPTION MATIERE PREMIERE', 5),
(N'collecte des informations de la prod', 6),
(N'SUIVI', 6),
(N'MACHINE', 7),
(N'CALIBREUSE', 8),
(N'TRIAGE', 8),
(N'HYGIENE', 9),
(N'BIANDRIE', 10),
(N'MOYENS GENERAUX', 11),
(N'RESTAURATION', 12),
(N'MAINTEANCE MACHINE', 13),
(N'STOCK', 14);


INSERT INTO EMPLACEMENT (NomEmplacement, IdSociete) VALUES
(N'GSF', 3),
(N'H E', 3),
(N'KT', 3),
(N'RH', 3),
(N'SALLE 03', 3),
(N'STOCK', 3),
(N'TECH', 3);

-- Insert multiple employees with same department and cellule
INSERT INTO EMPLOYE (
    IdSociete, Matricule, Nom, Prenom, DateNaissance, LieuNaissance, 
    CinPassport, Sexe, SituationFamiliale, NombreEnfants, Adresse, Telephone, 
    Email, Nationalite, NiveauScolaire, Diplome, DateEmbauche, DateSortie, 
    TypeContrat, TypeEmploye, IdTypeOuv, Rolle, PasswordHash, IdDepartement, 
    IdService, IdCellule, IdFonction, IdEmplacement, Shift, SituationCNSS, 
    NumeroCNSS, Actif, DateCreation, DateModification, ModifiePar
) VALUES 
-- Employee 2: Ahmed
(1, '2', 'Ahmed', 'Hassan', '1995-03-15', 'Rabat', 
'ah12345', 'Homme', 'Marié', 2, 'Hay Riad Rabat', '0612345678', 
NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 
'CDI', 'OUVRIER', 1, NULL, NULL, 1, 
3, 4, 7, NULL, 'JOUR', 'D', 
'029856', 1, GETDATE(), NULL, 'System'),

-- Employee 3: Fatima
(1, '3', 'Fatima', 'Benali', '1990-08-22', 'Casablanca', 
'ft67890', 'Femme', 'Mariée', 1, 'Ain Chock Casablanca', '0698765432', 
NULL, 'Marocaine', 'Licence', NULL, '2023-06-10', NULL, 
'CDI', 'OUVRIER', 1, NULL, NULL, 1, 
3, 4, 7, NULL, 'JOUR', 'D', 
'030147', 1, GETDATE(), NULL, 'System'),

-- Employee 4: Omar
(1, '4', 'Omar', 'Zaki', '1988-12-05', 'Fès', 
'oz11223', 'Homme', 'Célibataire', 0, 'Agdal Fès', '0656789123', 
NULL, 'Marocaine', 'Baccalauréat', NULL, '2023-09-01', NULL, 
'Temporaire', 'OUVRIER', 1, NULL, NULL, 1, 
3, 4, 7, NULL, 'JOUR', 'D', 
'031258', 1, GETDATE(), NULL, 'System'),

-- Employee 5: Khadija
(1, '5', 'Khadija', 'Alami', '1992-06-18', 'Marrakech', 
'kh44556', 'Femme', 'Célibataire', 0, 'Gueliz Marrakech', '0623456789', 
NULL, 'Marocaine', 'Primaire', NULL, '2024-03-20', NULL, 
'CDI', 'OUVRIER', 1, NULL, NULL, 1, 
3, 4, 7, NULL, 'JOUR', 'D', 
'032369', 1, GETDATE(), NULL, 'System'),

-- Employee 6: Youssef
(1, '6', 'Youssef', 'Idrissi', '1985-11-30', 'Tanger', 
'yi78901', 'Homme', 'Marié', 3, 'Malabata Tanger', '0687654321', 
NULL, 'Marocaine', 'BTS', NULL, '2022-12-01', NULL, 
'CDI', 'OUVRIER', 1, NULL, NULL, 1, 
3, 4, 7, NULL, 'JOUR', 'D', 
'033470', 1, GETDATE(), NULL, 'System'),

-- Employee 7: Aicha
(1, '7', 'Aicha', 'Chraibi', '1993-04-14', 'Agadir', 
'ac23456', 'Femme', 'Mariée', 2, 'Hay Mohammadi Agadir', '0634567890', 
NULL, 'Marocaine', 'Licence', NULL, '2024-05-15', NULL, 
'Temporaire', 'OUVRIER', 1, NULL, NULL, 1, 
3, 4, 7, NULL, 'JOUR', 'D', 
'034581', 1, GETDATE(), NULL, 'System'),

-- Employee 8: Rachid
(1, '8', 'Rachid', 'Berrada', '1987-09-08', 'Meknès', 
'rb34567', 'Homme', 'Marié', 1, 'Hamria Meknès', '0645678901', 
NULL, 'Marocaine', 'Baccalauréat', NULL, '2023-11-10', NULL, 
'CDI', 'OUVRIER', 1, NULL, NULL, 1, 
3, 4, 7, NULL, 'JOUR', 'D', 
'035692', 1, GETDATE(), NULL, 'System'),

-- Employee 9: Nadia
(1, '9', 'Nadia', 'Fassi', '1991-07-25', 'Salé', 
'nf45678', 'Femme', 'Célibataire', 0, 'Hay Karima Salé', '0656789012', 
NULL, 'Marocaine', 'Master', NULL, '2024-02-01', NULL, 
'CDI', 'OUVRIER', 1, NULL, NULL, 1, 
3, 4, 7, NULL, 'JOUR', 'D', 
'036703', 1, GETDATE(), NULL, 'System'),

-- Employee 10: Karim
(1, '10', 'Karim', 'Tazi', '1989-01-12', 'Oujda', 
'kt56789', 'Homme', 'Marié', 2, 'Hay Qods Oujda', '0667890123', 
NULL, 'Marocaine', 'Baccalauréat', NULL, '2023-08-15', NULL, 
'Temporaire', 'OUVRIER', 1, NULL, NULL, 1, 
3, 4, 7, NULL, 'JOUR', 'D', 
'037814', 1, GETDATE(), NULL, 'System');

-- relation chef equipe employer 
-- 1. Create CHEF_EQUIPE table
CREATE TABLE CHEF_EQUIPE (
    IdChefEquipe INT IDENTITY(1,1) PRIMARY KEY,
    
    -- Reference to employee who is chef equipe
    IdEmploye INT NOT NULL,
    
    -- Team information
    NomEquipe NVARCHAR(100) NOT NULL,
    
    -- Who assigned this chef equipe (chef salle)
    AssignePar INT NOT NULL, -- IdEmploye of chef salle
    
    -- Organization context
    IdSociete INT NOT NULL,
    IdDepartement INT NULL,
    
    -- Status and dates
    is_deleted BIT DEFAULT 0,
    DateAssignation DATE DEFAULT GETDATE(),
    DateCreation DATETIME DEFAULT GETDATE(),
    
    -- Foreign keys
    FOREIGN KEY (IdEmploye) REFERENCES EMPLOYE(IdEmploye),
    FOREIGN KEY (AssignePar) REFERENCES EMPLOYE(IdEmploye),
    FOREIGN KEY (IdSociete) REFERENCES SOCIETE(IdSociete),
    FOREIGN KEY (IdDepartement) REFERENCES DEPARTEMENT(IdDepartement),
    
    -- Constraints
    CONSTRAINT UK_ChefEquipe_Employe UNIQUE (IdEmploye) -- One chef equipe role per employee
);

-- 2. Create EMPLOYE_CHEF_EQUIPE table (Employee assignments to chef equipe)
CREATE TABLE EMPLOYE_CHEF_EQUIPE (
    IdEmployeChefEquipe INT IDENTITY(1,1) PRIMARY KEY,
    
    -- Employee assigned to chef equipe
    IdEmploye INT NOT NULL,
    
    -- Chef equipe they are assigned to
    IdChefEquipe INT NOT NULL,
    
    -- Who made the assignment (chef salle)
    AssignePar INT NOT NULL,
    
    -- Assignment details
    DateAssignation DATE DEFAULT GETDATE(),
    DateFinAssignation DATE NULL, -- NULL = still active
    
    -- Status
    is_deleted BIT DEFAULT 0,
    DateCreation DATETIME DEFAULT GETDATE(),
    
    -- Foreign keys
    FOREIGN KEY (IdEmploye) REFERENCES EMPLOYE(IdEmploye),
    FOREIGN KEY (IdChefEquipe) REFERENCES CHEF_EQUIPE(IdChefEquipe),
    FOREIGN KEY (AssignePar) REFERENCES EMPLOYE(IdEmploye),
    
    -- Constraints
    CONSTRAINT UK_EmployeChefEquipe_Active UNIQUE (IdEmploye, DateFinAssignation), -- One active assignment per employee
    CONSTRAINT CK_EmployeChefEquipe_DateFin CHECK (DateFinAssignation IS NULL OR DateFinAssignation >= DateAssignation)
);

-- 3. TABLE POINTAGE UNIQUE 

-- TABLE POINTAGE WITH CHEF_EQUIPE REFERENCE
CREATE TABLE POINTAGE (
    IdPointage INT IDENTITY(1,1) PRIMARY KEY,
    
    --  LIAISON EMPLOYÉ
    IdEmploye INT NOT NULL,
    
    --  INFORMATIONS POINTAGE
    DatePointage DATE NOT NULL,
    Shift NVARCHAR(10) CHECK (Shift IN ('JOUR', 'NUIT')),
    
    --  STATUT PRÉSENCE
    StatutPresence NVARCHAR(10) CHECK (StatutPresence IN ('P', 'A', 'P/2')), -- Présent, Absent, Demi-journée
    Coefficient DECIMAL(3,2) DEFAULT 1.00, -- 1.00=P, 0.00=A, 0.50=P/2
    
    --  HORAIRES (Null si absent)
    HeureEntree TIME NULL,
    HeureSortie TIME NULL,
    
    --  AFFECTATION DU JOUR (peut différer de l'affectation habituelle)
    IdDepartement INT NULL,
    IdService INT NULL,
    IdCellule INT NULL,
    IdFonction INT NULL,
    IdEmplacement INT NULL,
    
    --  RÉFÉRENCE CHEF D'ÉQUIPE (pour traçabilité et organisation)
    IdChefEquipe INT NULL, -- NULL si l'employé n'est pas assigné à une équipe ce jour-là
    
    --  MOTIF ABSENCE (si applicable)
    MotifAbsence NVARCHAR(100) NULL,
    
    --  TRAÇABILITÉ
    DateCreationPointage DATETIME DEFAULT GETDATE(),
    HeureCreationPointage TIME DEFAULT CAST(GETDATE() AS TIME),
    PointePar NVARCHAR(100) NOT NULL, -- Qui a fait le pointage
    
    --  CONTRAINTES
    FOREIGN KEY (IdEmploye) REFERENCES EMPLOYE(IdEmploye),
    FOREIGN KEY (IdDepartement) REFERENCES DEPARTEMENT(IdDepartement),
    FOREIGN KEY (IdService) REFERENCES SERVICE(IdService),
    FOREIGN KEY (IdCellule) REFERENCES CELLULE(IdCellule),
    FOREIGN KEY (IdFonction) REFERENCES FONCTION(IdFonction),
    FOREIGN KEY (IdEmplacement) REFERENCES EMPLACEMENT(IdEmplacement),
    FOREIGN KEY (IdChefEquipe) REFERENCES CHEF_EQUIPE(IdChefEquipe),
    
    --  CONTRAINTES MÉTIER
    CONSTRAINT UK_Pointage_Employe_Date UNIQUE (IdEmploye, DatePointage, Shift),
    CONSTRAINT CK_Horaires CHECK (
        (StatutPresence = 'A' AND HeureEntree IS NULL AND HeureSortie IS NULL) OR
        (StatutPresence IN ('P', 'P/2') AND HeureEntree IS NOT NULL)
    )
);

SELECT 
    e.IdEmploye,
    e.Matricule,
    e.Nom + ' ' + e.Prenom AS NomComplet,
    ce.IdChefEquipe,
    ce.NomEquipe,
    chef.Nom + ' ' + chef.Prenom AS NomChef,
    ISNULL(p.IdPointage, 0) AS IdPointage,
    CASE 
        WHEN p.IdPointage IS NULL THEN 'P'
        ELSE ISNULL(p.StatutPresence, 'P')
    END AS StatutPresence
FROM EMPLOYE_CHEF_EQUIPE ece
INNER JOIN EMPLOYE e ON ece.IdEmploye = e.IdEmploye AND e.is_deleted = 0
INNER JOIN CHEF_EQUIPE ce ON ece.IdChefEquipe = ce.IdChefEquipe AND ce.is_deleted = 0
INNER JOIN EMPLOYE chef ON ce.IdEmploye = chef.IdEmploye
LEFT JOIN POINTAGE p ON e.IdEmploye = p.IdEmploye 
    AND p.DatePointage = '2025-06-03'  -- Replace with your test date
WHERE ce.IdSociete = 1  -- Replace with your company ID
    AND ece.is_deleted = 0 
    AND (ece.DateFinAssignation IS NULL OR ece.DateFinAssignation >= '2025-06-05')
    AND ece.DateAssignation <= '2025-06-05'
ORDER BY ce.NomEquipe, e.Nom;

CREATE TABLE POINTAGE_HEURE_SUPP (
    IdHeureSupp INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye INT NOT NULL,
    DateHeureSupp DATE NOT NULL,
    HeureDebut TIME NOT NULL,
    HeureFin TIME NOT NULL,
    NombreHeures DECIMAL(5,2) NOT NULL,
    Coefficient DECIMAL(3,2) NOT NULL DEFAULT 1.00,
    Justification NVARCHAR(500) NULL,
    IdChefEquipe INT NOT NULL,
    SaisiPar NVARCHAR(100) NOT NULL,
    DateSaisie DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiePar NVARCHAR(100) NULL,
    DateModification DATETIME NULL,
    
    -- Constraints
    CONSTRAINT FK_POINTAGE_HEURE_SUPP_EMPLOYE 
        FOREIGN KEY (IdEmploye) REFERENCES EMPLOYE(IdEmploye),
    CONSTRAINT FK_POINTAGE_HEURE_SUPP_CHEF_EQUIPE 
        FOREIGN KEY (IdChefEquipe) REFERENCES CHEF_EQUIPE(IdChefEquipe),
    CONSTRAINT CK_POINTAGE_HEURE_SUPP_COEFFICIENT 
        CHECK (Coefficient IN (1.00, 1.25, 1.50, 2.00)),
    CONSTRAINT CK_POINTAGE_HEURE_SUPP_NOMBRE_HEURES 
        CHECK (NombreHeures > 0 AND NombreHeures <= 24),
    CONSTRAINT UQ_POINTAGE_HEURE_SUPP_EMPLOYE_DATE 
        UNIQUE (IdEmploye, DateHeureSupp)
);


-- Step 1: Alter HeureDebut to allow NULL values
ALTER TABLE POINTAGE_HEURE_SUPP 
ALTER COLUMN HeureDebut TIME NULL;

-- Step 2: Alter HeureFin to allow NULL values  
ALTER TABLE POINTAGE_HEURE_SUPP 
ALTER COLUMN HeureFin TIME NULL;


CREATE TABLE EMPLOYE_PORT (
    IdEmployePort INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye INT NOT NULL,
    NomPort VARCHAR(100) NOT NULL,
    AssignePar INT NULL,
    DateCreation DATETIME NOT NULL DEFAULT GETDATE(),
    DateModification DATETIME NULL,
    is_deleted BIT NOT NULL DEFAULT 0,
    
    -- Foreign key constraints
    CONSTRAINT FK_EMPLOYE_PORT_EMPLOYE FOREIGN KEY (IdEmploye) REFERENCES EMPLOYE(IdEmploye),
    CONSTRAINT FK_EMPLOYE_PORT_ASSIGNEUR FOREIGN KEY (AssignePar) REFERENCES EMPLOYE(IdEmploye)
);


-- Create EMPLOYE_POINTAGE table for time tracking at ports
CREATE TABLE EMPLOYE_POINTAGE (
    IdPointage INT IDENTITY(1,1) PRIMARY KEY,
    IdEmploye INT NOT NULL,
    NomPort VARCHAR(100) NOT NULL,
    TypePointage VARCHAR(10) NOT NULL, -- 'ENTREE' or 'SORTIE'
    DateHeure DATETIME NOT NULL,
    AssignePar INT NULL,
    DateCreation DATETIME NOT NULL DEFAULT GETDATE(),
    DateModification DATETIME NULL,
    is_deleted BIT NOT NULL DEFAULT 0,
    
    -- Foreign key constraints
    CONSTRAINT FK_EMPLOYE_POINTAGE_EMPLOYE FOREIGN KEY (IdEmploye) REFERENCES EMPLOYE(IdEmploye),
    CONSTRAINT FK_EMPLOYE_POINTAGE_ASSIGNEUR FOREIGN KEY (AssignePar) REFERENCES EMPLOYE(IdEmploye),
    
    -- Check constraint for TypePointage
    CONSTRAINT CK_EMPLOYE_POINTAGE_TYPE CHECK (TypePointage IN ('ENTREE', 'SORTIE'))
);

-- Optional: SQL Function to calculate working hours (more efficient for large datasets)
CREATE FUNCTION dbo.fn_CalculateWorkingHours(@IdEmploye INT, @Date DATE)
RETURNS VARCHAR(10)
AS
BEGIN
    DECLARE @TotalMinutes INT = 0;
    DECLARE @Result VARCHAR(10);
    
    -- Get all pointages for the employee on the specified date
    DECLARE @Entrees TABLE (DateHeure DATETIME);
    DECLARE @Sorties TABLE (DateHeure DATETIME);
    
    INSERT INTO @Entrees 
    SELECT DateHeure 
    FROM EMPLOYE_POINTAGE 
    WHERE IdEmploye = @IdEmploye 
    AND CAST(DateHeure AS DATE) = @Date 
    AND TypePointage = 'ENTREE' 
    AND is_deleted = 0
    ORDER BY DateHeure;
    
    INSERT INTO @Sorties 
    SELECT DateHeure 
    FROM EMPLOYE_POINTAGE 
    WHERE IdEmploye = @IdEmploye 
    AND CAST(DateHeure AS DATE) = @Date 
    AND TypePointage = 'SORTIE' 
    AND is_deleted = 0
    ORDER BY DateHeure;
    
    -- Calculate working time by pairing entries and exits
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
        BEGIN
            SET @TotalMinutes = @TotalMinutes + DATEDIFF(MINUTE, @EntreeHeure, @SortieHeure);
        END
        
        FETCH NEXT FROM entree_cursor INTO @EntreeHeure;
    END
    
    CLOSE entree_cursor;
    CLOSE sortie_cursor;
    DEALLOCATE entree_cursor;
    DEALLOCATE sortie_cursor;
    
    -- Format result as "Xh00"
    DECLARE @Hours INT = @TotalMinutes / 60;
    DECLARE @Minutes INT = @TotalMinutes % 60;
    
    SET @Result = CAST(@Hours AS VARCHAR(2)) + 'h' + RIGHT('00' + CAST(@Minutes AS VARCHAR(2)), 2);
    
    RETURN @Result;
END;