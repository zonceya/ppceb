using PPECB.Domain.Entities;

namespace PPECB.Services.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetUserCategoriesAsync(string userId);
    Task<Category?> GetCategoryByIdAsync(int id, string userId);
    Task<(bool Success, string ErrorMessage)> CreateCategoryAsync(Category category, string userId);
    Task<(bool Success, string ErrorMessage)> UpdateCategoryAsync(Category category, string userId);
}