using InventoryService.Data;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByProductIdAsync(
        int productId)
    {
        return await _context.Inventories
            .FirstOrDefaultAsync(i =>
                i.ProductId == productId);
    }

    public async Task<Inventory> AddAsync(
        Inventory inventory)
    {
        _context.Inventories.Add(inventory);

        await _context.SaveChangesAsync();

        return inventory;
    }

    public async Task<Inventory> UpdateAsync(
        Inventory inventory)
    {
        inventory.UpdatedAt = DateTime.UtcNow;

        _context.Inventories.Update(inventory);

        await _context.SaveChangesAsync();

        return inventory;
    }
}