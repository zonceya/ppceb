using Microsoft.EntityFrameworkCore;
using PPECB.Data;
using PPECB.Domain.Entities;
using PPECB.Services.Generators;
using PPECB.Services.Interfaces;

namespace PPECB.Services.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IProductCodeGenerator _codeGenerator;

    public ProductService(ApplicationDbContext context, IProductCodeGenerator codeGenerator)
    {
        _context = context;
        _codeGenerator = codeGenerator;
    }

    public async Task<(List<Product> Products, int TotalCount)> GetUserProductsPagedAsync(
    string userId, int page, int pageSize = 10, int? categoryId = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.UserId == userId)
            .AsNoTracking();  // Add this - improves read performance

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderByDescending(p => p.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<Product?> GetProductByIdAsync(int id, string userId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
    }

    public async Task<bool> CategoryBelongsToUserAsync(int categoryId, string userId)
    {
        return await _context.Categories
            .AnyAsync(c => c.Id == categoryId && c.UserId == userId && c.IsActive);
    }

    public async Task<(bool Success, string ErrorMessage, Product? Product)> CreateProductAsync(
        Product product, string userId)
    {
        if (!await CategoryBelongsToUserAsync(product.CategoryId, userId))
        {
            return (false, "Invalid category selected", null);
        }

        product.ProductCode = await _codeGenerator.GenerateNextCodeAsync(userId);
        product.UserId = userId;
        product.CreatedDate = DateTime.UtcNow;
        product.CreatedBy = userId;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return (true, string.Empty, product);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdateProductAsync(Product product, string userId)
    {
        var existing = await GetProductByIdAsync(product.Id, userId);
        if (existing == null)
        {
            return (false, "Product not found");
        }

        if (existing.CategoryId != product.CategoryId)
        {
            if (!await CategoryBelongsToUserAsync(product.CategoryId, userId))
            {
                return (false, "Invalid category selected");
            }
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.CategoryId = product.CategoryId;
        existing.UpdatedDate = DateTime.UtcNow;
        existing.UpdatedBy = userId;

        if (!string.IsNullOrEmpty(product.ImagePath))
        {
            existing.ImagePath = product.ImagePath;
        }

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> DeleteProductAsync(int id, string userId)
    {
        var product = await GetProductByIdAsync(id, userId);
        if (product == null)
        {
            return (false, "Product not found");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }
}