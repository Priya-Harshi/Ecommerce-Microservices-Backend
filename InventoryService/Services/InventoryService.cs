using InventoryService.DTOs;
using InventoryService.Models;
using InventoryService.Repositories;

namespace InventoryService.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;

    public InventoryService(
        IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryDto?> GetByProductIdAsync(
        int productId)
    {
        var inventory =
            await _repository.GetByProductIdAsync(productId);

        if (inventory != null)
        {
            await NormalizeLegacyReservedStockAsync(inventory);
        }

        return inventory == null
            ? null
            : MapToDto(inventory);
    }

    public async Task<InventoryDto> CreateAsync(
        CreateInventoryDto dto)
    {
        if (dto.Quantity < 0)
        {
            throw new ArgumentException(
                "Quantity cannot be negative.");
        }

        var existing =
            await _repository.GetByProductIdAsync(
                dto.ProductId);

        if (existing != null)
        {
            throw new InvalidOperationException(
                "Inventory already exists for this product.");
        }

        var inventory = new Inventory
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            ReservedQuantity = 0,
            UpdatedAt = DateTime.UtcNow
        };

        var created =
            await _repository.AddAsync(inventory);

        return MapToDto(created);
    }

    public async Task<bool> ReserveStockAsync(
        int productId,
        int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var inventory =
            await _repository.GetByProductIdAsync(
                productId);

        if (inventory == null)
        {
            return false;
        }

        await NormalizeLegacyReservedStockAsync(inventory);

        if (inventory.Quantity < quantity)
        {
            return false;
        }

        inventory.Quantity -= quantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(inventory);

        return true;
    }

    private async Task NormalizeLegacyReservedStockAsync(
        Inventory inventory)
    {
        if (inventory.ReservedQuantity <= 0)
        {
            return;
        }

        inventory.Quantity =
            Math.Max(
                inventory.Quantity - inventory.ReservedQuantity,
                0);

        inventory.ReservedQuantity = 0;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(inventory);
    }

    private static InventoryDto MapToDto(
        Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            Quantity = inventory.Quantity,
            ReservedQuantity = inventory.ReservedQuantity,
            UpdatedAt = inventory.UpdatedAt
        };
    }
}