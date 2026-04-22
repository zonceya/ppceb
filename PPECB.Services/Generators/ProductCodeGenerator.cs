using Microsoft.EntityFrameworkCore;
using PPECB.Data;
using PPECB.Domain.Entities;

namespace PPECB.Services.Generators;

public interface IProductCodeGenerator
{
    Task<string> GenerateNextCodeAsync(string userId);
}

public class ProductCodeGenerator : IProductCodeGenerator
{
    private readonly ApplicationDbContext _context;
    private readonly string _currentYearMonth;

    public ProductCodeGenerator(ApplicationDbContext context)
    {
        _context = context;
        _currentYearMonth = DateTime.Now.ToString("yyyyMM");
    }

    public async Task<string> GenerateNextCodeAsync(string userId)
    {
        var prefix = $"{_currentYearMonth}-";

        var lastProduct = await _context.Products
            .Where(p => p.UserId == userId && p.ProductCode.StartsWith(prefix))
            .OrderByDescending(p => p.ProductCode)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (lastProduct != null)
        {
            var lastNumberStr = lastProduct.ProductCode.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D3}";
    }
}