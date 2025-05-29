using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Dtos.Products;
using TechGadgets.API.Models.Entities;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Services.Implementations;

/// <summary>
/// Servicio para la gestión de imágenes de productos
/// </summary>
public class ProductosImagenService : IProductosImagenService
{
    private readonly TechGadgetsDbContext _context;
    private readonly ILogger<ProductosImagenService> _logger;

    public ProductosImagenService(TechGadgetsDbContext context, ILogger<ProductosImagenService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Consultas Básicas

    public async Task<IEnumerable<ProductoImagenDto>> GetImagenesByProductoIdAsync(int productoId)
    {
        try
        {
            _logger.LogInformation("Obteniendo imágenes para producto {ProductoId}", productoId);

            var imagenes = await _context.ProductosImagenes
                .Include(pi => pi.PimProducto)
                .Include(pi => pi.PimVariante)
                .Where(pi => pi.PimProductoId == productoId)
                .OrderBy(pi => pi.PimOrden)
                .ThenBy(pi => pi.PimId)
                .Select(pi => new ProductoImagenDto
                {
                    Id = pi.PimId,
                    ProductoId = pi.PimProductoId,
                    VarianteId = pi.PimVarianteId,
                    Url = pi.PimUrl,
                    AltText = pi.PimTextoAlternativo,
                    EsPrincipal = pi.PimEsPrincipal ?? false,
                    Orden = pi.PimOrden ?? 0,
                    Activo = pi.PimActivo ?? true,
                    ProductoNombre = pi.PimProducto.PrdNombre,
                    VarianteNombre = pi.PimVariante != null ? $"Variante {pi.PimVariante.PvaId}" : null
                })
                .ToListAsync();

            _logger.LogInformation("Se encontraron {Count} imágenes para el producto {ProductoId}", imagenes.Count, productoId);
            return imagenes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imágenes del producto {ProductoId}", productoId);
            throw;
        }
    }

    public async Task<IEnumerable<ProductoImagenDto>> GetImagenesByVarianteIdAsync(int varianteId)
    {
        try
        {
            _logger.LogInformation("Obteniendo imágenes para variante {VarianteId}", varianteId);

            var imagenes = await _context.ProductosImagenes
                .Include(pi => pi.PimProducto)
                .Include(pi => pi.PimVariante)
                .Where(pi => pi.PimVarianteId == varianteId)
                .OrderBy(pi => pi.PimOrden)
                .ThenBy(pi => pi.PimId)
                .Select(pi => new ProductoImagenDto
                {
                    Id = pi.PimId,
                    ProductoId = pi.PimProductoId,
                    VarianteId = pi.PimVarianteId,
                    Url = pi.PimUrl,
                    AltText = pi.PimTextoAlternativo,
                    EsPrincipal = pi.PimEsPrincipal ?? false,
                    Orden = pi.PimOrden ?? 0,
                    Activo = pi.PimActivo ?? true,
                    ProductoNombre = pi.PimProducto.PrdNombre,
                    VarianteNombre = pi.PimVariante != null ? $"Variante {pi.PimVariante.PvaId}" : null
                })
                .ToListAsync();

            _logger.LogInformation("Se encontraron {Count} imágenes para la variante {VarianteId}", imagenes.Count, varianteId);
            return imagenes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imágenes de la variante {VarianteId}", varianteId);
            throw;
        }
    }

    public async Task<ProductoImagenDto?> GetImagenByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Obteniendo imagen {ImagenId}", id);

            var imagen = await _context.ProductosImagenes
                .Include(pi => pi.PimProducto)
                .Include(pi => pi.PimVariante)
                .Where(pi => pi.PimId == id)
                .Select(pi => new ProductoImagenDto
                {
                    Id = pi.PimId,
                    ProductoId = pi.PimProductoId,
                    VarianteId = pi.PimVarianteId,
                    Url = pi.PimUrl,
                    AltText = pi.PimTextoAlternativo,
                    EsPrincipal = pi.PimEsPrincipal ?? false,
                    Orden = pi.PimOrden ?? 0,
                    Activo = pi.PimActivo ?? true,
                    ProductoNombre = pi.PimProducto.PrdNombre,
                    VarianteNombre = pi.PimVariante != null ? $"Variante {pi.PimVariante.PvaId}" : null
                })
                .FirstOrDefaultAsync();

            if (imagen != null)
            {
                _logger.LogInformation("Imagen {ImagenId} encontrada", id);
            }
            else
            {
                _logger.LogWarning("Imagen {ImagenId} no encontrada", id);
            }

            return imagen;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imagen {ImagenId}", id);
            throw;
        }
    }

    public async Task<ProductoImagenDto?> GetImagenPrincipalByProductoIdAsync(int productoId)
    {
        try
        {
            _logger.LogInformation("Obteniendo imagen principal para producto {ProductoId}", productoId);

            var imagen = await _context.ProductosImagenes
                .Include(pi => pi.PimProducto)
                .Include(pi => pi.PimVariante)
                .Where(pi => pi.PimProductoId == productoId && pi.PimEsPrincipal == true && pi.PimActivo == true)
                .OrderBy(pi => pi.PimOrden)
                .Select(pi => new ProductoImagenDto
                {
                    Id = pi.PimId,
                    ProductoId = pi.PimProductoId,
                    VarianteId = pi.PimVarianteId,
                    Url = pi.PimUrl,
                    AltText = pi.PimTextoAlternativo,
                    EsPrincipal = pi.PimEsPrincipal ?? false,
                    Orden = pi.PimOrden ?? 0,
                    Activo = pi.PimActivo ?? true,
                    ProductoNombre = pi.PimProducto.PrdNombre,
                    VarianteNombre = pi.PimVariante != null ? $"Variante {pi.PimVariante.PvaId}" : null
                })
                .FirstOrDefaultAsync();

            // Si no hay imagen principal, obtener la primera imagen activa
            if (imagen == null)
            {
                imagen = await _context.ProductosImagenes
                    .Include(pi => pi.PimProducto)
                    .Include(pi => pi.PimVariante)
                    .Where(pi => pi.PimProductoId == productoId && pi.PimActivo == true)
                    .OrderBy(pi => pi.PimOrden)
                    .ThenBy(pi => pi.PimId)
                    .Select(pi => new ProductoImagenDto
                    {
                        Id = pi.PimId,
                        ProductoId = pi.PimProductoId,
                        VarianteId = pi.PimVarianteId,
                        Url = pi.PimUrl,
                        AltText = pi.PimTextoAlternativo,
                        EsPrincipal = pi.PimEsPrincipal ?? false,
                        Orden = pi.PimOrden ?? 0,
                        Activo = pi.PimActivo ?? true,
                        ProductoNombre = pi.PimProducto.PrdNombre,
                        VarianteNombre = pi.PimVariante != null ? $"Variante {pi.PimVariante.PvaId}" : null
                    })
                    .FirstOrDefaultAsync();
            }

            return imagen;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imagen principal del producto {ProductoId}", productoId);
            throw;
        }
    }

    #endregion

    #region Operaciones CRUD

    public async Task<ProductoImagenDto> CreateImagenAsync(CreateProductoImagenDto createDto)
    {
        try
        {
            _logger.LogInformation("Creando nueva imagen para producto {ProductoId}", createDto.ProductoId);

            // Verificar que el producto existe
            var productoExists = await _context.Productos.AnyAsync(p => p.PrdId == createDto.ProductoId);
            if (!productoExists)
            {
                throw new ArgumentException($"El producto con ID {createDto.ProductoId} no existe");
            }

            // Verificar que la variante existe si se proporciona
            if (createDto.VarianteId.HasValue)
            {
                var varianteExists = await _context.ProductosVariantes.AnyAsync(v => v.PvaId == createDto.VarianteId.Value);
                if (!varianteExists)
                {
                    throw new ArgumentException($"La variante con ID {createDto.VarianteId} no existe");
                }
            }

            // Verificar URL duplicada
            var urlExists = await ExistsImagenByUrlAsync(createDto.Url);
            if (urlExists)
            {
                throw new ArgumentException($"Ya existe una imagen con la URL {createDto.Url}");
            }

            // Si se marca como principal, desmarcar otras imágenes principales
            if (createDto.EsPrincipal)
            {
                await UnsetImagenPrincipalAsync(createDto.ProductoId, createDto.VarianteId);
            }

            var imagen = new ProductosImagene
            {
                PimProductoId = createDto.ProductoId,
                PimVarianteId = createDto.VarianteId,
                PimUrl = createDto.Url,
                PimTextoAlternativo = createDto.AltText,
                PimEsPrincipal = createDto.EsPrincipal,
                PimOrden = createDto.Orden,
                PimActivo = createDto.Activo
            };

            _context.ProductosImagenes.Add(imagen);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Imagen creada con ID {ImagenId} para producto {ProductoId}", imagen.PimId, createDto.ProductoId);

            // Retornar el DTO con la información completa
            var imagenCreada = await GetImagenByIdAsync(imagen.PimId);
            return imagenCreada ?? throw new InvalidOperationException("Error al obtener la imagen creada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear imagen para producto {ProductoId}", createDto.ProductoId);
            throw;
        }
    }

    public async Task<ProductoImagenDto?> UpdateImagenAsync(int id, UpdateProductoImagenDto updateDto)
    {
        try
        {
            _logger.LogInformation("Actualizando imagen {ImagenId}", id);

            var imagen = await _context.ProductosImagenes.FindAsync(id);
            if (imagen == null)
            {
                _logger.LogWarning("Imagen {ImagenId} no encontrada para actualizar", id);
                return null;
            }

            // Verificar URL duplicada si se está cambiando
            if (!string.IsNullOrEmpty(updateDto.Url) && updateDto.Url != imagen.PimUrl)
            {
                var urlExists = await ExistsImagenByUrlAsync(updateDto.Url, id);
                if (urlExists)
                {
                    throw new ArgumentException($"Ya existe una imagen con la URL {updateDto.Url}");
                }
            }

            // Si se marca como principal, desmarcar otras imágenes principales
            if (updateDto.EsPrincipal)
            {
                await UnsetImagenPrincipalAsync(imagen.PimProductoId, imagen.PimVarianteId);
            }

            // Actualizar campos
            imagen.PimUrl = updateDto.Url;
            imagen.PimTextoAlternativo = updateDto.AltText;
            imagen.PimEsPrincipal = updateDto.EsPrincipal;
            imagen.PimOrden = updateDto.Orden;
            imagen.PimActivo = updateDto.Activo;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Imagen {ImagenId} actualizada correctamente", id);

            return await GetImagenByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar imagen {ImagenId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteImagenAsync(int id)
    {
        try
        {
            _logger.LogInformation("Eliminando imagen {ImagenId}", id);

            var imagen = await _context.ProductosImagenes.FindAsync(id);
            if (imagen == null)
            {
                _logger.LogWarning("Imagen {ImagenId} no encontrada para eliminar", id);
                return false;
            }

            // Si es la imagen principal, quitar la marca antes de eliminar
            if (imagen.PimEsPrincipal == true)
            {
                imagen.PimEsPrincipal = false;
                await _context.SaveChangesAsync();
            }

            _context.ProductosImagenes.Remove(imagen);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Imagen {ImagenId} eliminada correctamente", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar imagen {ImagenId}", id);
            throw;
        }
    }

    #endregion

    #region Operaciones Masivas y Especiales

    public async Task<IEnumerable<ProductoImagenDto>> CreateMultipleImagenesAsync(int productoId, IEnumerable<CreateProductoImagenDto> imagenes)
    {
        try
        {
            _logger.LogInformation("Creando múltiples imágenes para producto {ProductoId}", productoId);

            var imagenesCreadas = new List<ProductoImagenDto>();
            var imagenesList = imagenes.ToList();

            // Verificar que solo hay una imagen principal
            var imagenesPrincipales = imagenesList.Count(i => i.EsPrincipal);
            if (imagenesPrincipales > 1)
            {
                throw new ArgumentException("Solo puede haber una imagen principal");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var imagenDto in imagenesList)
                {
                    imagenDto.ProductoId = productoId; // Asegurar que el ProductoId esté correcto
                    var imagenCreada = await CreateImagenAsync(imagenDto);
                    imagenesCreadas.Add(imagenCreada);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Se crearon {Count} imágenes para el producto {ProductoId}", imagenesCreadas.Count, productoId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return imagenesCreadas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear múltiples imágenes para producto {ProductoId}", productoId);
            throw;
        }
    }

    public async Task<IEnumerable<ProductoImagenDto>> UpdateMultipleImagenesAsync(int productoId, IEnumerable<UpdateProductoImagenDto> imagenes)
    {
        try
        {
            _logger.LogInformation("Actualizando múltiples imágenes para producto {ProductoId}", productoId);

            var imagenesResultado = new List<ProductoImagenDto>();
            var imagenesList = imagenes.ToList();

            // Verificar que solo hay una imagen principal
            var imagenesPrincipales = imagenesList.Count(i => i.EsPrincipal && !i.Eliminar);
            if (imagenesPrincipales > 1)
            {
                throw new ArgumentException("Solo puede haber una imagen principal");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var imagenDto in imagenesList)
                {
                    if (imagenDto.Eliminar && imagenDto.Id.HasValue)
                    {
                        // Eliminar imagen
                        await DeleteImagenAsync(imagenDto.Id.Value);
                    }
                    else if (imagenDto.Id.HasValue)
                    {
                        // Actualizar imagen existente
                        var imagenActualizada = await UpdateImagenAsync(imagenDto.Id.Value, imagenDto);
                        if (imagenActualizada != null)
                        {
                            imagenesResultado.Add(imagenActualizada);
                        }
                    }
                    else if (!imagenDto.Eliminar)
                    {
                        // Crear nueva imagen
                        var createDto = new CreateProductoImagenDto
                        {
                            ProductoId = productoId,
                            Url = imagenDto.Url,
                            AltText = imagenDto.AltText,
                            EsPrincipal = imagenDto.EsPrincipal,
                            Orden = imagenDto.Orden,
                            Activo = imagenDto.Activo
                        };
                        var imagenCreada = await CreateImagenAsync(createDto);
                        imagenesResultado.Add(imagenCreada);
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Se procesaron múltiples imágenes para el producto {ProductoId}", productoId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return imagenesResultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar múltiples imágenes para producto {ProductoId}", productoId);
            throw;
        }
    }

    public async Task<bool> UpdateOrdenImagenesAsync(int productoId, IEnumerable<UpdateOrdenImagenDto> ordenImagenes)
    {
        try
        {
            _logger.LogInformation("Actualizando orden de imágenes para producto {ProductoId}", productoId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var ordenDto in ordenImagenes)
                {
                    var imagen = await _context.ProductosImagenes
                        .Where(pi => pi.PimId == ordenDto.Id && pi.PimProductoId == productoId)
                        .FirstOrDefaultAsync();

                    if (imagen != null)
                    {
                        imagen.PimOrden = ordenDto.Orden;
                    }
                    else
                    {
                        _logger.LogWarning("Imagen {ImagenId} no encontrada para producto {ProductoId}", ordenDto.Id, productoId);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Orden de imágenes actualizado para producto {ProductoId}", productoId);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar orden de imágenes para producto {ProductoId}", productoId);
            throw;
        }
    }

    public async Task<bool> SetImagenPrincipalAsync(int productoId, int imagenId)
    {
        try
        {
            _logger.LogInformation("Estableciendo imagen {ImagenId} como principal para producto {ProductoId}", imagenId, productoId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verificar que la imagen existe y pertenece al producto
                var imagen = await _context.ProductosImagenes
                    .Where(pi => pi.PimId == imagenId && pi.PimProductoId == productoId)
                    .FirstOrDefaultAsync();

                if (imagen == null)
                {
                    _logger.LogWarning("Imagen {ImagenId} no encontrada para producto {ProductoId}", imagenId, productoId);
                    return false;
                }

                // Desmarcar todas las imágenes principales del producto
                await UnsetImagenPrincipalAsync(productoId, imagen.PimVarianteId);

                // Marcar la imagen específica como principal
                imagen.PimEsPrincipal = true;
                imagen.PimActivo = true; // Asegurar que esté activa si es principal

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Imagen {ImagenId} establecida como principal para producto {ProductoId}", imagenId, productoId);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al establecer imagen principal {ImagenId} para producto {ProductoId}", imagenId, productoId);
            throw;
        }
    }

    public async Task<bool> ToggleImagenStatusAsync(int id)
    {
        try
        {
            _logger.LogInformation("Cambiando estado de imagen {ImagenId}", id);

            var imagen = await _context.ProductosImagenes.FindAsync(id);
            if (imagen == null)
            {
                _logger.LogWarning("Imagen {ImagenId} no encontrada para cambiar estado", id);
                return false;
            }

            var estadoAnterior = imagen.PimActivo ?? true;
            imagen.PimActivo = !estadoAnterior;

            // Si se desactiva una imagen principal, quitarle la marca de principal
            if (!imagen.PimActivo.Value && imagen.PimEsPrincipal == true)
            {
                imagen.PimEsPrincipal = false;
                _logger.LogInformation("Imagen principal {ImagenId} desactivada, se removió la marca de principal", id);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Estado de imagen {ImagenId} cambiado de {EstadoAnterior} a {EstadoNuevo}", 
                id, estadoAnterior, imagen.PimActivo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de imagen {ImagenId}", id);
            throw;
        }
    }

    public async Task<int> DeleteAllImagenesByProductoIdAsync(int productoId)
    {
        try
        {
            _logger.LogInformation("Eliminando todas las imágenes del producto {ProductoId}", productoId);

            var imagenes = await _context.ProductosImagenes
                .Where(pi => pi.PimProductoId == productoId)
                .ToListAsync();

            var count = imagenes.Count;
            
            if (count > 0)
            {
                _context.ProductosImagenes.RemoveRange(imagenes);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Se eliminaron {Count} imágenes del producto {ProductoId}", count, productoId);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar todas las imágenes del producto {ProductoId}", productoId);
            throw;
        }
    }

    #endregion

    #region Validaciones y Utilidades

    public async Task<bool> ExistsImagenByUrlAsync(string url, int? excludeId = null)
    {
        try
        {
            var query = _context.ProductosImagenes.Where(pi => pi.PimUrl == url);
            
            if (excludeId.HasValue)
            {
                query = query.Where(pi => pi.PimId != excludeId.Value);
            }

            var exists = await query.AnyAsync();
            _logger.LogDebug("Verificación de URL {Url}: {Exists}", url, exists ? "Existe" : "No existe");
            
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar si existe imagen con URL {Url}", url);
            throw;
        }
    }

    public async Task<bool> ProductoHasImagenesAsync(int productoId)
    {
        try
        {
            var hasImages = await _context.ProductosImagenes
                .AnyAsync(pi => pi.PimProductoId == productoId && pi.PimActivo == true);
            
            _logger.LogDebug("Producto {ProductoId} tiene imágenes: {HasImages}", productoId, hasImages);
            return hasImages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar si producto {ProductoId} tiene imágenes", productoId);
            throw;
        }
    }

    public async Task<ProductoImagenStatsDto> GetImagenStatsAsync(int productoId)
    {
        try
        {
            _logger.LogInformation("Obteniendo estadísticas de imágenes para producto {ProductoId}", productoId);

            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                throw new ArgumentException($"Producto con ID {productoId} no encontrado");
            }

            var imagenes = await GetImagenesByProductoIdAsync(productoId);
            var imagenPrincipal = await GetImagenPrincipalByProductoIdAsync(productoId);
            var imagenesList = imagenes.ToList();

            var stats = new ProductoImagenStatsDto
            {
                ProductoId = productoId,
                ProductoNombre = producto.PrdNombre,
                TotalImagenes = imagenesList.Count,
                ImagenesActivas = imagenesList.Count(i => i.Activo),
                ImagenesInactivas = imagenesList.Count(i => !i.Activo),
                TieneImagenPrincipal = imagenPrincipal != null,
                ImagenPrincipal = imagenPrincipal,
                ImagenesVariantes = imagenesList.Where(i => i.VarianteId.HasValue)
            };

            _logger.LogInformation("Estadísticas generadas para producto {ProductoId}: {Total} total, {Activas} activas, {Principal} principal", 
                productoId, stats.TotalImagenes, stats.ImagenesActivas, stats.TieneImagenPrincipal);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de imágenes para producto {ProductoId}", productoId);
            throw;
        }
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Desmarca todas las imágenes principales de un producto o variante
    /// </summary>
    private async Task UnsetImagenPrincipalAsync(int productoId, int? varianteId)
    {
        try
        {
            var query = _context.ProductosImagenes.Where(pi => pi.PimProductoId == productoId);
            
            if (varianteId.HasValue)
            {
                query = query.Where(pi => pi.PimVarianteId == varianteId.Value);
            }
            else
            {
                query = query.Where(pi => pi.PimVarianteId == null);
            }

            var imagenesActuales = await query.Where(pi => pi.PimEsPrincipal == true).ToListAsync();
            
            if (imagenesActuales.Any())
            {
                foreach (var imagen in imagenesActuales)
                {
                    imagen.PimEsPrincipal = false;
                }

                _logger.LogDebug("Se desmarcaron {Count} imágenes principales para producto {ProductoId}", 
                    imagenesActuales.Count, productoId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desmarcar imágenes principales para producto {ProductoId}", productoId);
            throw;
        }
    }

    #endregion
}