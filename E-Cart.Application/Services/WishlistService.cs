using ECart.Application.DTOs.Common;
using ECart.Application.DTOs.Wishlist;
using ECart.Application.Interfaces;
using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECart.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly DbContext _dbContext;

        public WishlistService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ==================== GET WISHLIST ====================

        public async Task<ApiResponse<WishlistDto>> GetWishlistAsync(int userId)
        {
            var wishlist = await GetOrCreateWishlistAsync(userId);
            var wishlistDto = await BuildWishlistDtoAsync(wishlist);

            return ApiResponse<WishlistDto>.SuccessResponse(wishlistDto);
        }

        // ==================== ADD ITEM ====================

        public async Task<ApiResponse<WishlistDto>> AddItemAsync(int userId, AddToWishlistDto dto)
        {
            // Step 1: Validate product exists and is active
            var product = await _dbContext.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive);

            if (product == null)
                return ApiResponse<WishlistDto>.FailResponse(
                    "Product not found",
                    new List<string> { "This product doesn't exist or is no longer available" }
                );

            // Step 2: Get or create wishlist
            var wishlist = await GetOrCreateWishlistAsync(userId);

            // Step 3: Check if already in wishlist (including soft-deleted)
            // Same pattern as Cart — use IgnoreQueryFilters to handle re-adding
            var existingItem = await _dbContext.Set<WishlistItem>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(wi => wi.WishlistId == wishlist.Id && wi.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                if (existingItem.IsDeleted)
                {
                    // Re-activate previously removed item
                    existingItem.IsDeleted = false;
                    existingItem.DeletedAt = null;
                }
                else
                {
                    // Already in wishlist — not an error, just return current state
                    var wishlistDto = await BuildWishlistDtoAsync(wishlist);
                    return ApiResponse<WishlistDto>.SuccessResponse(wishlistDto, "Product is already in your wishlist");
                }
            }
            else
            {
                // New item — add to wishlist
                var wishlistItem = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    ProductId = dto.ProductId
                };

                _dbContext.Set<WishlistItem>().Add(wishlistItem);
            }

            await _dbContext.SaveChangesAsync();

            var result = await BuildWishlistDtoAsync(wishlist);
            return ApiResponse<WishlistDto>.SuccessResponse(result, "Product added to wishlist");
        }

        // ==================== REMOVE ITEM ====================

        public async Task<ApiResponse<WishlistDto>> RemoveItemAsync(int userId, int productId)
        {
            var wishlist = await GetOrCreateWishlistAsync(userId);

            // Find the wishlist item by productId (not wishlistItemId)
            // This is more user-friendly — the frontend knows the productId
            var wishlistItem = await _dbContext.Set<WishlistItem>()
                .FirstOrDefaultAsync(wi => wi.WishlistId == wishlist.Id && wi.ProductId == productId);

            if (wishlistItem == null)
                return ApiResponse<WishlistDto>.FailResponse("Product is not in your wishlist");

            _dbContext.Set<WishlistItem>().Remove(wishlistItem);
            await _dbContext.SaveChangesAsync();

            var result = await BuildWishlistDtoAsync(wishlist);
            return ApiResponse<WishlistDto>.SuccessResponse(result, "Product removed from wishlist");
        }

        // ==================== CHECK IF IN WISHLIST ====================

        public async Task<ApiResponse<bool>> IsInWishlistAsync(int userId, int productId)
        {
            var wishlist = await _dbContext.Set<Wishlist>()
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
                return ApiResponse<bool>.SuccessResponse(false);

            var exists = await _dbContext.Set<WishlistItem>()
                .AnyAsync(wi => wi.WishlistId == wishlist.Id && wi.ProductId == productId);

            return ApiResponse<bool>.SuccessResponse(exists);
        }

        // ==================== PRIVATE HELPERS ====================

        private async Task<Wishlist> GetOrCreateWishlistAsync(int userId)
        {
            var wishlist = await _dbContext.Set<Wishlist>()
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = userId };
                _dbContext.Set<Wishlist>().Add(wishlist);
                await _dbContext.SaveChangesAsync();
            }

            return wishlist;
        }

        private async Task<WishlistDto> BuildWishlistDtoAsync(Wishlist wishlist)
        {
            // Use AsNoTracking to avoid ChangeTracker conflicts (same lesson as Cart)
            var items = await _dbContext.Set<WishlistItem>()
                .AsNoTracking()
                .Where(wi => wi.WishlistId == wishlist.Id)
                .Include(wi => wi.Product)
                    .ThenInclude(p => p.Images)
                .ToListAsync();

            var validItems = items
                .Where(wi => wi.Product != null && wi.Product.IsActive && !wi.Product.IsDeleted)
                .ToList();

            var itemDtos = validItems.Select(wi =>
            {
                var product = wi.Product;

                return new WishlistItemDto
                {
                    Id = wi.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = product.Images
                        .Where(i => i.IsMain)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() ?? product.Images.Select(i => i.ImageUrl).FirstOrDefault(),
                    Price = product.Price,
                    DiscountPercent = product.DiscountPercent,
                    DiscountedPrice = product.Price * (1 - product.DiscountPercent / 100),
                    Brand = product.Brand,
                    AverageRating = product.AverageRating,
                    InStock = product.StockQuantity > 0,
                    AddedAt = wi.CreatedAt
                };
            })
            .OrderByDescending(wi => wi.AddedAt) // Newest additions first
            .ToList();

            return new WishlistDto
            {
                Id = wishlist.Id,
                Items = itemDtos,
                TotalItems = itemDtos.Count
            };
        }
    }
}