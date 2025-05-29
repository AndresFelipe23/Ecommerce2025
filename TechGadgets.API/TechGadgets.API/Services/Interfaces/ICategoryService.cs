using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechGadgets.API.Dtos.Categories;
using TechGadgets.API.Models.Common;

namespace TechGadgets.API.Services.Interfaces
{
    public interface ICategoryService
    {
        // CRUD Básico
        Task<PagedResult<CategoryDto>> GetCategoriesAsync(CategoryFilterDto filter);
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto?> GetCategoryBySlugAsync(string slug);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
        Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);

        // Funcionalidades jerárquicas
        Task<List<CategoryTreeDto>> GetCategoryTreeAsync(bool activeOnly = true);
        Task<List<CategoryTreeDto>> GetSubcategoriesAsync(int parentId, bool activeOnly = true);
        Task<List<CategorySummaryDto>> GetRootCategoriesAsync(bool activeOnly = true);
        Task<List<CategoryBreadcrumbDto>> GetCategoryBreadcrumbAsync(int id);

        // Validaciones
        Task<bool> CategoryExistsAsync(string name, int? excludeId = null);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
        Task<bool> CategoryHasProductsAsync(int id, bool includeSubcategories = true);
        Task<bool> CategoryHasChildrenAsync(int id);
        Task<bool> IsValidParentAsync(int categoryId, int? parentId);

        // Operaciones avanzadas
        Task<bool> MoveCategoryAsync(int categoryId, int? newParentId, int newOrder);
        Task<List<CategoryDto>> GetCategoryPathAsync(int id);
        Task<int> GetCategoryLevelAsync(int id);

        // Utilidades
        Task<IEnumerable<CategorySummaryDto>> GetActiveCategoriesAsync();
        Task<CategoryStatsDto> GetCategoryStatsAsync();
        Task<IEnumerable<CategoryProductCountDto>> GetCategoriesWithProductCountAsync(bool includeSubcategories = true);

        // Operaciones masivas
        Task<bool> ToggleCategoryStatusAsync(int id);
        Task<int> BulkToggleStatusAsync(List<int> categoryIds, bool active);
        Task<int> BulkDeleteAsync(List<int> categoryIds);
        Task<int> ReorderCategoriesAsync(List<int> categoryIds);
    }
}