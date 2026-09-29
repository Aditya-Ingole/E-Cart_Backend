using ECart.Application.DTOs.Common;
using ECart.Application.DTOs.User;
using ECart.Application.Interfaces;
using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECart.Application.Services
{
    public class UserService : IUserService
    {
        private readonly DbContext _dbContext;

        public UserService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ==================== PROFILE ====================

        public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
        {
            var user = await _dbContext.Set<User>()
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ApiResponse<UserProfileDto>.FailResponse("User not found");

            var dto = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.Name,
                CreatedAt = user.CreatedAt
            };

            return ApiResponse<UserProfileDto>.SuccessResponse(dto);
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _dbContext.Set<User>()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ApiResponse<UserProfileDto>.FailResponse("User not found");

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Phone = dto.Phone?.Trim();

            await _dbContext.SaveChangesAsync();

            var result = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.Name,
                CreatedAt = user.CreatedAt
            };

            return ApiResponse<UserProfileDto>.SuccessResponse(result, "Profile updated successfully");
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ApiResponse<bool>.FailResponse("User not found");

            // Step 1: Verify current password
            var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);

            if (!isCurrentPasswordValid)
                return ApiResponse<bool>.FailResponse(
                    "Password change failed",
                    new List<string> { "Current password is incorrect" }
                );

            // Step 2: Ensure new password is different from current
            var isSamePassword = BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash);

            if (isSamePassword)
                return ApiResponse<bool>.FailResponse(
                    "Password change failed",
                    new List<string> { "New password must be different from current password" }
                );

            // Step 3: Hash and save new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, 11);

            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
        }

        // ==================== ADDRESSES ====================

        public async Task<ApiResponse<List<AddressDto>>> GetAddressesAsync(int userId)
        {
            var addresses = await _dbContext.Set<Address>()
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault) // Default address first
                .ThenByDescending(a => a.CreatedAt)   // Then newest first
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Phone = a.Phone,
                    Street = a.Street,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    IsDefault = a.IsDefault
                })
                .ToListAsync();

            return ApiResponse<List<AddressDto>>.SuccessResponse(addresses);
        }

        public async Task<ApiResponse<AddressDto>> AddAddressAsync(int userId, CreateAddressDto dto)
        {
            // If this is set as default, unset all other defaults first
            if (dto.IsDefault)
            {
                await UnsetDefaultAddressesAsync(userId);
            }

            // If this is the user's first address, make it default automatically
            var hasAnyAddress = await _dbContext.Set<Address>()
                .AnyAsync(a => a.UserId == userId);

            if (!hasAnyAddress)
            {
                dto.IsDefault = true;
            }

            var address = new Address
            {
                UserId = userId,
                FullName = dto.FullName.Trim(),
                Phone = dto.Phone.Trim(),
                Street = dto.Street.Trim(),
                City = dto.City.Trim(),
                State = dto.State.Trim(),
                PostalCode = dto.PostalCode.Trim(),
                Country = dto.Country.Trim(),
                IsDefault = dto.IsDefault
            };

            _dbContext.Set<Address>().Add(address);
            await _dbContext.SaveChangesAsync();

            var result = new AddressDto
            {
                Id = address.Id,
                FullName = address.FullName,
                Phone = address.Phone,
                Street = address.Street,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                IsDefault = address.IsDefault
            };

            return ApiResponse<AddressDto>.SuccessResponse(result, "Address added successfully");
        }

        public async Task<ApiResponse<AddressDto>> UpdateAddressAsync(int userId, int addressId, UpdateAddressDto dto)
        {
            var address = await _dbContext.Set<Address>()
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
                return ApiResponse<AddressDto>.FailResponse("Address not found");

            // If setting this as default, unset others
            if (dto.IsDefault && !address.IsDefault)
            {
                await UnsetDefaultAddressesAsync(userId);
            }

            address.FullName = dto.FullName.Trim();
            address.Phone = dto.Phone.Trim();
            address.Street = dto.Street.Trim();
            address.City = dto.City.Trim();
            address.State = dto.State.Trim();
            address.PostalCode = dto.PostalCode.Trim();
            address.Country = dto.Country.Trim();
            address.IsDefault = dto.IsDefault;

            await _dbContext.SaveChangesAsync();

            var result = new AddressDto
            {
                Id = address.Id,
                FullName = address.FullName,
                Phone = address.Phone,
                Street = address.Street,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                IsDefault = address.IsDefault
            };

            return ApiResponse<AddressDto>.SuccessResponse(result, "Address updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAddressAsync(int userId, int addressId)
        {
            var address = await _dbContext.Set<Address>()
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
                return ApiResponse<bool>.FailResponse("Address not found");

            // Don't allow deleting the default address if it's the only one
            if (address.IsDefault)
            {
                var addressCount = await _dbContext.Set<Address>()
                    .CountAsync(a => a.UserId == userId);

                if (addressCount > 1)
                    return ApiResponse<bool>.FailResponse(
                        "Cannot delete default address",
                        new List<string> { "Please set another address as default before deleting this one" }
                    );
            }

            _dbContext.Set<Address>().Remove(address);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Address deleted successfully");
        }

        public async Task<ApiResponse<bool>> SetDefaultAddressAsync(int userId, int addressId)
        {
            var address = await _dbContext.Set<Address>()
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
                return ApiResponse<bool>.FailResponse("Address not found");

            if (address.IsDefault)
                return ApiResponse<bool>.SuccessResponse(true, "Address is already the default");

            await UnsetDefaultAddressesAsync(userId);

            address.IsDefault = true;
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Default address updated");
        }

        // ==================== PRIVATE HELPERS ====================

        // Unset all default addresses for a user
        // This ensures only ONE address is marked as default at any time
        private async Task UnsetDefaultAddressesAsync(int userId)
        {
            var defaultAddresses = await _dbContext.Set<Address>()
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var addr in defaultAddresses)
            {
                addr.IsDefault = false;
            }
        }
    }
}