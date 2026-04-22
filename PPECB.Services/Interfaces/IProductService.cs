using PPECB.Domain.Entities;

namespace PPECB.Services.Interfaces;

public interface IProductService
{
    Task<(List<Product> Products, int TotalCount)> GetUserProductsPagedAsync(
        string userId, int page, int pageSize = 10, int? categoryId = null);
    Task<Product?> GetProductByIdAsync(int id, string userId);
    Task<(bool Success, string ErrorMessage, Product? Product)> CreateProductAsync(
        Product product, string userId);
    Task<(bool Success, string ErrorMessage)> UpdateProductAsync(Product product, string userId);
    Task<(bool Success, string ErrorMessage)> DeleteProductAsync(int id, string userId);
    Task<bool> CategoryBelongsToUserAsync(int categoryId, string userId);
}