using ECart.Application.DTOs.Category;
using ECart.Application.DTOs.Common;
using ECart.Application.Interfaces;
using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECart.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly DbContext _dbContext;

        public CategoryService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<CategoryDto>>> GetAllAsync()
        {
            var categories = await _dbContext.Set<Category>()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    ProductCount = c.Products.Count(p => p.IsActive && !p.IsDeleted)
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return ApiResponse<List<CategoryDto>>.SuccessResponse(categories);
        }

        public async Task<ApiResponse<CategoryDto>> GetByIdAsync(int id)
        {
            var category = await _dbContext.Set<Category>()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    ProductCount = c.Products.Count(p => p.IsActive && !p.IsDeleted)
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return ApiResponse<CategoryDto>.FailResponse("Category not found");

            return ApiResponse<CategoryDto>.SuccessResponse(category);
        }

        public async Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryDto dto)
        {
            // Check for duplicate name
            var exists = await _dbContext.Set<Category>()
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            if (exists)
                return ApiResponse<CategoryDto>.FailResponse(
                    "Category already exists",
                    new List<string> { $"A category with name '{dto.Name}' already exists" }
                );

            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsActive = true
            };

            _dbContext.Set<Category>().Add(category);
            await _dbContext.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive,
                ProductCount = 0
            };

            return ApiResponse<CategoryDto>.SuccessResponse(result, "Category created successfully");
        }

        public async Task<ApiResponse<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _dbContext.Set<Category>()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return ApiResponse<CategoryDto>.FailResponse("Category not found");

            // Check for duplicate name (excluding current category)
            var nameExists = await _dbContext.Set<Category>()
                .AnyAsync(c => c.Id != id && c.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
                return ApiResponse<CategoryDto>.FailResponse(
                    "Category name already taken",
                    new List<string> { $"A category with name '{dto.Name}' already exists" }
                );

            category.Name = dto.Name.Trim();
            category.Description = dto.Description;
            category.ImageUrl = dto.ImageUrl;
            category.IsActive = dto.IsActive;

            await _dbContext.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive,
                ProductCount = await _dbContext.Set<Product>()
                    .CountAsync(p => p.CategoryId == id && p.IsActive)
            };

            return ApiResponse<CategoryDto>.SuccessResponse(result, "Category updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var category = await _dbContext.Set<Category>()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return ApiResponse<bool>.FailResponse("Category not found");

            // Don't delete categories that have products
            var hasProducts = await _dbContext.Set<Product>()
                .AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
                return ApiResponse<bool>.FailResponse(
                    "Cannot delete category",
                    new List<string> { "This category has products. Remove or reassign them first." }
                );

            _dbContext.Set<Category>().Remove(category);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully");
        }
    }
}