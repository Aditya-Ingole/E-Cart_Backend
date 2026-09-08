namespace ECart.Application.DTOs.Cart
{
    public class CartDto
    {
        public int Id { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public int TotalItems { get; set; }
        public int TotalUniqueProducts { get; set; }

        // Financial summary — ALL calculated on the server
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal GrandTotal { get; set; }
    }
}