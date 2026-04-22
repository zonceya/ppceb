using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PPECB.Domain.Entities;
using PPECB.Services.Interfaces;

namespace PPECB.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        UserManager<ApplicationUser> userManager,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _userManager = userManager;
        _logger = logger;
    }

    private async Task<string> GetCurrentUserId()
    {
        var user = await _userManager.GetUserAsync(User);
        _logger.LogInformation($"GetCurrentUserId: User is {(user == null ? "null" : user.Email)}");
        return user!.Id;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("=== INDEX ACTION STARTED ===");
        var userId = await GetCurrentUserId();
        _logger.LogInformation($"Getting categories for userId: {userId}");

        var categories = await _categoryService.GetUserCategoriesAsync(userId);
        _logger.LogInformation($"Found {categories.Count} categories");

        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        _logger.LogInformation("=== CREATE GET ACTION CALLED ===");
        return View();
    }

    [HttpPost]  
    public async Task<IActionResult> Create(Category category)
    {
        _logger.LogInformation("=== CREATE POST ACTION STARTED ===");
        _logger.LogInformation($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        _logger.LogInformation($"Raw CategoryCode: '{category.CategoryCode}'");
        _logger.LogInformation($"Raw Name: '{category.Name}'");
        _logger.LogInformation($"Raw IsActive: {category.IsActive}");
        _logger.LogInformation($"Raw Id: {category.Id}");
        _logger.LogInformation($"Raw UserId: '{category.UserId}'");
        category.IsActive = true;
        // Log all model state before any changes
        _logger.LogInformation("=== MODEL STATE BEFORE REMOVAL ===");
        foreach (var key in ModelState.Keys)
        {
            var state = ModelState[key];
            var errors = state?.Errors;
            _logger.LogInformation($"Key: {key}, IsValid: {state?.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid}");
            if (errors != null && errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    _logger.LogWarning($"  Error: {error.ErrorMessage}");
                }
            }
        }

        // Remove navigation properties from validation
        _logger.LogInformation("Removing navigation properties from validation...");
        ModelState.Remove("User");
        ModelState.Remove("Products");
        ModelState.Remove("Category");

        _logger.LogInformation("=== MODEL STATE AFTER REMOVAL ===");
        foreach (var key in ModelState.Keys)
        {
            var state = ModelState[key];
            _logger.LogInformation($"Key: {key}, IsValid: {state?.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid}");
        }

        _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid - returning to Create view");

            // Log remaining errors
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key]?.Errors;
                if (errors != null && errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        _logger.LogWarning($"Final Error for {key}: {error.ErrorMessage}");
                    }
                }
            }

            return View(category);
        }

        _logger.LogInformation("ModelState is valid - proceeding with category creation");

        var userId = await GetCurrentUserId();
        _logger.LogInformation($"Current UserId: {userId}");

        _logger.LogInformation("Calling CategoryService.CreateCategoryAsync...");
        var result = await _categoryService.CreateCategoryAsync(category, userId);

        _logger.LogInformation($"Service Result - Success: {result.Success}");
        _logger.LogInformation($"Service Result - ErrorMessage: {result.ErrorMessage ?? "null"}");

        if (result.Success)
        {
            _logger.LogInformation($"SUCCESS! Category created. Redirecting to Index");
            TempData["Success"] = $"Category '{category.Name}' created successfully!";
            _logger.LogInformation("Redirecting to Index action");
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning($"FAILED: {result.ErrorMessage}");
        ModelState.AddModelError("CategoryCode", result.ErrorMessage);
        _logger.LogInformation("Returning to Create view with error");
        return View(category);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation($"=== EDIT GET ACTION CALLED for id: {id} ===");
        var category = await _categoryService.GetCategoryByIdAsync(id, await GetCurrentUserId());
        if (category == null)
        {
            _logger.LogWarning($"Category {id} not found");
            return NotFound();
        }
        return View(category);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        _logger.LogInformation($"=== EDIT POST ACTION CALLED for id: {id} ===");

        if (id != category.Id)
        {
            _logger.LogWarning($"ID mismatch: url id={id}, model id={category.Id}");
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState invalid in Edit POST");
            return View(category);
        }

        var result = await _categoryService.UpdateCategoryAsync(category, await GetCurrentUserId());

        if (result.Success)
        {
            _logger.LogInformation($"Category {id} updated successfully");
            TempData["Success"] = $"Category '{category.Name}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning($"Category update failed: {result.ErrorMessage}");
        ModelState.AddModelError("CategoryCode", result.ErrorMessage);
        return View(category);
    }
}