using ECart.Application.DTOs.User;
using ECart.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All user endpoints require authentication
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!);
        }

        // ==================== PROFILE ====================

        // GET /api/users/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var result = await _userService.GetProfileAsync(userId);

            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        // PUT /api/users/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            var result = await _userService.UpdateProfileAsync(userId, dto);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // POST /api/users/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetUserId();
            var result = await _userService.ChangePasswordAsync(userId, dto);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // ==================== ADDRESSES ====================

        // GET /api/users/addresses
        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = GetUserId();
            var result = await _userService.GetAddressesAsync(userId);
            return Ok(result);
        }

        // POST /api/users/addresses
        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto dto)
        {
            var userId = GetUserId();
            var result = await _userService.AddAddressAsync(userId, dto);

            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetAddresses), result);
        }

        // PUT /api/users/addresses/5
        [HttpPut("addresses/{addressId}")]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] UpdateAddressDto dto)
        {
            var userId = GetUserId();
            var result = await _userService.UpdateAddressAsync(userId, addressId, dto);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // DELETE /api/users/addresses/5
        [HttpDelete("addresses/{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var userId = GetUserId();
            var result = await _userService.DeleteAddressAsync(userId, addressId);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // PUT /api/users/addresses/5/default
        [HttpPut("addresses/{addressId}/default")]
        public async Task<IActionResult> SetDefaultAddress(int addressId)
        {
            var userId = GetUserId();
            var result = await _userService.SetDefaultAddressAsync(userId, addressId);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}