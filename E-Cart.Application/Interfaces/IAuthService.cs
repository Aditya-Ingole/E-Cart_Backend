using ECart.Application.DTOs.Auth;
using ECart.Application.DTOs.Common;

namespace ECart.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    }
}