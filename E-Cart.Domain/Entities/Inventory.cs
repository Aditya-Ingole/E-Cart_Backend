using E_Cart.Domain.Common;

namespace ECart.Domain.Entities
{
    public class Inventory : BaseEntity
    {
        public int Quantity { get; set; } = 0;
        public int LowStockThreshold { get; set; } = 10;
        public DateTime? LastRestockedAt { get; set; }

        // Foreign Key (also the Primary Key — one-to-one relationship)
        public int ProductId { get; set; }

        // Navigation property
        public Product Product { get; set; } = null!;
    }
}