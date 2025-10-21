CREATE DATABASE ERP_RH_Pointag_V2;
use ERP_RH_Pointag_V2


select * from EMPLOYE where Nom = 'jaafar'
UPDATE EMPLOYE 
SET Rolle = 'Dev',
    DateModification = GETDATE(),
    ModifiePar = 'moad'
WHERE IdEmploye = 55;


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
Delete EMPLOYE
select * from CELLULE

-- Add new column without unique constraint
ALTER TABLE EMPLOYE ADD CinPassport_New NVARCHAR(50);

-- Copy data from old column to new column
UPDATE EMPLOYE SET CinPassport_New = CinPassport;

-- Drop old column (this removes the unique constraint)
ALTER TABLE EMPLOYE DROP COLUMN CinPassport;
ALTER TABLE EMPLOYE DROP CONSTRAINT UQ__EMPLOYE__2671843439E79919;

-- Rename new column to original name
EXEC sp_rename 'EMPLOYE.CinPassport_New', 'CinPassport', 'COLUMN';
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
INSERT INTO SOCIETE (NomSociete, CodeSociete, Adresse, Telephone, Email, RegistreCommerce, ICE) VALUES
('BENIS PELAGIQUE', 'BPG', 'Zone Portuaire Dakhla', '+212-528-111333', 'contact@benispelagique.ma', 'RC11111', 'ICE004'),
('ERG PACKAGING', 'EPK', 'Zone Industrielle Dakhla', '+212-528-222444', 'info@ergpackaging.ma', 'RC22222', 'ICE005'),
('LOGISTIQUE & TRAITEMENT', 'LGT', 'Zone Logistique Dakhla', '+212-528-333555', 'contact@logistiquetraitement.ma', 'RC33333', 'ICE006'),
('BENIS PECHE', 'BPC', 'Port de Pêche Dakhla', '+212-528-444666', 'info@benispeche.ma', 'RC44444', 'ICE007'),
('ERG DELICE', 'EDL', 'Zone Commerciale Dakhla', '+212-528-555777', 'contact@ergdelice.ma', 'RC55555', 'ICE008'),
('PAOI', 'PAO', 'Zone Industrielle Agadir', '+212-528-666888', 'info@paoi.ma', 'RC66666', 'ICE009');
INSERT INTO SOCIETE (NomSociete, CodeSociete, Adresse, Telephone, Email, RegistreCommerce, ICE) VALUES
('PROTEIN PELAGIC', 'PPG', 'Zone Portuaire Agadir', '+212-528-777999', 'contact@proteinpelagic.ma', 'RC77777', 'ICE010');

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


select * from FONCTION

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

select * from SERVICE

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

select * from FONCTION
-- INSERT statements for FONCTION table (IdFonction will be auto-generated)
INSERT INTO FONCTION (IdService, IdCellule, NomFonction, CodeFonction, Description, NiveauResponsabilite, DateCreation, Actif) VALUES
(1, 1, 'ouvrier(e) de contrôle guano', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(1, 2, 'ouvrier(e) de contrôle M', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(1, 2, 'chef d equipe Controle', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(2, 3, 'ouvrier(e) de demoulage', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(3, 4, 'chargé(e) de collecte des informations de la prod', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(3, 4, 'ouvrier(e) de coupage', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(3, 4, 'ouvrier(e) d emballage', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(3, 4, 'chef d equipe emballage', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(4, 5, 'ouvrier(e) de menuiserie', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(4, 6, 'ouvrier(e) de transpalette emballage', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(4, 6, 'ouvrier(e) de traspalette demoulage', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(4, 6, 'ouvrier(e) de traspalette salle', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(5, 7, 'ouvrier(e) de coupage', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(5, 8, 'ouvrier(e) de décharge bassins', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(5, 9, 'ouvrier(e) de glace', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(5, 10, 'ouvrier(e) de slaurry', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(5, 11, 'AGENT DE PECHE COTIERE', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(5, 12, 'agent de réception MP', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(6, 13, 'chargé(e) de collecte des informations de la prod', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(6, 14, 'AGENT SUIVI DES OUVRIERE', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(7, 15, 'chef d equipe contrôle', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(7, 15, 'ouvrier(e) de machine', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(8, 16, 'ouvrier(e) de calibreuse', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(8, 17, 'ouvrier(e) de triage', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(9, 18, 'agent d hygiene', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(9, 18, 'ouvrier(e) de menage', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(10, 19, 'AGENT DE BIANDRIE', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(11, 20, 'ouvrier(e) de menage RH', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(11, 20, 'ouvrier(e) de menage externe', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(12, 21, 'agent de cantine', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(13, 22, 'TECHNICIEN DE MACHINES', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1),
(14, 23, 'agent de fermeture porte chambre froide', NULL, NULL, 1, '2025-06-01 16:19:34.527', 1);



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


SELECT * FROM EMPLOYE


SELECT IdSociete, NomSociete FROM SOCIETE WHERE Actif = 1 ORDER BY NomSociete

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
select * from EMPLOYE where Nom='jaafar'


INSERT INTO EMPLOYE (
    IdSociete, Matricule, Nom, Prenom, DateNaissance, LieuNaissance, 
    CinPassport, Sexe, SituationFamiliale, NombreEnfants, Adresse, Telephone, 
    Email, Nationalite, NiveauScolaire, Diplome, DateEmbauche, DateSortie, 
    TypeContrat, TypeEmploye, IdTypeOuv, Rolle, PasswordHash, IdDepartement, 
    IdService, IdCellule, IdFonction, IdEmplacement, Shift, SituationCNSS, 
    NumeroCNSS, is_deleted, DateCreation, DateModification, ModifiePar
) VALUES
(2, '00005', 'LAMSOUGUER', 'WADIE', '1990-01-01', 'Dakhla', 'CN00005', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00005', 0, GETDATE(), NULL, 'System'),
(2, '00030', 'MAZADI', 'ABDELMAJID', '1990-01-01', 'Dakhla', 'CN00030', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00030', 0, GETDATE(), NULL, 'System'),
(2, '00033', 'GARMAZ', 'BRAHIM', '1990-01-01', 'Dakhla', 'CN00033', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00033', 0, GETDATE(), NULL, 'System'),
(2, '00034', 'NAJAH', 'ZAHRA', '1990-01-01', 'Dakhla', 'CN00034', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00034', 0, GETDATE(), NULL, 'System'),
(2, '00055', 'EL OUADI', 'LEILA', '1990-01-01', 'Dakhla', 'CN00055', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00055', 0, GETDATE(), NULL, 'System'),
(2, '00059', 'AZHAF', 'LINA', '1990-01-01', 'Dakhla', 'CN00059', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00059', 0, GETDATE(), NULL, 'System'),
(2, '00084', 'HGUIG', 'MOHAMED', '1990-01-01', 'Dakhla', 'CN00084', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00084', 0, GETDATE(), NULL, 'System'),
(2, '00107', 'AGUSTIN', 'VASQUEZ', '1990-01-01', 'Dakhla', 'CN00107', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00107', 0, GETDATE(), NULL, 'System'),
(2, '00109', 'ENNOURI', 'SALOUA', '1990-01-01', 'Dakhla', 'CN00109', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00109', 0, GETDATE(), NULL, 'System'),
(2, '00117', 'CHAFIKI', 'ICAM', '1990-01-01', 'Dakhla', 'CN00117', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00117', 0, GETDATE(), NULL, 'System'),
(2, '00147', 'DIAN', 'BRAHIM', '1990-01-01', 'Dakhla', 'CN00147', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00147', 0, GETDATE(), NULL, 'System'),
(2, '00165', 'EL AYSSARI', 'FATIMA ZAHRA', '1990-01-01', 'Dakhla', 'CN00165', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00165', 0, GETDATE(), NULL, 'System'),
(2, '00252', 'ELGHARBAOUI', 'MUSTAPHA', '1990-01-01', 'Dakhla', 'CN00252', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00252', 0, GETDATE(), NULL, 'System'),
(2, '00272', 'BENRQUIA', 'NAWFAL', '1990-01-01', 'Dakhla', 'CN00272', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00272', 0, GETDATE(), NULL, 'System'),
(2, '00735', 'EL HIHI', 'HADIA', '1990-01-01', 'Dakhla', 'CN00735', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00735', 0, GETDATE(), NULL, 'System'),
(2, '01163', 'ELHOUARI', 'WIJDANE', '1990-01-01', 'Dakhla', 'CN01163', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN01163', 0, GETDATE(), NULL, 'System'),
(2, '01373', 'SAOUTANE', 'TARIQ', '1990-01-01', 'Dakhla', 'CN01373', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN01373', 0, GETDATE(), NULL, 'System'),
(2, '01376', 'DOUGHRI', 'HICHAM', '1990-01-01', 'Dakhla', 'CN01376', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN01376', 0, GETDATE(), NULL, 'System'),
(2, '01379', 'HAMMA', 'LIL MASSOUDA', '1990-01-01', 'Dakhla', 'CN01379', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN01379', 0, GETDATE(), NULL, 'System'),
(2, '01638', 'BOUCHIKHI', 'NAOUAL', '1990-01-01', 'Dakhla', 'CN01638', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN01638', 0, GETDATE(), NULL, 'System'),
(2, '01722', 'EL GHARIB', 'MOHAMED', '1990-01-01', 'Dakhla', 'CN01722', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN01722', 0, GETDATE(), NULL, 'System'),
(2, '01984', 'AGDACH', 'FARID', '1990-01-01', 'Dakhla', 'CN01984', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN01984', 0, GETDATE(), NULL, 'System'),
(2, '02189', 'JAIL', 'ILYASS', '1990-01-01', 'Dakhla', 'CN02189', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN02189', 0, GETDATE(), NULL, 'System'),
(2, '02321', 'BEN TAYBI ', 'AADNANE', '1990-01-01', 'Dakhla', 'CN02321', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN02321', 0, GETDATE(), NULL, 'System'),
(2, '03048', 'RHOUNI BELLOUTI', 'IMANE', '1990-01-01', 'Dakhla', 'CN03048', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN03048', 0, GETDATE(), NULL, 'System'),
(2, '03478', 'BASALLAM', 'ABDELAZIZ', '1990-01-01', 'Dakhla', 'CN03478', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN03478', 0, GETDATE(), NULL, 'System'),
(2, '03743', 'WARD', 'HICHAM', '1990-01-01', 'Dakhla', 'CN03743', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN03743', 0, GETDATE(), NULL, 'System'),
(2, '04097', 'ELANOUAR', 'SOUHAIL', '1990-01-01', 'Dakhla', 'CN04097', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN04097', 0, GETDATE(), NULL, 'System'),
(2, '04330', 'OULAABES', 'AYOUB', '1990-01-01', 'Dakhla', 'CN04330', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN04330', 0, GETDATE(), NULL, 'System'),
(2, '04332', 'AIT MANSOUR', 'RADOUANE', '1990-01-01', 'Dakhla', 'CN04332', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN04332', 0, GETDATE(), NULL, 'System'),
(2, '04333', 'IDERKOU', 'ABDELALI', '1990-01-01', 'Dakhla', 'CN04333', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN04333', 0, GETDATE(), NULL, 'System'),
(2, '04419', 'ABDOURABOU', 'FAISSAL', '1990-01-01', 'Dakhla', 'CN04419', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN04419', 0, GETDATE(), NULL, 'System'),
(2, '04538', 'KHAOULAN', 'SOUKAINA', '1990-01-01', 'Dakhla', 'CN04538', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN04538', 0, GETDATE(), NULL, 'System'),
(2, '04632', 'EL FRIYAKH', 'YASSINE', '1990-01-01', 'Dakhla', 'CN04632', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN04632', 0, GETDATE(), NULL, 'System'),
(2, '04663', 'WAHBI', 'YOUSSEF', '1990-01-01', 'Dakhla', 'CN04663', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN04663', 0, GETDATE(), NULL, 'System'),
(2, '04905', 'AMALOU', 'IBTIHAL', '1990-01-01', 'Dakhla', 'CN04905', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN04905', 0, GETDATE(), NULL, 'System'),
(2, '05335', 'OUTTALEB', 'MARIAM', '1990-01-01', 'Dakhla', 'CN05335', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN05335', 0, GETDATE(), NULL, 'System'),
(2, '05378', 'RADID', 'SYRINE', '1990-01-01', 'Dakhla', 'CN05378', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN05378', 0, GETDATE(), NULL, 'System'),
(2, '05725', 'LHACHIMI', 'RACHID', '1990-01-01', 'Dakhla', 'CN05725', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN05725', 0, GETDATE(), NULL, 'System'),
(2, '05824', 'MOHAMMADI', 'NAJWA', '1990-01-01', 'Dakhla', 'CN05824', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN05824', 0, GETDATE(), NULL, 'System'),
(2, '05825', 'ELYAMOUNI', 'CHAIMAA', '1990-01-01', 'Dakhla', 'CN05825', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN05825', 0, GETDATE(), NULL, 'System'),
(2, '05922', 'ELGOURCH', 'MOHAMED', '1990-01-01', 'Dakhla', 'CN05922', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN05922', 0, GETDATE(), NULL, 'System'),
(2, '06018', 'CHAKIR', 'KHADIJA', '1990-01-01', 'Dakhla', 'CN06018', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN06018', 0, GETDATE(), NULL, 'System'),
(2, '06029', 'IDRATE', 'MOHAMED', '1990-01-01', 'Dakhla', 'CN06029', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN06029', 0, GETDATE(), NULL, 'System'),
(2, '06285', 'FIRAOUNI', 'LAHCEN', '1990-01-01', 'Dakhla', 'CN06285', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN06285', 0, GETDATE(), NULL, 'System'),
(2, '06333', 'BENLAHOUCINE', 'AHMED', '1990-01-01', 'Dakhla', 'CN06333', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN06333', 0, GETDATE(), NULL, 'System'),
(2, '06369', 'BOUHMALA', 'AYOUB', '1990-01-01', 'Dakhla', 'CN06369', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN06369', 0, GETDATE(), NULL, 'System'),
(2, '06442', 'JAAFAR', 'MOAD', '1990-01-01', 'Dakhla', 'CN06442', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN06442', 0, GETDATE(), NULL, 'System');


INSERT INTO EMPLOYE (
    IdSociete, Matricule, Nom, Prenom, DateNaissance, LieuNaissance, 
    CinPassport, Sexe, SituationFamiliale, NombreEnfants, Adresse, Telephone, 
    Email, Nationalite, NiveauScolaire, Diplome, DateEmbauche, DateSortie, 
    TypeContrat, TypeEmploye, IdTypeOuv, Rolle, PasswordHash, IdDepartement, 
    IdService, IdCellule, IdFonction, IdEmplacement, Shift, SituationCNSS, 
    NumeroCNSS, is_deleted, DateCreation, DateModification, ModifiePar
) VALUES
(1, '00005', 'CHAGDALI', 'MORCHID', '1990-01-01', 'Dakhla', 'CN00005', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00005', 0, GETDATE(), NULL, 'System'),
(1, '00014', 'BOUZID', 'HOSSAM', '1990-01-01', 'Dakhla', 'CN00014', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00014', 0, GETDATE(), NULL, 'System'),
(1, '00203', 'RAGI', 'CHAIMAE', '1990-01-01', 'Dakhla', 'CN00203', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00203', 0, GETDATE(), NULL, 'System'),
(1, '00352', 'ID YASSINE', 'MUSTAPHA', '1990-01-01', 'Dakhla', 'CN00352', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00352', 0, GETDATE(), NULL, 'System'),
(1, '00783', 'EL MESBAHY', 'BOUCHRA', '1990-01-01', 'Dakhla', 'CN00783', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00783', 0, GETDATE(), NULL, 'System'),
(1, '02637', 'ISMAILI', 'ABDELAZIZ', '1990-01-01', 'Dakhla', 'CN02637', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN02637', 0, GETDATE(), NULL, 'System'),
(1, '03295', 'MASOIR', 'SANAA', '1990-01-01', 'Dakhla', 'CN03295', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN03295', 0, GETDATE(), NULL, 'System'),
(1, '05293', 'AMLIHA', 'HASSANIA', '1990-01-01', 'Dakhla', 'CN05293', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN05293', 0, GETDATE(), NULL, 'System'),
(1, '06393', 'ACHDAMI', 'SOUKAINA', '1990-01-01', 'Dakhla', 'CN06393', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN06393', 0, GETDATE(), NULL, 'System'),
(1, '06394', 'EL ATMANI', 'ABDELILAH', '1990-01-01', 'Dakhla', 'CN06394', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN06394', 0, GETDATE(), NULL, 'System'),
(1, '06451', 'RAINE ', 'AYOUB ', '1990-01-01', 'Dakhla', 'CN06451', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN06451', 0, GETDATE(), NULL, 'System'),
(1, '07121', 'ELJEBRI', 'IMAD', '1990-01-01', 'Dakhla', 'CN07121', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN07121', 0, GETDATE(), NULL, 'System'),
(1, '08219', 'DIACHI', 'AYMANE', '1990-01-01', 'Dakhla', 'CN08219', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN08219', 0, GETDATE(), NULL, 'System'),
(4, '00008', 'BADRAOUI', 'SAFAE', '1990-01-01', 'Dakhla', 'CN00008', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00008', 0, GETDATE(), NULL, 'System'),
(4, '00011', 'EL ASRI ', 'RACHID', '1990-01-01', 'Dakhla', 'CN00011', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00011', 0, GETDATE(), NULL, 'System'),
(4, '00014', 'AMEGROUD', 'RACHID ', '1990-01-01', 'Dakhla', 'CN00014', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00014', 0, GETDATE(), NULL, 'System'),
(4, '00017', 'LASRI ', 'ABDERAHMAN', '1990-01-01', 'Dakhla', 'CN00017', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00017', 0, GETDATE(), NULL, 'System'),
(4, '00021', 'AARAB', 'ESSAID', '1990-01-01', 'Dakhla', 'CN00021', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00021', 0, GETDATE(), NULL, 'System'),
(4, '00026', 'CHTABBOU', 'HASSAN', '1990-01-01', 'Dakhla', 'CN00026', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00026', 0, GETDATE(), NULL, 'System'),
(4, '00028', 'EL MENNARI', 'SOUKAINA', '1990-01-01', 'Dakhla', 'CN00028', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00028', 0, GETDATE(), NULL, 'System'),
(4, '00029', 'CHTABOU', 'KHALID', '1990-01-01', 'Dakhla', 'CN00029', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00029', 0, GETDATE(), NULL, 'System'),
(4, '00032', 'AIHI', 'HAMZA', '1990-01-01', 'Dakhla', 'CN00032', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00032', 0, GETDATE(), NULL, 'System'),
(4, '00055', 'EL MIMOUNI', 'MARIEME', '1990-01-01', 'Dakhla', 'CN00055', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00055', 0, GETDATE(), NULL, 'System'),
(4, '00077', 'KASSAM', 'MEHDI', '1990-01-01', 'Dakhla', 'CN00077', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00077', 0, GETDATE(), NULL, 'System'),
(4, '00082', 'MOUTAOIKIL', 'ZAKARIA', '1990-01-01', 'Dakhla', 'CN00082', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00082', 0, GETDATE(), NULL, 'System'),
(4, '00090', 'BAITAKA', 'MUSA', '1990-01-01', 'Dakhla', 'CN00090', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00090', 0, GETDATE(), NULL, 'System'),
(4, '00103', 'CHAABI', 'WIDADE', '1990-01-01', 'Dakhla', 'CN00103', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00103', 0, GETDATE(), NULL, 'System'),
(4, '00125', 'RAMZI', 'KAOUTAR', '1990-01-01', 'Dakhla', 'CN00125', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00125', 0, GETDATE(), NULL, 'System'),
(4, '00128', 'ELITAOUI', 'ACHRAF', '1990-01-01', 'Dakhla', 'CN00128', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00128', 0, GETDATE(), NULL, 'System'),
(5, '00011', 'EZZINE', 'CHAIMAE', '1990-01-01', 'Dakhla', 'CN00011', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00011', 0, GETDATE(), NULL, 'System'),
(5, '00065', 'EL JAOUHARI', 'OUALID', '1990-01-01', 'Dakhla', 'CN00065', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00065', 0, GETDATE(), NULL, 'System'),
(6, '00049', 'BENIDIR', 'ILYAS', '1990-01-01', 'Dakhla', 'CN00049', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00049', 0, GETDATE(), NULL, 'System'),
(6, '00053', 'EL KHAZOUM', 'ABDELKEBIR', '1990-01-01', 'Dakhla', 'CN00053', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00053', 0, GETDATE(), NULL, 'System'),
(6, '00059', 'EZZINE', 'FATIMA EZZAHRA', '1990-01-01', 'Dakhla', 'CN00059', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00059', 0, GETDATE(), NULL, 'System'),
(6, '00104', 'OURAMI', 'SAAD', '1990-01-01', 'Dakhla', 'CN00104', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00104', 0, GETDATE(), NULL, 'System'),
(9, '00811', 'BAKR', 'TIAR', '1990-01-01', 'Dakhla', 'CN00811', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00811', 0, GETDATE(), NULL, 'System'),
(10, '00008', 'ADERDOUR', 'FADOUA', '1990-01-01', 'Dakhla', 'CN00008', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00008', 0, GETDATE(), NULL, 'System'),
(8, '00006', 'FARHAT ', 'MOHSSINE', '1990-01-01', 'Dakhla', 'CN00006', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00006', 0, GETDATE(), NULL, 'System'),
(8, '00008', 'KARDI ', 'NAJIM', '1990-01-01', 'Dakhla', 'CN00008', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00008', 0, GETDATE(), NULL, 'System'),
(8, '00010', 'EZAHRI ', 'ABDERAHIM', '1990-01-01', 'Dakhla', 'CN00010', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00010', 0, GETDATE(), NULL, 'System'),
(8, '00014', 'BOUALOUCH ', 'BRAHIM', '1990-01-01', 'Dakhla', 'CN00014', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00014', 0, GETDATE(), NULL, 'System'),
(7, '00002', 'BOUJA ', 'RACHID', '1990-01-01', 'Dakhla', 'CN00002', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00002', 0, GETDATE(), NULL, 'System'),
(7, '00003', 'IBOURAK ', 'OMAR', '1990-01-01', 'Dakhla', 'CN00003', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00003', 0, GETDATE(), NULL, 'System'),
(7, '00004', 'NACEF ', 'FATIMA', '1990-01-01', 'Dakhla', 'CN00004', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN00004', 0, GETDATE(), NULL, 'System'),
(7, '03200', 'ADDAOUI', 'KHADIJA', '1990-01-01', 'Dakhla', 'CN03200', 'Homme', 'Célibataire', 0, 'Dakhla', '0612345678', NULL, 'Marocaine', 'Baccalauréat', NULL, '2024-01-15', NULL, 'CDI', 'OUVRIER', 1, NULL, NULL, 1, 3, 4, 7, NULL, 'JOUR', 'D', 'CN03200', 0, GETDATE(), NULL, 'System');