using Moq;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Domain.Entities;
using Match = Padel.Domain.Entities.Match;

namespace Padel.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IMemberRepository> _memberRepo = new();

    private PaymentService CreateService() => new(_paymentRepo.Object, _memberRepo.Object);

    private static Payment CreatePendingPayment(int id = 1, int memberId = 1) => new()
    {
        Id = id,
        MatchPlayerId = 1,
        MatchId = 1,
        MemberId = memberId,
        Amount = 15m,
        Status = PaymentStatus.Pending,
        CreatedAt = DateTime.Now,
        Member = new Member
        {
            Id = memberId, Matricule = "G0001", FirstName = "Jean",
            LastName = "Dupont", Email = "j@t.com", MemberType = MemberType.Global
        },
        Match = new Match
        {
            Id = 1, CourtId = 1, OrganizerId = memberId,
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
            EndsAt = DateTime.Today.AddDays(1).AddHours(11).AddMinutes(30),
            Court = new Court { Id = 1, Name = "T1", SiteId = 1, Site = new Site { Id = 1, Name = "S1", Address = "A1" } }
        }
    };

    // ═══════════════════════════════════════
    // ProcessPaymentAsync — Cas nominal
    // ═══════════════════════════════════════

    [Fact]
    public async Task ProcessPaymentAsync_Nominal_SetsStatusToPaid()
    {
        var payment = CreatePendingPayment();
        _paymentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(payment);
        _paymentRepo.Setup(r => r.GetUnpaidPublicMatchDebtAsync(1)).ReturnsAsync(0m);
        _paymentRepo.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        var service = CreateService();
        var result = await service.ProcessPaymentAsync(new ProcessPaymentDto { PaymentId = 1 });

        Assert.Equal("Paid", result.Status);
        Assert.NotNull(result.PaidAt);
        Assert.Equal(15m, result.Amount);
    }

    // ═══════════════════════════════════════
    // ProcessPaymentAsync — Double paiement (CF-RV-023)
    // ═══════════════════════════════════════

    [Fact]
    public async Task ProcessPaymentAsync_AlreadyPaid_ThrowsException()
    {
        var payment = CreatePendingPayment();
        payment.Status = PaymentStatus.Paid;
        payment.PaidAt = DateTime.Now;
        _paymentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(payment);

        var service = CreateService();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ProcessPaymentAsync(new ProcessPaymentDto { PaymentId = 1 }));
        Assert.Contains("already", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // ProcessPaymentAsync — Report solde (CF-RC-006)
    // ═══════════════════════════════════════

    [Fact]
    public async Task ProcessPaymentAsync_WithPublicDebt_IncreasesAmount()
    {
        var payment = CreatePendingPayment();
        _paymentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(payment);
        _paymentRepo.Setup(r => r.GetUnpaidPublicMatchDebtAsync(1)).ReturnsAsync(30m); // 2 empty slots
        _paymentRepo.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        var service = CreateService();
        var result = await service.ProcessPaymentAsync(new ProcessPaymentDto { PaymentId = 1 });

        Assert.Equal(45m, result.Amount); // 15 + 30
        Assert.Equal("Paid", result.Status);
    }

    // ═══════════════════════════════════════
    // ProcessPaymentAsync — Paiement introuvable (CF-RV-024)
    // ═══════════════════════════════════════

    [Fact]
    public async Task ProcessPaymentAsync_NotFound_ThrowsException()
    {
        _paymentRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Payment?)null);

        var service = CreateService();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ProcessPaymentAsync(new ProcessPaymentDto { PaymentId = 999 }));
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════
    // GetBalanceAsync — Calcul solde
    // ═══════════════════════════════════════

    [Fact]
    public async Task GetBalanceAsync_ReturnsCorrectBalance()
    {
        var member = new Member
        {
            Id = 1, Matricule = "G0001", FirstName = "J", LastName = "D",
            Email = "j@t.com", MemberType = MemberType.Global
        };
        _memberRepo.Setup(r => r.GetByMatriculeAsync("G0001")).ReturnsAsync(member);
        _paymentRepo.Setup(r => r.GetUnpaidBalanceAsync(1)).ReturnsAsync(30m);

        var service = CreateService();
        var balance = await service.GetBalanceAsync("G0001");

        Assert.Equal(30m, balance);
    }
}
