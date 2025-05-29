using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGadgets.API.Attributes;
using TechGadgets.API.Configuration;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly ISupabaseStorageService _storageService;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(
            ISupabaseStorageService storageService,
            ILogger<ImagesController> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        /// <summary>
        /// Sube una imagen individual
        /// </summary>
        /// <param name="file">Archivo de imagen</param>
        /// <param name="altText">Texto alternativo</param>
        /// <param name="folder">Carpeta de destino (opcional)</param>
        /// <returns>Información de la imagen subida</returns>
        [HttpPost("upload")]
        [RequirePermission("productos.crear")]
        [ProducesResponseType(typeof(ImageUploadResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadImage(
            IFormFile file,
            [FromForm] string? altText = null,
            [FromForm] string? folder = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ImageUploadResponseDto
                    {
                        Success = false,
                        Error = "No se recibió ningún archivo"
                    });
                }

                _logger.LogInformation("Iniciando subida de imagen: {FileName}, Tamaño: {Size} bytes", 
                    file.FileName, file.Length);

                var result = await _storageService.UploadImageAsync(file, altText, folder);
                
                if (result.Success)
                {
                    _logger.LogInformation("Imagen subida exitosamente: {FileName} -> {Url}", 
                        file.FileName, result.Data?.Url);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error al subir imagen {FileName}: {Error}", 
                        file.FileName, result.Error);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al subir imagen: {FileName}", file?.FileName);
                return BadRequest(new ImageUploadResponseDto
                {
                    Success = false,
                    Error = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Sube múltiples imágenes
        /// </summary>
        /// <param name="files">Archivos de imagen</param>
        /// <param name="folder">Carpeta de destino (opcional)</param>
        /// <returns>Resultado de la subida múltiple</returns>
        [HttpPost("upload-multiple")]
        [RequirePermission("productos.crear")]
        [ProducesResponseType(typeof(MultipleImageUploadResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadMultipleImages(
            IFormFileCollection files,
            [FromForm] string? folder = null)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new MultipleImageUploadResponseDto
                    {
                        Success = false,
                        Message = "No se recibieron archivos"
                    });
                }

                if (files.Count > 10)
                {
                    return BadRequest(new MultipleImageUploadResponseDto
                    {
                        Success = false,
                        Message = "Máximo 10 archivos por vez"
                    });
                }

                _logger.LogInformation("Iniciando subida múltiple de {Count} imágenes", files.Count);

                var result = await _storageService.UploadMultipleImagesAsync(files, folder);
                
                _logger.LogInformation("Subida múltiple completada: {Success}/{Total} exitosas", 
                    result.SuccessfulCount, result.TotalFiles);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en subida múltiple");
                return BadRequest(new MultipleImageUploadResponseDto
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Elimina una imagen
        /// </summary>
        /// <param name="request">Datos de la imagen a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("delete")]
        [RequirePermission("productos.eliminar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteImage([FromBody] DeleteImageRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Path))
                {
                    return BadRequest(new { success = false, message = "La ruta de la imagen es requerida" });
                }

                _logger.LogInformation("Eliminando imagen: {Path}", request.Path);

                var success = await _storageService.DeleteImageAsync(request.Path);
                
                if (success)
                {
                    _logger.LogInformation("Imagen eliminada exitosamente: {Path}", request.Path);
                    return Ok(new { success = true, message = "Imagen eliminada exitosamente" });
                }
                else
                {
                    return NotFound(new { success = false, message = "Imagen no encontrada" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen: {Path}", request.Path);
                return BadRequest(new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina múltiples imágenes
        /// </summary>
        /// <param name="paths">Rutas de las imágenes a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("delete-multiple")]
        [RequirePermission("productos.eliminar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteMultipleImages([FromBody] List<string> paths)
        {
            try
            {
                if (paths == null || paths.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Se requiere al menos una ruta" });
                }

                _logger.LogInformation("Eliminando {Count} imágenes", paths.Count);

                var success = await _storageService.DeleteMultipleImagesAsync(paths);
                
                if (success)
                {
                    _logger.LogInformation("{Count} imágenes eliminadas exitosamente", paths.Count);
                    return Ok(new { success = true, message = $"{paths.Count} imágenes eliminadas exitosamente" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Error al eliminar algunas imágenes" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar múltiples imágenes");
                return BadRequest(new { success = false, message = "Error interno del servidor" });
            }
        }
    
        /// <summary>
        /// Lista imágenes en una carpeta
        /// </summary>
        /// <param name="folder">Carpeta a listar (opcional)</param>
        /// <returns>Lista de nombres de archivos</returns>
        [HttpGet("list")]
        [RequirePermission("productos.leer")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListImages([FromQuery] string? folder = null)
        {
            try
            {
                var images = await _storageService.ListImagesAsync(folder);
                return Ok(new { success = true, data = images });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar imágenes en folder: {Folder}", folder);
                return BadRequest(new { success = false, message = "Error al listar imágenes" });
            }
        }

        /// <summary>
        /// Obtiene la URL pública de una imagen
        /// </summary>
        /// <param name="path">Ruta de la imagen</param>
        /// <returns>URL pública</returns>
        [HttpGet("public-url")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPublicUrl([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return BadRequest(new { success = false, message = "La ruta es requerida" });
                }

                var url = await _storageService.GetPublicUrlAsync(path);
                
                if (string.IsNullOrEmpty(url))
                {
                    return NotFound(new { success = false, message = "Imagen no encontrada" });
                }

                return Ok(new { success = true, url = url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener URL pública: {Path}", path);
                return BadRequest(new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Verifica si una imagen existe
        /// </summary>
        /// <param name="path">Ruta de la imagen</param>
        /// <returns>True si existe, false si no</returns>
        [HttpGet("exists")]
        [RequirePermission("productos.leer")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> ImageExists([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return BadRequest(new { success = false, message = "La ruta es requerida" });
                }

                var exists = await _storageService.ImageExistsAsync(path);
                return Ok(new { success = true, exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de imagen: {Path}", path);
                return BadRequest(new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene información del storage (uso, estadísticas)
        /// </summary>
        /// <returns>Información del storage</returns>
        [HttpGet("storage-info")]
        [RequirePermission("sistema.administrar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStorageInfo()
        {
            try
            {
                var totalImages = await _storageService.ListImagesAsync();
                
                return Ok(new { 
                    success = true, 
                    data = new {
                        totalImages = totalImages.Count,
                        bucketName = "product-images",
                        lastUpdated = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información del storage");
                return BadRequest(new { success = false, message = "Error interno del servidor" });
            }
        }
    }
}