using ECart.Application.DTOs.Cart;
using ECart.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ALL cart endpoints require authentication
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // Helper method to extract userId from JWT token
        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!);
        }

        // GET /api/cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            var result = await _cartService.GetCartAsync(userId);
            return Ok(result);
        }

        // POST /api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
        {
            var userId = GetUserId();
            var result = await _cartService.AddItemAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // PUT /api/cart/items/5
        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
        {
            var userId = GetUserId();
            var result = await _cartService.UpdateItemAsync(userId, cartItemId, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // DELETE /api/cart/items/5
        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveItemAsync(userId, cartItemId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // DELETE /api/cart/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var result = await _cartService.ClearCartAsync(userId);
            return Ok(result);
        }
    }
}