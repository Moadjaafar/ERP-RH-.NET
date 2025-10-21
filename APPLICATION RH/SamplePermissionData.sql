-- ===============================================
-- Sample Data Setup for Role-Based Menu System
-- ===============================================

-- 1. Insert Sample Roles
INSERT INTO ROLES (RoleName, Description, IsActive) VALUES
('Admin', 'Administrateur système - Accès complet', 1),
('Manager', 'Gestionnaire - Accès aux fonctions de gestion', 1),
('HR', 'Ressources Humaines - Gestion du personnel', 1),
('Chef_Equipe', 'Chef d''équipe - Gestion des pointages', 1),
('Employe', 'Employé standard - Accès limité', 1),
('Security', 'Agent de sécurité - Pointage aux ports', 1);

-- 2. Insert Pages (Based on your Site.Master menu structure)
INSERT INTO PAGES (PageName, PageURL, MenuGroup, IsActive) VALUES
-- Dashboard
('Dashboard HRM', '~/Default.aspx', 'Dashboard', 1),
('Rapport Pointage Port', '~/reporting/RaportPointagePort.aspx', 'Dashboard', 1),

-- Personnel
('Ajouter Employé', '~/personnel/AjouterEmploye.aspx', 'Personnel', 1),
('Liste des Employés', '~/personnel/ListeEmployes.aspx', 'Personnel', 1),

-- Pointage
('Assigner Employés', '~/pointage/AssignerEmployes.aspx', 'Pointage', 1),
('Assignation de Groupe', '~/pointage/TransfertAssignation.aspx', 'Pointage', 1),
('Mon Équipe', '~/pointage/MonEquipe.aspx', 'Pointage', 1),
('Pointage Production', '~/pointage/PointageProduction.aspx', 'Pointage', 1),
('Pointage Équipe', '~/pointage/PointageEquipe.aspx', 'Pointage', 1),
('Contrôle Pointage', '~/pointage/SupervisionPointage.aspx', 'Pointage', 1),
('Heures Supplémentaires', '~/pointage/PointageHeuresSupp.aspx', 'Pointage', 1),

-- Pointage Fix (Ports)
('Assigner Employés Ports', '~/pointagfix/AssignerEmployesPorts.aspx', 'PointageFix', 1),
('Pointage Équipe Ports', '~/pointagfix/PointageEmployesPorts.aspx', 'PointageFix', 1),
('Editer Pointage Port', '~/pointagfix/EditPointagePort.aspx', 'PointageFix', 1),
('Pointage Mobile', '~/PointageMobile/PointageMobile.aspx', 'PointageFix', 1),

-- Permissions
('Gestion des Rôles', '~/permissions/RollesManagment.aspx', 'Permissions', 1),
('Gestion des Pages', '~/permissions/PagesManagment.aspx', 'Permissions', 1),
('Permissions Rôle-Pages', '~/permissions/RollePage.aspx', 'Permissions', 1),
('Attribution des Rôles', '~/permissions/RolleEmployer.aspx', 'Permissions', 1);

-- 3. Grant Permissions by Role

-- ADMIN - Full Access
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p
WHERE r.RoleName = 'Admin' AND p.IsActive = 1;

-- MANAGER - Management Functions
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p
WHERE r.RoleName = 'Manager' 
AND p.IsActive = 1
AND (p.MenuGroup IN ('Dashboard', 'Personnel', 'Pointage') 
     OR p.PageURL IN ('~/permissions/RolleEmployer.aspx'));

-- HR - Personnel Management
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p
WHERE r.RoleName = 'HR' 
AND p.IsActive = 1
AND (p.MenuGroup IN ('Dashboard', 'Personnel') 
     OR p.PageURL LIKE '~/personnel/%');

-- CHEF_EQUIPE - Team Management
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p
WHERE r.RoleName = 'Chef_Equipe' 
AND p.IsActive = 1
AND (p.PageURL = '~/Default.aspx' 
     OR p.PageURL LIKE '~/pointage/MonEquipe.aspx'
     OR p.PageURL LIKE '~/pointage/PointageEquipe.aspx'
     OR p.PageURL LIKE '~/pointage/PointageProduction.aspx');

-- EMPLOYE - Basic Access
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p
WHERE r.RoleName = 'Employe' 
AND p.IsActive = 1
AND p.PageURL = '~/Default.aspx';

-- SECURITY - Port Access Only
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p
WHERE r.RoleName = 'Security' 
AND p.IsActive = 1
AND (p.PageURL = '~/Default.aspx' 
     OR p.MenuGroup = 'PointageFix');

-- ===============================================
-- Test Queries - Run these to verify setup
-- ===============================================

-- Check Roles
SELECT * FROM ROLES WHERE IsActive = 1 ORDER BY RoleName;

-- Check Pages  
SELECT * FROM PAGES WHERE IsActive = 1 ORDER BY MenuGroup, PageName;

-- Check Permissions by Role
SELECT 
    r.RoleName,
    p.MenuGroup,
    p.PageName,
    p.PageURL,
    rp.HasAccess
FROM ROLE_PERMISSIONS rp
INNER JOIN ROLES r ON rp.IdRole = r.IdRole
INNER JOIN PAGES p ON rp.IdPage = p.IdPage
WHERE r.IsActive = 1 AND p.IsActive = 1 AND rp.HasAccess = 1
ORDER BY r.RoleName, p.MenuGroup, p.PageName;

-- Check specific role permissions (replace 'Manager' with your test role)
SELECT DISTINCT p.PageURL
FROM ROLE_PERMISSIONS rp
INNER JOIN PAGES p ON rp.IdPage = p.IdPage
INNER JOIN ROLES r ON rp.IdRole = r.IdRole
WHERE r.RoleName = 'Manager' 
AND rp.HasAccess = 1 
AND p.IsActive = 1
AND r.IsActive = 1
ORDER BY p.PageURL;

-- ===============================================
-- Update Employee Roles - Examples
-- ===============================================

-- Make an employee an Admin (replace IdEmploye = 1 with actual employee ID)
-- UPDATE EMPLOYE SET Rolle = 'Admin' WHERE IdEmploye = 1;

-- Make an employee a Manager  
-- UPDATE EMPLOYE SET Rolle = 'Manager' WHERE IdEmploye = 2;

-- Make an employee HR
-- UPDATE EMPLOYE SET Rolle = 'HR' WHERE IdEmploye = 3;

-- Check employee roles
-- SELECT IdEmploye, Nom, Prenom, Rolle FROM EMPLOYE WHERE is_deleted = 0 AND Rolle IS NOT NULL;