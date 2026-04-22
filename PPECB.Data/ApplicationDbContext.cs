// PPECB.Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PPECB.Domain.Common;
using PPECB.Domain.Entities;
using System.Reflection.Emit;

namespace PPECB.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Category configuration
        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => new { c.CategoryCode, c.UserId }).IsUnique();
            entity.Property(c => c.CategoryCode).HasMaxLength(6).IsRequired();
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();

            entity.HasOne(c => c.User)
                  .WithMany(u => u.Categories)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Product configuration
        builder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => new { p.ProductCode, p.UserId }).IsUnique();
            entity.Property(p => p.ProductCode).HasMaxLength(20).IsRequired();
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);

            entity.HasOne(p => p.User)
                  .WithMany(u => u.Products)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedDate = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}