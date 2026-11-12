using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;
using Microsoft.AspNetCore.Authorization;

namespace ProductService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IBulkProductService _bulkProductService;

    public ProductController(
        IProductService productService,
        IBulkProductService bulkProductService)
    {
        _productService = productService;
        _bulkProductService = bulkProductService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();

        return Ok(products);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound("Product not found.");
        }

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateProductDto dto)
    {
        try
        {
            var product =
                await _productService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateProductDto dto)
    {
        try
        {
            var product =
                await _productService.UpdateAsync(id, dto);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("{id}/decrease-stock")]
    public async Task<IActionResult> DecreaseStock(
        int id,
        [FromQuery] int quantity)
    {
        try
        {
            var updated =
                await _productService
                    .DecreaseStockAsync(id, quantity);

            if (!updated)
            {
                return BadRequest(
                    "Insufficient stock or product not found.");
            }

            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPut("{id}/stock")]
    public async Task<IActionResult> SetStock(
        int id,
        [FromQuery] int quantity)
    {
        try
        {
            var updated =
                await _productService
                    .SetStockQuantityAsync(id, quantity);

            if (!updated)
            {
                return NotFound("Product not found.");
            }

            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _productService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound("Product not found.");
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("bulk-upload")]
    public async Task<IActionResult> BulkUpload(
        IFormFile file)
    {
        var result =
            await _bulkProductService.UploadAsync(file);

        if (result.TotalRows == 0 &&
            result.Errors.Count > 0)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}