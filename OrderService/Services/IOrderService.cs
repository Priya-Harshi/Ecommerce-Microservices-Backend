using OrderService.DTOs;

namespace OrderService.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(
        CreateOrderDto dto);

    Task<OrderDto?> GetByIdAsync(int id);

    Task<List<OrderDto>> GetByUserIdAsync(int userId);

    Task<bool> CheckoutAsync(int orderId);
}