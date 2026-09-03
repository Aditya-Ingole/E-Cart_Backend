using E_Cart.Domain.Common;

namespace ECart.Domain.Entities
{
    public class WishlistItem : BaseEntity
    {
        // Foreign Keys
        public int WishlistId { get; set; }
        public int ProductId { get; set; }

        // Navigation properties
        public Wishlist Wishlist { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}