using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TechGadgets.API.Attributes;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Dtos.Products;
using TechGadgets.API.Models.Common;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    [SwaggerTag("Gestión de productos")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;
        private readonly ILogService _logService;
        private readonly TechGadgetsDbContext _context;
        private readonly ISupabaseStorageService _storageService;

        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger,
            ILogService logService,
            TechGadgetsDbContext context,
            ISupabaseStorageService storageService)
        {
            _productService = productService;
            _logger = logger;
            _logService = logService;
            _context = context;
            _storageService = storageService;
        }

        #region CRUD Básico

        [HttpGet]
        [RequirePermission("productos.ver")]
        [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductFilterDto filter)
        {
            try
            {
                // ✅ LOG de entrada
                Console.WriteLine($"🔍 GetProducts - Filtros recibidos:");
                Console.WriteLine($"   Activo: {filter.Activo}");
                Console.WriteLine($"   Busqueda: '{filter.Busqueda}'");
                Console.WriteLine($"   Page: {filter.Page}, PageSize: {filter.PageSize}");

                // ✅ Validar filtros básicos
                if (filter.Page < 1) filter.Page = 1;
                if (filter.PageSize < 1 || filter.PageSize > 100) filter.PageSize = 10;

                var result = await _productService.GetProductsAsync(filter);

                // ✅ LOG de resultado
                Console.WriteLine($"📦 GetProducts - Resultado:");
                Console.WriteLine($"   TotalItems: {result.TotalItems}");
                Console.WriteLine($"   Items.Count: {result.Items?.Count ?? 0}");
                Console.WriteLine($"   Page: {result.Page}, PageSize: {result.PageSize}");

                if (result.Items?.Any() == true)
                {
                    var firstProduct = result.Items.First();
                    Console.WriteLine($"   Primer producto: ID={firstProduct.Id}, Nombre='{firstProduct.Nombre}'");
                }

                // ✅ RESPUESTA CORRECTA - NO ANIDAR 'result'
                return Ok(new
                {
                    success = true,
                    data = result  // result ya es PagedResult<ProductDto>
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetProducts: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                await _logService.LogErrorAsync("Error al obtener productos", ex);
                _logger.LogError(ex, "Error al obtener productos");

                return BadRequest(new
                {
                    success = false,
                    message = "Error al obtener productos",
                    error = ex.Message
                });
            }
        }


        [HttpGet("debug/quick")]
        [AllowAnonymous]
        public async Task<IActionResult> QuickDebug()
        {
            try
            {
                // Test de conexión
                var canConnect = await _context.Database.CanConnectAsync();
                Console.WriteLine($"🔗 Conexión a BD: {canConnect}");

                if (!canConnect)
                {
                    return BadRequest(new { error = "No se puede conectar a la base de datos" });
                }

                // Contar productos totales
                var totalCount = await _context.Productos.CountAsync();
                Console.WriteLine($"📊 Total productos en BD: {totalCount}");

                // Obtener primeros 3 productos sin filtros
                var rawProducts = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Take(3)
                    .ToListAsync();

                Console.WriteLine($"📦 Productos raw cargados: {rawProducts.Count}");

                var debugInfo = new
                {
                    DatabaseConnection = canConnect,
                    TotalProductsInDB = totalCount,
                    SampleProductsLoaded = rawProducts.Count,
                    SampleProducts = rawProducts.Select(p => new
                    {
                        Id = p.PrdId,
                        Nombre = p.PrdNombre,
                        SKU = p.PrdSku,
                        Precio = p.PrdPrecio,
                        Activo = p.PrdActivo,
                        TieneCategoria = p.PrdCategoria != null,
                        TieneMarca = p.PrdMarca != null,
                        TieneInventarios = p.Inventarios != null && p.Inventarios.Any()
                    }).ToList(),
                    TestProductService = totalCount > 0 ? "Ready to test" : "No products to test"
                };

                return Ok(new { success = true, debug = debugInfo });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en QuickDebug: {ex.Message}");
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }


        // Agregar este método en la región #region CRUD Básico de tu ProductsController

        /// <summary>
        /// Método de debug para verificar carga de datos
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Datos raw del producto para debug</returns>
        [HttpGet("debug/{id:int}")]
        [AllowAnonymous] // Temporal para testing
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductDebug(int id)
        {
            try
            {
                // ✅ 1. Verificar conexión a BD
                var connectionTest = await _context.Database.CanConnectAsync();
                if (!connectionTest)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede conectar a la base de datos"
                    });
                }

                // ✅ 2. Buscar producto con todas las relaciones
                var producto = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Include(p => p.ProductosImagenes)
                    .FirstOrDefaultAsync(p => p.PrdId == id);

                if (producto == null)
                {
                    // ✅ 3. Si no existe, verificar si hay productos en general
                    var totalProducts = await _context.Productos.CountAsync();
                    return NotFound(new
                    {
                        success = false,
                        message = $"Producto {id} no encontrado",
                        totalProductsInDB = totalProducts,
                        suggestion = totalProducts > 0 ? "Intenta con otro ID" : "No hay productos en la base de datos"
                    });
                }

                // ✅ 4. Información detallada del producto encontrado
                return Ok(new
                {
                    success = true,
                    message = $"Producto {id} encontrado exitosamente",
                    timestamp = DateTime.UtcNow,

                    // Datos raw de la entidad
                    ProductoRaw = new
                    {
                        Id = producto.PrdId,
                        SKU = producto.PrdSku ?? "NULL",
                        Nombre = producto.PrdNombre ?? "NULL",
                        Precio = producto.PrdPrecio,
                        Activo = producto.PrdActivo,
                        CategoriaId = producto.PrdCategoriaId,
                        MarcaId = producto.PrdMarcaId,
                        FechaCreacion = producto.PrdFechaCreacion
                    },

                    // Estado de las relaciones
                    RelacionesEstado = new
                    {
                        TieneCategoria = producto.PrdCategoria != null,
                        TieneMarca = producto.PrdMarca != null,
                        TieneInventarios = producto.Inventarios?.Any() == true,
                        TieneImagenes = producto.ProductosImagenes?.Any() == true,
                        CantidadInventarios = producto.Inventarios?.Count ?? 0,
                        CantidadImagenes = producto.ProductosImagenes?.Count ?? 0
                    },

                    // Datos de relaciones
                    Categoria = producto.PrdCategoria != null ? new
                    {
                        Id = producto.PrdCategoria.CatId,
                        Nombre = producto.PrdCategoria.CatNombre ?? "NULL",
                        Slug = producto.PrdCategoria.CatSlug ?? "NULL",
                        Activo = producto.PrdCategoria.CatActivo
                    } : null,

                    Marca = producto.PrdMarca != null ? new
                    {
                        Id = producto.PrdMarca.MarId,
                        Nombre = producto.PrdMarca.MarNombre ?? "NULL",
                        Logo = producto.PrdMarca.MarLogo,
                        Activo = producto.PrdMarca.MarActivo
                    } : null,

                    // Inventario
                    PrimerInventario = producto.Inventarios?.FirstOrDefault() != null ? new
                    {
                        Id = producto.Inventarios.FirstOrDefault()!.InvId,
                        Stock = producto.Inventarios.FirstOrDefault()!.InvStock,
                        StockMinimo = producto.Inventarios.FirstOrDefault()!.InvStockMinimo,
                        StockReservado = producto.Inventarios.FirstOrDefault()!.InvStockReservado
                    } : null,

                    // Imágenes
                    PrimeraImagen = producto.ProductosImagenes?.FirstOrDefault() != null ? new
                    {
                        Id = producto.ProductosImagenes.FirstOrDefault()!.PimId,
                        Url = producto.ProductosImagenes.FirstOrDefault()!.PimUrl ?? "NULL",
                        EsPrincipal = producto.ProductosImagenes.FirstOrDefault()!.PimEsPrincipal,
                        Orden = producto.ProductosImagenes.FirstOrDefault()!.PimOrden
                    } : null,

                    // ✅ 5. Probar el mapeo del service
                    TestMapeo = await TestProductMapping(id)
                });
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"❌ Error en debug del producto {id}", ex);
                _logger.LogError(ex, "❌ Error en debug del producto {ProductId}", id);
                return BadRequest(new
                {
                    success = false,
                    message = "Error en debug",
                    error = ex.Message,
                    stackTrace = ex.StackTrace?.Split('\n').Take(10).ToArray(),
                    innerException = ex.InnerException?.Message
                });
            }
        }


        // ✅ Método auxiliar para probar el mapeo
        private async Task<object> TestProductMapping(int id)
        {
            try
            {
                var productDto = await _productService.GetProductByIdAsync(id);
                if (productDto == null)
                {
                    return new { Error = "ProductService.GetProductByIdAsync retornó null" };
                }

                return new
                {
                    Success = true,
                    MappedData = new
                    {
                        Id = productDto.Id,
                        SKU = productDto.SKU ?? "NULL",
                        Nombre = productDto.Nombre ?? "NULL",
                        Precio = productDto.Precio,
                        Activo = productDto.Activo,
                        CategoriaNombre = productDto.CategoriaNombre ?? "NULL",
                        MarcaNombre = productDto.MarcaNombre ?? "NULL",
                        StockActual = productDto.StockActual,
                        TieneImagenes = productDto.Imagenes?.Any() == true,
                        CantidadImagenes = productDto.Imagenes?.Count ?? 0
                    }
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Error = "Error en mapeo del ProductService",
                    Message = ex.Message,
                    StackTrace = ex.StackTrace?.Split('\n').Take(5)
                };
            }
        }

        /// <summary>
        /// Obtiene un producto por su ID
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Producto encontrado</returns>
        [HttpGet("{id:int}")]
        [RequirePermission("productos.ver")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            try
            {
                // ✅ DEBUG LOG
                Console.WriteLine($"🔍 GetProductById - Buscando producto ID: {id}");
                await _logService.LogInformationAsync($"Obteniendo producto con ID: {id}");

                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    Console.WriteLine($"❌ Producto {id} no encontrado");
                    await _logService.LogWarningAsync($"Producto no encontrado con ID: {id}");
                    return NotFound(new { success = false, message = "Producto no encontrado" });
                }

                // ✅ DEBUG LOG DEL PRODUCTO ENCONTRADO
                Console.WriteLine($"✅ Producto {id} encontrado: {product.Nombre}");
                Console.WriteLine($"   SKU: {product.SKU}, Precio: {product.Precio}");

                return Ok(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetProductById {id}: {ex.Message}");
                await _logService.LogErrorAsync($"Error al obtener producto con ID {id}", ex);
                _logger.LogError(ex, "Error al obtener producto con ID {ProductId}", id);
                return BadRequest(new { success = false, message = "Error al obtener producto", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un producto por su slug
        /// </summary>
        /// <param name="slug">Slug del producto</param>
        /// <returns>Producto encontrado</returns>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous] // Permitir acceso público para catálogo
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProductBySlug(string slug)
        {
            try
            {
                var product = await _productService.GetProductBySlugAsync(slug);
                if (product == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                return Ok(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto con slug {Slug}", slug);
                return BadRequest(new { success = false, message = "Error al obtener producto", error = ex.Message });
            }
        }



        /// <summary>
        /// Crea un producto con imágenes (sube las imágenes a Supabase automáticamente)
        /// </summary>
        [HttpPost("create-with-images")]
        [RequirePermission("productos.crear")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProductWithImages([FromForm] CreateProductWithImagesRequest request)
        {
            try
            {
                _logger.LogInformation("Creando producto con imágenes: {ProductName}", request.Nombre);

                // ✅ 1. VALIDAR DATOS DEL PRODUCTO
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Datos del producto inválidos",
                        Errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                    });
                }

                // ✅ 2. SUBIR IMÁGENES A SUPABASE (SI HAY ARCHIVOS)
                var imageUrls = new List<CreateProductImageDto>();
                
                if (request.ImageFiles != null && request.ImageFiles.Count > 0)
                {
                    _logger.LogInformation("Subiendo {Count} imágenes a Supabase", request.ImageFiles.Count);
                    
                    var uploadResult = await _storageService.UploadMultipleImagesAsync(
                        request.ImageFiles, 
                        $"product-{Guid.NewGuid():N}"
                    );

                    if (uploadResult.Success && uploadResult.SuccessfulUploads.Any())
                    {
                        for (int i = 0; i < uploadResult.SuccessfulUploads.Count; i++)
                        {
                            var uploadedImage = uploadResult.SuccessfulUploads[i];
                            imageUrls.Add(new CreateProductImageDto
                            {
                                Url = uploadedImage.Url,
                                AltText = uploadedImage.AltText ?? $"{request.Nombre} - Imagen {i + 1}",
                                EsPrincipal = i == 0, // Primera imagen es principal
                                Orden = i + 1
                            });
                        }
                        
                        _logger.LogInformation("Imágenes subidas exitosamente: {Count}", uploadResult.SuccessfulUploads.Count);
                    }
                    else
                    {
                        _logger.LogWarning("Error subiendo imágenes: {Errors}", 
                            string.Join(", ", uploadResult.Errors));
                    }
                }

                // ✅ 3. AGREGAR URLs EXTERNAS (SI LAS HAY)
                if (request.ExternalImageUrls != null && request.ExternalImageUrls.Any())
                {
                    foreach (var url in request.ExternalImageUrls)
                    {
                        if (!string.IsNullOrWhiteSpace(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                        {
                            imageUrls.Add(new CreateProductImageDto
                            {
                                Url = url,
                                AltText = $"{request.Nombre} - Imagen externa",
                                EsPrincipal = imageUrls.Count == 0, // Primera imagen es principal
                                Orden = imageUrls.Count + 1
                            });
                        }
                    }
                }

                // ✅ 4. CREAR PRODUCTO CON IMÁGENES
                var createProductDto = new CreateProductDto
                {
                    SKU = request.SKU,
                    Nombre = request.Nombre,
                    DescripcionCorta = request.DescripcionCorta,
                    DescripcionLarga = request.DescripcionLarga,
                    Precio = request.Precio,
                    PrecioComparacion = request.PrecioComparacion,
                    Costo = request.Costo,
                    CategoriaId = request.CategoriaId,
                    MarcaId = request.MarcaId,
                    Tipo = request.Tipo,
                    Estado = request.Estado,
                    Destacado = request.Destacado,
                    Nuevo = request.Nuevo,
                    EnOferta = request.EnOferta,
                    Peso = request.Peso,
                    Dimensiones = request.Dimensiones,
                    MetaTitulo = request.MetaTitulo,
                    MetaDescripcion = request.MetaDescripcion,
                    PalabrasClaves = request.PalabrasClaves,
                    RequiereEnvio = request.RequiereEnvio,
                    PermiteReseñas = request.PermiteReseñas,
                    Garantia = request.Garantia,
                    Orden = request.Orden,
                    StockInicial = request.StockInicial,
                    Imagenes = imageUrls // ✅ INCLUIR TODAS LAS IMÁGENES
                };

                var result = await _productService.CreateProductAsync(createProductDto);
                
                _logger.LogInformation("Producto creado exitosamente: {ProductId} con {ImageCount} imágenes", 
                    result.Id, imageUrls.Count);

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Message = "Producto creado exitosamente con imágenes",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando producto con imágenes: {ProductName}", request.Nombre);
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Error al crear el producto con imágenes"
                });
            }
        }

        /// <summary>
        /// Actualiza un producto y permite agregar nuevas imágenes
        /// </summary>
        [HttpPut("{id}/update-with-images")]
        [RequirePermission("productos.editar")]
        public async Task<IActionResult> UpdateProductWithImages(int id, [FromForm] UpdateProductWithImagesRequest request)
        {
            try
            {
                _logger.LogInformation("Actualizando producto {ProductId} con imágenes", id);

                // ✅ 1. OBTENER PRODUCTO EXISTENTE
                var existingProduct = await _productService.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                // ✅ 2. SUBIR NUEVAS IMÁGENES SI LAS HAY
                var newImageUrls = new List<CreateProductImageDto>();
                
                if (request.NewImageFiles != null && request.NewImageFiles.Count > 0)
                {
                    var uploadResult = await _storageService.UploadMultipleImagesAsync(
                        request.NewImageFiles, 
                        $"product-{id}"
                    );

                    if (uploadResult.Success && uploadResult.SuccessfulUploads.Any())
                    {
                        for (int i = 0; i < uploadResult.SuccessfulUploads.Count; i++)
                        {
                            var uploadedImage = uploadResult.SuccessfulUploads[i];
                            newImageUrls.Add(new CreateProductImageDto
                            {
                                Url = uploadedImage.Url,
                                AltText = uploadedImage.AltText ?? $"{request.Nombre} - Nueva imagen {i + 1}",
                                EsPrincipal = false, // Las nuevas imágenes no son principales por defecto
                                Orden = existingProduct.Imagenes.Count + i + 1
                            });
                        }
                    }
                }

                // ✅ 3. COMBINAR IMÁGENES EXISTENTES CON NUEVAS
                var allImages = existingProduct.Imagenes.Select(img => new UpdateProductImageDto
                {
                    Id = img.Id,
                    Url = img.Url,
                    AltText = img.AltText,
                    EsPrincipal = img.EsPrincipal,
                    Orden = img.Orden,
                    Eliminar = false
                }).ToList();

                // Agregar nuevas imágenes
                allImages.AddRange(newImageUrls.Select(img => new UpdateProductImageDto
                {
                    Url = img.Url,
                    AltText = img.AltText,
                    EsPrincipal = img.EsPrincipal,
                    Orden = img.Orden,
                    Eliminar = false
                }));

                // ✅ 4. ACTUALIZAR PRODUCTO
                var updateProductDto = new UpdateProductDto
                {
                    SKU = request.SKU,
                    Nombre = request.Nombre,
                    DescripcionCorta = request.DescripcionCorta,
                    DescripcionLarga = request.DescripcionLarga,
                    Precio = request.Precio,
                    PrecioComparacion = request.PrecioComparacion,
                    Costo = request.Costo,
                    CategoriaId = request.CategoriaId,
                    MarcaId = request.MarcaId,
                    Tipo = request.Tipo,
                    Estado = request.Estado,
                    Destacado = request.Destacado,
                    Nuevo = request.Nuevo,
                    EnOferta = request.EnOferta,
                    Peso = request.Peso,
                    Dimensiones = request.Dimensiones,
                    MetaTitulo = request.MetaTitulo,
                    MetaDescripcion = request.MetaDescripcion,
                    PalabrasClaves = request.PalabrasClaves,
                    RequiereEnvio = request.RequiereEnvio,
                    PermiteReseñas = request.PermiteReseñas,
                    Garantia = request.Garantia,
                    Orden = request.Orden,
                    Activo = request.Activo,
                    Imagenes = allImages
                };

                var result = await _productService.UpdateProductAsync(id, updateProductDto);
                
                _logger.LogInformation("Producto {ProductId} actualizado exitosamente con {NewImageCount} nuevas imágenes", 
                    id, newImageUrls.Count);

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Message = "Producto actualizado exitosamente",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando producto {ProductId} con imágenes", id);
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Error al actualizar el producto"
                });
            }
        }

        /// <summary>
        /// Elimina una imagen específica de un producto
        /// </summary>
        [HttpDelete("{productId}/images/{imageId}")]
        [RequirePermission("productos.editar")]
        public async Task<IActionResult> DeleteProductImage(int productId, int imageId)
        {
            try
            {
                _logger.LogInformation("Eliminando imagen {ImageId} del producto {ProductId}", imageId, productId);

                // Obtener información de la imagen antes de eliminarla
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new { success = false, message = "Producto no encontrado" });
                }

                var imageToDelete = product.Imagenes.FirstOrDefault(img => img.Id == imageId);
                if (imageToDelete == null)
                {
                    return NotFound(new { success = false, message = "Imagen no encontrada" });
                }

                // Eliminar de Supabase si es una imagen subida (no URL externa)
                if (imageToDelete.Url.Contains("supabase") && !imageToDelete.Url.StartsWith("http://") && !imageToDelete.Url.StartsWith("https://external"))
                {
                    // Extraer path de la URL de Supabase
                    var uri = new Uri(imageToDelete.Url);
                    var path = uri.AbsolutePath.TrimStart('/');
                    
                    await _storageService.DeleteImageAsync(path);
                    _logger.LogInformation("Imagen eliminada de Supabase: {Path}", path);
                }

                // Eliminar de la base de datos
                var success = await _productService.DeleteProductImageAsync(productId, imageId);
                
                if (success)
                {
                    return Ok(new { success = true, message = "Imagen eliminada exitosamente" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Error al eliminar la imagen" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando imagen {ImageId} del producto {ProductId}", imageId, productId);
                return BadRequest(new { success = false, message = "Error interno del servidor" });
            }
        }

        
        #endregion

        #region Búsqueda y Filtros (Público para catálogo)

        /// <summary>
        /// Busca productos con filtros avanzados
        /// </summary>
        /// <param name="filter">Filtros de búsqueda</param>
        /// <returns>Resultados de búsqueda con filtros disponibles</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductSearchResultDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProductSearchResultDto>> SearchProducts([FromQuery] ProductFilterDto filter)
        {
            try
            {
                var result = await _productService.SearchProductsAsync(filter);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de productos");
                return BadRequest(new { success = false, message = "Error en búsqueda", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos destacados
        /// </summary>
        /// <param name="count">Cantidad de productos a retornar</param>
        /// <returns>Lista de productos destacados</returns>
        [HttpGet("featured")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetFeaturedProducts([FromQuery] int count = 8)
        {
            try
            {
                var products = await _productService.GetFeaturedProductsAsync(count);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos destacados");
                return BadRequest(new { success = false, message = "Error al obtener productos destacados", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos en oferta
        /// </summary>
        /// <param name="count">Cantidad de productos a retornar</param>
        /// <returns>Lista de productos en oferta</returns>
        [HttpGet("on-sale")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetProductsOnSale([FromQuery] int count = 12)
        {
            try
            {
                var products = await _productService.GetProductsOnSaleAsync(count);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos en oferta");
                return BadRequest(new { success = false, message = "Error al obtener productos en oferta", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos relacionados
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="count">Cantidad de productos relacionados</param>
        /// <returns>Lista de productos relacionados</returns>
        [HttpGet("{id:int}/related")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetRelatedProducts(int id, [FromQuery] int count = 6)
        {
            try
            {
                var products = await _productService.GetRelatedProductsAsync(id, count);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos relacionados para ID {ProductId}", id);
                return BadRequest(new { success = false, message = "Error al obtener productos relacionados", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos por categoría
        /// </summary>
        /// <param name="categoryId">ID de la categoría</param>
        /// <param name="count">Cantidad de productos</param>
        /// <returns>Lista de productos de la categoría</returns>
        [HttpGet("category/{categoryId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetProductsByCategory(int categoryId, [FromQuery] int count = 12)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId, count);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría {CategoryId}", categoryId);
                return BadRequest(new { success = false, message = "Error al obtener productos por categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos por marca
        /// </summary>
        /// <param name="brandId">ID de la marca</param>
        /// <param name="count">Cantidad de productos</param>
        /// <returns>Lista de productos de la marca</returns>
        [HttpGet("brand/{brandId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetProductsByBrand(int brandId, [FromQuery] int count = 12)
        {
            try
            {
                var products = await _productService.GetProductsByBrandAsync(brandId, count);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por marca {BrandId}", brandId);
                return BadRequest(new { success = false, message = "Error al obtener productos por marca", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos más nuevos
        /// </summary>
        /// <param name="count">Cantidad de productos</param>
        /// <returns>Lista de productos más recientes</returns>
        [HttpGet("newest")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetNewestProducts([FromQuery] int count = 8)
        {
            try
            {
                var products = await _productService.GetNewestProductsAsync(count);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más nuevos");
                return BadRequest(new { success = false, message = "Error al obtener productos más nuevos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene filtros disponibles para búsqueda
        /// </summary>
        /// <param name="filter">Filtros actuales (opcional)</param>
        /// <returns>Filtros disponibles</returns>
        [HttpGet("filters")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductSearchFiltersDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProductSearchFiltersDto>> GetAvailableFilters([FromQuery] ProductFilterDto? filter = null)
        {
            try
            {
                var filters = await _productService.GetAvailableFiltersAsync(filter);
                return Ok(new { success = true, data = filters });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener filtros disponibles");
                return BadRequest(new { success = false, message = "Error al obtener filtros", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos activos para selectores
        /// </summary>
        /// <returns>Lista de productos activos</returns>
        [HttpGet("active")]
        [RequirePermission("productos.ver")]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetActiveProducts()
        {
            try
            {
                var products = await _productService.GetActiveProductsAsync();
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos activos");
                return BadRequest(new { success = false, message = "Error al obtener productos activos", error = ex.Message });
            }
        }

        #endregion

        #region Gestión de Inventario

        /// <summary>
        /// Ajusta el stock de un producto
        /// </summary>
        /// <param name="dto">Datos del ajuste de stock</param>
        /// <returns>Confirmación del ajuste</returns>
        [HttpPost("adjust-stock")]
        [RequirePermission("inventario.actualizar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AdjustStock([FromBody] AdjustStockDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

                var result = await _productService.AdjustStockAsync(dto);
                if (!result)
                    return BadRequest(new { success = false, message = "No se pudo ajustar el stock" });

                return Ok(new { success = true, message = "Stock ajustado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ajustar stock del producto {ProductId}", dto.ProductoId);
                return BadRequest(new { success = false, message = "Error al ajustar stock", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el stock de un producto
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="request">Datos del nuevo stock</param>
        /// <returns>Confirmación de actualización</returns>
        [HttpPut("{id:int}/stock")]
        [RequirePermission("inventario.actualizar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                if (request.NuevoStock < 0)
                    return BadRequest(new { success = false, message = "El stock no puede ser negativo" });

                var result = await _productService.UpdateStockAsync(id, request.NuevoStock);
                if (!result)
                    return BadRequest(new { success = false, message = "No se pudo actualizar el stock" });

                return Ok(new { success = true, message = "Stock actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del producto {ProductId}", id);
                return BadRequest(new { success = false, message = "Error al actualizar stock", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos con bajo stock
        /// </summary>
        /// <returns>Lista de productos con bajo stock</returns>
        [HttpGet("low-stock")]
        [RequirePermission("inventario.alertas")]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetLowStockProducts()
        {
            try
            {
                var products = await _productService.GetLowStockProductsAsync();
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con bajo stock");
                return BadRequest(new { success = false, message = "Error al obtener productos con bajo stock", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos sin stock
        /// </summary>
        /// <returns>Lista de productos sin stock</returns>
        [HttpGet("out-of-stock")]
        [RequirePermission("inventario.alertas")]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetOutOfStockProducts()
        {
            try
            {
                var products = await _productService.GetOutOfStockProductsAsync();
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos sin stock");
                return BadRequest(new { success = false, message = "Error al obtener productos sin stock", error = ex.Message });
            }
        }

        #endregion

        #region Operaciones Masivas

        /// <summary>
        /// Cambia el estado de un producto (activo/inactivo)
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Confirmación del cambio</returns>
        [HttpPatch("{id:int}/toggle-status")]
        [RequirePermission("productos.publicar")] // ← Verificar este permiso
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ToggleProductStatus(int id)
        {
            try
            {
                // ✅ AGREGAR DEBUG
                Console.WriteLine($"🔄 ToggleProductStatus - ID: {id}");
                Console.WriteLine($"👤 Usuario: {User?.Identity?.Name}");
                Console.WriteLine($"🔑 Claims: {string.Join(", ", User?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? new string[0])}");

                var result = await _productService.ToggleProductStatusAsync(id);
                if (!result)
                {
                    Console.WriteLine($"❌ Producto {id} no encontrado");
                    return BadRequest(new { success = false, message = "Producto no encontrado" });
                }

                Console.WriteLine($"✅ Estado del producto {id} cambiado exitosamente");
                return Ok(new { success = true, message = "Estado del producto actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ToggleProductStatus {id}: {ex.Message}");
                _logger.LogError(ex, "Error al cambiar estado del producto {ProductId}", id);
                return BadRequest(new { success = false, message = "Error al cambiar estado", error = ex.Message });
            }
        }

        /// <summary>
        /// Cambia el estado de múltiples productos
        /// </summary>
        /// <param name="dto">Datos de la operación masiva</param>
        /// <returns>Cantidad de productos actualizados</returns>
        [HttpPatch("bulk-toggle-status")]
        [RequirePermission("productos.publicar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BulkToggleStatus([FromBody] BulkToggleStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

                var updatedCount = await _productService.BulkToggleStatusAsync(dto.ProductIds, dto.Activo);
                return Ok(new
                {
                    success = true,
                    message = $"{updatedCount} productos actualizados exitosamente",
                    data = new { count = updatedCount }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en operación masiva de cambio de estado");
                return BadRequest(new { success = false, message = "Error en operación masiva", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza precios de múltiples productos
        /// </summary>
        /// <param name="dto">Datos de actualización de precios</param>
        /// <returns>Cantidad de productos actualizados</returns>
        [HttpPatch("bulk-update-prices")]
        [RequirePermission("productos.editar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BulkUpdatePrices([FromBody] BulkPriceUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

                var updatedCount = await _productService.BulkUpdatePricesAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = $"{updatedCount} productos actualizados exitosamente",
                    data = new { count = updatedCount }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en actualización masiva de precios");
                return BadRequest(new { success = false, message = "Error en actualización de precios", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina múltiples productos
        /// </summary>
        /// <param name="dto">IDs de productos a eliminar</param>
        /// <returns>Cantidad de productos eliminados</returns>
        [HttpDelete("bulk-delete")]
        [RequirePermission("productos.eliminar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BulkDelete([FromBody] BulkOperationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

                var deletedCount = await _productService.BulkDeleteAsync(dto.ProductIds);
                return Ok(new
                {
                    success = true,
                    message = $"{deletedCount} productos eliminados exitosamente",
                    data = new { count = deletedCount }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en eliminación masiva de productos");
                return BadRequest(new { success = false, message = "Error en eliminación masiva", error = ex.Message });
            }
        }

        #endregion

        #region Estadísticas y Reportes

        /// <summary>
        /// Obtiene estadísticas generales de productos
        /// </summary>
        /// <returns>Estadísticas de productos</returns>
        [HttpGet("stats")]
        [RequirePermission("reportes.productos")]
        [ProducesResponseType(typeof(ProductStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ProductStatsDto>> GetProductStats()
        {
            try
            {
                var stats = await _productService.GetProductStatsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de productos");
                return BadRequest(new { success = false, message = "Error al obtener estadísticas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene productos más vendidos
        /// </summary>
        /// <param name="count">Cantidad de productos</param>
        /// <returns>Lista de productos más vendidos</returns>
        [HttpGet("best-selling")]
        [RequirePermission("reportes.ventas")]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetBestSellingProducts([FromQuery] int count = 10)
        {
            try
            {
                var products = await _productService.GetBestSellingProductsAsync(count);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                return BadRequest(new { success = false, message = "Error al obtener productos más vendidos", error = ex.Message });
            }
        }

        #endregion

        #region Validaciones

        /// <summary>
        /// Verifica si existe un producto con el nombre dado
        /// </summary>
        /// <param name="name">Nombre del producto</param>
        /// <param name="excludeId">ID a excluir de la búsqueda</param>
        /// <returns>True si existe</returns>
        [HttpGet("exists/name")]
        [RequirePermission("productos.ver")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> ProductExists([FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            try
            {
                var exists = await _productService.ProductExistsAsync(name, excludeId);
                return Ok(new { success = true, data = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de producto");
                return BadRequest(new { success = false, message = "Error en verificación", error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si existe un producto con el SKU dado
        /// </summary>
        /// <param name="sku">SKU del producto</param>
        /// <param name="excludeId">ID a excluir de la búsqueda</param>
        /// <returns>True si existe</returns>
        [HttpGet("exists/sku")]
        [RequirePermission("productos.ver")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> ProductExistsBySku([FromQuery] string sku, [FromQuery] int? excludeId = null)
        {
            try
            {
                var exists = await _productService.SKUExistsAsync(sku, excludeId);
                return Ok(new { success = true, data = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de producto por SKU");
                return BadRequest(new { success = false, message = "Error en verificación", error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si existe un producto con el slug dado
        /// </summary>
        /// <param name="slug">Slug del producto</param>
        /// <param name="excludeId">ID a excluir de la búsqueda</param>
        /// <returns>True si existe</returns>
        [HttpGet("exists/slug")]
        [RequirePermission("productos.ver")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> ProductExistsBySlug([FromQuery] string slug, [FromQuery] int? excludeId = null)
        {
            try
            {
                var exists = await _productService.SlugExistsAsync(slug, excludeId);
                return Ok(new { success = true, data = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de producto por slug");
                return BadRequest(new { success = false, message = "Error en verificación", error = ex.Message });
            }
        }

        #endregion

        #region Métodos Privados

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ??
                             User.FindFirst("sub")?.Value ??
                             User.FindFirst("id")?.Value;

            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        #endregion
    }
}