# 📖 Guide Rapide - Gestion des Jours Fériés

## 🎯 Accès Rapide
**URL:** `/conge/GestionJoursFeries.aspx`

---

## ⚡ Actions Rapides

### ➕ Ajouter un Jour Férié
1. Remplir le formulaire en haut de page
2. Cliquer **Enregistrer**

### ✏️ Modifier un Jour Férié
1. Cliquer sur l'icône **crayon** (✏️)
2. Modifier les champs
3. Cliquer **Mettre à jour**

### 🗑️ Supprimer un Jour Férié
1. Cliquer sur l'icône **poubelle** (🗑️)
2. Confirmer la suppression

### 🔍 Filtrer
- **Par année:** Utiliser les boutons ◄ **2025** ►
- **Par type:** Sélectionner dans la liste déroulante
- **Par statut:** Choisir Passés / À Venir / Aujourd'hui

### ⚡ Ajout Automatique
1. Cliquer sur **Ajouter Jours Fériés Officiels**
2. Les 9 jours fériés du Maroc sont ajoutés automatiquement

---

## 📋 Champs du Formulaire

| Champ | Obligatoire | Description | Exemple |
|-------|-------------|-------------|---------|
| **Nom** | ✅ | Nom du jour férié | "Aid Al Fitr 2025" |
| **Type** | ✅ | Jour Férié ou Jour Spécial | Jour Férié |
| **Payé** | ✅ | Oui (payé) ou Non (non payé) | Oui |
| **Date** | ✅ | Date du jour férié | 01/05/2025 |
| **Récurrent** | ✅ | Se répète chaque année? | Oui / Non |
| **Type Récurrence** | ⚠️ Si récurrent | Annuel, Mensuel, Personnalisé | Annuel |
| **Description** | ❌ | Informations additionnelles | "Fin du Ramadan" |

---

## 🏷️ Types de Jours

### 🔴 Jour Férié
Jour férié officiel (généralement payé)
- **Exemples:** Fête du Travail, Fête du Trône
- **Couleur:** Badge rouge

### 🟡 Jour Spécial
Jour spécial de l'entreprise (payé ou non payé)
- **Exemples:** Journée d'entreprise, Événement spécial
- **Couleur:** Badge jaune

---

## 📊 Lecture des Statistiques

### Carte 1: Jours Fériés
Nombre de jours fériés pour l'année sélectionnée

### Carte 2: Jours Spéciaux
Nombre de jours spéciaux configurés

### Carte 3: À Venir
Jours fériés qui n'ont pas encore eu lieu

### Carte 4: Total Repos
Total de tous les jours de repos (fériés + spéciaux)

---

## 🎨 Code Couleur

| Couleur | Signification |
|---------|---------------|
| 🔴 Rouge | Jour Férié |
| 🟡 Jaune | Jour Spécial |
| 🟢 Vert | Payé / Récurrent |
| 🟠 Orange | Non Payé |
| 🔵 Bleu | À Venir |
| ⚫ Gris | Passé |

---

## 🗓️ Jours Fériés Officiels du Maroc

### Jours Fixes (Calendrier Grégorien)
- **01 Janvier** - Nouvel An
- **11 Janvier** - Manifeste de l'Indépendance
- **01 Mai** - Fête du Travail
- **30 Juillet** - Fête du Trône
- **14 Août** - Journée de Oued Ed-Dahab
- **20 Août** - Révolution du Roi et du Peuple
- **21 Août** - Fête de la Jeunesse
- **06 Novembre** - Marche Verte
- **18 Novembre** - Fête de l'Indépendance

### Jours Variables (Calendrier Islamique)
Ces dates changent chaque année selon le calendrier lunaire:
- **Aid Al Fitr** (2-3 jours)
- **Aid Al Adha** (2-3 jours)
- **1er Moharrem** (Nouvel An Hégirien)
- **Achoura**
- **Mouled Nabaoui** (Anniversaire du Prophète)

⚠️ **Important:** Les jours islamiques doivent être ajoutés manuellement chaque année

---

## 💡 Conseils d'Utilisation

### ✅ Bonnes Pratiques

1. **Début d'Année**
   - Ajouter les jours fériés officiels via le bouton automatique
   - Ajouter manuellement les jours islamiques

2. **Descriptions Claires**
   - Ajouter une description pour chaque jour férié
   - Facilite la compréhension pour tous

3. **Récurrence**
   - Utiliser "Récurrent: Oui" pour les jours fixes
   - Simplifie la gestion année après année

4. **Vérification**
   - Vérifier régulièrement les jours à venir
   - S'assurer que tous les jours sont bien configurés

### ❌ À Éviter

1. **Ne pas créer de doublons**
   - Le système prévient, mais vérifier avant d'ajouter

2. **Ne pas supprimer l'historique**
   - Les jours passés servent de référence
   - Utile pour les rapports annuels

3. **Ne pas oublier les jours islamiques**
   - Les ajouter dès que les dates sont connues

---

## 🔍 Cas d'Usage Fréquents

### Cas 1: Planification Annuelle
**Objectif:** Préparer le calendrier de l'année
```
1. Début janvier, sélectionner l'année suivante (ex: 2026)
2. Cliquer "Ajouter Jours Fériés Officiels"
3. Consulter le calendrier islamique officiel
4. Ajouter manuellement Aid Al Fitr, Aid Al Adha, etc.
5. Vérifier que tous les jours sont bien présents (13-14 jours au total)
```

### Cas 2: Vérification Avant Paie
**Objectif:** S'assurer que tous les jours payés sont comptabilisés
```
1. Filtrer par "Payé: Oui"
2. Filtrer par mois concerné (ex: Mai 2025)
3. Noter les dates
4. Intégrer dans le calcul de paie
```

### Cas 3: Communication Équipe
**Objectif:** Informer l'équipe des prochains jours fériés
```
1. Filtrer "Statut: À Venir"
2. Consulter la liste
3. Communiquer via email/réunion
```

### Cas 4: Ajout d'un Événement Spécial
**Objectif:** Déclarer un jour spécial d'entreprise
```
Exemple: Journée Team Building
1. Nom: "Journée Team Building 2025"
2. Type: Jour Spécial
3. Payé: Oui
4. Date: Sélectionner la date
5. Récurrent: Non
6. Description: "Team building annuel de l'entreprise"
7. Enregistrer
```

---

## ❓ FAQ - Questions Fréquentes

### Q: Comment ajouter les jours fériés islamiques?
**R:** Les dates varient chaque année. Consultez le calendrier islamique officiel et ajoutez-les manuellement.

### Q: Puis-je supprimer un jour férié déjà passé?
**R:** Oui, mais il est recommandé de les garder pour l'historique. La suppression est "soft" (is_deleted=1), les données restent en base.

### Q: Que signifie "Récurrent: Annuel"?
**R:** Le jour férié se répète chaque année à la même date (ex: 1er Mai).

### Q: Les jours fériés sont-ils automatiquement pris en compte dans le pointage?
**R:** Oui, le système vérifie la table JOURS_FERIES lors des calculs de présence.

### Q: Puis-je avoir des jours fériés non payés?
**R:** Oui, sélectionnez "Payé: Non" lors de l'ajout.

### Q: Comment exporter la liste?
**R:** Actuellement, pas d'export direct. Utilisez Ctrl+A puis copier-coller dans Excel, ou demandez l'ajout de cette fonctionnalité.

### Q: Que se passe-t-il si j'ajoute deux fois le même jour?
**R:** Le bouton "Jours Prédéfinis" vérifie les doublons. Si vous ajoutez manuellement, deux entrées distinctes seront créées (à éviter).

### Q: Qui peut ajouter/modifier des jours fériés?
**R:** Les utilisateurs avec le rôle "Administrateur RH" ou "Gestionnaire RH". Configuré dans Permissions > Role-Page.

---

## 🆘 Problèmes Courants

### Problème: Je ne vois pas la page
**Solution:** Vérifiez vos permissions. Contactez l'administrateur si besoin.

### Problème: Les statistiques affichent 0
**Solution:**
1. Vérifiez que vous êtes sur la bonne année
2. Cliquez sur "Ajouter Jours Fériés Officiels"
3. Rafraîchissez la page (F5)

### Problème: Le formulaire ne se vide pas après ajout
**Solution:** Cliquez sur le bouton "Annuler" pour réinitialiser le formulaire.

### Problème: Je ne trouve pas un jour férié spécifique
**Solution:**
1. Vérifiez l'année sélectionnée
2. Vérifiez les filtres (Type, Statut)
3. Réinitialisez les filtres à "Tous"

---

## 📞 Support

**Besoin d'aide?**
- 📧 Email: support-rh@votre-entreprise.com
- 📞 Téléphone: Extension RH
- 💬 Chat: Slack #support-erp-rh

**Documentation:**
- Guide complet: `conge/README_GestionJoursFeries.md`
- Installation: `INSTALLATION_JOURS_FERIES.md`

---

## 🎓 Formation

**Vidéos tutorielles disponibles:**
- ▶️ Introduction (5 min)
- ▶️ Ajout de jours fériés (3 min)
- ▶️ Gestion annuelle (10 min)
- ▶️ Intégration avec la paie (8 min)

**Sessions de formation:**
- Tous les premiers lundis du mois
- Inscription: formation@votre-entreprise.com

---

## ✅ Checklist Mensuelle

- [ ] Vérifier les jours fériés du mois prochain
- [ ] Communiquer à l'équipe
- [ ] Vérifier que tous les jours sont payés/non payés correctement
- [ ] Informer la comptabilité

## ✅ Checklist Annuelle (Janvier)

- [ ] Sélectionner la nouvelle année
- [ ] Cliquer "Ajouter Jours Fériés Officiels"
- [ ] Consulter calendrier islamique
- [ ] Ajouter manuellement Aid Al Fitr
- [ ] Ajouter manuellement Aid Al Adha
- [ ] Ajouter manuellement 1er Moharrem
- [ ] Ajouter manuellement Achoura
- [ ] Ajouter manuellement Mouled Nabaoui
- [ ] Vérifier total = 13-14 jours
- [ ] Exporter liste et partager avec direction

---

**Version:** 1.0
**Dernière mise à jour:** Janvier 2025
**Auteur:** Équipe ERP-RH

---

💡 **Astuce du jour:** Utilisez les filtres pour voir rapidement les jours fériés payés du mois en cours!
