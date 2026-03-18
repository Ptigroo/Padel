using Moq;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Domain.Entities;
using Match = Padel.Domain.Entities.Match;
using MatchType = Padel.Domain.Entities.MatchType;

namespace Padel.Tests.Services;

public class MatchServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();
    private readonly Mock<IMemberRepository> _memberRepo = new();
    private readonly Mock<ICourtRepository> _courtRepo = new();
    private readonly Mock<ISiteScheduleRepository> _scheduleRepo = new();
    private readonly Mock<IClosureDayRepository> _closureRepo = new();
    private readonly Mock<IPaymentRepository> _paymentRepo = new();

    private MatchService CreateService() => new(
        _matchRepo.Object,
        _memberRepo.Object,
        _courtRepo.Object,
        _scheduleRepo.Object,
        _closureRepo.Object,
        _paymentRepo.Object);

    private static Member CreateGlobalMember(int id = 1, string matricule = "G0001") => new()
    {
        Id = id,
        Matricule = matricule,
        FirstName = "Jean",
        LastName = "Dupont",
        Email = "jean@test.com",
        MemberType = MemberType.Global
    };

    private static Member CreateSiteMember(int id = 2, string matricule = "S00001", int siteId = 1) => new()
    {
        Id = id,
        Matricule = matricule,
        FirstName = "Marie",
        LastName = "Martin",
        Email = "marie@test.com",
        MemberType = MemberType.Site,
        SiteId = siteId
    };

    private static Court CreateCourt(int id = 1, int siteId = 1) => new()
    {
        Id = id,
        Name = "Terrain 1",
        SiteId = siteId,
        Site = new Site { Id = siteId, Name = "Site 1", Address = "Adresse 1" }
    };

    private static SiteSchedule CreateSchedule(int siteId = 1) => new()
    {
        Id = 1,
        SiteId = siteId,
        Year = DateTime.Today.Year,
        StartTime = new TimeOnly(8, 0),
        EndTime = new TimeOnly(22, 0)
    };

    private void SetupNominalCreate(Member organizer, Court court)
    {
        _memberRepo.Setup(r => r.GetByMatriculeAsync(organizer.Matricule))
            .ReturnsAsync(organizer);
        _paymentRepo.Setup(r => r.GetUnpaidBalanceAsync(organizer.Id))
            .ReturnsAsync(0m);
        _courtRepo.Setup(r => r.GetByIdAsync(court.Id))
            .ReturnsAsync(court);
        _closureRepo.Setup(r => r.ExistsForDateAndSiteAsync(It.IsAny<DateOnly>(), court.SiteId))
            .ReturnsAsync(false);
        _scheduleRepo.Setup(r => r.GetForYearAsync(court.SiteId, It.IsAny<int>()))
            .ReturnsAsync(CreateSchedule(court.SiteId));
        _matchRepo.Setup(r => r.HasConflictAsync(court.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        _matchRepo.Setup(r => r.CreateAsync(It.IsAny<Match>()))
            .ReturnsAsync((Match m) => { m.Id = 1; return m; });
        _matchRepo.Setup(r => r.UpdateAsync(It.IsAny<Match>()))
            .ReturnsAsync((Match m) => m);
        _paymentRepo.Setup(r => r.CreateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => { p.Id = 1; return p; });
        _matchRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((int id) =>
            {
                var m = new Match
                {
                    Id = id,
                    CourtId = court.Id,
                    Court = court,
                    OrganizerId = organizer.Id,
                    Organizer = organizer,
                    ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
                    EndsAt = DateTime.Today.AddDays(1).AddHours(11).AddMinutes(30),
                    MatchType = MatchType.Private,
                    Status = MatchStatus.Scheduled,
                    Players = [new MatchPlayer { Id = 1, MatchId = id, MemberId = organizer.Id, Member = organizer, JoinedAt = DateTime.Now }]
                };
                return m;
            });
    }

    // ═══════════════════════════════════════
    // CreateAsync — Cas nominal
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_Nominal_ReturnsMatchWithOrganizer()
    {
        var organizer = CreateGlobalMember();
        var court = CreateCourt();
        SetupNominalCreate(organizer, court);

        var service = CreateService();
        var dto = new CreateMatchDto
        {
            CourtId = court.Id,
            OrganizerMatricule = organizer.Matricule,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            MatchType = "Private"
        };

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Scheduled", result.Status);
        Assert.Equal(1, result.PlayerCount);
        _matchRepo.Verify(r => r.CreateAsync(It.IsAny<Match>()), Times.Once);
        _paymentRepo.Verify(r => r.CreateAsync(It.Is<Payment>(p => p.Amount == 15m)), Times.Once);
    }

    // ═══════════════════════════════════════
    // CreateAsync — Membre bloqué (CF-RV-014)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_BlockedMember_ThrowsException()
    {
        var organizer = CreateGlobalMember();
        organizer.ReservationBlocked = true;
        organizer.BlockedUntil = DateTime.Now.AddDays(5);

        _memberRepo.Setup(r => r.GetByMatriculeAsync(organizer.Matricule)).ReturnsAsync(organizer);

        var service = CreateService();
        var dto = new CreateMatchDto
        {
            CourtId = 1,
            OrganizerMatricule = organizer.Matricule,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            MatchType = "Private"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        Assert.Contains("blocked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // CreateAsync — Solde impayé (CF-RV-013)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_UnpaidBalance_ThrowsException()
    {
        var organizer = CreateGlobalMember();
        _memberRepo.Setup(r => r.GetByMatriculeAsync(organizer.Matricule)).ReturnsAsync(organizer);
        _paymentRepo.Setup(r => r.GetUnpaidBalanceAsync(organizer.Id)).ReturnsAsync(15m);

        var service = CreateService();
        var dto = new CreateMatchDto
        {
            CourtId = 1,
            OrganizerMatricule = organizer.Matricule,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            MatchType = "Private"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        Assert.Contains("unpaid", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // CreateAsync — Mauvais site (CF-RV-015)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_SiteMemberWrongSite_ThrowsException()
    {
        var organizer = CreateSiteMember(siteId: 2);
        var court = CreateCourt(siteId: 1);

        _memberRepo.Setup(r => r.GetByMatriculeAsync(organizer.Matricule)).ReturnsAsync(organizer);
        _paymentRepo.Setup(r => r.GetUnpaidBalanceAsync(organizer.Id)).ReturnsAsync(0m);
        _courtRepo.Setup(r => r.GetByIdAsync(court.Id)).ReturnsAsync(court);

        var service = CreateService();
        var dto = new CreateMatchDto
        {
            CourtId = court.Id,
            OrganizerMatricule = organizer.Matricule,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            MatchType = "Private"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        Assert.Contains("site", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // CreateAsync — Hors fenêtre (CF-RV-016)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_OutsideReservationWindow_ThrowsException()
    {
        var organizer = CreateGlobalMember();
        organizer.MemberType = MemberType.Libre; // 5 days max
        var court = CreateCourt();

        _memberRepo.Setup(r => r.GetByMatriculeAsync(organizer.Matricule)).ReturnsAsync(organizer);
        _paymentRepo.Setup(r => r.GetUnpaidBalanceAsync(organizer.Id)).ReturnsAsync(0m);
        _courtRepo.Setup(r => r.GetByIdAsync(court.Id)).ReturnsAsync(court);

        var service = CreateService();
        var dto = new CreateMatchDto
        {
            CourtId = court.Id,
            OrganizerMatricule = organizer.Matricule,
            ScheduledAt = DateTime.Today.AddDays(10).AddHours(10), // > 5 days
            MatchType = "Private"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        Assert.Contains("days", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // CreateAsync — Jour de fermeture (CF-RV-017)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_ClosureDay_ThrowsException()
    {
        var organizer = CreateGlobalMember();
        var court = CreateCourt();

        _memberRepo.Setup(r => r.GetByMatriculeAsync(organizer.Matricule)).ReturnsAsync(organizer);
        _paymentRepo.Setup(r => r.GetUnpaidBalanceAsync(organizer.Id)).ReturnsAsync(0m);
        _courtRepo.Setup(r => r.GetByIdAsync(court.Id)).ReturnsAsync(court);
        _closureRepo.Setup(r => r.ExistsForDateAndSiteAsync(It.IsAny<DateOnly>(), court.SiteId))
            .ReturnsAsync(true);

        var service = CreateService();
        var dto = new CreateMatchDto
        {
            CourtId = court.Id,
            OrganizerMatricule = organizer.Matricule,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            MatchType = "Private"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        Assert.Contains("closure", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // CreateAsync — Créneau déjà pris (CF-RV-019)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_ConflictingSlot_ThrowsException()
    {
        var organizer = CreateGlobalMember();
        var court = CreateCourt();

        _memberRepo.Setup(r => r.GetByMatriculeAsync(organizer.Matricule)).ReturnsAsync(organizer);
        _paymentRepo.Setup(r => r.GetUnpaidBalanceAsync(organizer.Id)).ReturnsAsync(0m);
        _courtRepo.Setup(r => r.GetByIdAsync(court.Id)).ReturnsAsync(court);
        _closureRepo.Setup(r => r.ExistsForDateAndSiteAsync(It.IsAny<DateOnly>(), court.SiteId))
            .ReturnsAsync(false);
        _scheduleRepo.Setup(r => r.GetForYearAsync(court.SiteId, It.IsAny<int>()))
            .ReturnsAsync(CreateSchedule(court.SiteId));
        _matchRepo.Setup(r => r.HasConflictAsync(court.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var dto = new CreateMatchDto
        {
            CourtId = court.Id,
            OrganizerMatricule = organizer.Matricule,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            MatchType = "Private"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        Assert.Contains("already scheduled", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // CreateAsync — Horaires hors limites (CF-RV-018)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_OutsideScheduleHours_ThrowsException()
    {
        var organizer = CreateGlobalMember();
        var court = CreateCourt();

        _memberRepo.Setup(r => r.GetByMatriculeAsync(organizer.Matricule)).ReturnsAsync(organizer);
        _paymentRepo.Setup(r => r.GetUnpaidBalanceAsync(organizer.Id)).ReturnsAsync(0m);
        _courtRepo.Setup(r => r.GetByIdAsync(court.Id)).ReturnsAsync(court);
        _closureRepo.Setup(r => r.ExistsForDateAndSiteAsync(It.IsAny<DateOnly>(), court.SiteId))
            .ReturnsAsync(false);
        _scheduleRepo.Setup(r => r.GetForYearAsync(court.SiteId, It.IsAny<int>()))
            .ReturnsAsync(new SiteSchedule
            {
                Id = 1, SiteId = court.SiteId, Year = DateTime.Today.Year,
                StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0) // tight window
            });
        _matchRepo.Setup(r => r.HasConflictAsync(court.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        var service = CreateService();
        var dto = new CreateMatchDto
        {
            CourtId = court.Id,
            OrganizerMatricule = organizer.Matricule,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(7), // 07:00 before 09:00
            MatchType = "Private"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        Assert.Contains("operating hours", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // JoinAsync — Cas nominal
    // ═══════════════════════════════════════

    [Fact]
    public async Task JoinAsync_Nominal_AddsPlayerAndCreatesPayment()
    {
        var organizer = CreateGlobalMember(1, "G0001");
        var player = CreateGlobalMember(2, "G0002");
        player.FirstName = "Paul";
        var court = CreateCourt();
        var match = new Match
        {
            Id = 1, CourtId = court.Id, Court = court,
            OrganizerId = organizer.Id, Organizer = organizer,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            EndsAt = DateTime.Today.AddDays(1).AddHours(11).AddMinutes(30),
            MatchType = MatchType.Public, Status = MatchStatus.Scheduled,
            Players = [new MatchPlayer { Id = 1, MatchId = 1, MemberId = organizer.Id, Member = organizer }]
        };

        _matchRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);
        _memberRepo.Setup(r => r.GetByMatriculeAsync("G0002")).ReturnsAsync(player);
        _matchRepo.Setup(r => r.UpdateAsync(It.IsAny<Match>())).ReturnsAsync((Match m) => m);
        _paymentRepo.Setup(r => r.CreateAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => { p.Id = 2; return p; });

        var service = CreateService();
        var result = await service.JoinAsync(1, "G0002");

        Assert.Equal(2, result.PlayerCount);
        _paymentRepo.Verify(r => r.CreateAsync(It.Is<Payment>(p => p.Amount == 15m && p.MemberId == 2)), Times.Once);
    }

    // ═══════════════════════════════════════
    // JoinAsync — Match déjà complet (CF-RV-021)
    // ═══════════════════════════════════════

    [Fact]
    public async Task JoinAsync_FullMatch_ThrowsException()
    {
        var court = CreateCourt();
        var match = new Match
        {
            Id = 1, CourtId = court.Id, Court = court,
            OrganizerId = 1, Organizer = CreateGlobalMember(),
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            EndsAt = DateTime.Today.AddDays(1).AddHours(11).AddMinutes(30),
            MatchType = MatchType.Public, Status = MatchStatus.Full,
            Players = []
        };

        _matchRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);

        var service = CreateService();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.JoinAsync(1, "G0002"));
        Assert.Contains("full", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // AddPlayerAsync — Non-organisateur (CF-RV-022)
    // ═══════════════════════════════════════

    [Fact]
    public async Task AddPlayerAsync_NotOrganizer_ThrowsException()
    {
        var organizer = CreateGlobalMember(1, "G0001");
        var impostor = CreateGlobalMember(3, "G0003");
        var court = CreateCourt();
        var match = new Match
        {
            Id = 1, CourtId = court.Id, Court = court,
            OrganizerId = organizer.Id, Organizer = organizer,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            EndsAt = DateTime.Today.AddDays(1).AddHours(11).AddMinutes(30),
            MatchType = MatchType.Private, Status = MatchStatus.Scheduled,
            Players = [new MatchPlayer { Id = 1, MatchId = 1, MemberId = organizer.Id, Member = organizer }]
        };

        _matchRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);
        _memberRepo.Setup(r => r.GetByMatriculeAsync("G0003")).ReturnsAsync(impostor);

        var service = CreateService();
        var dto = new AddPlayerDto { Matricule = "G0002", OrganizerMatricule = "G0003" };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddPlayerAsync(1, dto));
        Assert.Contains("organizer", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // AddPlayerAsync — Joueur déjà inscrit (CF-RV-020)
    // ═══════════════════════════════════════

    [Fact]
    public async Task AddPlayerAsync_AlreadyRegistered_ThrowsException()
    {
        var organizer = CreateGlobalMember(1, "G0001");
        var player = CreateGlobalMember(2, "G0002");
        var court = CreateCourt();
        var match = new Match
        {
            Id = 1, CourtId = court.Id, Court = court,
            OrganizerId = organizer.Id, Organizer = organizer,
            MatchType = MatchType.Private, Status = MatchStatus.Scheduled,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            EndsAt = DateTime.Today.AddDays(1).AddHours(11).AddMinutes(30),
            Players = [
                new MatchPlayer { Id = 1, MatchId = 1, MemberId = organizer.Id, Member = organizer },
                new MatchPlayer { Id = 2, MatchId = 1, MemberId = player.Id, Member = player }
            ]
        };

        _matchRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);
        _memberRepo.Setup(r => r.GetByMatriculeAsync("G0001")).ReturnsAsync(organizer);
        _memberRepo.Setup(r => r.GetByMatriculeAsync("G0002")).ReturnsAsync(player);

        var service = CreateService();
        var dto = new AddPlayerDto { Matricule = "G0002", OrganizerMatricule = "G0001" };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddPlayerAsync(1, dto));
        Assert.Contains("already registered", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
