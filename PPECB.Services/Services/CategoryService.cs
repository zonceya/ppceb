using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PPECB.Data;
using PPECB.Domain.Entities;
using PPECB.Services.Interfaces;
using PPECB.Services.Validators;

namespace PPECB.Services.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ICategoryCodeValidator _codeValidator;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ApplicationDbContext context,
        ICategoryCodeValidator codeValidator,
        ILogger<CategoryService> logger)
    {
        _context = context;
        _codeValidator = codeValidator;
        _logger = logger;
    }

    public async Task<List<Category>> GetUserCategoriesAsync(string userId)
    {
        _logger.LogInformation($"GetUserCategoriesAsync for userId: {userId}");
        var categories = await _context.Categories
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        _logger.LogInformation($"Found {categories.Count} categories");
        return categories;
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, string userId)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<(bool Success, string ErrorMessage)> CreateCategoryAsync(Category category, string userId)
    {
        _logger.LogInformation($"CreateCategoryAsync called for code: {category.CategoryCode}");

        // Validate format
        if (!_codeValidator.IsValidFormat(category.CategoryCode))
        {
            _logger.LogWarning($"Invalid format for code: {category.CategoryCode}");
            return (false, $"Category code must be {_codeValidator.GetFormatDescription()}");
        }

        category.CategoryCode = category.CategoryCode.ToUpper();

        // Check uniqueness
        var exists = await _context.Categories
            .AnyAsync(c => c.CategoryCode == category.CategoryCode && c.UserId == userId);

        if (exists)
        {
            _logger.LogWarning($"Duplicate code: {category.CategoryCode} for user {userId}");
            return (false, "Category code already exists for your account");
        }

        category.UserId = userId;
        category.CreatedDate = DateTime.UtcNow;
        category.CreatedBy = userId;

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Category created successfully with Id: {category.Id}");
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdateCategoryAsync(Category category, string userId)
    {
        var existing = await GetCategoryByIdAsync(category.Id, userId);
        if (existing == null)
        {
            return (false, "Category not found");
        }

        if (existing.CategoryCode != category.CategoryCode)
        {
            if (!_codeValidator.IsValidFormat(category.CategoryCode))
            {
                return (false, $"Category code must be {_codeValidator.GetFormatDescription()}");
            }

            category.CategoryCode = category.CategoryCode.ToUpper();

            var exists = await _context.Categories
                .AnyAsync(c => c.CategoryCode == category.CategoryCode && c.UserId == userId && c.Id != category.Id);

            if (exists)
            {
                return (false, "Category code already exists for your account");
            }

            existing.CategoryCode = category.CategoryCode;
        }

        existing.Name = category.Name;
        existing.IsActive = category.IsActive;
        existing.UpdatedDate = DateTime.UtcNow;
        existing.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }
}