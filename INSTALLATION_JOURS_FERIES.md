# 🚀 Guide d'Installation Rapide - Gestion des Jours Fériés

## Vue d'ensemble
Ce guide vous accompagne dans l'installation du nouveau module **Gestion des Jours Fériés et Jours Spéciaux** qui permet de gérer les jours comptés comme repos dans le système RH.

---

## ✅ Prérequis

- SQL Server 2016+ avec la base `ERP_RH_Pointag_V2`
- Visual Studio 2017+
- .NET Framework 4.6.1
- Droits d'administration sur la base de données

---

## 📋 Étapes d'Installation

### Étape 1: Créer la Table et les Procédures (5 min)

1. **Ouvrir SQL Server Management Studio**
   ```
   Serveur: 10.77.105.112,1433
   Base de données: ERP_RH_Pointag_V2
   Authentification: SQL Server
   User: abdelilah
   Password: abdoabdo200
   ```

2. **Exécuter le Script SQL**
   - Ouvrir le fichier: `APPLICATION RH\Database_JoursFeries_Setup.sql`
   - Cliquer sur **Execute** (F5)
   - Vérifier les messages de succès:
     ```
     Table JOURS_FERIES created successfully.
     Index IX_JOURS_FERIES_DateJour created successfully.
     Index IX_JOURS_FERIES_TypeJour created successfully.
     View VW_JOURS_FERIES_UPCOMING created successfully.
     View VW_JOURS_FERIES_CURRENT_YEAR created successfully.
     Stored Procedure SP_IsHoliday created successfully.
     Stored Procedure SP_GetHolidaysByDateRange created successfully.
     Stored Procedure SP_CountWorkingDays created successfully.
     Default public holidays for 2025 inserted successfully.
     Database setup completed successfully!
     ```

3. **Vérifier la Création de la Table**
   ```sql
   SELECT * FROM JOURS_FERIES;
   -- Vous devriez voir 9 jours fériés pour 2025
   ```

---

### Étape 2: Compiler le Projet (2 min)

1. **Ouvrir Visual Studio**
   - Ouvrir le projet: `ERP-RH-.NET\APPLICATION RH.sln`

2. **Vérifier les Nouveaux Fichiers**
   - Dans Solution Explorer, vérifier que les fichiers suivants apparaissent:
     ```
     📁 conge/
       ├── 📄 GestionJoursFeries.aspx
       ├── 📄 GestionJoursFeries.aspx.cs
       └── 📄 GestionJoursFeries.aspx.designer.cs
     ```

3. **Rebuild Solution**
   - Build > Rebuild Solution (Ctrl+Shift+B)
   - Attendre la compilation complète
   - ✅ **Succès:** "Build succeeded"
   - ❌ **Erreur:** Vérifier les messages d'erreur

---

### Étape 3: Configurer les Permissions (3 min)

1. **Ajouter la Page au Système de Permissions**
   - Se connecter à l'application en tant qu'administrateur
   - Aller à: **Permissions > Pages Management** (`/permissions/PagesManagment.aspx`)

2. **Créer une Nouvelle Page**
   - Cliquer sur "Ajouter une Page"
   - Remplir les champs:
     ```
     Page Name: Gestion Jours Fériés
     Page URL: ~/conge/GestionJoursFeries.aspx
     Menu Group: Congés
     Is Active: ✅ Oui
     ```
   - Enregistrer

3. **Assigner les Permissions aux Rôles**
   - Aller à: **Permissions > Role-Page Permissions** (`/permissions/RollePage.aspx`)
   - Sélectionner le rôle: **Administrateur RH**
   - Cocher: ✅ **Gestion Jours Fériés**
   - Enregistrer

4. **Assigner d'Autres Rôles (Optionnel)**
   - Répéter pour: Gestionnaire RH, Comptabilité, etc.

---

### Étape 4: Tester le Module (5 min)

1. **Accéder au Module**
   - Naviguer vers: `/conge/GestionJoursFeries.aspx`
   - ✅ **Succès:** La page s'affiche avec 4 statistiques en haut
   - ❌ **Erreur 404:** Vérifier que le fichier .aspx existe
   - ❌ **Erreur Permission:** Vérifier l'étape 3

2. **Vérifier les Statistiques**
   - Année: 2025
   - Jours Fériés: **9**
   - Jours Spéciaux: **0**
   - À Venir: **X** (selon la date actuelle)
   - Total Jours Repos: **9**

3. **Tester l'Ajout de Jours Prédéfinis**
   - Scroller vers le bas
   - Cliquer sur: **Ajouter Jours Fériés Officiels**
   - Message attendu: "Tous les jours fériés officiels sont déjà présents pour cette année."
   - (Car déjà ajoutés par le script SQL)

4. **Tester l'Ajout Manuel**
   - Remplir le formulaire:
     ```
     Nom: Aid Al Fitr 2025
     Type: Jour Férié
     Payé: Oui
     Date: 2025-04-01 (exemple)
     Récurrent: Non
     Description: Fin du Ramadan
     ```
   - Cliquer **Enregistrer**
   - ✅ Le jour férié apparaît dans la liste
   - ✅ Les statistiques se mettent à jour (Jours Fériés: 10)

5. **Tester la Modification**
   - Cliquer sur l'icône **Modifier** (crayon) sur Aid Al Fitr
   - Modifier la description
   - Cliquer **Mettre à jour**
   - ✅ Les changements sont sauvegardés

6. **Tester la Suppression**
   - Cliquer sur l'icône **Supprimer** (poubelle)
   - Confirmer
   - ✅ Le jour férié disparaît de la liste

7. **Tester les Filtres**
   - Sélectionner l'année **2024** avec le bouton ◄
   - Vérifier que la liste est vide (pas de données pour 2024)
   - Revenir à **2025** avec le bouton ►
   - Sélectionner filtre **Type: Jours Fériés**
   - ✅ Seuls les jours fériés s'affichent

---

## 🎨 Ajouter le Menu dans Site.Master (Optionnel)

Si vous souhaitez ajouter un lien direct dans le menu de navigation:

1. **Ouvrir Site.Master**
   - Fichier: `APPLICATION RH\Site.Master`

2. **Trouver la Section Congés**
   - Chercher: `<!-- Congés Section -->`
   - Si elle n'existe pas, la créer après la section "Pointage Fix"

3. **Ajouter le Lien**
   ```aspx
   <li class="nav-item">
       <a class="nav-link menu-link" href="#sidebarConge" data-bs-toggle="collapse" role="button" aria-expanded="false" aria-controls="sidebarConge">
           <i class="ti ti-calendar-time"></i>
           <span data-key="t-conge">Congés</span>
       </a>
       <div class="collapse menu-dropdown" id="sidebarConge">
           <ul class="nav nav-sm flex-column">
               <li class="nav-item">
                   <asp:HyperLink ID="hlGestionJoursFeries" runat="server"
                       NavigateUrl="~/conge/GestionJoursFeries.aspx"
                       CssClass="nav-link" data-key="t-jours-feries">
                       Jours Fériés
                   </asp:HyperLink>
               </li>
               <li class="nav-item">
                   <asp:HyperLink ID="hlAbsencePermissions" runat="server"
                       NavigateUrl="~/conge/absence-permissions.aspx"
                       CssClass="nav-link" data-key="t-absences">
                       Absences & Permissions
                   </asp:HyperLink>
               </li>
           </ul>
       </div>
   </li>
   ```

4. **Ajouter dans Site.Master.designer.cs**
   ```csharp
   protected global::System.Web.UI.WebControls.HyperLink hlGestionJoursFeries;
   ```

5. **Rebuild et Tester**

---

## 📊 Vérification de la Base de Données

### Requêtes de Vérification

```sql
-- 1. Vérifier que la table existe
SELECT COUNT(*) AS TableExists
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'JOURS_FERIES';
-- Résultat attendu: 1

-- 2. Vérifier les données
SELECT COUNT(*) AS TotalHolidays
FROM JOURS_FERIES
WHERE is_deleted = 0 AND YEAR(DateJour) = 2025;
-- Résultat attendu: 9

-- 3. Lister tous les jours fériés 2025
SELECT
    Nom,
    FORMAT(DateJour, 'dd/MM/yyyy') AS Date,
    TypeJour,
    IsPaid,
    IsRecurrent
FROM JOURS_FERIES
WHERE is_deleted = 0 AND YEAR(DateJour) = 2025
ORDER BY DateJour;

-- 4. Tester la procédure stockée
DECLARE @IsHol BIT, @Name NVARCHAR(200), @Paid BIT;
EXEC SP_IsHoliday '2025-05-01', @IsHol OUTPUT, @Name OUTPUT, @Paid OUTPUT;
SELECT @IsHol AS IsHoliday, @Name AS HolidayName, @Paid AS IsPaid;
-- Résultat attendu: IsHoliday=1, HolidayName='Fête du Travail', IsPaid=1

-- 5. Tester la vue
SELECT * FROM VW_JOURS_FERIES_UPCOMING;
-- Affiche tous les jours fériés à venir
```

---

## 🐛 Résolution des Problèmes Courants

### Problème 1: "Table JOURS_FERIES does not exist"
**Cause:** Le script SQL n'a pas été exécuté
**Solution:**
1. Vérifier que vous êtes connecté à la bonne base de données
2. Ré-exécuter `Database_JoursFeries_Setup.sql`
3. Vérifier les messages de succès

### Problème 2: Erreur de Compilation "GestionJoursFeries does not exist"
**Cause:** Les fichiers ne sont pas inclus dans le projet
**Solution:**
1. Dans Solution Explorer, clic droit sur `conge/`
2. Add > Existing Item
3. Sélectionner les 3 fichiers GestionJoursFeries.*
4. Rebuild Solution

### Problème 3: "Permission Denied" lors de l'accès
**Cause:** L'utilisateur n'a pas les permissions
**Solution:**
1. Vérifier que la page est ajoutée dans **Pages Management**
2. Vérifier que le rôle a accès dans **Role-Page Permissions**
3. Se déconnecter et reconnecter

### Problème 4: Les Statistiques Affichent 0
**Cause:** Pas de données pour l'année sélectionnée
**Solution:**
1. Cliquer sur "Ajouter Jours Fériés Officiels"
2. Vérifier que l'année est 2025
3. Vérifier en BD: `SELECT * FROM JOURS_FERIES WHERE YEAR(DateJour) = 2025`

### Problème 5: Double Ajout des Jours Fériés
**Cause:** Le script SQL a été exécuté plusieurs fois
**Solution:**
```sql
-- Supprimer les doublons
WITH CTE AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY DateJour, Nom ORDER BY IdJourFerie) AS rn
    FROM JOURS_FERIES
)
UPDATE CTE SET is_deleted = 1 WHERE rn > 1;
```

---

## 📝 Checklist d'Installation

- [ ] Script SQL exécuté avec succès
- [ ] Table JOURS_FERIES créée
- [ ] 9 jours fériés insérés pour 2025
- [ ] Indexes créés
- [ ] Vues créées
- [ ] Procédures stockées créées
- [ ] Projet compilé sans erreurs
- [ ] Page ajoutée dans Pages Management
- [ ] Permissions assignées aux rôles
- [ ] Page accessible via URL
- [ ] Statistiques affichent correctement
- [ ] Ajout de jour férié fonctionne
- [ ] Modification fonctionne
- [ ] Suppression fonctionne
- [ ] Filtres fonctionnent
- [ ] Navigation par année fonctionne

---

## 📞 Support

**En cas de problème:**
1. Vérifier cette checklist
2. Consulter la section "Résolution des Problèmes"
3. Consulter le README détaillé: `conge/README_GestionJoursFeries.md`
4. Contacter l'équipe de développement

---

## 🎯 Prochaines Étapes

Une fois l'installation terminée:

1. **Ajouter les Jours Islamiques pour 2025**
   - Aid Al Fitr (dates à confirmer selon le calendrier lunaire)
   - Aid Al Adha
   - 1er Moharrem
   - Achoura
   - Mouled Nabaoui

2. **Former les Utilisateurs**
   - Organiser une démonstration
   - Partager le guide utilisateur

3. **Intégrer avec le Pointage**
   - Modifier les rapports de pointage pour exclure les jours fériés
   - Calculer les heures supplémentaires pour travail sur jour férié

4. **Configurer les Notifications**
   - Email de rappel 3 jours avant un jour férié
   - Notification sur le dashboard

---

## ✅ Installation Terminée!

Vous pouvez maintenant utiliser le module Gestion des Jours Fériés.

**Accès:** `/conge/GestionJoursFeries.aspx`

**Bon travail! 🎉**

---

**Version:** 1.0
**Date:** Janvier 2025
**Temps d'installation:** ~15 minutes
