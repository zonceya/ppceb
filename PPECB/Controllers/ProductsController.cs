using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PPECB.Domain.Entities;
using PPECB.Services.Interfaces;
using PPECB.Services.Helpers;

namespace PPECB.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IImageUploadHelper _imageUploadHelper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ICategoryService categoryService,
        IImageUploadHelper imageUploadHelper,
        UserManager<ApplicationUser> userManager,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _categoryService = categoryService;
        _imageUploadHelper = imageUploadHelper;
        _userManager = userManager;
        _logger = logger;
    }

    private async Task<string> GetCurrentUserId()
    {
        var user = await _userManager.GetUserAsync(User);
        _logger.LogInformation($"GetCurrentUserId: {(user == null ? "null" : user.Email)}");
        return user!.Id;
    }

    // GET: Products (with paging)
    public async Task<IActionResult> Index(int page = 1, int? categoryId = null)
    {
        var userId = await GetCurrentUserId();

        // Run sequentially to avoid DbContext concurrency issues
        var (products, totalCount) = await _productService.GetUserProductsPagedAsync(userId, page, 10, categoryId);
        var categories = await _categoryService.GetUserCategoriesAsync(userId);

        ViewBag.Categories = categories;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 10.0);
        ViewBag.SelectedCategory = categoryId;

        return View(products);
    }

    // GET: Products/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        _logger.LogInformation("=== PRODUCT CREATE GET ACTION CALLED ===");
        var userId = await GetCurrentUserId();
        _logger.LogInformation($"UserId: {userId}");

        var categories = await _categoryService.GetUserCategoriesAsync(userId);
        _logger.LogInformation($"Found {categories.Count} categories for user");

        foreach (var cat in categories)
        {
            _logger.LogInformation($"Category: Id={cat.Id}, Name={cat.Name}, Code={cat.CategoryCode}, IsActive={cat.IsActive}");
        }

        ViewBag.Categories = categories;
        return View();
    }

    // POST: Products/Create
    [HttpPost]
    public async Task<IActionResult> Create(Product product, IFormFile? image)
    {
        _logger.LogInformation("=== PRODUCT CREATE POST ACTION CALLED ===");
        _logger.LogInformation($"Product Name: '{product.Name}'");
        _logger.LogInformation($"Product Price: {product.Price}");
        _logger.LogInformation($"Product Description: '{product.Description}'");
        _logger.LogInformation($"CategoryId: {product.CategoryId}");
        _logger.LogInformation($"Image: {(image != null ? image.FileName : "null")}");
        _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid - returning to Create view");

            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key]?.Errors;
                if (errors != null && errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        _logger.LogWarning($"ModelState Error for {key}: {error.ErrorMessage}");
                    }
                }
            }

            var categories = await _categoryService.GetUserCategoriesAsync(await GetCurrentUserId());
            ViewBag.Categories = categories;
            return View(product);
        }

        // Handle image upload
        if (image != null)
        {
            try
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                _logger.LogInformation($"Saving image to: {webRootPath}");
                product.ImagePath = await _imageUploadHelper.SaveImageAsync(image, await GetCurrentUserId(), webRootPath);
                _logger.LogInformation($"Image saved to: {product.ImagePath}");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Image upload failed: {ex.Message}");
                ModelState.AddModelError("Image", ex.Message);
                var categories = await _categoryService.GetUserCategoriesAsync(await GetCurrentUserId());
                ViewBag.Categories = categories;
                return View(product);
            }
        }

        var userId = await GetCurrentUserId();
        _logger.LogInformation($"Creating product for userId: {userId}");

        var result = await _productService.CreateProductAsync(product, userId);

        _logger.LogInformation($"Create Result - Success: {result.Success}");
        _logger.LogInformation($"Create Result - ErrorMessage: {result.ErrorMessage ?? "null"}");
        _logger.LogInformation($"Create Result - Product Code: {result.Product?.ProductCode ?? "null"}");

        if (result.Success)
        {
            _logger.LogInformation($"SUCCESS! Product created with code: {result.Product?.ProductCode}");
            TempData["Success"] = $"Product '{product.Name}' created successfully! Product Code: {result.Product?.ProductCode}";
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning($"FAILED: {result.ErrorMessage}");
        ModelState.AddModelError("", result.ErrorMessage);
        var categories2 = await _categoryService.GetUserCategoriesAsync(await GetCurrentUserId());
        ViewBag.Categories = categories2;
        return View(product);
    }

    // GET: Products/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation($"=== PRODUCT EDIT GET ACTION CALLED for id: {id} ===");
        var userId = await GetCurrentUserId();

        var product = await _productService.GetProductByIdAsync(id, userId);
        if (product == null)
        {
            _logger.LogWarning($"Product {id} not found for user {userId}");
            return NotFound();
        }

        _logger.LogInformation($"Found product: Id={product.Id}, Name={product.Name}, Code={product.ProductCode}");

        var categories = await _categoryService.GetUserCategoriesAsync(userId);
        ViewBag.Categories = categories;
        return View(product);
    }

    // POST: Products/Edit/{id}
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Product product, IFormFile? image)
    {
        _logger.LogInformation($"=== PRODUCT EDIT POST ACTION CALLED for id: {id} ===");
        _logger.LogInformation($"Product Name: '{product.Name}', Price: {product.Price}, CategoryId: {product.CategoryId}");

        if (id != product.Id)
        {
            _logger.LogWarning($"ID mismatch: url id={id}, model id={product.Id}");
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid in Edit POST");
            var categories = await _categoryService.GetUserCategoriesAsync(await GetCurrentUserId());
            ViewBag.Categories = categories;
            return View(product);
        }

        // Handle image upload if new image is provided
        if (image != null)
        {
            try
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var newImagePath = await _imageUploadHelper.SaveImageAsync(image, await GetCurrentUserId(), webRootPath);
                _logger.LogInformation($"New image saved: {newImagePath}");

                // Delete old image if exists
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    _imageUploadHelper.DeleteImage(product.ImagePath, webRootPath);
                    _logger.LogInformation($"Old image deleted: {product.ImagePath}");
                }

                product.ImagePath = newImagePath;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Image upload failed: {ex.Message}");
                ModelState.AddModelError("Image", ex.Message);
                var categories = await _categoryService.GetUserCategoriesAsync(await GetCurrentUserId());
                ViewBag.Categories = categories;
                return View(product);
            }
        }

        var result = await _productService.UpdateProductAsync(product, await GetCurrentUserId());

        if (result.Success)
        {
            _logger.LogInformation($"Product {id} updated successfully");
            TempData["Success"] = $"Product '{product.Name}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning($"Product update failed: {result.ErrorMessage}");
        ModelState.AddModelError("", result.ErrorMessage);
        var categories2 = await _categoryService.GetUserCategoriesAsync(await GetCurrentUserId());
        ViewBag.Categories = categories2;
        return View(product);
    }

    // POST: Products/Delete/{id}
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation($"=== PRODUCT DELETE ACTION CALLED for id: {id} ===");

        var product = await _productService.GetProductByIdAsync(id, await GetCurrentUserId());

        if (product != null && !string.IsNullOrEmpty(product.ImagePath))
        {
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _imageUploadHelper.DeleteImage(product.ImagePath, webRootPath);
            _logger.LogInformation($"Deleted image: {product.ImagePath}");
        }

        var result = await _productService.DeleteProductAsync(id, await GetCurrentUserId());

        if (result.Success)
        {
            _logger.LogInformation($"Product {id} deleted successfully");
            TempData["Success"] = "Product deleted successfully";
        }
        else
        {
            _logger.LogWarning($"Product delete failed: {result.ErrorMessage}");
            TempData["Error"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Download sample Excel template
    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        var excelService = HttpContext.RequestServices.GetRequiredService<IExcelService>();
        var templateBytes = excelService.GenerateSampleExcelTemplate();
        return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProductImportTemplate.xlsx");
    }

    // POST: Import products from Excel
    [HttpPost]
    public async Task<IActionResult> ImportExcel(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            TempData["Error"] = "Please select an Excel file to upload.";
            return RedirectToAction(nameof(Index));
        }

        var extension = Path.GetExtension(excelFile.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
        {
            TempData["Error"] = "Please upload a valid Excel file (.xlsx or .xls)";
            return RedirectToAction(nameof(Index));
        }

        var excelService = HttpContext.RequestServices.GetRequiredService<IExcelService>();
        var userId = await GetCurrentUserId();

        using var stream = excelFile.OpenReadStream();
        var (successCount, errors) = await excelService.ImportProductsFromExcelAsync(stream, userId);

        if (successCount > 0)
        {
            TempData["Success"] = $"Successfully imported {successCount} product(s).";
        }

        if (errors.Any())
        {
            TempData["Error"] = $"Imported {successCount} products with {errors.Count} error(s):<br/>{string.Join("<br/>", errors.Take(10))}";
            if (errors.Count > 10)
            {
                TempData["Error"] += $"<br/>... and {errors.Count - 10} more errors.";
            }
        }
        else if (successCount == 0)
        {
            TempData["Error"] = "No products were imported. Please check the file format and try again.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Export products to Excel
    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var excelService = HttpContext.RequestServices.GetRequiredService<IExcelService>();
        var userId = await GetCurrentUserId();

        var excelBytes = await excelService.ExportProductsToExcelAsync(userId);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );
    }
}