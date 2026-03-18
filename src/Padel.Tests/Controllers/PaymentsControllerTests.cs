using Microsoft.AspNetCore.Mvc;
using Moq;
using Padel.Api.Controllers;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> _paymentService = new();

    private PaymentsController CreateController() => new(_paymentService.Object);

    [Fact]
    public async Task GetByMember_Existing_ReturnsOk()
    {
        _paymentService.Setup(s => s.GetByMemberAsync("G0001"))
            .ReturnsAsync(new List<PaymentDto>());

        var controller = CreateController();
        var result = await controller.GetByMember("G0001");

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByMember_NotFound_Returns404()
    {
        _paymentService.Setup(s => s.GetByMemberAsync("XXXXX"))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        var controller = CreateController();
        var result = await controller.GetByMember("XXXXX");

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Pay_Nominal_ReturnsOk()
    {
        var paymentDto = new PaymentDto
        {
            Id = 1, MatchPlayerId = 1, MatchId = 1, MemberId = 1,
            Matricule = "G0001", Amount = 15, Status = "Paid",
            CreatedAt = DateTime.Now, PaidAt = DateTime.Now,
            CourtName = "T1", SiteName = "S1", ScheduledAt = DateTime.Today.AddDays(1)
        };
        _paymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDto>()))
            .ReturnsAsync(paymentDto);

        var controller = CreateController();
        var result = await controller.Pay(new ProcessPaymentDto { PaymentId = 1 });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Pay_AlreadyPaid_Returns400()
    {
        _paymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDto>()))
            .ThrowsAsync(new InvalidOperationException("Already paid"));

        var controller = CreateController();
        var result = await controller.Pay(new ProcessPaymentDto { PaymentId = 1 });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetBalance_ReturnsOk()
    {
        _paymentService.Setup(s => s.GetBalanceAsync("G0001")).ReturnsAsync(30m);

        var controller = CreateController();
        var result = await controller.GetBalance("G0001");

        Assert.IsType<OkObjectResult>(result.Result);
    }
}
