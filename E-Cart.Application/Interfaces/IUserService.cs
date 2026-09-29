using ECart.Application.DTOs.Common;
using ECart.Application.DTOs.User;

namespace ECart.Application.Interfaces
{
    public interface IUserService
    {
        // Profile
        Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId);
        Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);

        // Addresses
        Task<ApiResponse<List<AddressDto>>> GetAddressesAsync(int userId);
        Task<ApiResponse<AddressDto>> AddAddressAsync(int userId, CreateAddressDto dto);
        Task<ApiResponse<AddressDto>> UpdateAddressAsync(int userId, int addressId, UpdateAddressDto dto);
        Task<ApiResponse<bool>> DeleteAddressAsync(int userId, int addressId);
        Task<ApiResponse<bool>> SetDefaultAddressAsync(int userId, int addressId);
    }
}