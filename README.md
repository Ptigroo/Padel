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

Exécuter le script SQL pour créer la base et les utilisateurs :

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -i src/Padel.Infrastructure/Data/Scripts/InitializeDatabase.sql
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
