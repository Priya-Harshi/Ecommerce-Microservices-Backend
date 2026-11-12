using InventoryService.DTOs;

namespace InventoryService.Services;

public interface IInventoryService
{
    Task<InventoryDto?> GetByProductIdAsync(int productId);

    Task<InventoryDto> CreateAsync(
        CreateInventoryDto dto);

    Task<bool> ReserveStockAsync(
        int productId,
        int quantity);
}