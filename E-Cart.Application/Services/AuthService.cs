using ECart.Application.DTOs.Auth;
using ECart.Application.DTOs.Common;
using ECart.Application.Interfaces;
using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECart.Application.Services
{
    public class AuthService : IAuthService
    {
        // We depend on ABSTRACTIONS, not concrete classes.
        // DbContext is injected by the DI container at runtime.
        private readonly DbContext _dbContext;
        private readonly IConfiguration _configuration;

        // Constructor Injection — the DI container provides these automatically
        public AuthService(DbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        // ==================== REGISTER ====================

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            // Step 1: Check if email already exists
            // We query the Users DbSet to see if any user has this email.
            // .AnyAsync() returns true if at least one match is found.
            // This is more efficient than .FirstOrDefaultAsync() because
            // it stops searching as soon as it finds one match.
            var emailExists = await _dbContext.Set<User>()
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (emailExists)
            {
                return ApiResponse<AuthResponseDto>.FailResponse(
                    "Registration failed",
                    new List<string> { "An account with this email already exists" }
                );
            }

            // Step 2: Hash the password
            // BCrypt generates a unique salt each time, so even if two users
            // have the same password, their hashes will be different.
            // The "11" is the work factor — higher = slower but more secure.
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 11);

            // Step 3: Create the user entity
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = passwordHash,
                Phone = dto.Phone,
                RoleId = 1, // 1 = Customer (from our seed data)
                IsActive = true
            };

            // Step 4: Save to database
            _dbContext.Set<User>().Add(user);
            await _dbContext.SaveChangesAsync();

            // Step 5: Generate JWT token
            var token = GenerateJwtToken(user, "Customer");

            // Step 6: Build response
            var response = new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("JwtSettings:ExpirationInMinutes")
                ),
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = "Customer"
                }
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Registration successful");
        }

        // ==================== LOGIN ====================

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            // Step 1: Find user by email
            // We use .Include() to also load the related Role entity.
            // Without Include, user.Role would be null.
            var user = await _dbContext.Set<User>()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            // Step 2: Check if user exists
            // IMPORTANT: We use the same error message for "user not found"
            // and "wrong password." This prevents attackers from figuring out
            // which emails are registered (security best practice).
            if (user == null)
            {
                return ApiResponse<AuthResponseDto>.FailResponse(
                    "Invalid email or password"
                );
            }

            // Step 3: Check if user is active
            if (!user.IsActive)
            {
                return ApiResponse<AuthResponseDto>.FailResponse(
                    "Your account has been deactivated. Please contact support."
                );
            }

            // Step 4: Verify password
            // BCrypt.Verify takes the plain text password and the stored hash.
            // It extracts the salt from the hash, hashes the plain text with
            // that same salt, and compares the results.
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return ApiResponse<AuthResponseDto>.FailResponse(
                    "Invalid email or password"
                );
            }

            // Step 5: Generate JWT token
            var token = GenerateJwtToken(user, user.Role.Name);

            // Step 6: Build response
            var response = new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("JwtSettings:ExpirationInMinutes")
                ),
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role.Name
                }
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful");
        }

        // ==================== JWT TOKEN GENERATION ====================

        private string GenerateJwtToken(User user, string roleName)
        {
            // Step 1: Get JWT settings from configuration
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var expirationMinutes = jwtSettings.GetValue<int>("ExpirationInMinutes");

            // Step 2: Create the security key from the secret
            // The secret key is a string. We convert it to bytes,
            // then create a symmetric key (same key for signing and verifying).
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Step 3: Create signing credentials
            // HMAC-SHA256 is the algorithm used to sign the token.
            // It ensures that no one can tamper with the token contents.
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Step 4: Define claims (the data INSIDE the token)
            // Claims are like ID card fields: name, role, email, etc.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, roleName)
            };

            // Step 5: Create the token
            var token = new JwtSecurityToken(
                issuer: issuer,       // Who created this token (our API)
                audience: audience,   // Who this token is for (our React app)
                claims: claims,       // The data inside the token
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            // Step 6: Serialize the token to a string
            // This produces the long "eyJhbGciOi..." string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}