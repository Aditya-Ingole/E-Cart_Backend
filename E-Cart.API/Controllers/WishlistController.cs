using ECart.Application.DTOs.Wishlist;
using ECart.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All wishlist endpoints require authentication
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!);
        }

        // GET /api/wishlist
        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = GetUserId();
            var result = await _wishlistService.GetWishlistAsync(userId);
            return Ok(result);
        }

        // POST /api/wishlist/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToWishlistDto dto)
        {
            var userId = GetUserId();
            var result = await _wishlistService.AddItemAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // DELETE /api/wishlist/items/5  (5 = productId)
        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = GetUserId();
            var result = await _wishlistService.RemoveItemAsync(userId, productId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // GET /api/wishlist/check/5  (5 = productId)
        // Used by product detail page to show filled/empty heart icon
        [HttpGet("check/{productId}")]
        public async Task<IActionResult> IsInWishlist(int productId)
        {
            var userId = GetUserId();
            var result = await _wishlistService.IsInWishlistAsync(userId, productId);
            return Ok(result);
        }
    }
}