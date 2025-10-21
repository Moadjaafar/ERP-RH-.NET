# Gestion des Jours Fériés et Jours Spéciaux

## 📋 Vue d'ensemble

Le module **Gestion des Jours Fériés** permet de gérer tous les jours fériés (publics) et jours spéciaux qui sont comptés comme **repos** dans le système RH.

### Objectifs
- ✅ Gérer les jours fériés officiels du Maroc
- ✅ Ajouter des jours spéciaux personnalisés
- ✅ Définir si un jour est payé ou non payé
- ✅ Configurer les jours récurrents (annuels)
- ✅ Calculer automatiquement les jours de repos pour la paie
- ✅ Intégration avec le système de pointage

---

## 🚀 Accès au Module

**URL:** `/conge/GestionJoursFeries.aspx`

**Permissions requises:**
- Role: Administrateur RH ou Gestionnaire Congés
- Page: À configurer dans le système de permissions

---

## 📊 Fonctionnalités Principales

### 1. **Tableau de Bord Statistiques**

Affiche en temps réel:
- 📅 **Jours Fériés**: Nombre total de jours fériés pour l'année sélectionnée
- ⭐ **Jours Spéciaux**: Nombre de jours spéciaux configurés
- 🔜 **À Venir**: Nombre de jours fériés/spéciaux à venir
- 🏖️ **Total Repos**: Total de jours de repos (fériés + spéciaux)

### 2. **Ajout de Jours Fériés/Spéciaux**

**Champs du formulaire:**

| Champ | Type | Obligatoire | Description |
|-------|------|-------------|-------------|
| Nom | Texte | ✅ Oui | Nom du jour férié (ex: "Aid Al Fitr", "Fête du Travail") |
| Type | Liste | ✅ Oui | **Jour Férié** ou **Jour Spécial** |
| Payé | Liste | ✅ Oui | **Oui** (payé) ou **Non** (non payé) |
| Date | Date | ✅ Oui | Date du jour férié |
| Récurrent | Radio | ✅ Oui | Le jour se répète-t-il chaque année? |
| Type de Récurrence | Liste | ⚠️ Si récurrent | **Annuel**, **Mensuel**, ou **Personnalisé** |
| Description | Texte long | ❌ Non | Description additionnelle |

**Types de Récurrence:**
- **YEARLY** (Annuel): Se répète chaque année à la même date (ex: Fête du Travail - 1er Mai)
- **MONTHLY**: Se répète chaque mois (rare pour les jours fériés)
- **CUSTOM**: Récurrence personnalisée (à configurer manuellement)

### 3. **Liste des Jours Fériés**

Affiche tous les jours fériés avec:
- 📅 **Date** + jour de la semaine
- 🏷️ **Nom** du jour férié
- 🎯 **Type** (Jour Férié ou Jour Spécial)
- 💰 **Payé** (Oui/Non)
- 🔄 **Récurrent** (Annuel, Mensuel, etc.)
- 📝 **Description**
- 🚦 **Statut** (Aujourd'hui, Passé, À venir)
- ⚙️ **Actions** (Modifier, Supprimer)

### 4. **Filtres Avancés**

**Navigation par Année:**
- Boutons ◄ Année ► pour naviguer entre les années
- Affichage automatique des jours fériés de l'année sélectionnée

**Filtres:**
- **Type**: Tous / Jours Fériés / Jours Spéciaux
- **Statut**: Tous / Passés / À Venir / Aujourd'hui

### 5. **Jours Fériés Prédéfinis (Maroc)**

Bouton magique pour ajouter automatiquement les **9 jours fériés officiels du Maroc**:

| Date | Jour Férié | Type |
|------|-----------|------|
| 1er Janvier | Nouvel An | Civil |
| 11 Janvier | Manifeste de l'Indépendance | National |
| 1er Mai | Fête du Travail | Civil |
| 30 Juillet | Fête du Trône | National |
| 14 Août | Journée de Oued Ed-Dahab | National |
| 20 Août | Révolution du Roi et du Peuple | National |
| 21 Août | Fête de la Jeunesse | National |
| 6 Novembre | Marche Verte | National |
| 18 Novembre | Fête de l'Indépendance | National |

**Note:** Les jours fériés islamiques (Aid Al Fitr, Aid Al Adha, Achoura, Mouled) doivent être ajoutés manuellement car leurs dates varient selon le calendrier lunaire (Hijri).

---

## 💾 Structure de la Base de Données

### Table: `JOURS_FERIES`

```sql
CREATE TABLE JOURS_FERIES (
    IdJourFerie INT IDENTITY(1,1) PRIMARY KEY,
    Nom NVARCHAR(200) NOT NULL,
    DateJour DATE NOT NULL,
    TypeJour NVARCHAR(20) NOT NULL DEFAULT 'FERIE',
    IsPaid BIT NOT NULL DEFAULT 1,
    IsRecurrent BIT NOT NULL DEFAULT 0,
    RecurringType NVARCHAR(20) NULL,
    Description NVARCHAR(500) NULL,
    CreatedBy INT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL,
    is_deleted BIT NOT NULL DEFAULT 0
);
```

**Index:**
- `IX_JOURS_FERIES_DateJour` - Index sur DateJour pour requêtes rapides
- `IX_JOURS_FERIES_TypeJour` - Index sur TypeJour pour filtrage

---

## 🔧 Procédures Stockées

### 1. `SP_IsHoliday`
Vérifie si une date donnée est un jour férié.

```sql
DECLARE @IsHol BIT, @Name NVARCHAR(200), @Paid BIT;
EXEC SP_IsHoliday '2025-05-01', @IsHol OUTPUT, @Name OUTPUT, @Paid OUTPUT;
SELECT @IsHol AS IsHoliday, @Name AS HolidayName, @Paid AS IsPaid;
```

**Résultat:**
```
IsHoliday | HolidayName      | IsPaid
----------|------------------|-------
1         | Fête du Travail  | 1
```

### 2. `SP_GetHolidaysByDateRange`
Récupère tous les jours fériés dans une plage de dates.

```sql
EXEC SP_GetHolidaysByDateRange '2025-01-01', '2025-12-31', NULL;
-- ou filtre par type
EXEC SP_GetHolidaysByDateRange '2025-01-01', '2025-12-31', 'FERIE';
```

### 3. `SP_CountWorkingDays`
Compte les jours ouvrables (excluant weekends et jours fériés).

```sql
DECLARE @WorkDays INT, @HolDays INT;
EXEC SP_CountWorkingDays '2025-01-01', '2025-12-31', @WorkDays OUTPUT, @HolDays OUTPUT;
SELECT @WorkDays AS WorkingDays, @HolDays AS Holidays;
```

---

## 📈 Vues Disponibles

### 1. `VW_JOURS_FERIES_UPCOMING`
Affiche tous les jours fériés à venir pour l'année en cours.

```sql
SELECT * FROM VW_JOURS_FERIES_UPCOMING ORDER BY DateJour;
```

**Colonnes:**
- IdJourFerie, Nom, DateJour, TypeJour, IsPaid, IsRecurrent
- DaysUntil (nombre de jours jusqu'à la date)
- DayOfWeek (jour de la semaine)
- TypeDescription (description du type)

### 2. `VW_JOURS_FERIES_CURRENT_YEAR`
Affiche tous les jours fériés de l'année en cours.

```sql
SELECT * FROM VW_JOURS_FERIES_CURRENT_YEAR;
```

---

## 🔗 Intégration avec le Système de Pointage

### Cas d'Utilisation

1. **Calcul des Heures de Travail:**
   - Le système ignore les jours fériés lors du calcul des heures attendues
   - Si un employé travaille un jour férié, c'est considéré comme heures supplémentaires

2. **Rapports de Présence:**
   - Les jours fériés sont marqués comme "Repos" dans les rapports
   - Distinction entre repos payé et non payé

3. **Calcul de Paie:**
   - Les jours fériés payés sont inclus dans le salaire mensuel
   - Les jours fériés non payés sont déduits

### Exemple de Requête d'Intégration

```sql
-- Vérifier si un employé a travaillé un jour férié
SELECT
    e.Nom,
    e.Prenom,
    ep.DateHeure,
    jf.Nom AS JourFerie,
    CASE WHEN jf.IdJourFerie IS NOT NULL THEN 'Heures Supplémentaires' ELSE 'Normal' END AS TypeHeures
FROM EMPLOYE_POINTAGE ep
INNER JOIN EMPLOYE e ON ep.IdEmploye = e.IdEmploye
LEFT JOIN JOURS_FERIES jf ON CAST(ep.DateHeure AS DATE) = jf.DateJour AND jf.is_deleted = 0
WHERE ep.TypePointage = 'ENTREE'
    AND YEAR(ep.DateHeure) = 2025
    AND MONTH(ep.DateHeure) = 5
ORDER BY ep.DateHeure;
```

---

## 📝 Guide d'Utilisation

### Ajouter un Jour Férié

1. Accéder à `/conge/GestionJoursFeries.aspx`
2. Remplir le formulaire en haut de la page:
   - **Nom**: "Aid Al Fitr 2025"
   - **Type**: Jour Férié
   - **Payé**: Oui
   - **Date**: Sélectionner la date
   - **Récurrent**: Non (car date varie chaque année)
   - **Description**: "Fin du Ramadan"
3. Cliquer sur **Enregistrer**

### Ajouter les Jours Fériés Officiels

1. Sélectionner l'année souhaitée avec les boutons ◄ Année ►
2. Cliquer sur **Ajouter Jours Fériés Officiels**
3. Les 9 jours fériés fixes sont automatiquement ajoutés
4. Ajouter manuellement les jours islamiques

### Modifier un Jour Férié

1. Cliquer sur le bouton **Modifier** (icône crayon) dans la ligne correspondante
2. Le formulaire se remplit avec les données existantes
3. Modifier les champs souhaités
4. Cliquer sur **Mettre à jour**

### Supprimer un Jour Férié

1. Cliquer sur le bouton **Supprimer** (icône poubelle)
2. Confirmer la suppression
3. Le jour férié est marqué comme supprimé (soft delete)

### Filtrer les Jours Fériés

1. **Par Type**: Sélectionner "Jours Fériés" ou "Jours Spéciaux" dans le filtre
2. **Par Statut**: Sélectionner "Passés", "À Venir", ou "Aujourd'hui"
3. Les résultats se mettent à jour automatiquement

---

## 🎨 Codes Couleur

- 🔴 **Rouge** (`#dc3545`): Jour Férié
- 🟡 **Jaune** (`#ffc107`): Jour Spécial
- 🟢 **Vert** (`#28a745`): Récurrent
- 🔵 **Bleu** (`#007bff`): À venir
- ⚫ **Gris** (`#6c757d`): Passé

---

## ⚙️ Installation

### 1. Exécuter le Script SQL

```bash
# Se connecter à SQL Server
sqlcmd -S 10.77.105.112,1433 -U abdelilah -P abdoabdo200 -d ERP_RH_Pointag_V2

# Exécuter le script
:r C:\Users\M.JAAFAR\Desktop\ERP-RH-.NET\APPLICATION RH\Database_JoursFeries_Setup.sql
GO
```

### 2. Compiler le Projet

1. Ouvrir le projet dans Visual Studio
2. Build > Rebuild Solution
3. Vérifier qu'il n'y a pas d'erreurs

### 3. Configurer les Permissions

Dans le module **Permissions**:
1. Aller à **Pages Management**
2. Ajouter la page: `/conge/GestionJoursFeries.aspx`
3. Assigner les permissions aux rôles appropriés (RH, Admin)

### 4. Tester

1. Naviguer vers `/conge/GestionJoursFeries.aspx`
2. Cliquer sur "Ajouter Jours Fériés Officiels"
3. Vérifier que les 9 jours fériés sont ajoutés
4. Ajouter manuellement un jour férié islamique

---

## 🧪 Tests Recommandés

### Test 1: Ajout d'un Jour Férié
- ✅ Ajouter un jour férié avec tous les champs
- ✅ Vérifier qu'il apparaît dans la liste
- ✅ Vérifier les statistiques mises à jour

### Test 2: Modification
- ✅ Modifier un jour férié existant
- ✅ Vérifier que les changements sont sauvegardés
- ✅ Vérifier que le formulaire se vide après annulation

### Test 3: Suppression
- ✅ Supprimer un jour férié
- ✅ Vérifier qu'il n'apparaît plus dans la liste
- ✅ Vérifier que is_deleted = 1 dans la BD

### Test 4: Filtres
- ✅ Tester filtre par type (Fériés/Spéciaux)
- ✅ Tester filtre par statut (Passés/À Venir)
- ✅ Tester navigation par année

### Test 5: Jours Prédéfinis
- ✅ Cliquer sur "Ajouter Jours Fériés Officiels"
- ✅ Vérifier que 9 jours sont ajoutés
- ✅ Re-cliquer pour vérifier qu'il n'y a pas de doublons

### Test 6: Intégration Pointage
- ✅ Créer un pointage pour un jour férié
- ✅ Vérifier que le système le détecte comme férié
- ✅ Vérifier le calcul des heures supplémentaires

---

## 🐛 Dépannage

### Problème: "Table JOURS_FERIES does not exist"
**Solution:** Exécuter le script SQL `Database_JoursFeries_Setup.sql`

### Problème: "Permission denied"
**Solution:**
1. Aller dans **Permissions > Pages Management**
2. Ajouter la page `/conge/GestionJoursFeries.aspx`
3. Assigner au rôle de l'utilisateur

### Problème: Les statistiques ne se mettent pas à jour
**Solution:**
1. Vérifier que `is_deleted = 0` dans la table
2. Vérifier que l'année est correctement sélectionnée
3. F5 pour rafraîchir la page

### Problème: Doublons lors de l'ajout des jours prédéfinis
**Solution:** Le système vérifie automatiquement les doublons. Si le message "déjà présents" apparaît, c'est normal.

---

## 📚 Références

### Jours Fériés Officiels du Maroc

**Source:** Bulletin Officiel du Royaume du Maroc

**Jours Civils:**
- 1er Janvier: Nouvel An
- 1er Mai: Fête du Travail

**Jours Nationaux:**
- 11 Janvier: Manifeste de l'Indépendance
- 30 Juillet: Fête du Trône
- 14 Août: Journée de Oued Ed-Dahab
- 20 Août: Révolution du Roi et du Peuple
- 21 Août: Fête de la Jeunesse
- 6 Novembre: Marche Verte
- 18 Novembre: Fête de l'Indépendance

**Jours Islamiques (dates variables):**
- Aid Al Fitr (2-3 jours)
- Aid Al Adha (2-3 jours)
- 1er Moharrem (Hijri New Year)
- Achoura
- Mouled Nabaoui (Anniversaire du Prophète)

---

## 🔄 Évolutions Futures

- [ ] Calcul automatique des dates islamiques via API
- [ ] Export PDF de calendrier annuel
- [ ] Notifications avant jours fériés
- [ ] Synchronisation avec Outlook/Google Calendar
- [ ] Gestion multi-pays (jours fériés internationaux)
- [ ] Historique des modifications
- [ ] Bulk import via Excel

---

## 👥 Support

**Questions/Bugs:** Contacter l'équipe de développement RH
**Documentation:** Ce README
**Formation:** Vidéo tutorielle disponible sur le portail interne

---

**Version:** 1.0
**Date:** Janvier 2025
**Développé par:** ERP-RH Team
**Licence:** Usage Interne Uniquement
