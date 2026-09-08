using ECart.Application.DTOs.Common;
using ECart.Application.DTOs.Wishlist;

namespace ECart.Application.Interfaces
{
    public interface IWishlistService
    {
        Task<ApiResponse<WishlistDto>> GetWishlistAsync(int userId);
        Task<ApiResponse<WishlistDto>> AddItemAsync(int userId, AddToWishlistDto dto);
        Task<ApiResponse<WishlistDto>> RemoveItemAsync(int userId, int productId);
        Task<ApiResponse<bool>> IsInWishlistAsync(int userId, int productId);
    }
}