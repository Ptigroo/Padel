# Analyse du projet — Gestion de terrains de Padel

## Table des matières

1. [Vue d'ensemble](#vue-densemble)
2. [Feature 1 — Gestion des sites](#feature-1--gestion-des-sites)
3. [Feature 2 — Gestion des terrains](#feature-2--gestion-des-terrains)
4. [Feature 3 — Gestion des horaires](#feature-3--gestion-des-horaires)
5. [Feature 4 — Gestion des jours de fermeture](#feature-4--gestion-des-jours-de-fermeture)
6. [Feature 5 — Gestion des membres](#feature-5--gestion-des-membres)
7. [Feature 6 — Gestion des réservations (matchs)](#feature-6--gestion-des-réservations-matchs)
8. [Feature 7 — Gestion des paiements](#feature-7--gestion-des-paiements)
9. [Feature 8 — Règles automatiques J-1](#feature-8--règles-automatiques-j-1)
10. [Feature 9 — Interface utilisateur (Frontend)](#feature-9--interface-utilisateur-frontend)
11. [Feature 10 — Interface administrateur (Frontend)](#feature-10--interface-administrateur-frontend)
12. [Feature 11 — Statistiques et chiffre d'affaires](#feature-11--statistiques-et-chiffre-daffaires)
13. [Feature 12 — Sécurité base de données](#feature-12--sécurité-base-de-données)
14. [Feature 13 — Tests](#feature-13--tests)
15. [Feature 14 — Infrastructure et DevOps](#feature-14--infrastructure-et-devops)
16. [Modèle de données](#modèle-de-données)
17. [Priorisation](#priorisation)

---

## Vue d'ensemble

Le projet consiste à développer une application de gestion de terrains de padel avec :

- **Back-end** : REST API (.NET 10 / C# 14) avec architecture en couches (Controllers → Services → Repositories → Models)
- **Front-end** : Blazor WebAssembly (application séparée)
- **Base de données** : SQL Server relationnelle avec Entity Framework Core
- **Pas de login** : identification par matricule uniquement

---

## Feature 1 — Gestion des sites

> Un gestionnaire possède plusieurs sites de terrains de padel.

### Tâches back-end

| # | Tâche | Détail |
|---|-------|--------|
| 1.1 | Modèle `Site` | Entité avec `Id`, `Name`, `Address` |
| 1.2 | DTO `SiteDto`, `CreateSiteDto`, `UpdateSiteDto` | Objets de transfert pour l'API |
| 1.3 | Repository `ISiteRepository` / `SiteRepository` | CRUD + requêtes avec Include (Courts, Schedules, ClosureDays) |
| 1.4 | Service `ISiteService` / `SiteService` | Logique métier : création, mise à jour, suppression, mapping DTO |
| 1.5 | Controller `SitesController` | Endpoints : `GET /api/sites`, `GET /api/sites/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| 1.6 | Configuration EF Core | Clé primaire, contraintes `IsRequired`, `HasMaxLength` |
| 1.7 | Tests unitaires | Tests du controller, du service, du repository |

### Tâches front-end

| # | Tâche | Détail |
|---|-------|--------|
| 1.8 | Page admin sites | Liste, création, modification, suppression de sites |

---

## Feature 2 — Gestion des terrains

> Chaque site a un nombre différent de terrains.

### Tâches back-end

| # | Tâche | Détail |
|---|-------|--------|
| 2.1 | Modèle `Court` | Entité avec `Id`, `Name`, `SiteId` + relation FK vers `Site` |
| 2.2 | DTO `CourtDto`, `CreateCourtDto` | Objets de transfert |
| 2.3 | Repository générique `IRepository<Court>` | Utilisation du repository générique |
| 2.4 | Service `ICourtService` / `CourtService` | CRUD + calcul des créneaux disponibles |
| 2.5 | Controller `CourtsController` | Endpoints : `GET /api/courts/site/{siteId}`, `GET/{id}`, `POST`, `DELETE/{id}` |
| 2.6 | Endpoint créneaux | `GET /api/courts/{id}/slots/{date}` — retourne les créneaux libres pour une date |
| 2.7 | Logique créneaux | Slots de 1h30 + 15 min de pause, basés sur les horaires du site et les matchs existants |
| 2.8 | Vérification jour de fermeture | Aucun créneau si jour de fermeture (global ou site) |
| 2.9 | Configuration EF Core | FK `Court → Site` avec `OnDelete(Restrict)` |
| 2.10 | Tests unitaires | Tests du service (calcul créneaux), controller, repository |

### Tâches front-end

| # | Tâche | Détail |
|---|-------|--------|
| 2.11 | Gestion terrains dans page admin sites | Ajout/suppression de terrains pour un site sélectionné |

---

## Feature 3 — Gestion des horaires

> Les heures de début et de fin de réservation sont spécifiques à chaque site, par année civile.

### Tâches back-end

| # | Tâche | Détail |
|---|-------|--------|
| 3.1 | Modèle `SiteSchedule` | `Id`, `SiteId`, `Year`, `StartTime` (TimeOnly), `EndTime` (TimeOnly) |
| 3.2 | DTO `SiteScheduleDto`, `CreateSiteScheduleDto` | Objets de transfert |
| 3.3 | Repository générique | `IRepository<SiteSchedule>` |
| 3.4 | Service `IScheduleService` / `ScheduleService` | Création avec vérification d'unicité (un seul horaire par site/année) |
| 3.5 | Controller `SchedulesController` | `GET /api/schedules/site/{siteId}`, `POST`, `DELETE/{id}` |
| 3.6 | Index unique EF Core | Contrainte unique sur `(SiteId, Year)` |
| 3.7 | Tests unitaires | Vérification de la contrainte d'unicité, CRUD |

### Tâches front-end

| # | Tâche | Détail |
|---|-------|--------|
| 3.8 | Gestion horaires dans page admin sites | Ajout/suppression d'horaires annuels pour un site |

---

## Feature 4 — Gestion des jours de fermeture

> Pour chaque site et de manière globale, prévoir des jours de fermeture.

### Tâches back-end

| # | Tâche | Détail |
|---|-------|--------|
| 4.1 | Modèle `ClosureDay` | `Id`, `Date` (DateOnly), `Reason`, `SiteId` (nullable — null = global) |
| 4.2 | DTO `ClosureDayDto`, `CreateClosureDayDto` | Objets de transfert |
| 4.3 | Service `IClosureDayService` / `ClosureDayService` | Liste filtrée par site (inclut les globaux), création, suppression |
| 4.4 | Controller `ClosureDaysController` | `GET /api/closuredays?siteId=`, `POST`, `DELETE/{id}` |
| 4.5 | Intégration réservation | Vérifier les jours de fermeture lors de la création d'un match |
| 4.6 | Intégration créneaux | Aucun créneau retourné si le jour est fermé |
| 4.7 | Tests unitaires | Test de filtrage global + site, intégration avec réservation |

### Tâches front-end

| # | Tâche | Détail |
|---|-------|--------|
| 4.8 | Gestion fermetures dans page admin sites | Ajout/suppression, affichage (global vs site spécifique) |

---

## Feature 5 — Gestion des membres

> 3 types de membres : Global (Gxxxx), Site (Sxxxxx), Libre (Lxxxxx).

### Tâches back-end

| # | Tâche | Détail |
|---|-------|--------|
| 5.1 | Modèle `Member` | `Id`, `Matricule`, `FirstName`, `LastName`, `Email`, `MemberType`, `SiteId`, `ReservationBlocked`, `BlockedUntil` |
| 5.2 | Enum `MemberType` | `Global`, `Site`, `Libre` |
| 5.3 | Génération du matricule | Format automatique : `G0001`, `S00001`, `L00001` selon le type |
| 5.4 | DTO `MemberDto`, `CreateMemberDto`, `UpdateMemberDto` | Objets de transfert |
| 5.5 | Repository `IMemberRepository` / `MemberRepository` | Recherche par matricule, par site, avec include |
| 5.6 | Service `IMemberService` / `MemberService` | Création avec validation (Site nécessite SiteId), génération matricule |
| 5.7 | Controller `MembersController` | `GET`, `GET/site/{siteId}`, `GET/{matricule}`, `POST`, `PUT/{matricule}`, `DELETE/{matricule}` |
| 5.8 | Validation | Membre Site doit avoir un `SiteId` |
| 5.9 | Index unique EF Core | Matricule unique |
| 5.10 | Tests unitaires | Génération matricule, validation type/site, CRUD |

### Règles de visibilité

| Type | Visible | Peut réserver | Délai réservation |
|------|---------|---------------|-------------------|
| Global (`Gxxxx`) | Tous les sites | N'importe quel site | 3 semaines avant |
| Site (`Sxxxxx`) | Tous les sites | Son site uniquement | 2 semaines avant |
| Libre (`Lxxxxx`) | Tous les sites | N'importe quel site | 5 jours avant |

### Tâches front-end

| # | Tâche | Détail |
|---|-------|--------|
| 5.11 | Page admin membres | Création, liste, filtrage par site, suppression |
| 5.12 | Affichage blocage | Badge visuel si membre bloqué + date de fin |

---

## Feature 6 — Gestion des réservations (matchs)

> Cœur du projet. Matchs privés ou publics avec règles spécifiques.

### Tâches back-end

| # | Tâche | Détail |
|---|-------|--------|
| 6.1 | Modèle `Match` | `Id`, `CourtId`, `OrganizerId`, `ScheduledAt`, `EndsAt`, `MatchType`, `Status` |
| 6.2 | Modèle `MatchPlayer` | Table de jointure `Match ↔ Member` avec `JoinedAt` |
| 6.3 | Enums `MatchType`, `MatchStatus` | Private/Public, Scheduled/Full/Completed/Cancelled |
| 6.4 | DTO `MatchDto`, `CreateMatchDto`, `AddPlayerDto`, `MatchPlayerDto` | Objets de transfert |
| 6.5 | Repository `IMatchRepository` / `MatchRepository` | Requêtes spécialisées avec Include complet (Court→Site, Organizer, Players→Member→Payment) |
| 6.6 | Service `IMatchService` / `MatchService` | Logique métier centrale (voir sous-tâches ci-dessous) |
| 6.7 | Controller `MatchesController` | Endpoints listés ci-dessous |

### Sous-tâches du service de réservation

| # | Tâche | Détail |
|---|-------|--------|
| 6.8 | Création de match | Validation : blocage organisateur, solde impayé, accès site, fenêtre de réservation, horaire, fermeture, disponibilité créneau |
| 6.9 | Auto-inscription organisateur | L'organisateur est automatiquement ajouté comme premier joueur |
| 6.10 | Création paiement automatique | Un paiement `Pending` de 15 € est créé pour chaque joueur ajouté |
| 6.11 | Match privé — ajout joueur | Seul l'organisateur peut ajouter des joueurs via `POST /api/matches/{id}/players` |
| 6.12 | Match public — rejoindre | N'importe quel membre rejoint via `POST /api/matches/{id}/join/{matricule}` |
| 6.13 | Match public — interdiction | L'organisateur ne peut PAS ajouter d'autres joueurs dans un match public |
| 6.14 | Validation fenêtre de réservation | Global = 21 jours, Site = 14 jours, Libre = 5 jours max avant le match |
| 6.15 | Validation accès site | Membre Site ne peut réserver que sur son propre site |
| 6.16 | Validation blocage | Vérifier `ReservationBlocked` et `BlockedUntil` avant de créer un match |
| 6.17 | Validation solde | Organisateur avec solde impayé ne peut pas réserver |
| 6.18 | Passage à `Full` | Quand 4 joueurs sont inscrits, le statut passe à `Full` |

### Endpoints

| Méthode | Route | Description |
|---------|-------|-------------|
| `GET` | `/api/matches` | Tous les matchs (admin) |
| `GET` | `/api/matches/{id}` | Détail d'un match |
| `GET` | `/api/matches/public?siteId=` | Matchs publics (filtrable par site) |
| `GET` | `/api/matches/organizer/{matricule}` | Matchs organisés par un membre |
| `GET` | `/api/matches/player/{matricule}` | Matchs d'un joueur |
| `GET` | `/api/matches/site/{siteId}` | Matchs d'un site |
| `POST` | `/api/matches` | Créer un match |
| `POST` | `/api/matches/{id}/players` | Ajouter un joueur (privé) |
| `POST` | `/api/matches/{id}/join/{matricule}` | Rejoindre un match (public) |
| `POST` | `/api/matches/process-day-before` | Déclencher les règles J-1 manuellement |

### Tâches front-end

| # | Tâche | Détail |
|---|-------|--------|
| 6.19 | Page réservation | Sélection site → terrain → date → créneau → type → création |
| 6.20 | Ajout joueurs (privé) | Formulaire d'ajout de matricule après création d'un match privé |
| 6.21 | Page matchs publics | Liste des matchs publics avec filtre par site, bouton "Rejoindre" |
| 6.22 | Page mes matchs | Recherche par matricule, affichage rôle (organisateur/joueur) |

### Tests

| # | Tâche | Détail |
|---|-------|--------|
| 6.23 | Tests service — création match | Cas nominal + tous les cas de rejet (blocage, solde, site, fenêtre, horaire, fermeture, créneau pris) |
| 6.24 | Tests service — ajout joueur privé | Cas nominal + rejet si public, si complet, si déjà inscrit |
| 6.25 | Tests service — rejoindre public | Cas nominal + rejet si privé, si complet, si déjà inscrit |
| 6.26 | Tests controller | Vérification des codes HTTP (201, 400, 404) |
| 6.27 | Tests repository | Vérification des requêtes Include, filtres |

---

## Feature 7 — Gestion des paiements

> 60 € par match, divisé en 4 × 15 €. Paiement à l'avance obligatoire.

### Tâches back-end

| # | Tâche | Détail |
|---|-------|--------|
| 7.1 | Modèle `Payment` | `Id`, `MatchPlayerId`, `MatchId`, `MemberId`, `Amount`, `Status`, `CreatedAt`, `PaidAt` |
| 7.2 | Enum `PaymentStatus` | `Pending`, `Paid`, `Refunded` |
| 7.3 | DTO `PaymentDto`, `ProcessPaymentDto` | Objets de transfert |
| 7.4 | Repository `IPaymentRepository` / `PaymentRepository` | Par membre, par match, solde impayé, revenus par site/global |
| 7.5 | Service `IPaymentService` / `PaymentService` | Logique de paiement (voir sous-tâches) |
| 7.6 | Controller `PaymentsController` | Endpoints ci-dessous |

### Sous-tâches du service de paiement

| # | Tâche | Détail |
|---|-------|--------|
| 7.7 | Paiement d'un joueur | `POST /api/payments/pay` — marque le paiement comme `Paid` |
| 7.8 | Report de solde | Si l'organisateur a un solde impayé d'un autre match public, celui-ci s'ajoute au montant lors du prochain paiement |
| 7.9 | Solde bloque réservation | Un organisateur avec solde > 0 ne peut pas créer de nouveau match |
| 7.10 | Calcul solde | `GET /api/payments/balance/{matricule}` — retourne le solde dû |
| 7.11 | Match public incomplet | Si < 4 joueurs à la date du match, l'organisateur doit payer le solde restant (places non remplies) |

### Endpoints

| Méthode | Route | Description |
|---------|-------|-------------|
| `POST` | `/api/payments/pay` | Effectuer un paiement |
| `GET` | `/api/payments/member/{matricule}` | Paiements d'un membre |
| `GET` | `/api/payments/match/{matchId}` | Paiements d'un match |
| `GET` | `/api/payments/balance/{matricule}` | Solde dû d'un membre |

### Tâches front-end

| # | Tâche | Détail |
|---|-------|--------|
| 7.12 | Page mes paiements | Liste des paiements, solde dû, bouton "Payer" |

### Tests

| # | Tâche | Détail |
|---|-------|--------|
| 7.13 | Tests service — paiement | Cas nominal, double paiement, report de solde |
| 7.14 | Tests service — solde | Calcul correct du solde impayé |
| 7.15 | Tests controller | Codes HTTP, erreurs métier |

---

## Feature 8 — Règles automatiques J-1

> La veille du match, des règles s'appliquent automatiquement.

### Tâches back-end

| # | Tâche | Détail |
|---|-------|--------|
| 8.1 | Règle 1 : privé → public | Si un match privé n'a pas 4 joueurs la veille → il devient public |
| 8.2 | Pénalité organisateur | L'organisateur d'un match privé incomplet reçoit un blocage de réservation d'1 semaine |
| 8.3 | Règle 2 : joueurs impayés | Si un joueur n'a pas payé la veille → il est retiré du match, sa place redevient libre |
| 8.4 | Transition de statut | Si un match passe en dessous de 4 joueurs suite au retrait → redevient `Public` + `Scheduled` |
| 8.5 | Background job `DayBeforeMatchJob` | Service hébergé qui exécute les règles périodiquement (toutes les heures) |
| 8.6 | Endpoint manuel | `POST /api/matches/process-day-before` pour déclencher manuellement (utile en dev/admin) |
| 8.7 | Repository — requêtes spécialisées | `GetMatchesBecomingPublicAsync()`, `GetMatchesWithUnpaidPlayersAsync()` |

### Tests

| # | Tâche | Détail |
|---|-------|--------|
| 8.8 | Test règle privé → public | Vérifier le changement de type et la pénalité |
| 8.9 | Test retrait joueur impayé | Vérifier le retrait et la transition de statut |
| 8.10 | Test match complet payé | Aucun changement ne doit se produire |

---

## Feature 9 — Interface utilisateur (Frontend)

> Première interface : pour les utilisateurs (joueurs).

### Pages

| # | Tâche | Route | Détail |
|---|-------|-------|--------|
| 9.1 | Accueil | `/` | Informations générales, liens rapides, règles (tarifs, durée, délais) |
| 9.2 | Matchs publics | `/matches/public` | Liste des matchs publics, filtre par site, bouton "Rejoindre" avec saisie matricule |
| 9.3 | Réservation | `/reservations` | Wizard : site → terrain → date → créneau → type → création. Puis ajout joueurs si privé |
| 9.4 | Mes matchs | `/my-matches` | Saisie matricule → liste des matchs (organisateur ou joueur), statut, type |
| 9.5 | Mes paiements | `/my-payments` | Saisie matricule → solde dû, liste des paiements, bouton "Payer" |

### Composants transversaux

| # | Tâche | Détail |
|---|-------|--------|
| 9.6 | NavMenu | Menu latéral avec liens utilisateur + admin |
| 9.7 | MainLayout | Layout principal (sidebar + barre de navigation + contenu) |
| 9.8 | ApiService | Service HTTP centralisé pour tous les appels API |
| 9.9 | Modèles DTO front | Miroir des DTOs de l'API pour la désérialisation |
| 9.10 | Gestion d'erreurs | Affichage des erreurs API dans des alertes Bootstrap |

---

## Feature 10 — Interface administrateur (Frontend)

> Deuxième interface : pour les administrateurs (global et par site).

### Pages

| # | Tâche | Route | Détail |
|---|-------|-------|--------|
| 10.1 | Gestion des sites | `/admin/sites` | CRUD sites + terrains + horaires + jours de fermeture |
| 10.2 | Gestion des membres | `/admin/members` | Création, liste, filtre par site/type, suppression, affichage blocage |
| 10.3 | Tous les matchs | `/admin/matches` | Liste de tous les matchs, filtre par site, détails (modal), déclenchement règles J-1 |
| 10.4 | Statistiques | `/admin/stats` | Dashboard global + détails par site |

### Tâches techniques

| # | Tâche | Détail |
|---|-------|--------|
| 10.5 | Admin global vs site | L'admin global voit et gère tous les sites ; l'admin site ne voit que son site |
| 10.6 | Distinction visuelle | Badges, couleurs, icônes pour différencier types, statuts, paiements |

---

## Feature 11 — Statistiques et chiffre d'affaires

> L'administrateur peut voir les statistiques et le chiffre d'affaires.

### Tâches back-end

| # | Tâche | Détail |
|---|-------|--------|
| 11.1 | Service `IStatsService` / `StatsService` | Agrégation des données par site et globalement |
| 11.2 | DTO `GlobalStatsDto`, `SiteStatsDto` | Total sites, matchs, revenus, membres + détail par site |
| 11.3 | Controller `StatsController` | `GET /api/stats` (global), `GET /api/stats/site/{siteId}` |
| 11.4 | Calcul revenus | Somme des paiements `Paid`, par site et globalement |
| 11.5 | Comptage matchs par statut | Scheduled, Full, Completed, Cancelled par site |

### Tâches front-end

| # | Tâche | Détail |
|---|-------|--------|
| 11.6 | Dashboard global | Cartes : total sites, matchs, membres, CA |
| 11.7 | Détails par site | Cartes individuelles avec breakdown matchs + revenus + membres |

### Tests

| # | Tâche | Détail |
|---|-------|--------|
| 11.8 | Tests service stats | Vérification des agrégations |
| 11.9 | Tests controller | Codes HTTP, réponses |

---

## Feature 12 — Sécurité base de données

> Ne pas utiliser un user avec tous les droits. Users spécifiques avec droits spécifiques.

### Tâches

| # | Tâche | Détail |
|---|-------|--------|
| 12.1 | Script SQL création utilisateurs | Deux logins/users SQL : `padel_app_user` (CRUD) et `padel_readonly` (SELECT uniquement) |
| 12.2 | Droits granulaires | `GRANT SELECT, INSERT, UPDATE, DELETE` table par table pour l'utilisateur applicatif |
| 12.3 | Droits lecture seule | `GRANT SELECT` table par table pour l'utilisateur stats/reporting |
| 12.4 | Connection string spécifique | Utiliser `padel_app_user` dans la connection string de l'API (pas `sa` ni `dbo`) |
| 12.5 | Documentation du choix | Accès direct aux tables vs procédures stockées : avantages/inconvénients (justifier) |

### Choix de conception à documenter

| Approche | Avantages | Inconvénients |
|----------|-----------|---------------|
| Accès direct aux tables | Simple, compatible EF Core, pas de duplication de logique | L'utilisateur peut exécuter des requêtes non prévues |
| Procédures stockées uniquement | Contrôle total sur les opérations possibles | Complexité, duplication de logique, moins compatible avec l'ORM |

---

## Feature 13 — Tests

> Tests obligatoires dans la partie back-end (controllers, services, repositories).

### Tâches

| # | Tâche | Détail |
|---|-------|--------|
| 13.1 | Projet de test | Créer un projet xUnit/NUnit séparé (ex: `PadelManager.Tests`) |
| 13.2 | Mocking | Utiliser Moq ou NSubstitute pour mocker les dépendances (repositories dans les services, services dans les controllers) |
| 13.3 | Base en mémoire | Utiliser `UseInMemoryDatabase` ou SQLite in-memory pour les tests de repositories |
| 13.4 | Tests controllers | Vérifier les codes HTTP retournés (200, 201, 400, 404, 204) |
| 13.5 | Tests services | Vérifier toute la logique métier (validations, calculs, règles) |
| 13.6 | Tests repositories | Vérifier les requêtes (filtres, includes, agrégations) |
| 13.7 | Couverture métier critique | Priorité : MatchService, PaymentService, règles J-1 |

### Scénarios de test prioritaires

| Scénario | Service | Ce qu'on vérifie |
|----------|---------|-----------------|
| Créer un match — cas nominal | MatchService | Match créé, organisateur ajouté, paiement créé |
| Créer un match — membre bloqué | MatchService | Rejet avec message d'erreur |
| Créer un match — solde impayé | MatchService | Rejet avec message d'erreur |
| Créer un match — mauvais site (membre site) | MatchService | Rejet |
| Créer un match — hors fenêtre | MatchService | Rejet selon le type de membre |
| Créer un match — jour de fermeture | MatchService | Rejet |
| Créer un match — créneau pris | MatchService | Rejet |
| Rejoindre un match public | MatchService | Joueur ajouté, paiement créé |
| Ajouter joueur match privé | MatchService | Joueur ajouté par l'organisateur |
| Payer — cas nominal | PaymentService | Statut `Paid`, date de paiement |
| Payer — double paiement | PaymentService | Rejet |
| Payer — avec report de solde | PaymentService | Montant augmenté du solde dû |
| Règle J-1 — privé incomplet | MatchService | Devient public, organisateur bloqué 7 jours |
| Règle J-1 — joueur impayé | MatchService | Joueur retiré, match redevient ouvert |
| Calcul créneaux | CourtService | Slots corrects selon horaires, matchs existants, fermetures |

---

## Feature 14 — Infrastructure et DevOps

### Tâches

| # | Tâche | Détail |
|---|-------|--------|
| 14.1 | Git | Dépôt avec historique propre, branches, commits significatifs |
| 14.2 | Issues Git | Une issue par feature/tâche majeure dans GitHub |
| 14.3 | Séparation front/back | Deux projets distincts (PadelManager API + PadelFrontend Blazor WASM) |
| 14.4 | CORS | Configuration CORS dans l'API pour autoriser le frontend |
| 14.5 | Connection string | Configuration via `appsettings.json`, pas de mot de passe en dur en production |
| 14.6 | EnsureCreated / Migrations | Création automatique de la base en dev (`EnsureCreated`) ou migrations EF Core |
| 14.7 | README | Documentation de lancement (API + Frontend) |
| 14.8 | Injection de dépendances | Enregistrement de tous les services/repos dans `Program.cs` |

---

## Modèle de données

```
┌──────────────┐       ┌──────────────┐       ┌──────────────────┐
│    Site       │1────*│    Court      │1────*│     Match         │
│──────────────│       │──────────────│       │──────────────────│
│ Id           │       │ Id           │       │ Id               │
│ Name         │       │ Name         │       │ CourtId (FK)     │
│ Address      │       │ SiteId (FK)  │       │ OrganizerId (FK) │
└──────┬───────┘       └──────────────┘       │ ScheduledAt      │
       │                                       │ EndsAt           │
       │1                                      │ MatchType        │
       │                                       │ Status           │
       ├────* SiteSchedule                     └────────┬─────────┘
       │      │ Id, SiteId, Year,                       │1
       │      │ StartTime, EndTime                      │
       │                                                │
       ├────* ClosureDay                        ┌───────┴────────┐
       │      │ Id, Date, Reason,               │  MatchPlayer   │*
       │      │ SiteId (nullable)               │────────────────│
       │                                        │ Id             │
       └────* Member                            │ MatchId (FK)   │
              │ Id, Matricule,                  │ MemberId (FK)  │
              │ FirstName, LastName,            │ JoinedAt       │
              │ Email, MemberType,              └───────┬────────┘
              │ SiteId, ReservationBlocked,             │1
              │ BlockedUntil                            │
              │                                 ┌───────┴────────┐
              └──────────────────────────────*│    Payment      │
                                               │────────────────│
                                               │ Id             │
                                               │ MatchPlayerId  │
                                               │ MatchId (FK)   │
                                               │ MemberId (FK)  │
                                               │ Amount         │
                                               │ Status         │
                                               │ CreatedAt      │
                                               │ PaidAt         │
                                               └────────────────┘
```

### Relations clés

| Relation | Type | Détail |
|----------|------|--------|
| Site → Court | 1:N | `OnDelete(Restrict)` — ne pas supprimer un site avec des terrains |
| Site → SiteSchedule | 1:N | `OnDelete(Cascade)` — unique par `(SiteId, Year)` |
| Site → ClosureDay | 1:N | `OnDelete(Cascade)` — `SiteId` nullable (null = global) |
| Site → Member | 1:N | `OnDelete(SetNull)` — membre Site rattaché à un site |
| Court → Match | 1:N | `OnDelete(Restrict)` |
| Member → Match (Organizer) | 1:N | `OnDelete(Restrict)` |
| Match → MatchPlayer | 1:N | `OnDelete(Cascade)` — unique par `(MatchId, MemberId)` |
| Member → MatchPlayer | 1:N | `OnDelete(Restrict)` |
| MatchPlayer → Payment | 1:1 | `OnDelete(Cascade)` |
| Match → Payment | 1:N | `OnDelete(Restrict)` |
| Member → Payment | 1:N | `OnDelete(Restrict)` |

---

## Priorisation

Basée sur l'énoncé : *"la priorité est la gestion des réservations, des paiements, des stats"*.

| Priorité | Feature | Justification |
|----------|---------|---------------|
| 🔴 P0 | Feature 12 — Sécurité DB | Prérequis technique (users SQL) |
| 🔴 P0 | Feature 14 — Infrastructure | Git, séparation front/back, DI, CORS |
| 🔴 P0 | Feature 1 — Sites | Fondation : tout repose sur les sites |
| 🔴 P0 | Feature 2 — Terrains | Fondation : les matchs se jouent sur des terrains |
| 🔴 P0 | Feature 3 — Horaires | Nécessaire pour calculer les créneaux |
| 🔴 P0 | Feature 5 — Membres | Nécessaire pour les réservations |
| 🟠 P1 | Feature 6 — Réservations | **Priorité projet** — cœur métier |
| 🟠 P1 | Feature 7 — Paiements | **Priorité projet** — couplé aux réservations |
| 🟠 P1 | Feature 8 — Règles J-1 | **Priorité projet** — logique métier critique |
| 🟡 P2 | Feature 4 — Jours de fermeture | Important mais secondaire |
| 🟡 P2 | Feature 11 — Statistiques | **Priorité projet** — demandé explicitement |
| 🟢 P3 | Feature 9 — Frontend utilisateur | Interface utilisateur |
| 🟢 P3 | Feature 10 — Frontend admin | Interface administrateur |
| 🟢 P3 | Feature 13 — Tests | Obligatoires mais peuvent être écrits en parallèle |

### Ordre de développement recommandé

```
Sprint 1 : Infrastructure + Modèles + DB + Sites + Terrains + Horaires + Membres
Sprint 2 : Réservations + Paiements + Règles J-1
Sprint 3 : Jours de fermeture + Statistiques + Tests
Sprint 4 : Frontend utilisateur + Frontend admin + Tests complémentaires
```
