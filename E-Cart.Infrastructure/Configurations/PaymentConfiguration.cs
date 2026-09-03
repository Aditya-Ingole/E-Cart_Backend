using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECart.Infrastructure.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Method)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.TransactionId)
                .HasMaxLength(200);

            // Store PaymentStatus as string (like OrderStatus)
            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Unique constraint — one payment per order
            builder.HasIndex(p => p.OrderId)
                .IsUnique();
        }
    }
}