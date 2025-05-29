// Services/Implementations/CategoryService.cs
using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Dtos.Categories;
using TechGadgets.API.Models.Entities;
using TechGadgets.API.Models.Common;
using TechGadgets.API.Services.Interfaces;
using TechGadgets.API.Helpers;

namespace TechGadgets.API.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly TechGadgetsDbContext _context;

        public CategoryService(TechGadgetsDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(CategoryFilterDto filter)
        {
            var query = _context.Categorias.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(filter.Nombre))
            {
                query = query.Where(c => c.CatNombre.Contains(filter.Nombre));
            }

            if (filter.CategoriaPadreId.HasValue)
            {
                query = query.Where(c => c.CatCategoriaPadreId == filter.CategoriaPadreId.Value);
            }

            if (filter.SoloRaiz == true)
            {
                query = query.Where(c => c.CatCategoriaPadreId == null);
            }

            if (filter.Activo.HasValue)
            {
                query = query.Where(c => c.CatActivo == filter.Activo.Value);
            }

            if (filter.FechaDesde.HasValue)
            {
                query = query.Where(c => c.CatFechaCreacion >= filter.FechaDesde.Value);
            }

            if (filter.FechaHasta.HasValue)
            {
                query = query.Where(c => c.CatFechaCreacion <= filter.FechaHasta.Value);
            }

            // Aplicar ordenamiento
            query = filter.SortBy?.ToLower() switch
            {
                "nombre" => filter.SortDescending 
                    ? query.OrderByDescending(c => c.CatNombre)
                    : query.OrderBy(c => c.CatNombre),
                "orden" => filter.SortDescending 
                    ? query.OrderByDescending(c => c.CatOrden).ThenBy(c => c.CatNombre)
                    : query.OrderBy(c => c.CatOrden).ThenBy(c => c.CatNombre),
                "fechacreacion" => filter.SortDescending 
                    ? query.OrderByDescending(c => c.CatFechaCreacion)
                    : query.OrderBy(c => c.CatFechaCreacion),
                _ => query.OrderBy(c => c.CatOrden).ThenBy(c => c.CatNombre)
            };

            var totalItems = await query.CountAsync();

           var categorias = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(c => c.CatCategoriaPadre)
            .Include(c => c.Productos)
            .Include(c => c.InverseCatCategoriaPadre)
            .ToListAsync();

            var items = categorias.Select(c => new CategoryDto
            {
                Id = c.CatId,
                Nombre = c.CatNombre,
                Descripcion = c.CatDescripcion,
                CategoriaPadreId = c.CatCategoriaPadreId,
                CategoriaPadreNombre = c.CatCategoriaPadre?.CatNombre,
                Imagen = c.CatImagen,
                Icono = c.CatIcono,
                Slug = c.CatSlug,
                Orden = c.CatOrden ?? 0,
                Activo = c.CatActivo ?? true,
                FechaCreacion = c.CatFechaCreacion ?? DateTime.UtcNow,
                TotalProductos = c.Productos.Count(p => p.PrdActivo == true),
                TotalSubcategorias = c.InverseCatCategoriaPadre.Count(sub => sub.CatActivo == true),
                RutaCompleta = BuildCategoryPath(c) // ✅ Ahora es válido
            }).ToList();



            // Incluir subcategorías si se solicita
            if (filter.IncluirSubcategorias)
            {
                foreach (var item in items)
                {
                    item.Subcategorias = await GetSubcategoriesDtoAsync(item.Id);
                }
            }

            return new PagedResult<CategoryDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.CatCategoriaPadre)
                .Include(c => c.Productos)
                .Include(c => c.InverseCatCategoriaPadre)
                .FirstOrDefaultAsync(c => c.CatId == id);

            if (categoria == null) return null;

            var dto = new CategoryDto
            {
                Id = categoria.CatId,
                Nombre = categoria.CatNombre,
                Descripcion = categoria.CatDescripcion,
                CategoriaPadreId = categoria.CatCategoriaPadreId,
                CategoriaPadreNombre = categoria.CatCategoriaPadre?.CatNombre,
                Imagen = categoria.CatImagen,
                Icono = categoria.CatIcono,
                Slug = categoria.CatSlug,
                Orden = categoria.CatOrden.HasValue ? categoria.CatOrden.Value : 0,
                Activo = categoria.CatActivo.HasValue ? categoria.CatActivo.Value : true,
                FechaCreacion = categoria.CatFechaCreacion.HasValue ? categoria.CatFechaCreacion.Value : DateTime.UtcNow,
                TotalProductos = categoria.Productos.Count(p => p.PrdActivo == true),
                TotalSubcategorias = categoria.InverseCatCategoriaPadre.Count(sub => sub.CatActivo == true),
                RutaCompleta = BuildCategoryPath(categoria)
            };

            // Cargar subcategorías
            dto.Subcategorias = await GetSubcategoriesDtoAsync(id);

            return dto;
        }

        private async Task<List<CategoryDto>> GetSubcategoriesDtoAsync(int parentId)
        {
            return await _context.Categorias
                .Where(c => c.CatCategoriaPadreId == parentId && c.CatActivo == true)
                .OrderBy(c => c.CatOrden).ThenBy(c => c.CatNombre)
                .Select(c => new CategoryDto
                {
                    Id = c.CatId,
                    Nombre = c.CatNombre,
                    Descripcion = c.CatDescripcion,
                    CategoriaPadreId = c.CatCategoriaPadreId,
                    Imagen = c.CatImagen,
                    Icono = c.CatIcono,
                    Slug = c.CatSlug,
                    Orden = c.CatOrden.HasValue ? c.CatOrden.Value : 0,
                    Activo = c.CatActivo.HasValue ? c.CatActivo.Value : true,
                    FechaCreacion = c.CatFechaCreacion.HasValue ? c.CatFechaCreacion.Value : DateTime.UtcNow,
                    TotalProductos = c.Productos.Count(p => p.PrdActivo == true),
                    TotalSubcategorias = c.InverseCatCategoriaPadre.Count(sub => sub.CatActivo == true)
                })
                .ToListAsync();
        }

        private string BuildCategoryPath(Categoria categoria)
        {
            var path = new List<string>();
            var current = categoria;

            while (current != null)
            {
                path.Insert(0, current.CatNombre);
                current = current.CatCategoriaPadre;
            }

            return string.Join(" > ", path);
        }

        public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.CatSlug == slug);

            if (categoria == null) return null;

            return await GetCategoryByIdAsync(categoria.CatId);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            // Generar slug si no se proporciona
            var slug = !string.IsNullOrWhiteSpace(dto.Slug) 
                ? SlugHelper.GenerateSlug(dto.Slug)
                : SlugHelper.GenerateSlug(dto.Nombre);

            // Asegurar que el slug sea único
            slug = await EnsureUniqueSlugAsync(slug);

            var categoria = new Categoria
            {
                CatNombre = dto.Nombre.Trim(),
                CatDescripcion = dto.Descripcion?.Trim(),
                CatCategoriaPadreId = dto.CategoriaPadreId,
                CatImagen = dto.Imagen?.Trim(),
                CatIcono = dto.Icono?.Trim(),
                CatSlug = slug,
                CatOrden = dto.Orden,
                CatActivo = true,
                CatFechaCreacion = DateTime.UtcNow
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return await GetCategoryByIdAsync(categoria.CatId) 
                ?? throw new InvalidOperationException("Error al crear la categoría");
        }

        private async Task<string> EnsureUniqueSlugAsync(string baseSlug, int? excludeId = null)
        {
            var slug = baseSlug;
            var counter = 1;

            while (await SlugExistsAsync(slug, excludeId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return null;

            // Validar que no se convierta en su propio padre
            if (dto.CategoriaPadreId.HasValue && !await IsValidParentAsync(id, dto.CategoriaPadreId.Value))
            {
                throw new InvalidOperationException("No se puede asignar como padre una categoría descendiente");
            }

            // Actualizar slug si cambió el nombre o se proporcionó uno nuevo
            var newSlug = !string.IsNullOrWhiteSpace(dto.Slug) 
                ? SlugHelper.GenerateSlug(dto.Slug)
                : SlugHelper.GenerateSlug(dto.Nombre);

            if (newSlug != categoria.CatSlug)
            {
                newSlug = await EnsureUniqueSlugAsync(newSlug, id);
            }

            categoria.CatNombre = dto.Nombre.Trim();
            categoria.CatDescripcion = dto.Descripcion?.Trim();
            categoria.CatCategoriaPadreId = dto.CategoriaPadreId;
            categoria.CatImagen = dto.Imagen?.Trim();
            categoria.CatIcono = dto.Icono?.Trim();
            categoria.CatSlug = newSlug;
            categoria.CatOrden = dto.Orden;
            categoria.CatActivo = dto.Activo;

            await _context.SaveChangesAsync();
            return await GetCategoryByIdAsync(id);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.InverseCatCategoriaPadre)
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.CatId == id);

            if (categoria == null) return false;

            // Verificar si tiene subcategorías o productos
            var hasChildren = categoria.InverseCatCategoriaPadre.Any(sub => sub.CatActivo == true);
            var hasProducts = categoria.Productos.Any(p => p.PrdActivo == true);

            if (hasChildren || hasProducts)
            {
                // Desactivar en lugar de eliminar
                categoria.CatActivo = false;
            }
            else
            {
                // Eliminar si no tiene dependencias
                _context.Categorias.Remove(categoria);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CategoryTreeDto>> GetCategoryTreeAsync(bool activeOnly = true)
        {
            var query = _context.Categorias.AsQueryable();

            if (activeOnly)
            {
                query = query.Where(c => c.CatActivo == true);
            }

            var allCategories = await query
                .Include(c => c.Productos)
                .OrderBy(c => c.CatOrden).ThenBy(c => c.CatNombre)
                .ToListAsync();

            return BuildCategoryTree(allCategories, null, 0);
        }

        private List<CategoryTreeDto> BuildCategoryTree(List<Categoria> allCategories, int? parentId, int level)
        {
            return allCategories
                .Where(c => c.CatCategoriaPadreId == parentId)
                .Select(c => new CategoryTreeDto
                {
                    Id = c.CatId,
                    Nombre = c.CatNombre,
                    Descripcion = c.CatDescripcion,
                    Imagen = c.CatImagen,
                    Icono = c.CatIcono,
                    Slug = c.CatSlug,
                    Orden = c.CatOrden.HasValue ? c.CatOrden.Value : 0,
                    Activo = c.CatActivo.HasValue ? c.CatActivo.Value : true,
                    TotalProductos = c.Productos.Count(p => p.PrdActivo == true),
                    Nivel = level,
                    Hijos = BuildCategoryTree(allCategories, c.CatId, level + 1)
                })
                .ToList();
        }

        public async Task<List<CategoryTreeDto>> GetSubcategoriesAsync(int parentId, bool activeOnly = true)
        {
            var query = _context.Categorias.Where(c => c.CatCategoriaPadreId == parentId);

            if (activeOnly)
            {
                query = query.Where(c => c.CatActivo == true);
            }

            return await query
                .Include(c => c.Productos)
                .OrderBy(c => c.CatOrden).ThenBy(c => c.CatNombre)
                .Select(c => new CategoryTreeDto
                {
                    Id = c.CatId,
                    Nombre = c.CatNombre,
                    Descripcion = c.CatDescripcion,
                    Imagen = c.CatImagen,
                    Icono = c.CatIcono,
                    Slug = c.CatSlug,
                    Orden = c.CatOrden.HasValue ? c.CatOrden.Value : 0,
                    Activo = c.CatActivo.HasValue ? c.CatActivo.Value : true,
                    TotalProductos = c.Productos.Count(p => p.PrdActivo == true),
                    Nivel = 1
                })
                .ToListAsync();
        }

        public async Task<List<CategorySummaryDto>> GetRootCategoriesAsync(bool activeOnly = true)
        {
            var query = _context.Categorias.Where(c => c.CatCategoriaPadreId == null);

            if (activeOnly)
            {
                query = query.Where(c => c.CatActivo == true);
            }

            return await query
                .OrderBy(c => c.CatOrden).ThenBy(c => c.CatNombre)
                .Select(c => new CategorySummaryDto
                {
                    Id = c.CatId,
                    Nombre = c.CatNombre,
                    Icono = c.CatIcono,
                    Slug = c.CatSlug,
                    Activo = c.CatActivo.HasValue ? c.CatActivo.Value : true,
                    RutaCompleta = c.CatNombre
                })
                .ToListAsync();
        }

        public async Task<List<CategoryBreadcrumbDto>> GetCategoryBreadcrumbAsync(int id)
        {
            var breadcrumbs = new List<CategoryBreadcrumbDto>();
            var categoria = await _context.Categorias
                .Include(c => c.CatCategoriaPadre)
                .FirstOrDefaultAsync(c => c.CatId == id);

            while (categoria != null)
            {
                breadcrumbs.Insert(0, new CategoryBreadcrumbDto
                {
                    Id = categoria.CatId,
                    Nombre = categoria.CatNombre,
                    Slug = categoria.CatSlug
                });

                categoria = categoria.CatCategoriaPadre;
            }

            return breadcrumbs;
        }

        public async Task<bool> CategoryExistsAsync(string name, int? excludeId = null)
        {
            return await _context.Categorias
                .AnyAsync(c => c.CatNombre.ToLower() == name.ToLower() && 
                          (excludeId == null || c.CatId != excludeId));
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
        {
            return await _context.Categorias
                .AnyAsync(c => c.CatSlug == slug && 
                          (excludeId == null || c.CatId != excludeId));
        }

        public async Task<bool> CategoryHasProductsAsync(int id, bool includeSubcategories = true)
        {
            var hasDirectProducts = await _context.Productos
                .AnyAsync(p => p.PrdCategoriaId == id && p.PrdActivo == true);

            if (hasDirectProducts) return true;

            if (includeSubcategories)
            {
                var subcategories = await GetAllSubcategoryIdsAsync(id);
                return await _context.Productos
                    .AnyAsync(p => subcategories.Contains(p.PrdCategoriaId) && p.PrdActivo == true);
            }

            return false;
        }

        private async Task<List<int>> GetAllSubcategoryIdsAsync(int parentId)
        {
            var result = new List<int>();
            var directChildren = await _context.Categorias
                .Where(c => c.CatCategoriaPadreId == parentId)
                .Select(c => c.CatId)
                .ToListAsync();

            foreach (var childId in directChildren)
            {
                result.Add(childId);
                var grandChildren = await GetAllSubcategoryIdsAsync(childId);
                result.AddRange(grandChildren);
            }

            return result;
        }

        public async Task<bool> CategoryHasChildrenAsync(int id)
        {
            return await _context.Categorias
                .AnyAsync(c => c.CatCategoriaPadreId == id && c.CatActivo == true);
        }

        public async Task<bool> IsValidParentAsync(int categoryId, int? parentId)
        {
            if (!parentId.HasValue) return true;

            // No puede ser su propio padre
            if (categoryId == parentId.Value) return false;

            // Verificar que el padre no sea un descendiente
            var allDescendants = await GetAllSubcategoryIdsAsync(categoryId);
            return !allDescendants.Contains(parentId.Value);
        }

        public async Task<bool> MoveCategoryAsync(int categoryId, int? newParentId, int newOrder)
        {
            var categoria = await _context.Categorias.FindAsync(categoryId);
            if (categoria == null) return false;

            if (!await IsValidParentAsync(categoryId, newParentId))
                return false;

            categoria.CatCategoriaPadreId = newParentId;
            categoria.CatOrden = newOrder;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CategoryDto>> GetCategoryPathAsync(int id)
        {
            var path = new List<CategoryDto>();
            var categoria = await _context.Categorias
                .Include(c => c.CatCategoriaPadre)
                .FirstOrDefaultAsync(c => c.CatId == id);

            while (categoria != null)
            {
                path.Insert(0, new CategoryDto
                {
                    Id = categoria.CatId,
                    Nombre = categoria.CatNombre,
                    Slug = categoria.CatSlug,
                    Activo = categoria.CatActivo.HasValue ? categoria.CatActivo.Value : true
                });

                categoria = categoria.CatCategoriaPadre;
            }

            return path;
        }

        public async Task<int> GetCategoryLevelAsync(int id)
        {
            var level = 0;
            var categoria = await _context.Categorias
                .Include(c => c.CatCategoriaPadre)
                .FirstOrDefaultAsync(c => c.CatId == id);

            while (categoria?.CatCategoriaPadre != null)
            {
                level++;
                categoria = categoria.CatCategoriaPadre;
            }

            return level;
        }

        public async Task<IEnumerable<CategorySummaryDto>> GetActiveCategoriesAsync()
        {
            return await _context.Categorias
                .Where(c => c.CatActivo == true)
                .OrderBy(c => c.CatOrden).ThenBy(c => c.CatNombre)
                .Select(c => new CategorySummaryDto
                {
                    Id = c.CatId,
                    Nombre = c.CatNombre,
                    Icono = c.CatIcono,
                    Slug = c.CatSlug,
                    Activo = c.CatActivo.HasValue ? c.CatActivo.Value : true,
                    RutaCompleta = BuildCategoryPath(c)
                })
                .ToListAsync();
        }

        public async Task<CategoryStatsDto> GetCategoryStatsAsync()
        {
            var stats = await _context.Categorias
                .GroupBy(c => 1)
                .Select(g => new CategoryStatsDto
                {
                    TotalCategorias = g.Count(),
                    CategoriasActivas = g.Count(c => c.CatActivo == true),
                    CategoriasInactivas = g.Count(c => c.CatActivo == false),
                    CategoriasRaiz = g.Count(c => c.CatCategoriaPadreId == null),
                    CategoriasConHijos = g.Count(c => c.InverseCatCategoriaPadre.Any(sub => sub.CatActivo == true)),
                    UltimaCategoriaCreada = g.Max(c => c.CatFechaCreacion)
                })
                .FirstOrDefaultAsync() ?? new CategoryStatsDto();

            // Calcular categorías con y sin productos
            stats.CategoriasConProductos = await _context.Categorias
                .Where(c => c.Productos.Any(p => p.PrdActivo == true))
                .CountAsync();

            stats.CategoriasSinProductos = stats.TotalCategorias - stats.CategoriasConProductos;

            // Calcular niveles máximos
            stats.NivelesMaximos = await CalculateMaxLevelsAsync();

            // Top categorías por productos
            stats.TopCategoriasPorProductos = await GetTopCategoriesByProductsAsync();

            return stats;
        }

        private async Task<int> CalculateMaxLevelsAsync()
        {
            var maxLevel = 0;
            var rootCategories = await _context.Categorias
                .Where(c => c.CatCategoriaPadreId == null)
                .Select(c => c.CatId)
                .ToListAsync();

            foreach (var rootId in rootCategories)
            {
                var level = await GetMaxDepthAsync(rootId, 0);
                maxLevel = Math.Max(maxLevel, level);
            }

            return maxLevel;
        }

        private async Task<int> GetMaxDepthAsync(int categoryId, int currentDepth)
        {
            var childrenIds = await _context.Categorias
                .Where(c => c.CatCategoriaPadreId == categoryId)
                .Select(c => c.CatId)
                .ToListAsync();

            if (!childrenIds.Any())
                return currentDepth;

            var maxChildDepth = currentDepth;
            foreach (var childId in childrenIds)
            {
                var childDepth = await GetMaxDepthAsync(childId, currentDepth + 1);
                maxChildDepth = Math.Max(maxChildDepth, childDepth);
            }

            return maxChildDepth;
        }

        private async Task<List<CategoryProductCountDto>> GetTopCategoriesByProductsAsync()
        {
            return await _context.Categorias
                .Where(c => c.CatActivo == true)
                .Select(c => new CategoryProductCountDto
                {
                    Id = c.CatId,
                    Nombre = c.CatNombre,
                    ProductosDirectos = c.Productos.Count(p => p.PrdActivo == true),
                    TotalProductos = c.Productos.Count(p => p.PrdActivo == true)
                })
                .OrderByDescending(c => c.TotalProductos)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryProductCountDto>> GetCategoriesWithProductCountAsync(bool includeSubcategories = true)
        {
            var categories = await _context.Categorias
                .Where(c => c.CatActivo == true)
                .Select(c => new CategoryProductCountDto
                {
                    Id = c.CatId,
                    Nombre = c.CatNombre,
                    ProductosDirectos = c.Productos.Count(p => p.PrdActivo == true),
                    TotalProductos = c.Productos.Count(p => p.PrdActivo == true)
                })
                .ToListAsync();

            if (includeSubcategories)
            {
                foreach (var category in categories)
                {
                    var subcategoryIds = await GetAllSubcategoryIdsAsync(category.Id);
                    var subcategoryProducts = await _context.Productos
                        .Where(p => subcategoryIds.Contains(p.PrdCategoriaId) && p.PrdActivo == true)
                        .CountAsync();

                    category.ProductosDeSubcategorias = subcategoryProducts;
                    category.TotalProductos += subcategoryProducts;
                }
            }

            return categories.OrderBy(c => c.Nombre);
        }

        public async Task<bool> ToggleCategoryStatusAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return false;

            categoria.CatActivo = !(categoria.CatActivo.HasValue ? categoria.CatActivo.Value : true);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> BulkToggleStatusAsync(List<int> categoryIds, bool active)
        {
            var categorias = await _context.Categorias
                .Where(c => categoryIds.Contains(c.CatId))
                .ToListAsync();

            foreach (var categoria in categorias)
            {
                categoria.CatActivo = active;
            }

            await _context.SaveChangesAsync();
            return categorias.Count;
        }

        public async Task<int> BulkDeleteAsync(List<int> categoryIds)
        {
            var categorias = await _context.Categorias
                .Include(c => c.InverseCatCategoriaPadre)
                .Include(c => c.Productos)
                .Where(c => categoryIds.Contains(c.CatId))
                .ToListAsync();

            var deletedCount = 0;

            foreach (var categoria in categorias)
            {
                var hasChildren = categoria.InverseCatCategoriaPadre.Any(sub => sub.CatActivo == true);
                var hasProducts = categoria.Productos.Any(p => p.PrdActivo == true);

                if (hasChildren || hasProducts)
                {
                    categoria.CatActivo = false;
                }
                else
                {
                    _context.Categorias.Remove(categoria);
                }
                deletedCount++;
            }

            await _context.SaveChangesAsync();
            return deletedCount;
        }

        public async Task<int> ReorderCategoriesAsync(List<int> categoryIds)
        {
            var reorderedCount = 0;
            for (int i = 0; i < categoryIds.Count; i++)
            {
                var categoria = await _context.Categorias.FindAsync(categoryIds[i]);
                if (categoria != null)
                {
                    categoria.CatOrden = i;
                    reorderedCount++;
                }
            }

            await _context.SaveChangesAsync();
            return reorderedCount;
        }
    }
}