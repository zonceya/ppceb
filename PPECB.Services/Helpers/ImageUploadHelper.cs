using Microsoft.AspNetCore.Http;

namespace PPECB.Services.Helpers;

public interface IImageUploadHelper
{
    Task<string?> SaveImageAsync(IFormFile image, string userId, string webRootPath);
    void DeleteImage(string imagePath, string webRootPath);
}

public class ImageUploadHelper : IImageUploadHelper
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    private const int MaxFileSize = 2 * 1024 * 1024;

    public async Task<string?> SaveImageAsync(IFormFile image, string userId, string webRootPath)
    {
        if (image == null || image.Length == 0) return null;

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Only JPG, JPEG, PNG, and GIF images are allowed");
        }

        if (image.Length > MaxFileSize)
        {
            throw new ArgumentException("Image size must be less than 2MB");
        }

        var userDir = Path.Combine(webRootPath, "images", "products", userId);
        if (!Directory.Exists(userDir))
        {
            Directory.CreateDirectory(userDir);
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(userDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return $"/images/products/{userId}/{fileName}";
    }

    public void DeleteImage(string imagePath, string webRootPath)
    {
        if (string.IsNullOrEmpty(imagePath)) return;

        var fullPath = Path.Combine(webRootPath, imagePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}