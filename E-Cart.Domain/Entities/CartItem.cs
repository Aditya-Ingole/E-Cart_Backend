using E_Cart.Domain.Common;

namespace ECart.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }

        // Foreign Keys
        public int CartId { get; set; }
        public int ProductId { get; set; }

        // Navigation properties
        public Cart Cart { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}