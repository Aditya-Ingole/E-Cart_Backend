using E_Cart.Domain.Common;

namespace ECart.Domain.Entities
{
    public class Cart : BaseEntity
    {
        // Foreign Key
        public int UserId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}