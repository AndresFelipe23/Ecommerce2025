
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechGadgets.API.Dtos.Products;
using TechGadgets.API.Models.Common;

namespace TechGadgets.API.Services.Interfaces
{
    public interface IProductService
    {
        #region CRUD Básico
        Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto?> GetProductBySlugAsync(string slug);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<ProductImageDto> AddProductImageAsync(int productId, CreateProductImageDto imageDto);
        Task<List<ProductImageDto>> AddMultipleProductImagesAsync(int productId, List<CreateProductImageDto> imageDtos);
        Task<bool> DeleteProductAsync(int id);
        #endregion

        #region Búsqueda y Filtros Avanzados
        Task<ProductSearchResultDto> SearchProductsAsync(ProductFilterDto filter);
        Task<IEnumerable<ProductSummaryDto>> GetFeaturedProductsAsync(int count = 8);
        Task<IEnumerable<ProductSummaryDto>> GetProductsOnSaleAsync(int count = 12);
        Task<IEnumerable<ProductSummaryDto>> GetRelatedProductsAsync(int productId, int count = 6);
        Task<IEnumerable<ProductSummaryDto>> GetProductsByCategoryAsync(int categoryId, int count = 12);
        Task<IEnumerable<ProductSummaryDto>> GetProductsByBrandAsync(int brandId, int count = 12);
        Task<IEnumerable<ProductSummaryDto>> GetNewestProductsAsync(int count = 8);
        Task<IEnumerable<ProductSummaryDto>> GetActiveProductsAsync();
        Task<bool> UpdateImageOrderAsync(int productId, List<UpdateImageOrderDto> imageOrders);
        Task<bool> SetMainImageAsync(int productId, int imageId);
        Task<ProductSearchFiltersDto> GetAvailableFiltersAsync(ProductFilterDto? currentFilter = null);
        #endregion

        #region Gestión de Stock
        Task<bool> AdjustStockAsync(AdjustStockDto dto);
        Task<bool> UpdateStockAsync(int productId, int newStock);
        Task<IEnumerable<ProductSummaryDto>> GetLowStockProductsAsync();
        Task<IEnumerable<ProductSummaryDto>> GetOutOfStockProductsAsync();
        #endregion

        #region Validaciones
        Task<bool> ProductExistsAsync(string name, int? excludeId = null);
        Task<bool> SKUExistsAsync(string sku, int? excludeId = null);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
        #endregion

        #region Operaciones Masivas
        Task<bool> ToggleProductStatusAsync(int id);
        Task<int> BulkToggleStatusAsync(List<int> productIds, bool active);
        Task<int> BulkUpdatePricesAsync(BulkPriceUpdateDto dto);
        Task<int> BulkDeleteAsync(List<int> productIds);
        #endregion

        #region Estadísticas y Reportes
        Task<ProductStatsDto> GetProductStatsAsync();
        Task<IEnumerable<ProductSummaryDto>> GetBestSellingProductsAsync(int count = 10);
        #endregion

        #region Gestión de Imágenes
        Task<bool> ProductHasImagesAsync(int productId);
        Task<string?> GetProductMainImageUrlAsync(int productId);
        Task<IEnumerable<ProductImageDto>> GetProductImagesAsync(int productId);
        Task<bool> DeleteProductImageAsync(int productId, int imageId);
        Task<ProductImageDto?> GetProductImageAsync(int productId, int imageId);
        Task<bool> ReorderProductImagesAsync(int productId);
        #endregion

        #region Debug y Utilidades
        Task<object> GetProductDebugInfoAsync(int id);
        #endregion
    }
}