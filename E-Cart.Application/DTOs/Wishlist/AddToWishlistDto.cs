using System.ComponentModel.DataAnnotations;

namespace ECart.Application.DTOs.Wishlist
{
    public class AddToWishlistDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }
    }
}