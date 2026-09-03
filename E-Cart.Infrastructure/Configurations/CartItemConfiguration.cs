using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECart.Infrastructure.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");

            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // A user shouldn't have the same product twice in their cart.
            // This composite index enforces: one row per (CartId, ProductId) combination.
            builder.HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();
        }
    }
}