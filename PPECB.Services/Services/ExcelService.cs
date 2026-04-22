using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PPECB.Data;
using PPECB.Domain.Entities;
using PPECB.Services.Generators;
using PPECB.Services.Interfaces;

namespace PPECB.Services.Services;

public class ExcelService : IExcelService
{
    private readonly ApplicationDbContext _context;
    private readonly IProductCodeGenerator _codeGenerator;

    public ExcelService(ApplicationDbContext context, IProductCodeGenerator codeGenerator)
    {
        _context = context;
        _codeGenerator = codeGenerator;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<(int SuccessCount, List<string> Errors)> ImportProductsFromExcelAsync(
        Stream excelStream, string userId)
    {
        var errors = new List<string>();
        int successCount = 0;

        using var package = new ExcelPackage(excelStream);
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        // Get user's categories for lookup
        var categories = await _context.Categories
            .Where(c => c.UserId == userId && c.IsActive)
            .ToDictionaryAsync(c => c.Name.ToLower(), c => c.Id);

        if (categories.Count == 0)
        {
            errors.Add("No categories found. Please create at least one category before importing products.");
            return (0, errors);
        }

        // Expected headers: Category, Product Name, Price, Description (optional)
        for (int row = 2; row <= rowCount; row++) // Skip header row
        {
            try
            {
                var categoryName = worksheet.Cells[row, 1].Text?.Trim();
                var productName = worksheet.Cells[row, 2].Text?.Trim();
                var priceText = worksheet.Cells[row, 3].Text?.Trim();
                var description = worksheet.Cells[row, 4].Text?.Trim();

                // Validate required fields
                if (string.IsNullOrEmpty(productName))
                {
                    errors.Add($"Row {row}: Product name is required");
                    continue;
                }

                if (string.IsNullOrEmpty(categoryName))
                {
                    errors.Add($"Row {row}: Category is required");
                    continue;
                }

                if (!categories.ContainsKey(categoryName.ToLower()))
                {
                    errors.Add($"Row {row}: Category '{categoryName}' not found. Available categories: {string.Join(", ", categories.Keys)}");
                    continue;
                }

                if (!decimal.TryParse(priceText, out decimal price) || price <= 0)
                {
                    errors.Add($"Row {row}: Invalid price '{priceText}'. Must be a positive number.");
                    continue;
                }

                // Check if product already exists with same name for this user
                var existingProduct = await _context.Products
                    .AnyAsync(p => p.UserId == userId && p.Name.ToLower() == productName.ToLower());

                if (existingProduct)
                {
                    errors.Add($"Row {row}: Product '{productName}' already exists for your account");
                    continue;
                }

                var product = new Product
                {
                    Name = productName,
                    Description = description ?? string.Empty,
                    Price = price,
                    CategoryId = categories[categoryName.ToLower()],
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId,
                    ProductCode = await _codeGenerator.GenerateNextCodeAsync(userId)
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row {row}: {ex.Message}");
            }
        }

        return (successCount, errors);
    }

    public async Task<byte[]> ExportProductsToExcelAsync(string userId)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Products");

        // Headers
        worksheet.Cells[1, 1].Value = "Category";
        worksheet.Cells[1, 2].Value = "Product Code";
        worksheet.Cells[1, 3].Value = "Product Name";
        worksheet.Cells[1, 4].Value = "Price";
        worksheet.Cells[1, 5].Value = "Description";
        worksheet.Cells[1, 6].Value = "Created Date";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 6])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        int row = 2;
        foreach (var product in products)
        {
            worksheet.Cells[row, 1].Value = product.Category?.Name ?? "N/A";
            worksheet.Cells[row, 2].Value = product.ProductCode;
            worksheet.Cells[row, 3].Value = product.Name;
            worksheet.Cells[row, 4].Value = product.Price;
            worksheet.Cells[row, 5].Value = product.Description;
            worksheet.Cells[row, 6].Value = product.CreatedDate.ToString("yyyy-MM-dd HH:mm");
            row++;
        }

        worksheet.Cells.AutoFitColumns();

        return await package.GetAsByteArrayAsync();
    }

    public byte[] GenerateSampleExcelTemplate()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("ProductTemplate");

        // Headers
        worksheet.Cells[1, 1].Value = "Category*";
        worksheet.Cells[1, 2].Value = "Product Name*";
        worksheet.Cells[1, 3].Value = "Price*";
        worksheet.Cells[1, 4].Value = "Description";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 4])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Sample data
        worksheet.Cells[2, 1].Value = "Electronics";
        worksheet.Cells[2, 2].Value = "Sample Laptop";
        worksheet.Cells[2, 3].Value = "999.99";
        worksheet.Cells[2, 4].Value = "High performance laptop";

        worksheet.Cells[3, 1].Value = "Electronics";
        worksheet.Cells[3, 2].Value = "Sample Mouse";
        worksheet.Cells[3, 3].Value = "25.50";
        worksheet.Cells[3, 4].Value = "Wireless mouse";

        // Add notes
        worksheet.Cells[5, 1].Value = "Instructions:";
        worksheet.Cells[6, 1].Value = "1. * Required fields";
        worksheet.Cells[7, 1].Value = "2. Category must match existing category names exactly";
        worksheet.Cells[8, 1].Value = "3. Price must be a positive number (use decimal format like 99.99)";
        worksheet.Cells[9, 1].Value = "4. Product names must be unique for your account";

        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }
}