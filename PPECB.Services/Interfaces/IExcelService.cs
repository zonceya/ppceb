using PPECB.Domain.Entities;

namespace PPECB.Services.Interfaces;

public interface IExcelService
{
    Task<(int SuccessCount, List<string> Errors)> ImportProductsFromExcelAsync(
        Stream excelStream, string userId);

    Task<byte[]> ExportProductsToExcelAsync(string userId);

    byte[] GenerateSampleExcelTemplate();
}