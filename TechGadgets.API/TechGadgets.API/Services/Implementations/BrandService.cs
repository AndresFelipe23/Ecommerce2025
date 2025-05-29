using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Dtos.Brands;
using TechGadgets.API.Models.Common;
using TechGadgets.API.Models.Entities;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Services.Implementations
{
    public class BrandService : IBrandService
    {
        private readonly TechGadgetsDbContext _context;

        public BrandService(TechGadgetsDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<BrandDto>> GetBrandsAsync(BrandFilterDto filter)
        {
            var query = _context.Marcas.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(filter.Nombre))
            {
                query = query.Where(m => m.MarNombre.Contains(filter.Nombre));
            }

            if (filter.Activo.HasValue)
            {
                query = query.Where(m => m.MarActivo == filter.Activo.Value);
            }

            if (filter.FechaDesde.HasValue)
            {
                query = query.Where(m => m.MarFechaCreacion >= filter.FechaDesde.Value);
            }

            if (filter.FechaHasta.HasValue)
            {
                query = query.Where(m => m.MarFechaCreacion <= filter.FechaHasta.Value);
            }

            // Aplicar ordenamiento
            query = filter.SortBy?.ToLower() switch
            {
                "nombre" => filter.SortDescending 
                    ? query.OrderByDescending(m => m.MarNombre)
                    : query.OrderBy(m => m.MarNombre),
                "fechacreacion" => filter.SortDescending 
                    ? query.OrderByDescending(m => m.MarFechaCreacion)
                    : query.OrderBy(m => m.MarFechaCreacion),
                "activo" => filter.SortDescending 
                    ? query.OrderByDescending(m => m.MarActivo)
                    : query.OrderBy(m => m.MarActivo),
                _ => query.OrderBy(m => m.MarNombre)
            };

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Include(m => m.Productos)
                .Select(m => new BrandDto
                {
                    Id = m.MarId,
                    Nombre = m.MarNombre,
                    Descripcion = m.MarDescripcion,
                    Logo = m.MarLogo,
                    SitioWeb = m.MarSitioWeb,
                    Activo = m.MarActivo ?? true,
                    FechaCreacion = m.MarFechaCreacion ?? DateTime.UtcNow,
                    TotalProductos = m.Productos.Count(p => p.PrdActivo == true)
                })
                .ToListAsync();

            return new PagedResult<BrandDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<BrandDto?> GetBrandByIdAsync(int id)
        {
            var marca = await _context.Marcas
                .Include(m => m.Productos)
                .FirstOrDefaultAsync(m => m.MarId == id);

            if (marca == null) return null;

            return new BrandDto
            {
                Id = marca.MarId,
                Nombre = marca.MarNombre,
                Descripcion = marca.MarDescripcion,
                Logo = marca.MarLogo,
                SitioWeb = marca.MarSitioWeb,
                Activo = marca.MarActivo ?? true,
                FechaCreacion = marca.MarFechaCreacion ?? DateTime.UtcNow,
                TotalProductos = marca.Productos.Count(p => p.PrdActivo == true)
            };
        }

        public async Task<BrandDto?> GetBrandByNameAsync(string name)
        {
            var marca = await _context.Marcas
                .Include(m => m.Productos)
                .FirstOrDefaultAsync(m => m.MarNombre.ToLower() == name.ToLower());

            if (marca == null) return null;

            return await GetBrandByIdAsync(marca.MarId);
        }

        public async Task<BrandDto> CreateBrandAsync(CreateBrandDto dto)
        {
            var marca = new Marca
            {
                MarNombre = dto.Nombre.Trim(),
                MarDescripcion = dto.Descripcion?.Trim(),
                MarLogo = dto.Logo?.Trim(),
                MarSitioWeb = dto.SitioWeb?.Trim(),
                MarActivo = true,
                MarFechaCreacion = DateTime.UtcNow
            };

            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();

            return await GetBrandByIdAsync(marca.MarId) 
                ?? throw new InvalidOperationException("Error al crear la marca");
        }

        public async Task<BrandDto?> UpdateBrandAsync(int id, UpdateBrandDto dto)
        {
            var marca = await _context.Marcas.FindAsync(id);
            if (marca == null) return null;

            marca.MarNombre = dto.Nombre.Trim();
            marca.MarDescripcion = dto.Descripcion?.Trim();
            marca.MarLogo = dto.Logo?.Trim();
            marca.MarSitioWeb = dto.SitioWeb?.Trim();
            marca.MarActivo = dto.Activo;

            await _context.SaveChangesAsync();
            return await GetBrandByIdAsync(id);
        }

        public async Task<bool> DeleteBrandAsync(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);
            if (marca == null) return false;

            // Verificar si tiene productos asociados
            var hasProducts = await _context.Productos
                .AnyAsync(p => p.PrdMarcaId == id && p.PrdActivo == true);

            if (hasProducts)
            {
                // Desactivar en lugar de eliminar
                marca.MarActivo = false;
            }
            else
            {
                // Eliminar si no tiene productos
                _context.Marcas.Remove(marca);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BrandExistsAsync(string name, int? excludeId = null)
        {
            return await _context.Marcas
                .AnyAsync(m => m.MarNombre.ToLower() == name.ToLower() && 
                          (excludeId == null || m.MarId != excludeId));
        }

        public async Task<bool> BrandHasProductsAsync(int id)
        {
            return await _context.Productos
                .AnyAsync(p => p.PrdMarcaId == id && p.PrdActivo == true);
        }

        public async Task<IEnumerable<BrandSummaryDto>> GetActiveBrandsAsync()
        {
            return await _context.Marcas
                .Where(m => m.MarActivo == true)
                .OrderBy(m => m.MarNombre)
                .Select(m => new BrandSummaryDto
                {
                    Id = m.MarId,
                    Nombre = m.MarNombre,
                    Logo = m.MarLogo,
                    Activo = m.MarActivo ?? true
                })
                .ToListAsync();
        }

        public async Task<BrandStatsDto> GetBrandStatsAsync()
        {
            var stats = await _context.Marcas
                .GroupBy(m => 1)
                .Select(g => new BrandStatsDto
                {
                    TotalMarcas = g.Count(),
                    MarcasActivas = g.Count(m => m.MarActivo == true),
                    MarcasInactivas = g.Count(m => m.MarActivo == false),
                    UltimaMarcaCreada = g.Max(m => m.MarFechaCreacion)
                })
                .FirstOrDefaultAsync() ?? new BrandStatsDto();

            // Contar marcas con y sin productos
            stats.MarcasConProductos = await _context.Marcas
                .Where(m => m.Productos.Any(p => p.PrdActivo == true))
                .CountAsync();

            stats.MarcasSinProductos = stats.TotalMarcas - stats.MarcasConProductos;

            // Top marcas por cantidad de productos
            stats.TopMarcasPorProductos = await _context.Marcas
                .Where(m => m.MarActivo == true)
                .Select(m => new BrandProductCountDto
                {
                    Id = m.MarId,
                    Nombre = m.MarNombre,
                    TotalProductos = m.Productos.Count(p => p.PrdActivo == true)
                })
                .OrderByDescending(b => b.TotalProductos)
                .Take(5)
                .ToListAsync();

            return stats;
        }

        public async Task<IEnumerable<BrandProductCountDto>> GetBrandsWithProductCountAsync()
        {
            return await _context.Marcas
                .Where(m => m.MarActivo == true)
                .Select(m => new BrandProductCountDto
                {
                    Id = m.MarId,
                    Nombre = m.MarNombre,
                    TotalProductos = m.Productos.Count(p => p.PrdActivo == true)
                })
                .OrderBy(b => b.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ToggleBrandStatusAsync(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);
            if (marca == null) return false;

            marca.MarActivo = !(marca.MarActivo ?? true);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> BulkToggleStatusAsync(List<int> brandIds, bool active)
        {
            var marcas = await _context.Marcas
                .Where(m => brandIds.Contains(m.MarId))
                .ToListAsync();

            foreach (var marca in marcas)
            {
                marca.MarActivo = active;
            }

            await _context.SaveChangesAsync();
            return marcas.Count;
        }
    }
}