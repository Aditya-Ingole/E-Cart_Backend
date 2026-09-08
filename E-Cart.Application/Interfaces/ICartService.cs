using ECart.Application.DTOs.Cart;
using ECart.Application.DTOs.Common;

namespace ECart.Application.Interfaces
{
    public interface ICartService
    {
        Task<ApiResponse<CartDto>> GetCartAsync(int userId);
        Task<ApiResponse<CartDto>> AddItemAsync(int userId, AddToCartDto dto);
        Task<ApiResponse<CartDto>> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto);
        Task<ApiResponse<CartDto>> RemoveItemAsync(int userId, int cartItemId);
        Task<ApiResponse<bool>> ClearCartAsync(int userId);
    }
}