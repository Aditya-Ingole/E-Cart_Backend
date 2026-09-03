using ECart.Application.DTOs.Category;
using ECart.Application.DTOs.Common;

namespace ECart.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<List<CategoryDto>>> GetAllAsync();
        Task<ApiResponse<CategoryDto>> GetByIdAsync(int id);
        Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryDto dto);
        Task<ApiResponse<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}