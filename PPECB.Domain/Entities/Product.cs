using PPECB.Domain.Common;

namespace PPECB.Domain.Entities;

public class Product : BaseEntity, IUserOwned
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public string UserId { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    public virtual Category? Category { get; set; }
}