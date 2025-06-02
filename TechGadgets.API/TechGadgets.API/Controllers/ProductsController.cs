using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TechGadgets.API.Attributes;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Dtos.Products;
using TechGadgets.API.Models.Common;
using TechGadgets.API.Services.Interfaces;
using TechGadgets.API.Configuration;

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
                _logger.LogInformation("🔍 GetProducts - Filtros: Activo={Activo}, Busqueda='{Busqueda}', Page={Page}, PageSize={PageSize}", 
                    filter.Activo, filter.Busqueda, filter.Page, filter.PageSize);

                if (filter.Page < 1) filter.Page = 1;
                if (filter.PageSize < 1 || filter.PageSize > 100) filter.PageSize = 10;

                var result = await _productService.GetProductsAsync(filter);

                _logger.LogInformation("📦 GetProducts - Resultado: TotalItems={TotalItems}, Items={ItemsCount}", 
                    result.TotalItems, result.Items?.Count ?? 0);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en GetProducts");
                await _logService.LogErrorAsync("Error al obtener productos", ex);

                return BadRequest(new
                {
                    success = false,
                    message = "Error al obtener productos",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id:int}")]
        [RequirePermission("productos.ver")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            try
            {
                _logger.LogInformation("🔍 GetProductById - Buscando producto ID: {ProductId}", id);
                
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("❌ Producto {ProductId} no encontrado", id);
                    return NotFound(new { success = false, message = "Producto no encontrado" });
                }

                _logger.LogInformation("✅ Producto {ProductId} encontrado: {ProductName}", id, product.Nombre);

                return Ok(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en GetProductById {ProductId}", id);
                await _logService.LogErrorAsync($"Error al obtener producto con ID {id}", ex);
                
                return BadRequest(new { success = false, message = "Error al obtener producto", error = ex.Message });
            }
        }

        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
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
        /// Crea un producto básico (sin imágenes). Las imágenes se agregan después usando otros endpoints.
        /// </summary>
        [HttpPost]
        [RequirePermission("productos.crear")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createDto)
        {
            try
            {
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

                _logger.LogInformation("📦 Creando producto básico: {ProductName}", createDto.Nombre);

                var result = await _productService.CreateProductAsync(createDto);
                
                _logger.LogInformation("✅ Producto creado exitosamente: ID={ProductId}, Nombre='{ProductName}'", 
                    result.Id, result.Nombre);

                return CreatedAtAction(
                    nameof(GetProductById),
                    new { id = result.Id },
                    new ApiResponse<ProductDto>
                    {
                        Success = true,
                        Message = "Producto creado exitosamente",
                        Data = result
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creando producto: {ProductName}", createDto.Nombre);
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Error al crear el producto"
                });
            }
        }

        /// <summary>
        /// Crea un producto y automáticamente sube las imágenes a Supabase
        /// </summary>
        [HttpPost("create-with-images")]
        [RequirePermission("productos.crear")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProductWithImages([FromForm] CreateProductWithImagesRequest request)
        {
            try
            {
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

                _logger.LogInformation("📦🖼️ Creando producto con imágenes: {ProductName}", request.Nombre);

                // ✅ 1. CREAR PRODUCTO PRIMERO
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
                    Imagenes = new List<CreateProductImageDto>() // Sin imágenes por ahora
                };

                var product = await _productService.CreateProductAsync(createProductDto);
                _logger.LogInformation("✅ Producto base creado: ID={ProductId}", product.Id);

                // ✅ 2. SUBIR IMÁGENES A SUPABASE Y ASOCIARLAS AL PRODUCTO
                var imageUrls = new List<CreateProductImageDto>();
                
                if (request.ImageFiles != null && request.ImageFiles.Count > 0)
                {
                    _logger.LogInformation("📸 Subiendo {Count} archivos de imagen a Supabase", request.ImageFiles.Count);
                    
                    // Crear carpeta específica para este producto
                    var productFolder = $"product-{product.Id}";
                    
                    var uploadResult = await _storageService.UploadMultipleImagesAsync(
                        request.ImageFiles, 
                        productFolder
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
                        
                        _logger.LogInformation("✅ Imágenes subidas exitosamente: {Count}/{Total}", 
                            uploadResult.SuccessfulUploads.Count, request.ImageFiles.Count);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Error subiendo imágenes: {Errors}", 
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

                // ✅ 4. ASOCIAR IMÁGENES AL PRODUCTO (SI TENEMOS IMÁGENES)
                if (imageUrls.Any())
                {
                    _logger.LogInformation("🔗 Asociando {Count} imágenes al producto {ProductId}", imageUrls.Count, product.Id);
                    
                    // Actualizar el producto con las imágenes
                    var updateDto = new UpdateProductDto
                    {
                        SKU = product.SKU,
                        Nombre = product.Nombre,
                        DescripcionCorta = product.DescripcionCorta,
                        DescripcionLarga = product.DescripcionLarga,
                        Precio = product.Precio,
                        PrecioComparacion = product.PrecioComparacion,
                        Costo = product.Costo,
                        CategoriaId = product.CategoriaId,
                        MarcaId = product.MarcaId,
                        Tipo = product.Tipo,
                        Estado = product.Estado,
                        Destacado = product.Destacado,
                        Nuevo = product.Nuevo,
                        EnOferta = product.EnOferta,
                        Peso = product.Peso,
                        Dimensiones = product.Dimensiones,
                        MetaTitulo = product.MetaTitulo,
                        MetaDescripcion = product.MetaDescripcion,
                        PalabrasClaves = product.PalabrasClaves,
                        RequiereEnvio = product.RequiereEnvio,
                        PermiteReseñas = product.PermiteReseñas,
                        Garantia = product.Garantia,
                        Orden = product.Orden,
                        Activo = product.Activo,
                        Imagenes = imageUrls.Select(img => new UpdateProductImageDto
                        {
                            Url = img.Url,
                            AltText = img.AltText,
                            EsPrincipal = img.EsPrincipal,
                            Orden = img.Orden,
                            Eliminar = false
                        }).ToList()
                    };

                    product = await _productService.UpdateProductAsync(product.Id, updateDto);
                }

                _logger.LogInformation("🎉 Producto con imágenes creado exitosamente: ID={ProductId}, Imágenes={ImageCount}", 
                    product.Id, imageUrls.Count);

                return CreatedAtAction(
                    nameof(GetProductById),
                    new { id = product.Id },
                    new ApiResponse<ProductDto>
                    {
                        Success = true,
                        Message = $"Producto creado exitosamente con {imageUrls.Count} imágenes",
                        Data = product
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creando producto con imágenes: {ProductName}", request.Nombre);
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Error al crear el producto con imágenes"
                });
            }
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        [HttpPut("{id:int}")]
        [RequirePermission("productos.editar")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Datos inválidos",
                        Errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                    });
                }

                _logger.LogInformation("📝 Actualizando producto {ProductId}", id);

                var result = await _productService.UpdateProductAsync(id, updateDto);
                
                if (result == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                _logger.LogInformation("✅ Producto {ProductId} actualizado exitosamente", id);

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Message = "Producto actualizado exitosamente",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error actualizando producto {ProductId}", id);
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Error al actualizar el producto"
                });
            }
        }

        /// <summary>
        /// Actualiza un producto y permite agregar nuevas imágenes
        /// </summary>
        [HttpPut("{id:int}/update-with-images")]
        [RequirePermission("productos.editar")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProductWithImages(int id, [FromForm] UpdateProductWithImagesRequest request)
        {
            try
            {
                _logger.LogInformation("📝🖼️ Actualizando producto {ProductId} con imágenes", id);

                // ✅ 1. VERIFICAR QUE EL PRODUCTO EXISTE
                var existingProduct = await _productService.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                // ✅ 2. SUBIR NUEVAS IMÁGENES A SUPABASE
                var newImageUrls = new List<CreateProductImageDto>();
                
                if (request.NewImageFiles != null && request.NewImageFiles.Count > 0)
                {
                    _logger.LogInformation("📸 Subiendo {Count} nuevas imágenes para producto {ProductId}", 
                        request.NewImageFiles.Count, id);

                    var productFolder = $"product-{id}";
                    var uploadResult = await _storageService.UploadMultipleImagesAsync(
                        request.NewImageFiles, 
                        productFolder
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

                // ✅ 3. AGREGAR URLs EXTERNAS NUEVAS
                if (request.NewExternalImageUrls != null && request.NewExternalImageUrls.Any())
                {
                    foreach (var url in request.NewExternalImageUrls)
                    {
                        if (!string.IsNullOrWhiteSpace(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                        {
                            newImageUrls.Add(new CreateProductImageDto
                            {
                                Url = url,
                                AltText = $"{request.Nombre} - Nueva imagen externa",
                                EsPrincipal = false,
                                Orden = existingProduct.Imagenes.Count + newImageUrls.Count + 1
                            });
                        }
                    }
                }

                // ✅ 4. COMBINAR IMÁGENES EXISTENTES CON NUEVAS
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

                // ✅ 5. ACTUALIZAR PRODUCTO
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
                
                _logger.LogInformation("✅ Producto {ProductId} actualizado con {NewImageCount} nuevas imágenes", 
                    id, newImageUrls.Count);

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Message = $"Producto actualizado exitosamente con {newImageUrls.Count} nuevas imágenes",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error actualizando producto {ProductId} con imágenes", id);
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Error al actualizar el producto"
                });
            }
        }

        /// <summary>
        /// Agrega imágenes a un producto existente
        /// </summary>
        [HttpPost("{id:int}/add-images")]
        [RequirePermission("productos.editar")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddImagesToProduct(int id, IFormFileCollection imageFiles, [FromForm] List<string>? externalUrls = null)
        {
            try
            {
                _logger.LogInformation("📸 Agregando imágenes al producto {ProductId}", id);

                // ✅ 1. VERIFICAR QUE EL PRODUCTO EXISTE
                var existingProduct = await _productService.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                var newImages = new List<CreateProductImageDto>();

                // ✅ 2. SUBIR ARCHIVOS DE IMAGEN
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    var productFolder = $"product-{id}";
                    var uploadResult = await _storageService.UploadMultipleImagesAsync(imageFiles, productFolder);

                    if (uploadResult.Success && uploadResult.SuccessfulUploads.Any())
                    {
                        for (int i = 0; i < uploadResult.SuccessfulUploads.Count; i++)
                        {
                            var uploadedImage = uploadResult.SuccessfulUploads[i];
                            newImages.Add(new CreateProductImageDto
                            {
                                Url = uploadedImage.Url,
                                AltText = uploadedImage.AltText ?? $"{existingProduct.Nombre} - Imagen {existingProduct.Imagenes.Count + i + 1}",
                                EsPrincipal = existingProduct.Imagenes.Count == 0 && i == 0, // Primera imagen si no hay otras
                                Orden = existingProduct.Imagenes.Count + i + 1
                            });
                        }
                    }
                }

                // ✅ 3. AGREGAR URLs EXTERNAS
                if (externalUrls != null && externalUrls.Any())
                {
                    foreach (var url in externalUrls)
                    {
                        if (!string.IsNullOrWhiteSpace(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                        {
                            newImages.Add(new CreateProductImageDto
                            {
                                Url = url,
                                AltText = $"{existingProduct.Nombre} - Imagen externa",
                                EsPrincipal = existingProduct.Imagenes.Count == 0 && newImages.Count == 0,
                                Orden = existingProduct.Imagenes.Count + newImages.Count + 1
                            });
                        }
                    }
                }

                if (!newImages.Any())
                {
                    return BadRequest(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "No se proporcionaron imágenes válidas"
                    });
                }

                // ✅ 4. AGREGAR LAS IMÁGENES AL PRODUCTO
                foreach (var newImage in newImages)
                {
                    await _productService.AddProductImageAsync(id, newImage);
                }

                // ✅ 5. OBTENER EL PRODUCTO ACTUALIZADO
                var updatedProduct = await _productService.GetProductByIdAsync(id);

                _logger.LogInformation("✅ Se agregaron {ImageCount} imágenes al producto {ProductId}", newImages.Count, id);

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Message = $"Se agregaron {newImages.Count} imágenes al producto exitosamente",
                    Data = updatedProduct
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error agregando imágenes al producto {ProductId}", id);
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Error al agregar imágenes al producto"
                });
            }
        }

        /// <summary>
        /// Elimina una imagen específica de un producto
        /// </summary>
        [HttpDelete("{productId:int}/images/{imageId:int}")]
        [RequirePermission("productos.editar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteProductImage(int productId, int imageId)
        {
            try
            {
                _logger.LogInformation("🗑️ Eliminando imagen {ImageId} del producto {ProductId}", imageId, productId);

                // ✅ 1. OBTENER INFORMACIÓN DE LA IMAGEN
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

                // ✅ 2. ELIMINAR DE SUPABASE (si es una imagen subida)
                if (imageToDelete.Url.Contains("supabase.co") && !imageToDelete.Url.StartsWith("http://external"))
                {
                    try
                    {
                        // Extraer path de la URL de Supabase
                        var uri = new Uri(imageToDelete.Url);
                        var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        
                        // Buscar el segmento después de "object" en la URL de Supabase
                        var objectIndex = Array.IndexOf(pathSegments, "object");
                        if (objectIndex >= 0 && objectIndex < pathSegments.Length - 1)
                        {
                            var path = string.Join("/", pathSegments.Skip(objectIndex + 1));
                            await _storageService.DeleteImageAsync(path);
                            _logger.LogInformation("🗑️ Imagen eliminada de Supabase: {Path}", path);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Error eliminando imagen de Supabase: {Url}", imageToDelete.Url);
                        // Continuar con la eliminación de la BD aunque falle Supabase
                    }
                }

                // ✅ 3. ELIMINAR DE LA BASE DE DATOS
                var success = await _productService.DeleteProductImageAsync(productId, imageId);
                
                if (success)
                {
                    _logger.LogInformation("✅ Imagen {ImageId} eliminada del producto {ProductId}", imageId, productId);
                    return Ok(new { success = true, message = "Imagen eliminada exitosamente" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Error al eliminar la imagen de la base de datos" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error eliminando imagen {ImageId} del producto {ProductId}", imageId, productId);
                return BadRequest(new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina un producto (soft delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        [RequirePermission("productos.eliminar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                _logger.LogInformation("🗑️ Eliminando producto {ProductId}", id);

                var success = await _productService.DeleteProductAsync(id);
                
                if (!success)
                {
                    return NotFound(new { success = false, message = "Producto no encontrado" });
                }

                _logger.LogInformation("✅ Producto {ProductId} eliminado exitosamente", id);
                return Ok(new { success = true, message = "Producto eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error eliminando producto {ProductId}", id);
                return BadRequest(new { success = false, message = "Error al eliminar el producto" });
            }
        }

        #endregion

        #region Búsqueda y Filtros (Público para catálogo)

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

        [HttpGet("active")]
        [RequirePermission("productos.ver")]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
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

        [HttpPost("adjust-stock")]
        [RequirePermission("inventario.ajustar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        [HttpPut("{id:int}/stock")]
        [RequirePermission("inventario.ajustar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                if (request.NuevoStock < 0)
                    return BadRequest(new { success = false, message = "El stock no puede ser negativo" });

                var result = await _productService.UpdateStockAsync(id, request.NuevoStock);
                if (!result)
                    return BadRequest(new { success = false, message = "No se pudo actualizar el stock" });

                _logger.LogInformation("✅ Stock actualizado para producto {ProductId}: {NewStock}", id, request.NuevoStock);
                return Ok(new { success = true, message = "Stock actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del producto {ProductId}", id);
                return BadRequest(new { success = false, message = "Error al actualizar stock", error = ex.Message });
            }
        }

        [HttpGet("low-stock")]
        [RequirePermission("inventario.alertas")]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
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

        [HttpGet("out-of-stock")]
        [RequirePermission("inventario.alertas")]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
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

        [HttpPatch("{id:int}/toggle-status")]
        [RequirePermission("productos.editar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ToggleProductStatus(int id)
        {
            try
            {
                _logger.LogInformation("🔄 Cambiando estado del producto {ProductId}", id);

                var result = await _productService.ToggleProductStatusAsync(id);
                if (!result)
                {
                    return BadRequest(new { success = false, message = "Producto no encontrado" });
                }

                _logger.LogInformation("✅ Estado del producto {ProductId} cambiado exitosamente", id);
                return Ok(new { success = true, message = "Estado del producto actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cambiar estado del producto {ProductId}", id);
                return BadRequest(new { success = false, message = "Error al cambiar estado", error = ex.Message });
            }
        }

        [HttpPatch("bulk-toggle-status")]
        [RequirePermission("productos.editar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        [HttpPatch("bulk-update-prices")]
        [RequirePermission("productos.editar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        [HttpDelete("bulk-delete")]
        [RequirePermission("productos.eliminar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        [HttpGet("stats")]
        [RequirePermission("reportes.productos")]
        [ProducesResponseType(typeof(ProductStatsDto), StatusCodes.Status200OK)]
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

        [HttpGet("best-selling")]
        [RequirePermission("reportes.ventas")]
        [ProducesResponseType(typeof(IEnumerable<ProductSummaryDto>), StatusCodes.Status200OK)]
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

        #region Debug y Testing

        [HttpGet("debug/quick")]
        [AllowAnonymous]
        public async Task<IActionResult> QuickDebug()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                _logger.LogInformation("🔗 Conexión a BD: {CanConnect}", canConnect);

                if (!canConnect)
                {
                    return BadRequest(new { error = "No se puede conectar a la base de datos" });
                }

                var totalCount = await _context.Productos.CountAsync();
                _logger.LogInformation("📊 Total productos en BD: {TotalCount}", totalCount);

                var rawProducts = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Take(3)
                    .ToListAsync();

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
                _logger.LogError(ex, "❌ Error en QuickDebug");
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("debug/{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductDebug(int id)
        {
            try
            {
                var connectionTest = await _context.Database.CanConnectAsync();
                if (!connectionTest)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede conectar a la base de datos"
                    });
                }

                var producto = await _context.Productos
                    .Include(p => p.PrdCategoria)
                    .Include(p => p.PrdMarca)
                    .Include(p => p.Inventarios)
                    .Include(p => p.ProductosImagenes)
                    .FirstOrDefaultAsync(p => p.PrdId == id);

                if (producto == null)
                {
                    var totalProducts = await _context.Productos.CountAsync();
                    return NotFound(new
                    {
                        success = false,
                        message = $"Producto {id} no encontrado",
                        totalProductsInDB = totalProducts,
                        suggestion = totalProducts > 0 ? "Intenta con otro ID" : "No hay productos en la base de datos"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Producto {id} encontrado exitosamente",
                    timestamp = DateTime.UtcNow,
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
                    RelacionesEstado = new
                    {
                        TieneCategoria = producto.PrdCategoria != null,
                        TieneMarca = producto.PrdMarca != null,
                        TieneInventarios = producto.Inventarios?.Any() == true,
                        TieneImagenes = producto.ProductosImagenes?.Any() == true,
                        CantidadInventarios = producto.Inventarios?.Count ?? 0,
                        CantidadImagenes = producto.ProductosImagenes?.Count ?? 0
                    },
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