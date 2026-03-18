using Moq;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Domain.Entities;
using Match = Padel.Domain.Entities.Match;
using MatchType = Padel.Domain.Entities.MatchType;

namespace Padel.Tests.Services;

public class DayBeforeMatchTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();
    private readonly Mock<IMemberRepository> _memberRepo = new();
    private readonly Mock<ICourtRepository> _courtRepo = new();
    private readonly Mock<ISiteScheduleRepository> _scheduleRepo = new();
    private readonly Mock<IClosureDayRepository> _closureRepo = new();
    private readonly Mock<IPaymentRepository> _paymentRepo = new();

    private MatchService CreateService() => new(
        _matchRepo.Object, _memberRepo.Object, _courtRepo.Object,
        _scheduleRepo.Object, _closureRepo.Object, _paymentRepo.Object);

    private static Member CreateOrganizer() => new()
    {
        Id = 1, Matricule = "G0001", FirstName = "Jean", LastName = "D",
        Email = "j@t.com", MemberType = MemberType.Global
    };

    // ═══════════════════════════════════════
    // Règle 1 — Privé incomplet → Public + Pénalité (CF-RV-025)
    // ═══════════════════════════════════════

    [Fact]
    public async Task ProcessDayBefore_PrivateIncomplete_BecomesPublicAndOrganizerBlocked()
    {
        var organizer = CreateOrganizer();
        var match = new Match
        {
            Id = 1, CourtId = 1, OrganizerId = 1, Organizer = organizer,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            EndsAt = DateTime.Today.AddDays(1).AddHours(11).AddMinutes(30),
            MatchType = MatchType.Private, Status = MatchStatus.Scheduled,
            Players = [new MatchPlayer { Id = 1, MatchId = 1, MemberId = 1, Member = organizer }]
        };

        _matchRepo.Setup(r => r.GetMatchesBecomingPublicAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Match> { match });
        _matchRepo.Setup(r => r.GetMatchesWithUnpaidPlayersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Match>());
        _matchRepo.Setup(r => r.UpdateAsync(It.IsAny<Match>())).ReturnsAsync((Match m) => m);
        _memberRepo.Setup(r => r.UpdateAsync(It.IsAny<Member>())).ReturnsAsync((Member m) => m);

        var service = CreateService();
        await service.ProcessDayBeforeAsync();

        Assert.Equal(MatchType.Public, match.MatchType);
        Assert.True(organizer.ReservationBlocked);
        Assert.NotNull(organizer.BlockedUntil);
        _matchRepo.Verify(r => r.UpdateAsync(match), Times.Once);
        _memberRepo.Verify(r => r.UpdateAsync(organizer), Times.Once);
    }

    // ═══════════════════════════════════════
    // Règle 2 — Retrait joueur impayé (CF-RV-026)
    // ═══════════════════════════════════════

    [Fact]
    public async Task ProcessDayBefore_UnpaidPlayer_IsRemoved()
    {
        var organizer = CreateOrganizer();
        var unpaidPlayer = new Member
        {
            Id = 2, Matricule = "G0002", FirstName = "P", LastName = "U",
            Email = "p@t.com", MemberType = MemberType.Global
        };

        var unpaidMatchPlayer = new MatchPlayer
        {
            Id = 2, MatchId = 1, MemberId = 2, Member = unpaidPlayer,
            Payment = new Payment
            {
                Id = 2, MatchPlayerId = 2, MatchId = 1, MemberId = 2,
                Amount = 15, Status = PaymentStatus.Pending, CreatedAt = DateTime.Now
            }
        };

        var match = new Match
        {
            Id = 1, CourtId = 1, OrganizerId = 1, Organizer = organizer,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            EndsAt = DateTime.Today.AddDays(1).AddHours(11).AddMinutes(30),
            MatchType = MatchType.Public, Status = MatchStatus.Full,
            Players = new List<MatchPlayer>
            {
                new() { Id = 1, MatchId = 1, MemberId = 1, Member = organizer,
                    Payment = new Payment { Id = 1, Status = PaymentStatus.Paid, Amount = 15, MatchPlayerId = 1, MatchId = 1, MemberId = 1, CreatedAt = DateTime.Now } },
                unpaidMatchPlayer,
                new() { Id = 3, MatchId = 1, MemberId = 3,
                    Member = new Member { Id = 3, Matricule = "G0003", FirstName = "A", LastName = "B", Email = "a@t.com", MemberType = MemberType.Global },
                    Payment = new Payment { Id = 3, Status = PaymentStatus.Paid, Amount = 15, MatchPlayerId = 3, MatchId = 1, MemberId = 3, CreatedAt = DateTime.Now } },
                new() { Id = 4, MatchId = 1, MemberId = 4,
                    Member = new Member { Id = 4, Matricule = "G0004", FirstName = "C", LastName = "D", Email = "c@t.com", MemberType = MemberType.Global },
                    Payment = new Payment { Id = 4, Status = PaymentStatus.Paid, Amount = 15, MatchPlayerId = 4, MatchId = 1, MemberId = 4, CreatedAt = DateTime.Now } }
            }
        };

        _matchRepo.Setup(r => r.GetMatchesBecomingPublicAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Match>());
        _matchRepo.Setup(r => r.GetMatchesWithUnpaidPlayersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Match> { match });
        _matchRepo.Setup(r => r.UpdateAsync(It.IsAny<Match>())).ReturnsAsync((Match m) => m);

        var service = CreateService();
        await service.ProcessDayBeforeAsync();

        // CF-RC-007: Player removed, status goes back to Scheduled + Public
        Assert.Equal(3, match.Players.Count);
        Assert.DoesNotContain(match.Players, p => p.MemberId == 2);
        Assert.Equal(MatchStatus.Scheduled, match.Status);
        Assert.Equal(MatchType.Public, match.MatchType);
    }

    // ═══════════════════════════════════════
    // Match complet et payé — Aucun changement
    // ═══════════════════════════════════════

    [Fact]
    public async Task ProcessDayBefore_FullAndPaid_NoChanges()
    {
        _matchRepo.Setup(r => r.GetMatchesBecomingPublicAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Match>());
        _matchRepo.Setup(r => r.GetMatchesWithUnpaidPlayersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Match>());

        var service = CreateService();
        await service.ProcessDayBeforeAsync();

        _matchRepo.Verify(r => r.UpdateAsync(It.IsAny<Match>()), Times.Never);
        _memberRepo.Verify(r => r.UpdateAsync(It.IsAny<Member>()), Times.Never);
    }
}
