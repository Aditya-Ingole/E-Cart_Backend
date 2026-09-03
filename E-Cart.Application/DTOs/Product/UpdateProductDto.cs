using System.ComponentModel.DataAnnotations;

namespace ECart.Application.DTOs.Product
{
    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(5000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public decimal DiscountPercent { get; set; } = 0;

        [Required(ErrorMessage = "SKU is required")]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int StockQuantity { get; set; } = 0;

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; } = true;

        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}