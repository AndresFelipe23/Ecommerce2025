using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Dtos.Products;
using TechGadgets.API.Models.Common;
using TechGadgets.API.Models.Entities;
using TechGadgets.API.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using TechGadgets.API.Dtos.Categories;
using TechGadgets.API.Dtos.Brands;

namespace TechGadgets.API.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly TechGadgetsDbContext _context;
        private readonly ISlugService _slugService;
        private readonly IProductosImagenService _imageService;

        public ProductService(
            TechGadgetsDbContext context,
            ISlugService slugService,
            IProductosImagenService imageService)
        {
            _context = context;
            _slugService = slugService;
            _imageService = imageService;
        }

        /// <summary>
        /// Agrega múltiples imágenes a un producto existente
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="imageDtos">Lista de imágenes a agregar</param>
        /// <returns>Lista de imágenes creadas</returns>
        public async Task<List<ProductImageDto>> AddMultipleProductImagesAsync(int productId, List<CreateProductImageDto> imageDtos)
        {
            try
            {
                Console.WriteLine($"📸 SERVICE - AddMultipleProductImagesAsync: {imageDtos.Count} imágenes para producto {productId}");

                // ✅ 1. VERIFICAR QUE EL PRODUCTO EXISTE
                var product = await _context.Productos.FindAsync(productId);
                if (product == null)
                {
                    throw new ArgumentException($"Producto con ID {productId} no encontrado");
                }

                // ✅ 2. OBTENER EL ÚLTIMO ORDEN DE IMAGEN
                var maxOrder = await _context.ProductosImagenes
                    .Where(img => img.PimProductoId == productId)
                    .MaxAsync(img => (int?)img.PimOrden) ?? 0;

                // ✅ 3. VERIFICAR SI ES EL PRIMER CONJUNTO DE IMÁGENES
                var hasExistingImages = await _context.ProductosImagenes
                    .AnyAsync(img => img.PimProductoId == productId);

                var newImages = new List<ProductosImagene>();
                var currentOrder = maxOrder;

                // ✅ 4. CREAR TODAS LAS IMÁGENES
                for (int i = 0; i < imageDtos.Count; i++)
                {
                    var imageDto = imageDtos[i];
                    currentOrder++;

                    var newImage = new ProductosImagene
                    {
                        PimProductoId = productId,
                        PimUrl = imageDto.Url,
                        PimTextoAlternativo = imageDto.AltText ?? $"{product.PrdNombre} - Imagen {currentOrder}",
                        PimEsPrincipal = imageDto.EsPrincipal || (!hasExistingImages && i == 0), // Primera imagen como principal si no hay otras
                        PimOrden = imageDto.Orden > 0 ? imageDto.Orden : currentOrder,
                        PimActivo = true
                    };

                    newImages.Add(newImage);
                }

                // ✅ 5. SI ALGUNA IMAGEN SE MARCA COMO PRINCIPAL, QUITAR EL FLAG DE OTRAS
                var hasPrincipalImage = newImages.Any(img => img.PimEsPrincipal == true);
                if (hasPrincipalImage)
                {
                    var existingMainImages = await _context.ProductosImagenes
                        .Where(img => img.PimProductoId == productId && img.PimEsPrincipal == true)
                        .ToListAsync();

                    foreach (var img in existingMainImages)
                    {
                        img.PimEsPrincipal = false;
                    }
                }

                // ✅ 6. GUARDAR TODAS LAS IMÁGENES
                _context.ProductosImagenes.AddRange(newImages);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ SERVICE - {newImages.Count} imágenes agregadas exitosamente al producto {productId}");

                // ✅ 7. MAPEAR Y RETORNAR
                return newImages.Select(img => new ProductImageDto
                {
                    Id = img.PimId,
                    Url = img.PimUrl ?? string.Empty,
                    AltText = img.PimTextoAlternativo,
                    EsPrincipal = img.PimEsPrincipal ?? false,
                    Orden = img.PimOrden ?? 0,
                    Activo = img.PimActivo ?? true
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en AddMultipleProductImagesAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Agrega una nueva imagen a un producto existente
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="imageDto">Datos de la imagen a agregar</param>
        /// <returns>Datos de la imagen creada</returns>
        public async Task<ProductImageDto> AddProductImageAsync(int productId, CreateProductImageDto imageDto)
        {
            try
            {
                Console.WriteLine($"📸 SERVICE - AddProductImageAsync: Producto {productId}, URL: {imageDto.Url}");

                // ✅ 1. VERIFICAR QUE EL PRODUCTO EXISTE
                var product = await _context.Productos.FindAsync(productId);
                if (product == null)
                {
                    throw new ArgumentException($"Producto con ID {productId} no encontrado");
                }

                // ✅ 2. OBTENER EL ÚLTIMO ORDEN DE IMAGEN
                var maxOrder = await _context.ProductosImagenes
                    .Where(img => img.PimProductoId == productId)
                    .MaxAsync(img => (int?)img.PimOrden) ?? 0;

                // ✅ 3. SI ES LA PRIMERA IMAGEN, ESTABLECERLA COMO PRINCIPAL
                var isFirstImage = !await _context.ProductosImagenes
                    .AnyAsync(img => img.PimProductoId == productId);

                // ✅ 4. CREAR LA NUEVA IMAGEN
                var newImage = new ProductosImagene
                {
                    PimProductoId = productId,
                    PimUrl = imageDto.Url,
                    PimTextoAlternativo = imageDto.AltText ?? $"{product.PrdNombre} - Imagen",
                    PimEsPrincipal = imageDto.EsPrincipal || isFirstImage,
                    PimOrden = imageDto.Orden > 0 ? imageDto.Orden : maxOrder + 1,
                    PimActivo = true
                };

                // ✅ 5. SI ESTA IMAGEN SE MARCA COMO PRINCIPAL, QUITAR EL FLAG DE OTRAS
                if (newImage.PimEsPrincipal == true)
                {
                    var existingMainImages = await _context.ProductosImagenes
                        .Where(img => img.PimProductoId == productId && img.PimEsPrincipal == true)
                        .ToListAsync();

                    foreach (var img in existingMainImages)
                    {
                        img.PimEsPrincipal = false;
                    }
                }

                // ✅ 6. GUARDAR LA NUEVA IMAGEN
                _context.ProductosImagenes.Add(newImage);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ SERVICE - Imagen agregada exitosamente: ID={newImage.PimId}, Producto={productId}");

                // ✅ 7. MAPEAR Y RETORNAR
                return new ProductImageDto
                {
                    Id = newImage.PimId,
                    Url = newImage.PimUrl ?? string.Empty,
                    AltText = newImage.PimTextoAlternativo,
                    EsPrincipal = newImage.PimEsPrincipal ?? false,
                    Orden = newImage.PimOrden ?? 0,
                    Activo = newImage.PimActivo ?? true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en AddProductImageAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ajusta el stock de un producto (incremento o decremento)
        /// </summary>
        /// <param name="dto">Datos del ajuste de stock</param>
        /// <returns>True si se ajustó correctamente</returns>
        public async Task<bool> AdjustStockAsync(AdjustStockDto dto)
        {
            try
            {
                Console.WriteLine($"📦 SERVICE - AdjustStockAsync: Producto {dto.ProductoId}, Cantidad: {dto.Cantidad}");

                // ✅ 1. BUSCAR O CREAR INVENTARIO
                var inventario = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.InvProductoId == dto.ProductoId);

                if (inventario == null)
                {
                    // Crear inventario si no existe
                    inventario = new Inventario
                    {
                        InvProductoId = dto.ProductoId,
                        InvStock = Math.Max(0, dto.Cantidad), // No permitir stock negativo
                        InvStockReservado = 0,
                        InvStockMinimo = 5, // Valor por defecto
                        InvFechaUltimaActualizacion = DateTime.UtcNow
                    };
                    _context.Inventarios.Add(inventario);
                    Console.WriteLine($"📦 SERVICE - Nuevo inventario creado para producto {dto.ProductoId}");
                }
                else
                {
                    // Actualizar inventario existente
                    var stockAnterior = inventario.InvStock;
                    inventario.InvStock = Math.Max(0, inventario.InvStock + dto.Cantidad);
                    inventario.InvFechaUltimaActualizacion = DateTime.UtcNow;

                    Console.WriteLine($"📦 SERVICE - Stock actualizado: {stockAnterior} → {inventario.InvStock}");
                }

                // ✅ 2. REGISTRAR MOVIMIENTO DE INVENTARIO
                var movimiento = new MovimientosInventario
                {
                    MovInventarioId = inventario.InvId,
                    MovTipo = dto.Cantidad > 0 ? "entrada" : "salida",
                    MovCantidad = Math.Abs(dto.Cantidad),
                    MovCantidadAnterior = inventario.InvStock - dto.Cantidad,
                    MovMotivo = dto.Motivo ?? "Ajuste manual",
                    MovFecha = DateTime.UtcNow
                };
                _context.MovimientosInventarios.Add(movimiento);

                Console.WriteLine($"📝 SERVICE - Movimiento registrado: {movimiento.MovTipo} de {movimiento.MovCantidad} unidades");

                // ✅ 3. GUARDAR CAMBIOS
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ SERVICE - AdjustStockAsync completado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en AdjustStockAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Elimina múltiples productos (soft delete)
        /// </summary>
        /// <param name="productIds">Lista de IDs de productos a eliminar</param>
        /// <returns>Cantidad de productos eliminados</returns>
        public async Task<int> BulkDeleteAsync(List<int> productIds)
        {
            try
            {
                Console.WriteLine($"🗑️ SERVICE - BulkDeleteAsync: Eliminando {productIds.Count} productos");

                // ✅ 1. VALIDAR QUE HAY PRODUCTOS PARA ELIMINAR
                if (productIds == null || !productIds.Any())
                {
                    Console.WriteLine($"⚠️ SERVICE - Lista de productos vacía");
                    return 0;
                }

                // ✅ 2. OBTENER PRODUCTOS EXISTENTES
                var products = await _context.Productos
                    .Where(p => productIds.Contains(p.PrdId))
                    .ToListAsync();

                if (!products.Any())
                {
                    Console.WriteLine($"⚠️ SERVICE - No se encontraron productos con los IDs proporcionados");
                    return 0;
                }

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos para eliminar");

                // ✅ 3. APLICAR SOFT DELETE A CADA PRODUCTO
                foreach (var product in products)
                {
                    // Soft delete - solo cambiar el estado a inactivo
                    product.PrdActivo = false;
                    product.PrdFechaModificacion = DateTime.UtcNow;

                    Console.WriteLine($"🗑️ SERVICE - Producto eliminado: {product.PrdNombre} (ID: {product.PrdId})");
                }

                // ✅ 4. GUARDAR CAMBIOS
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ SERVICE - BulkDeleteAsync completado: {products.Count} productos eliminados");
                return products.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en BulkDeleteAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cambia el estado de múltiples productos (activo/inactivo)
        /// </summary>
        /// <param name="productIds">Lista de IDs de productos</param>
        /// <param name="active">Estado a establecer (true = activo, false = inactivo)</param>
        /// <returns>Cantidad de productos actualizados</returns>
        public async Task<int> BulkToggleStatusAsync(List<int> productIds, bool active)
        {
            try
            {
                Console.WriteLine($"🔄 SERVICE - BulkToggleStatusAsync: {productIds.Count} productos → {(active ? "ACTIVO" : "INACTIVO")}");

                // ✅ 1. VALIDAR QUE HAY PRODUCTOS PARA PROCESAR
                if (productIds == null || !productIds.Any())
                {
                    Console.WriteLine($"⚠️ SERVICE - Lista de productos vacía");
                    return 0;
                }

                // ✅ 2. OBTENER PRODUCTOS EXISTENTES
                var products = await _context.Productos
                    .Where(p => productIds.Contains(p.PrdId))
                    .ToListAsync();

                if (!products.Any())
                {
                    Console.WriteLine($"⚠️ SERVICE - No se encontraron productos con los IDs proporcionados");
                    return 0;
                }

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos para actualizar");

                // ✅ 3. ACTUALIZAR ESTADO DE CADA PRODUCTO
                foreach (var product in products)
                {
                    var estadoAnterior = product.PrdActivo;
                    product.PrdActivo = active;
                    product.PrdFechaModificacion = DateTime.UtcNow;

                    Console.WriteLine($"🔄 SERVICE - {product.PrdNombre} (ID: {product.PrdId}): {estadoAnterior} → {active}");
                }

                // ✅ 4. GUARDAR CAMBIOS
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ SERVICE - BulkToggleStatusAsync completado: {products.Count} productos actualizados");
                return products.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en BulkToggleStatusAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Actualiza precios de múltiples productos usando diferentes operaciones
        /// </summary>
        /// <param name="dto">Datos de actualización de precios</param>
        /// <returns>Cantidad de productos actualizados</returns>
        public async Task<int> BulkUpdatePricesAsync(BulkPriceUpdateDto dto)
        {
            try
            {
                Console.WriteLine($"💰 SERVICE - BulkUpdatePricesAsync: {dto.ProductIds.Count} productos, Operación: {dto.TipoOperacion}");

                // ✅ 1. VALIDAR QUE HAY PRODUCTOS PARA PROCESAR
                if (dto.ProductIds == null || !dto.ProductIds.Any())
                {
                    Console.WriteLine($"⚠️ SERVICE - Lista de productos vacía");
                    return 0;
                }

                // ✅ 2. OBTENER PRODUCTOS EXISTENTES
                var products = await _context.Productos
                    .Where(p => dto.ProductIds.Contains(p.PrdId))
                    .ToListAsync();

                if (!products.Any())
                {
                    Console.WriteLine($"⚠️ SERVICE - No se encontraron productos con los IDs proporcionados");
                    return 0;
                }

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos para actualizar precios");

                // ✅ 3. APLICAR OPERACIÓN DE PRECIO SEGÚN EL TIPO
                foreach (var product in products)
                {
                    var precioAnterior = product.PrdPrecio;

                    switch (dto.TipoOperacion.ToLower())
                    {
                        case "precio":
                            // Establecer precio fijo
                            if (dto.NuevoPrecio.HasValue)
                            {
                                product.PrdPrecio = dto.NuevoPrecio.Value;
                                Console.WriteLine($"💰 Precio fijo: {product.PrdNombre} - ${precioAnterior} → ${product.PrdPrecio}");
                            }
                            break;

                        case "comparacion":
                            // Establecer precio de comparación fijo
                            if (dto.NuevoPrecioComparacion.HasValue)
                            {
                                product.PrdPrecioComparacion = dto.NuevoPrecioComparacion.Value;
                                Console.WriteLine($"💰 Precio comparación: {product.PrdNombre} - ${product.PrdPrecioComparacion}");
                            }
                            break;

                        case "incremento":
                            // Aplicar incremento porcentual
                            if (dto.PorcentajeIncremento.HasValue)
                            {
                                var factor = 1 + (dto.PorcentajeIncremento.Value / 100);
                                product.PrdPrecio = Math.Round(product.PrdPrecio * factor, 2);
                                Console.WriteLine($"📈 Incremento {dto.PorcentajeIncremento}%: {product.PrdNombre} - ${precioAnterior} → ${product.PrdPrecio}");
                            }
                            break;

                        case "descuento":
                            // Aplicar descuento porcentual
                            if (dto.PorcentajeDescuento.HasValue)
                            {
                                var factor = 1 - (dto.PorcentajeDescuento.Value / 100);
                                product.PrdPrecio = Math.Round(product.PrdPrecio * factor, 2);
                                Console.WriteLine($"📉 Descuento {dto.PorcentajeDescuento}%: {product.PrdNombre} - ${precioAnterior} → ${product.PrdPrecio}");
                            }
                            break;

                        default:
                            Console.WriteLine($"⚠️ SERVICE - Tipo de operación no reconocido: {dto.TipoOperacion}");
                            continue;
                    }

                    // Actualizar fecha de modificación
                    product.PrdFechaModificacion = DateTime.UtcNow;
                }

                // ✅ 4. GUARDAR CAMBIOS
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ SERVICE - BulkUpdatePricesAsync completado: {products.Count} productos actualizados");
                return products.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en BulkUpdatePricesAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo producto con todas sus relaciones
        /// </summary>
        /// <param name="dto">Datos del producto a crear</param>
        /// <returns>Producto creado completo</returns>
        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine($"📦 SERVICE - CreateProductAsync: {dto.Nombre}");

                // ✅ 1. VALIDAR SKU ÚNICO
                if (await SKUExistsAsync(dto.SKU))
                {
                    throw new ArgumentException($"Ya existe un producto con el SKU '{dto.SKU}'");
                }

                // ✅ 2. GENERAR SLUG ÚNICO USANDO EL SERVICIO
                var uniqueSlug = await _slugService.GenerateSlugAsync(
                    dto.Slug ?? dto.Nombre,
                    "productos"
                );

                Console.WriteLine($"🏷️ SERVICE - Slug generado: {uniqueSlug}");

                // ✅ 3. CREAR EL PRODUCTO
                var producto = new Producto
                {
                    PrdSku = dto.SKU,
                    PrdNombre = dto.Nombre,
                    PrdDescripcionCorta = dto.DescripcionCorta,
                    PrdDescripcionLarga = dto.DescripcionLarga,
                    PrdSlug = uniqueSlug,
                    PrdPrecio = dto.Precio,
                    PrdPrecioComparacion = dto.PrecioComparacion,
                    PrdCosto = dto.Costo,
                    PrdCategoriaId = dto.CategoriaId,
                    PrdMarcaId = dto.MarcaId,
                    PrdTipo = dto.Tipo ?? "simple",
                    PrdEstado = dto.Estado ?? "borrador",
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
                    PrdActivo = dto.Activo,
                    PrdFechaCreacion = DateTime.UtcNow,
                    PrdFechaModificacion = DateTime.UtcNow
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ SERVICE - Producto base creado: ID={producto.PrdId}, Slug={uniqueSlug}");

                // ✅ 4. CREAR INVENTARIO INICIAL
                if (dto.StockInicial > 0)
                {
                    var inventario = new Inventario
                    {
                        InvProductoId = producto.PrdId,
                        InvStock = dto.StockInicial,
                        InvStockReservado = 0,
                        InvStockMinimo = 5,
                        InvStockMaximo = 100,
                        InvFechaUltimaActualizacion = DateTime.UtcNow
                    };
                    _context.Inventarios.Add(inventario);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"📦 SERVICE - Inventario inicial creado: Stock={dto.StockInicial}");
                }

                // ✅ 5. AGREGAR IMÁGENES SI LAS HAY
                if (dto.Imagenes?.Any() == true)
                {
                    Console.WriteLine($"📸 SERVICE - Agregando {dto.Imagenes.Count} imágenes");

                    for (int i = 0; i < dto.Imagenes.Count; i++)
                    {
                        var img = dto.Imagenes[i];
                        var newImage = new ProductosImagene
                        {
                            PimProductoId = producto.PrdId,
                            PimUrl = img.Url,
                            PimTextoAlternativo = img.AltText ?? $"{producto.PrdNombre} - Imagen {i + 1}",
                            PimEsPrincipal = img.EsPrincipal || i == 0, // Primera imagen como principal
                            PimOrden = img.Orden > 0 ? img.Orden : i + 1,
                            PimActivo = true
                        };
                        _context.ProductosImagenes.Add(newImage);
                    }

                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ SERVICE - {dto.Imagenes.Count} imágenes agregadas");
                }

                // ✅ 6. REGISTRAR MOVIMIENTO DE INVENTARIO INICIAL
                if (dto.StockInicial > 0)
                {
                    var inventario = await _context.Inventarios
                        .FirstAsync(i => i.InvProductoId == producto.PrdId);

                    var movimiento = new MovimientosInventario
                    {
                        MovInventarioId = inventario.InvId,
                        MovTipo = "entrada",
                        MovCantidad = dto.StockInicial,
                        MovCantidadAnterior = 0,
                        MovMotivo = "Stock inicial del producto",
                        MovFecha = DateTime.UtcNow
                    };
                    _context.MovimientosInventarios.Add(movimiento);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"📝 SERVICE - Movimiento de inventario inicial registrado");
                }

                await transaction.CommitAsync();

                // ✅ 7. CARGAR EL PRODUCTO COMPLETO PARA RETORNAR
                var createdProduct = await GetProductByIdAsync(producto.PrdId);
                if (createdProduct == null)
                {
                    throw new Exception("Error al recuperar el producto creado");
                }

                Console.WriteLine($"🎉 SERVICE - Producto creado exitosamente: {createdProduct.Nombre} (ID: {createdProduct.Id})");
                return createdProduct;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"❌ SERVICE - Error en CreateProductAsync: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Elimina un producto (soft delete)
        /// </summary>
        /// <param name="id">ID del producto a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                Console.WriteLine($"🗑️ SERVICE - DeleteProductAsync: Eliminando producto {id}");

                // ✅ 1. BUSCAR EL PRODUCTO
                var product = await _context.Productos.FirstOrDefaultAsync(p => p.PrdId == id);
                if (product == null)
                {
                    Console.WriteLine($"❌ SERVICE - Producto {id} no encontrado");
                    return false;
                }

                Console.WriteLine($"📦 SERVICE - Producto encontrado: {product.PrdNombre}");

                // ✅ 2. VERIFICAR SI TIENE DEPENDENCIAS CRÍTICAS (OPCIONAL)
                var hasPedidos = await _context.PedidosItems
                    .AnyAsync(pi => pi.PitProductoId == id);

                if (hasPedidos)
                {
                    Console.WriteLine($"⚠️ SERVICE - Producto {id} tiene pedidos asociados, aplicando soft delete");
                }

                // ✅ 3. APLICAR SOFT DELETE
                product.PrdActivo = false;
                product.PrdFechaModificacion = DateTime.UtcNow;

                // ✅ 4. DESACTIVAR IMÁGENES ASOCIADAS (OPCIONAL)
                var imagenes = await _context.ProductosImagenes
                    .Where(img => img.PimProductoId == id)
                    .ToListAsync();

                foreach (var imagen in imagenes)
                {
                    imagen.PimActivo = false;
                }

                Console.WriteLine($"📸 SERVICE - {imagenes.Count} imágenes desactivadas");

                // ✅ 5. REGISTRAR MOVIMIENTO DE INVENTARIO (SALIDA TOTAL)
                var inventario = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.InvProductoId == id);

                if (inventario != null && inventario.InvStock > 0)
                {
                    var movimiento = new MovimientosInventario
                    {
                        MovInventarioId = inventario.InvId,
                        MovTipo = "salida",
                        MovCantidad = inventario.InvStock,
                        MovCantidadAnterior = inventario.InvStock,
                        MovMotivo = "Producto eliminado",
                        MovFecha = DateTime.UtcNow
                    };
                    _context.MovimientosInventarios.Add(movimiento);

                    // Establecer stock a 0
                    inventario.InvStock = 0;
                    inventario.InvFechaUltimaActualizacion = DateTime.UtcNow;

                    Console.WriteLine($"📦 SERVICE - Inventario ajustado a 0");
                }

                // ✅ 6. GUARDAR CAMBIOS
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ SERVICE - Producto {id} eliminado exitosamente (soft delete)");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en DeleteProductAsync: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

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
                    Console.WriteLine($"📸 Imagen a eliminar - Principal: {wasMainImage}, URL: {imageToDelete.PimUrl}");

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
        /// Mapea un ProductDto a ProductSummaryDto
        /// </summary>
        /// <param name="productDto">Producto completo</param>
        /// <returns>Resumen del producto</returns>
        private ProductSummaryDto MapToProductSummaryDto(ProductDto productDto)
        {
            return new ProductSummaryDto
            {
                Id = productDto.Id,
                Nombre = productDto.Nombre,
                Slug = productDto.Slug,
                Precio = productDto.Precio,
                PrecioOferta = productDto.PrecioComparacion != null && productDto.PrecioComparacion < productDto.Precio
                    ? productDto.PrecioComparacion
                    : null,
                Stock = productDto.StockActual,
                ImagenPrincipal = productDto.ImagenPrincipal,
                CategoriaNombre = productDto.CategoriaNombre,
                MarcaNombre = productDto.MarcaNombre,
                Destacado = productDto.Destacado,
                Activo = productDto.Activo
            };
        }

        /// <summary>
        /// Versión síncrona del mapeo para casos donde ya se tiene la información cargada
        /// </summary>
        /// <param name="product">Entidad producto con relaciones cargadas</param>
        /// <param name="imagenPrincipal">URL de imagen principal (opcional)</param>
        /// <returns>Resumen del producto</returns>
        private ProductSummaryDto MapToProductSummaryDtoSync(Producto product, string? imagenPrincipal = null)
        {
            try
            {
                // ✅ 1. OBTENER INFORMACIÓN DE INVENTARIO
                var inventario = product.Inventarios?.FirstOrDefault();
                var stockActual = inventario?.InvStock ?? 0;

                // ✅ 2. CALCULAR PRECIO DE OFERTA SEGÚN LA LÓGICA DEL DTO
                // En tu DTO, PrecioOferta debe ser menor que Precio para ser considerada oferta
                decimal? precioOferta = null;
                if (product.PrdPrecioComparacion.HasValue &&
                    product.PrdPrecioComparacion.Value < product.PrdPrecio &&
                    product.PrdEnOferta == true)
                {
                    precioOferta = product.PrdPrecioComparacion.Value;
                }

                // ✅ 3. CONSTRUIR Y RETORNAR EL DTO SEGÚN TU ESTRUCTURA
                return new ProductSummaryDto
                {
                    Id = product.PrdId,
                    Nombre = product.PrdNombre ?? string.Empty,
                    Slug = product.PrdSlug ?? string.Empty,
                    Precio = product.PrdPrecio,
                    PrecioOferta = precioOferta,
                    ImagenPrincipal = imagenPrincipal,
                    Activo = product.PrdActivo ?? false,
                    Destacado = product.PrdDestacado ?? false,
                    Stock = stockActual,
                    MarcaNombre = product.PrdMarca?.MarNombre ?? string.Empty,
                    CategoriaNombre = product.PrdCategoria?.CatNombre ?? string.Empty
                };
            }
            catch
            {
                // Retornar DTO básico en caso de error
                return new ProductSummaryDto
                {
                    Id = product.PrdId,
                    Nombre = product.PrdNombre ?? "Producto sin nombre",
                    Slug = product.PrdSlug ?? string.Empty,
                    Precio = product.PrdPrecio,
                    PrecioOferta = null,
                    ImagenPrincipal = imagenPrincipal,
                    Activo = false,
                    Destacado = false,
                    Stock = 0,
                    MarcaNombre = "Error",
                    CategoriaNombre = "Error"
                };
            }
        }
        /// <summary>
        /// Obtiene productos activos para selectores
        /// </summary>
        /// <returns>Lista de productos activos</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetActiveProductsAsync()
        {
            try
            {
                Console.WriteLine($"📋 SERVICE - GetActiveProductsAsync: Obteniendo productos activos");

                // ✅ 1. CONSULTAR PRODUCTOS ACTIVOS CON SUS RELACIONES
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true)
                    .OrderBy(p => p.PrdNombre)
                    .ToListAsync();

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos activos");

                // ✅ 2. MAPEAR A DTO USANDO EL MÉTODO ASÍNCRONO
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetActiveProductsAsync completado: {result.Count} productos mapeados");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetActiveProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene filtros disponibles para búsqueda (CORREGIDO)
        /// </summary>
        /// <param name="currentFilter">Filtros actuales (opcional)</param>
        /// <returns>Filtros disponibles</returns>
        public async Task<ProductSearchFiltersDto> GetAvailableFiltersAsync(ProductFilterDto? currentFilter = null)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - GetAvailableFiltersAsync: Generando filtros disponibles");

                // ✅ 1. QUERY BASE - SOLO PRODUCTOS ACTIVOS
                var query = _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Where(p => p.PrdActivo == true);

                // ✅ 2. APLICAR FILTROS ACTUALES PARA OBTENER OPCIONES RELEVANTES
                if (currentFilter != null)
                {
                    Console.WriteLine($"🔧 SERVICE - Aplicando filtros actuales para refinar opciones");

                    if (currentFilter.CategoriaId.HasValue)
                    {
                        query = query.Where(p => p.PrdCategoriaId == currentFilter.CategoriaId);
                        Console.WriteLine($"   - Filtro categoría: {currentFilter.CategoriaId}");
                    }

                    if (currentFilter.MarcaId.HasValue)
                    {
                        query = query.Where(p => p.PrdMarcaId == currentFilter.MarcaId);
                        Console.WriteLine($"   - Filtro marca: {currentFilter.MarcaId}");
                    }

                    if (currentFilter.PrecioMin.HasValue)
                    {
                        query = query.Where(p => p.PrdPrecio >= currentFilter.PrecioMin);
                        Console.WriteLine($"   - Precio mínimo: ${currentFilter.PrecioMin}");
                    }

                    if (currentFilter.PrecioMax.HasValue)
                    {
                        query = query.Where(p => p.PrdPrecio <= currentFilter.PrecioMax);
                        Console.WriteLine($"   - Precio máximo: ${currentFilter.PrecioMax}");
                    }

                    if (!string.IsNullOrEmpty(currentFilter.Busqueda))
                    {
                        var search = currentFilter.Busqueda.ToLower();
                        query = query.Where(p =>
                            p.PrdNombre.ToLower().Contains(search) ||
                            (p.PrdDescripcionCorta != null && p.PrdDescripcionCorta.ToLower().Contains(search)) ||
                            p.PrdSku.ToLower().Contains(search));
                        Console.WriteLine($"   - Búsqueda: '{currentFilter.Busqueda}'");
                    }
                }

                // ✅ 3. OBTENER CATEGORÍAS CON CONTEO (CORREGIDO)
                var categories = await query
                    .Where(p => p.PrdCategoriaId != null) // Si PrdCategoriaId puede ser null
                    .GroupBy(p => new { p.PrdCategoriaId, p.PrdCategoria.CatNombre })
                    .Select(g => new CategoryFilterOption
                    {
                        Id = g.Key.PrdCategoriaId, // ❌ REMOVIDO .Value
                        Nombre = g.Key.CatNombre ?? "Sin nombre",
                        Count = g.Count()
                    })
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();

                Console.WriteLine($"🏷️ SERVICE - Categorías encontradas: {categories.Count}");

                // ✅ 4. OBTENER MARCAS CON CONTEO (CORREGIDO)
                var brands = await query
                    .Where(p => p.PrdMarcaId != null) // Si PrdMarcaId puede ser null
                    .GroupBy(p => new { p.PrdMarcaId, p.PrdMarca.MarNombre })
                    .Select(g => new BrandFilterOption
                    {
                        Id = g.Key.PrdMarcaId, // ❌ REMOVIDO .Value
                        Nombre = g.Key.MarNombre ?? "Sin nombre",
                        Count = g.Count()
                    })
                    .OrderBy(b => b.Nombre)
                    .ToListAsync();

                Console.WriteLine($"🏭 SERVICE - Marcas encontradas: {brands.Count}");

                // ✅ 5. OBTENER RANGO DE PRECIOS
                var prices = await query.Select(p => p.PrdPrecio).ToListAsync();
                var priceRange = new PriceRangeDto
                {
                    Min = prices.Any() ? prices.Min() : 0,
                    Max = prices.Any() ? prices.Max() : 0
                };

                Console.WriteLine($"💰 SERVICE - Rango de precios: ${priceRange.Min} - ${priceRange.Max}");

                // ✅ 6. CONSTRUIR RESULTADO
                var result = new ProductSearchFiltersDto
                {
                    Categorias = categories,
                    Marcas = brands,
                    RangoPrecios = priceRange
                };

                Console.WriteLine($"✅ SERVICE - GetAvailableFiltersAsync completado");
                Console.WriteLine($"   📊 Resumen: {categories.Count} categorías, {brands.Count} marcas");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetAvailableFiltersAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos más vendidos basado en la cantidad vendida en pedidos
        /// </summary>
        /// <param name="count">Cantidad de productos a retornar</param>
        /// <returns>Lista de productos más vendidos</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetBestSellingProductsAsync(int count = 10)
        {
            try
            {
                Console.WriteLine($"🏆 SERVICE - GetBestSellingProductsAsync: Obteniendo top {count} productos más vendidos");

                // ✅ 1. OBTENER PRODUCTOS MÁS VENDIDOS CON AGREGACIÓN
                var bestSellingIds = await _context.PedidosItems
                    .Include(pi => pi.PitProducto)
                    .Where(pi => pi.PitProducto != null && pi.PitProducto.PrdActivo == true)
                    .GroupBy(pi => pi.PitProductoId)
                    .Select(g => new
                    {
                        ProductoId = g.Key,
                        TotalVendido = g.Sum(pi => pi.PitCantidad)
                    })
                    .OrderByDescending(x => x.TotalVendido)
                    .Take(count)
                    .Select(x => x.ProductoId)
                    .ToListAsync();

                Console.WriteLine($"📊 SERVICE - Encontrados {bestSellingIds.Count} productos con ventas");

                if (!bestSellingIds.Any())
                {
                    Console.WriteLine($"⚠️ SERVICE - No se encontraron productos con ventas, retornando productos destacados");
                    // Si no hay ventas, retornar productos destacados como fallback
                    return await GetFeaturedProductsAsync(count);
                }

                // ✅ 2. CARGAR PRODUCTOS COMPLETOS EN EL ORDEN DE VENTAS
                var products = new List<Producto>();
                foreach (var productId in bestSellingIds)
                {
                    var product = await _context.Productos
                        .Include(p => p.PrdCategoria)
                        .Include(p => p.PrdMarca)
                        .Include(p => p.Inventarios)
                        .FirstOrDefaultAsync(p => p.PrdId == productId && p.PrdActivo == true);

                    if (product != null)
                    {
                        products.Add(product);
                    }
                }

                Console.WriteLine($"📦 SERVICE - Cargados {products.Count} productos más vendidos");

                // ✅ 3. MAPEAR A DTO CON INFORMACIÓN DE VENTAS
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetBestSellingProductsAsync completado: {result.Count} productos mapeados");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetBestSellingProductsAsync: {ex.Message}");

                // En caso de error, retornar productos destacados como fallback
                Console.WriteLine($"🔄 SERVICE - Fallback: Retornando productos destacados");
                try
                {
                    return await GetFeaturedProductsAsync(count);
                }
                catch
                {
                    Console.WriteLine($"❌ SERVICE - Error también en fallback, retornando lista vacía");
                    return Enumerable.Empty<ProductSummaryDto>();
                }
            }
        }

        /// <summary>
        /// Obtiene productos destacados ordenados por prioridad
        /// </summary>
        /// <param name="count">Cantidad de productos a retornar</param>
        /// <returns>Lista de productos destacados</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetFeaturedProductsAsync(int count = 8)
        {
            try
            {
                Console.WriteLine($"⭐ SERVICE - GetFeaturedProductsAsync: Obteniendo {count} productos destacados");

                // ✅ 1. CONSULTAR PRODUCTOS DESTACADOS ACTIVOS
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true && p.PrdDestacado == true)
                    .OrderBy(p => p.PrdOrden ?? int.MaxValue) // Orden específico, nulls al final
                    .ThenByDescending(p => p.PrdFechaCreacion) // Más recientes primero como secundario
                    .ThenBy(p => p.PrdNombre) // Alfabético como terciario
                    .Take(count)
                    .ToListAsync();

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos destacados");

                // ✅ 2. SI NO HAY SUFICIENTES DESTACADOS, COMPLETAR CON OTROS CRITERIOS
                if (products.Count < count)
                {
                    var needed = count - products.Count;
                    var existingIds = products.Select(p => p.PrdId).ToList();

                    Console.WriteLine($"📈 SERVICE - Solo {products.Count} destacados, buscando {needed} productos adicionales");

                    // Buscar productos en oferta que no estén ya incluidos
                    var additionalProducts = await _context.Productos
                        .Include(p => p.PrdCategoria)
                        .Include(p => p.PrdMarca)
                        .Include(p => p.Inventarios)
                        .Where(p => p.PrdActivo == true &&
                                   !existingIds.Contains(p.PrdId) &&
                                   (p.PrdEnOferta == true || p.PrdNuevo == true))
                        .OrderByDescending(p => p.PrdEnOferta) // Ofertas primero
                        .ThenByDescending(p => p.PrdNuevo) // Nuevos después
                        .ThenByDescending(p => p.PrdFechaCreacion) // Más recientes
                        .Take(needed)
                        .ToListAsync();

                    products.AddRange(additionalProducts);
                    Console.WriteLine($"📦 SERVICE - Agregados {additionalProducts.Count} productos adicionales (ofertas/nuevos)");
                }

                // ✅ 3. SI AÚN FALTAN, COMPLETAR CON PRODUCTOS ACTIVOS ALEATORIOS
                if (products.Count < count)
                {
                    var stillNeeded = count - products.Count;
                    var existingIds = products.Select(p => p.PrdId).ToList();

                    Console.WriteLine($"🎲 SERVICE - Faltan {stillNeeded} productos, agregando aleatorios");

                    var randomProducts = await _context.Productos
                        .Include(p => p.PrdCategoria)
                        .Include(p => p.PrdMarca)
                        .Include(p => p.Inventarios)
                        .Where(p => p.PrdActivo == true && !existingIds.Contains(p.PrdId))
                        .OrderBy(p => Guid.NewGuid()) // Orden aleatorio
                        .Take(stillNeeded)
                        .ToListAsync();

                    products.AddRange(randomProducts);
                    Console.WriteLine($"📦 SERVICE - Agregados {randomProducts.Count} productos aleatorios");
                }

                // ✅ 4. MAPEAR A DTO
                var result = new List<ProductSummaryDto>();
                foreach (var product in products.Take(count)) // Asegurar que no exceda el count
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetFeaturedProductsAsync completado: {result.Count} productos mapeados");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetFeaturedProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos destacados con criterios más estrictos (versión alternativa)
        /// </summary>
        /// <param name="count">Cantidad de productos a retornar</param>
        /// <returns>Lista de productos destacados</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetFeaturedProductsStrictAsync(int count = 8)
        {
            try
            {
                Console.WriteLine($"⭐ SERVICE - GetFeaturedProductsStrictAsync: Obteniendo {count} productos destacados (estricto)");

                // ✅ SOLO PRODUCTOS MARCADOS COMO DESTACADOS, SIN FALLBACKS
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true &&
                               p.PrdDestacado == true)
                    .OrderBy(p => p.PrdOrden ?? int.MaxValue)
                    .ThenByDescending(p => p.PrdFechaCreacion)
                    .Take(count)
                    .ToListAsync();

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos destacados (modo estricto)");

                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetFeaturedProductsStrictAsync completado: {result.Count} productos");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetFeaturedProductsStrictAsync: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Obtiene productos con bajo stock (stock <= stock mínimo pero > 0)
        /// </summary>
        /// <returns>Lista de productos con bajo stock</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetLowStockProductsAsync()
        {
            try
            {
                Console.WriteLine($"⚠️ SERVICE - GetLowStockProductsAsync: Obteniendo productos con bajo stock");

                // ✅ 1. CONSULTAR PRODUCTOS CON BAJO STOCK
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true &&
                               p.Inventarios.Any(i => i.InvStock <= i.InvStockMinimo && i.InvStock > 0))
                    .OrderBy(p => p.Inventarios.FirstOrDefault().InvStock) // Los de menor stock primero
                    .ThenBy(p => p.PrdNombre) // Alfabético como secundario
                    .ToListAsync();

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos con bajo stock");

                // ✅ 2. MOSTRAR DETALLE DE STOCK EN LOGS
                foreach (var product in products.Take(5)) // Solo los primeros 5 en log
                {
                    var inventario = product.Inventarios?.FirstOrDefault();
                    if (inventario != null)
                    {
                        Console.WriteLine($"   ⚠️ {product.PrdNombre}: Stock={inventario.InvStock}, Mínimo={inventario.InvStockMinimo}");
                    }
                }

                if (products.Count > 5)
                {
                    Console.WriteLine($"   ... y {products.Count - 5} productos más");
                }

                // ✅ 3. MAPEAR A DTO
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetLowStockProductsAsync completado: {result.Count} productos mapeados");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetLowStockProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos sin stock (stock = 0)
        /// </summary>
        /// <returns>Lista de productos sin stock</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetOutOfStockProductsAsync()
        {
            try
            {
                Console.WriteLine($"🚫 SERVICE - GetOutOfStockProductsAsync: Obteniendo productos sin stock");

                // ✅ 1. CONSULTAR PRODUCTOS SIN STOCK
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true &&
                               p.Inventarios.Any(i => i.InvStock <= 0))
                    .OrderByDescending(p => p.Inventarios.FirstOrDefault().InvFechaUltimaActualizacion) // Más recientes primero
                    .ThenBy(p => p.PrdNombre)
                    .ToListAsync();

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos sin stock");

                // ✅ 2. MAPEAR A DTO
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetOutOfStockProductsAsync completado: {result.Count} productos mapeados");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetOutOfStockProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos más nuevos ordenados por fecha de creación
        /// </summary>
        /// <param name="count">Cantidad de productos a retornar</param>
        /// <returns>Lista de productos más recientes</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetNewestProductsAsync(int count = 8)
        {
            try
            {
                Console.WriteLine($"🆕 SERVICE - GetNewestProductsAsync: Obteniendo {count} productos más nuevos");

                // ✅ 1. CONSULTAR PRODUCTOS MÁS RECIENTES
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true)
                    .OrderByDescending(p => p.PrdFechaCreacion) // Más recientes primero
                    .ThenByDescending(p => p.PrdNuevo) // Productos marcados como "nuevo" tienen prioridad
                    .ThenBy(p => p.PrdNombre) // Alfabético como último criterio
                    .Take(count)
                    .ToListAsync();

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos más nuevos");

                // ✅ 2. MOSTRAR INFORMACIÓN DE FECHAS EN LOGS
                var oldestInSelection = products.LastOrDefault();
                var newestInSelection = products.FirstOrDefault();

                if (newestInSelection != null && oldestInSelection != null)
                {
                    Console.WriteLine($"   📅 Más nuevo: {newestInSelection.PrdNombre} ({newestInSelection.PrdFechaCreacion:yyyy-MM-dd})");
                    Console.WriteLine($"   📅 Más antiguo en selección: {oldestInSelection.PrdNombre} ({oldestInSelection.PrdFechaCreacion:yyyy-MM-dd})");
                }

                // ✅ 3. ESTADÍSTICAS DE PRODUCTOS "NUEVOS"
                var markedAsNew = products.Count(p => p.PrdNuevo == true);
                if (markedAsNew > 0)
                {
                    Console.WriteLine($"   🏷️ Productos marcados como 'nuevo': {markedAsNew} de {products.Count}");
                }

                // ✅ 4. MAPEAR A DTO
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetNewestProductsAsync completado: {result.Count} productos mapeados");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetNewestProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos marcados específicamente como "nuevos"
        /// </summary>
        /// <param name="count">Cantidad de productos a retornar</param>
        /// <returns>Lista de productos marcados como nuevos</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetNewProductsAsync(int count = 12)
        {
            try
            {
                Console.WriteLine($"🏷️ SERVICE - GetNewProductsAsync: Obteniendo {count} productos marcados como nuevos");

                // ✅ 1. CONSULTAR PRODUCTOS MARCADOS COMO "NUEVOS"
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true && p.PrdNuevo == true)
                    .OrderByDescending(p => p.PrdFechaCreacion) // Más recientes primero
                    .ThenBy(p => p.PrdOrden ?? int.MaxValue) // Orden específico
                    .ThenBy(p => p.PrdNombre) // Alfabético
                    .Take(count)
                    .ToListAsync();

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos marcados como nuevos");

                // ✅ 2. MAPEAR A DTO
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetNewProductsAsync completado: {result.Count} productos mapeados");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetNewProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos recientes por rango de fechas
        /// </summary>
        /// <param name="days">Días hacia atrás desde hoy</param>
        /// <param name="count">Cantidad máxima de productos</param>
        /// <returns>Lista de productos creados en los últimos días especificados</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetRecentProductsAsync(int days = 30, int count = 20)
        {
            try
            {
                Console.WriteLine($"📅 SERVICE - GetRecentProductsAsync: Productos de los últimos {days} días (máx {count})");

                var cutoffDate = DateTime.UtcNow.AddDays(-days);
                Console.WriteLine($"   📅 Fecha de corte: {cutoffDate:yyyy-MM-dd HH:mm:ss} UTC");

                // ✅ 1. CONSULTAR PRODUCTOS RECIENTES POR FECHA
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true &&
                               p.PrdFechaCreacion >= cutoffDate)
                    .OrderByDescending(p => p.PrdFechaCreacion)
                    .Take(count)
                    .ToListAsync();

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos de los últimos {days} días");

                // ✅ 2. ESTADÍSTICAS POR DÍAS
                if (products.Any())
                {
                    var today = DateTime.UtcNow.Date;
                    var todayCount = products.Count(p => p.PrdFechaCreacion?.Date == today);
                    var yesterdayCount = products.Count(p => p.PrdFechaCreacion?.Date == today.AddDays(-1));
                    var thisWeekCount = products.Count(p => p.PrdFechaCreacion >= today.AddDays(-7));

                    Console.WriteLine($"   📊 Hoy: {todayCount}, Ayer: {yesterdayCount}, Esta semana: {thisWeekCount}");
                }

                // ✅ 3. MAPEAR A DTO
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetRecentProductsAsync completado: {result.Count} productos mapeados");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetRecentProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Mapea una entidad Producto a ProductDto (método asíncrono para cargar imágenes)
        /// </summary>
        /// <param name="product">Entidad producto</param>
        /// <returns>DTO del producto completo</returns>
        private async Task<ProductDto> MapToProductDtoAsync(Producto product)
        {
            try
            {
                // ✅ 1. CARGAR IMÁGENES DEL PRODUCTO USANDO EL SERVICIO DEDICADO
                var images = await _imageService.GetImagenesByProductoIdAsync(product.PrdId);
                var imagesList = images.ToList();

                // ✅ 2. OBTENER IMAGEN PRINCIPAL
                var mainImage = imagesList.FirstOrDefault(img => img.EsPrincipal);
                var mainImageUrl = mainImage?.Url ?? imagesList.FirstOrDefault()?.Url;

                // ✅ 3. MAPEAR IMÁGENES A DTOs
                var imageDtos = imagesList.Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    Url = img.Url ?? string.Empty,
                    AltText = img.AltText,
                    EsPrincipal = img.EsPrincipal,
                    Orden = img.Orden,
                    Activo = true // El servicio ya filtra las activas
                }).OrderBy(img => img.Orden).ToList();

                // ✅ 4. CALCULAR INFORMACIÓN DE INVENTARIO
                var inventario = product.Inventarios?.FirstOrDefault();
                var stockActual = inventario?.InvStock ?? 0;
                var stockReservado = inventario?.InvStockReservado ?? 0;

                // ✅ 5. CONSTRUIR EL DTO SEGÚN TU ESTRUCTURA EXACTA
                var productDto = new ProductDto
                {
                    // Información básica
                    Id = product.PrdId,
                    SKU = product.PrdSku ?? string.Empty,
                    Nombre = product.PrdNombre ?? string.Empty,
                    DescripcionCorta = product.PrdDescripcionCorta,
                    DescripcionLarga = product.PrdDescripcionLarga,
                    Slug = product.PrdSlug ?? string.Empty,

                    // Precios
                    Precio = product.PrdPrecio,
                    PrecioComparacion = product.PrdPrecioComparacion,
                    Costo = product.PrdCosto,

                    // Clasificación
                    Tipo = product.PrdTipo,
                    Estado = product.PrdEstado,
                    Destacado = product.PrdDestacado ?? false,
                    Nuevo = product.PrdNuevo ?? false,
                    EnOferta = product.PrdEnOferta ?? false,

                    // Especificaciones físicas
                    Peso = product.PrdPeso,
                    Dimensiones = product.PrdDimensiones,

                    // SEO
                    MetaTitulo = product.PrdMetaTitulo,
                    MetaDescripcion = product.PrdMetaDescripcion,
                    PalabrasClaves = product.PrdPalabrasClaves,

                    // Configuración
                    RequiereEnvio = product.PrdRequiereEnvio ?? true,
                    PermiteReseñas = product.PrdPermiteReseñas ?? true,
                    Garantia = product.PrdGarantia,
                    Orden = product.PrdOrden ?? 0,

                    // Estado y fechas
                    Activo = product.PrdActivo ?? false,
                    FechaCreacion = product.PrdFechaCreacion ?? DateTime.UtcNow,
                    FechaModificacion = product.PrdFechaModificacion,

                    // Relaciones - Categoría
                    CategoriaId = product.PrdCategoriaId,
                    CategoriaNombre = product.PrdCategoria?.CatNombre ?? string.Empty,
                    CategoriaRuta = product.PrdCategoria?.CatSlug ?? string.Empty,

                    // Relaciones - Marca
                    MarcaId = product.PrdMarcaId,
                    MarcaNombre = product.PrdMarca?.MarNombre ?? string.Empty,
                    MarcaLogo = product.PrdMarca?.MarLogo,

                    // Inventario
                    StockActual = stockActual,
                    StockReservado = stockReservado,
                    // StockDisponible se calcula automáticamente en el DTO

                    // Imágenes
                    ImagenPrincipal = mainImageUrl,
                    Imagenes = imageDtos
                    // PrecioFinal, PorcentajeDescuento y EstadoStock se calculan automáticamente en el DTO
                };

                return productDto;
            }
            catch
            {
                // En caso de error, retornar un DTO básico con la información mínima disponible
                return new ProductDto
                {
                    Id = product.PrdId,
                    SKU = product.PrdSku ?? string.Empty,
                    Nombre = product.PrdNombre ?? "Producto con error",
                    DescripcionCorta = "Error al cargar descripción",
                    Slug = product.PrdSlug ?? string.Empty,
                    Precio = product.PrdPrecio,
                    PrecioComparacion = product.PrdPrecioComparacion,
                    Costo = product.PrdCosto,
                    Tipo = product.PrdTipo,
                    Estado = "error",
                    Destacado = false,
                    Nuevo = false,
                    EnOferta = false,
                    RequiereEnvio = true,
                    PermiteReseñas = false,
                    Orden = 0,
                    Activo = false,
                    FechaCreacion = product.PrdFechaCreacion ?? DateTime.UtcNow,
                    FechaModificacion = product.PrdFechaModificacion,
                    CategoriaId = product.PrdCategoriaId,
                    CategoriaNombre = product.PrdCategoria?.CatNombre ?? "Error",
                    CategoriaRuta = "error",
                    MarcaId = product.PrdMarcaId,
                    MarcaNombre = product.PrdMarca?.MarNombre ?? "Error",
                    MarcaLogo = null,
                    StockActual = 0,
                    StockReservado = 0,
                    ImagenPrincipal = null,
                    Imagenes = new List<ProductImageDto>()
                };
            }
        }

        /// <summary>
        /// Obtiene un producto por su ID con toda la información completa
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Producto completo o null si no existe</returns>
        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - GetProductByIdAsync: Buscando producto ID {id}");

                // ✅ 1. CONSULTAR PRODUCTO CON TODAS SUS RELACIONES
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

                Console.WriteLine($"✅ SERVICE - Producto encontrado: {product.PrdNombre} (Activo: {product.PrdActivo})");

                // ✅ 2. MAPEAR A DTO COMPLETO CON IMÁGENES
                var productDto = await MapToProductDtoAsync(product);

                // ✅ 3. LOGGING ADICIONAL PARA DEBUG
                Console.WriteLine($"📊 SERVICE - Información del producto:");
                Console.WriteLine($"   - Nombre: {productDto.Nombre}");
                Console.WriteLine($"   - SKU: {productDto.SKU}");
                Console.WriteLine($"   - Slug: {productDto.Slug}");
                Console.WriteLine($"   - Precio: ${productDto.Precio}");
                Console.WriteLine($"   - Categoría: {productDto.CategoriaNombre}");
                Console.WriteLine($"   - Marca: {productDto.MarcaNombre}");
                Console.WriteLine($"   - Stock: {productDto.StockActual}");
                Console.WriteLine($"   - Imágenes: {productDto.Imagenes.Count}");
                Console.WriteLine($"   - Imagen principal: {(!string.IsNullOrEmpty(productDto.ImagenPrincipal) ? "✅" : "❌")}");

                Console.WriteLine($"✅ SERVICE - GetProductByIdAsync completado exitosamente");
                return productDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetProductByIdAsync ID {id}: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        /// <summary>
        /// Mapea una entidad Producto a ProductSummaryDto (método asíncrono para cargar imagen principal)
        /// </summary>
        /// <param name="product">Entidad producto</param>
        /// <returns>Resumen del producto</returns>
        private async Task<ProductSummaryDto> MapToProductSummaryDtoAsync(Producto product)
        {
            try
            {
                // ✅ 1. OBTENER IMAGEN PRINCIPAL DE FORMA ASÍNCRONA
                string? imagenPrincipal = null;
                try
                {
                    imagenPrincipal = await GetProductMainImageUrlAsync(product.PrdId);
                }
                catch
                {
                    // Continuar sin imagen principal
                }

                // ✅ 2. OBTENER INFORMACIÓN DE INVENTARIO
                var inventario = product.Inventarios?.FirstOrDefault();
                var stockActual = inventario?.InvStock ?? 0;

                // ✅ 3. CALCULAR PRECIO DE OFERTA SEGÚN LA LÓGICA DEL DTO
                // En tu DTO, PrecioOferta debe ser menor que Precio para ser considerada oferta
                decimal? precioOferta = null;
                if (product.PrdPrecioComparacion.HasValue &&
                    product.PrdPrecioComparacion.Value < product.PrdPrecio &&
                    product.PrdEnOferta == true)
                {
                    precioOferta = product.PrdPrecioComparacion.Value;
                }

                // ✅ 4. CONSTRUIR Y RETORNAR EL DTO SEGÚN TU ESTRUCTURA
                var summary = new ProductSummaryDto
                {
                    Id = product.PrdId,
                    Nombre = product.PrdNombre ?? string.Empty,
                    Slug = product.PrdSlug ?? string.Empty,
                    Precio = product.PrdPrecio,
                    PrecioOferta = precioOferta,
                    ImagenPrincipal = imagenPrincipal,
                    Activo = product.PrdActivo ?? false,
                    Destacado = product.PrdDestacado ?? false,
                    Stock = stockActual,
                    MarcaNombre = product.PrdMarca?.MarNombre ?? string.Empty,
                    CategoriaNombre = product.PrdCategoria?.CatNombre ?? string.Empty
                };

                return summary;
            }
            catch
            {
                // En caso de error, retornar un DTO básico con la información mínima
                return new ProductSummaryDto
                {
                    Id = product.PrdId,
                    Nombre = product.PrdNombre ?? "Producto sin nombre",
                    Slug = product.PrdSlug ?? string.Empty,
                    Precio = product.PrdPrecio,
                    PrecioOferta = null,
                    ImagenPrincipal = null,
                    Activo = false,
                    Destacado = false,
                    Stock = 0,
                    MarcaNombre = "Error",
                    CategoriaNombre = "Error"
                };
            }
        }

        /// <summary>
        /// Obtiene un producto por su slug (para URLs amigables)
        /// </summary>
        /// <param name="slug">Slug del producto</param>
        /// <returns>Producto completo o null si no existe</returns>
        public async Task<ProductDto?> GetProductBySlugAsync(string slug)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - GetProductBySlugAsync: Buscando producto con slug '{slug}'");

                // ✅ 1. VALIDAR SLUG
                if (string.IsNullOrWhiteSpace(slug))
                {
                    Console.WriteLine($"❌ SERVICE - Slug inválido o vacío");
                    return null;
                }

                // ✅ 2. CONSULTAR PRODUCTO POR SLUG - SOLO ACTIVOS
                var product = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .FirstOrDefaultAsync(p => p.PrdSlug == slug && p.PrdActivo == true);

                if (product == null)
                {
                    Console.WriteLine($"❌ SERVICE - Producto con slug '{slug}' no encontrado o inactivo");
                    return null;
                }

                Console.WriteLine($"✅ SERVICE - Producto encontrado por slug: {product.PrdNombre} (ID: {product.PrdId})");

                // ✅ 3. MAPEAR A DTO COMPLETO CON IMÁGENES
                var productDto = await MapToProductDtoAsync(product);

                // ✅ 4. VALIDACIONES ADICIONALES PARA FRONTEND
                if (!productDto.Activo)
                {
                    Console.WriteLine($"⚠️ SERVICE - Producto encontrado pero inactivo");
                    return null; // No mostrar productos inactivos en frontend
                }

                // ✅ 5. LOGGING PARA SEO Y DEBUG
                Console.WriteLine($"📊 SERVICE - Información SEO del producto:");
                Console.WriteLine($"   - URL amigable: /productos/{productDto.Slug}");
                Console.WriteLine($"   - Meta título: {productDto.MetaTitulo ?? "No definido"}");
                Console.WriteLine($"   - Meta descripción: {(!string.IsNullOrEmpty(productDto.MetaDescripcion) ? "✅" : "❌")}");
                Console.WriteLine($"   - Palabras clave: {productDto.PalabrasClaves ?? "No definidas"}");
                Console.WriteLine($"   - Imagen principal: {(!string.IsNullOrEmpty(productDto.ImagenPrincipal) ? "✅" : "❌")}");

                Console.WriteLine($"✅ SERVICE - GetProductBySlugAsync completado exitosamente");
                return productDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetProductBySlugAsync slug '{slug}': {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un producto activo por ID (versión pública para frontend)
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Producto activo o null si no existe/inactivo</returns>
        public async Task<ProductDto?> GetActiveProductByIdAsync(int id)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - GetActiveProductByIdAsync: Buscando producto activo ID {id}");

                // ✅ 1. CONSULTAR SOLO PRODUCTOS ACTIVOS
                var product = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .FirstOrDefaultAsync(p => p.PrdId == id && p.PrdActivo == true);

                if (product == null)
                {
                    Console.WriteLine($"❌ SERVICE - Producto activo {id} no encontrado");
                    return null;
                }

                Console.WriteLine($"✅ SERVICE - Producto activo encontrado: {product.PrdNombre}");

                // ✅ 2. MAPEAR A DTO COMPLETO
                var productDto = await MapToProductDtoAsync(product);

                Console.WriteLine($"✅ SERVICE - GetActiveProductByIdAsync completado");
                return productDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetActiveProductByIdAsync ID {id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene información básica de un producto por ID (más rápida, sin imágenes)
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Información básica del producto</returns>
        public async Task<ProductSummaryDto?> GetProductSummaryByIdAsync(int id)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - GetProductSummaryByIdAsync: Buscando resumen producto ID {id}");

                // ✅ 1. CONSULTAR PRODUCTO CON RELACIONES BÁSICAS
                var product = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .FirstOrDefaultAsync(p => p.PrdId == id);

                if (product == null)
                {
                    Console.WriteLine($"❌ SERVICE - Producto {id} no encontrado para resumen");
                    return null;
                }

                // ✅ 2. MAPEAR A RESUMEN (MÁS RÁPIDO QUE DTO COMPLETO)
                var summary = await MapToProductSummaryDtoAsync(product);

                Console.WriteLine($"✅ SERVICE - GetProductSummaryByIdAsync completado: {summary.Nombre}");
                return summary;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetProductSummaryByIdAsync ID {id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si un producto existe por ID
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>True si existe</returns>
        public async Task<bool> ProductExistsByIdAsync(int id)
        {
            try
            {
                var exists = await _context.Productos.AnyAsync(p => p.PrdId == id);
                Console.WriteLine($"🔍 SERVICE - ProductExistsByIdAsync ID {id}: {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en ProductExistsByIdAsync ID {id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si un producto activo existe por slug
        /// </summary>
        /// <param name="slug">Slug del producto</param>
        /// <returns>True si existe y está activo</returns>
        public async Task<bool> ActiveProductExistsBySlugAsync(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                    return false;

                var exists = await _context.Productos.AnyAsync(p => p.PrdSlug == slug && p.PrdActivo == true);
                Console.WriteLine($"🔍 SERVICE - ActiveProductExistsBySlugAsync slug '{slug}': {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en ActiveProductExistsBySlugAsync slug '{slug}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene múltiples productos por sus IDs
        /// </summary>
        /// <param name="ids">Lista de IDs de productos</param>
        /// <param name="onlyActive">Si true, solo retorna productos activos</param>
        /// <returns>Lista de productos encontrados</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetProductsByIdsAsync(List<int> ids, bool onlyActive = true)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - GetProductsByIdsAsync: Buscando {ids.Count} productos (Solo activos: {onlyActive})");

                if (!ids.Any())
                {
                    Console.WriteLine($"⚠️ SERVICE - Lista de IDs vacía");
                    return Enumerable.Empty<ProductSummaryDto>();
                }

                // ✅ 1. CONSULTAR PRODUCTOS POR IDS
                var query = _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => ids.Contains(p.PrdId));

                if (onlyActive)
                {
                    query = query.Where(p => p.PrdActivo == true);
                }

                var products = await query.ToListAsync();

                Console.WriteLine($"📦 SERVICE - Encontrados {products.Count} productos de {ids.Count} solicitados");

                // ✅ 2. MAPEAR A RESUMEN
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetProductsByIdsAsync completado: {result.Count} productos mapeados");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetProductsByIdsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene información completa de debug de un producto
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Objeto con toda la información de debug del producto</returns>
        public async Task<object?> GetProductDebugInfoAsync(int id)
        {
            try
            {
                Console.WriteLine($"🐛 SERVICE - GetProductDebugInfoAsync: Obteniendo info de debug para producto {id}");

                // ✅ 1. CARGAR PRODUCTO CON TODAS LAS RELACIONES
                var product = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Include(p => p.ProductosImagenes)
                    .FirstOrDefaultAsync(p => p.PrdId == id);

                if (product == null)
                {
                    Console.WriteLine($"❌ SERVICE - Producto {id} no encontrado para debug");
                    return null;
                }

                Console.WriteLine($"🔍 SERVICE - Generando información de debug para: {product.PrdNombre}");

                // ✅ 2. CONSTRUIR OBJETO DE DEBUG COMPLETO
                var debugInfo = new
                {
                    // Información básica del producto
                    ProductoInfo = new
                    {
                        product.PrdId,
                        product.PrdNombre,
                        product.PrdSku,
                        product.PrdSlug,
                        product.PrdDescripcionCorta,
                        product.PrdDescripcionLarga,
                        product.PrdPrecio,
                        product.PrdPrecioComparacion,
                        product.PrdCosto,
                        product.PrdTipo,
                        product.PrdEstado,
                        product.PrdDestacado,
                        product.PrdNuevo,
                        product.PrdEnOferta,
                        product.PrdPeso,
                        product.PrdDimensiones,
                        product.PrdRequiereEnvio,
                        product.PrdPermiteReseñas,
                        product.PrdGarantia,
                        product.PrdOrden,
                        product.PrdActivo,
                        product.PrdFechaCreacion,
                        product.PrdFechaModificacion
                    },

                    // Información SEO
                    SeoInfo = new
                    {
                        product.PrdMetaTitulo,
                        product.PrdMetaDescripcion,
                        product.PrdPalabrasClaves,
                        UrlAmigable = $"/productos/{product.PrdSlug}"
                    },

                    // Información de categoría
                    Categoria = product.PrdCategoria != null ? new
                    {
                        Id = product.PrdCategoriaId,
                        Nombre = product.PrdCategoria.CatNombre,
                        Slug = product.PrdCategoria.CatSlug,
                        Activa = product.PrdCategoria.CatActivo
                    } : null,

                    // Información de marca
                    Marca = product.PrdMarca != null ? new
                    {
                        Id = product.PrdMarcaId,
                        Nombre = product.PrdMarca.MarNombre,
                        Logo = product.PrdMarca.MarLogo,
                        Activa = product.PrdMarca.MarActivo
                    } : null,

                    // Información de inventario
                    Inventario = product.Inventarios?.Select(i => new
                    {
                        i.InvId,
                        i.InvStock,
                        i.InvStockReservado,
                        i.InvStockMinimo,
                        i.InvStockMaximo,
                        i.InvFechaUltimaActualizacion,
                        StockDisponible = i.InvStock - i.InvStockReservado,
                        AlertaBajoStock = i.InvStock <= i.InvStockMinimo,
                        SinStock = i.InvStock <= 0
                    }).ToList(),

                    // Información de imágenes
                    Imagenes = product.ProductosImagenes?.Select(i => new
                    {
                        i.PimId,
                        i.PimUrl,
                        i.PimTextoAlternativo,
                        i.PimEsPrincipal,
                        i.PimOrden,
                        i.PimActivo,
                        UrlValida = !string.IsNullOrEmpty(i.PimUrl) && Uri.IsWellFormedUriString(i.PimUrl, UriKind.Absolute)
                    }).OrderBy(i => i.PimOrden).ToList(),

                    // Estadísticas y validaciones
                    Estadisticas = new
                    {
                        TotalImagenes = product.ProductosImagenes?.Count ?? 0,
                        ImagenesActivas = product.ProductosImagenes?.Count(i => i.PimActivo == true) ?? 0,
                        TieneImagenPrincipal = product.ProductosImagenes?.Any(i => i.PimEsPrincipal == true) ?? false,
                        TieneInventario = product.Inventarios?.Any() ?? false,
                        TieneStock = product.Inventarios?.Any(i => i.InvStock > 0) ?? false,
                        PrecioValido = product.PrdPrecio > 0,
                        SlugValido = !string.IsNullOrEmpty(product.PrdSlug),
                        SkuValido = !string.IsNullOrEmpty(product.PrdSku)
                    },

                    // Información del sistema
                    SistemaInfo = new
                    {
                        ConsultadoEn = DateTime.UtcNow,
                        ConsultadoPor = "ProductService.GetProductDebugInfoAsync",
                        VersionAPI = "1.0"
                    }
                };

                // ✅ 3. LOGGING DE ESTADÍSTICAS
                var stats = debugInfo.Estadisticas;
                Console.WriteLine($"📊 SERVICE - Estadísticas de debug:");
                Console.WriteLine($"   - Imágenes: {stats.TotalImagenes} total, {stats.ImagenesActivas} activas");
                Console.WriteLine($"   - Imagen principal: {(stats.TieneImagenPrincipal ? "✅" : "❌")}");
                Console.WriteLine($"   - Inventario: {(stats.TieneInventario ? "✅" : "❌")}");
                Console.WriteLine($"   - Stock: {(stats.TieneStock ? "✅" : "❌")}");
                Console.WriteLine($"   - Validaciones: Precio {(stats.PrecioValido ? "✅" : "❌")}, Slug {(stats.SlugValido ? "✅" : "❌")}, SKU {(stats.SkuValido ? "✅" : "❌")}");

                Console.WriteLine($"✅ SERVICE - GetProductDebugInfoAsync completado");
                return debugInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetProductDebugInfoAsync: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
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
                Console.WriteLine($"📸 SERVICE - GetProductImageAsync: Producto {productId}, Imagen {imageId}");

                // ✅ 1. BUSCAR LA IMAGEN ESPECÍFICA
                var image = await _context.ProductosImagenes
                    .FirstOrDefaultAsync(img => img.PimId == imageId && img.PimProductoId == productId);

                if (image == null)
                {
                    Console.WriteLine($"❌ SERVICE - Imagen {imageId} no encontrada en producto {productId}");
                    return null;
                }

                Console.WriteLine($"✅ SERVICE - Imagen encontrada: {image.PimUrl}");

                // ✅ 2. MAPEAR A DTO
                var imageDto = new ProductImageDto
                {
                    Id = image.PimId,
                    Url = image.PimUrl ?? string.Empty,
                    AltText = image.PimTextoAlternativo,
                    EsPrincipal = image.PimEsPrincipal ?? false,
                    Orden = image.PimOrden ?? 0,
                    Activo = image.PimActivo ?? true
                };

                // ✅ 3. LOGGING DE INFORMACIÓN DE LA IMAGEN
                Console.WriteLine($"📊 SERVICE - Detalles de la imagen:");
                Console.WriteLine($"   - URL: {imageDto.Url}");
                Console.WriteLine($"   - Alt Text: {imageDto.AltText ?? "No definido"}");
                Console.WriteLine($"   - Es Principal: {(imageDto.EsPrincipal ? "✅" : "❌")}");
                Console.WriteLine($"   - Orden: {imageDto.Orden}");
                Console.WriteLine($"   - Activa: {(imageDto.Activo ? "✅" : "❌")}");

                Console.WriteLine($"✅ SERVICE - GetProductImageAsync completado");
                return imageDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetProductImageAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene todas las imágenes de un producto
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Lista de imágenes del producto</returns>
        public async Task<IEnumerable<ProductImageDto>> GetProductImagesAsync(int productId)
        {
            try
            {
                Console.WriteLine($"📸 SERVICE - GetProductImagesAsync: Obteniendo imágenes del producto {productId}");

                // ✅ 1. VERIFICAR QUE EL PRODUCTO EXISTE
                var productExists = await _context.Productos.AnyAsync(p => p.PrdId == productId);
                if (!productExists)
                {
                    Console.WriteLine($"❌ SERVICE - Producto {productId} no encontrado");
                    return Enumerable.Empty<ProductImageDto>();
                }

                // ✅ 2. OBTENER IMÁGENES USANDO EL SERVICIO DEDICADO
                var images = await _imageService.GetImagenesByProductoIdAsync(productId);
                var imagesList = images.ToList();

                Console.WriteLine($"📦 SERVICE - Encontradas {imagesList.Count} imágenes para producto {productId}");

                // ✅ 3. MAPEAR A DTO
                var result = imagesList.Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    Url = img.Url ?? string.Empty,
                    AltText = img.AltText,
                    EsPrincipal = img.EsPrincipal,
                    Orden = img.Orden,
                    Activo = true // El servicio de imágenes ya filtra las activas
                }).ToList();

                // ✅ 4. ESTADÍSTICAS Y VALIDACIONES
                var mainImages = result.Count(img => img.EsPrincipal);
                var hasMainImage = mainImages > 0;
                var hasMultipleMain = mainImages > 1;

                Console.WriteLine($"📊 SERVICE - Estadísticas de imágenes:");
                Console.WriteLine($"   - Total imágenes: {result.Count}");
                Console.WriteLine($"   - Imagen principal: {(hasMainImage ? "✅" : "❌")}");
                if (hasMultipleMain)
                {
                    Console.WriteLine($"   ⚠️ ADVERTENCIA: {mainImages} imágenes marcadas como principales");
                }

                // ✅ 5. MOSTRAR DETALLES DE PRIMERAS IMÁGENES
                foreach (var img in result.Take(3))
                {
                    Console.WriteLine($"   - Imagen {img.Id}: {(img.EsPrincipal ? "PRINCIPAL" : $"Orden {img.Orden}")} - {img.Url}");
                }

                if (result.Count > 3)
                {
                    Console.WriteLine($"   ... y {result.Count - 3} imágenes más");
                }

                Console.WriteLine($"✅ SERVICE - GetProductImagesAsync completado: {result.Count} imágenes");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetProductImagesAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene la URL de la imagen principal de un producto
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>URL de la imagen principal o null si no tiene</returns>
        public async Task<string?> GetProductMainImageUrlAsync(int productId)
        {
            try
            {
                Console.WriteLine($"📸 SERVICE - GetProductMainImageUrlAsync: Obteniendo imagen principal del producto {productId}");

                // ✅ 1. USAR EL SERVICIO DEDICADO DE IMÁGENES
                var mainImage = await _imageService.GetImagenPrincipalByProductoIdAsync(productId);

                if (mainImage == null)
                {
                    Console.WriteLine($"❌ SERVICE - No se encontró imagen principal para producto {productId}");
                    return null;
                }

                var imageUrl = mainImage.Url;
                Console.WriteLine($"✅ SERVICE - Imagen principal encontrada: {imageUrl}");

                // ✅ 2. VALIDAR QUE LA URL SEA VÁLIDA
                if (string.IsNullOrEmpty(imageUrl))
                {
                    Console.WriteLine($"⚠️ SERVICE - URL de imagen principal vacía para producto {productId}");
                    return null;
                }

                // ✅ 3. VALIDACIÓN OPCIONAL DE URL BIEN FORMADA
                if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                {
                    Console.WriteLine($"⚠️ SERVICE - URL de imagen principal mal formada: {imageUrl}");
                    // Aún retornamos la URL porque podría ser relativa válida
                }

                Console.WriteLine($"✅ SERVICE - GetProductMainImageUrlAsync completado");
                return imageUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetProductMainImageUrlAsync: {ex.Message}");
                return null; // No lanzar excepción para este método, solo retornar null
            }
        }


        /// <summary>
        /// Construye el query base para productos con todos los filtros aplicados
        /// </summary>
        /// <param name="filter">Filtros a aplicar</param>
        /// <returns>Query configurado</returns>
        private IQueryable<Producto> BuildProductQuery(ProductFilterDto filter)
        {
            try
            {
                // ✅ 1. QUERY BASE CON INCLUDES
                var query = _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .AsQueryable();

                // ✅ 2. FILTRO POR ESTADO ACTIVO/INACTIVO
                if (filter.Activo.HasValue)
                {
                    query = query.Where(p => p.PrdActivo == filter.Activo.Value);
                }
                else if (!filter.IncluirInactivos)
                {
                    // Por defecto, solo mostrar productos activos si no se especifica incluir inactivos
                    query = query.Where(p => p.PrdActivo == true);
                }

                // ✅ 3. FILTRO POR BÚSQUEDA DE TEXTO
                if (!string.IsNullOrEmpty(filter.Busqueda))
                {
                    var searchTerm = filter.Busqueda.ToLower().Trim();
                    query = query.Where(p =>
                        p.PrdNombre.ToLower().Contains(searchTerm) ||
                        p.PrdSku.ToLower().Contains(searchTerm) ||
                        (p.PrdDescripcionCorta != null && p.PrdDescripcionCorta.ToLower().Contains(searchTerm)) ||
                        (p.PrdDescripcionLarga != null && p.PrdDescripcionLarga.ToLower().Contains(searchTerm)) ||
                        (p.PrdCategoria != null && p.PrdCategoria.CatNombre.ToLower().Contains(searchTerm)) ||
                        (p.PrdMarca != null && p.PrdMarca.MarNombre.ToLower().Contains(searchTerm))
                    );
                }

                // ✅ 4. FILTRO POR SKU ESPECÍFICO
                if (!string.IsNullOrEmpty(filter.SKU))
                {
                    query = query.Where(p => p.PrdSku.ToLower().Contains(filter.SKU.ToLower()));
                }

                // ✅ 5. FILTRO POR CATEGORÍA
                if (filter.CategoriaId.HasValue)
                {
                    query = query.Where(p => p.PrdCategoriaId == filter.CategoriaId.Value);
                }

                // ✅ 6. FILTRO POR MARCA
                if (filter.MarcaId.HasValue)
                {
                    query = query.Where(p => p.PrdMarcaId == filter.MarcaId.Value);
                }

                // ✅ 7. FILTRO POR RANGO DE PRECIOS
                if (filter.PrecioMin.HasValue)
                {
                    query = query.Where(p => p.PrdPrecio >= filter.PrecioMin.Value);
                }

                if (filter.PrecioMax.HasValue)
                {
                    query = query.Where(p => p.PrdPrecio <= filter.PrecioMax.Value);
                }

                // ✅ 8. FILTROS DE ESTADOS ESPECIALES
                if (filter.EnOferta.HasValue)
                {
                    query = query.Where(p => p.PrdEnOferta == filter.EnOferta.Value);
                }

                if (filter.Destacado.HasValue)
                {
                    query = query.Where(p => p.PrdDestacado == filter.Destacado.Value);
                }

                // ✅ 9. FILTROS DE STOCK
                if (filter.BajoStock.HasValue && filter.BajoStock.Value)
                {
                    query = query.Where(p => p.Inventarios.Any(i => i.InvStock > 0 && i.InvStock <= i.InvStockMinimo));
                }

                if (filter.SinStock.HasValue && filter.SinStock.Value)
                {
                    query = query.Where(p => p.Inventarios.Any(i => i.InvStock <= 0));
                }
                else if (filter.SinStock.HasValue && !filter.SinStock.Value)
                {
                    // Excluir productos sin stock
                    query = query.Where(p => p.Inventarios.Any(i => i.InvStock > 0));
                }

                // ✅ 10. FILTROS POR RANGO DE FECHAS
                if (filter.FechaDesde.HasValue)
                {
                    query = query.Where(p => p.PrdFechaCreacion >= filter.FechaDesde.Value);
                }

                if (filter.FechaHasta.HasValue)
                {
                    // Incluir todo el día hasta las 23:59:59
                    var fechaHastaFinal = filter.FechaHasta.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(p => p.PrdFechaCreacion <= fechaHastaFinal);
                }

                // ✅ 11. APLICAR ORDENAMIENTO
                var sortBy = filter.SortBy?.ToLower() ?? "nombre";
                var sortDesc = filter.SortDescending;

                query = sortBy switch
                {
                    "nombre" => sortDesc
                        ? query.OrderByDescending(p => p.PrdNombre)
                        : query.OrderBy(p => p.PrdNombre),

                    "precio" => sortDesc
                        ? query.OrderByDescending(p => p.PrdPrecio)
                        : query.OrderBy(p => p.PrdPrecio),

                    "fecha" or "fechacreacion" => sortDesc
                        ? query.OrderByDescending(p => p.PrdFechaCreacion)
                        : query.OrderBy(p => p.PrdFechaCreacion),

                    "fechamodificacion" => sortDesc
                        ? query.OrderByDescending(p => p.PrdFechaModificacion)
                        : query.OrderBy(p => p.PrdFechaModificacion),

                    "stock" => sortDesc
                        ? query.OrderByDescending(p => p.Inventarios.Sum(i => (int?)i.InvStock) ?? 0)
                        : query.OrderBy(p => p.Inventarios.Sum(i => (int?)i.InvStock) ?? 0),

                    "categoria" => sortDesc
                        ? query.OrderByDescending(p => p.PrdCategoria.CatNombre)
                        : query.OrderBy(p => p.PrdCategoria.CatNombre),

                    "marca" => sortDesc
                        ? query.OrderByDescending(p => p.PrdMarca.MarNombre)
                        : query.OrderBy(p => p.PrdMarca.MarNombre),

                    "sku" => sortDesc
                        ? query.OrderByDescending(p => p.PrdSku)
                        : query.OrderBy(p => p.PrdSku),

                    "orden" => sortDesc
                        ? query.OrderByDescending(p => p.PrdOrden ?? int.MaxValue)
                        : query.OrderBy(p => p.PrdOrden ?? int.MaxValue),

                    "estado" => sortDesc
                        ? query.OrderByDescending(p => p.PrdEstado)
                        : query.OrderBy(p => p.PrdEstado),

                    "destacado" => sortDesc
                        ? query.OrderByDescending(p => p.PrdDestacado)
                        : query.OrderBy(p => p.PrdDestacado),

                    "oferta" => sortDesc
                        ? query.OrderByDescending(p => p.PrdEnOferta)
                        : query.OrderBy(p => p.PrdEnOferta),

                    _ => query.OrderBy(p => p.PrdNombre) // Default fallback
                };

                // ✅ 12. ORDENAMIENTO SECUNDARIO PARA CONSISTENCIA
                // Agregar ordenamiento secundario por ID para garantizar resultados consistentes en paginación
                if (sortBy != "nombre")
                {
                    query = sortDesc
                        ? ((IOrderedQueryable<Producto>)query).ThenByDescending(p => p.PrdId)
                        : ((IOrderedQueryable<Producto>)query).ThenBy(p => p.PrdId);
                }

                return query;
            }
            catch
            {
                // En caso de error, retornar query básico
                return _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true)
                    .OrderBy(p => p.PrdNombre);
            }
        }

        /// <summary>
        /// Obtiene productos con filtros y paginación
        /// </summary>
        /// <param name="filter">Filtros de búsqueda y paginación</param>
        /// <returns>Resultado paginado de productos</returns>
        public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            try
            {


                // ✅ 1. CONSTRUIR QUERY CON FILTROS
                var query = BuildProductQuery(filter);

                // ✅ 2. CONTAR TOTAL DE ELEMENTOS PRIMERO (OPTIMIZACIÓN)
                var totalItems = await query.CountAsync();


                if (totalItems == 0)
                {

                    return new PagedResult<ProductDto>
                    {
                        Items = new List<ProductDto>(),
                        TotalItems = 0,
                        Page = filter.Page,
                        PageSize = filter.PageSize
                    };
                }

                // ✅ 3. APLICAR PAGINACIÓN Y CARGAR PRODUCTOS
                var products = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();



                // ✅ 4. MAPEAR A DTO (ASÍNCRONO PARA CARGAR IMÁGENES)
                var productDtos = new List<ProductDto>();
                foreach (var product in products)
                {
                    var dto = await MapToProductDtoAsync(product);
                    productDtos.Add(dto);
                }

                // ✅ 5. CONSTRUIR RESULTADO PAGINADO
                var result = new PagedResult<ProductDto>
                {
                    Items = productDtos,
                    TotalItems = totalItems,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };


                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Obtiene productos por marca específica
        /// </summary>
        /// <param name="brandId">ID de la marca</param>
        /// <param name="count">Cantidad de productos a retornar</param>
        /// <returns>Lista de productos de la marca</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetProductsByBrandAsync(int brandId, int count = 12)
        {
            try
            {

                // ✅ 1. VERIFICAR QUE LA MARCA EXISTE
                var brand = await _context.Marcas.FirstOrDefaultAsync(m => m.MarId == brandId);
                if (brand == null)
                {

                    return Enumerable.Empty<ProductSummaryDto>();
                }



                // ✅ 2. CONSULTAR PRODUCTOS DE LA MARCA
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true && p.PrdMarcaId == brandId)
                    .OrderBy(p => p.PrdOrden ?? int.MaxValue) // Orden específico primero
                    .ThenByDescending(p => p.PrdDestacado) // Destacados después
                    .ThenByDescending(p => p.PrdFechaCreacion) // Más recientes después
                    .ThenBy(p => p.PrdNombre) // Alfabético al final
                    .Take(count)
                    .ToListAsync();



                // ✅ 3. ESTADÍSTICAS DE LOS PRODUCTOS
                if (products.Any())
                {
                    var destacados = products.Count(p => p.PrdDestacado == true);
                    var enOferta = products.Count(p => p.PrdEnOferta == true);
                    var nuevos = products.Count(p => p.PrdNuevo == true);
                    var conStock = products.Count(p => p.Inventarios.Any(i => i.InvStock > 0));


                    // Mostrar algunos productos en log
                    foreach (var product in products.Take(3))
                    {
                        var stock = product.Inventarios?.FirstOrDefault()?.InvStock ?? 0;

                    }

                    if (products.Count > 3)
                    {
                    }
                }

                // ✅ 4. MAPEAR A DTO SUMMARY
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }


                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Obtiene productos por marca con filtros adicionales
        /// </summary>
        /// <param name="brandId">ID de la marca</param>
        /// <param name="filter">Filtros adicionales</param>
        /// <returns>Resultado paginado de productos de la marca</returns>
        public async Task<PagedResult<ProductSummaryDto>> GetProductsByBrandWithFiltersAsync(int brandId, ProductFilterDto filter)
        {
            try
            {

                // ✅ 1. VERIFICAR QUE LA MARCA EXISTE
                var brand = await _context.Marcas.FirstOrDefaultAsync(m => m.MarId == brandId);
                if (brand == null)
                {

                    return new PagedResult<ProductSummaryDto>
                    {
                        Items = new List<ProductSummaryDto>(),
                        TotalItems = 0,
                        Page = filter.Page,
                        PageSize = filter.PageSize
                    };
                }

                // ✅ 2. FORZAR FILTRO POR MARCA
                filter.MarcaId = brandId;

                // ✅ 3. USAR EL MÉTODO PRINCIPAL DE PRODUCTOS PERO MAPEAR A SUMMARY
                var productsResult = await GetProductsAsync(filter);

                // ✅ 4. CONVERTIR A SUMMARY DTOs
                var summaryItems = productsResult.Items.Select(MapToProductSummaryDto).ToList();

                var result = new PagedResult<ProductSummaryDto>
                {
                    Items = summaryItems,
                    TotalItems = productsResult.TotalItems,
                    Page = productsResult.Page,
                    PageSize = productsResult.PageSize
                };


                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Obtiene las marcas más populares basadas en cantidad de productos
        /// </summary>
        /// <param name="count">Cantidad de marcas a retornar</param>
        /// <returns>Lista de marcas con cantidad de productos</returns>
        public async Task<IEnumerable<object>> GetTopBrandsAsync(int count = 10)
        {
            try
            {

                var topBrands = await _context.Productos
                    .Include(p => p.PrdMarca)
                    .Where(p => p.PrdActivo == true && p.PrdMarca != null)
                    .GroupBy(p => new { p.PrdMarcaId, p.PrdMarca.MarNombre, p.PrdMarca.MarLogo })
                    .Select(g => new
                    {
                        MarcaId = g.Key.PrdMarcaId,
                        Nombre = g.Key.MarNombre,
                        Logo = g.Key.MarLogo,
                        CantidadProductos = g.Count(),
                        ProductosDestacados = g.Count(p => p.PrdDestacado == true),
                        ProductosEnOferta = g.Count(p => p.PrdEnOferta == true),
                        PrecioPromedio = g.Average(p => p.PrdPrecio)
                    })
                    .OrderByDescending(x => x.CantidadProductos)
                    .Take(count)
                    .ToListAsync();



                foreach (var brand in topBrands.Take(5))
                {

                }

                return topBrands;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Obtiene productos por categoría específica
        /// </summary>
        /// <param name="categoryId">ID de la categoría</param>
        /// <param name="count">Cantidad de productos a retornar</param>
        /// <returns>Lista de productos de la categoría</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetProductsByCategoryAsync(int categoryId, int count = 12)
        {
            try
            {


                // ✅ 1. VERIFICAR QUE LA CATEGORÍA EXISTE
                var category = await _context.Categorias.FirstOrDefaultAsync(c => c.CatId == categoryId);
                if (category == null)
                {

                    return Enumerable.Empty<ProductSummaryDto>();
                }



                // ✅ 2. CONSULTAR PRODUCTOS DE LA CATEGORÍA
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true && p.PrdCategoriaId == categoryId)
                    .OrderBy(p => p.PrdOrden ?? int.MaxValue) // Orden específico primero
                    .ThenByDescending(p => p.PrdDestacado) // Destacados después
                    .ThenByDescending(p => p.PrdEnOferta) // En oferta después
                    .ThenByDescending(p => p.PrdFechaCreacion) // Más recientes después
                    .ThenBy(p => p.PrdNombre) // Alfabético al final
                    .Take(count)
                    .ToListAsync();



                // ✅ 3. ESTADÍSTICAS DE LOS PRODUCTOS
                if (products.Any())
                {
                    var destacados = products.Count(p => p.PrdDestacado == true);
                    var enOferta = products.Count(p => p.PrdEnOferta == true);
                    var nuevos = products.Count(p => p.PrdNuevo == true);
                    var conStock = products.Count(p => p.Inventarios.Any(i => i.InvStock > 0));
                    var precioPromedio = products.Average(p => p.PrdPrecio);
                    var precioMin = products.Min(p => p.PrdPrecio);
                    var precioMax = products.Max(p => p.PrdPrecio);



                    // Mostrar algunos productos en log
                    foreach (var product in products.Take(3))
                    {
                        var stock = product.Inventarios?.FirstOrDefault()?.InvStock ?? 0;
                        var flags = new List<string>();
                        if (product.PrdDestacado == true) flags.Add("DESTACADO");
                        if (product.PrdEnOferta == true) flags.Add("OFERTA");
                        if (product.PrdNuevo == true) flags.Add("NUEVO");


                    }

                    if (products.Count > 3)
                    {

                    }
                }

                // ✅ 4. MAPEAR A DTO SUMMARY
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }


                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Obtiene productos en oferta
        /// </summary>
        /// <param name="count">Cantidad de productos a retornar</param>
        /// <returns>Lista de productos en oferta</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetProductsOnSaleAsync(int count = 12)
        {
            try
            {
                Console.WriteLine($"🏷️ SERVICE - GetProductsOnSaleAsync: Obteniendo {count} productos en oferta");

                // ✅ 1. CONSULTAR PRODUCTOS EN OFERTA
                var products = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true && p.PrdEnOferta == true)
                    .OrderBy(p => p.PrdOrden ?? int.MaxValue) // Orden específico primero
                    .ThenByDescending(p => p.PrdDestacado) // Destacados después
                    .ThenByDescending(p => p.PrdFechaModificacion) // Ofertas más recientes
                    .ThenBy(p => p.PrdNombre) // Alfabético al final
                    .Take(count)
                    .ToListAsync();


                // ✅ 2. ESTADÍSTICAS Y ANÁLISIS DE OFERTAS
                if (products.Any())
                {
                    var productosConPrecioComparacion = products.Count(p => p.PrdPrecioComparacion.HasValue && p.PrdPrecioComparacion > p.PrdPrecio);
                    var descuentoPromedio = products
                        .Where(p => p.PrdPrecioComparacion.HasValue && p.PrdPrecioComparacion > p.PrdPrecio)
                        .Average(p => ((p.PrdPrecioComparacion!.Value - p.PrdPrecio) / p.PrdPrecioComparacion.Value) * 100);

                    var conStock = products.Count(p => p.Inventarios.Any(i => i.InvStock > 0));
                    var destacados = products.Count(p => p.PrdDestacado == true);


                    // Mostrar ofertas más atractivas
                    var mejoresOfertas = products
                        .Where(p => p.PrdPrecioComparacion.HasValue && p.PrdPrecioComparacion > p.PrdPrecio)
                        .OrderByDescending(p => ((p.PrdPrecioComparacion!.Value - p.PrdPrecio) / p.PrdPrecioComparacion.Value) * 100)
                        .Take(3);


                    foreach (var oferta in mejoresOfertas)
                    {
                        var descuento = ((oferta.PrdPrecioComparacion!.Value - oferta.PrdPrecio) / oferta.PrdPrecioComparacion.Value) * 100;

                    }
                }

                // ✅ 3. MAPEAR A DTO SUMMARY
                var result = new List<ProductSummaryDto>();
                foreach (var product in products)
                {
                    var summary = await MapToProductSummaryDtoAsync(product);
                    result.Add(summary);
                }


                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas completas de productos
        /// </summary>
        /// <returns>Estadísticas completas de productos</returns>
        public async Task<ProductStatsDto> GetProductStatsAsync()
        {
            try
            {
                // ✅ 1. ESTADÍSTICAS BÁSICAS DE PRODUCTOS
                var totalProducts = await _context.Productos.CountAsync();
                var activeProducts = await _context.Productos.CountAsync(p => p.PrdActivo == true);
                var inactiveProducts = totalProducts - activeProducts;
                var featuredProducts = await _context.Productos.CountAsync(p => p.PrdActivo == true && p.PrdDestacado == true);
                var onSaleProducts = await _context.Productos.CountAsync(p => p.PrdActivo == true && p.PrdEnOferta == true);

                // ✅ 2. ESTADÍSTICAS DE INVENTARIO
                var lowStockCount = await _context.Inventarios
                    .CountAsync(i => i.InvStock <= i.InvStockMinimo && i.InvStock > 0);

                var outOfStockCount = await _context.Inventarios
                    .CountAsync(i => i.InvStock <= 0);

                // ✅ 3. ESTADÍSTICAS DE PRECIOS Y VALOR DE INVENTARIO
                var activeProductsWithInventory = await _context.Productos
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true)
                    .ToListAsync();

                var prices = activeProductsWithInventory.Select(p => p.PrdPrecio).ToList();

                decimal avgPrice = 0;
                decimal? minPrice = null;
                decimal? maxPrice = null;
                decimal totalInventoryValue = 0;

                if (prices.Any())
                {
                    avgPrice = prices.Average();
                    minPrice = prices.Min();
                    maxPrice = prices.Max();

                    // Calcular valor total del inventario
                    totalInventoryValue = activeProductsWithInventory
                        .Sum(p => p.PrdPrecio * (p.Inventarios?.Sum(i => (decimal?)i.InvStock) ?? 0));
                }

                // ✅ 4. FECHA DEL ÚLTIMO PRODUCTO CREADO
                var lastProductDate = await _context.Productos
                    .OrderByDescending(p => p.PrdFechaCreacion)
                    .Select(p => p.PrdFechaCreacion)
                    .FirstOrDefaultAsync();

                // ✅ 5. TOP PRODUCTOS MÁS VENDIDOS
                var topSellingProducts = await _context.PedidosItems
                    .Include(pi => pi.PitProducto)
                    .Where(pi => pi.PitProducto != null && pi.PitProducto.PrdActivo == true)
                    .GroupBy(pi => new
                    {
                        pi.PitProductoId,
                        pi.PitProducto.PrdNombre,
                        pi.PitProducto.PrdSku
                    })
                    .Select(g => new ProductSalesStatsDto
                    {
                        Id = g.Key.PitProductoId,
                        Nombre = g.Key.PrdNombre ?? "Sin nombre",
                        SKU = g.Key.PrdSku ?? "Sin SKU",
                        TotalVentas = g.Sum(pi => pi.PitCantidad),
                        IngresoTotal = g.Sum(pi => pi.PitPrecio * pi.PitCantidad)
                    })
                    .OrderByDescending(x => x.TotalVentas)
                    .Take(10)
                    .ToListAsync();

                // ✅ 6. ESTADÍSTICAS POR CATEGORÍA (COMO OBJETOS ANÓNIMOS)
                var productsByCategory = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Where(p => p.PrdActivo == true)
                    .GroupBy(p => new
                    {
                        CategoriaId = p.PrdCategoriaId,
                        CategoriaNombre = p.PrdCategoria.CatNombre
                    })
                    .Select(g => new
                    {
                        CategoriaId = g.Key.CategoriaId,
                        CategoriaNombre = g.Key.CategoriaNombre ?? "Sin nombre",
                        CantidadProductos = g.Count(),
                        ProductosActivos = g.Count(p => p.PrdActivo == true),
                        ProductosDestacados = g.Count(p => p.PrdDestacado == true),
                        PrecioPromedio = g.Average(p => p.PrdPrecio)
                    })
                    .OrderByDescending(x => x.CantidadProductos)
                    .ToListAsync();

                // ✅ 7. ESTADÍSTICAS POR MARCA (COMO OBJETOS ANÓNIMOS)
                var productsByBrand = await _context.Productos
                    .Include(p => p.PrdMarca)
                    .Where(p => p.PrdActivo == true)
                    .GroupBy(p => new
                    {
                        MarcaId = p.PrdMarcaId,
                        MarcaNombre = p.PrdMarca.MarNombre
                    })
                    .Select(g => new
                    {
                        MarcaId = g.Key.MarcaId,
                        MarcaNombre = g.Key.MarcaNombre ?? "Sin nombre",
                        CantidadProductos = g.Count(),
                        ProductosActivos = g.Count(p => p.PrdActivo == true),
                        ProductosDestacados = g.Count(p => p.PrdDestacado == true),
                        PrecioPromedio = g.Average(p => p.PrdPrecio)
                    })
                    .OrderByDescending(x => x.CantidadProductos)
                    .ToListAsync();

                // ✅ 8. CONSTRUIR DTO DE ESTADÍSTICAS
                var stats = new ProductStatsDto
                {
                    // Estadísticas básicas
                    TotalProductos = totalProducts,
                    ProductosActivos = activeProducts,
                    ProductosInactivos = inactiveProducts,
                    ProductosDestacados = featuredProducts,
                    ProductosEnOferta = onSaleProducts,
                    ProductosBajoStock = lowStockCount,
                    ProductosSinStock = outOfStockCount,

                    // Estadísticas financieras
                    ValorTotalInventario = Math.Round(totalInventoryValue, 2),
                    PrecioPromedio = Math.Round(avgPrice, 2),
                    PrecioMin = minPrice,
                    PrecioMax = maxPrice,

                    // Información temporal
                    UltimoProductoCreado = lastProductDate,

                    // Lista de productos más vendidos
                    TopProductosVendidos = topSellingProducts,

                    // Listas vacías para categorías y marcas (si no tienes los DTOs)
                    ProductosPorCategoria = new List<CategoryProductCountDto>(),  // Temporalmente como object
                    ProductosPorMarca = new List<BrandProductCountDto>()       // Temporalmente como object
                };

                return stats;
            }
            catch (Exception ex)
            {
                // En caso de error, retornar estadísticas básicas
                return new ProductStatsDto
                {
                    TotalProductos = 0,
                    ProductosActivos = 0,
                    ProductosInactivos = 0,
                    ProductosDestacados = 0,
                    ProductosEnOferta = 0,
                    ProductosBajoStock = 0,
                    ProductosSinStock = 0,
                    ValorTotalInventario = 0,
                    PrecioPromedio = 0,
                    PrecioMin = null,
                    PrecioMax = null,
                    UltimoProductoCreado = null,
                    TopProductosVendidos = new List<ProductSalesStatsDto>(),
                    ProductosPorCategoria = new List<CategoryProductCountDto>(),
                    ProductosPorMarca = new List<BrandProductCountDto>()
                };
            }
        }

        /// <summary>
        /// Obtiene productos relacionados a un producto específico
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="count">Cantidad de productos relacionados</param>
        /// <returns>Lista de productos relacionados</returns>
        public async Task<IEnumerable<ProductSummaryDto>> GetRelatedProductsAsync(int productId, int count = 6)
        {
            try
            {
                Console.WriteLine($"🔗 SERVICE - GetRelatedProductsAsync: Productos relacionados para ID {productId}, máximo {count}");

                // ✅ 1. OBTENER EL PRODUCTO BASE
                var product = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .FirstOrDefaultAsync(p => p.PrdId == productId);

                if (product == null)
                {
                    Console.WriteLine($"❌ SERVICE - Producto {productId} no encontrado");
                    return Enumerable.Empty<ProductSummaryDto>();
                }

                Console.WriteLine($"✅ SERVICE - Producto base: {product.PrdNombre}");
                Console.WriteLine($"   - Categoría: {product.PrdCategoria?.CatNombre ?? "Sin categoría"}");
                Console.WriteLine($"   - Marca: {product.PrdMarca?.MarNombre ?? "Sin marca"}");

                // ✅ 2. BUSCAR PRODUCTOS RELACIONADOS CON ALGORITMO DE RELEVANCIA
                var relatedQuery = _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Where(p => p.PrdActivo == true && p.PrdId != productId);

                // ✅ 3. APLICAR CRITERIOS DE RELACIÓN CON PUNTUACIÓN
                var relatedProducts = await relatedQuery
                    .Select(p => new
                    {
                        Product = p,
                        RelevanceScore =
                            // Misma categoría: +100 puntos
                            (p.PrdCategoriaId == product.PrdCategoriaId ? 100 : 0) +
                            // Misma marca: +50 puntos
                            (p.PrdMarcaId == product.PrdMarcaId ? 50 : 0) +
                            // Rango de precio similar (±20%): +30 puntos
                            (Math.Abs(p.PrdPrecio - product.PrdPrecio) <= (product.PrdPrecio * 0.2m) ? 30 : 0) +
                            // Producto destacado: +20 puntos
                            (p.PrdDestacado == true ? 20 : 0) +
                            // En oferta: +15 puntos
                            (p.PrdEnOferta == true ? 15 : 0) +
                            // Nuevo: +10 puntos
                            (p.PrdNuevo == true ? 10 : 0) +
                            // Con stock: +5 puntos
                            (p.Inventarios.Any(i => i.InvStock > 0) ? 5 : 0)
                    })
                    .Where(x => x.RelevanceScore > 0) // Solo productos con alguna relación
                    .OrderByDescending(x => x.RelevanceScore)
                    .ThenBy(x => Guid.NewGuid()) // Aleatorio como desempate
                    .Take(count * 2) // Obtener más para filtrar después
                    .ToListAsync();

                Console.WriteLine($"🎯 SERVICE - Encontrados {relatedProducts.Count} productos relacionados con puntuación");

                // ✅ 4. FILTRAR Y OPTIMIZAR SELECCIÓN
                var selectedProducts = relatedProducts
                    .Take(count)
                    .Select(x => x.Product)
                    .ToList();

                // ✅ 5. LOGGING DE CRITERIOS DE RELACIÓN
                if (selectedProducts.Any())
                {
                    var sameCategoryCount = selectedProducts.Count(p => p.PrdCategoriaId == product.PrdCategoriaId);
                    var sameBrandCount = selectedProducts.Count(p => p.PrdMarcaId == product.PrdMarcaId);
                    var inStockCount = selectedProducts.Count(p => p.Inventarios.Any(i => i.InvStock > 0));

                    Console.WriteLine($"📊 SERVICE - Análisis de productos relacionados:");
                    Console.WriteLine($"   - Misma categoría: {sameCategoryCount}");
                    Console.WriteLine($"   - Misma marca: {sameBrandCount}");
                    Console.WriteLine($"   - Con stock: {inStockCount}");

                    // Mostrar algunos productos relacionados
                    foreach (var related in selectedProducts.Take(3))
                    {
                        var criteria = new List<string>();
                        if (related.PrdCategoriaId == product.PrdCategoriaId) criteria.Add("CAT");
                        if (related.PrdMarcaId == product.PrdMarcaId) criteria.Add("MARCA");
                        if (Math.Abs(related.PrdPrecio - product.PrdPrecio) <= (product.PrdPrecio * 0.2m)) criteria.Add("PRECIO");
                        if (related.PrdDestacado == true) criteria.Add("DEST");

                        Console.WriteLine($"   - {related.PrdNombre}: ${related.PrdPrecio} [{string.Join(",", criteria)}]");
                    }
                }

                // ✅ 6. MAPEAR A DTO SUMMARY
                var result = new List<ProductSummaryDto>();
                foreach (var relatedProduct in selectedProducts)
                {
                    var summary = await MapToProductSummaryDtoAsync(relatedProduct);
                    result.Add(summary);
                }

                Console.WriteLine($"✅ SERVICE - GetRelatedProductsAsync completado: {result.Count} productos relacionados");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en GetRelatedProductsAsync: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// Verifica si existe un producto por nombre
        /// </summary>
        /// <param name="name">Nombre del producto</param>
        /// <param name="excludeId">ID a excluir de la búsqueda (para actualizaciones)</param>
        /// <returns>True si existe un producto con ese nombre</returns>
        public async Task<bool> ProductExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - ProductExistsAsync: Verificando existencia del nombre '{name}'");

                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine($"⚠️ SERVICE - Nombre vacío o nulo");
                    return false;
                }

                // ✅ 1. CONSTRUIR QUERY BASE
                var query = _context.Productos.Where(p => p.PrdNombre == name);

                // ✅ 2. EXCLUIR ID SI SE PROPORCIONA (PARA ACTUALIZACIONES)
                if (excludeId.HasValue)
                {
                    query = query.Where(p => p.PrdId != excludeId.Value);
                    Console.WriteLine($"   - Excluyendo producto ID: {excludeId.Value}");
                }

                // ✅ 3. VERIFICAR EXISTENCIA
                var exists = await query.AnyAsync();

                Console.WriteLine($"🔍 SERVICE - ProductExistsAsync resultado:");
                Console.WriteLine($"   - Nombre: '{name}'");
                Console.WriteLine($"   - Excluir ID: {excludeId?.ToString() ?? "Ninguno"}");
                Console.WriteLine($"   - Existe: {(exists ? "✅ SÍ" : "❌ NO")}");

                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en ProductExistsAsync: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Verifica si un producto tiene imágenes activas
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>True si tiene imágenes</returns>
        public async Task<bool> ProductHasImagesAsync(int productId)
        {
            try
            {
                Console.WriteLine($"📸 SERVICE - ProductHasImagesAsync: Verificando imágenes del producto {productId}");

                // ✅ 1. DELEGAR AL SERVICIO ESPECIALIZADO DE IMÁGENES
                var hasImages = await _imageService.ProductoHasImagenesAsync(productId);

                Console.WriteLine($"📸 SERVICE - ProductHasImagesAsync resultado:");
                Console.WriteLine($"   - Producto ID: {productId}");
                Console.WriteLine($"   - Tiene imágenes: {(hasImages ? "✅ SÍ" : "❌ NO")}");

                // ✅ 2. INFORMACIÓN ADICIONAL PARA DEBUG
                if (hasImages)
                {
                    try
                    {
                        var images = await _imageService.GetImagenesByProductoIdAsync(productId);
                        var imagesList = images.ToList();
                        var principalImage = imagesList.FirstOrDefault(img => img.EsPrincipal);

                        Console.WriteLine($"   - Total imágenes: {imagesList.Count}");
                        Console.WriteLine($"   - Imagen principal: {(principalImage != null ? "✅" : "❌")}");
                        if (principalImage != null)
                        {
                            Console.WriteLine($"   - URL principal: {principalImage.Url}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ⚠️ Error obteniendo detalles de imágenes: {ex.Message}");
                    }
                }

                return hasImages;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en ProductHasImagesAsync: {ex.Message}");
                return false; // En caso de error, asumir que no tiene imágenes
            }
        }


        /// <summary>
        /// Reordena automáticamente las imágenes de un producto (método interno)
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>True si se reordenó exitosamente</returns>
        private async Task<bool> ReorderProductImagesInternalAsync(int productId)
        {
            try
            {
                // Obtener todas las imágenes activas del producto ordenadas
                var images = await _context.ProductosImagenes
                    .Where(img => img.PimProductoId == productId && img.PimActivo == true)
                    .OrderBy(img => img.PimOrden ?? int.MaxValue)
                    .ThenBy(img => img.PimId)
                    .ToListAsync();

                if (!images.Any())
                {
                    return true; // No es error, simplemente no hay nada que hacer
                }

                // Reordenar secuencialmente
                for (int i = 0; i < images.Count; i++)
                {
                    var newOrder = i + 1;
                    if ((images[i].PimOrden ?? 0) != newOrder)
                    {
                        images[i].PimOrden = newOrder;
                    }
                }

                var changesCount = await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reordena las imágenes de un producto
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>True si se reordenó exitosamente</returns>
        public async Task<bool> ReorderProductImagesAsync(int productId)
        {
            try
            {
                Console.WriteLine($"🔄 SERVICE - ReorderProductImagesAsync: Reordenando imágenes del producto {productId}");

                // ✅ 1. DELEGAR AL MÉTODO PRIVADO INTERNO
                var result = await ReorderProductImagesInternalAsync(productId);

                Console.WriteLine($"🔄 SERVICE - ReorderProductImagesAsync resultado:");
                Console.WriteLine($"   - Producto ID: {productId}");
                Console.WriteLine($"   - Reordenado: {(result ? "✅ EXITOSO" : "❌ FALLÓ")}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en ReorderProductImagesAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Busca productos con filtros avanzados y retorna resultados con filtros disponibles
        /// </summary>
        /// <param name="filter">Filtros de búsqueda</param>
        /// <returns>Resultados de búsqueda con filtros disponibles</returns>
        public async Task<ProductSearchResultDto> SearchProductsAsync(ProductFilterDto filter)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - SearchProductsAsync iniciado");
                Console.WriteLine($"   Búsqueda: '{filter.Busqueda ?? "Sin término de búsqueda"}'");
                Console.WriteLine($"   Filtros activos:");
                Console.WriteLine($"     - Activo: {filter.Activo?.ToString() ?? "Todos"}");
                Console.WriteLine($"     - Categoría: {filter.CategoriaId?.ToString() ?? "Todas"}");
                Console.WriteLine($"     - Marca: {filter.MarcaId?.ToString() ?? "Todas"}");
                Console.WriteLine($"     - Precio Min: {filter.PrecioMin?.ToString("C") ?? "Sin mínimo"}");
                Console.WriteLine($"     - Precio Max: {filter.PrecioMax?.ToString("C") ?? "Sin máximo"}");
                Console.WriteLine($"     - Página: {filter.Page}, Tamaño: {filter.PageSize}");

                // ✅ 1. OBTENER PRODUCTOS CON LOS FILTROS APLICADOS
                var productsResult = await GetProductsAsync(filter);

                Console.WriteLine($"📦 SERVICE - Productos encontrados: {productsResult.TotalItems}");

                // ✅ 2. OBTENER FILTROS DISPONIBLES BASADOS EN LA BÚSQUEDA ACTUAL
                var availableFilters = await GetAvailableFiltersAsync(filter);

                Console.WriteLine($"🔧 SERVICE - Filtros disponibles calculados:");
                Console.WriteLine($"   - Categorías: {availableFilters.Categorias.Count}");
                Console.WriteLine($"   - Marcas: {availableFilters.Marcas.Count}");
                Console.WriteLine($"   - Rango precios: ${availableFilters.RangoPrecios.Min} - ${availableFilters.RangoPrecios.Max}");

                // ✅ 3. CONVERTIR PRODUCTOS COMPLETOS A SUMMARY PARA LA BÚSQUEDA
                var productSummaries = productsResult.Items.Select(MapToProductSummaryDto).ToList();

                // ✅ 4. CONSTRUIR RESULTADO DE BÚSQUEDA
                var searchResult = new ProductSearchResultDto
                {
                    Productos = productSummaries,
                    FiltrosDisponibles = availableFilters,
                    TotalResultados = productsResult.TotalItems,
                    Pagina = productsResult.Page,
                    TotalPaginas = productsResult.TotalPages,
                    TerminoBusqueda = filter.Busqueda,
                    FiltrosAplicados = BuildAppliedFiltersInfo(filter)
                };

                // ✅ 5. ESTADÍSTICAS DE BÚSQUEDA
                Console.WriteLine($"📊 SERVICE - Estadísticas de búsqueda:");
                Console.WriteLine($"   - Término: '{searchResult.TerminoBusqueda ?? "Sin término"}'");
                Console.WriteLine($"   - Total resultados: {searchResult.TotalResultados}");
                Console.WriteLine($"   - Página {searchResult.Pagina} de {searchResult.TotalPaginas}");
                Console.WriteLine($"   - Productos en página: {searchResult.Productos.Count}");

                if (searchResult.Productos.Any())
                {
                    var conStock = searchResult.Productos.Count(p => p.Stock > 0);
                    var destacados = searchResult.Productos.Count(p => p.Destacado);
                    var enOferta = searchResult.Productos.Count(p => p.PrecioOferta.HasValue);

                    Console.WriteLine($"   - Con stock: {conStock}");
                    Console.WriteLine($"   - Destacados: {destacados}");
                    Console.WriteLine($"   - En oferta: {enOferta}");

                    // Mostrar primeros resultados
                    foreach (var product in searchResult.Productos.Take(3))
                    {
                        Console.WriteLine($"   - {product.Nombre}: ${product.Precio} ({product.CategoriaNombre})");
                    }

                    if (searchResult.Productos.Count > 3)
                    {
                        Console.WriteLine($"   ... y {searchResult.Productos.Count - 3} productos más");
                    }
                }

                Console.WriteLine($"✅ SERVICE - SearchProductsAsync completado exitosamente");
                return searchResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en SearchProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Construye información de filtros aplicados para el resultado de búsqueda
        /// </summary>
        /// <param name="filter">Filtros aplicados</param>
        /// <returns>Información de filtros aplicados</returns>
        private AppliedFiltersDto BuildAppliedFiltersInfo(ProductFilterDto filter)
        {
            var appliedFilters = new AppliedFiltersDto();

            if (filter.Activo.HasValue)
            {
                appliedFilters.Estado = filter.Activo.Value ? "Solo activos" : "Solo inactivos";
            }

            if (filter.CategoriaId.HasValue)
            {
                // Buscar nombre de categoría
                try
                {
                    var categoria = _context.Categorias.FirstOrDefault(c => c.CatId == filter.CategoriaId);
                    appliedFilters.Categoria = categoria?.CatNombre ?? $"Categoría ID {filter.CategoriaId}";
                }
                catch
                {
                    appliedFilters.Categoria = $"Categoría ID {filter.CategoriaId}";
                }
            }

            if (filter.MarcaId.HasValue)
            {
                // Buscar nombre de marca
                try
                {
                    var marca = _context.Marcas.FirstOrDefault(m => m.MarId == filter.MarcaId);
                    appliedFilters.Marca = marca?.MarNombre ?? $"Marca ID {filter.MarcaId}";
                }
                catch
                {
                    appliedFilters.Marca = $"Marca ID {filter.MarcaId}";
                }
            }

            if (filter.PrecioMin.HasValue || filter.PrecioMax.HasValue)
            {
                var min = filter.PrecioMin?.ToString("C") ?? "Sin mínimo";
                var max = filter.PrecioMax?.ToString("C") ?? "Sin máximo";
                appliedFilters.RangoPrecios = $"{min} - {max}";
            }

            if (!string.IsNullOrEmpty(filter.Busqueda))
            {
                appliedFilters.TerminoBusqueda = filter.Busqueda;
            }

            appliedFilters.Ordenamiento = filter.SortBy ?? "Nombre";
            appliedFilters.OrdenDescendente = filter.SortDescending;

            return appliedFilters;
        }


        /// <summary>
        /// Establece una imagen como principal para un producto
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="imageId">ID de la imagen a establecer como principal</param>
        /// <returns>True si se estableció exitosamente</returns>
        public async Task<bool> SetMainImageAsync(int productId, int imageId)
        {
            try
            {
                Console.WriteLine($"📌 SERVICE - SetMainImageAsync: Estableciendo imagen {imageId} como principal del producto {productId}");

                // ✅ 1. VERIFICAR QUE LA IMAGEN EXISTE Y PERTENECE AL PRODUCTO
                var image = await _context.ProductosImagenes
                    .FirstOrDefaultAsync(img => img.PimId == imageId && img.PimProductoId == productId);

                if (image == null)
                {
                    Console.WriteLine($"❌ SERVICE - Imagen {imageId} no encontrada en producto {productId}");
                    return false;
                }

                Console.WriteLine($"✅ SERVICE - Imagen encontrada: {image.PimUrl}");

                // ✅ 2. VERIFICAR SI YA ES LA IMAGEN PRINCIPAL
                if (image.PimEsPrincipal == true)
                {
                    Console.WriteLine($"ℹ️ SERVICE - La imagen {imageId} ya es la principal del producto {productId}");
                    return true; // Ya es principal, operación exitosa
                }

                // ✅ 3. QUITAR EL FLAG PRINCIPAL DE TODAS LAS IMÁGENES DEL PRODUCTO
                var existingMainImages = await _context.ProductosImagenes
                    .Where(img => img.PimProductoId == productId && img.PimEsPrincipal == true)
                    .ToListAsync();

                Console.WriteLine($"🔄 SERVICE - Removiendo flag principal de {existingMainImages.Count} imágenes existentes");

                foreach (var img in existingMainImages)
                {
                    img.PimEsPrincipal = false;
                    Console.WriteLine($"   - Imagen {img.PimId}: Principal = false");
                }

                // ✅ 4. ESTABLECER LA NUEVA IMAGEN COMO PRINCIPAL
                image.PimEsPrincipal = true;
                Console.WriteLine($"📌 SERVICE - Imagen {imageId} marcada como principal");

                // ✅ 5. ACTUALIZAR FECHA DE MODIFICACIÓN DEL PRODUCTO
                var product = await _context.Productos.FindAsync(productId);
                if (product != null)
                {
                    product.PrdFechaModificacion = DateTime.UtcNow;
                    Console.WriteLine($"📅 SERVICE - Fecha de modificación del producto actualizada");
                }

                // ✅ 6. GUARDAR CAMBIOS
                var changesCount = await _context.SaveChangesAsync();

                if (changesCount > 0)
                {
                    Console.WriteLine($"✅ SERVICE - SetMainImageAsync completado exitosamente");
                    Console.WriteLine($"   - Producto: {productId}");
                    Console.WriteLine($"   - Nueva imagen principal: {imageId}");
                    Console.WriteLine($"   - URL: {image.PimUrl}");
                    Console.WriteLine($"   - Cambios guardados: {changesCount}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"⚠️ SERVICE - No se guardaron cambios en SetMainImageAsync");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en SetMainImageAsync: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un producto con el SKU especificado
        /// </summary>
        /// <param name="sku">SKU del producto</param>
        /// <param name="excludeId">ID a excluir de la búsqueda (para actualizaciones)</param>
        /// <returns>True si existe un producto con ese SKU</returns>
        public async Task<bool> SKUExistsAsync(string sku, int? excludeId = null)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - SKUExistsAsync: Verificando existencia del SKU '{sku}'");

                // ✅ 1. VALIDAR SKU
                if (string.IsNullOrWhiteSpace(sku))
                {
                    Console.WriteLine($"⚠️ SERVICE - SKU vacío o nulo");
                    return false;
                }

                // ✅ 2. CONSTRUIR QUERY BASE
                var query = _context.Productos.Where(p => p.PrdSku == sku);

                // ✅ 3. EXCLUIR ID SI SE PROPORCIONA (PARA ACTUALIZACIONES)
                if (excludeId.HasValue)
                {
                    query = query.Where(p => p.PrdId != excludeId.Value);
                    Console.WriteLine($"   - Excluyendo producto ID: {excludeId.Value}");
                }

                // ✅ 4. VERIFICAR EXISTENCIA
                var exists = await query.AnyAsync();

                Console.WriteLine($"🔍 SERVICE - SKUExistsAsync resultado:");
                Console.WriteLine($"   - SKU: '{sku}'");
                Console.WriteLine($"   - Excluir ID: {excludeId?.ToString() ?? "Ninguno"}");
                Console.WriteLine($"   - Existe: {(exists ? "✅ SÍ" : "❌ NO")}");

                // ✅ 5. INFORMACIÓN ADICIONAL SI EXISTE
                if (exists && !excludeId.HasValue)
                {
                    try
                    {
                        var existingProduct = await _context.Productos
                            .Where(p => p.PrdSku == sku)
                            .Select(p => new { p.PrdId, p.PrdNombre, p.PrdActivo })
                            .FirstOrDefaultAsync();

                        if (existingProduct != null)
                        {
                            Console.WriteLine($"   - Producto existente: {existingProduct.PrdNombre} (ID: {existingProduct.PrdId})");
                            Console.WriteLine($"   - Estado: {(existingProduct.PrdActivo == true ? "Activo" : "Inactivo")}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ⚠️ Error obteniendo detalles: {ex.Message}");
                    }
                }

                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en SKUExistsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un producto con el slug especificado
        /// </summary>
        /// <param name="slug">Slug del producto</param>
        /// <param name="excludeId">ID a excluir de la búsqueda (para actualizaciones)</param>
        /// <returns>True si existe un producto con ese slug</returns>
        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
        {
            try
            {
                Console.WriteLine($"🔍 SERVICE - SlugExistsAsync: Verificando existencia del slug '{slug}'");

                // ✅ 1. VALIDAR SLUG
                if (string.IsNullOrWhiteSpace(slug))
                {
                    Console.WriteLine($"⚠️ SERVICE - Slug vacío o nulo");
                    return false;
                }

                // ✅ 2. CONSTRUIR QUERY BASE
                var query = _context.Productos.Where(p => p.PrdSlug == slug);

                // ✅ 3. EXCLUIR ID SI SE PROPORCIONA (PARA ACTUALIZACIONES)
                if (excludeId.HasValue)
                {
                    query = query.Where(p => p.PrdId != excludeId.Value);
                    Console.WriteLine($"   - Excluyendo producto ID: {excludeId.Value}");
                }

                // ✅ 4. VERIFICAR EXISTENCIA
                var exists = await query.AnyAsync();

                Console.WriteLine($"🔍 SERVICE - SlugExistsAsync resultado:");
                Console.WriteLine($"   - Slug: '{slug}'");
                Console.WriteLine($"   - Excluir ID: {excludeId?.ToString() ?? "Ninguno"}");
                Console.WriteLine($"   - Existe: {(exists ? "✅ SÍ" : "❌ NO")}");

                // ✅ 5. INFORMACIÓN ADICIONAL SI EXISTE
                if (exists && !excludeId.HasValue)
                {
                    try
                    {
                        var existingProduct = await _context.Productos
                            .Where(p => p.PrdSlug == slug)
                            .Select(p => new { p.PrdId, p.PrdNombre, p.PrdActivo, p.PrdSku })
                            .FirstOrDefaultAsync();

                        if (existingProduct != null)
                        {
                            Console.WriteLine($"   - Producto existente: {existingProduct.PrdNombre} (ID: {existingProduct.PrdId})");
                            Console.WriteLine($"   - SKU: {existingProduct.PrdSku}");
                            Console.WriteLine($"   - Estado: {(existingProduct.PrdActivo == true ? "Activo" : "Inactivo")}");
                            Console.WriteLine($"   - URL actual: /productos/{slug}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ⚠️ Error obteniendo detalles: {ex.Message}");
                    }
                }

                // ✅ 6. VALIDACIÓN DE FORMATO DE SLUG (OPCIONAL)
                if (!exists && !string.IsNullOrEmpty(slug))
                {
                    var isValidSlugFormat = System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$");
                    if (!isValidSlugFormat)
                    {
                        Console.WriteLine($"   ⚠️ Formato de slug no recomendado: '{slug}'");
                        Console.WriteLine($"   💡 Recomendación: usar solo letras minúsculas, números y guiones");
                    }
                }

                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en SlugExistsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cambia el estado de un producto (activo/inactivo)
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>True si se cambió correctamente</returns>
        public async Task<bool> ToggleProductStatusAsync(int id)
        {
            try
            {
                Console.WriteLine($"🔄 SERVICE - ToggleProductStatusAsync: Cambiando estado del producto {id}");

                // ✅ 1. BUSCAR EL PRODUCTO
                var product = await _context.Productos.FindAsync(id);
                if (product == null)
                {
                    Console.WriteLine($"❌ SERVICE - Producto {id} no encontrado");
                    return false;
                }

                // ✅ 2. OBTENER ESTADO ACTUAL
                var currentStatus = product.PrdActivo ?? false;
                var newStatus = !currentStatus;

                Console.WriteLine($"📊 SERVICE - Estado del producto:");
                Console.WriteLine($"   - Producto: {product.PrdNombre}");
                Console.WriteLine($"   - SKU: {product.PrdSku}");
                Console.WriteLine($"   - Estado actual: {(currentStatus ? "✅ ACTIVO" : "❌ INACTIVO")}");
                Console.WriteLine($"   - Nuevo estado: {(newStatus ? "✅ ACTIVO" : "❌ INACTIVO")}");

                // ✅ 3. CAMBIAR ESTADO
                product.PrdActivo = newStatus;
                product.PrdFechaModificacion = DateTime.UtcNow;

                // ✅ 4. LOGGING ADICIONAL SEGÚN EL CAMBIO
                if (newStatus)
                {
                    Console.WriteLine($"🟢 SERVICE - Activando producto: {product.PrdNombre}");

                    // Verificaciones adicionales al activar
                    var hasImages = await ProductHasImagesAsync(id);
                    var hasStock = await _context.Inventarios
                        .AnyAsync(i => i.InvProductoId == id && i.InvStock > 0);

                    Console.WriteLine($"   - Tiene imágenes: {(hasImages ? "✅" : "⚠️ NO")}");
                    Console.WriteLine($"   - Tiene stock: {(hasStock ? "✅" : "⚠️ NO")}");

                    if (!hasImages)
                    {
                        Console.WriteLine($"   ⚠️ ADVERTENCIA: Producto sin imágenes se está activando");
                    }
                }
                else
                {
                    Console.WriteLine($"🔴 SERVICE - Desactivando producto: {product.PrdNombre}");
                    Console.WriteLine($"   ℹ️ El producto no será visible en el catálogo público");
                }

                // ✅ 5. GUARDAR CAMBIOS
                var changesCount = await _context.SaveChangesAsync();

                if (changesCount > 0)
                {
                    Console.WriteLine($"✅ SERVICE - ToggleProductStatusAsync completado exitosamente");
                    Console.WriteLine($"   - Producto ID: {id}");
                    Console.WriteLine($"   - Estado final: {(newStatus ? "ACTIVO" : "INACTIVO")}");
                    Console.WriteLine($"   - Fecha modificación: {product.PrdFechaModificacion:yyyy-MM-dd HH:mm:ss}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"⚠️ SERVICE - No se guardaron cambios en ToggleProductStatusAsync");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SERVICE - Error en ToggleProductStatusAsync: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Actualiza el orden de las imágenes de un producto
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="imageOrders">Lista con el nuevo orden de las imágenes</param>
        /// <returns>True si se actualizó correctamente</returns>
        public async Task<bool> UpdateImageOrderAsync(int productId, List<UpdateImageOrderDto> imageOrders)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ✅ 1. VERIFICAR QUE EL PRODUCTO EXISTE
                var product = await _context.Productos.FindAsync(productId);
                if (product == null)
                {
                    return false;
                }

                // ✅ 2. VALIDAR QUE LA LISTA NO ESTÉ VACÍA
                if (imageOrders == null || !imageOrders.Any())
                {
                    return false;
                }

                // ✅ 3. OBTENER TODAS LAS IMÁGENES DEL PRODUCTO
                var productImages = await _context.ProductosImagenes
                    .Where(img => img.PimProductoId == productId)
                    .ToListAsync();

                if (!productImages.Any())
                {
                    return false;
                }

                // ✅ 4. VALIDAR QUE TODOS LOS IDS DE IMÁGENES EXISTEN
                var providedImageIds = imageOrders.Select(io => io.ImageId).ToList();
                var existingImageIds = productImages.Select(img => img.PimId).ToList();

                var invalidIds = providedImageIds.Except(existingImageIds).ToList();
                if (invalidIds.Any())
                {
                    throw new ArgumentException($"Las siguientes imágenes no pertenecen al producto {productId}: {string.Join(", ", invalidIds)}");
                }

                // ✅ 5. ACTUALIZAR EL ORDEN DE LAS IMÁGENES
                var updatedCount = 0;
                foreach (var orderDto in imageOrders)
                {
                    var image = productImages.FirstOrDefault(img => img.PimId == orderDto.ImageId);
                    if (image != null)
                    {
                        image.PimOrden = orderDto.NewOrder;
                        updatedCount++;
                    }
                }

                // ✅ 6. ACTUALIZAR FECHA DE MODIFICACIÓN DEL PRODUCTO
                product.PrdFechaModificacion = DateTime.UtcNow;

                // ✅ 7. GUARDAR CAMBIOS
                var saveResult = await _context.SaveChangesAsync();

                if (saveResult > 0)
                {
                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        /// <summary>
        /// Actualiza un producto existente con nueva información
        /// </summary>
        /// <param name="id">ID del producto a actualizar</param>
        /// <param name="dto">Datos actualizados del producto</param>
        /// <returns>Producto actualizado</returns>
        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ✅ 1. BUSCAR EL PRODUCTO EXISTENTE
                var product = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .FirstOrDefaultAsync(p => p.PrdId == id);

                if (product == null)
                {
                    throw new ArgumentException($"Producto con ID {id} no encontrado");
                }

                // ✅ 2. VALIDAR SKU ÚNICO (SI CAMBIÓ)
                if (dto.SKU != product.PrdSku)
                {
                    if (await SKUExistsAsync(dto.SKU, id))
                    {
                        throw new ArgumentException($"Ya existe un producto con el SKU '{dto.SKU}'");
                    }
                }

                // ✅ 3. GENERAR NUEVO SLUG SI CAMBIÓ EL NOMBRE O SE PROPORCIONÓ UNO NUEVO
                string newSlug = product.PrdSlug ?? string.Empty;

                if (!string.IsNullOrEmpty(dto.Slug) && dto.Slug != product.PrdSlug)
                {
                    // Se proporcionó un slug específico
                    newSlug = await _slugService.GenerateSlugAsync(dto.Slug, "productos", id);
                }
                else if (dto.Nombre != product.PrdNombre)
                {
                    // El nombre cambió, generar nuevo slug basado en el nombre
                    newSlug = await _slugService.GenerateSlugAsync(dto.Nombre, "productos", id);
                }

                // ✅ 4. ACTUALIZAR CAMPOS DEL PRODUCTO
                // Campos requeridos - siempre se actualizan
                product.PrdNombre = dto.Nombre;
                product.PrdSku = dto.SKU;
                product.PrdPrecio = dto.Precio;
                product.PrdCategoriaId = dto.CategoriaId;
                product.PrdMarcaId = dto.MarcaId;
                product.PrdSlug = newSlug;

                // Campos opcionales - solo si no son null o empty
                if (!string.IsNullOrWhiteSpace(dto.DescripcionCorta))
                    product.PrdDescripcionCorta = dto.DescripcionCorta;

                if (!string.IsNullOrWhiteSpace(dto.DescripcionLarga))
                    product.PrdDescripcionLarga = dto.DescripcionLarga;

                // Precios opcionales
                product.PrdPrecioComparacion = dto.PrecioComparacion;
                product.PrdCosto = dto.Costo;

                // Campos con valores por defecto
                product.PrdTipo = dto.Tipo ?? product.PrdTipo ?? "simple";
                product.PrdEstado = dto.Estado ?? product.PrdEstado ?? "borrador";

                // Campos booleanos - se actualizan siempre
                product.PrdDestacado = dto.Destacado;
                product.PrdNuevo = dto.Nuevo;
                product.PrdEnOferta = dto.EnOferta;
                product.PrdRequiereEnvio = dto.RequiereEnvio;
                product.PrdPermiteReseñas = dto.PermiteReseñas;
                product.PrdActivo = dto.Activo;
                product.PrdOrden = dto.Orden;

                // Campos opcionales adicionales
                product.PrdPeso = dto.Peso;
                product.PrdDimensiones = dto.Dimensiones;
                product.PrdMetaTitulo = dto.MetaTitulo;
                product.PrdMetaDescripcion = dto.MetaDescripcion;
                product.PrdPalabrasClaves = dto.PalabrasClaves;
                product.PrdGarantia = dto.Garantia;

                // Siempre actualizar fecha de modificación
                product.PrdFechaModificacion = DateTime.UtcNow;

                // ✅ 5. GUARDAR CAMBIOS DEL PRODUCTO
                await _context.SaveChangesAsync();

                // ✅ 6. ACTUALIZAR IMÁGENES SI SE PROPORCIONARON
                if (dto.Imagenes?.Any() == true)
                {
                    foreach (var imageUpdate in dto.Imagenes)
                    {
                        try
                        {
                            // Si está marcada para eliminar
                            if (imageUpdate.Eliminar && imageUpdate.Id.HasValue)
                            {
                                await DeleteProductImageAsync(id, imageUpdate.Id.Value);
                            }
                            // Si tiene ID, es una imagen existente para actualizar
                            else if (imageUpdate.Id.HasValue && !imageUpdate.Eliminar)
                            {
                                // Establecer como principal si se especifica
                                if (imageUpdate.EsPrincipal)
                                {
                                    await SetMainImageAsync(id, imageUpdate.Id.Value);
                                }

                                // Actualizar orden usando el método de reordenamiento
                                var orderList = new List<UpdateImageOrderDto>
                        {
                            new UpdateImageOrderDto
                            {
                                ImageId = imageUpdate.Id.Value,
                                NewOrder = imageUpdate.Orden
                            }
                        };
                                await UpdateImageOrderAsync(id, orderList);
                            }
                            // Si no tiene ID, es una nueva imagen
                            else if (!imageUpdate.Id.HasValue && !string.IsNullOrEmpty(imageUpdate.Url))
                            {
                                var newImageDto = new CreateProductImageDto
                                {
                                    Url = imageUpdate.Url,
                                    AltText = imageUpdate.AltText,
                                    EsPrincipal = imageUpdate.EsPrincipal,
                                    Orden = imageUpdate.Orden
                                };
                                await AddProductImageAsync(id, newImageDto);
                            }
                        }
                        catch
                        {
                            // Continuar con las demás imágenes en caso de error
                        }
                    }
                }

                await transaction.CommitAsync();

                // ✅ 7. CARGAR EL PRODUCTO ACTUALIZADO COMPLETO
                var updatedProduct = await GetProductByIdAsync(id);
                if (updatedProduct == null)
                {
                    throw new Exception("Error al recuperar el producto actualizado");
                }

                return updatedProduct;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        /// <summary>
        /// Actualiza el stock de un producto específico
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="newStock">Nuevo valor de stock</param>
        /// <returns>True si se actualizó correctamente</returns>
        public async Task<bool> UpdateStockAsync(int productId, int newStock)
        {
            try
            {
                // ✅ 1. VALIDAR PARÁMETROS
                if (newStock < 0)
                {
                    throw new ArgumentException("El stock no puede ser negativo");
                }

                // ✅ 2. VERIFICAR QUE EL PRODUCTO EXISTE
                var product = await _context.Productos.FindAsync(productId);
                if (product == null)
                {
                    return false;
                }

                // ✅ 3. BUSCAR O CREAR INVENTARIO
                var inventario = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.InvProductoId == productId);

                if (inventario == null)
                {
                    // Crear inventario si no existe
                    inventario = new Inventario
                    {
                        InvProductoId = productId,
                        InvStock = newStock,
                        InvStockReservado = 0,
                        InvStockMinimo = 5, // Valor por defecto
                        InvStockMaximo = 100, // Valor por defecto
                        InvFechaUltimaActualizacion = DateTime.UtcNow
                    };
                    _context.Inventarios.Add(inventario);

                    // Registrar movimiento de entrada inicial
                    var movimientoInicial = new MovimientosInventario
                    {
                        MovInventarioId = 0, // Se actualizará después del SaveChanges
                        MovTipo = "entrada",
                        MovCantidad = newStock,
                        MovCantidadAnterior = 0,
                        MovMotivo = "Stock inicial del producto",
                        MovFecha = DateTime.UtcNow
                    };

                    // Guardar inventario primero para obtener el ID
                    await _context.SaveChangesAsync();

                    // Actualizar el movimiento con el ID correcto
                    movimientoInicial.MovInventarioId = inventario.InvId;
                    _context.MovimientosInventarios.Add(movimientoInicial);
                }
                else
                {
                    // Actualizar inventario existente
                    var stockAnterior = inventario.InvStock;
                    var diferencia = newStock - stockAnterior;

                    // Actualizar stock
                    inventario.InvStock = newStock;
                    inventario.InvFechaUltimaActualizacion = DateTime.UtcNow;

                    // Registrar movimiento de inventario solo si hay diferencia
                    if (diferencia != 0)
                    {
                        var movimiento = new MovimientosInventario
                        {
                            MovInventarioId = inventario.InvId,
                            MovTipo = diferencia > 0 ? "entrada" : "salida",
                            MovCantidad = Math.Abs(diferencia),
                            MovCantidadAnterior = stockAnterior,
                            MovMotivo = "Actualización manual de stock",
                            MovFecha = DateTime.UtcNow
                        };
                        _context.MovimientosInventarios.Add(movimiento);
                    }
                }

                // ✅ 4. ACTUALIZAR FECHA DE MODIFICACIÓN DEL PRODUCTO
                product.PrdFechaModificacion = DateTime.UtcNow;

                // ✅ 5. GUARDAR TODOS LOS CAMBIOS
                var changesCount = await _context.SaveChangesAsync();

                return changesCount > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}