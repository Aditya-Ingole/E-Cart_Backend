using System.ComponentModel.DataAnnotations;

namespace ECart.Application.DTOs.Cart
{
    public class AddToCartDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; } = 1;
    }
}