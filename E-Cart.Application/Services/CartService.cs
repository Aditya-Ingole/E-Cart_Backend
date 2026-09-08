using ECart.Application.DTOs.Cart;
using ECart.Application.DTOs.Common;
using ECart.Application.Interfaces;
using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECart.Application.Services
{
    public class CartService : ICartService
    {
        private readonly DbContext _dbContext;

        // Tax rate: 18% (GST)
        private const decimal TaxRate = 0.18m;

        // Free shipping above ₹500, otherwise ₹50
        private const decimal FreeShippingThreshold = 500m;
        private const decimal ShippingCost = 50m;

        public CartService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ==================== GET CART ====================

        public async Task<ApiResponse<CartDto>> GetCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartDto = await BuildCartDtoAsync(cart);

            return ApiResponse<CartDto>.SuccessResponse(cartDto);
        }

        // ==================== ADD ITEM ====================

        public async Task<ApiResponse<CartDto>> AddItemAsync(int userId, AddToCartDto dto)
        {
            var product = await _dbContext.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive);

            if (product == null)
                return ApiResponse<CartDto>.FailResponse(
                    "Product not found",
                    new List<string> { "The product you're trying to add doesn't exist or is no longer available" }
                );

            if (product.StockQuantity < dto.Quantity)
                return ApiResponse<CartDto>.FailResponse(
                    "Insufficient stock",
                    new List<string> { $"Only {product.StockQuantity} items available in stock" }
                );

            var cart = await GetOrCreateCartAsync(userId);

            // Ignore query filters here to check if item was soft-deleted
            var existingItem = await _dbContext.Set<CartItem>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                if (existingItem.IsDeleted)
                {
                    existingItem.IsDeleted = false;
                    existingItem.DeletedAt = null;
                    existingItem.Quantity = dto.Quantity;
                    existingItem.UnitPrice = product.Price;
                }
                else
                {
                    var newQuantity = existingItem.Quantity + dto.Quantity;

                    if (newQuantity > product.StockQuantity)
                        return ApiResponse<CartDto>.FailResponse(
                            "Insufficient stock",
                            new List<string> { $"You already have {existingItem.Quantity} in your cart. Only {product.StockQuantity} available total." }
                        );

                    existingItem.Quantity = newQuantity;
                    existingItem.UnitPrice = product.Price;
                }
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UnitPrice = product.Price
                };

                _dbContext.Set<CartItem>().Add(cartItem);
            }

            await _dbContext.SaveChangesAsync();

            var cartDto = await BuildCartDtoAsync(cart);

            return ApiResponse<CartDto>.SuccessResponse(cartDto, "Item added to cart");
        }

        // ==================== UPDATE QUANTITY ====================

        public async Task<ApiResponse<CartDto>> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto)
        {
            var cart = await GetOrCreateCartAsync(userId);

            // Secure simplified lookup
            var cartItem = await _dbContext.Set<CartItem>()
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);

            if (cartItem == null)
                return ApiResponse<CartDto>.FailResponse("Cart item not found");

            var product = await _dbContext.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

            if (product == null || !product.IsActive)
                return ApiResponse<CartDto>.FailResponse(
                    "Product unavailable",
                    new List<string> { "This product is no longer available" }
                );

            if (dto.Quantity > product.StockQuantity)
                return ApiResponse<CartDto>.FailResponse(
                    "Insufficient stock",
                    new List<string> { $"Only {product.StockQuantity} items available" }
                );

            cartItem.Quantity = dto.Quantity;
            cartItem.UnitPrice = product.Price;

            await _dbContext.SaveChangesAsync();

            var cartDto = await BuildCartDtoAsync(cart);

            return ApiResponse<CartDto>.SuccessResponse(cartDto, "Cart updated");
        }

        // ==================== REMOVE ITEM ====================

        public async Task<ApiResponse<CartDto>> RemoveItemAsync(int userId, int cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId);

            // Secure simplified lookup
            var cartItem = await _dbContext.Set<CartItem>()
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);

            if (cartItem == null)
                return ApiResponse<CartDto>.FailResponse("Cart item not found");

            _dbContext.Set<CartItem>().Remove(cartItem);
            await _dbContext.SaveChangesAsync();

            var cartDto = await BuildCartDtoAsync(cart);

            return ApiResponse<CartDto>.SuccessResponse(cartDto, "Item removed from cart");
        }

        // ==================== CLEAR CART ====================

        public async Task<ApiResponse<bool>> ClearCartAsync(int userId)
        {
            var cart = await _dbContext.Set<Cart>()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return ApiResponse<bool>.SuccessResponse(true, "Cart is already empty");

            _dbContext.Set<CartItem>().RemoveRange(cart.Items);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Cart cleared");
        }

        // ==================== PRIVATE HELPER METHODS ====================

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _dbContext.Set<Cart>()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _dbContext.Set<Cart>().Add(cart);
                await _dbContext.SaveChangesAsync();
            }

            return cart;
        }

        private async Task<CartDto> BuildCartDtoAsync(Cart cart)
        {
            // CRITICAL: We use AsNoTracking() to bypass tracking cache conflicts
            var cartItems = await _dbContext.Set<CartItem>()
                .AsNoTracking()
                .Where(ci => ci.CartId == cart.Id)
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                .ToListAsync();

            var validItems = cartItems
                .Where(ci => ci.Product != null && ci.Product.IsActive && !ci.Product.IsDeleted)
                .ToList();

            var items = validItems.Select(ci =>
            {
                var product = ci.Product;
                var discountedPrice = product.Price * (1 - product.DiscountPercent / 100);

                return new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = product.Images
                        .Where(i => i.IsMain)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() ?? product.Images.Select(i => i.ImageUrl).FirstOrDefault(),
                    UnitPrice = product.Price,
                    DiscountPercent = product.DiscountPercent,
                    DiscountedUnitPrice = discountedPrice,
                    Quantity = ci.Quantity,
                    TotalPrice = discountedPrice * ci.Quantity,
                    StockAvailable = product.StockQuantity,
                    InStock = product.StockQuantity > 0 && product.IsActive
                };
            }).ToList();

            // Financial summary calculations
            var subTotal = items.Sum(i => i.UnitPrice * i.Quantity);
            var totalDiscount = items.Sum(i => (i.UnitPrice - i.DiscountedUnitPrice) * i.Quantity);
            var taxableAmount = subTotal - totalDiscount;

            var tax = items.Any() ? Math.Round(taxableAmount * TaxRate, 2) : 0m;
            var shippingCost = (!items.Any() || taxableAmount >= FreeShippingThreshold) ? 0m : ShippingCost;
            var grandTotal = items.Any() ? Math.Round(taxableAmount + tax + shippingCost, 2) : 0m;

            return new CartDto
            {
                Id = cart.Id,
                Items = items,
                TotalItems = items.Sum(i => i.Quantity),
                TotalUniqueProducts = items.Count,
                SubTotal = subTotal,
                TotalDiscount = totalDiscount,
                Tax = tax,
                ShippingCost = shippingCost,
                GrandTotal = grandTotal
            };
        }
    }
}