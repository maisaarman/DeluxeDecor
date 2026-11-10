using Microsoft.EntityFrameworkCore;

namespace DeluxeDecor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Category>(e =>
        {
            e.Property(x => x.Slug).HasMaxLength(100);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        b.Entity<CategoryTranslation>(e =>
        {
            e.Property(x => x.LanguageCode).HasMaxLength(5);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasIndex(x => new { x.CategoryId, x.LanguageCode }).IsUnique();
            e.HasOne(x => x.Category).WithMany(c => c.Translations)
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Product>(e =>
        {
            e.Property(x => x.Slug).HasMaxLength(120);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasOne(x => x.Category).WithMany(c => c.Products)
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProductTranslation>(e =>
        {
            e.Property(x => x.LanguageCode).HasMaxLength(5);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(4000);
            e.HasIndex(x => new { x.ProductId, x.LanguageCode }).IsUnique();
            e.HasOne(x => x.Product).WithMany(p => p.Translations)
                .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ProductImage>(e =>
        {
            e.Property(x => x.FileName).HasMaxLength(100);
            e.HasOne(x => x.Product).WithMany(p => p.Images)
                .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}