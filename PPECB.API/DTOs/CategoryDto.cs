namespace PPECB.API.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}