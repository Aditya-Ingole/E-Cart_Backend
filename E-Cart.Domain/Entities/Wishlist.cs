using E_Cart.Domain.Common;

namespace ECart.Domain.Entities
{
    public class Wishlist : BaseEntity
    {
        // Foreign Key
        public int UserId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
    }
}