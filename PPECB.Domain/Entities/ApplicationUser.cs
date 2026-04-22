// PPECB.Domain/Entities/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace PPECB.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}