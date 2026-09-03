using ECart.Application.DTOs.Common;
using ECart.Application.DTOs.Product;
using ECart.Application.Interfaces;
using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECart.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly DbContext _dbContext;

        public ProductService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ==================== GET ALL (with Search, Filter, Sort, Pagination) ====================

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetAllAsync(ProductFilterDto filter)
        {
            // Start with the base query
            var query = _dbContext.Set<Product>()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsActive);
            // Note: IsDeleted filter is already applied globally by our DbContext

            // ---- SEARCH ----
            // If the user typed something in the search bar, filter by name or description
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchTerm = filter.Search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.Brand != null && p.Brand.ToLower().Contains(searchTerm)
                );
            }

            // ---- FILTERS ----
            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Brand))
                query = query.Where(p => p.Brand != null && p.Brand.ToLower() == filter.Brand.ToLower());

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.MinRating.HasValue)
                query = query.Where(p => p.AverageRating >= filter.MinRating.Value);

            if (filter.InStock.HasValue && filter.InStock.Value)
                query = query.Where(p => p.StockQuantity > 0);

            // ---- COUNT (before pagination) ----
            // We need the total count for the pagination metadata.
            // This must be done BEFORE Skip/Take.
            var totalCount = await query.CountAsync();

            // ---- SORT ----
            query = filter.SortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt) // default: newest first
            };

            // ---- PAGINATION ----
            // Skip = how many items to skip (page 2 with size 12 = skip 12)
            // Take = how many items to return
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountPercent = p.DiscountPercent,
                    DiscountedPrice = p.Price * (1 - p.DiscountPercent / 100),
                    SKU = p.SKU,
                    Brand = p.Brand,
                    StockQuantity = p.StockQuantity,
                    AverageRating = p.AverageRating,
                    ReviewCount = p.ReviewCount,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    MainImageUrl = p.Images
                        .Where(i => i.IsMain)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                    Images = p.Images
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => new ProductImageDto
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                            IsMain = i.IsMain,
                            DisplayOrder = i.DisplayOrder
                        })
                        .ToList(),
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var result = new PagedResult<ProductDto>
            {
                Items = items,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };

            return ApiResponse<PagedResult<ProductDto>>.SuccessResponse(result);
        }

        // ==================== GET BY ID ====================

        public async Task<ApiResponse<ProductDto>> GetByIdAsync(int id)
        {
            var product = await _dbContext.Set<Product>()
                .Include(p => p.Category)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return ApiResponse<ProductDto>.FailResponse("Product not found");

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPercent = product.DiscountPercent,
                DiscountedPrice = product.Price * (1 - product.DiscountPercent / 100),
                SKU = product.SKU,
                Brand = product.Brand,
                StockQuantity = product.StockQuantity,
                AverageRating = product.AverageRating,
                ReviewCount = product.ReviewCount,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                MainImageUrl = product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl,
                Images = product.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain,
                    DisplayOrder = i.DisplayOrder
                }).ToList(),
                CreatedAt = product.CreatedAt
            };

            return ApiResponse<ProductDto>.SuccessResponse(dto);
        }

        // ==================== CREATE ====================

        public async Task<ApiResponse<ProductDto>> CreateAsync(CreateProductDto dto)
        {
            // Validate category exists
            var categoryExists = await _dbContext.Set<Domain.Entities.Category>()
                .AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);

            if (!categoryExists)
                return ApiResponse<ProductDto>.FailResponse(
                    "Invalid category",
                    new List<string> { "The specified category does not exist" }
                );

            // Validate unique SKU
            var skuExists = await _dbContext.Set<Product>()
                .AnyAsync(p => p.SKU.ToLower() == dto.SKU.ToLower());

            if (skuExists)
                return ApiResponse<ProductDto>.FailResponse(
                    "Duplicate SKU",
                    new List<string> { $"A product with SKU '{dto.SKU}' already exists" }
                );

            // Create product entity
            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Price = dto.Price,
                DiscountPercent = dto.DiscountPercent,
                SKU = dto.SKU.Trim().ToUpper(),
                Brand = dto.Brand?.Trim(),
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                IsActive = true
            };

            _dbContext.Set<Product>().Add(product);
            await _dbContext.SaveChangesAsync();

            // Add images
            if (dto.ImageUrls.Any())
            {
                var images = dto.ImageUrls.Select((url, index) => new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = url,
                    IsMain = index == 0, // First image is the main image
                    DisplayOrder = index
                }).ToList();

                _dbContext.Set<ProductImage>().AddRange(images);
                await _dbContext.SaveChangesAsync();
            }

            // Create inventory record
            var inventory = new Inventory
            {
                ProductId = product.Id,
                Quantity = dto.StockQuantity,
                LowStockThreshold = 10,
                LastRestockedAt = DateTime.UtcNow
            };

            _dbContext.Set<Inventory>().Add(inventory);
            await _dbContext.SaveChangesAsync();

            // Return the created product
            return await GetByIdAsync(product.Id);
        }

        // ==================== UPDATE ====================

        public async Task<ApiResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _dbContext.Set<Product>()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return ApiResponse<ProductDto>.FailResponse("Product not found");

            // Validate category
            var categoryExists = await _dbContext.Set<Domain.Entities.Category>()
                .AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);

            if (!categoryExists)
                return ApiResponse<ProductDto>.FailResponse(
                    "Invalid category",
                    new List<string> { "The specified category does not exist" }
                );

            // Validate unique SKU (excluding current product)
            var skuExists = await _dbContext.Set<Product>()
                .AnyAsync(p => p.Id != id && p.SKU.ToLower() == dto.SKU.ToLower());

            if (skuExists)
                return ApiResponse<ProductDto>.FailResponse(
                    "Duplicate SKU",
                    new List<string> { $"A product with SKU '{dto.SKU}' already exists" }
                );

            // Update product fields
            product.Name = dto.Name.Trim();
            product.Description = dto.Description.Trim();
            product.Price = dto.Price;
            product.DiscountPercent = dto.DiscountPercent;
            product.SKU = dto.SKU.Trim().ToUpper();
            product.Brand = dto.Brand?.Trim();
            product.StockQuantity = dto.StockQuantity;
            product.CategoryId = dto.CategoryId;
            product.IsActive = dto.IsActive;

            // Update images: remove old, add new
            if (dto.ImageUrls.Any())
            {
                _dbContext.Set<ProductImage>().RemoveRange(product.Images);

                var newImages = dto.ImageUrls.Select((url, index) => new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = url,
                    IsMain = index == 0,
                    DisplayOrder = index
                }).ToList();

                _dbContext.Set<ProductImage>().AddRange(newImages);
            }

            // Update inventory
            var inventory = await _dbContext.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == id);

            if (inventory != null)
            {
                inventory.Quantity = dto.StockQuantity;
                inventory.LastRestockedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        // ==================== DELETE (Soft Delete) ====================

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var product = await _dbContext.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return ApiResponse<bool>.FailResponse("Product not found");

            // Soft delete — the global SaveChanges override will handle this
            _dbContext.Set<Product>().Remove(product);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully");
        }
    }
}