# 🎾 Gestion de Terrains de Padel - Démo et Explication Technique

## 📋 Table des matières

1. [Vue d'ensemble du projet](#vue-densemble-du-projet)
2. [Architecture technique](#architecture-technique)
3. [Scénarios de démonstration](#scénarios-de-démonstration)
4. [Explication technique détaillée](#explication-technique-détaillée)
5. [Sécurité de la base de données](#sécurité-de-la-base-de-données)
6. [Tests](#tests)
7. [Procédure de lancement](#procédure-de-lancement)

---

## 🎯 Vue d'ensemble du projet

### Contexte métier

Le projet consiste en une application de **gestion de terrains de Padel** pour un gestionnaire possédant plusieurs sites. L'application permet :

- **Gestion multi-sites** : chaque site a ses propres terrains, horaires et jours de fermeture
- **Système de réservation** : matchs privés ou publics avec règles spécifiques
- **Gestion des membres** : 3 types (Global, Site, Libre) avec privilèges différents
- **Paiements** : 60€ par match divisé en 4 × 15€, paiement obligatoire à l'avance
- **Règles automatiques J-1** : transformations automatiques la veille du match
- **Statistiques** : chiffre d'affaires et données par site

### Technologies utilisées

| Couche | Technologie | Version |
|--------|-------------|---------|
| **Backend** | ASP.NET Core Web API | .NET 10 |
| **Frontend** | Blazor WebAssembly | .NET 10 |
| **Base de données** | SQL Server | Dernière |
| **ORM** | Entity Framework Core | 10.0 |
| **Tests** | xUnit + Moq | Derniers |
| **Conteneurisation** | (Option Docker) | - |

---

## 🏗️ Architecture technique

### Architecture globale

```
┌─────────────────────────────────────────────────────────────┐
│                    Frontend (Blazor WASM)                   │
│  - Pages utilisateur (réservations, matchs publics)         │
│  - Pages admin (gestion sites, membres, stats)              │
└───────────────────────────┬─────────────────────────────────┘
							│ HTTP/REST
							▼
┌─────────────────────────────────────────────────────────────┐
│                  Backend (ASP.NET Core API)                 │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         Controllers (Endpoints REST)                │   │
│  └───────────────────┬─────────────────────────────────┘   │
│                      │                                      │
│  ┌───────────────────▼─────────────────────────────────┐   │
│  │  Services (Logique métier)                          │   │
│  │  - MatchService (règles, validations)               │   │
│  │  - PaymentService (paiements, soldes)               │   │
│  │  - DayBeforeMatchJob (règles automatiques)          │   │
│  └───────────────────┬─────────────────────────────────┘   │
│                      │                                      │
│  ┌───────────────────▼─────────────────────────────────┐   │
│  │  Repositories (Accès données)                       │   │
│  │  - Generic Repository Pattern                       │   │
│  │  - Repositories spécialisés                         │   │
│  └───────────────────┬─────────────────────────────────┘   │
│                      │                                      │
│  ┌───────────────────▼─────────────────────────────────┐   │
│  │  DbContext (EF Core)                                │   │
│  └───────────────────┬─────────────────────────────────┘   │
└──────────────────────┼─────────────────────────────────────┘
					   │
					   ▼
┌─────────────────────────────────────────────────────────────┐
│              Base de données SQL Server                     │
│  - Utilisateur padel_app_user (CRUD limité)                 │
│  - Utilisateur padel_readonly (SELECT uniquement)           │
└─────────────────────────────────────────────────────────────┘
```

### Structure des projets

```
Padel.sln
├── src/
│   ├── Padel.Domain/           # Entités métier
│   │   ├── Entities/
│   │   │   ├── Site.cs
│   │   │   ├── Court.cs
│   │   │   ├── Member.cs
│   │   │   ├── Match.cs
│   │   │   ├── MatchPlayer.cs
│   │   │   ├── Payment.cs
│   │   │   ├── SiteSchedule.cs
│   │   │   └── ClosureDay.cs
│   │   └── Enums/
│   │       ├── MemberType.cs
│   │       ├── MatchType.cs
│   │       ├── MatchStatus.cs
│   │       └── PaymentStatus.cs
│   │
│   ├── Padel.Application/      # Logique métier
│   │   ├── DTOs/
│   │   ├── Services/
│   │   │   ├── ISiteService / SiteService
│   │   │   ├── IMatchService / MatchService
│   │   │   ├── IPaymentService / PaymentService
│   │   │   ├── IMemberService / MemberService
│   │   │   └── IStatsService / StatsService
│   │   └── BackgroundJobs/
│   │       └── DayBeforeMatchJob.cs
│   │
│   ├── Padel.Infrastructure/   # Accès données
│   │   ├── Data/
│   │   │   └── PadelDbContext.cs
│   │   ├── Repositories/
│   │   │   ├── IRepository<T> / Repository<T>
│   │   │   ├── ISiteRepository / SiteRepository
│   │   │   ├── IMatchRepository / MatchRepository
│   │   │   ├── IPaymentRepository / PaymentRepository
│   │   │   └── IMemberRepository / MemberRepository
│   │   └── Configurations/     # EF Core configurations
│   │
│   ├── Padel.Api/             # API REST
│   │   ├── Controllers/
│   │   │   ├── SitesController.cs
│   │   │   ├── CourtsController.cs
│   │   │   ├── MembersController.cs
│   │   │   ├── MatchesController.cs
│   │   │   ├── PaymentsController.cs
│   │   │   └── StatsController.cs
│   │   └── Program.cs
│   │
│   ├── Padel.Frontend/        # Blazor WASM
│   │   ├── Pages/
│   │   │   ├── Index.razor
│   │   │   ├── PublicMatches.razor
│   │   │   ├── Reservations.razor
│   │   │   ├── MyMatches.razor
│   │   │   ├── MyPayments.razor
│   │   │   ├── Admin/
│   │   │   │   ├── Sites.razor
│   │   │   │   ├── Members.razor
│   │   │   │   ├── Matches.razor
│   │   │   │   └── Stats.razor
│   │   ├── Services/
│   │   │   └── ApiService.cs
│   │   └── Program.cs
│   │
│   └── Padel.Tests/           # Tests unitaires
│       ├── Services/
│       ├── Controllers/
│       └── Repositories/
```

### Modèle de données relationnel

```
┌──────────────┐       ┌──────────────┐       ┌──────────────────┐
│    Site      │1────*│    Court     │1────*│     Match        │
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
	   │                                                 │
	   ├────* ClosureDay                        ┌───────┴─────────┐
	   │      │ Id, Date, Reason,                │  MatchPlayer    │*
	   │      │ SiteId (nullable)                │─────────────────│
	   │                                         │ Id              │
	   └────* Member                             │ MatchId (FK)    │
			  │ Id, Matricule,                   │ MemberId (FK)   │
			  │ FirstName, LastName,             │ JoinedAt        │
			  │ Email, MemberType,               └───────┬─────────┘
			  │ SiteId, ReservationBlocked,              │1
			  │ BlockedUntil                             │
			  │                                  ┌───────┴─────────┐
			  └──────────────────────────────────*│    Payment      │
												 │─────────────────│
												 │ Id              │
												 │ MatchPlayerId   │
												 │ MatchId (FK)    │
												 │ MemberId (FK)   │
												 │ Amount          │
												 │ Status          │
												 │ CreatedAt       │
												 │ PaidAt          │
												 └─────────────────┘
```

---

## 🎬 Scénarios de démonstration

### Scénario 1 : Configuration initiale (Admin Global)

**Objectif** : Montrer la gestion des sites et la configuration de base

1. **Créer un site**
   - Aller sur `/admin/sites`
   - Créer "Padel Brussels Center" (Adresse: Rue de la Loi 123)
   - Ajouter 3 terrains (Court 1, Court 2, Court 3)
   - Définir horaires 2025 : 08:00 - 22:00
   - Ajouter jours de fermeture : 25/12/2024, 01/01/2025

2. **Créer un second site**
   - Créer "Padel Antwerp Club"
   - 2 terrains (Court A, Court B)
   - Horaires 2025 : 09:00 - 21:00

3. **Créer des membres**
   - Membre Global : Jean Dupont (G0001) - peut réserver partout, 21 jours avant
   - Membre Site Brussels : Marie Martin (S00001) - site Brussels uniquement, 14 jours avant
   - Membre Libre : Paul Leroy (L00001) - peut réserver partout, 5 jours avant

**Points techniques à expliquer** :
- Architecture REST : appels `POST /api/sites`, `POST /api/courts`
- Validation côté backend : horaires cohérents, contraintes d'unicité
- Relations FK : Court → Site, Member → Site

---

### Scénario 2 : Réservation de match privé (Utilisateur)

**Objectif** : Démontrer le cycle complet d'une réservation privée

1. **Créer un match privé** (Jean Dupont - G0001)
   - Utiliser `/reservations`
   - Sélectionner site Brussels, Court 1
   - Date : dans 10 jours (dans sa fenêtre de 21 jours)
   - Heure : 14:00 (créneau disponible)
   - Type : Privé

2. **Ajouter des joueurs**
   - Ajouter Marie (S00001)
   - Ajouter Paul (L00001)
   - Ajouter Sophie (G0002)
   - Le match passe à statut `Full` (4 joueurs)

3. **Paiements**
   - Chaque joueur voit son paiement de 15€ en attente
   - Jean paie → statut `Paid`
   - Marie paie → statut `Paid`
   - Paul paie → statut `Paid`
   - Sophie paie → statut `Paid`

**Points techniques à expliquer** :
- Validation de la fenêtre de réservation selon `MemberType`
- Calcul des créneaux disponibles (1h30 + 15 min pause)
- Création automatique des paiements lors de l'ajout de joueurs
- Statut du match : `Scheduled` → `Full`

---

### Scénario 3 : Match public et jointure (Utilisateur)

**Objectif** : Montrer le mécanisme des matchs publics

1. **Créer un match public** (Pierre - G0003)
   - Site Brussels, Court 2
   - Date : dans 8 jours
   - Heure : 16:00
   - Type : Public
   - Pierre est automatiquement ajouté comme premier joueur

2. **Consulter les matchs publics** (Thomas - L00002)
   - Aller sur `/matches/public`
   - Filtrer par site Brussels
   - Voir le match de Pierre avec 1/4 joueurs

3. **Rejoindre le match**
   - Thomas clique sur "Rejoindre"
   - Entre son matricule L00002
   - Est ajouté instantanément (2/4 joueurs)
   - Un paiement de 15€ est créé pour lui

4. **Deux autres joueurs rejoignent**
   - Lucas (S00002) rejoint → 3/4
   - Emma (G0004) rejoint → 4/4
   - Le match passe à `Full`

**Points techniques à expliquer** :
- Endpoint `POST /api/matches/{id}/join/{matricule}`
- Interdiction pour l'organisateur d'ajouter des joueurs dans un match public
- Validation : maximum 4 joueurs, pas de doublons

---

### Scénario 4 : Règles automatiques J-1 (Background Job)

**Objectif** : Démontrer les règles métier critiques appliquées la veille

#### 4.1 Match privé incomplet → devient public + pénalité

1. **Créer un match privé** (Alice - G0005)
   - Site Antwerp, Court A
   - Date : demain
   - Type : Privé
   - Ajoute seulement 2 joueurs (Alice incluse) → 2/4

2. **Déclencher les règles J-1** (Admin)
   - Clic sur "Exécuter règles J-1" dans `/admin/matches`
   - Ou attendre l'exécution automatique du background job

3. **Résultat**
   - Le match passe de `Private` à `Public`
   - Alice reçoit un blocage de réservation de 7 jours
   - `Alice.ReservationBlocked = true`
   - `Alice.BlockedUntil = DateTime.Now + 7 jours`
   - Le match peut maintenant être rejoint par d'autres

4. **Vérification**
   - Alice essaie de créer un nouveau match → **Refusé**
   - Message : "Vous êtes temporairement bloqué jusqu'au [date]"

#### 4.2 Joueur impayé → retrait du match

1. **Match existant** (4 joueurs dont Bob - L00003)
   - Bob n'a pas payé ses 15€
   - Les 3 autres ont payé

2. **Règles J-1**
   - Bob est retiré du match
   - Sa place redevient libre (3/4 joueurs)
   - Le match repasse à `Scheduled` et `Public`
   - Un autre joueur peut rejoindre

**Points techniques à expliquer** :
- Background service hébergé (`DayBeforeMatchJob`) s'exécutant toutes les heures
- Requêtes spécialisées dans `MatchRepository` :
  - `GetMatchesBecomingPublicAsync()` : matchs privés J-1 avec < 4 joueurs
  - `GetMatchesWithUnpaidPlayersAsync()` : matchs J-1 avec joueurs `Pending`
- Mise à jour en masse via transactions
- Endpoint manuel pour démo : `POST /api/matches/process-day-before`

---

### Scénario 5 : Solde impayé et report (Edge case)

**Objectif** : Montrer la gestion des soldes et blocages

1. **Match public incomplet** (Claire - S00003)
   - Claire crée un match public
   - Seulement 2 joueurs au total ont payé (Claire + 1 autre)
   - Date du match atteinte

2. **Fin de match**
   - Le système calcule : 4 places × 15€ = 60€
   - Payé : 2 × 15€ = 30€
   - Solde dû par Claire (organisateur) : 30€

3. **Claire essaie de réserver un nouveau match**
   - **Refusé** : "Vous avez un solde impayé de 30€"

4. **Claire rejoint un autre match public**
   - Montant à payer : 15€ de participation + 30€ de solde = **45€**
   - Une fois payé, son solde est à 0 et elle peut réserver à nouveau

**Points techniques à expliquer** :
- Calcul du solde : `PaymentRepository.GetUnpaidBalanceAsync(matricule)`
- Validation dans `MatchService.CreateMatchAsync()` : refus si solde > 0
- Report automatique lors du paiement : montant augmenté du solde

---

### Scénario 6 : Statistiques et chiffre d'affaires (Admin)

**Objectif** : Démontrer les capacités de reporting

1. **Dashboard global** (`/admin/stats`)
   - Total de sites : 2
   - Total de membres : 15
   - Total de matchs : 42
   - Chiffre d'affaires total : 2.520€

2. **Statistiques par site**
   - **Brussels Center**
	 - Terrains : 3
	 - Membres rattachés : 8
	 - Matchs : 28
	 - CA : 1.680€

   - **Antwerp Club**
	 - Terrains : 2
	 - Membres : 4
	 - Matchs : 14
	 - CA : 840€

3. **Détails matchs**
   - Scheduled : 5
   - Full : 10
   - Completed : 25
   - Cancelled : 2

**Points techniques à expliquer** :
- Agrégation via LINQ dans `StatsService`
- Calcul CA : somme des paiements avec `Status = Paid`
- DTO `GlobalStatsDto` et `SiteStatsDto`
- Endpoint `GET /api/stats` et `GET /api/stats/site/{siteId}`

---

### Scénario 7 : Validations métier (Tests en direct)

**Objectif** : Montrer la robustesse des validations

#### Tests de refus (à démontrer via Swagger ou tests unitaires)

1. **Membre Site essaie de réserver dans un autre site**
   - Marie (S00001, site Brussels) essaie de réserver à Antwerp
   - **Refusé** : "Vous ne pouvez réserver que dans votre site"

2. **Membre Libre essaie de réserver trop tôt**
   - Paul (L00001) essaie de réserver dans 10 jours
   - **Refusé** : "Vous ne pouvez réserver que 5 jours avant maximum"

3. **Réservation sur un jour de fermeture**
   - Essai de réserver le 25/12/2024
   - **Refusé** : "Le site est fermé ce jour-là"

4. **Créneau déjà pris**
   - Match existe à 14:00 sur Court 1
   - Essai de réserver 14:30 sur Court 1 (chevauche 14:00-15:45)
   - **Refusé** : "Ce créneau n'est pas disponible"

5. **Heure hors horaires du site**
   - Site ouvert 08:00-22:00
   - Essai de réserver à 23:00
   - **Refusé** : "Hors des horaires d'ouverture du site"

**Points techniques à expliquer** :
- Logique de validation centralisée dans `MatchService`
- Tests unitaires correspondants dans `Padel.Tests`
- Gestion d'erreurs côté API : codes HTTP 400 avec messages explicites

---

## 🔧 Explication technique détaillée

### 1. Architecture en couches (Clean Architecture)

#### Domain Layer (`Padel.Domain`)
- **Responsabilité** : Entités métier pures, sans dépendances
- **Contenu** : 
  - Entités (Site, Court, Match, Member, Payment, etc.)
  - Enums (MemberType, MatchType, MatchStatus, PaymentStatus)
  - Pas de logique métier complexe ici

```csharp
// Exemple : Member.cs
public class Member
{
	public int Id { get; set; }
	public string Matricule { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public MemberType MemberType { get; set; }
	public int? SiteId { get; set; }
	public bool ReservationBlocked { get; set; }
	public DateTime? BlockedUntil { get; set; }

	// Navigation properties
	public Site? Site { get; set; }
	public ICollection<Match> OrganizedMatches { get; set; } = [];
	public ICollection<MatchPlayer> MatchPlayers { get; set; } = [];
	public ICollection<Payment> Payments { get; set; } = [];
}
```

#### Application Layer (`Padel.Application`)
- **Responsabilité** : Logique métier, orchestration
- **Contenu** :
  - Services (interfaces + implémentations)
  - DTOs (Data Transfer Objects)
  - Background jobs

```csharp
// Exemple : MatchService.cs (simplifié)
public class MatchService : IMatchService
{
	private readonly IMatchRepository _matchRepo;
	private readonly IMemberRepository _memberRepo;
	private readonly IPaymentRepository _paymentRepo;

	public async Task<MatchDto> CreateMatchAsync(CreateMatchDto dto)
	{
		// 1. Validation de l'organisateur
		var organizer = await _memberRepo.GetByMatriculeAsync(dto.OrganizerMatricule);
		if (organizer.ReservationBlocked && organizer.BlockedUntil > DateTime.Now)
			throw new BusinessException("Vous êtes temporairement bloqué");

		// 2. Vérification du solde
		var balance = await _paymentRepo.GetUnpaidBalanceAsync(organizer.Matricule);
		if (balance > 0)
			throw new BusinessException($"Vous avez un solde impayé de {balance}€");

		// 3. Validation de la fenêtre de réservation
		var maxDays = organizer.MemberType switch
		{
			MemberType.Global => 21,
			MemberType.Site => 14,
			MemberType.Libre => 5,
			_ => throw new InvalidOperationException()
		};
		var daysUntilMatch = (dto.ScheduledAt - DateTime.Now).Days;
		if (daysUntilMatch > maxDays)
			throw new BusinessException("Réservation trop anticipée");

		// 4. Validation site (membre Site)
		if (organizer.MemberType == MemberType.Site && court.SiteId != organizer.SiteId)
			throw new BusinessException("Vous ne pouvez réserver que dans votre site");

		// 5. Vérification jour de fermeture
		// 6. Vérification disponibilité créneau
		// 7. Création du match
		// 8. Auto-inscription de l'organisateur
		// 9. Création du paiement

		return matchDto;
	}
}
```

#### Infrastructure Layer (`Padel.Infrastructure`)
- **Responsabilité** : Accès aux données, persistence
- **Contenu** :
  - `DbContext` EF Core
  - Repositories (générique + spécialisés)
  - Configurations EF Core

```csharp
// Repository générique
public class Repository<T> : IRepository<T> where T : class
{
	protected readonly PadelDbContext _context;

	public async Task<T?> GetByIdAsync(int id)
		=> await _context.Set<T>().FindAsync(id);

	public async Task<IEnumerable<T>> GetAllAsync()
		=> await _context.Set<T>().ToListAsync();

	public async Task<T> AddAsync(T entity)
	{
		await _context.Set<T>().AddAsync(entity);
		await _context.SaveChangesAsync();
		return entity;
	}
}

// Repository spécialisé
public class MatchRepository : Repository<Match>, IMatchRepository
{
	public async Task<IEnumerable<Match>> GetMatchesBecomingPublicAsync()
	{
		var tomorrow = DateTime.Today.AddDays(1);
		return await _context.Matches
			.Include(m => m.MatchPlayers)
			.Where(m => m.MatchType == MatchType.Private
					 && m.ScheduledAt.Date == tomorrow
					 && m.MatchPlayers.Count < 4)
			.ToListAsync();
	}
}
```

#### API Layer (`Padel.Api`)
- **Responsabilité** : Endpoints REST, authentification, validation entrées
- **Contenu** : Controllers, middleware, configuration

```csharp
[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
	private readonly IMatchService _matchService;

	[HttpPost]
	public async Task<ActionResult<MatchDto>> CreateMatch([FromBody] CreateMatchDto dto)
	{
		try
		{
			var match = await _matchService.CreateMatchAsync(dto);
			return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, match);
		}
		catch (BusinessException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}

	[HttpPost("{id}/join/{matricule}")]
	public async Task<ActionResult> JoinPublicMatch(int id, string matricule)
	{
		try
		{
			await _matchService.JoinPublicMatchAsync(id, matricule);
			return Ok();
		}
		catch (NotFoundException)
		{
			return NotFound();
		}
		catch (BusinessException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}
}
```

#### Frontend Layer (`Padel.Frontend`)
- **Responsabilité** : Interface utilisateur
- **Technologie** : Blazor WebAssembly (rendu côté client)

```razor
@* Reservations.razor (simplifié) *@
@page "/reservations"
@inject ApiService Api

<h3>Nouvelle Réservation</h3>

<EditForm Model="@model" OnValidSubmit="@HandleSubmit">
	<div class="form-group">
		<label>Site</label>
		<InputSelect @bind-Value="model.SiteId" class="form-control">
			@foreach (var site in sites)
			{
				<option value="@site.Id">@site.Name</option>
			}
		</InputSelect>
	</div>

	<div class="form-group">
		<label>Terrain</label>
		<InputSelect @bind-Value="model.CourtId" class="form-control">
			@foreach (var court in courts)
			{
				<option value="@court.Id">@court.Name</option>
			}
		</InputSelect>
	</div>

	<div class="form-group">
		<label>Date</label>
		<InputDate @bind-Value="model.Date" class="form-control" />
	</div>

	<div class="form-group">
		<label>Heure</label>
		<InputSelect @bind-Value="model.TimeSlot" class="form-control">
			@foreach (var slot in availableSlots)
			{
				<option value="@slot">@slot</option>
			}
		</InputSelect>
	</div>

	<button type="submit" class="btn btn-primary">Réserver</button>
</EditForm>

@code {
	private CreateMatchDto model = new();
	private List<SiteDto> sites = new();
	private List<CourtDto> courts = new();
	private List<string> availableSlots = new();

	protected override async Task OnInitializedAsync()
	{
		sites = await Api.GetSitesAsync();
	}

	private async Task HandleSubmit()
	{
		try
		{
			await Api.CreateMatchAsync(model);
			// Redirection ou message de succès
		}
		catch (Exception ex)
		{
			// Affichage erreur
		}
	}
}
```

---

### 2. Injection de dépendances (DI)

Configuration dans `Program.cs` (API) :

```csharp
var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<PadelDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ISiteRepository, SiteRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();

// Services
builder.Services.AddScoped<ISiteService, SiteService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IStatsService, StatsService>();

// Background job
builder.Services.AddHostedService<DayBeforeMatchJob>();

// CORS
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.WithOrigins("https://localhost:7001") // Frontend URL
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
```

**Avantages** :
- Testabilité : facilité de mocker les dépendances
- Couplage faible : changement d'implémentation sans toucher aux consommateurs
- Durée de vie contrôlée : `Scoped` pour les requêtes, `Singleton` pour le cache

---

### 3. Background Job : règles J-1

```csharp
public class DayBeforeMatchJob : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<DayBeforeMatchJob> _logger;

	public DayBeforeMatchJob(IServiceProvider serviceProvider, ILogger<DayBeforeMatchJob> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("DayBeforeMatchJob démarré");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await ProcessDayBeforeRulesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erreur lors de l'exécution des règles J-1");
			}

			// Exécution toutes les heures
			await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
		}
	}

	private async Task ProcessDayBeforeRulesAsync()
	{
		using var scope = _serviceProvider.CreateScope();
		var matchRepo = scope.ServiceProvider.GetRequiredService<IMatchRepository>();
		var memberRepo = scope.ServiceProvider.GetRequiredService<IMemberRepository>();

		// Règle 1 : Matchs privés incomplets → publics + pénalité
		var incompletePrivateMatches = await matchRepo.GetMatchesBecomingPublicAsync();
		foreach (var match in incompletePrivateMatches)
		{
			match.MatchType = MatchType.Public;

			var organizer = await memberRepo.GetByIdAsync(match.OrganizerId);
			organizer.ReservationBlocked = true;
			organizer.BlockedUntil = DateTime.Now.AddDays(7);

			_logger.LogInformation($"Match {match.Id} converti en public, organisateur {organizer.Matricule} bloqué 7 jours");
		}

		// Règle 2 : Joueurs impayés → retrait
		var matchesWithUnpaid = await matchRepo.GetMatchesWithUnpaidPlayersAsync();
		foreach (var match in matchesWithUnpaid)
		{
			var unpaidPlayers = match.MatchPlayers
				.Where(mp => mp.Payment?.Status == PaymentStatus.Pending)
				.ToList();

			foreach (var mp in unpaidPlayers)
			{
				match.MatchPlayers.Remove(mp);
				_logger.LogInformation($"Joueur {mp.MemberId} retiré du match {match.Id} (impayé)");
			}

			// Si < 4 joueurs après retrait, redevient ouvert
			if (match.MatchPlayers.Count < 4)
			{
				match.Status = MatchStatus.Scheduled;
				match.MatchType = MatchType.Public;
			}
		}

		await matchRepo.SaveChangesAsync();
	}
}
```

**Points clés** :
- `BackgroundService` hébergé par ASP.NET Core
- Exécution périodique (toutes les heures)
- Utilisation de scopes DI pour accéder aux services
- Logging des opérations
- Gestion d'erreurs (ne doit pas crasher l'API)

---

### 4. Calcul des créneaux disponibles

```csharp
public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsAsync(int courtId, DateOnly date)
{
	var court = await _courtRepo.GetByIdAsync(courtId);
	var site = await _siteRepo.GetByIdAsync(court.SiteId);

	// 1. Vérifier jour de fermeture
	var closures = await _closureRepo.GetByDateAsync(date);
	if (closures.Any(c => c.SiteId == site.Id || c.SiteId == null))
		return Enumerable.Empty<TimeSlot>();

	// 2. Récupérer horaires du site pour l'année
	var schedule = await _scheduleRepo.GetBySiteAndYearAsync(site.Id, date.Year);
	if (schedule == null)
		return Enumerable.Empty<TimeSlot>();

	// 3. Récupérer les matchs existants ce jour-là sur ce terrain
	var existingMatches = await _matchRepo.GetByCourtAndDateAsync(courtId, date);

	// 4. Générer tous les créneaux possibles
	var slots = new List<TimeSlot>();
	var currentTime = schedule.StartTime;
	var matchDuration = TimeSpan.FromMinutes(90);
	var pauseDuration = TimeSpan.FromMinutes(15);

	while (currentTime.Add(matchDuration) <= schedule.EndTime)
	{
		var slotStart = currentTime;
		var slotEnd = currentTime.Add(matchDuration);

		// Vérifier si le créneau chevauche un match existant
		var isAvailable = !existingMatches.Any(m =>
			(m.ScheduledAt.TimeOfDay < slotEnd.ToTimeSpan() && m.EndsAt.TimeOfDay > slotStart.ToTimeSpan())
		);

		if (isAvailable)
		{
			slots.Add(new TimeSlot
			{
				StartTime = slotStart,
				EndTime = slotEnd,
				IsAvailable = true
			});
		}

		currentTime = currentTime.Add(matchDuration + pauseDuration);
	}

	return slots;
}
```

**Logique** :
1. Vérifier fermetures (globales + site)
2. Récupérer horaires annuels du site
3. Récupérer matchs existants sur le terrain
4. Générer créneaux de 1h30 avec pause de 15 min
5. Filtrer créneaux qui chevauchent des matchs

---

## 🔐 Sécurité de la base de données

### Principe : principe du moindre privilège

L'application N'UTILISE PAS l'utilisateur `sa` ou un compte avec tous les droits.

### Utilisateurs SQL créés

```sql
-- Script de création des utilisateurs

-- 1. Utilisateur applicatif (CRUD sur les tables nécessaires)
CREATE LOGIN padel_app_user WITH PASSWORD = 'S3cur3P@ssw0rd!';
CREATE USER padel_app_user FOR LOGIN padel_app_user;

-- Droits granulaires table par table
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Sites TO padel_app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Courts TO padel_app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Members TO padel_app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Matches TO padel_app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.MatchPlayers TO padel_app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Payments TO padel_app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.SiteSchedules TO padel_app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.ClosureDays TO padel_app_user;

-- 2. Utilisateur lecture seule (pour les stats/reporting)
CREATE LOGIN padel_readonly WITH PASSWORD = 'R34d0nlyP@ss!';
CREATE USER padel_readonly FOR LOGIN padel_readonly;

GRANT SELECT ON dbo.Sites TO padel_readonly;
GRANT SELECT ON dbo.Courts TO padel_readonly;
GRANT SELECT ON dbo.Members TO padel_readonly;
GRANT SELECT ON dbo.Matches TO padel_readonly;
GRANT SELECT ON dbo.MatchPlayers TO padel_readonly;
GRANT SELECT ON dbo.Payments TO padel_readonly;
GRANT SELECT ON dbo.SiteSchedules TO padel_readonly;
GRANT SELECT ON dbo.ClosureDays TO padel_readonly;
```

### Connection string utilisée

```json
// appsettings.json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Database=PadelDB;User Id=padel_app_user;Password=S3cur3P@ssw0rd!;TrustServerCertificate=True;"
  }
}
```

### Choix de conception : Accès direct aux tables vs Procédures stockées

| Critère | Accès direct aux tables (notre choix) | Procédures stockées uniquement |
|---------|---------------------------------------|--------------------------------|
| **Avantages** | ✅ Compatible avec EF Core ORM<br>✅ Pas de duplication de logique<br>✅ Développement plus rapide<br>✅ Migrations EF Core automatiques | ✅ Contrôle total des opérations SQL<br>✅ Performances optimisables manuellement<br>✅ Sécurité maximale (pas de requêtes arbitraires) |
| **Inconvénients** | ⚠️ L'utilisateur peut techniquement exécuter des requêtes non prévues (atténué par les droits GRANT spécifiques)<br>⚠️ Moins de contrôle fin sur les performances | ⚠️ Duplication de logique (C# + SQL)<br>⚠️ Maintenance complexe<br>⚠️ Moins compatible avec l'ORM<br>⚠️ Tests plus difficiles |
| **Sécurité** | Bonne (droits limités par table) | Excellente (contrôle total) |
| **Maintenabilité** | Excellente (code C# uniquement) | Moyenne (C# + SQL) |

**Justification du choix** : Nous avons opté pour l'accès direct aux tables car :
1. EF Core assure une protection contre les injections SQL
2. Les droits GRANT limitent les opérations possibles
3. La logique métier en C# est plus facile à tester et maintenir
4. Les migrations EF Core automatiques simplifient l'évolution du schéma
5. Le niveau de sécurité est suffisant pour ce cas d'usage (pas de données ultra-sensibles)

**Mesures de sécurité supplémentaires** :
- Pas de `DROP`, `CREATE`, `ALTER` pour `padel_app_user`
- Validation stricte côté services avant insertion/modification
- Logging des opérations sensibles
- Utilisation de paramètres pour toutes les requêtes (EF Core)

---

## ✅ Tests

### Structure des tests

```
Padel.Tests/
├── Services/
│   ├── MatchServiceTests.cs
│   ├── PaymentServiceTests.cs
│   ├── MemberServiceTests.cs
│   └── StatsServiceTests.cs
├── Controllers/
│   ├── MatchesControllerTests.cs
│   ├── PaymentsControllerTests.cs
│   └── MembersControllerTests.cs
└── Repositories/
	├── MatchRepositoryTests.cs
	└── PaymentRepositoryTests.cs
```

### Exemples de tests unitaires

#### Test de service (avec mocking)

```csharp
public class MatchServiceTests
{
	private readonly Mock<IMatchRepository> _mockMatchRepo;
	private readonly Mock<IMemberRepository> _mockMemberRepo;
	private readonly Mock<IPaymentRepository> _mockPaymentRepo;
	private readonly MatchService _service;

	public MatchServiceTests()
	{
		_mockMatchRepo = new Mock<IMatchRepository>();
		_mockMemberRepo = new Mock<IMemberRepository>();
		_mockPaymentRepo = new Mock<IPaymentRepository>();

		_service = new MatchService(
			_mockMatchRepo.Object,
			_mockMemberRepo.Object,
			_mockPaymentRepo.Object
		);
	}

	[Fact]
	public async Task CreateMatch_WithBlockedOrganizer_ShouldThrowException()
	{
		// Arrange
		var dto = new CreateMatchDto
		{
			OrganizerMatricule = "G0001",
			CourtId = 1,
			ScheduledAt = DateTime.Now.AddDays(5)
		};

		_mockMemberRepo.Setup(r => r.GetByMatriculeAsync("G0001"))
			.ReturnsAsync(new Member
			{
				Matricule = "G0001",
				ReservationBlocked = true,
				BlockedUntil = DateTime.Now.AddDays(3)
			});

		// Act & Assert
		var exception = await Assert.ThrowsAsync<BusinessException>(
			() => _service.CreateMatchAsync(dto)
		);

		Assert.Contains("temporairement bloqué", exception.Message);
	}

	[Fact]
	public async Task CreateMatch_WithUnpaidBalance_ShouldThrowException()
	{
		// Arrange
		var dto = new CreateMatchDto
		{
			OrganizerMatricule = "G0001",
			CourtId = 1,
			ScheduledAt = DateTime.Now.AddDays(5)
		};

		_mockMemberRepo.Setup(r => r.GetByMatriculeAsync("G0001"))
			.ReturnsAsync(new Member
			{
				Matricule = "G0001",
				ReservationBlocked = false,
				MemberType = MemberType.Global
			});

		_mockPaymentRepo.Setup(r => r.GetUnpaidBalanceAsync("G0001"))
			.ReturnsAsync(30.0m);

		// Act & Assert
		var exception = await Assert.ThrowsAsync<BusinessException>(
			() => _service.CreateMatchAsync(dto)
		);

		Assert.Contains("solde impayé", exception.Message);
	}

	[Fact]
	public async Task CreateMatch_ValidRequest_ShouldCreateMatchAndPayment()
	{
		// Arrange
		var dto = new CreateMatchDto
		{
			OrganizerMatricule = "G0001",
			CourtId = 1,
			ScheduledAt = DateTime.Now.AddDays(10),
			MatchType = MatchType.Private
		};

		var member = new Member
		{
			Id = 1,
			Matricule = "G0001",
			ReservationBlocked = false,
			MemberType = MemberType.Global
		};

		_mockMemberRepo.Setup(r => r.GetByMatriculeAsync("G0001"))
			.ReturnsAsync(member);

		_mockPaymentRepo.Setup(r => r.GetUnpaidBalanceAsync("G0001"))
			.ReturnsAsync(0);

		_mockMatchRepo.Setup(r => r.AddAsync(It.IsAny<Match>()))
			.ReturnsAsync((Match m) => m);

		// Act
		var result = await _service.CreateMatchAsync(dto);

		// Assert
		Assert.NotNull(result);
		_mockMatchRepo.Verify(r => r.AddAsync(It.IsAny<Match>()), Times.Once);
	}
}
```

#### Test de controller

```csharp
public class MatchesControllerTests
{
	private readonly Mock<IMatchService> _mockService;
	private readonly MatchesController _controller;

	public MatchesControllerTests()
	{
		_mockService = new Mock<IMatchService>();
		_controller = new MatchesController(_mockService.Object);
	}

	[Fact]
	public async Task CreateMatch_ValidRequest_ShouldReturn201()
	{
		// Arrange
		var dto = new CreateMatchDto { /* ... */ };
		var matchDto = new MatchDto { Id = 1 };

		_mockService.Setup(s => s.CreateMatchAsync(dto))
			.ReturnsAsync(matchDto);

		// Act
		var result = await _controller.CreateMatch(dto);

		// Assert
		var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
		Assert.Equal(201, createdResult.StatusCode);
		Assert.Equal(matchDto, createdResult.Value);
	}

	[Fact]
	public async Task CreateMatch_BusinessException_ShouldReturn400()
	{
		// Arrange
		var dto = new CreateMatchDto { /* ... */ };

		_mockService.Setup(s => s.CreateMatchAsync(dto))
			.ThrowsAsync(new BusinessException("Erreur métier"));

		// Act
		var result = await _controller.CreateMatch(dto);

		// Assert
		var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
		Assert.Equal(400, badRequestResult.StatusCode);
	}
}
```

#### Test de repository (avec base en mémoire)

```csharp
public class MatchRepositoryTests : IDisposable
{
	private readonly PadelDbContext _context;
	private readonly MatchRepository _repository;

	public MatchRepositoryTests()
	{
		var options = new DbContextOptionsBuilder<PadelDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		_context = new PadelDbContext(options);
		_repository = new MatchRepository(_context);
	}

	[Fact]
	public async Task GetMatchesBecomingPublicAsync_ShouldReturnPrivateMatchesWithLessThan4Players()
	{
		// Arrange
		var tomorrow = DateTime.Today.AddDays(1);

		var match1 = new Match
		{
			Id = 1,
			MatchType = MatchType.Private,
			ScheduledAt = tomorrow.AddHours(14),
			MatchPlayers = new List<MatchPlayer>
			{
				new MatchPlayer { MemberId = 1 },
				new MatchPlayer { MemberId = 2 }
			}
		};

		var match2 = new Match
		{
			Id = 2,
			MatchType = MatchType.Private,
			ScheduledAt = tomorrow.AddHours(16),
			MatchPlayers = new List<MatchPlayer>
			{
				new MatchPlayer { MemberId = 3 },
				new MatchPlayer { MemberId = 4 },
				new MatchPlayer { MemberId = 5 },
				new MatchPlayer { MemberId = 6 }
			}
		};

		await _context.Matches.AddRangeAsync(match1, match2);
		await _context.SaveChangesAsync();

		// Act
		var result = await _repository.GetMatchesBecomingPublicAsync();

		// Assert
		Assert.Single(result);
		Assert.Equal(1, result.First().Id);
	}

	public void Dispose()
	{
		_context.Dispose();
	}
}
```

### Couverture de tests

**Objectifs** :
- Services : > 80% de couverture (focus sur logique métier)
- Controllers : > 70% (focus sur codes HTTP)
- Repositories : > 60% (requêtes complexes)

**Priorités** :
1. MatchService (réservations, validations, règles J-1)
2. PaymentService (paiements, soldes, reports)
3. MatchesController (endpoints critiques)
4. PaymentsController (paiements)

**Commandes** :
```bash
# Exécuter tous les tests
dotnet test

# Avec couverture
dotnet test /p:CollectCoverage=true
```

---

## 🚀 Procédure de lancement

### Prérequis

- **.NET 10 SDK** installé
- **SQL Server** (LocalDB, Express ou complet)
- **Visual Studio 2026** ou VS Code avec extensions C#

### Étapes de lancement

#### 1. Configuration de la base de données

```bash
# Créer la base de données
sqlcmd -S localhost -Q "CREATE DATABASE PadelDB"

# Créer les utilisateurs SQL (exécuter le script de sécurité)
sqlcmd -S localhost -i Scripts/CreateUsers.sql
```

#### 2. Migration de la base de données

```bash
# Depuis le dossier src/Padel.Infrastructure
dotnet ef database update --startup-project ../Padel.Api

# Ou via Package Manager Console dans Visual Studio
Update-Database -Project Padel.Infrastructure -StartupProject Padel.Api
```

#### 3. Lancement du backend (API)

```bash
# Depuis src/Padel.Api
dotnet run

# Ou avec watch (rechargement automatique)
dotnet watch run
```

L'API démarre sur `https://localhost:7000` (par défaut)

#### 4. Lancement du frontend (Blazor WASM)

```bash
# Depuis src/Padel.Frontend
dotnet run

# Ou avec watch
dotnet watch run
```

Le frontend démarre sur `https://localhost:7001` (par défaut)

#### 5. Vérification

- **API** : ouvrir `https://localhost:7000/swagger` → Swagger UI
- **Frontend** : ouvrir `https://localhost:7001` → Interface Blazor

### Seed de données de démo

Créer un fichier `Scripts/SeedData.sql` :

```sql
-- Sites
INSERT INTO Sites (Name, Address) VALUES
('Padel Brussels Center', 'Rue de la Loi 123, Bruxelles'),
('Padel Antwerp Club', 'Avenue du Roi 45, Anvers');

-- Courts
INSERT INTO Courts (Name, SiteId) VALUES
('Court 1', 1), ('Court 2', 1), ('Court 3', 1),
('Court A', 2), ('Court B', 2);

-- Horaires 2025
INSERT INTO SiteSchedules (SiteId, Year, StartTime, EndTime) VALUES
(1, 2025, '08:00', '22:00'),
(2, 2025, '09:00', '21:00');

-- Fermetures
INSERT INTO ClosureDays (Date, Reason, SiteId) VALUES
('2024-12-25', 'Noël', NULL),  -- Global
('2025-01-01', 'Nouvel An', NULL);  -- Global

-- Membres
INSERT INTO Members (Matricule, FirstName, LastName, Email, MemberType, SiteId, ReservationBlocked, BlockedUntil) VALUES
('G0001', 'Jean', 'Dupont', 'jean@email.com', 0, NULL, 0, NULL),  -- Global
('S00001', 'Marie', 'Martin', 'marie@email.com', 1, 1, 0, NULL),  -- Site Brussels
('L00001', 'Paul', 'Leroy', 'paul@email.com', 2, NULL, 0, NULL);  -- Libre
```

Exécuter :
```bash
sqlcmd -S localhost -d PadelDB -U padel_app_user -P S3cur3P@ssw0rd! -i Scripts/SeedData.sql
```

---

## 📊 Points clés pour la démo

### Ce qu'il faut absolument montrer

1. **Architecture séparée** : Backend (API Swagger) + Frontend (Blazor)
2. **CRUD de base** : Création d'un site, terrain, membre
3. **Réservation complète** : De la sélection de créneau au paiement
4. **Match public** : Création + jointure par un autre membre
5. **Règles J-1** : Transformation d'un match privé incomplet + blocage
6. **Validations** : Refus d'une réservation (membre bloqué, solde dû, mauvais site, etc.)
7. **Statistiques** : Dashboard avec CA par site
8. **Sécurité DB** : Montrer la connection string avec `padel_app_user`, expliquer les droits

### Ce qu'il faut expliquer techniquement

1. **Clean Architecture** : Séparation Domain / Application / Infrastructure / API
2. **Injection de dépendances** : Pattern Repository, Services, Controllers
3. **Background Job** : `DayBeforeMatchJob` hébergé, exécution périodique
4. **EF Core** : Migrations, configurations, relations FK, Include
5. **DTOs** : Séparation entités / objets de transfert
6. **Validations métier** : Dans les services, pas dans les controllers
7. **Tests** : Exemples xUnit avec Moq et InMemoryDatabase
8. **Sécurité** : Utilisateurs SQL dédiés, pas de `sa`

### Points bonus (si temps)

- **CORS** : Configuration pour autoriser le frontend
- **Logging** : Utilisation de `ILogger` dans les services
- **Exceptions métier** : `BusinessException`, `NotFoundException` custom
- **Calcul de créneaux** : Algorithme avec horaires + matchs existants + fermetures
- **Génération matricule** : Automatique selon le type (G/S/L + numéro)
- **Report de solde** : Logique d'ajout du solde lors d'un paiement

---

## 📝 Checklist de préparation

### Avant la démo

- [ ] Base de données créée avec utilisateurs SQL
- [ ] Seed de données exécuté (sites, terrains, membres)
- [ ] API démarre sans erreur (vérifier Swagger)
- [ ] Frontend démarre sans erreur (vérifier interface)
- [ ] Tests passent (`dotnet test`)
- [ ] Git propre avec historique de commits
- [ ] README à jour avec instructions de lancement

### Pendant la démo

- [ ] Présenter l'architecture globale (schéma)
- [ ] Ouvrir Swagger et montrer les endpoints
- [ ] Créer un match complet (scénario 2)
- [ ] Montrer les règles J-1 (scénario 4)
- [ ] Expliquer la sécurité DB (utilisateurs, droits)
- [ ] Montrer 2-3 tests unitaires
- [ ] Expliquer l'injection de dépendances
- [ ] Répondre aux questions techniques

### Questions attendues

1. **Pourquoi Clean Architecture ?**
   → Séparation des responsabilités, testabilité, maintenabilité, indépendance des frameworks

2. **Pourquoi pas Swagger UI en production ?**
   → Sécurité : exposition de l'API, risque de découverte d'endpoints sensibles. À désactiver ou sécuriser.

3. **Comment gérer les transactions ?**
   → `_context.Database.BeginTransaction()` ou `using var transaction = ...` pour les opérations multi-tables

4. **Pourquoi pas de login ?**
   → Simplification demandée dans l'énoncé, focus sur la logique métier

5. **Scalabilité du background job ?**
   → Pour production : utiliser Hangfire, Azure Functions ou Kubernetes CronJobs plutôt que `BackgroundService`

6. **Gestion des erreurs globales ?**
   → Middleware d'exception : `app.UseExceptionHandler()` pour catcher et logger toutes les exceptions

7. **Tests d'intégration ?**
   → Possibles avec `WebApplicationFactory<Program>` pour tester l'API end-to-end

---

## 🎓 Conclusion

Ce projet démontre une application complète .NET avec :

✅ Architecture propre et modulaire  
✅ API REST sécurisée  
✅ Frontend moderne Blazor  
✅ Logique métier complexe (règles J-1, validations, soldes)  
✅ Base de données sécurisée (utilisateurs dédiés)  
✅ Tests unitaires complets  
✅ Background jobs  
✅ Injection de dépendances  
✅ Bonnes pratiques (DTOs, repositories, services)  

**Prêt pour la démo et l'explication technique ! 🚀**
