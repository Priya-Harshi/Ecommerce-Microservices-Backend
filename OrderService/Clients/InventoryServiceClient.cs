using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OrderService.Clients;

public class InventoryServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InventoryServiceClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> ReserveStockAsync(
        int productId,
        int quantity)
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

        var response =
            await _httpClient.PostAsync(
                $"api/Inventory/{productId}/reserve?quantity={quantity}",
                null);

        return response.IsSuccessStatusCode;
    }

    public async Task<InventoryResponse?> GetInventoryAsync(
        int productId)
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

        var response =
            await _httpClient.GetAsync($"api/Inventory/{productId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<InventoryResponse>();
    }
}

public class InventoryResponse
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public int ReservedQuantity { get; set; }

    public int AvailableQuantity { get; set; }

    public DateTime UpdatedAt { get; set; }
}
