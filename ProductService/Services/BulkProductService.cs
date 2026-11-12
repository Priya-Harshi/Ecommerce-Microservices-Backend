using ClosedXML.Excel;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Services;

public class BulkProductService : IBulkProductService
{
    private readonly ProductDbContext _context;

    public BulkProductService(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<BulkUploadResult> UploadAsync(IFormFile file)
    {
        var result = new BulkUploadResult();

        if (file == null || file.Length == 0)
        {
            result.Errors.Add("Excel file is required.");
            return result;
        }

        if (!Path.GetExtension(file.FileName)
                .Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            result.Errors.Add("Only .xlsx files are supported.");
            return result;
        }

        using var stream = file.OpenReadStream();

        using var workbook = new XLWorkbook(stream);

        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            result.Errors.Add("Excel worksheet not found.");
            return result;
        }

        var rows = worksheet.RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            result.TotalRows++;

            var productName = row.Cell(1).GetString().Trim();
            var description = row.Cell(2).GetString().Trim();
            var priceText = row.Cell(3).GetString().Trim();
            var stockText = row.Cell(4).GetString().Trim();
            var isActiveText = row.Cell(5).GetString().Trim();
            var imageUrl = row.Cell(6).GetString().Trim();

            if (string.IsNullOrWhiteSpace(productName))
            {
                result.FailedRows++;
                result.Errors.Add(
                    $"Row {row.RowNumber()}: Product name is required.");

                continue;
            }

            if (!decimal.TryParse(priceText, out var price))
            {
                result.FailedRows++;
                result.Errors.Add(
                    $"Row {row.RowNumber()}: Price must be a valid number.");

                continue;
            }

            if (price < 0)
            {
                result.FailedRows++;
                result.Errors.Add(
                    $"Row {row.RowNumber()}: Price cannot be negative.");

                continue;
            }

            if (!int.TryParse(stockText, out var stockQuantity))
            {
                result.FailedRows++;
                result.Errors.Add(
                    $"Row {row.RowNumber()}: StockQuantity must be a valid number.");

                continue;
            }

            if (stockQuantity < 0)
            {
                result.FailedRows++;
                result.Errors.Add(
                    $"Row {row.RowNumber()}: StockQuantity cannot be negative.");

                continue;
            }

            if (!bool.TryParse(isActiveText, out var isActive))
            {
                result.FailedRows++;
                result.Errors.Add(
                    $"Row {row.RowNumber()}: IsActive must be TRUE or FALSE.");

                continue;
            }

            var product = new Product
            {
                Name = productName,
                Description = description,
                Price = price,
                StockQuantity = stockQuantity,
                IsActive = isActive,
                ImageUrl = imageUrl
            };

            _context.Products.Add(product);

            result.SuccessfulRows++;
        }

        await _context.SaveChangesAsync();

        return result;
    }
}