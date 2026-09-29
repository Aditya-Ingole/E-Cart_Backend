using ECart.API.Extensions;
using ECart.Application.Interfaces;
using ECart.Application.Services;
using ECart.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// ==================== Add Services ====================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to support JWT authentication
// This adds a "Authorize" button in Swagger UI where you can paste your token
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ECart API",
        Version = "v1",
        Description = "E-Commerce API for ECart application"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGciOi..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register database
builder.Services.AddDatabaseConfiguration(builder.Configuration);

// ==================== Register Application Services (DI) ====================

// This is the magic line that connects the interface to the implementation.
// When a controller asks for IAuthService, the DI container gives it AuthService.
// "Scoped" means one instance per HTTP request.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// We need to register the DbContext as a plain DbContext for the Application layer.
// The Application layer doesn't know about ApplicationDbContext (Clean Architecture!).
// So we register a factory that provides the ApplicationDbContext as a DbContext.
builder.Services.AddScoped<DbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// ==================== Configure JWT Authentication ====================

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
        // ClockSkew = 0 means the token expires EXACTLY at the expiration time.
        // By default, there's a 5-minute grace period. We remove it for precision.
    };
});

builder.Services.AddAuthorization();

// ==================== Build App ====================

var app = builder.Build();

// ==================== Configure Pipeline ====================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANT: Authentication must come BEFORE Authorization
// Authentication = "Who are you?" (check the JWT token)
// Authorization = "Are you allowed?" (check the role)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==================== Run ====================

app.Run();