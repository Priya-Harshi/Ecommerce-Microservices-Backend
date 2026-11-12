using Microsoft.AspNetCore.Http;

namespace ProductService.Services;

public interface IBulkProductService
{
    Task<BulkUploadResult> UploadAsync(IFormFile file);
}

public class BulkUploadResult
{
    public int TotalRows { get; set; }

    public int SuccessfulRows { get; set; }

    public int FailedRows { get; set; }

    public List<string> Errors { get; set; } = new();
}