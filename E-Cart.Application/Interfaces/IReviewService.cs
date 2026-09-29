using ECart.Application.DTOs.Common;
using ECart.Application.DTOs.Review;

namespace ECart.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ApiResponse<List<ReviewDto>>> GetProductReviewsAsync(int productId);
        Task<ApiResponse<ReviewDto>> AddReviewAsync(int userId, CreateReviewDto dto);
    }
}