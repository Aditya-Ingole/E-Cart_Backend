using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECart.Infrastructure.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            // All money fields need decimal(18,2)
            builder.Property(o => o.SubTotal)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.Discount)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.Tax)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.ShippingCost)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.GrandTotal)
                .HasColumnType("decimal(18,2)");

            // Shipping address snapshot fields
            builder.Property(o => o.ShippingFullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(o => o.ShippingPhone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.ShippingStreet)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.ShippingCity)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.ShippingState)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.ShippingPostalCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.ShippingCountry)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.Notes)
                .HasMaxLength(1000);

            // Unique order number
            builder.HasIndex(o => o.OrderNumber)
                .IsUnique();

            // Index for user's order history queries
            builder.HasIndex(o => o.UserId);

            // Index for admin order status filtering
            builder.HasIndex(o => o.Status);

            // Convert OrderStatus enum to string in database
            // Instead of storing 0, 1, 2... we store "Pending", "Confirmed"...
            // This makes the database human-readable and easier to debug
            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Relationship: Order → OrderItems
            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Order → Payment (One-to-One)
            builder.HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}