# ✅ Deployment Checklist - Gestion des Jours Fériés

## Pre-Deployment

### 1. Code Verification
- [x] All files created and saved
- [x] Code compiles without errors
- [x] No syntax errors in SQL script
- [x] Bug fixes applied (GetStatusBadge visibility)
- [x] Project file (.csproj) updated with new files

### 2. Files Checklist
```
✅ /conge/GestionJoursFeries.aspx
✅ /conge/GestionJoursFeries.aspx.cs
✅ /conge/GestionJoursFeries.aspx.designer.cs
✅ /conge/README_GestionJoursFeries.md
✅ /conge/GUIDE_RAPIDE_UTILISATEUR.md
✅ /Database_JoursFeries_Setup.sql
✅ /INSTALLATION_JOURS_FERIES.md
✅ /FEATURE_SUMMARY_JOURS_FERIES.md
✅ /UI_WIREFRAME_JOURS_FERIES.txt
✅ /BUGFIXES_JOURS_FERIES.md
✅ /DEPLOYMENT_CHECKLIST.md (this file)
```

---

## Deployment Steps

### Step 1: Backup (5 min)
- [ ] Backup current database `ERP_RH_Pointag_V2`
- [ ] Backup current application code
- [ ] Create restore point if possible
- [ ] Document current version number

**Backup Command:**
```sql
BACKUP DATABASE [ERP_RH_Pointag_V2]
TO DISK = 'C:\Backups\ERP_RH_Pointag_V2_BeforeJoursFeries.bak'
WITH FORMAT, INIT, NAME = 'Before Jours Feries Feature';
```

### Step 2: Database Deployment (5 min)
- [ ] Connect to SQL Server (10.77.105.112,1433)
- [ ] Select database `ERP_RH_Pointag_V2`
- [ ] Open `Database_JoursFeries_Setup.sql`
- [ ] Execute script (F5)
- [ ] Verify success messages:
  ```
  ✓ Table JOURS_FERIES created successfully
  ✓ Index IX_JOURS_FERIES_DateJour created
  ✓ Index IX_JOURS_FERIES_TypeJour created
  ✓ View VW_JOURS_FERIES_UPCOMING created
  ✓ View VW_JOURS_FERIES_CURRENT_YEAR created
  ✓ SP_IsHoliday created
  ✓ SP_GetHolidaysByDateRange created
  ✓ SP_CountWorkingDays created
  ✓ Default holidays inserted (9 rows)
  ```
- [ ] Run verification query:
  ```sql
  SELECT COUNT(*) FROM JOURS_FERIES WHERE YEAR(DateJour) = 2025;
  -- Expected: 9
  ```

### Step 3: Application Deployment (5 min)
- [ ] Stop IIS Application Pool (if running)
- [ ] Copy new files to application directory:
  - [ ] `/conge/GestionJoursFeries.aspx`
  - [ ] `/conge/GestionJoursFeries.aspx.cs`
  - [ ] `/conge/GestionJoursFeries.aspx.designer.cs`
- [ ] Copy documentation files (optional but recommended):
  - [ ] `/conge/README_GestionJoursFeries.md`
  - [ ] `/conge/GUIDE_RAPIDE_UTILISATEUR.md`
- [ ] Verify file permissions (IIS user needs read access)
- [ ] Start IIS Application Pool

**Alternative: Publish from Visual Studio**
- [ ] Open solution in Visual Studio
- [ ] Build > Rebuild Solution
- [ ] Verify 0 errors
- [ ] Right-click project > Publish
- [ ] Select publish profile
- [ ] Publish

### Step 4: Permissions Configuration (5 min)
- [ ] Login as Administrator
- [ ] Navigate to `/permissions/PagesManagment.aspx`
- [ ] Add new page:
  ```
  Page Name: Gestion Jours Fériés
  Page URL: ~/conge/GestionJoursFeries.aspx
  Menu Group: Congés
  Is Active: ✓ Yes
  ```
- [ ] Save

- [ ] Navigate to `/permissions/RollePage.aspx`
- [ ] Select role: **Administrateur RH**
- [ ] Enable access to: **Gestion Jours Fériés**
- [ ] Save

- [ ] Repeat for other roles as needed:
  - [ ] Gestionnaire RH
  - [ ] Comptabilité (view only)

### Step 5: Verification Testing (10 min)

#### 5.1 Basic Access
- [ ] Navigate to `/conge/GestionJoursFeries.aspx`
- [ ] Page loads without errors
- [ ] No 404 or 500 errors

#### 5.2 Statistics Display
- [ ] Statistics cards visible
- [ ] Jours Fériés shows: **9**
- [ ] Jours Spéciaux shows: **0**
- [ ] À Venir shows: **[number]** (based on current date)
- [ ] Total Repos shows: **9**

#### 5.3 Data Display
- [ ] GridView displays 9 holidays
- [ ] Dates formatted correctly (dd/MM/yyyy)
- [ ] Badges show correct colors
- [ ] Status badges show correctly

#### 5.4 CRUD Operations
**Add:**
- [ ] Fill form with test data:
  ```
  Nom: Test Holiday
  Type: Jour Spécial
  Payé: Non
  Date: [future date]
  Récurrent: Non
  ```
- [ ] Click "Enregistrer"
- [ ] Success message appears
- [ ] New holiday appears in GridView
- [ ] Statistics updated (Jours Spéciaux = 1)

**Edit:**
- [ ] Click edit icon (✏️) on "Test Holiday"
- [ ] Form populates with data
- [ ] Modify description
- [ ] Click "Mettre à jour"
- [ ] Changes saved

**Delete:**
- [ ] Click delete icon (🗑️) on "Test Holiday"
- [ ] Confirm deletion
- [ ] Holiday removed from list
- [ ] Statistics updated (Jours Spéciaux = 0)

#### 5.5 Filters
- [ ] Change year to 2024
- [ ] List shows empty (no data for 2024)
- [ ] Change back to 2025
- [ ] List shows 9 holidays
- [ ] Filter Type: "Jours Fériés"
- [ ] Only FERIE type shows
- [ ] Filter Status: "À Venir"
- [ ] Only future dates show

#### 5.6 Predefined Holidays
- [ ] Click "Ajouter Jours Fériés Officiels"
- [ ] Message: "Tous les jours fériés officiels sont déjà présents"
- [ ] (Already added by SQL script)

---

## Post-Deployment

### Step 6: User Training (As needed)
- [ ] Schedule training session
- [ ] Share documentation:
  - [ ] Quick Guide (`GUIDE_RAPIDE_UTILISATEUR.md`)
  - [ ] Full README (`README_GestionJoursFeries.md`)
- [ ] Create video tutorial (optional)
- [ ] Add to internal knowledge base

### Step 7: Monitoring (First 48 hours)
- [ ] Monitor application logs for errors
- [ ] Check SQL Server logs
- [ ] Verify no performance degradation
- [ ] Collect user feedback
- [ ] Address any issues promptly

### Step 8: Documentation Update
- [ ] Update system documentation
- [ ] Add to release notes
- [ ] Update version number
- [ ] Notify stakeholders

---

## Rollback Plan (If Issues Occur)

### Database Rollback
```sql
-- Drop new objects
DROP PROCEDURE IF EXISTS SP_IsHoliday;
DROP PROCEDURE IF EXISTS SP_GetHolidaysByDateRange;
DROP PROCEDURE IF EXISTS SP_CountWorkingDays;
DROP VIEW IF EXISTS VW_JOURS_FERIES_UPCOMING;
DROP VIEW IF EXISTS VW_JOURS_FERIES_CURRENT_YEAR;
DROP TABLE IF EXISTS JOURS_FERIES;

-- Restore from backup
RESTORE DATABASE [ERP_RH_Pointag_V2]
FROM DISK = 'C:\Backups\ERP_RH_Pointag_V2_BeforeJoursFeries.bak'
WITH REPLACE;
```

### Application Rollback
- [ ] Stop IIS Application Pool
- [ ] Delete new files:
  - `/conge/GestionJoursFeries.*`
- [ ] Restore previous version from backup
- [ ] Start IIS Application Pool
- [ ] Remove page from Permissions system

---

## Known Issues & Workarounds

### Issue: Permission Denied
**Symptom:** User cannot access the page
**Workaround:**
1. Verify page added in Pages Management
2. Verify role assigned in Role-Page Permissions
3. User logout/login to refresh permissions

### Issue: Statistics Show 0
**Symptom:** All statistics display 0
**Workaround:**
1. Check selected year (top right)
2. Ensure database has data for that year
3. Click "Ajouter Jours Fériés Officiels"
4. Refresh page (F5)

### Issue: GridView Empty
**Symptom:** "Aucun jour férié trouvé"
**Workaround:**
1. Reset filters (Type: Tous, Status: Tous)
2. Check year selection
3. Verify data in database:
   ```sql
   SELECT * FROM JOURS_FERIES WHERE is_deleted = 0;
   ```

---

## Support Contacts

**During Deployment:**
- Database Admin: [Contact]
- System Admin: [Contact]
- Developer: [Your contact]

**Post-Deployment:**
- Help Desk: [Extension/Email]
- HR Department: [Contact]
- IT Support: [Ticket system]

---

## Success Criteria

✅ **Deployment is successful if:**
- [x] Database script executes without errors
- [x] Application compiles and publishes
- [x] Page accessible to authorized users
- [x] All CRUD operations work
- [x] Filters function correctly
- [x] No performance impact on existing features
- [x] No errors in application logs
- [x] User feedback is positive

---

## Timeline

| Phase | Duration | Status |
|-------|----------|--------|
| Pre-deployment checks | 15 min | ⏳ Pending |
| Backup | 5 min | ⏳ Pending |
| Database deployment | 5 min | ⏳ Pending |
| Application deployment | 5 min | ⏳ Pending |
| Permissions config | 5 min | ⏳ Pending |
| Testing | 10 min | ⏳ Pending |
| User training | As needed | ⏳ Pending |
| **Total** | **~45 min** | ⏳ Pending |

---

## Sign-Off

**Deployed By:** _________________ **Date:** ___/___/2025

**Tested By:** _________________ **Date:** ___/___/2025

**Approved By:** _________________ **Date:** ___/___/2025

---

## Notes & Observations

```
[Space for deployment notes, issues encountered, and resolutions]




```

---

**Version:** 1.0
**Deployment Date:** _________________
**Status:** ⏳ Ready for Deployment
