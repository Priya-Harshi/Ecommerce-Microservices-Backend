using InventoryService.Models;

namespace InventoryService.Repositories;

public interface IInventoryRepository
{
    Task<Inventory?> GetByProductIdAsync(int productId);

    Task<Inventory> AddAsync(Inventory inventory);

    Task<Inventory> UpdateAsync(Inventory inventory);
}