# Padel Manager

Application de gestion de terrains de padel — réservations, paiements et statistiques.

## Architecture

```
Padel/
├── src/
│   ├── Padel.Domain/          # Entités et enums
│   ├── Padel.Application/     # Interfaces, Services, DTOs
│   ├── Padel.Infrastructure/  # Repositories, EF Core, Jobs
│   ├── Padel.Api/             # REST API (ASP.NET Core)
│   ├── Padel.Frontend/        # Blazor WebAssembly
│   └── Padel.Tests/           # Tests unitaires (xUnit + Moq)
└── docs/                      # Documentation technique
```

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB inclus avec Visual Studio)

## Lancement

### 1. Base de données

**Option A : Initialisation automatique (recommandé)**

L'API initialise automatiquement la base de données au premier démarrage, incluant :
- Création du schéma (tables, contraintes, index)
- Insertion de données de démonstration si la base est vide

Il suffit de lancer directement l'API (étape 2). La base sera créée automatiquement.

**Option B : Initialisation manuelle**

Si vous préférez contrôler l'initialisation manuellement :

```bash
# 1. Créer le schéma
sqlcmd -S "(localdb)\MSSQLLocalDB" -i src/Padel.Infrastructure/Data/Scripts/InitializeDatabase.sql

# 2. (Optionnel) Insérer des données de démonstration
sqlcmd -S "(localdb)\MSSQLLocalDB" -i src/Padel.Infrastructure/Data/Scripts/SeedData.sql
```

Puis désactiver l'auto-seeding dans `appsettings.json` :
```json
"Database": {
  "AutoSeedData": false
}
```

### 2. API (back-end)

```bash
cd src/Padel.Api
dotnet run
```

L'API démarre sur `https://localhost:7xxx` (port affiché dans la console).

### 3. Frontend (Blazor WebAssembly)

```bash
cd src/Padel.Frontend
dotnet run
```

Le frontend démarre sur `https://localhost:7271`.

## Utilisateurs SQL

| Utilisateur | Droits | Usage |
|---|---|---|
| `padel_app_user` | CRUD (SELECT, INSERT, UPDATE, DELETE) | API applicative |
| `padel_readonly` | SELECT uniquement | Reporting / statistiques |

## Tests

```bash
dotnet test src/Padel.Tests
```

## Fonctionnalités principales

- **Gestion des sites** : CRUD sites, terrains, horaires, jours de fermeture
- **Gestion des membres** : 3 types (Global, Site, Libre) avec matricule auto-généré
- **Réservations** : Matchs privés/publics avec créneaux calculés automatiquement
- **Paiements** : 15 € par joueur, report de solde, blocage si impayé
- **Règles J-1** : Conversion privé→public, retrait joueurs impayés
- **Statistiques** : Dashboard global et par site avec chiffre d'affaires

## Données de démonstration

Au premier lancement, l'application initialise automatiquement :
- **3 sites** : Bruxelles, Liège, Namur
- **7 terrains** répartis sur les sites
- **14 membres** : 5 Global, 5 Site, 3 Libre, dont 1 bloqué
- **7 matchs** variés (privés/publics, complets/incomplets, futurs/passés)
- **22 paiements** (210 € payés, 120 € en attente)

Cette initialisation ne se produit que si la base est vide. Pour réinitialiser, supprimez la base `PadelDb` et relancez l'API.
