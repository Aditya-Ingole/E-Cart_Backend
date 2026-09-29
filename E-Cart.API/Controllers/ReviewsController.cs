using ECart.Application.DTOs.Review;
using ECart.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!);
        }

        // GET /api/reviews/product/1
        // Public endpoint - anyone can see reviews
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var result = await _reviewService.GetProductReviewsAsync(productId);

            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        // POST /api/reviews
        // Protected endpoint - only logged-in users can write reviews
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
        {
            var userId = GetUserId();
            var result = await _reviewService.AddReviewAsync(userId, dto);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}