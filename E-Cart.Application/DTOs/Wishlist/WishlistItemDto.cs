namespace ECart.Application.DTOs.Wishlist
{
    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountedPrice { get; set; }
        public string? Brand { get; set; }
        public decimal AverageRating { get; set; }
        public bool InStock { get; set; }
        public DateTime AddedAt { get; set; }
    }
}