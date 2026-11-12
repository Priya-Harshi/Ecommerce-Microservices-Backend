using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OrderService.Clients;

public class PaymentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentServiceClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaymentResponse?> ProcessPaymentAsync(
        int orderId,
        decimal amount)
    {
        var token =
            _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"]
                .FirstOrDefault();

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse(token);
        }

        var request = new
        {
            orderId,
            amount,
            paymentMethod = "Mock"
        };

        var response = await _httpClient.PostAsJsonAsync(
            "api/Payment",
            request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<PaymentResponse>();
    }
}

public class PaymentResponse
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}