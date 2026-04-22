namespace PPECB.API.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}