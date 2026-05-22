# Initialisation automatique de la base de données

## Vue d'ensemble

L'application **Padel Manager** initialise automatiquement la base de données au premier démarrage, incluant :
- Création du schéma (tables, contraintes, index)
- Insertion de données de démonstration si la base est vide

Cette fonctionnalité est idéale pour :
- ✅ Développement local rapide
- ✅ Présentations et démonstrations
- ✅ Tests fonctionnels avec données réalistes

## Fonctionnement

### 1. Démarrage automatique

Au lancement de l'API (`dotnet run` dans `src/Padel.Api`), le système :

1. **Vérifie** si la base `PadelDb` existe
2. **Crée** le schéma si nécessaire (`EnsureCreatedAsync`)
3. **Vérifie** si des sites existent déjà (indicateur que la base est initialisée)
4. **Insère** les données de démonstration uniquement si la base est vide

### 2. Code d'initialisation

Le service `DatabaseSeeder` (fichier `src/Padel.Infrastructure/Data/DatabaseSeeder.cs`) contient toute la logique d'insertion.

La méthode principale `SeedAsync()` :

```csharp
public async Task SeedAsync()
{
    // Vérifier si la base contient déjà des données
    if (await _context.Sites.AnyAsync())
    {
        // La base est déjà initialisée
        return;
    }

    Console.WriteLine("⏳ Initialisation de la base de données avec des données de démonstration...");

    // Insertion de 3 sites, 7 terrains, 14 membres, 7 matchs, 22 paiements...
    // ...
}
```

### 3. Exécution dans `Program.cs`

Le code suivant est exécuté automatiquement au démarrage de l'API :

```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PadelDbContext>();
        
        // Créer la base de données si elle n'existe pas
        await context.Database.EnsureCreatedAsync();
        
        // Initialiser les données de démonstration si la base est vide
        var seeder = new DatabaseSeeder(context);
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Une erreur s'est produite lors de l'initialisation de la base de données.");
    }
}
```

## Données insérées

### Sites (3)
- Padel Center Bruxelles
- Padel Club Liège
- Padel Arena Namur

### Terrains (7)
- 3 terrains à Bruxelles
- 2 terrains à Liège
- 2 terrains à Namur

### Membres (14)
- **5 Global** (G0001-G0005) : réservation 21 jours à l'avance, tous sites
- **5 Site** (S00001-S00005) : réservation 14 jours à l'avance, site unique
- **3 Libre** (L00001-L00003) : réservation 5 jours à l'avance, tous sites
- **1 membre bloqué** (G0005) : bloqué pendant 5 jours

### Matchs (7)
- Match 1 : Privé complet, demain 10h-11h30 (Bruxelles) — **tous payés**
- Match 2 : Public incomplet (2/4), J+2 14h-15h30 (Bruxelles) — **1 impayé**
- Match 3 : Public complet, J+3 16h-17h30 (Liège) — **tous payés**
- Match 4 : Privé incomplet (2/4), J+4 18h-19h30 (Namur) — **1 impayé**
- Match 5 : Public incomplet (3/4), J+5 10h-11h30 (Bruxelles) — **1 impayé**
- Match 6 : Privé incomplet (3/4), J+6 20h-21h30 (Liège) — **3 impayés (risque retrait J-1)**
- Match 7 : Public complet, J-2 10h-11h30 (Bruxelles) — **complété, tous payés**

### Paiements (22)
- **14 paiements payés** (statut `Paid`) = **210 €** de chiffre d'affaires
- **8 paiements en attente** (statut `Pending`) = **120 €** de solde dû

## Configuration

### Activer/Désactiver l'initialisation automatique

Par défaut, l'auto-seeding est **activé**.

Pour le **désactiver** (en production par exemple) :

**`appsettings.json` ou `appsettings.Production.json` :**
```json
{
  "Database": {
    "AutoSeedData": false
  }
}
```

**`appsettings.Development.json` :**
```json
{
  "Database": {
    "AutoSeedData": true
  }
}
```

### Réinitialiser la base de données

Pour réinitialiser complètement la base avec de nouvelles données :

**Option 1 : Supprimer la base via SQL Server Management Studio ou sqlcmd**
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "DROP DATABASE PadelDb"
```

**Option 2 : Supprimer le fichier de base de données LocalDB**
```bash
# Le fichier se trouve généralement dans :
%LOCALAPPDATA%\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\
```

Ensuite, relancez l'API. La base sera recréée et réinitialisée automatiquement.

## Scénarios de démonstration possibles

### 1. Tester les créneaux disponibles
- Allez sur **Réservation** → sélectionnez **Terrain Central** (Bruxelles) → choisissez **demain**
- Vous verrez que le créneau **10h-11h30** est indisponible (Match 1 déjà réservé)

### 2. Rejoindre un match public
- Allez sur **Matchs publics** → filtre **Bruxelles**
- Match 2 (après-demain 14h) : seulement 2 joueurs inscrits, vous pouvez rejoindre

### 3. Tester le solde impayé
- Allez sur **Mes paiements** → saisir matricule **L00001** (Paul Roux)
- Solde dû : **15 €** (Match 2)
- Essayez de créer un nouveau match avec ce membre → refusé !

### 4. Tester le blocage
- Essayez de créer un match avec le membre **G0005** (Alex Bloqué) → refusé (bloqué pendant 5 jours)

### 5. Tester les règles J-1 (manuellement)
- Allez sur **Administration** → **Matchs**
- Cliquez sur **Déclencher règles J-1**
- Les matchs incomplets seront convertis en publics, les joueurs impayés retirés

### 6. Consulter les statistiques
- Allez sur **Statistiques**
- Chiffre d'affaires global : **210 €**
- Répartition par site (Bruxelles, Liège, Namur)

## Avantages de l'approche C# vs Script SQL

| Aspect | Script SQL (`SeedData.sql`) | Service C# (`DatabaseSeeder`) |
|--------|------------------------------|-------------------------------|
| Portabilité | ❌ Dépendant de SQL Server | ✅ Portable (compatible EF Core) |
| Maintenance | ❌ Duplication de logique | ✅ Réutilise les entités Domain |
| Exécution | ❌ Manuelle (`sqlcmd`) | ✅ Automatique au démarrage |
| Dates dynamiques | ⚠️ `GETDATE()`, `DATEADD()` | ✅ `DateTime.Now.AddDays(1)` |
| Tests | ❌ Difficile à tester | ✅ Testable avec InMemory DB |
| Déploiement | ❌ Fichier SQL séparé | ✅ Intégré dans l'application |

## Notes techniques

### Gestion des conflits de noms

Le fichier `DatabaseSeeder.cs` utilise des **alias de types** pour éviter les conflits avec `System.IO.MatchType` :

```csharp
using DomainMatchType = Padel.Domain.Entities.MatchType;
using DomainMatchStatus = Padel.Domain.Entities.MatchStatus;
using DomainMemberType = Padel.Domain.Entities.MemberType;
using DomainPaymentStatus = Padel.Domain.Entities.PaymentStatus;
```

Cela permet d'utiliser `DomainMatchType.Private` au lieu de `MatchType.Private` (ambigu).

### Performance

L'initialisation prend environ **2-3 secondes** au premier démarrage. Les démarrages suivants détectent immédiatement que la base est déjà initialisée (requête `Sites.AnyAsync()` en quelques millisecondes) et skip l'insertion.

### Logs

La console affiche un résumé détaillé à la fin de l'initialisation :

```
⏳ Initialisation de la base de données avec des données de démonstration...
✅ Initialisation terminée !
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📍 Sites : 3
🎾 Terrains : 7
⏰ Horaires : 3
🚫 Jours de fermeture : 4
👥 Membres : 14
   - Global : 5
   - Site : 5
   - Libre : 3
🏆 Matchs : 7
   - Privés : 4
   - Publics : 3
🎮 Joueurs inscrits : 22
💰 Paiements : 22
   - Payés : 14
   - En attente : 8
💵 Chiffre d'affaires total : 210,00 €
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## Recommandations

### En développement
✅ Garder `AutoSeedData: true` pour faciliter les tests

### En production
⚠️ Définir `AutoSeedData: false` pour éviter de réinitialiser les données à chaque démarrage

### Pour une présentation
✅ Supprimer la base avant chaque démo pour garantir des données fraîches et cohérentes
