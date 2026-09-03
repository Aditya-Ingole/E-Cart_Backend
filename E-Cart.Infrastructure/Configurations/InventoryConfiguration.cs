using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECart.Infrastructure.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.ToTable("Inventories");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.LowStockThreshold)
                .HasDefaultValue(10);

            // Index for low-stock queries
            builder.HasIndex(i => i.Quantity);
        }
    }
}