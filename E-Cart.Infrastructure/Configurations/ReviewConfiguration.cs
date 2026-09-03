using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECart.Infrastructure.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Title)
                .HasMaxLength(200);

            builder.Property(r => r.Comment)
                .HasMaxLength(2000);

            // A user can only review a product once
            builder.HasIndex(r => new { r.ProductId, r.UserId })
                .IsUnique();

            // Index for fetching all reviews of a product
            builder.HasIndex(r => r.ProductId);
        }
    }
}