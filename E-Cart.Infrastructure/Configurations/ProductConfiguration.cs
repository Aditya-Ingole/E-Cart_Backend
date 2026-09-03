using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECart.Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(5000);

            // DECIMAL PRECISION — Critical for money!
            // decimal(18,2) means: up to 18 total digits, 2 after decimal point
            // Example: 9999999999999999.99 (more than enough for any product)
            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.DiscountPercent)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0);
            // 5,2 because discount is 0.00 to 100.00

            builder.Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Brand)
                .HasMaxLength(100);

            builder.Property(p => p.AverageRating)
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(0);
            // 3,2 because rating is 0.00 to 5.00

            // Unique SKU — every product has a unique stock code
            builder.HasIndex(p => p.SKU)
                .IsUnique();

            // Index on Name for search performance
            // When users search "iPhone", this index makes the query faster
            builder.HasIndex(p => p.Name);

            // Index on Price for sorting performance
            // When users sort by "Price: Low to High", this index helps
            builder.HasIndex(p => p.Price);

            // Index on CategoryId for filtering
            // When users filter by "Electronics", this speeds up the query
            builder.HasIndex(p => p.CategoryId);

            // Relationship: Product → Category (Many-to-One)
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            // Restrict = can't delete a category that has products

            // Relationship: Product → Images (One-to-Many)
            builder.HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Product → Inventory (One-to-One)
            builder.HasOne(p => p.Inventory)
                .WithOne(i => i.Product)
                .HasForeignKey<Inventory>(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Product → Reviews (One-to-Many)
            builder.HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}