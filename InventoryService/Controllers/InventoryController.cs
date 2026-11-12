using InventoryService.DTOs;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InventoryService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(
        IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [Authorize]
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetByProductId(
        int productId)
    {
        var inventory =
            await _inventoryService
                .GetByProductIdAsync(productId);

        if (inventory == null)
        {
            return NotFound(
                "Inventory not found.");
        }

        return Ok(inventory);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateInventoryDto dto)
    {
        try
        {
            var inventory =
                await _inventoryService
                    .CreateAsync(dto);

            return Ok(inventory);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("{productId}/reserve")]
    public async Task<IActionResult> ReserveStock(
        int productId,
        [FromQuery] int quantity)
    {
        try
        {
            var reserved =
                await _inventoryService
                    .ReserveStockAsync(
                        productId,
                        quantity);

            if (!reserved)
            {
                return BadRequest(
                    "Insufficient stock.");
            }

            return Ok(new
            {
                message = "Stock reserved successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}