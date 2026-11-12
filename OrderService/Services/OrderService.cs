using OrderService.Clients;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;
using Microsoft.AspNetCore.SignalR;
using OrderService.Hubs;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ProductServiceClient _productClient;
    private readonly InventoryServiceClient _inventoryClient;
    private readonly PaymentServiceClient _paymentClient;
    private readonly IHubContext<OrderHub> _hubContext;
    public OrderService(
    IOrderRepository repository,
    ProductServiceClient productClient,
    InventoryServiceClient inventoryClient,
    PaymentServiceClient paymentClient,
    IHubContext<OrderHub> hubContext)
    {
        _repository = repository;
        _productClient = productClient;
        _inventoryClient = inventoryClient;
        _paymentClient = paymentClient;
        _hubContext = hubContext;
    }

    public async Task<OrderDto> CreateOrderAsync(
        CreateOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new ArgumentException(
                "Order must contain at least one item.");
        }

        var order = new Order
        {
            UserId = dto.UserId
        };

        decimal totalAmount = 0;

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }

            // 1. Ask ProductService for product information
            var product =
                await _productClient
                    .GetProductAsync(item.ProductId);

            if (product == null)
            {
                throw new ArgumentException(
                    $"Product {item.ProductId} not found.");
            }

            // 2. Ask InventoryService to reserve stock
            var stockReserved =
                await _inventoryClient
                    .ReserveStockAsync(
                        item.ProductId,
                        item.Quantity);

            if (!stockReserved)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for product {item.ProductId}.");
            }

            var inventoryAfterReserve =
                await _inventoryClient
                    .GetInventoryAsync(item.ProductId);

            if (inventoryAfterReserve == null)
            {
                throw new InvalidOperationException(
                    $"Failed to read inventory for product {item.ProductId}.");
            }

            var productStockUpdated =
                await _productClient
                    .SetStockQuantityAsync(
                        item.ProductId,
                        inventoryAfterReserve.Quantity);

            if (!productStockUpdated)
            {
                productStockUpdated =
                    await _productClient
                        .DecreaseStockAsync(
                            item.ProductId,
                            item.Quantity);
            }

            // 3. Calculate using price returned by ProductService
            var totalPrice =
                product.Price * item.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                TotalPrice = totalPrice
            };

            order.OrderItems.Add(orderItem);

            totalAmount += totalPrice;
        }

        order.TotalAmount = totalAmount;
        order.Status = "Pending";

        var createdOrder =
            await _repository.CreateAsync(order);

        return MapToDto(createdOrder);
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order =
            await _repository.GetByIdAsync(id);

        return order == null
            ? null
            : MapToDto(order);
    }

    public async Task<List<OrderDto>> GetByUserIdAsync(int userId)
    {
        var orders =
            await _repository.GetByUserIdAsync(userId);

        return orders
            .Select(MapToDto)
            .ToList();
    }

    public async Task<bool> CheckoutAsync(int orderId)
    {
        var order =
            await _repository.GetByIdAsync(orderId);

        if (order == null)
        {
            return false;
        }

        if (order.Status != "Pending")
        {
            return false;
        }

        // Process payment through PaymentService
        var payment =
            await _paymentClient.ProcessPaymentAsync(
                order.Id,
                order.TotalAmount);

        if (payment == null ||
            payment.Status != "Paid")
        {
            return false;
        }

        // Payment successful
        order.Status = "Paid";

        await _repository.UpdateAsync(order);

        await _hubContext.Clients
            .Group($"order-{order.Id}")
            .SendAsync(
                "OrderStatusChanged",
                new
                {
                    orderId = order.Id,
                    status = order.Status
                });

        return true;
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,

            Items = order.OrderItems
                .Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                })
                .ToList()
        };
    }
}