using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OrderService.Clients;

public class ProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductServiceClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ProductResponse?> GetProductAsync(int productId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/Product/{productId}");

        // Get JWT from the incoming OrderService request
        var authorization =
            _httpContextAccessor.HttpContext?
                .Request.Headers.Authorization.ToString();

        // Forward JWT to ProductService
        if (!string.IsNullOrWhiteSpace(authorization))
        {
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                authorization);
        }

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody =
                await response.Content
                    .ReadAsStringAsync();

            throw new InvalidOperationException(
                $"Failed to fetch product {productId}. Status: {(int)response.StatusCode}. {errorBody}");
        }

        return await response.Content
            .ReadFromJsonAsync<ProductResponse>();
    }

    public async Task<bool> DecreaseStockAsync(
        int productId,
        int quantity)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/Product/{productId}/decrease-stock?quantity={quantity}");

        var authorization =
            _httpContextAccessor.HttpContext?
                .Request.Headers.Authorization.ToString();

        if (!string.IsNullOrWhiteSpace(authorization))
        {
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                authorization);
        }

        var response = await _httpClient.SendAsync(request);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetStockQuantityAsync(
        int productId,
        int quantity)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/Product/{productId}/stock?quantity={quantity}");

        var authorization =
            _httpContextAccessor.HttpContext?
                .Request.Headers.Authorization.ToString();

        if (!string.IsNullOrWhiteSpace(authorization))
        {
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                authorization);
        }

        var response = await _httpClient.SendAsync(request);

        return response.IsSuccessStatusCode;
    }
}

public class ProductResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }
}