-- Sample SQL Script to populate the permission system
-- This script demonstrates how to set up roles, pages, and permissions

-- 1. Insert Sample Roles
INSERT INTO ROLES (RoleName, Description, IsActive) VALUES 
('Administrator', 'Full system access', 1),
('Manager', 'Management level access', 1),
('HR Officer', 'Human Resources access', 1),
('Employee', 'Basic employee access', 1),
('Security Guard', 'Security and attendance tracking', 1);

-- 2. Insert Sample Pages (based on your existing application structure)
INSERT INTO PAGES (PageName, PageURL, MenuGroup, IsActive) VALUES 
-- Dashboard
('Dashboard HRM', '~/Default.aspx', 'Dashboard', 1),
('Rapport de Pointage par Port', '~/reporting/RaportPointagePort.aspx', 'Dashboard', 1),

-- Personnel Management
('Ajouter Employé', '~/personnel/AjouterEmploye.aspx', 'Personnel', 1),
('Liste des Employés', '~/personnel/ListeEmployes.aspx', 'Personnel', 1),
('Editer Employé', '~/personnel/EditEmployee.aspx', 'Personnel', 1),

-- Pointage (Time Tracking)
('Assigner Employés', '~/pointage/AssignerEmployes.aspx', 'Pointage', 1),
('Assignation de Groupe', '~/pointage/TransfertAssignation.aspx', 'Pointage', 1),
('Mon Équipe', '~/pointage/MonEquipe.aspx', 'Pointage', 1),
('Pointage Production', '~/pointage/PointageProduction.aspx', 'Pointage', 1),
('Pointage Équipe', '~/pointage/PointageEquipe.aspx', 'Pointage', 1),
('Contrôle Pointage', '~/pointage/SupervisionPointage.aspx', 'Pointage', 1),
('Heures Supplémentaires', '~/pointage/PointageHeuresSupp.aspx', 'Pointage', 1),

-- Pointage Fix (Port Security)
('Assigner Employés Ports', '~/pointagfix/AssignerEmployesPorts.aspx', 'Pointage Fix', 1),
('Pointage Équipe Ports', '~/pointagfix/PointageEmployesPorts.aspx', 'Pointage Fix', 1),
('Editer Pointage Port', '~/pointagfix/EditPointagePort.aspx', 'Pointage Fix', 1),
('Pointage Mobile', '~/PointageMobile/PointageMobile.aspx', 'Pointage Fix', 1),

-- Permissions Management
('Gestion des Rôles', '~/permissions/RollesManagment.aspx', 'Permissions', 1),
('Gestion des Pages', '~/permissions/PagesManagment.aspx', 'Permissions', 1),
('Permissions Rôle-Pages', '~/permissions/RollePage.aspx', 'Permissions', 1),
('Attribution des Rôles', '~/permissions/RolleEmployer.aspx', 'Permissions', 1),

-- Reports
('Rapport Employé Pointage', '~/reporting/RaportEmploiyerPointage.aspx', 'Rapports', 1);

-- 3. Set up Role-Page Permissions

-- Administrator: Full access to everything
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p 
WHERE r.RoleName = 'Administrator';

-- Manager: Access to most features except system administration
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p 
WHERE r.RoleName = 'Manager' 
AND p.MenuGroup IN ('Dashboard', 'Personnel', 'Pointage', 'Rapports');

-- HR Officer: Personnel management and reports
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p 
WHERE r.RoleName = 'HR Officer' 
AND p.MenuGroup IN ('Dashboard', 'Personnel', 'Rapports');

-- Employee: Basic dashboard and their own time tracking
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p 
WHERE r.RoleName = 'Employee' 
AND (p.MenuGroup = 'Dashboard' OR p.PageName IN ('Mon Équipe', 'Pointage Production'));

-- Security Guard: Port-related functionality only
INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess)
SELECT r.IdRole, p.IdPage, 1
FROM ROLES r, PAGES p 
WHERE r.RoleName = 'Security Guard' 
AND (p.MenuGroup = 'Dashboard' OR p.MenuGroup = 'Pointage Fix');

-- 4. Sample query to assign roles to employees (update existing employees)
/*
UPDATE EMPLOYEES 
SET IdRole = (SELECT IdRole FROM ROLES WHERE RoleName = 'Administrator')
WHERE Matricule = 'ADMIN001';

UPDATE EMPLOYEES 
SET IdRole = (SELECT IdRole FROM ROLES WHERE RoleName = 'Manager')
WHERE Matricule = 'MGR001';

UPDATE EMPLOYEES 
SET IdRole = (SELECT IdRole FROM ROLES WHERE RoleName = 'HR Officer')
WHERE Matricule = 'HR001';

UPDATE EMPLOYEES 
SET IdRole = (SELECT IdRole FROM ROLES WHERE RoleName = 'Employee')
WHERE Matricule = 'EMP001';

UPDATE EMPLOYEES 
SET IdRole = (SELECT IdRole FROM ROLES WHERE RoleName = 'Security Guard')
WHERE Matricule = 'SEC001';
*/

-- 5. Verification Queries

-- Check roles
SELECT * FROM ROLES ORDER BY RoleName;

-- Check pages by menu group
SELECT MenuGroup, COUNT(*) as PageCount 
FROM PAGES 
WHERE IsActive = 1 
GROUP BY MenuGroup 
ORDER BY MenuGroup;

-- Check permissions by role
SELECT 
    r.RoleName,
    COUNT(rp.IdPage) as TotalPermissions,
    COUNT(CASE WHEN rp.HasAccess = 1 THEN 1 END) as GrantedPermissions
FROM ROLES r
LEFT JOIN ROLE_PERMISSIONS rp ON r.IdRole = rp.IdRole
GROUP BY r.IdRole, r.RoleName
ORDER BY r.RoleName;

-- Check what pages each role can access
SELECT 
    r.RoleName,
    p.MenuGroup,
    p.PageName,
    p.PageURL
FROM ROLES r
INNER JOIN ROLE_PERMISSIONS rp ON r.IdRole = rp.IdRole
INNER JOIN PAGES p ON rp.IdPage = p.IdPage
WHERE rp.HasAccess = 1
AND r.IsActive = 1
AND p.IsActive = 1
ORDER BY r.RoleName, p.MenuGroup, p.PageName;