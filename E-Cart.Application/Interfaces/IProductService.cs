using ECart.Application.DTOs.Common;
using ECart.Application.DTOs.Product;

namespace ECart.Application.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<PagedResult<ProductDto>>> GetAllAsync(ProductFilterDto filter);
        Task<ApiResponse<ProductDto>> GetByIdAsync(int id);
        Task<ApiResponse<ProductDto>> CreateAsync(CreateProductDto dto);
        Task<ApiResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}