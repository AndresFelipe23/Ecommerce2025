using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechGadgets.API.Dtos.Brands;
using TechGadgets.API.Models.Common;

namespace TechGadgets.API.Services.Interfaces
{
    public interface IBrandService
    {
        // CRUD Básico
        Task<PagedResult<BrandDto>> GetBrandsAsync(BrandFilterDto filter);
        Task<BrandDto?> GetBrandByIdAsync(int id);
        Task<BrandDto?> GetBrandByNameAsync(string name);
        Task<BrandDto> CreateBrandAsync(CreateBrandDto dto);
        Task<BrandDto?> UpdateBrandAsync(int id, UpdateBrandDto dto);
        Task<bool> DeleteBrandAsync(int id);

        // Métodos de utilidad
        Task<bool> BrandExistsAsync(string name, int? excludeId = null);
        Task<bool> BrandHasProductsAsync(int id);
        Task<IEnumerable<BrandSummaryDto>> GetActiveBrandsAsync();
        
        // Estadísticas y reportes
        Task<BrandStatsDto> GetBrandStatsAsync();
        Task<IEnumerable<BrandProductCountDto>> GetBrandsWithProductCountAsync();

        // Gestión de estado
        Task<bool> ToggleBrandStatusAsync(int id);
        Task<int> BulkToggleStatusAsync(List<int> brandIds, bool active);
    }
}