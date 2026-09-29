using ECart.Application.DTOs.Common;
using ECart.Application.DTOs.Review;
using ECart.Application.Interfaces;
using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECart.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly DbContext _dbContext;

        public ReviewService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ==================== GET REVIEWS FOR A PRODUCT ====================

        public async Task<ApiResponse<List<ReviewDto>>> GetProductReviewsAsync(int productId)
        {
            // Verify product exists
            var productExists = await _dbContext.Set<Product>().AnyAsync(p => p.Id == productId);
            if (!productExists)
                return ApiResponse<List<ReviewDto>>.FailResponse("Product not found");

            var reviews = await _dbContext.Set<Review>()
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .Include(r => r.User) // Include user to get their name
                .OrderByDescending(r => r.CreatedAt) // Newest first
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = $"{r.User.FirstName} {r.User.LastName.Substring(0, 1)}.", // e.g. "Rahul S."
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<ReviewDto>>.SuccessResponse(reviews);
        }

        // ==================== ADD REVIEW ====================

        public async Task<ApiResponse<ReviewDto>> AddReviewAsync(int userId, CreateReviewDto dto)
        {
            // 1. Verify product exists and is active
            var product = await _dbContext.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive);

            if (product == null)
                return ApiResponse<ReviewDto>.FailResponse("Product not found or inactive");

            // 2. Check if user already reviewed this product
            var existingReview = await _dbContext.Set<Review>()
                .FirstOrDefaultAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);

            if (existingReview != null)
                return ApiResponse<ReviewDto>.FailResponse(
                    "Review already exists",
                    new List<string> { "You have already reviewed this product." });

            // 3. Create the review
            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Rating = dto.Rating,
                Title = dto.Title?.Trim(),
                Comment = dto.Comment?.Trim()
            };

            _dbContext.Set<Review>().Add(review);
            await _dbContext.SaveChangesAsync(); // Save to generate Review ID

            // 4. Update the Product's cached rating and count
            await UpdateProductRatingAsync(dto.ProductId);

            // 5. Fetch the user info to build the return DTO
            var user = await _dbContext.Set<User>().FirstAsync(u => u.Id == userId);

            var resultDto = new ReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                UserName = $"{user.FirstName} {user.LastName.Substring(0, 1)}.",
                Rating = review.Rating,
                Title = review.Title,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };

            return ApiResponse<ReviewDto>.SuccessResponse(resultDto, "Review added successfully");
        }

        // ==================== PRIVATE HELPER ====================

        // Recalculates the average rating for a product and saves it
        private async Task UpdateProductRatingAsync(int productId)
        {
            var product = await _dbContext.Set<Product>().FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return;

            // Get all ratings for this product
            var ratings = await _dbContext.Set<Review>()
                .Where(r => r.ProductId == productId)
                .Select(r => r.Rating)
                .ToListAsync();

            if (ratings.Any())
            {
                // Calculate average and round to 1 decimal place (e.g., 4.5)
                product.AverageRating = Math.Round((decimal)ratings.Average(), 1);
                product.ReviewCount = ratings.Count;
            }
            else
            {
                product.AverageRating = 0;
                product.ReviewCount = 0;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}