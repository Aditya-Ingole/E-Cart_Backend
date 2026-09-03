using E_Cart.Domain.Common;

namespace ECart.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        // Foreign Key
        public int ProductId { get; set; }

        // Navigation property
        public Product Product { get; set; } = null!;
    }
}