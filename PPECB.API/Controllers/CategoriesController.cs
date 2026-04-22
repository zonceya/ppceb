using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPECB.API.DTOs;
using PPECB.Domain.Entities;
using PPECB.Services.Interfaces;
using System.Security.Claims;

namespace PPECB.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               throw new UnauthorizedAccessException("User ID not found");
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var userId = GetCurrentUserId();
        var categories = await _categoryService.GetUserCategoriesAsync(userId);

        var categoryDtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            CategoryCode = c.CategoryCode,
            Name = c.Name,
            IsActive = c.IsActive
        });

        return Ok(categoryDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var userId = GetCurrentUserId();
        var category = await _categoryService.GetCategoryByIdAsync(id, userId);
        if (category == null)
            return NotFound();

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            CategoryCode = category.CategoryCode,
            Name = category.Name,
            IsActive = category.IsActive
        };

        return Ok(categoryDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] Category category)
    {
        var userId = GetCurrentUserId();
        var result = await _categoryService.CreateCategoryAsync(category, userId);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            CategoryCode = category.CategoryCode,
            Name = category.Name,
            IsActive = category.IsActive
        };

        return Ok(categoryDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
    {
        if (id != category.Id)
            return BadRequest("ID mismatch");

        var userId = GetCurrentUserId();
        var result = await _categoryService.UpdateCategoryAsync(category, userId);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            CategoryCode = category.CategoryCode,
            Name = category.Name,
            IsActive = category.IsActive
        };

        return Ok(categoryDto);
    }
}