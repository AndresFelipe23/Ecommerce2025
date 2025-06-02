using Microsoft.AspNetCore.Mvc;
using TechGadgets.API.Services.Interfaces;
using TechGadgets.API.Attributes;
using System.ComponentModel.DataAnnotations;
using TechGadgets.API.Models.Common;
using TechGadgets.API.Dtos.Products;
using Swashbuckle.AspNetCore.Annotations;

namespace TechGadgets.API.Controllers;

/// <summary>
/// Controlador para la gestión de imágenes de productos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Gestión de imágenes de productos")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class ProductosImagenController : ControllerBase
{
    private readonly IProductosImagenService _imagenService;
    private readonly ILogger<ProductosImagenController> _logger;

    public ProductosImagenController(
        IProductosImagenService imagenService,
        ILogger<ProductosImagenController> logger)
    {
        _imagenService = imagenService;
        _logger = logger;
    }

    #region Consultas Básicas

    /// <summary>
    /// Obtiene todas las imágenes de un producto específico
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Lista de imágenes del producto</returns>
    [HttpGet("producto/{productoId:int}")]
    [RequirePermission("productos.ver")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductoImagenDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetImagenesByProducto([Range(1, int.MaxValue)] int productoId)
    {
        try
        {
            _logger.LogInformation("Obteniendo imágenes para producto {ProductoId}", productoId);

            var imagenes = await _imagenService.GetImagenesByProductoIdAsync(productoId);
            var imagenesList = imagenes.ToList();

            if (!imagenesList.Any())
            {
                _logger.LogWarning("No se encontraron imágenes para el producto {ProductoId}", productoId);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"No se encontraron imágenes para el producto {productoId}"
                });
            }

            _logger.LogInformation("Se encontraron {Count} imágenes para el producto {ProductoId}", imagenesList.Count, productoId);

            return Ok(new ApiResponse<IEnumerable<ProductoImagenDto>>
            {
                Success = true,
                Data = imagenesList,
                Message = $"Se encontraron {imagenesList.Count} imágenes"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imágenes del producto {ProductoId}", productoId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al obtener las imágenes del producto"
            });
        }
    }

    /// <summary>
    /// Obtiene todas las imágenes de una variante específica
    /// </summary>
    /// <param name="varianteId">ID de la variante</param>
    /// <returns>Lista de imágenes de la variante</returns>
    [HttpGet("variante/{varianteId:int}")]
    [RequirePermission("productos.ver")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductoImagenDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetImagenesByVariante([Range(1, int.MaxValue)] int varianteId)
    {
        try
        {
            _logger.LogInformation("Obteniendo imágenes para variante {VarianteId}", varianteId);

            var imagenes = await _imagenService.GetImagenesByVarianteIdAsync(varianteId);
            var imagenesList = imagenes.ToList();

            if (!imagenesList.Any())
            {
                _logger.LogWarning("No se encontraron imágenes para la variante {VarianteId}", varianteId);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"No se encontraron imágenes para la variante {varianteId}"
                });
            }

            _logger.LogInformation("Se encontraron {Count} imágenes para la variante {VarianteId}", imagenesList.Count, varianteId);

            return Ok(new ApiResponse<IEnumerable<ProductoImagenDto>>
            {
                Success = true,
                Data = imagenesList,
                Message = $"Se encontraron {imagenesList.Count} imágenes"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imágenes de la variante {VarianteId}", varianteId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al obtener las imágenes de la variante"
            });
        }
    }

    /// <summary>
    /// Obtiene una imagen específica por su ID
    /// </summary>
    /// <param name="id">ID de la imagen</param>
    /// <returns>Datos de la imagen</returns>
    [HttpGet("{id:int}")]
    [RequirePermission("productos.ver")]
    [ProducesResponseType(typeof(ApiResponse<ProductoImagenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetImagenById([Range(1, int.MaxValue)] int id)
    {
        try
        {
            _logger.LogInformation("Obteniendo imagen {ImagenId}", id);

            var imagen = await _imagenService.GetImagenByIdAsync(id);

            if (imagen == null)
            {
                _logger.LogWarning("Imagen {ImagenId} no encontrada", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Imagen con ID {id} no encontrada"
                });
            }

            _logger.LogInformation("Imagen {ImagenId} encontrada", id);

            return Ok(new ApiResponse<ProductoImagenDto>
            {
                Success = true,
                Data = imagen,
                Message = "Imagen encontrada"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imagen {ImagenId}", id);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al obtener la imagen"
            });
        }
    }

    /// <summary>
    /// Obtiene la imagen principal de un producto
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Imagen principal del producto</returns>
    [HttpGet("producto/{productoId:int}/principal")]
    [RequirePermission("productos.ver")]
    [ProducesResponseType(typeof(ApiResponse<ProductoImagenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetImagenPrincipalByProducto([Range(1, int.MaxValue)] int productoId)
    {
        try
        {
            _logger.LogInformation("Obteniendo imagen principal para producto {ProductoId}", productoId);

            var imagen = await _imagenService.GetImagenPrincipalByProductoIdAsync(productoId);

            if (imagen == null)
            {
                _logger.LogWarning("No se encontró imagen principal para el producto {ProductoId}", productoId);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"No se encontró imagen principal para el producto {productoId}"
                });
            }

            _logger.LogInformation("Imagen principal encontrada para producto {ProductoId}", productoId);

            return Ok(new ApiResponse<ProductoImagenDto>
            {
                Success = true,
                Data = imagen,
                Message = "Imagen principal encontrada"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imagen principal del producto {ProductoId}", productoId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al obtener la imagen principal"
            });
        }
    }

    #endregion

    #region Operaciones CRUD

    /// <summary>
    /// Crea una nueva imagen de producto
    /// </summary>
    /// <param name="createDto">Datos para crear la imagen</param>
    /// <returns>Imagen creada</returns>
    [HttpPost]
    [RequirePermission("productos.crear")]
    [ProducesResponseType(typeof(ApiResponse<ProductoImagenDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateImagen([FromBody] CreateProductoImagenDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos para crear imagen: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos inválidos",
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            _logger.LogInformation("Creando nueva imagen para producto {ProductoId}", createDto.ProductoId);

            var imagenCreada = await _imagenService.CreateImagenAsync(createDto);

            _logger.LogInformation("Imagen creada con ID {ImagenId} para producto {ProductoId}", 
                imagenCreada.Id, createDto.ProductoId);

            return CreatedAtAction(
                nameof(GetImagenById),
                new { id = imagenCreada.Id },
                new ApiResponse<ProductoImagenDto>
                {
                    Success = true,
                    Data = imagenCreada,
                    Message = "Imagen creada exitosamente"
                }
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear imagen para producto {ProductoId}", createDto.ProductoId);
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear imagen para producto {ProductoId}", createDto.ProductoId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al crear la imagen"
            });
        }
    }

    /// <summary>
    /// Actualiza una imagen existente
    /// </summary>
    /// <param name="id">ID de la imagen</param>
    /// <param name="updateDto">Datos para actualizar</param>
    /// <returns>Imagen actualizada</returns>
    [HttpPut("{id:int}")]
    [RequirePermission("productos.editar")]
    [ProducesResponseType(typeof(ApiResponse<ProductoImagenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateImagen([Range(1, int.MaxValue)] int id, [FromBody] UpdateProductoImagenDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos para actualizar imagen {ImagenId}: {Errors}", 
                    id, string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos inválidos",
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            _logger.LogInformation("Actualizando imagen {ImagenId}", id);

            var imagenActualizada = await _imagenService.UpdateImagenAsync(id, updateDto);

            if (imagenActualizada == null)
            {
                _logger.LogWarning("Imagen {ImagenId} no encontrada para actualizar", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Imagen con ID {id} no encontrada"
                });
            }

            _logger.LogInformation("Imagen {ImagenId} actualizada exitosamente", id);

            return Ok(new ApiResponse<ProductoImagenDto>
            {
                Success = true,
                Data = imagenActualizada,
                Message = "Imagen actualizada exitosamente"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar imagen {ImagenId}", id);
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar imagen {ImagenId}", id);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al actualizar la imagen"
            });
        }
    }

    /// <summary>
    /// Elimina una imagen
    /// </summary>
    /// <param name="id">ID de la imagen</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id:int}")]
    [RequirePermission("productos.eliminar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteImagen([Range(1, int.MaxValue)] int id)
    {
        try
        {
            _logger.LogInformation("Eliminando imagen {ImagenId}", id);

            var eliminada = await _imagenService.DeleteImagenAsync(id);

            if (!eliminada)
            {
                _logger.LogWarning("Imagen {ImagenId} no encontrada para eliminar", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Imagen con ID {id} no encontrada"
                });
            }

            _logger.LogInformation("Imagen {ImagenId} eliminada exitosamente", id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Imagen eliminada exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar imagen {ImagenId}", id);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al eliminar la imagen"
            });
        }
    }

    #endregion

    #region Operaciones Masivas

    /// <summary>
    /// Crea múltiples imágenes para un producto
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="imagenes">Lista de imágenes a crear</param>
    /// <returns>Lista de imágenes creadas</returns>
    [HttpPost("producto/{productoId:int}/multiple")]
    [RequirePermission("productos.crear")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductoImagenDto>>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateMultipleImagenes(
        [Range(1, int.MaxValue)] int productoId,
        [FromBody] IEnumerable<CreateProductoImagenDto> imagenes)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos para crear múltiples imágenes del producto {ProductoId}", productoId);
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos inválidos",
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var imagenesList = imagenes.ToList();
            if (!imagenesList.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Debe proporcionar al menos una imagen"
                });
            }

            _logger.LogInformation("Creando {Count} imágenes para producto {ProductoId}", imagenesList.Count, productoId);

            var imagenesCreadas = await _imagenService.CreateMultipleImagenesAsync(productoId, imagenesList);
            var imagenesResultado = imagenesCreadas.ToList();

            _logger.LogInformation("Se crearon {Count} imágenes para el producto {ProductoId}", imagenesResultado.Count, productoId);

            return CreatedAtAction(
                nameof(GetImagenesByProducto),
                new { productoId },
                new ApiResponse<IEnumerable<ProductoImagenDto>>
                {
                    Success = true,
                    Data = imagenesResultado,
                    Message = $"Se crearon {imagenesResultado.Count} imágenes exitosamente"
                }
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear múltiples imágenes para producto {ProductoId}", productoId);
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear múltiples imágenes para producto {ProductoId}", productoId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al crear las imágenes"
            });
        }
    }

    /// <summary>
    /// Actualiza múltiples imágenes de un producto (crear, actualizar, eliminar)
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="imagenes">Lista de imágenes a procesar</param>
    /// <returns>Lista de imágenes resultantes</returns>
    [HttpPut("producto/{productoId:int}/multiple")]
    [RequirePermission("productos.editar")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductoImagenDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMultipleImagenes(
        [Range(1, int.MaxValue)] int productoId,
        [FromBody] IEnumerable<UpdateProductoImagenDto> imagenes)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos para actualizar múltiples imágenes del producto {ProductoId}", productoId);
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos inválidos",
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var imagenesList = imagenes.ToList();
            if (!imagenesList.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Debe proporcionar al menos una imagen"
                });
            }

            _logger.LogInformation("Actualizando múltiples imágenes para producto {ProductoId}", productoId);

            var imagenesResultado = await _imagenService.UpdateMultipleImagenesAsync(productoId, imagenesList);
            var imagenesActualizadas = imagenesResultado.ToList();

            _logger.LogInformation("Se procesaron múltiples imágenes para el producto {ProductoId}, resultado: {Count} imágenes", 
                productoId, imagenesActualizadas.Count);

            return Ok(new ApiResponse<IEnumerable<ProductoImagenDto>>
            {
                Success = true,
                Data = imagenesActualizadas,
                Message = "Imágenes procesadas exitosamente"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar múltiples imágenes para producto {ProductoId}", productoId);
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar múltiples imágenes para producto {ProductoId}", productoId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al procesar las imágenes"
            });
        }
    }

    /// <summary>
    /// Actualiza el orden de las imágenes de un producto
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="ordenImagenes">Lista con ID y nuevo orden</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPatch("producto/{productoId:int}/orden")]
    [RequirePermission("productos.editar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrdenImagenes(
        [Range(1, int.MaxValue)] int productoId,
        [FromBody] IEnumerable<UpdateOrdenImagenDto> ordenImagenes)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos para actualizar orden de imágenes del producto {ProductoId}", productoId);
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos inválidos",
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var ordenList = ordenImagenes.ToList();
            if (!ordenList.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Debe proporcionar al menos una imagen para reordenar"
                });
            }

            _logger.LogInformation("Actualizando orden de {Count} imágenes para producto {ProductoId}", ordenList.Count, productoId);

            var actualizado = await _imagenService.UpdateOrdenImagenesAsync(productoId, ordenList);

            if (!actualizado)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al actualizar el orden de las imágenes"
                });
            }

            _logger.LogInformation("Orden de imágenes actualizado para producto {ProductoId}", productoId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Orden de imágenes actualizado exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar orden de imágenes para producto {ProductoId}", productoId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al actualizar el orden de las imágenes"
            });
        }
    }

    #endregion

    #region Operaciones Especiales

    /// <summary>
    /// Establece una imagen como principal para un producto
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="imagenId">ID de la imagen a establecer como principal</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPatch("producto/{productoId:int}/principal/{imagenId:int}")]
    [RequirePermission("productos.editar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetImagenPrincipal(
        [Range(1, int.MaxValue)] int productoId,
        [Range(1, int.MaxValue)] int imagenId)
    {
        try
        {
            _logger.LogInformation("Estableciendo imagen {ImagenId} como principal para producto {ProductoId}", imagenId, productoId);

            var establecida = await _imagenService.SetImagenPrincipalAsync(productoId, imagenId);

            if (!establecida)
            {
                _logger.LogWarning("No se pudo establecer imagen {ImagenId} como principal para producto {ProductoId}", imagenId, productoId);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Imagen {imagenId} no encontrada para el producto {productoId}"
                });
            }

            _logger.LogInformation("Imagen {ImagenId} establecida como principal para producto {ProductoId}", imagenId, productoId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Imagen establecida como principal exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al establecer imagen {ImagenId} como principal para producto {ProductoId}", imagenId, productoId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al establecer la imagen como principal"
            });
        }
    }

    /// <summary>
    /// Cambia el estado activo/inactivo de una imagen
    /// </summary>
    /// <param name="id">ID de la imagen</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPatch("{id:int}/toggle-status")]
    [RequirePermission("productos.editar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ToggleImagenStatus([Range(1, int.MaxValue)] int id)
    {
        try
        {
            _logger.LogInformation("Cambiando estado de imagen {ImagenId}", id);

            var cambiado = await _imagenService.ToggleImagenStatusAsync(id);

            if (!cambiado)
            {
                _logger.LogWarning("Imagen {ImagenId} no encontrada para cambiar estado", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Imagen con ID {id} no encontrada"
                });
            }

            _logger.LogInformation("Estado de imagen {ImagenId} cambiado exitosamente", id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Estado de la imagen cambiado exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de imagen {ImagenId}", id);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error al cambiar el estado de la imagen"
            });
        }
    }

    #endregion
}

