namespace InventoryService.DTOs;

public class InventoryDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public int ReservedQuantity { get; set; }

    public int AvailableQuantity =>
        Quantity;

    public DateTime UpdatedAt { get; set; }
}