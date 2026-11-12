using Microsoft.Extensions.Caching.Memory;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMemoryCache _cache;

    public ProductService(
        IProductRepository productRepository,
        IMemoryCache cache)
    {
        _productRepository = productRepository;
        _cache = cache;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        if (_cache.TryGetValue(
            "products",
            out IEnumerable<ProductDto>? cachedProducts))
        {
            return cachedProducts!;
        }

        var products = await _productRepository.GetAllAsync();

        var result = products.Select(MapToDto).ToList();

        _cache.Set(
            "products",
            result,
            TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        if (dto.Price < 0)
        {
            throw new ArgumentException(
                "Product price cannot be negative.");
        }

        if (dto.StockQuantity < 0)
        {
            throw new ArgumentException(
                "Stock quantity cannot be negative.");
        }

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            IsActive = dto.IsActive,
            ImageUrl = dto.ImageUrl
        };

        var createdProduct =
            await _productRepository.AddAsync(product);

        _cache.Remove("products");

        return MapToDto(createdProduct);
    }

    public async Task<ProductDto?> UpdateAsync(
        int id,
        UpdateProductDto dto)
    {
        var existingProduct =
            await _productRepository.GetByIdAsync(id);

        if (existingProduct == null)
        {
            return null;
        }

        if (dto.Price < 0)
        {
            throw new ArgumentException(
                "Product price cannot be negative.");
        }

        if (dto.StockQuantity < 0)
        {
            throw new ArgumentException(
                "Stock quantity cannot be negative.");
        }

        existingProduct.Name = dto.Name;
        existingProduct.Description = dto.Description;
        existingProduct.Price = dto.Price;
        existingProduct.StockQuantity = dto.StockQuantity;
        existingProduct.IsActive = dto.IsActive;
        existingProduct.ImageUrl = dto.ImageUrl;

        var updatedProduct =
            await _productRepository.UpdateAsync(existingProduct);

        _cache.Remove("products");

        return MapToDto(updatedProduct);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result =
            await _productRepository.DeleteAsync(id);

        _cache.Remove("products");

        return result;
    }

    public async Task<bool> DecreaseStockAsync(int id, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var existingProduct =
            await _productRepository.GetByIdAsync(id);

        if (existingProduct == null)
        {
            return false;
        }

        if (existingProduct.StockQuantity < quantity)
        {
            return false;
        }

        existingProduct.StockQuantity -= quantity;

        if (existingProduct.StockQuantity == 0)
        {
            await _productRepository.DeleteAsync(id);
        }
        else
        {
            await _productRepository.UpdateAsync(existingProduct);
        }

        _cache.Remove("products");

        return true;
    }

    public async Task<bool> SetStockQuantityAsync(
        int id,
        int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException(
                "Stock quantity cannot be negative.");
        }

        var existingProduct =
            await _productRepository.GetByIdAsync(id);

        if (existingProduct == null)
        {
            return false;
        }

        if (quantity == 0)
        {
            await _productRepository.DeleteAsync(id);
        }
        else
        {
            existingProduct.StockQuantity = quantity;
            await _productRepository.UpdateAsync(existingProduct);
        }

        _cache.Remove("products");

        return true;
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            ImageUrl = product.ImageUrl
        };
    }
}