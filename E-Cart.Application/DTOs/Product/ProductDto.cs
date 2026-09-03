namespace ECart.Application.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountedPrice { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public int StockQuantity { get; set; }
        public bool InStock => StockQuantity > 0;
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsActive { get; set; }

        // Category info (flattened — we don't send the whole Category object)
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // Main image for product cards
        public string? MainImageUrl { get; set; }

        // All images for product detail page
        public List<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();

        public DateTime CreatedAt { get; set; }
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
    }
}