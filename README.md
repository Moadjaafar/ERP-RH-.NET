# ERP-RH-.NET

Un système de gestion des ressources humaines (ERP-RH) développé en ASP.NET Web Forms avec SQL Server, conçu pour la gestion du personnel et du pointage dans un environnement d'entreprise.

## 🚀 Vue d'ensemble

Ce système ERP-RH offre une solution complète pour la gestion des ressources humaines, incluant la gestion des employés, le système de pointage, et la génération de rapports. L'application est spécialement adaptée aux entreprises industrielles avec des fonctionnalités de pointage par équipes et de gestion des ports.

## 📋 Fonctionnalités principales

### 🧑‍💼 Gestion du Personnel
- **Ajout et gestion des employés** : Interface complète pour créer et modifier les profils des employés
- **Structure organisationnelle** : Gestion des sociétés, départements, services, cellules et fonctions
- **Types d'employés** : Support pour différents types d'ouvriers et contrats
- **Données personnelles complètes** : Informations personnelles, situation familiale, niveau scolaire

### ⏰ Système de Pointage
- **Pointage par équipe** : Gestion des équipes de travail avec chefs d'équipe
- **Pointage de production** : Suivi des heures de travail et de production
- **Heures supplémentaires** : Gestion et calcul des heures supplémentaires
- **Pointage des ports** : Module spécialisé pour le pointage dans les environnements portuaires
- **Supervision** : Outils de supervision et validation des pointages

### 📊 Rapports et Analyses
- **Rapports de pointage** : Génération de rapports détaillés par période
- **Export Excel** : Exportation des données au format Excel avec ClosedXML
- **Analyses par employé/équipe** : Suivi individuel et collectif des performances

### 🔐 Sécurité et Authentification
- **Système de connexion sécurisé** : Authentification par email/matricule et mot de passe
- **Gestion des rôles** : Différents niveaux d'accès (employé, chef d'équipe, sécurité port, etc.)
- **Sessions sécurisées** : Gestion des sessions avec cookies persistants optionnels
- **Audit trail** : Suivi des modifications avec table d'audit

## 🛠 Stack Technique

### Backend
- **Framework** : ASP.NET Web Forms 4.6.1
- **Langage** : C#
- **Base de données** : SQL Server (ERP_RH_Pointag_V2)
- **ORM** : ADO.NET avec SqlConnection

### Frontend
- **UI Framework** : Bootstrap 3.3.7
- **JavaScript** : jQuery 3.3.1
- **Icons** : Tabler Icons, Material Design Icons
- **Charts** : ApexCharts
- **Notifications** : SweetAlert2

### Librairies et packages
- **ClosedXML** : Export Excel
- **DocumentFormat.OpenXml** : Manipulation de documents Office
- **Microsoft.AspNetCore.Components** : Composants modernes
- **Newtonsoft.Json** : Sérialisation JSON

## 📁 Structure du projet

```
ERP-RH-.NET/
├── APPLICATION RH/                 # Application principale
│   ├── personnel/                  # Module de gestion du personnel
│   │   ├── AjouterEmploye.aspx    # Ajout d'employés
│   │   ├── ListeEmployes.aspx     # Liste des employés
│   │   └── EditEmployee.aspx     # Modification des employés
│   ├── pointage/                   # Module de pointage
│   │   ├── PointageEquipe.aspx    # Pointage par équipe
│   │   ├── PointageProduction.aspx # Pointage de production
│   │   ├── PointageHeuresSupp.aspx # Heures supplémentaires
│   │   ├── MonEquipe.aspx         # Gestion de l'équipe
│   │   └── SupervisionPointage.aspx # Supervision
│   ├── pointagfix/                 # Module pointage ports
│   │   ├── PointageEmployesPorts.aspx
│   │   └── AssignerEmployesPorts.aspx
│   ├── reporting/                  # Module de rapports
│   │   └── RaportPointagePort.aspx
│   ├── assets/                     # Ressources statiques
│   │   ├── css/                   # Feuilles de style
│   │   ├── js/                    # Scripts JavaScript
│   │   ├── images/                # Images et icônes
│   │   └── libs/                  # Librairies tierces
│   ├── connexion.aspx             # Page de connexion
│   ├── Default.aspx               # Page d'accueil
│   ├── Site.Master                # Template principal
│   └── Web.config                 # Configuration
├── AppPointage/                    # Application de pointage dédiée
└── SQLQuery3.sql                  # Scripts de base de données
```

## 🗄 Base de données

La base de données `ERP_RH_Pointag_V2` comprend les tables principales :

- **EMPLOYE** : Données des employés
- **SOCIETE** : Informations des sociétés
- **DEPARTEMENT** : Départements organisationnels
- **SERVICE** : Services par département
- **CELLULE** : Cellules de travail
- **FONCTION** : Fonctions et postes
- **TYPEOUVRIER** : Types d'ouvriers
- **AUDIT_EMPLOYE** : Audit des modifications

## ⚙️ Installation et configuration

### Prérequis
- Windows Server ou Windows 10/11
- IIS (Internet Information Services)
- SQL Server 2016 ou plus récent
- .NET Framework 4.6.1

### Configuration
1. **Base de données**
   ```sql
   -- Exécuter le script SQLQuery3.sql pour créer la structure
   -- Modifier la chaîne de connexion dans Web.config
   ```

2. **Web.config**
   ```xml
   <connectionStrings>
     <add name="ConnectionString" 
          connectionString="Data Source=.;Initial Catalog=ERP_RH_Pointag_V2;Integrated Security=True;Connect Timeout=320" 
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

3. **IIS**
   - Créer une application dans IIS
   - Pointer vers le dossier `APPLICATION RH`
   - Configurer le pool d'applications (.NET Framework v4.0)

### Première connexion
- URL par défaut : `/connexion.aspx`
- Créer un utilisateur administrateur dans la table EMPLOYE
- Les rôles disponibles incluent : admin, chef_equipe, securityPortA

## 👥 Rôles et permissions

### Administrateur
- Accès complet à toutes les fonctionnalités
- Gestion des employés et de l'organisation
- Configuration du système

### Chef d'équipe
- Gestion de son équipe
- Pointage des membres de l'équipe
- Rapports de son équipe

### Sécurité Port (securityPortA)
- Accès spécialisé au module de pointage des ports
- Redirection automatique vers `/pointagfix/PointageEmployesPorts.aspx`

### Employé
- Accès limité aux fonctionnalités de base
- Consultation de ses propres données

## 🚦 Démarrage

1. Cloner le repository
2. Configurer SQL Server et exécuter les scripts de base de données
3. Modifier les chaînes de connexion
4. Déployer sur IIS
5. Accéder à l'application via `/connexion.aspx`

## 🔧 Maintenance

### Logs et monitoring
- Les erreurs sont gérées via try-catch dans le code
- Utilisation de SweetAlert2 pour les notifications utilisateur
- Session timeout configuré à 30 minutes

### Sauvegarde
- Sauvegarder régulièrement la base de données SQL Server
- Maintenir les fichiers d'application et de configuration

## 📝 Notes techniques

- **Architecture** : Application Web Forms traditionnelle avec code-behind C#
- **Sécurité** : Hachage des mots de passe, sessions sécurisées, validation des entrées
- **Performance** : Utilisation d'ADO.NET pour l'accès aux données optimisé
- **Compatibilité** : Testé sur IIS 8.5+ et SQL Server 2016+

## 🤝 Support

Pour le support technique ou les questions sur l'implémentation, référez-vous à la documentation interne ou contactez l'équipe de développement.

---

*Développé pour la gestion efficace des ressources humaines dans l'environnement .NET Framework*