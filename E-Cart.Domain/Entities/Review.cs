using E_Cart.Domain.Common;

namespace ECart.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }

        // Foreign Keys
        public int ProductId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}