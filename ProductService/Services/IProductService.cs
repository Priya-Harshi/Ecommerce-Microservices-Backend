using ProductService.DTOs;

namespace ProductService.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();

    Task<ProductDto?> GetByIdAsync(int id);

    Task<ProductDto> CreateAsync(CreateProductDto dto);

    Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);

    Task<bool> DeleteAsync(int id);

    Task<bool> DecreaseStockAsync(int id, int quantity);

    Task<bool> SetStockQuantityAsync(int id, int quantity);
}