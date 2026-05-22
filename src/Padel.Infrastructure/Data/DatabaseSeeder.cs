using Microsoft.EntityFrameworkCore;
using Padel.Domain.Entities;
using DomainMatchType = Padel.Domain.Entities.MatchType;
using DomainMatchStatus = Padel.Domain.Entities.MatchStatus;
using DomainMemberType = Padel.Domain.Entities.MemberType;
using DomainPaymentStatus = Padel.Domain.Entities.PaymentStatus;

namespace Padel.Infrastructure.Data;

/// <summary>
/// Service responsable de l'initialisation automatique des données de démonstration
/// </summary>
public class DatabaseSeeder
{
    private readonly PadelDbContext _context;

    public DatabaseSeeder(PadelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Initialise la base de données avec des données de démonstration si elle est vide
    /// </summary>
    public async Task SeedAsync()
    {
        // Vérifier si la base contient déjà des données
        if (await _context.Sites.AnyAsync())
        {
            // La base est déjà initialisée
            return;
        }

        Console.WriteLine("⏳ Initialisation de la base de données avec des données de démonstration...");

        // =============================================
        // 1. SITES
        // =============================================
        var sites = new List<Site>
        {
            new() { Name = "Padel Center Bruxelles", Address = "123 Avenue Louise, 1050 Bruxelles" },
            new() { Name = "Padel Club Liège", Address = "45 Rue Saint-Laurent, 4000 Liège" },
            new() { Name = "Padel Arena Namur", Address = "78 Boulevard de la Meuse, 5000 Namur" }
        };
        await _context.Sites.AddRangeAsync(sites);
        await _context.SaveChangesAsync();

        // =============================================
        // 2. TERRAINS
        // =============================================
        var courts = new List<Court>
        {
            // Site 1 : Bruxelles (3 terrains)
            new() { Name = "Terrain Central", SiteId = sites[0].Id },
            new() { Name = "Terrain Nord", SiteId = sites[0].Id },
            new() { Name = "Terrain Sud", SiteId = sites[0].Id },
            // Site 2 : Liège (2 terrains)
            new() { Name = "Court Principal", SiteId = sites[1].Id },
            new() { Name = "Court Annexe", SiteId = sites[1].Id },
            // Site 3 : Namur (2 terrains)
            new() { Name = "Arena 1", SiteId = sites[2].Id },
            new() { Name = "Arena 2", SiteId = sites[2].Id }
        };
        await _context.Courts.AddRangeAsync(courts);
        await _context.SaveChangesAsync();

        // =============================================
        // 3. HORAIRES (année en cours)
        // =============================================
        var currentYear = DateTime.Now.Year;
        var schedules = new List<SiteSchedule>
        {
            new() { SiteId = sites[0].Id, Year = currentYear, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(22, 0) },
            new() { SiteId = sites[1].Id, Year = currentYear, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(21, 0) },
            new() { SiteId = sites[2].Id, Year = currentYear, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(23, 0) }
        };
        await _context.SiteSchedules.AddRangeAsync(schedules);
        await _context.SaveChangesAsync();

        // =============================================
        // 4. JOURS DE FERMETURE
        // =============================================
        var closureDays = new List<ClosureDay>
        {
            // Fermetures globales
            new() { Date = new DateOnly(2025, 1, 1), Reason = "Nouvel An", SiteId = null },
            new() { Date = new DateOnly(2025, 12, 25), Reason = "Noël", SiteId = null },
            // Fermetures spécifiques
            new() { Date = new DateOnly(2025, 3, 15), Reason = "Maintenance annuelle", SiteId = sites[0].Id },
            new() { Date = new DateOnly(2025, 6, 20), Reason = "Événement privé", SiteId = sites[1].Id }
        };
        await _context.ClosureDays.AddRangeAsync(closureDays);
        await _context.SaveChangesAsync();

        // =============================================
        // 5. MEMBRES
        // =============================================
        var members = new List<Member>
        {
            // Membres Global
            new() { Matricule = "G0001", FirstName = "Jean", LastName = "Dupont", Email = "jean.dupont@email.com", MemberType = DomainMemberType.Global },
            new() { Matricule = "G0002", FirstName = "Marie", LastName = "Martin", Email = "marie.martin@email.com", MemberType = DomainMemberType.Global },
            new() { Matricule = "G0003", FirstName = "Pierre", LastName = "Bernard", Email = "pierre.bernard@email.com", MemberType = DomainMemberType.Global },
            new() { Matricule = "G0004", FirstName = "Sophie", LastName = "Dubois", Email = "sophie.dubois@email.com", MemberType = DomainMemberType.Global },
            // Membres Site
            new() { Matricule = "S00001", FirstName = "Luc", LastName = "Leroy", Email = "luc.leroy@email.com", MemberType = DomainMemberType.Site, SiteId = sites[0].Id },
            new() { Matricule = "S00002", FirstName = "Anne", LastName = "Moreau", Email = "anne.moreau@email.com", MemberType = DomainMemberType.Site, SiteId = sites[0].Id },
            new() { Matricule = "S00003", FirstName = "Thomas", LastName = "Simon", Email = "thomas.simon@email.com", MemberType = DomainMemberType.Site, SiteId = sites[1].Id },
            new() { Matricule = "S00004", FirstName = "Julie", LastName = "Laurent", Email = "julie.laurent@email.com", MemberType = DomainMemberType.Site, SiteId = sites[1].Id },
            new() { Matricule = "S00005", FirstName = "Marc", LastName = "Petit", Email = "marc.petit@email.com", MemberType = DomainMemberType.Site, SiteId = sites[2].Id },
            // Membres Libre
            new() { Matricule = "L00001", FirstName = "Paul", LastName = "Roux", Email = "paul.roux@email.com", MemberType = DomainMemberType.Libre },
            new() { Matricule = "L00002", FirstName = "Emma", LastName = "Garcia", Email = "emma.garcia@email.com", MemberType = DomainMemberType.Libre },
            new() { Matricule = "L00003", FirstName = "Lucas", LastName = "Robert", Email = "lucas.robert@email.com", MemberType = DomainMemberType.Libre },
            // Membre bloqué
            new() { Matricule = "G0005", FirstName = "Alex", LastName = "Bloqué", Email = "alex.bloque@email.com", MemberType = DomainMemberType.Global, ReservationBlocked = true, BlockedUntil = DateTime.Now.AddDays(5) }
        };
        await _context.Members.AddRangeAsync(members);
        await _context.SaveChangesAsync();

        // =============================================
        // 6. MATCHS
        // =============================================
        var today = DateTime.Today;
        var matches = new List<Match>
        {
            // Match 1 : Privé complet demain 10h-11h30
            new() 
            { 
                CourtId = courts[0].Id, 
                OrganizerId = members[0].Id, 
                ScheduledAt = today.AddDays(1).AddHours(10), 
                EndsAt = today.AddDays(1).AddHours(11).AddMinutes(30), 
                MatchType = DomainMatchType.Private, 
                Status = DomainMatchStatus.Full
            },
            // Match 2 : Public avec 2 joueurs après-demain 14h-15h30
            new() 
            { 
                CourtId = courts[0].Id, 
                OrganizerId = members[2].Id, 
                ScheduledAt = today.AddDays(2).AddHours(14), 
                EndsAt = today.AddDays(2).AddHours(15).AddMinutes(30), 
                MatchType = DomainMatchType.Public, 
                Status = DomainMatchStatus.Scheduled 
            },
            // Match 3 : Public complet dans 3 jours 16h-17h30
            new() 
            { 
                CourtId = courts[3].Id, 
                OrganizerId = members[6].Id, 
                ScheduledAt = today.AddDays(3).AddHours(16), 
                EndsAt = today.AddDays(3).AddHours(17).AddMinutes(30), 
                MatchType = DomainMatchType.Public, 
                Status = DomainMatchStatus.Full
            },
            // Match 4 : Privé incomplet dans 4 jours 18h-19h30
            new() 
            { 
                CourtId = courts[5].Id, 
                OrganizerId = members[8].Id, 
                ScheduledAt = today.AddDays(4).AddHours(18), 
                EndsAt = today.AddDays(4).AddHours(19).AddMinutes(30), 
                MatchType = DomainMatchType.Private, 
                Status = DomainMatchStatus.Scheduled 
            },
            // Match 5 : Public avec 3 joueurs dans 5 jours 10h-11h30
            new() 
            { 
                CourtId = courts[1].Id, 
                OrganizerId = members[4].Id, 
                ScheduledAt = today.AddDays(5).AddHours(10), 
                EndsAt = today.AddDays(5).AddHours(11).AddMinutes(30), 
                MatchType = DomainMatchType.Public, 
                Status = DomainMatchStatus.Scheduled
            },
            // Match 6 : Privé avec 3 joueurs dans 6 jours 20h-21h30
            new() 
            { 
                CourtId = courts[4].Id, 
                OrganizerId = members[7].Id, 
                ScheduledAt = today.AddDays(6).AddHours(20), 
                EndsAt = today.AddDays(6).AddHours(21).AddMinutes(30), 
                MatchType = DomainMatchType.Private, 
                Status = DomainMatchStatus.Scheduled 
            },
            // Match 7 : Passé et complété (il y a 2 jours)
            new() 
            { 
                CourtId = courts[2].Id, 
                OrganizerId = members[1].Id, 
                ScheduledAt = today.AddDays(-2).AddHours(10), 
                EndsAt = today.AddDays(-2).AddHours(11).AddMinutes(30), 
                MatchType = DomainMatchType.Public, 
                Status = DomainMatchStatus.Completed
            }
        };
        await _context.Matches.AddRangeAsync(matches);
        await _context.SaveChangesAsync();

        // =============================================
        // 7. JOUEURS INSCRITS AUX MATCHS
        // =============================================
        var matchPlayers = new List<MatchPlayer>
        {
            // Match 1 : 4 joueurs
            new() { MatchId = matches[0].Id, MemberId = members[0].Id, JoinedAt = DateTime.Now.AddDays(-10) },
            new() { MatchId = matches[0].Id, MemberId = members[1].Id, JoinedAt = DateTime.Now.AddDays(-10) },
            new() { MatchId = matches[0].Id, MemberId = members[2].Id, JoinedAt = DateTime.Now.AddDays(-9) },
            new() { MatchId = matches[0].Id, MemberId = members[3].Id, JoinedAt = DateTime.Now.AddDays(-8) },
            // Match 2 : 2 joueurs
            new() { MatchId = matches[1].Id, MemberId = members[2].Id, JoinedAt = DateTime.Now.AddDays(-7) },
            new() { MatchId = matches[1].Id, MemberId = members[9].Id, JoinedAt = DateTime.Now.AddDays(-6) },
            // Match 3 : 4 joueurs
            new() { MatchId = matches[2].Id, MemberId = members[6].Id, JoinedAt = DateTime.Now.AddDays(-5) },
            new() { MatchId = matches[2].Id, MemberId = members[7].Id, JoinedAt = DateTime.Now.AddDays(-5) },
            new() { MatchId = matches[2].Id, MemberId = members[10].Id, JoinedAt = DateTime.Now.AddDays(-4) },
            new() { MatchId = matches[2].Id, MemberId = members[11].Id, JoinedAt = DateTime.Now.AddDays(-3) },
            // Match 4 : 2 joueurs
            new() { MatchId = matches[3].Id, MemberId = members[8].Id, JoinedAt = DateTime.Now.AddDays(-4) },
            new() { MatchId = matches[3].Id, MemberId = members[3].Id, JoinedAt = DateTime.Now.AddDays(-3) },
            // Match 5 : 3 joueurs
            new() { MatchId = matches[4].Id, MemberId = members[4].Id, JoinedAt = DateTime.Now.AddDays(-3) },
            new() { MatchId = matches[4].Id, MemberId = members[5].Id, JoinedAt = DateTime.Now.AddDays(-2) },
            new() { MatchId = matches[4].Id, MemberId = members[1].Id, JoinedAt = DateTime.Now.AddDays(-2) },
            // Match 6 : 3 joueurs
            new() { MatchId = matches[5].Id, MemberId = members[7].Id, JoinedAt = DateTime.Now.AddDays(-2) },
            new() { MatchId = matches[5].Id, MemberId = members[0].Id, JoinedAt = DateTime.Now.AddDays(-1) },
            new() { MatchId = matches[5].Id, MemberId = members[9].Id, JoinedAt = DateTime.Now.AddDays(-1) },
            // Match 7 : 4 joueurs
            new() { MatchId = matches[6].Id, MemberId = members[1].Id, JoinedAt = DateTime.Now.AddDays(-15) },
            new() { MatchId = matches[6].Id, MemberId = members[4].Id, JoinedAt = DateTime.Now.AddDays(-15) },
            new() { MatchId = matches[6].Id, MemberId = members[5].Id, JoinedAt = DateTime.Now.AddDays(-14) },
            new() { MatchId = matches[6].Id, MemberId = members[10].Id, JoinedAt = DateTime.Now.AddDays(-13) }
        };
        await _context.MatchPlayers.AddRangeAsync(matchPlayers);
        await _context.SaveChangesAsync();

        // =============================================
        // 8. PAIEMENTS
        // =============================================
        var payments = new List<Payment>
        {
            // Match 1 : Tous payés
            new() { MatchPlayerId = matchPlayers[0].Id, MatchId = matches[0].Id, MemberId = members[0].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-10), PaidAt = DateTime.Now.AddDays(-9) },
            new() { MatchPlayerId = matchPlayers[1].Id, MatchId = matches[0].Id, MemberId = members[1].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-10), PaidAt = DateTime.Now.AddDays(-9) },
            new() { MatchPlayerId = matchPlayers[2].Id, MatchId = matches[0].Id, MemberId = members[2].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-9), PaidAt = DateTime.Now.AddDays(-8) },
            new() { MatchPlayerId = matchPlayers[3].Id, MatchId = matches[0].Id, MemberId = members[3].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-8), PaidAt = DateTime.Now.AddDays(-7) },
            // Match 2 : 1 payé, 1 en attente
            new() { MatchPlayerId = matchPlayers[4].Id, MatchId = matches[1].Id, MemberId = members[2].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-7), PaidAt = DateTime.Now.AddDays(-6) },
            new() { MatchPlayerId = matchPlayers[5].Id, MatchId = matches[1].Id, MemberId = members[9].Id, Amount = 15.00m, Status = DomainPaymentStatus.Pending, CreatedAt = DateTime.Now.AddDays(-6), PaidAt = null },
            // Match 3 : Tous payés
            new() { MatchPlayerId = matchPlayers[6].Id, MatchId = matches[2].Id, MemberId = members[6].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-5), PaidAt = DateTime.Now.AddDays(-4) },
            new() { MatchPlayerId = matchPlayers[7].Id, MatchId = matches[2].Id, MemberId = members[7].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-5), PaidAt = DateTime.Now.AddDays(-4) },
            new() { MatchPlayerId = matchPlayers[8].Id, MatchId = matches[2].Id, MemberId = members[10].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-4), PaidAt = DateTime.Now.AddDays(-3) },
            new() { MatchPlayerId = matchPlayers[9].Id, MatchId = matches[2].Id, MemberId = members[11].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-3), PaidAt = DateTime.Now.AddDays(-2) },
            // Match 4 : 1 payé, 1 en attente
            new() { MatchPlayerId = matchPlayers[10].Id, MatchId = matches[3].Id, MemberId = members[8].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-4), PaidAt = DateTime.Now.AddDays(-3) },
            new() { MatchPlayerId = matchPlayers[11].Id, MatchId = matches[3].Id, MemberId = members[3].Id, Amount = 15.00m, Status = DomainPaymentStatus.Pending, CreatedAt = DateTime.Now.AddDays(-3), PaidAt = null },
            // Match 5 : 2 payés, 1 en attente
            new() { MatchPlayerId = matchPlayers[12].Id, MatchId = matches[4].Id, MemberId = members[4].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-3), PaidAt = DateTime.Now.AddDays(-2) },
            new() { MatchPlayerId = matchPlayers[13].Id, MatchId = matches[4].Id, MemberId = members[5].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-2), PaidAt = DateTime.Now.AddDays(-1) },
            new() { MatchPlayerId = matchPlayers[14].Id, MatchId = matches[4].Id, MemberId = members[1].Id, Amount = 15.00m, Status = DomainPaymentStatus.Pending, CreatedAt = DateTime.Now.AddDays(-2), PaidAt = null },
            // Match 6 : Tous en attente
            new() { MatchPlayerId = matchPlayers[15].Id, MatchId = matches[5].Id, MemberId = members[7].Id, Amount = 15.00m, Status = DomainPaymentStatus.Pending, CreatedAt = DateTime.Now.AddDays(-2), PaidAt = null },
            new() { MatchPlayerId = matchPlayers[16].Id, MatchId = matches[5].Id, MemberId = members[0].Id, Amount = 15.00m, Status = DomainPaymentStatus.Pending, CreatedAt = DateTime.Now.AddDays(-1), PaidAt = null },
            new() { MatchPlayerId = matchPlayers[17].Id, MatchId = matches[5].Id, MemberId = members[9].Id, Amount = 15.00m, Status = DomainPaymentStatus.Pending, CreatedAt = DateTime.Now.AddDays(-1), PaidAt = null },
            // Match 7 : Tous payés
            new() { MatchPlayerId = matchPlayers[18].Id, MatchId = matches[6].Id, MemberId = members[1].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-15), PaidAt = DateTime.Now.AddDays(-14) },
            new() { MatchPlayerId = matchPlayers[19].Id, MatchId = matches[6].Id, MemberId = members[4].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-15), PaidAt = DateTime.Now.AddDays(-14) },
            new() { MatchPlayerId = matchPlayers[20].Id, MatchId = matches[6].Id, MemberId = members[5].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-14), PaidAt = DateTime.Now.AddDays(-13) },
            new() { MatchPlayerId = matchPlayers[21].Id, MatchId = matches[6].Id, MemberId = members[10].Id, Amount = 15.00m, Status = DomainPaymentStatus.Paid, CreatedAt = DateTime.Now.AddDays(-13), PaidAt = DateTime.Now.AddDays(-12) }
        };
        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        // =============================================
        // Résumé
        // =============================================
        var totalPaid = payments.Where(p => p.Status == DomainPaymentStatus.Paid).Sum(p => p.Amount);

        Console.WriteLine("✅ Initialisation terminée !");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine($"📍 Sites : {sites.Count}");
        Console.WriteLine($"🎾 Terrains : {courts.Count}");
        Console.WriteLine($"⏰ Horaires : {schedules.Count}");
        Console.WriteLine($"🚫 Jours de fermeture : {closureDays.Count}");
        Console.WriteLine($"👥 Membres : {members.Count}");
        Console.WriteLine($"   - Global : {members.Count(m => m.MemberType == DomainMemberType.Global)}");
        Console.WriteLine($"   - Site : {members.Count(m => m.MemberType == DomainMemberType.Site)}");
        Console.WriteLine($"   - Libre : {members.Count(m => m.MemberType == DomainMemberType.Libre)}");
        Console.WriteLine($"🏆 Matchs : {matches.Count}");
        Console.WriteLine($"   - Privés : {matches.Count(m => m.MatchType == DomainMatchType.Private)}");
        Console.WriteLine($"   - Publics : {matches.Count(m => m.MatchType == DomainMatchType.Public)}");
        Console.WriteLine($"🎮 Joueurs inscrits : {matchPlayers.Count}");
        Console.WriteLine($"💰 Paiements : {payments.Count}");
        Console.WriteLine($"   - Payés : {payments.Count(p => p.Status == DomainPaymentStatus.Paid)}");
        Console.WriteLine($"   - En attente : {payments.Count(p => p.Status == DomainPaymentStatus.Pending)}");
        Console.WriteLine($"💵 Chiffre d'affaires total : {totalPaid:F2} €");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
}
