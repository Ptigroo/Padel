using System.Net.Http.Json;
using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public class PaymentService(HttpClient httpClient) : IPaymentService
{
    public async Task<List<PaymentDto>> GetByMemberAsync(string matricule)
    {
        return await httpClient.GetFromJsonAsync<List<PaymentDto>>($"api/payments/member/{matricule}") ?? [];
    }

    public async Task<List<PaymentDto>> GetByMatchAsync(int matchId)
    {
        return await httpClient.GetFromJsonAsync<List<PaymentDto>>($"api/payments/match/{matchId}") ?? [];
    }

    public async Task<decimal> GetBalanceAsync(string matricule)
    {
        var result = await httpClient.GetFromJsonAsync<BalanceResponse>($"api/payments/balance/{matricule}");
        return result?.Balance ?? 0;
    }

    public async Task<PaymentDto?> ProcessPaymentAsync(int paymentId)
    {
        var dto = new ProcessPaymentDto { PaymentId = paymentId };
        var response = await httpClient.PostAsJsonAsync("api/payments/pay", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PaymentDto>();
        return null;
    }

    private class BalanceResponse
    {
        public string Matricule { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
