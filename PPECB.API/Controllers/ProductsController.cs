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
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               throw new UnauthorizedAccessException("User ID not found");
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var (products, totalCount) = await _productService.GetUserProductsPagedAsync(userId, page, pageSize);

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            ProductCode = p.ProductCode,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImagePath = p.ImagePath,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? "N/A"
        });

        return Ok(new
        {
            products = productDtos,
            totalCount = totalCount,
            page = page,
            pageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var userId = GetCurrentUserId();
        var product = await _productService.GetProductByIdAsync(id, userId);
        if (product == null)
            return NotFound();

        var productDto = new ProductDto
        {
            Id = product.Id,
            ProductCode = product.ProductCode,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImagePath = product.ImagePath,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "N/A"
        };

        return Ok(productDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        var userId = GetCurrentUserId();
        var result = await _productService.CreateProductAsync(product, userId);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        var productDto = new ProductDto
        {
            Id = result.Product.Id,
            ProductCode = result.Product.ProductCode,
            Name = result.Product.Name,
            Description = result.Product.Description,
            Price = result.Product.Price,
            ImagePath = result.Product.ImagePath,
            CategoryId = result.Product.CategoryId,
            CategoryName = result.Product.Category?.Name ?? "N/A"
        };

        return Ok(productDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
    {
        if (id != product.Id)
            return BadRequest("ID mismatch");

        var userId = GetCurrentUserId();
        var result = await _productService.UpdateProductAsync(product, userId);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        var productDto = new ProductDto
        {
            Id = product.Id,
            ProductCode = product.ProductCode,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImagePath = product.ImagePath,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "N/A"
        };

        return Ok(productDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _productService.DeleteProductAsync(id, userId);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Product deleted successfully" });
    }
}