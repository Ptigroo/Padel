# Cahier des charges — Gestion de terrains de Padel

**Cours de Projet SGBD 2025 — EPHEC**

---

## Table des matières

1. [Contexte](#1-contexte)
   - 1.1 [Cadre](#11-cadre)
   - 1.2 [Projet](#12-projet)
2. [Analyse métier](#2-analyse-métier)
   - 2.1 [Description de la solution envisagée](#21-description-de-la-solution-envisagée)
   - 2.2 [Les intervenants](#22-les-intervenants)
   - 2.3 [Fonctions attendues](#23-fonctions-attendues)
   - 2.4 [Contraintes business](#24-contraintes-business)
3. [Analyse fonctionnelle](#3-analyse-fonctionnelle)
   - 3.1 [EF-001 — Gérer les sites](#31-ef-001--gérer-les-sites)
   - 3.2 [EF-002 — Gérer les terrains d'un site](#32-ef-002--gérer-les-terrains-dun-site)
   - 3.3 [EF-003 — Gérer les horaires d'un site](#33-ef-003--gérer-les-horaires-dun-site)
   - 3.4 [EF-004 — Gérer les jours de fermeture](#34-ef-004--gérer-les-jours-de-fermeture)
   - 3.5 [EF-005 — Gérer les membres](#35-ef-005--gérer-les-membres)
   - 3.6 [EF-006 — Réserver un terrain (créer un match)](#36-ef-006--réserver-un-terrain-créer-un-match)
   - 3.7 [EF-007 — Rejoindre un match public](#37-ef-007--rejoindre-un-match-public)
   - 3.8 [EF-008 — Ajouter un joueur à un match privé](#38-ef-008--ajouter-un-joueur-à-un-match-privé)
   - 3.9 [EF-009 — Payer sa part d'un match](#39-ef-009--payer-sa-part-dun-match)
   - 3.10 [EF-010 — Consulter ses matchs et paiements](#310-ef-010--consulter-ses-matchs-et-paiements)
   - 3.11 [EF-011 — Traitement automatique J-1](#311-ef-011--traitement-automatique-j-1)
   - 3.12 [EF-012 — Consulter les statistiques (admin)](#312-ef-012--consulter-les-statistiques-admin)
4. [Contraintes fonctionnelles](#4-contraintes-fonctionnelles)
   - 4.1 [Règles d'accès et autorisation](#41-règles-daccès-et-autorisation)
   - 4.2 [Règles de structure](#42-règles-de-structure)
   - 4.3 [Règles de validation](#43-règles-de-validation)
   - 4.4 [Règles de calcul](#44-règles-de-calcul)
5. [Description des entités](#5-description-des-entités)
6. [Schéma relationnel de la solution](#6-schéma-relationnel-de-la-solution)
   - 6.1 [Schéma EA](#61-schéma-ea)
   - 6.2 [Schéma relationnel](#62-schéma-relationnel)
   - 6.3 [Implémentation des contraintes](#63-implémentation-des-contraintes)
7. [Analyse technique](#7-analyse-technique)
   - 7.1 [Technologies proposées](#71-technologies-proposées)
   - 7.2 [Architecture applicative](#72-architecture-applicative)

---

## 1. Contexte

### 1.1 Cadre

Ce projet est réalisé dans le cadre du cours de Projet de développement SGBD 2024-2025, à l'EPHEC.

L'objectif de la réalisation de ce projet est de démontrer l'acquisition des compétences acquises au sein de ce cours : compréhension, acquisition et mise en pratique des concepts de développement SGBD.

### 1.2 Projet

Le projet consiste en la création d'une application web nommée **Padel Manager**, ayant pour objectif de permettre la gestion de sites de terrains de padel, de leurs membres et de leurs réservations.

La solution se compose de :
- Un **back-end** REST API développé en .NET 10 / C# 14, avec architecture en couches stricte
- Un **front-end** Blazor WebAssembly
- Une **base de données** SQL Server
- Des **tests unitaires** xUnit couvrant la logique métier critique

---

## 2. Analyse métier

### 2.1 Description de la solution envisagée

La solution permet la gestion centralisée de plusieurs sites de padel par un gestionnaire. Chaque site dispose de terrains, d'horaires d'ouverture définis par année civile, de jours de fermeture, et d'une liste de membres associés.

Les membres peuvent réserver des créneaux de jeu (matchs), payer leur part, et consulter leurs réservations et paiements. Des règles automatiques s'appliquent la veille de chaque match pour gérer les matchs incomplets et les joueurs impayés.

Les utilisateurs sont capables de :

1. **Via l'interface d'administration** : créer, modifier et supprimer des sites ; gérer les terrains, les horaires annuels et les jours de fermeture de chaque site ; gérer les membres ; consulter tous les matchs et déclencher les règles J-1 ; visualiser les statistiques et le chiffre d'affaires global et par site.

2. **Via l'interface membre** : consulter les matchs publics et rejoindre ceux qui ont encore des places disponibles ; créer une réservation (match privé ou public) sur un terrain ; consulter ses propres matchs ; consulter et payer ses paiements en attente.

### 2.2 Les intervenants

| Intervenant | Rôle |
|-------------|------|
| Gestionnaire | Accès complet à la gestion des sites, terrains, horaires, jours de fermeture, membres, matchs et statistiques via l'interface web |
| Membre Global (`Gxxxx`) | Peut réserver sur n'importe quel site, jusqu'à 3 semaines à l'avance |
| Membre Site (`Sxxxxx`) | Peut réserver uniquement sur son site d'appartenance, jusqu'à 2 semaines à l'avance |
| Membre Libre (`Lxxxxx`) | Peut réserver sur n'importe quel site, jusqu'à 5 jours à l'avance |

### 2.3 Fonctions attendues

#### Exigences fonctionnelles

| Code | Description |
|------|-------------|
| EF-001 | Créer, modifier et supprimer un site |
| EF-002 | Ajouter et supprimer des terrains pour un site |
| EF-003 | Définir les horaires d'ouverture d'un site par année civile |
| EF-004 | Gérer les jours de fermeture (globaux et par site) |
| EF-005 | Créer, modifier et supprimer un membre avec génération automatique du matricule |
| EF-006 | Réserver un terrain (créer un match privé ou public) |
| EF-007 | Rejoindre un match public existant |
| EF-008 | Ajouter un joueur à un match privé (par l'organisateur) |
| EF-009 | Payer sa part d'un match |
| EF-010 | Consulter ses matchs et ses paiements |
| EF-011 | Traitement automatique J-1 : conversion privé→public et retrait des joueurs impayés |
| EF-012 | Consulter les statistiques et le chiffre d'affaires (global et par site) |

#### Exigences non fonctionnelles

| Code | Description |
|------|-------------|
| ENF-001 | Aucune authentification requise pour accéder à l'application |
| ENF-002 | Les membres sont identifiés par leur matricule uniquement |
| ENF-003 | La base de données est accessible via un utilisateur SQL dédié à l'application (`padel_app_user`) |
| ENF-004 | Un utilisateur SQL en lecture seule (`padel_readonly`) est fourni pour le reporting |
| ENF-005 | Le back-end et le front-end sont deux projets distincts communiquant par HTTP |
| ENF-006 | L'application dispose d'une suite de tests unitaires couvrant les services et les controllers |

### 2.4 Contraintes business

**Point de vue métier :**

- Un match dure 1h30 ; une pause de 15 minutes est automatiquement réservée après chaque match sur un terrain.
- Un match coûte 60 € pour 4 joueurs, soit 15 € par joueur. Le paiement est obligatoire avant le début du match.
- Un organisateur ayant un solde impayé ne peut pas créer de nouveau match.
- Un membre bloqué ne peut pas créer de réservation pendant la durée du blocage.
- Un membre de type Site ne peut réserver que sur son propre site.
- Pour un match public incomplet (moins de 4 joueurs à la date du match), l'organisateur doit payer les places non remplies.

**Point de vue technique :**

- Architecture en couches obligatoire : Domain → Application (Interfaces + Services + DTOs) → Infrastructure (Repositories + Configurations) → API (Controllers)
- Séparation stricte entre le back-end (REST API) et le front-end (Blazor WebAssembly)
- Utilisation d'Entity Framework Core pour l'accès aux données
- Utilisation d'un utilisateur SQL dédié à l'application (`padel_app_user`)
- Script SQL de création de la base de données fourni séparément

---

## 3. Analyse fonctionnelle

### 3.1 EF-001 — Gérer les sites

Ce cas d'utilisation permet au gestionnaire de créer, modifier et supprimer des sites de padel.

**Déroulement :**

Le gestionnaire accède à la page **Sites** via le menu de navigation. La liste des sites existants s'affiche dans un tableau avec leurs colonnes : Nom et Adresse.

Pour **créer** un site, le gestionnaire clique sur « Nouveau site ». Un formulaire apparaît avec les champs Nom et Adresse. Après soumission, le site est créé et la liste se rafraîchit.

Pour **modifier** un site, le gestionnaire clique sur « Modifier » dans la ligne du site souhaité. Le formulaire se pré-remplit avec les données actuelles. Le gestionnaire effectue ses changements et soumet le formulaire.

Pour **supprimer** un site, le gestionnaire clique sur « Supprimer » dans la ligne correspondante. Le site est supprimé et la liste se rafraîchit. La suppression est refusée si des terrains sont encore associés au site.

Depuis la liste des sites, le gestionnaire peut accéder aux terrains, aux horaires et aux jours de fermeture du site via des boutons dédiés.

**Liens avec les règles/contraintes :**
- CF-RS-001, CF-RS-002, CF-RS-003
- CF-RV-001, CF-RV-002

---

### 3.2 EF-002 — Gérer les terrains d'un site

Ce cas d'utilisation permet au gestionnaire d'ajouter et de supprimer des terrains pour un site donné.

**Déroulement :**

Depuis la page **Sites**, le gestionnaire clique sur le bouton **Terrains** de la ligne d'un site. Une section de gestion des terrains apparaît, affichant la liste des terrains existants pour ce site.

Pour **ajouter** un terrain, le gestionnaire saisit le nom du terrain dans le champ prévu et soumet le formulaire. Le terrain est créé et associé au site sélectionné.

Pour **supprimer** un terrain, le gestionnaire clique sur « Supprimer » dans la ligne du terrain correspondant.

**Liens avec les règles/contraintes :**
- CF-RS-004, CF-RS-005
- CF-RV-003

---

### 3.3 EF-003 — Gérer les horaires d'un site

Ce cas d'utilisation permet au gestionnaire de définir les horaires d'ouverture d'un site, par année civile.

**Déroulement :**

Depuis la page **Sites**, le gestionnaire clique sur le bouton **Horaires** de la ligne d'un site. Une section de gestion des horaires apparaît, affichant les horaires existants pour ce site (année, heure de début, heure de fin).

Pour **ajouter** un horaire, le gestionnaire renseigne l'année, l'heure de début et l'heure de fin, puis soumet le formulaire. Le système vérifie qu'aucun horaire n'existe déjà pour ce site et cette année, et que l'heure de début est strictement inférieure à l'heure de fin, avant la création.

Pour **supprimer** un horaire, le gestionnaire clique sur « Supprimer » dans la ligne correspondante.

**Liens avec les règles/contraintes :**
- CF-RS-006, CF-RS-007, CF-RS-008
- CF-RV-004, CF-RV-005, CF-RV-006

---

### 3.4 EF-004 — Gérer les jours de fermeture

Ce cas d'utilisation permet au gestionnaire de définir des jours de fermeture, soit pour un site spécifique, soit de manière globale (tous les sites).

**Déroulement :**

Depuis la page **Sites**, le gestionnaire clique sur le bouton **Fermetures** de la ligne d'un site, ou accède à la section fermetures globales. Une liste des jours de fermeture s'affiche avec la date, la raison et le périmètre (global ou site).

Pour **ajouter** un jour de fermeture, le gestionnaire saisit la date, une raison optionnelle, et choisit si la fermeture est globale ou propre au site. Après soumission, le jour est enregistré.

Pour **supprimer** un jour de fermeture, le gestionnaire clique sur « Supprimer » dans la ligne correspondante.

Lors du calcul des créneaux disponibles (EF-006) ou lors de la création d'un match, le système consulte les jours de fermeture (globaux + propres au site du terrain) et refuse toute réservation sur un jour fermé.

**Liens avec les règles/contraintes :**
- CF-RS-009, CF-RS-010, CF-RS-011
- CF-RV-007, CF-RV-008

---

### 3.5 EF-005 — Gérer les membres

Ce cas d'utilisation permet au gestionnaire de créer, modifier et supprimer des membres.

**Déroulement :**

Le gestionnaire accède à la page **Membres** via le menu de navigation. La liste de tous les membres s'affiche dans un tableau avec les colonnes : Matricule, Nom, Email, Type, Site, Statut (actif / bloqué).

Un filtre par site permet d'afficher uniquement les membres rattachés à un site particulier.

Pour **créer** un membre, le gestionnaire clique sur « Nouveau membre ». Un formulaire apparaît avec les champs : Prénom, Nom, Email, Type (Global / Site / Libre) et, si le type est Site, un menu déroulant pour sélectionner le site rattaché. Le matricule est généré automatiquement par le système selon le type du membre. Après soumission, le membre est créé et la liste se rafraîchit.

Pour **modifier** un membre, le gestionnaire clique sur « Modifier » dans la ligne correspondante. Seuls les champs Prénom, Nom et Email sont modifiables.

Pour **supprimer** un membre, le gestionnaire clique sur « Supprimer » dans la ligne correspondante.

Si un membre est bloqué, un badge rouge indique la date de fin de blocage dans la colonne Statut.

**Liens avec les règles/contraintes :**
- CF-RS-012, CF-RS-013, CF-RS-014, CF-RS-015, CF-RS-016
- CF-RV-009, CF-RV-010, CF-RV-011, CF-RV-012
- CF-RC-001

---

### 3.6 EF-006 — Réserver un terrain (créer un match)

Ce cas d'utilisation permet à un membre de créer une réservation sur un terrain (un match privé ou public).

**Déroulement :**

Le membre accède à la page **Réservation** depuis le menu. Un assistant en plusieurs étapes guide la création :

1. **Sélection du site** : le membre choisit un site parmi la liste disponible.
2. **Sélection du terrain** : les terrains du site sélectionné sont affichés.
3. **Sélection de la date** : le membre choisit une date. Si la date correspond à un jour de fermeture (global ou propre au site), aucun créneau n'est disponible.
4. **Sélection du créneau horaire** : le système calcule les créneaux disponibles de 1h30 (avec pause de 15 min entre créneaux) en tenant compte des horaires d'ouverture du site pour l'année courante et des matchs déjà planifiés sur ce terrain.
5. **Choix du type** : le membre choisit entre match Privé ou Public.
6. **Saisie du matricule** : le membre saisit son matricule pour s'identifier.
7. **Confirmation** : après soumission, le match est créé, l'organisateur est automatiquement inscrit comme premier joueur, et un paiement de 15 € en statut `Pending` lui est assigné.

Si le match est **privé**, le membre est ensuite invité à ajouter d'autres joueurs par leur matricule (voir EF-008).

Si le match est **public**, il apparaît dans la liste des matchs publics (voir EF-007).

**Liens avec les règles/contraintes :**
- CF-RV-013, CF-RV-014, CF-RV-015, CF-RV-016, CF-RV-017, CF-RV-018, CF-RV-019
- CF-RC-002, CF-RC-003, CF-RC-004

---

### 3.7 EF-007 — Rejoindre un match public

Ce cas d'utilisation permet à un membre de rejoindre un match public existant qui n'est pas encore complet.

**Déroulement :**

Le membre accède à la page **Matchs publics**. La liste des matchs publics en statut `Scheduled` s'affiche, avec filtre optionnel par site. Pour chaque match, sont indiqués : le site, le terrain, la date/heure, le nombre de joueurs inscrits sur 4.

Pour rejoindre un match, le membre clique sur « Rejoindre » et saisit son matricule. Le système valide que le membre peut rejoindre ce match (non bloqué, même site si membre Site, dans la fenêtre de réservation autorisée), inscrit le joueur, et crée un paiement de 15 € en statut `Pending`.

Quand le 4ème joueur s'inscrit, le statut du match passe automatiquement à `Full`.

**Liens avec les règles/contraintes :**
- CF-RV-009 (blocage), CF-RV-015 (accès site), CF-RV-016 (fenêtre), CF-RV-020, CF-RV-021
- CF-RC-003 (paiement automatique), CF-RC-005 (passage Full)

---

### 3.8 EF-008 — Ajouter un joueur à un match privé

Ce cas d'utilisation permet à l'organisateur d'un match privé d'y ajouter d'autres joueurs.

**Déroulement :**

Depuis la page **Réservation** (après création d'un match privé) ou depuis la page **Mes matchs**, l'organisateur saisit le matricule d'un autre membre et clique sur « Ajouter ». Le système vérifie que l'organisateur est bien l'auteur du match, que le match est privé, que le membre n'est pas déjà inscrit, et que le match n'est pas complet. Si tout est valide, le joueur est inscrit et un paiement de 15 € est créé pour lui.

**Liens avec les règles/contraintes :**
- CF-RV-022, CF-RV-020, CF-RV-021
- CF-RC-003 (paiement automatique), CF-RC-005 (passage Full)

---

### 3.9 EF-009 — Payer sa part d'un match

Ce cas d'utilisation permet à un membre de payer sa part (15 €) pour un match auquel il est inscrit.

**Déroulement :**

Le membre accède à la page **Mes paiements** en saisissant son matricule. La liste de ses paiements s'affiche, avec un indicateur du solde total dû. Pour chaque paiement en statut `Pending`, un bouton « Payer » est disponible.

En cliquant sur « Payer », le système effectue le paiement : le statut passe à `Paid` et la date de paiement est enregistrée. Si le membre est organisateur d'un match public incomplet (moins de 4 joueurs à la date du match), le solde des places non remplies est intégré automatiquement au montant à payer lors de cette opération.

**Liens avec les règles/contraintes :**
- CF-RV-023, CF-RV-024
- CF-RC-006 (solde report match public incomplet)

---

### 3.10 EF-010 — Consulter ses matchs et paiements

Ce cas d'utilisation permet à un membre de consulter ses matchs et ses paiements.

**Déroulement :**

**Page Mes matchs :** le membre saisit son matricule. La liste de ses matchs s'affiche (matchs qu'il a organisés et matchs auxquels il participe comme joueur), avec pour chaque match : le site, le terrain, la date/heure, le type (Privé/Public), le statut (Scheduled/Full/Completed/Cancelled), et son rôle (Organisateur/Joueur).

**Page Mes paiements :** le membre saisit son matricule. Son solde total dû s'affiche, ainsi que la liste de tous ses paiements avec leur statut (Pending/Paid/Refunded).

**Liens avec les règles/contraintes :**
- CF-RS-020, CF-RS-021

---

### 3.11 EF-011 — Traitement automatique J-1

Ce cas d'utilisation décrit le traitement automatique qui s'exécute chaque heure pour gérer les matchs dont la date est le lendemain.

**Déroulement :**

Un service d'arrière-plan (`DayBeforeMatchJob`) s'exécute toutes les heures. Pour chaque match planifié dont la date est le lendemain :

**Règle 1 — Match privé incomplet :** Si un match est privé et possède moins de 4 joueurs, il devient public (`MatchType = Public`). L'organisateur reçoit un blocage de réservation d'une durée de 7 jours (`ReservationBlocked = true`, `BlockedUntil = aujourd'hui + 7 jours`).

**Règle 2 — Joueurs impayés :** Pour chaque joueur inscrit n'ayant pas encore payé (`Payment.Status = Pending`), le joueur est retiré du match (`MatchPlayer` supprimé). Si le match avait le statut `Full` et repasse sous 4 joueurs, il revient au statut `Scheduled` et son type redevient `Public`.

Ce traitement peut également être déclenché manuellement par l'administrateur via le bouton **Déclencher J-1** sur la page de gestion des matchs.

**Liens avec les règles/contraintes :**
- CF-RV-025 (conversion privé→public + pénalité)
- CF-RV-026 (retrait joueur impayé)
- CF-RC-007 (recalcul statut match)

---

### 3.12 EF-012 — Consulter les statistiques (admin)

Ce cas d'utilisation permet à l'administrateur de consulter des statistiques globales et par site.

**Déroulement :**

L'administrateur accède à la page **Statistiques** depuis le menu. Un tableau de bord global affiche : le nombre total de sites, de membres, de matchs, et le chiffre d'affaires global (somme des paiements `Paid`).

En dessous, des cartes individuelles par site affichent, pour chacun : le nombre de terrains, de membres, de matchs par statut (Scheduled, Full, Completed, Cancelled), et le chiffre d'affaires du site.

**Liens avec les règles/contraintes :**
- CF-RC-008 (calcul CA global), CF-RC-009 (calcul CA par site)

---

## 4. Contraintes fonctionnelles

### 4.1 Règles d'accès et autorisation

| Code | Description |
|------|-------------|
| CF-AA-001 | L'application accède à la base de données via l'utilisateur SQL `padel_app_user`, configuré dans `appsettings.json`. Cet utilisateur dispose des droits `SELECT`, `INSERT`, `UPDATE`, `DELETE` sur toutes les tables applicatives. |
| CF-AA-002 | Un second utilisateur SQL `padel_readonly` dispose uniquement de droits `SELECT` sur toutes les tables. Il est destiné aux opérations de reporting et de statistiques. |

### 4.2 Règles de structure

#### Sites

| Code | Description |
|------|-------------|
| CF-RS-001 | Un site possède un identifiant unique (clé primaire `Id` auto-incrémentée) |
| CF-RS-002 | Un site possède un nom obligatoire de maximum 100 caractères |
| CF-RS-003 | Un site possède une adresse obligatoire de maximum 250 caractères |

#### Terrains

| Code | Description |
|------|-------------|
| CF-RS-004 | Un terrain possède un identifiant unique (clé primaire `Id` auto-incrémentée) |
| CF-RS-005 | Un terrain possède un nom obligatoire de maximum 100 caractères et est rattaché à un site (`SiteId FK NOT NULL`) |

#### Horaires

| Code | Description |
|------|-------------|
| CF-RS-006 | Un horaire possède un identifiant unique (clé primaire `Id` auto-incrémentée) |
| CF-RS-007 | Un horaire est défini par un site, une année, une heure de début et une heure de fin, tous obligatoires |
| CF-RS-008 | La combinaison `(SiteId, Year)` est unique : un seul horaire par site et par année civile — `DB > SiteSchedules > UQ_SiteSchedules_SiteId_Year` |

#### Jours de fermeture

| Code | Description |
|------|-------------|
| CF-RS-009 | Un jour de fermeture possède un identifiant unique (clé primaire `Id` auto-incrémentée) |
| CF-RS-010 | Un jour de fermeture est défini par une date (`DateOnly`, obligatoire) et une raison optionnelle |
| CF-RS-011 | Le champ `SiteId` est nullable : `null` signifie une fermeture globale (tous les sites) ; une valeur non nulle désigne une fermeture propre à un site spécifique |

#### Membres

| Code | Description |
|------|-------------|
| CF-RS-012 | Un membre possède un identifiant unique (clé primaire `Id` auto-incrémentée) |
| CF-RS-013 | Un membre possède un matricule unique de maximum 10 caractères — `DB > Members > UQ_Members_Matricule` |
| CF-RS-014 | Un membre possède un prénom, un nom et un email, tous obligatoires |
| CF-RS-015 | Un membre possède un type parmi : `Global`, `Site`, ou `Libre` |
| CF-RS-016 | Un membre de type `Site` doit obligatoirement être rattaché à un site (`SiteId` non nul) |

#### Matchs

| Code | Description |
|------|-------------|
| CF-RS-017 | Un match possède un identifiant unique (clé primaire `Id` auto-incrémentée) |
| CF-RS-018 | Un match est associé à un terrain (`CourtId FK NOT NULL`), un organisateur (`OrganizerId FK NOT NULL`), une heure de début (`ScheduledAt`), une heure de fin (`EndsAt`), un type (`Private` ou `Public`) et un statut (`Scheduled`, `Full`, `Completed`, ou `Cancelled`) |
| CF-RS-019 | La table `MatchPlayer` constitue la jointure entre `Match` et `Member`. La combinaison `(MatchId, MemberId)` y est unique — `DB > MatchPlayers > UQ_MatchPlayers_MatchId_MemberId` |

#### Paiements

| Code | Description |
|------|-------------|
| CF-RS-020 | Un paiement possède un identifiant unique (clé primaire `Id` auto-incrémentée) |
| CF-RS-021 | Un paiement est lié à un `MatchPlayer`, au match et au membre concernés. Il possède un montant, un statut (`Pending`, `Paid`, ou `Refunded`), une date de création, et une date de paiement (nullable) |

### 4.3 Règles de validation

#### Sites

| Code | Description |
|------|-------------|
| CF-RV-001 | La suppression d'un site est refusée si des terrains y sont associés — `DB > CourtConfiguration > OnDelete(Restrict)` |
| CF-RV-002 | La mise à jour et la suppression d'un site retournent `404 Not Found` si le site n'existe pas |

#### Terrains

| Code | Description |
|------|-------------|
| CF-RV-003 | La création d'un terrain est refusée si le `SiteId` fourni ne correspond à aucun site existant — `VS > CourtService > CreateAsync : InvalidOperationException` |

#### Horaires

| Code | Description |
|------|-------------|
| CF-RV-004 | La création d'un horaire est refusée si le `SiteId` fourni ne correspond à aucun site existant — `VS > ScheduleService > CreateAsync : InvalidOperationException` |
| CF-RV-005 | La création d'un horaire est refusée si l'heure de début est supérieure ou égale à l'heure de fin — `VS > ScheduleService > CreateAsync : InvalidOperationException` |
| CF-RV-006 | La création d'un horaire est refusée si un horaire existe déjà pour ce site et cette année — `VS > ScheduleService > CreateAsync : InvalidOperationException` / `DB > UQ_SiteSchedules_SiteId_Year` |

#### Jours de fermeture

| Code | Description |
|------|-------------|
| CF-RV-007 | La création d'un jour de fermeture de type site est refusée si le `SiteId` fourni ne correspond à aucun site existant — `VS > ClosureDayService > CreateAsync : InvalidOperationException` |
| CF-RV-008 | La suppression d'un jour de fermeture retourne `404 Not Found` si l'identifiant n'existe pas |

#### Membres

| Code | Description |
|------|-------------|
| CF-RV-009 | Un membre bloqué (`ReservationBlocked = true` et `BlockedUntil > maintenant`) ne peut ni créer de match ni rejoindre un match — `VS > MatchService > CreateAsync / AddPlayerAsync : InvalidOperationException` |
| CF-RV-010 | Un membre de type `Site` sans `SiteId` est refusé à la création — `VS > MemberService > CreateAsync : InvalidOperationException` |
| CF-RV-011 | Un membre de type `Global` ou `Libre` avec un `SiteId` est refusé à la création — `VS > MemberService > CreateAsync : InvalidOperationException` |
| CF-RV-012 | La création d'un membre de type `Site` est refusée si le `SiteId` fourni ne correspond à aucun site existant — `VS > MemberService > CreateAsync : InvalidOperationException` |

#### Réservations (matchs)

| Code | Description |
|------|-------------|
| CF-RV-013 | La création d'un match est refusée si l'organisateur a un solde impayé (`balance > 0`) — `VS > MatchService > CreateAsync : InvalidOperationException` |
| CF-RV-014 | La création d'un match est refusée si l'organisateur est bloqué (cf. CF-RV-009) |
| CF-RV-015 | Un membre de type `Site` ne peut créer ou rejoindre un match que sur un terrain appartenant à son propre site — `VS > MatchService : InvalidOperationException` |
| CF-RV-016 | La fenêtre de réservation autorisée est vérifiée selon le type du membre : Global = 21 jours max avant le match, Site = 14 jours max, Libre = 5 jours max — `VS > MatchService : InvalidOperationException` |
| CF-RV-017 | La création d'un match est refusée si la date est un jour de fermeture (global ou propre au site) — `VS > MatchService > CreateAsync : InvalidOperationException` |
| CF-RV-018 | La création d'un match est refusée si l'heure de début ou de fin dépasse les horaires d'ouverture du site pour l'année concernée — `VS > MatchService > CreateAsync : InvalidOperationException` |
| CF-RV-019 | La création d'un match est refusée si un autre match est déjà planifié sur le même terrain au même créneau (chevauchement) — `VS > MatchService > CreateAsync : InvalidOperationException` |
| CF-RV-020 | Un joueur ne peut pas s'inscrire deux fois au même match — `DB > MatchPlayers > UQ_MatchPlayers_MatchId_MemberId` / `VS > MatchService : InvalidOperationException` |
| CF-RV-021 | L'ajout d'un joueur est refusé si le match est complet (4 joueurs inscrits, statut `Full`) — `VS > MatchService : InvalidOperationException` |
| CF-RV-022 | Seul l'organisateur peut ajouter des joueurs dans un match privé — `VS > MatchService > AddPlayerAsync : InvalidOperationException` |
| CF-RV-023 | Un paiement déjà effectué (`Status = Paid`) ne peut pas être payé une seconde fois — `VS > PaymentService > ProcessPaymentAsync : InvalidOperationException` |
| CF-RV-024 | Le paiement d'un `MatchPlayer` est refusé si le `MatchPlayerId` fourni n'existe pas — `VS > PaymentService : InvalidOperationException` |

#### Règles J-1

| Code | Description |
|------|-------------|
| CF-RV-025 | La veille d'un match privé avec moins de 4 joueurs, le match est converti en public et l'organisateur reçoit un blocage de 7 jours — `VS > DayBeforeMatchJob > ProcessPrivateIncompleteMatchesAsync` |
| CF-RV-026 | La veille d'un match, tout joueur avec un paiement en statut `Pending` est retiré du match — `VS > DayBeforeMatchJob > ProcessUnpaidPlayersAsync` |

### 4.4 Règles de calcul

| Code | Description |
|------|-------------|
| CF-RC-001 | Le matricule est généré automatiquement selon le type : `G` + 4 chiffres (Global), `S` + 5 chiffres (Site), `L` + 5 chiffres (Libre). Le numéro est calculé en prenant le dernier matricule du même type et en l'incrémentant de 1 — `VS > MemberService > GenerateMatriculeAsync` |
| CF-RC-002 | Les créneaux disponibles sur un terrain pour une date donnée sont calculés comme suit : en partant de l'heure d'ouverture du site, on découpe la plage en slots de 1h30, séparés par 15 min de pause. Un créneau est disponible s'il ne chevauche aucun match existant sur ce terrain et si le jour n'est pas un jour de fermeture — `VS > CourtService > GetAvailableSlotsAsync` |
| CF-RC-003 | Lors de l'ajout de tout joueur à un match (création, ajout privé, rejoindre public), un paiement de 15 € en statut `Pending` est automatiquement créé — `VS > MatchService` |
| CF-RC-004 | L'heure de fin d'un match est calculée automatiquement comme `ScheduledAt + 1h30` — `VS > MatchService > CreateAsync` |
| CF-RC-005 | Lorsque le 4ème joueur est ajouté à un match, le statut passe automatiquement de `Scheduled` à `Full` — `VS > MatchService` |
| CF-RC-006 | Si un organisateur paie alors qu'il est redevable de places non remplies pour un match public passé (< 4 joueurs à la date du match), le solde dû pour ces places est ajouté au montant du paiement courant — `VS > PaymentService > ProcessPaymentAsync` |
| CF-RC-007 | Après retrait d'un joueur impayé lors du traitement J-1, si le nombre de joueurs restants est inférieur à 4 et que le statut était `Full`, le statut repasse à `Scheduled` et le type repasse à `Public` — `VS > DayBeforeMatchJob` |
| CF-RC-008 | Le chiffre d'affaires global est la somme de tous les paiements dont le statut est `Paid` — `VS > StatsService > GetGlobalStatsAsync` |
| CF-RC-009 | Le chiffre d'affaires par site est la somme des paiements `Paid` dont le match est joué sur un terrain du site concerné — `VS > StatsService > GetSiteStatsAsync` |

---

## 5. Description des entités

| Nom | Description |
|-----|-------------|
| `Site` | Représente un site de padel. Point d'ancrage des terrains, des horaires, des jours de fermeture et des membres. Attributs : `Id`, `Name`, `Address`. |
| `Court` | Représente un terrain de padel. Rattaché obligatoirement à un site. Attributs : `Id`, `Name`, `SiteId`. |
| `SiteSchedule` | Représente les horaires d'ouverture d'un site pour une année civile donnée. Un seul horaire autorisé par couple `(SiteId, Year)`. Attributs : `Id`, `SiteId`, `Year`, `StartTime`, `EndTime`. |
| `ClosureDay` | Représente un jour de fermeture. Si `SiteId` est null, la fermeture est globale (tous les sites). Attributs : `Id`, `Date`, `Reason`, `SiteId`. |
| `Member` | Représente un membre inscrit. Identifié par son matricule généré automatiquement. Peut être bloqué temporairement. Attributs : `Id`, `Matricule`, `FirstName`, `LastName`, `Email`, `MemberType`, `SiteId`, `ReservationBlocked`, `BlockedUntil`. |
| `Match` | Représente une réservation de terrain (une séance de jeu). Lié à un terrain, à un organisateur, avec un type (Privé/Public) et un statut (Scheduled/Full/Completed/Cancelled). Attributs : `Id`, `CourtId`, `OrganizerId`, `ScheduledAt`, `EndsAt`, `MatchType`, `Status`. |
| `MatchPlayer` | Table de jointure entre `Match` et `Member`. Enregistre l'inscription d'un joueur à un match. Attributs : `Id`, `MatchId`, `MemberId`, `JoinedAt`. |
| `Payment` | Représente la part de 15 € due par un joueur pour un match. Créé automatiquement lors de l'inscription. Attributs : `Id`, `MatchPlayerId`, `MatchId`, `MemberId`, `Amount`, `Status`, `CreatedAt`, `PaidAt`. |

---

## 6. Schéma relationnel de la solution

### 6.1 Schéma EA

```
┌──────────────────────────────────────────────────────────────────┐
│                             Site                                  │
│  Id (PK) | Name | Address                                         │
└──────┬──────────────┬───────────────┬────────────────┬───────────┘
       │ 1            │ 1             │ 1              │ 1
       │ *            │ *             │ *              │ *
┌──────▼──────┐  ┌────▼────────┐  ┌──▼───────────┐  ┌▼───────────┐
│   Court     │  │SiteSchedule │  │  ClosureDay  │  │   Member   │
│─────────────│  │─────────────│  │──────────────│  │────────────│
│ Id (PK)     │  │ Id (PK)     │  │ Id (PK)      │  │ Id (PK)    │
│ Name        │  │ SiteId (FK) │  │ Date         │  │ Matricule  │
│ SiteId (FK) │  │ Year        │  │ Reason       │  │ FirstName  │
└──────┬──────┘  │ StartTime   │  │ SiteId(FK,?) │  │ LastName   │
       │ 1       │ EndTime     │  └──────────────┘  │ Email      │
       │ *       └─────────────┘                    │ MemberType │
┌──────▼──────────────────────────────────┐         │ SiteId(FK?)│
│                  Match                  │         │ ResBlocked │
│─────────────────────────────────────────│         │ BlockedUnt │
│ Id (PK)                                 │         └────────────┘
│ CourtId (FK)                            │
│ OrganizerId (FK → Member)               │
│ ScheduledAt | EndsAt                    │
│ MatchType (Private/Public)              │
│ Status (Scheduled/Full/Completed/Cancel)│
└────────────────────┬────────────────────┘
                     │ 1
                     │ *
              ┌──────▼──────────┐
              │   MatchPlayer   │
              │─────────────────│
              │ Id (PK)         │
              │ MatchId (FK)    │
              │ MemberId (FK)   │
              │ JoinedAt        │
              └──────┬──────────┘
                     │ 1
                     │ 1
              ┌──────▼──────────┐
              │    Payment      │
              │─────────────────│
              │ Id (PK)         │
              │ MatchPlayerId   │
              │ MatchId (FK)    │
              │ MemberId (FK)   │
              │ Amount          │
              │ Status          │
              │ CreatedAt       │
              │ PaidAt          │
              └─────────────────┘
```

### 6.2 Schéma relationnel

```
Sites (
  Id          INT IDENTITY PK,
  Name        NVARCHAR(100)  NOT NULL,
  Address     NVARCHAR(250)  NOT NULL
)

Courts (
  Id          INT IDENTITY PK,
  Name        NVARCHAR(100)  NOT NULL,
  SiteId      INT NOT NULL   FK → Sites(Id) ON DELETE RESTRICT
)

SiteSchedules (
  Id          INT IDENTITY PK,
  SiteId      INT NOT NULL   FK → Sites(Id) ON DELETE CASCADE,
  Year        INT NOT NULL,
  StartTime   TIME NOT NULL,
  EndTime     TIME NOT NULL,
  UNIQUE (SiteId, Year)
)

ClosureDays (
  Id          INT IDENTITY PK,
  Date        DATE NOT NULL,
  Reason      NVARCHAR(250)  NULL,
  SiteId      INT NULL       FK → Sites(Id) ON DELETE CASCADE
)

Members (
  Id                   INT IDENTITY PK,
  Matricule            NVARCHAR(10)   NOT NULL UNIQUE,
  FirstName            NVARCHAR(100)  NOT NULL,
  LastName             NVARCHAR(100)  NOT NULL,
  Email                NVARCHAR(200)  NOT NULL,
  MemberType           NVARCHAR(10)   NOT NULL,
  SiteId               INT NULL       FK → Sites(Id) ON DELETE SET NULL,
  ReservationBlocked   BIT NOT NULL DEFAULT 0,
  BlockedUntil         DATETIME2 NULL
)

Matches (
  Id              INT IDENTITY PK,
  CourtId         INT NOT NULL     FK → Courts(Id) ON DELETE RESTRICT,
  OrganizerId     INT NOT NULL     FK → Members(Id) ON DELETE RESTRICT,
  ScheduledAt     DATETIME2 NOT NULL,
  EndsAt          DATETIME2 NOT NULL,
  MatchType       NVARCHAR(10) NOT NULL,
  Status          NVARCHAR(15) NOT NULL
)

MatchPlayers (
  Id          INT IDENTITY PK,
  MatchId     INT NOT NULL   FK → Matches(Id) ON DELETE CASCADE,
  MemberId    INT NOT NULL   FK → Members(Id) ON DELETE RESTRICT,
  JoinedAt    DATETIME2 NOT NULL,
  UNIQUE (MatchId, MemberId)
)

Payments (
  Id              INT IDENTITY PK,
  MatchPlayerId   INT NOT NULL   FK → MatchPlayers(Id) ON DELETE CASCADE,
  MatchId         INT NOT NULL   FK → Matches(Id)  ON DELETE RESTRICT,
  MemberId        INT NOT NULL   FK → Members(Id)  ON DELETE RESTRICT,
  Amount          DECIMAL(10,2)  NOT NULL,
  Status          NVARCHAR(10)   NOT NULL,
  CreatedAt       DATETIME2 NOT NULL,
  PaidAt          DATETIME2 NULL
)
```

### 6.3 Implémentation des contraintes

| Règle | Implémentation |
|-------|---------------|
| CF-RS-008 — Unicité `(SiteId, Year)` | `DB > SiteSchedules > UQ_SiteSchedules_SiteId_Year` (contrainte UNIQUE SQL) + `DB > SiteScheduleConfiguration > HasIndex(...).IsUnique()` (EF Core) + `VS > ScheduleService > ExistsAsync` (validation applicative) |
| CF-RS-013 — Matricule unique | `DB > Members > UQ_Members_Matricule` (contrainte UNIQUE SQL) + `DB > MemberConfiguration > HasIndex(m => m.Matricule).IsUnique()` (EF Core) |
| CF-RS-016 — Membre Site avec SiteId | `VS > MemberService > CreateAsync` : rejet si type `Site` sans `SiteId`, ou type autre que `Site` avec `SiteId` |
| CF-RS-019 — Unicité `(MatchId, MemberId)` | `DB > MatchPlayers > UQ_MatchPlayers_MatchId_MemberId` (UNIQUE SQL) + `DB > MatchPlayerConfiguration > HasIndex(...).IsUnique()` (EF Core) |
| CF-RV-001 — Suppression site restreinte | `DB > CourtConfiguration > OnDelete(DeleteBehavior.Restrict)` (EF Core) |
| CF-RV-003 — Vérification site à la création d'un terrain | `VS > CourtService > CreateAsync` : appel `siteRepository.GetByIdAsync` puis exception si null |
| CF-RV-004 à CF-RV-006 — Validations horaire | `VS > ScheduleService > CreateAsync` : trois blocs de validation séquentiels |
| CF-RC-001 — Génération du matricule | `VS > MemberService > GenerateMatriculeAsync` : récupère le dernier matricule du type via `GetLastMatriculeByTypeAsync`, incrémente et formate selon le préfixe et le nombre de chiffres |
| CF-RV-013 — Blocage si solde impayé | `VS > MatchService > CreateAsync` : appel `paymentRepository.GetUnpaidBalanceAsync(organizerId)` puis rejet si > 0 |
| CF-RV-015 — Accès site membre Site | `VS > MatchService > CreateAsync / AddPlayerAsync / JoinAsync` : vérification `member.SiteId == court.SiteId` |
| CF-RV-016 — Fenêtre de réservation | `VS > MatchService` : calcul `(scheduledAt - now).Days` comparé au maximum autorisé selon `MemberType` |
| CF-RV-017 — Jour de fermeture | `VS > MatchService > CreateAsync` : appel `closureDayRepository.ExistsForDateAndSiteAsync` |
| CF-RV-019 — Chevauchement de créneaux | `VS > MatchService > CreateAsync` : appel `matchRepository.HasConflictAsync(courtId, scheduledAt, endsAt)` |
| CF-RV-025 — Règle J-1 privé incomplet | `VS > DayBeforeMatchJob > ProcessPrivateIncompleteMatchesAsync` : changement `MatchType = Public` + mise à jour `ReservationBlocked / BlockedUntil` sur l'organisateur |
| CF-RV-026 — Règle J-1 joueurs impayés | `VS > DayBeforeMatchJob > ProcessUnpaidPlayersAsync` : suppression `MatchPlayer` + recalcul statut match |
| CF-RC-006 — Report solde match public | `VS > PaymentService > ProcessPaymentAsync` : calcul du solde des places non remplies et ajout au montant |

---

## 7. Analyse technique

### 7.1 Technologies proposées

| Composant | Technologie |
|-----------|------------|
| Back-end | .NET 10 / C# 14, REST API (ASP.NET Core) |
| Front-end | Blazor WebAssembly |
| Base de données | SQL Server |
| ORM | Entity Framework Core (Code First + script SQL) |
| Communication front/back | HTTP (HttpClient) avec CORS configuré |
| Tests | xUnit + Moq (mocking) + EF Core InMemory |
| Background job | `IHostedService` (.NET) pour le traitement J-1 |

### 7.2 Architecture applicative

L'application respecte une architecture en couches stricte, découpée en 4 projets .NET distincts pour le back-end, plus un projet front-end séparé et un projet de tests.

#### La couche Domain (`Padel.Domain`)

Contient les entités métier pures et les enums, sans dépendance vers aucune autre couche :

- Entités : `Site`, `Court`, `SiteSchedule`, `ClosureDay`, `Member`, `Match`, `MatchPlayer`, `Payment`
- Enums : `MemberType` (`Global`, `Site`, `Libre`), `MatchType` (`Private`, `Public`), `MatchStatus` (`Scheduled`, `Full`, `Completed`, `Cancelled`), `PaymentStatus` (`Pending`, `Paid`, `Refunded`)

#### La couche Application (`Padel.Application`)

Contient les interfaces, les DTOs et les services métier. Elle ne dépend que de `Padel.Domain`.

**Interfaces de repository :**

| Interface | Méthodes exposées |
|-----------|------------------|
| `ISiteRepository` | `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` |
| `ICourtRepository` | `GetBySiteIdAsync`, `GetByIdAsync`, `CreateAsync`, `DeleteAsync`, `GetAvailableSlotsAsync` |
| `ISiteScheduleRepository` | `GetBySiteIdAsync`, `GetByIdAsync`, `ExistsAsync`, `GetForYearAsync`, `CreateAsync`, `DeleteAsync` |
| `IClosureDayRepository` | `GetBySiteIdAsync`, `GetByIdAsync`, `ExistsForDateAndSiteAsync`, `CreateAsync`, `DeleteAsync` |
| `IMemberRepository` | `GetAllAsync`, `GetBySiteIdAsync`, `GetByMatriculeAsync`, `GetLastMatriculeByTypeAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` |
| `IMatchRepository` | `GetAllAsync`, `GetByIdAsync`, `GetPublicAsync`, `GetByOrganizerAsync`, `GetByPlayerAsync`, `GetBySiteAsync`, `HasConflictAsync`, `GetMatchesBecomingPublicAsync`, `GetMatchesWithUnpaidPlayersAsync`, `CreateAsync`, `UpdateAsync` |
| `IPaymentRepository` | `GetByMemberAsync`, `GetByMatchAsync`, `GetUnpaidBalanceAsync`, `GetUnpaidPublicMatchDebtAsync`, `CreateAsync`, `UpdateAsync` |

**Interfaces de service :**

| Interface | Méthodes exposées |
|-----------|------------------|
| `ISiteService` | `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` |
| `ICourtService` | `GetBySiteIdAsync`, `GetByIdAsync`, `CreateAsync`, `DeleteAsync`, `GetAvailableSlotsAsync` |
| `IScheduleService` | `GetBySiteIdAsync`, `CreateAsync`, `DeleteAsync` |
| `IClosureDayService` | `GetBySiteIdAsync`, `CreateAsync`, `DeleteAsync` |
| `IMemberService` | `GetAllAsync`, `GetBySiteIdAsync`, `GetByMatriculeAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` |
| `IMatchService` | `GetAllAsync`, `GetByIdAsync`, `GetPublicAsync`, `GetByOrganizerAsync`, `GetByPlayerAsync`, `CreateAsync`, `AddPlayerAsync`, `JoinAsync`, `ProcessDayBeforeAsync` |
| `IPaymentService` | `GetByMemberAsync`, `GetByMatchAsync`, `GetBalanceAsync`, `ProcessPaymentAsync` |
| `IStatsService` | `GetGlobalStatsAsync`, `GetSiteStatsAsync` |

**DTOs :**

| DTO | Usage |
|-----|-------|
| `SiteDto` / `CreateSiteDto` / `UpdateSiteDto` | Transfert des données d'un site |
| `CourtDto` / `CreateCourtDto` | Transfert des données d'un terrain |
| `SiteScheduleDto` / `CreateSiteScheduleDto` | Transfert des données d'un horaire |
| `ClosureDayDto` / `CreateClosureDayDto` | Transfert des données d'un jour de fermeture |
| `MemberDto` / `CreateMemberDto` / `UpdateMemberDto` | Transfert des données d'un membre |
| `MatchDto` / `CreateMatchDto` / `AddPlayerDto` / `MatchPlayerDto` | Transfert des données d'un match et de ses joueurs |
| `PaymentDto` / `ProcessPaymentDto` | Transfert des données d'un paiement |
| `GlobalStatsDto` / `SiteStatsDto` | Transfert des statistiques globales et par site |

#### La couche Infrastructure (`Padel.Infrastructure`)

Contient les implémentations concrètes des repositories et la configuration EF Core. Elle ne dépend que de `Padel.Domain` et `Padel.Application`.

**Repositories :** `SiteRepository`, `CourtRepository`, `SiteScheduleRepository`, `ClosureDayRepository`, `MemberRepository`, `MatchRepository`, `PaymentRepository`

**Configurations EF Core :** `SiteConfiguration`, `CourtConfiguration`, `SiteScheduleConfiguration`, `ClosureDayConfiguration`, `MemberConfiguration`, `MatchConfiguration`, `MatchPlayerConfiguration`, `PaymentConfiguration`

**Contexte :** `PadelDbContext` — charge les configurations via `ApplyConfigurationsFromAssembly`

**Script SQL :** `InitializeDatabase.sql` — crée la base `PadelDb`, les utilisateurs `padel_app_user` et `padel_readonly`, toutes les tables avec leurs contraintes, et attribue les droits appropriés à chaque utilisateur.

**Background job :** `DayBeforeMatchJob` (implémente `IHostedService`) — s'exécute toutes les heures et appelle `IMatchService.ProcessDayBeforeAsync`.

#### La couche API (`Padel.Api`)

Contient les controllers REST et le point d'entrée de l'application. Elle dépend de `Padel.Application`.

**Endpoints exposés :**

| Controller | Méthode | Route | Description |
|------------|---------|-------|-------------|
| `SitesController` | `GET` | `/api/sites` | Lister tous les sites |
| `SitesController` | `GET` | `/api/sites/{id}` | Obtenir un site par son Id |
| `SitesController` | `POST` | `/api/sites` | Créer un site |
| `SitesController` | `PUT` | `/api/sites/{id}` | Modifier un site |
| `SitesController` | `DELETE` | `/api/sites/{id}` | Supprimer un site |
| `CourtsController` | `GET` | `/api/courts/site/{siteId}` | Lister les terrains d'un site |
| `CourtsController` | `GET` | `/api/courts/{id}` | Obtenir un terrain par son Id |
| `CourtsController` | `GET` | `/api/courts/{id}/slots/{date}` | Obtenir les créneaux disponibles pour une date |
| `CourtsController` | `POST` | `/api/courts` | Créer un terrain |
| `CourtsController` | `DELETE` | `/api/courts/{id}` | Supprimer un terrain |
| `SchedulesController` | `GET` | `/api/schedules/site/{siteId}` | Lister les horaires d'un site |
| `SchedulesController` | `POST` | `/api/schedules` | Créer un horaire |
| `SchedulesController` | `DELETE` | `/api/schedules/{id}` | Supprimer un horaire |
| `ClosureDaysController` | `GET` | `/api/closuredays?siteId=` | Lister les jours de fermeture (global + site) |
| `ClosureDaysController` | `POST` | `/api/closuredays` | Créer un jour de fermeture |
| `ClosureDaysController` | `DELETE` | `/api/closuredays/{id}` | Supprimer un jour de fermeture |
| `MembersController` | `GET` | `/api/members` | Lister tous les membres |
| `MembersController` | `GET` | `/api/members/site/{siteId}` | Lister les membres d'un site |
| `MembersController` | `GET` | `/api/members/{matricule}` | Obtenir un membre par son matricule |
| `MembersController` | `POST` | `/api/members` | Créer un membre |
| `MembersController` | `PUT` | `/api/members/{matricule}` | Modifier un membre |
| `MembersController` | `DELETE` | `/api/members/{matricule}` | Supprimer un membre |
| `MatchesController` | `GET` | `/api/matches` | Lister tous les matchs (admin) |
| `MatchesController` | `GET` | `/api/matches/{id}` | Détail d'un match |
| `MatchesController` | `GET` | `/api/matches/public?siteId=` | Matchs publics (filtre optionnel par site) |
| `MatchesController` | `GET` | `/api/matches/organizer/{matricule}` | Matchs organisés par un membre |
| `MatchesController` | `GET` | `/api/matches/player/{matricule}` | Matchs auxquels participe un membre |
| `MatchesController` | `GET` | `/api/matches/site/{siteId}` | Matchs d'un site |
| `MatchesController` | `POST` | `/api/matches` | Créer un match |
| `MatchesController` | `POST` | `/api/matches/{id}/players` | Ajouter un joueur (match privé) |
| `MatchesController` | `POST` | `/api/matches/{id}/join/{matricule}` | Rejoindre un match public |
| `MatchesController` | `POST` | `/api/matches/process-day-before` | Déclencher manuellement les règles J-1 |
| `PaymentsController` | `POST` | `/api/payments/pay` | Effectuer un paiement |
| `PaymentsController` | `GET` | `/api/payments/member/{matricule}` | Paiements d'un membre |
| `PaymentsController` | `GET` | `/api/payments/match/{matchId}` | Paiements d'un match |
| `PaymentsController` | `GET` | `/api/payments/balance/{matricule}` | Solde dû d'un membre |
| `StatsController` | `GET` | `/api/stats` | Statistiques globales |
| `StatsController` | `GET` | `/api/stats/site/{siteId}` | Statistiques d'un site |

**Injection de dépendances** (`Program.cs`) :

```
ISiteRepository           → SiteRepository
ISiteService              → SiteService
ICourtRepository          → CourtRepository
ICourtService             → CourtService
ISiteScheduleRepository   → SiteScheduleRepository
IScheduleService          → ScheduleService
IClosureDayRepository     → ClosureDayRepository
IClosureDayService        → ClosureDayService
IMemberRepository         → MemberRepository
IMemberService            → MemberService
IMatchRepository          → MatchRepository
IPaymentRepository        → PaymentRepository
IMatchService             → MatchService
IPaymentService           → PaymentService
IStatsService             → StatsService
IHostedService            → DayBeforeMatchJob
```

**CORS :** politique `AllowFrontend` autorisant les origines du projet frontend Blazor.

#### Le front-end (`Padel.Frontend`)

Application Blazor WebAssembly communiquant avec le back-end via `HttpClient`. Organisée en pages, services et modèles.

**Pages utilisateur :**

| Page | Route | Description |
|------|-------|-------------|
| `Home.razor` | `/` | Accueil : informations, tarifs (60 €/match, 15 €/joueur), durée (1h30), délais de réservation par type |
| `PublicMatches.razor` | `/matches/public` | Liste des matchs publics, filtre par site, bouton Rejoindre avec saisie matricule |
| `Reservation.razor` | `/reservations` | Wizard de réservation : site → terrain → date → créneau → type → matricule → confirmation + ajout joueurs si privé |
| `MyMatches.razor` | `/my-matches` | Saisie matricule → liste des matchs (organisateur et joueur), statut, type, rôle |
| `MyPayments.razor` | `/my-payments` | Saisie matricule → solde dû, liste des paiements, bouton Payer |

**Pages administrateur :**

| Page | Route | Description |
|------|-------|-------------|
| `AdminSites.razor` | `/admin/sites` | CRUD sites + terrains + horaires + jours de fermeture |
| `AdminMembers.razor` | `/admin/members` | Création, liste, filtre par site/type, suppression, badge blocage |
| `AdminMatches.razor` | `/admin/matches` | Liste tous les matchs, filtre par site, détails (modal), bouton Déclencher J-1 |
| `AdminStats.razor` | `/admin/stats` | Dashboard global + cartes par site (membres, matchs, CA) |

**Services front-end :** `SiteService`, `CourtService`, `ScheduleService`, `ClosureDayService`, `MemberService`, `MatchService`, `PaymentService`, `StatsService` — wrappers `HttpClient` appelant les endpoints de l'API.

**Modèles front-end :** miroir des DTOs du back-end.

#### Projet de tests (`Padel.Tests`)

Projet xUnit séparé, utilisant Moq pour le mocking et EF Core InMemory / SQLite pour les tests de repositories.

**Couverture prioritaire :**

| Scénario | Service |
|----------|---------|
| Créer un match — cas nominal | `MatchService` |
| Créer un match — membre bloqué | `MatchService` |
| Créer un match — solde impayé | `MatchService` |
| Créer un match — mauvais site (membre Site) | `MatchService` |
| Créer un match — hors fenêtre de réservation | `MatchService` |
| Créer un match — jour de fermeture | `MatchService` |
| Créer un match — créneau déjà pris | `MatchService` |
| Rejoindre un match public | `MatchService` |
| Ajouter un joueur à un match privé | `MatchService` |
| Règle J-1 — privé incomplet → public + pénalité | `DayBeforeMatchJob` |
| Règle J-1 — retrait joueur impayé | `DayBeforeMatchJob` |
| Payer — cas nominal | `PaymentService` |
| Payer — double paiement | `PaymentService` |
| Payer — avec report de solde (match public incomplet) | `PaymentService` |
| Calcul des créneaux disponibles | `CourtService` |
| Génération du matricule | `MemberService` |

#### Architecture globale du projet

```
Padel.Domain
    ↑
Padel.Application  (dépend de Domain)
    ↑
Padel.Infrastructure  (dépend de Domain + Application)
    ↑
Padel.Api  (dépend de Application + Infrastructure)

Padel.Frontend  (indépendant — communique via HTTP)

Padel.Tests  (dépend de Application + Infrastructure)
```
