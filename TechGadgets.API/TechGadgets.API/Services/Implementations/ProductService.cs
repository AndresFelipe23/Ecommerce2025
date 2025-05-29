using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Dtos.Products;
using TechGadgets.API.Models.Common;
using TechGadgets.API.Models.Entities;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly TechGadgetsDbContext _context;
        private readonly ISlugService _slugService;
        private readonly IProductosImagenService _imageService; // ✅ Inyectar servicio de imágenes

        public ProductService(
            TechGadgetsDbContext context,
            ISlugService slugService,
            IProductosImagenService imageService) // ✅ Agregar dependencia
        {
            _context = context;
            _slugService = slugService;
            _imageService = imageService; // ✅ Inicializar
        }

        #region CRUD Básico

        public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - GetProductsAsync iniciado");
                Console.WriteLine($"   Filtro Activo: {filter.Activo}");
                Console.WriteLine($"   Page: {filter.Page}, PageSize: {filter.PageSize}");

                var query = BuildProductQuery(filter);

                // ✅ CONTAR PRIMERO
                var totalItems = await query.CountAsync();
                Console.WriteLine($"📊 Total items encontrados: {totalItems}");

                if (totalItems == 0)
                {
                    Console.WriteLine("⚠️ No se encontraron productos con los filtros aplicados");
                    return new PagedResult<ProductDto>
                    {
                        Items = new List<ProductDto>(),
                        TotalItems = 0,
                        Page = filter.Page,
                        PageSize = filter.PageSize
                    };
                }

                // ✅ CARGAR PRODUCTOS CON PAGINACIÓN
                var products = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                Console.WriteLine($"📦 Productos cargados de BD: {products.Count}");

                // ✅ MAPEAR A DTO
                var productDtos = new List<ProductDto>();
                foreach (var product in products)
                {
                    var dto = await MapToProductDtoAsync(product); // ✅ Mapeo asíncrono
                    productDtos.Add(dto);
                }

                var result = new PagedResult<ProductDto>
                {
                    Items = productDtos,
                    TotalItems = totalItems,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                Console.WriteLine($"✅ SERVICE - Resultado final preparado");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en SERVICE GetProductsAsync: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - GetProductByIdAsync: Buscando ID {id}");

                var product = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .FirstOrDefaultAsync(p => p.PrdId == id);

                if (product == null)
                {
                    Console.WriteLine($"❌ SERVICE - Producto {id} no encontrado en BD");
                    return null;
                }

                Console.WriteLine($"✅ SERVICE - Producto {id} encontrado en BD");
                return await MapToProductDtoAsync(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetProductByIdAsync {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<ProductDto?> GetProductBySlugAsync(string slug)
        {
            var product = await _context.Productos
                .Include(p => p.PrdCategoria)
                .Include(p => p.PrdMarca)
                .Include(p => p.Inventarios)
                .FirstOrDefaultAsync(p => p.PrdSlug == slug && p.PrdActivo == true);

            return product != null ? await MapToProductDtoAsync(product) : null;
        }

        public async Task<bool> ProductExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _context.Productos.Where(p => p.PrdNombre == name);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(p => p.PrdId != excludeId.Value);
                }

                var exists = await query.AnyAsync();
                Console.WriteLine($"🔍 ProductExistsAsync - Nombre: '{name}', Existe: {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ProductExistsAsync: {ex.Message}");
                throw;
            }
        }

public async Task<bool> SKUExistsAsync(string sku, int? excludeId = null)
{
    try
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return false;
        }

        var query = _context.Productos.Where(p => p.PrdSku == sku);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.PrdId != excludeId.Value);
        }

        var exists = await query.AnyAsync();
        Console.WriteLine($"🔍 SKUExistsAsync - SKU: '{sku}', ExcludeId: {excludeId}, Existe: {exists}");
        return exists;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error en SKUExistsAsync: {ex.Message}");
        throw;
    }
}

public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
{
    try
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return false;
        }

        var query = _context.Productos.Where(p => p.PrdSlug == slug);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.PrdId != excludeId.Value);
        }

        var exists = await query.AnyAsync();
        Console.WriteLine($"🔍 SlugExistsAsync - Slug: '{slug}', ExcludeId: {excludeId}, Existe: {exists}");
        return exists;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error en SlugExistsAsync: {ex.Message}");
        throw;
    }
}

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validar unicidad
                if (await SKUExistsAsync(dto.SKU))
                    throw new InvalidOperationException("El SKU ya existe");

                // Generar slug si no se proporciona
                var slug = !string.IsNullOrEmpty(dto.Slug)
                    ? dto.Slug
                    : await _slugService.GenerateSlugAsync(dto.Nombre, "productos");

                if (await SlugExistsAsync(slug))
                    throw new InvalidOperationException("El slug ya existe");

                var product = new Producto
                {
                    PrdSku = dto.SKU,
                    PrdNombre = dto.Nombre,
                    PrdDescripcionCorta = dto.DescripcionCorta,
                    PrdDescripcionLarga = dto.DescripcionLarga,
                    PrdSlug = slug,
                    PrdPrecio = dto.Precio,
                    PrdPrecioComparacion = dto.PrecioComparacion,
                    PrdCosto = dto.Costo,
                    PrdCategoriaId = dto.CategoriaId,
                    PrdMarcaId = dto.MarcaId,
                    PrdTipo = dto.Tipo ?? "simple",
                    PrdEstado = dto.Estado ?? "disponible",
                    PrdDestacado = dto.Destacado,
                    PrdNuevo = dto.Nuevo,
                    PrdEnOferta = dto.EnOferta,
                    PrdPeso = dto.Peso,
                    PrdDimensiones = dto.Dimensiones,
                    PrdMetaTitulo = dto.MetaTitulo,
                    PrdMetaDescripcion = dto.MetaDescripcion,
                    PrdPalabrasClaves = dto.PalabrasClaves,
                    PrdRequiereEnvio = dto.RequiereEnvio,
                    PrdPermiteReseñas = dto.PermiteReseñas,
                    PrdGarantia = dto.Garantia,
                    PrdOrden = dto.Orden,
                    PrdActivo = true,
                    PrdFechaCreacion = DateTime.UtcNow
                };

                _context.Productos.Add(product);
                await _context.SaveChangesAsync();

                // ✅ CREAR IMÁGENES USANDO EL SERVICIO DEDICADO
                if (dto.Imagenes?.Any() == true)
                {
                    var imagenesDto = dto.Imagenes.Select(img => new CreateProductoImagenDto
                    {
                        ProductoId = product.PrdId,
                        Url = img.Url,
                        AltText = img.AltText,
                        EsPrincipal = img.EsPrincipal,
                        Orden = img.Orden,
                        Activo = true
                    });

                    await _imageService.CreateMultipleImagenesAsync(product.PrdId, imagenesDto);
                }

                // Crear inventario inicial
                if (dto.StockInicial > 0)
                {
                    var inventario = new Inventario
                    {
                        InvProductoId = product.PrdId,
                        InvStock = dto.StockInicial,
                        InvStockReservado = 0,
                        InvStockMinimo = 5,
                        InvFechaUltimaActualizacion = DateTime.UtcNow
                    };
                    _context.Inventarios.Add(inventario);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return await GetProductByIdAsync(product.PrdId) ??
                       throw new InvalidOperationException("Error al crear el producto");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ✅ CONTINUAR CON LOS DEMÁS MÉTODOS...
        // [Los demás métodos CRUD, validaciones, etc. siguen igual]

        #endregion

        #region Gestión de Imágenes (Delegada)

        public async Task<bool> ProductHasImagesAsync(int productId)
        {
            return await _imageService.ProductoHasImagenesAsync(productId);
        }

        public async Task<string?> GetProductMainImageUrlAsync(int productId)
        {
            var mainImage = await _imageService.GetImagenPrincipalByProductoIdAsync(productId);
            return mainImage?.Url;
        }

        public async Task<IEnumerable<ProductImageDto>> GetProductImagesAsync(int productId)
        {
            var images = await _imageService.GetImagenesByProductoIdAsync(productId);
            // ✅ Mapear de ProductoImagenDto a ProductImageDto si es necesario
            return images.Select(img => new ProductImageDto
            {
                Id = img.Id,
                Url = img.Url,
                AltText = img.AltText,
                EsPrincipal = img.EsPrincipal,
                Orden = img.Orden
            });
        }

        #endregion

        #region Métodos Privados

        private IQueryable<Producto> BuildProductQuery(ProductFilterDto filter)
        {
            Console.WriteLine($"🔧 BuildProductQuery - Construyendo query");

            // ✅ NO incluir imágenes aquí - se cargan por separado
            var query = _context.Productos
                .Include(p => p.PrdCategoria)
                .Include(p => p.PrdMarca)
                .Include(p => p.Inventarios)
                .AsQueryable();

            Console.WriteLine($"   Query base creado con includes básicos");

            // ✅ Filtros
            if (filter.Activo.HasValue)
            {
                query = query.Where(p => p.PrdActivo == filter.Activo.Value);
            }

            if (!string.IsNullOrEmpty(filter.Busqueda))
            {
                var search = filter.Busqueda.ToLower();
                query = query.Where(p =>
                    p.PrdNombre.ToLower().Contains(search) ||
                    (p.PrdDescripcionCorta != null && p.PrdDescripcionCorta.ToLower().Contains(search)) ||
                    p.PrdSku.ToLower().Contains(search));
            }

            // ✅ Ordenamiento
            query = filter.SortBy?.ToLower() switch
            {
                "nombre" => filter.SortDescending
                    ? query.OrderByDescending(p => p.PrdNombre)
                    : query.OrderBy(p => p.PrdNombre),
                "precio" => filter.SortDescending
                    ? query.OrderByDescending(p => p.PrdPrecio)
                    : query.OrderBy(p => p.PrdPrecio),
                "fecha" => filter.SortDescending
                    ? query.OrderByDescending(p => p.PrdFechaCreacion)
                    : query.OrderBy(p => p.PrdFechaCreacion),
                _ => query.OrderBy(p => p.PrdNombre)
            };

            return query;
        }

        // ✅ MAPEO ASÍNCRONO CON SERVICIO DE IMÁGENES
        private async Task<ProductDto> MapToProductDtoAsync(Producto product)
        {
            try
            {
                if (product == null)
                {
                    throw new ArgumentNullException(nameof(product));
                }

                var inventario = product.Inventarios?.FirstOrDefault();

                // ✅ OBTENER IMÁGENES USANDO EL SERVICIO DEDICADO
                var imagenes = await _imageService.GetImagenesByProductoIdAsync(product.PrdId);
                var imagenesList = imagenes.ToList();

                var dto = new ProductDto
                {
                    // Datos básicos
                    Id = product.PrdId,
                    SKU = product.PrdSku ?? string.Empty,
                    Nombre = product.PrdNombre ?? "Sin nombre",
                    DescripcionCorta = product.PrdDescripcionCorta,
                    DescripcionLarga = product.PrdDescripcionLarga,
                    Slug = product.PrdSlug ?? string.Empty,

                    // Precios
                    Precio = product.PrdPrecio,
                    PrecioComparacion = product.PrdPrecioComparacion,
                    Costo = product.PrdCosto,

                    // Propiedades booleanas
                    Activo = product.PrdActivo ?? false,
                    Destacado = product.PrdDestacado ?? false,
                    Nuevo = product.PrdNuevo ?? false,
                    EnOferta = product.PrdEnOferta ?? false,
                    RequiereEnvio = product.PrdRequiereEnvio ?? true,
                    PermiteReseñas = product.PrdPermiteReseñas ?? true,

                    // Otros campos
                    Tipo = product.PrdTipo ?? "simple",
                    Estado = product.PrdEstado ?? "disponible",
                    Peso = product.PrdPeso,
                    Dimensiones = product.PrdDimensiones,
                    Garantia = product.PrdGarantia,
                    Orden = product.PrdOrden ?? 0,

                    // Fechas
                    FechaCreacion = product.PrdFechaCreacion ?? DateTime.MinValue,
                    FechaModificacion = product.PrdFechaModificacion,

                    // SEO
                    MetaTitulo = product.PrdMetaTitulo,
                    MetaDescripcion = product.PrdMetaDescripcion,
                    PalabrasClaves = product.PrdPalabrasClaves,

                    // Relaciones
                    CategoriaId = product.PrdCategoriaId,
                    CategoriaNombre = product.PrdCategoria?.CatNombre ?? "Sin categoría",
                    CategoriaRuta = product.PrdCategoria?.CatSlug ?? string.Empty,

                    MarcaId = product.PrdMarcaId,
                    MarcaNombre = product.PrdMarca?.MarNombre ?? "Sin marca",
                    MarcaLogo = product.PrdMarca?.MarLogo,

                    // Inventario
                    StockActual = inventario?.InvStock ?? 0,
                    StockReservado = inventario?.InvStockReservado ?? 0,

                    // ✅ IMÁGENES DESDE EL SERVICIO DEDICADO
                    Imagenes = imagenesList.Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        Url = i.Url ?? string.Empty,
                        AltText = i.AltText,
                        EsPrincipal = i.EsPrincipal,
                        Orden = i.Orden
                    }).ToList(),

                    ImagenPrincipal = imagenesList.FirstOrDefault(i => i.EsPrincipal)?.Url
                                     ?? imagenesList.FirstOrDefault()?.Url
                };

                Console.WriteLine($"✅ Mapeo exitoso: {dto.Nombre} (ID: {dto.Id}) con {dto.Imagenes.Count} imágenes");
                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en MapToProductDtoAsync: {ex.Message}");
                throw;
            }
        }

        #region Gestión de Imágenes de Productos

        /// <summary>
        /// Elimina una imagen específica de un producto
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="imageId">ID de la imagen a eliminar</param>
        /// <returns>True si se eliminó exitosamente</returns>
        public async Task<bool> DeleteProductImageAsync(int productId, int imageId)
        {
            try
            {
                Console.WriteLine($"🗑️ SERVICE - DeleteProductImageAsync: Producto {productId}, Imagen {imageId}");

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // ✅ 1. VERIFICAR QUE EL PRODUCTO EXISTS
                    var product = await _context.Productos
                        .Include(p => p.ProductosImagenes)
                        .FirstOrDefaultAsync(p => p.PrdId == productId);

                    if (product == null)
                    {
                        Console.WriteLine($"❌ SERVICE - Producto {productId} no encontrado");
                        return false;
                    }

                    // ✅ 2. VERIFICAR QUE LA IMAGEN EXISTS Y PERTENECE AL PRODUCTO
                    var imageToDelete = product.ProductosImagenes?.FirstOrDefault(img => img.PimId == imageId);
                    if (imageToDelete == null)
                    {
                        Console.WriteLine($"❌ SERVICE - Imagen {imageId} no encontrada en producto {productId}");
                        return false;
                    }

                    // ✅ 3. VERIFICAR QUE NO SEA LA ÚNICA IMAGEN (REGLA DE NEGOCIO)
                    var totalImages = product.ProductosImagenes?.Count ?? 0;
                    if (totalImages <= 1)
                    {
                        Console.WriteLine($"⚠️ SERVICE - No se puede eliminar la única imagen del producto {productId}");
                        throw new InvalidOperationException("No se puede eliminar la única imagen del producto. Un producto debe tener al menos una imagen.");
                    }

                    // ✅ 4. VERIFICAR SI ES LA IMAGEN PRINCIPAL
                    bool wasMainImage = imageToDelete.PimEsPrincipal ?? false;
                    Console.WriteLine($"📸 Imagen a eliminar - Principal: {wasMainImage}");

                    // ✅ 5. ELIMINAR LA IMAGEN DE LA BASE DE DATOS
                    _context.ProductosImagenes.Remove(imageToDelete);
                    var saveResult = await _context.SaveChangesAsync();

                    if (saveResult <= 0)
                    {
                        Console.WriteLine($"❌ SERVICE - Error al eliminar imagen {imageId} de la BD");
                        await transaction.RollbackAsync();
                        return false;
                    }

                    Console.WriteLine($"✅ SERVICE - Imagen {imageId} eliminada de la BD");

                    // ✅ 6. SI ERA LA IMAGEN PRINCIPAL, ASIGNAR OTRA COMO PRINCIPAL
                    if (wasMainImage)
                    {
                        Console.WriteLine($"🔄 SERVICE - Reasignando imagen principal para producto {productId}");

                        var newMainImage = await _context.ProductosImagenes
                            .Where(img => img.PimProductoId == productId && img.PimActivo == true)
                            .OrderBy(img => img.PimOrden)
                            .ThenBy(img => img.PimId)
                            .FirstOrDefaultAsync();

                        if (newMainImage != null)
                        {
                            newMainImage.PimEsPrincipal = true;
                            await _context.SaveChangesAsync();
                            Console.WriteLine($"✅ SERVICE - Nueva imagen principal asignada: {newMainImage.PimId}");
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ SERVICE - No se encontró otra imagen para marcar como principal");
                        }
                    }

                    // ✅ 7. REORDENAR IMÁGENES RESTANTES
                    await ReorderProductImagesInternalAsync(productId);

                    // ✅ 8. ACTUALIZAR FECHA DE MODIFICACIÓN DEL PRODUCTO
                    product.PrdFechaModificacion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    Console.WriteLine($"✅ SERVICE - DeleteProductImageAsync completado exitosamente");
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"❌ SERVICE - Error en transacción: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en DeleteProductImageAsync: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene información de una imagen específica de un producto
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="imageId">ID de la imagen</param>
        /// <returns>Información de la imagen o null si no existe</returns>
        public async Task<ProductImageDto?> GetProductImageAsync(int productId, int imageId)
        {
            try
            {
                var image = await _context.ProductosImagenes
                    .FirstOrDefaultAsync(img => img.PimId == imageId && img.PimProductoId == productId);

                if (image == null)
                    return null;

                return new ProductImageDto
                {
                    Id = image.PimId,
                    Url = image.PimUrl ?? string.Empty,
                    AltText = image.PimTextoAlternativo,
                    EsPrincipal = image.PimEsPrincipal ?? false,
                    Orden = image.PimOrden ?? 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetProductImageAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Reordena las imágenes de un producto después de eliminar una
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>True si se reordenó exitosamente</returns>
        public async Task<bool> ReorderProductImagesAsync(int productId)
        {
            try
            {
                return await ReorderProductImagesInternalAsync(productId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en ReorderProductImagesAsync: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Métodos Privados de Imágenes

        /// <summary>
        /// Método interno para reordenar imágenes
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>True si se reordenó exitosamente</returns>
        private async Task<bool> ReorderProductImagesInternalAsync(int productId)
        {
            try
            {
                var images = await _context.ProductosImagenes
                    .Where(img => img.PimProductoId == productId)
                    .OrderBy(img => img.PimOrden)
                    .ThenBy(img => img.PimId)
                    .ToListAsync();

                for (int i = 0; i < images.Count; i++)
                {
                    images[i].PimOrden = i + 1;
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ SERVICE - Imágenes reordenadas para producto {productId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error reordenando imágenes: {ex.Message}");
                return false;
            }
        }



        
    }
}

