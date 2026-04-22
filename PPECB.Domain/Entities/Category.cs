using PPECB.Domain.Common;

namespace PPECB.Domain.Entities;

public class Category : BaseEntity, IUserOwned
{
    public string CategoryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string UserId { get; set; } = string.Empty;

    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    public virtual ICollection<Product>? Products { get; set; }
}