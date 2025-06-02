using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Supabase;
using TechGadgets.API.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Services.Implementations
{
    public class SupabaseStorageService : ISupabaseStorageService
    {
       private readonly Client _supabaseClient;
        private readonly SupabaseSettings _settings;
        private readonly ILogger<SupabaseStorageService> _logger;

        public SupabaseStorageService(
            IOptions<SupabaseSettings> settings,
            ILogger<SupabaseStorageService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };

            _supabaseClient = new Client(_settings.Url, _settings.ServiceKey, options);
        }

        /// <summary>
        /// Sube una imagen individual a Supabase Storage con carpeta específica para el producto
        /// </summary>
        public async Task<ImageUploadResponseDto> UploadImageAsync(IFormFile file, string? altText = null, string? folder = null)
        {
            try
            {
                _logger.LogInformation("🔄 Iniciando subida de imagen: {FileName}", file.FileName);

                // ✅ VALIDACIONES BÁSICAS
                var validation = ValidateFile(file);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("❌ Validación fallida para {FileName}: {Error}", file.FileName, validation.ErrorMessage);
                    return new ImageUploadResponseDto
                    {
                        Success = false,
                        Error = validation.ErrorMessage
                    };
                }

                // ✅ GENERAR NOMBRE ÚNICO Y RUTA
                var fileName = GenerateUniqueFileName(file.FileName);
                var filePath = GenerateFilePath(fileName, folder);

                _logger.LogInformation("📁 Ruta generada: {FilePath}", filePath);

                // ✅ PROCESAR IMAGEN (COMPRESIÓN Y REDIMENSIONAMIENTO)
                var processedImageData = await ProcessImageAsync(file);
                _logger.LogInformation("🖼️ Imagen procesada. Tamaño original: {OriginalSize} bytes, Tamaño procesado: {ProcessedSize} bytes", 
                    file.Length, processedImageData.Length);

                // ✅ SUBIR A SUPABASE
                var uploadResult = await _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .Upload(processedImageData, filePath, new Supabase.Storage.FileOptions
                    {
                        CacheControl = "3600",
                        Upsert = false
                    });

                if (uploadResult == null)
                {
                    _logger.LogError("❌ Error al subir imagen a Supabase: {FileName}", file.FileName);
                    return new ImageUploadResponseDto
                    {
                        Success = false,
                        Error = "Error al subir imagen a Supabase Storage"
                    };
                }

                // ✅ OBTENER URL PÚBLICA
                var publicUrl = _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .GetPublicUrl(filePath);

                _logger.LogInformation("🌐 URL pública generada: {PublicUrl}", publicUrl);

                // ✅ GENERAR THUMBNAILS SI ESTÁ HABILITADO
                List<ThumbnailDto>? thumbnails = null;
                if (_settings.Storage.GenerateThumbnails)
                {
                    _logger.LogInformation("📸 Generando thumbnails...");
                    thumbnails = await GenerateThumbnailsAsync(file, filePath);
                    _logger.LogInformation("✅ Thumbnails generados: {Count}", thumbnails?.Count ?? 0);
                }

                var result = new ImageUploadResponseDto
                {
                    Success = true,
                    Message = "Imagen subida exitosamente",
                    Data = new ImageDataDto
                    {
                        Url = publicUrl,
                        Path = filePath,
                        FileName = fileName,
                        OriginalName = file.FileName,
                        Size = processedImageData.Length, // Tamaño después del procesamiento
                        ContentType = file.ContentType,
                        AltText = altText ?? $"Imagen de producto - {Path.GetFileNameWithoutExtension(file.FileName)}",
                        Thumbnails = thumbnails
                    }
                };

                _logger.LogInformation("✅ Imagen subida exitosamente: {FileName} -> {Path}", file.FileName, filePath);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error crítico al subir imagen: {FileName}", file.FileName);
                return new ImageUploadResponseDto
                {
                    Success = false,
                    Error = $"Error interno al subir imagen: {ex.Message}"
                };
            }
        }

       /// <summary>
        /// Sube múltiples imágenes de forma paralela para mejor rendimiento
        /// </summary>
        public async Task<MultipleImageUploadResponseDto> UploadMultipleImagesAsync(IFormFileCollection files, string? folder = null)
        {
            var result = new MultipleImageUploadResponseDto
            {
                TotalFiles = files.Count
            };

            if (files.Count == 0)
            {
                result.Success = false;
                result.Message = "No se recibieron archivos para subir";
                return result;
            }

            _logger.LogInformation("🔄 Iniciando subida múltiple de {Count} archivos", files.Count);

            // ✅ PROCESAR EN PARALELO CON LÍMITE DE CONCURRENCIA
            var semaphore = new SemaphoreSlim(3, 3); // Máximo 3 subidas simultáneas
            var uploadTasks = files.Select(async file =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var uploadResult = await UploadImageAsync(file, null, folder);
                    
                    lock (result) // Thread-safe para modificar el resultado
                    {
                        if (uploadResult.Success && uploadResult.Data != null)
                        {
                            result.SuccessfulUploads.Add(uploadResult.Data);
                            result.SuccessfulCount++;
                        }
                        else
                        {
                            result.Errors.Add($"{file.FileName}: {uploadResult.Error}");
                            result.ErrorCount++;
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(uploadTasks);

            result.Success = result.SuccessfulCount > 0;
            result.Message = $"{result.SuccessfulCount} de {result.TotalFiles} imágenes subidas exitosamente";

            if (result.ErrorCount > 0)
            {
                result.Message += $". {result.ErrorCount} errores encontrados.";
            }

            _logger.LogInformation("✅ Subida múltiple completada: {Success}/{Total} exitosas", 
                result.SuccessfulCount, result.TotalFiles);

            return result;
        }

        /// <summary>
        /// Elimina una imagen y sus thumbnails de Supabase Storage
        /// </summary>
        public async Task<bool> DeleteImageAsync(string path)
        {
            try
            {
                _logger.LogInformation("🗑️ Eliminando imagen: {Path}", path);

                var deleteResult = await _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .Remove(new List<string> { path });

                if (deleteResult?.Count > 0)
                {
                    // También eliminar thumbnails si existen
                    if (_settings.Storage.GenerateThumbnails)
                    {
                        await DeleteThumbnailsAsync(path);
                    }

                    _logger.LogInformation("✅ Imagen eliminada exitosamente: {Path}", path);
                    return true;
                }

                _logger.LogWarning("⚠️ No se pudo eliminar la imagen: {Path}", path);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar imagen: {Path}", path);
                return false;
            }
        }

        
        /// <summary>
        /// Elimina múltiples imágenes de forma eficiente
        /// </summary>
        public async Task<bool> DeleteMultipleImagesAsync(List<string> paths)
        {
            try
            {
                if (!paths.Any())
                {
                    _logger.LogWarning("⚠️ Lista de rutas vacía para eliminación múltiple");
                    return false;
                }

                _logger.LogInformation("🗑️ Eliminando {Count} imágenes", paths.Count);

                var deleteResult = await _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .Remove(paths);

                // Eliminar thumbnails en paralelo
                if (_settings.Storage.GenerateThumbnails)
                {
                    var deleteThumbnailTasks = paths.Select(DeleteThumbnailsAsync);
                    await Task.WhenAll(deleteThumbnailTasks);
                }

                var success = deleteResult?.Count == paths.Count;
                
                if (success)
                {
                    _logger.LogInformation("✅ {Count} imágenes eliminadas exitosamente", paths.Count);
                }
                else
                {
                    _logger.LogWarning("⚠️ Solo se eliminaron {DeletedCount} de {TotalCount} imágenes", 
                        deleteResult?.Count ?? 0, paths.Count);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar múltiples imágenes");
                return false;
            }
        }

        /// <summary>
        /// Obtiene la URL pública de una imagen
        /// </summary>
        public async Task<string> GetPublicUrlAsync(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.LogWarning("⚠️ Ruta vacía para obtener URL pública");
                    return string.Empty;
                }

                var url = _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .GetPublicUrl(path);

                return url ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener URL pública: {Path}", path);
                return string.Empty;
            }
        }

        /// <summary>
        /// Lista imágenes en una carpeta específica
        /// </summary>
        public async Task<List<string>> ListImagesAsync(string? folder = null)
        {
            try
            {
                var path = string.IsNullOrEmpty(folder) ? "products" : $"products/{folder}";
                
                _logger.LogInformation("📂 Listando imágenes en: {Path}", path);

                var files = await _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .List(path);

                var imageNames = files?.Where(f => f.Name != null)
                                     .Select(f => f.Name!)
                                     .Where(name => !name.Contains("/thumbnails/")) // Excluir thumbnails del listado
                                     .ToList() ?? new List<string>();

                _logger.LogInformation("📋 Se encontraron {Count} imágenes en {Path}", imageNames.Count, path);
                return imageNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al listar imágenes en folder: {Folder}", folder);
                return new List<string>();
            }
        }

        /// <summary>
        /// Verifica si una imagen existe en el storage
        /// </summary>
        public async Task<bool> ImageExistsAsync(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                var directory = Path.GetDirectoryName(path)?.Replace("\\", "/") ?? string.Empty;
                var fileName = Path.GetFileName(path);
                
                var files = await _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .List(directory);

                return files?.Any(f => f.Name == fileName) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar existencia de imagen: {Path}", path);
                return false;
            }
        }

        #region Private Methods

        /// <summary>
        /// Valida el archivo subido
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "No se recibió ningún archivo o el archivo está vacío");

            if (file.Length > _settings.Storage.MaxFileSize)
            {
                var maxSizeMB = _settings.Storage.MaxFileSize / 1024.0 / 1024.0;
                return (false, $"El archivo es demasiado grande. Máximo permitido: {maxSizeMB:F1}MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_settings.Storage.AllowedExtensions.Contains(extension))
                return (false, $"Tipo de archivo no permitido. Extensiones permitidas: {string.Join(", ", _settings.Storage.AllowedExtensions)}");

            if (!file.ContentType.StartsWith("image/"))
                return (false, "El archivo debe ser una imagen válida");

            return (true, string.Empty);
        }

        /// <summary>
        /// Genera un nombre único para el archivo
        /// </summary>
        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var guid = Guid.NewGuid().ToString("N")[..8]; // 8 caracteres del GUID
            var cleanName = Path.GetFileNameWithoutExtension(originalFileName)
                .Replace(" ", "-")
                .Replace("_", "-")
                .ToLowerInvariant();
            
            return $"{timestamp}-{guid}-{cleanName}{extension}";
        }

        /// <summary>
        /// Genera la ruta completa del archivo
        /// </summary>
        private string GenerateFilePath(string fileName, string? folder = null)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return $"products/{fileName}";
            }
            
            // Limpiar el nombre de la carpeta
            var cleanFolder = folder.Replace(" ", "-").Replace("_", "-").ToLowerInvariant();
            return $"products/{cleanFolder}/{fileName}";
        }

        /// <summary>
        /// Procesa la imagen (compresión, redimensionamiento)
        /// </summary>
        private async Task<byte[]> ProcessImageAsync(IFormFile file)
        {
            // Si no está habilitada la compresión, retornar archivo original
            if (!_settings.Storage.CompressImages)
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                return stream.ToArray();
            }

            using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);
            using var outputStream = new MemoryStream();

            // Redimensionar si es necesario
            if (image.Width > _settings.Storage.MaxImageWidth || image.Height > _settings.Storage.MaxImageHeight)
            {
                var ratio = Math.Min(
                    (double)_settings.Storage.MaxImageWidth / image.Width,
                    (double)_settings.Storage.MaxImageHeight / image.Height
                );

                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                image.Mutate(x => x.Resize(newWidth, newHeight));
                
                _logger.LogInformation("🔄 Imagen redimensionada de {OriginalW}x{OriginalH} a {NewW}x{NewH}", 
                    (int)(newWidth / ratio), (int)(newHeight / ratio), newWidth, newHeight);
            }

            // Comprimir y guardar como JPEG para mejor compatibilidad
            var encoder = new JpegEncoder { Quality = _settings.Storage.ImageQuality };
            await image.SaveAsync(outputStream, encoder);

            return outputStream.ToArray();
        }


        /// <summary>
        /// Genera thumbnails de diferentes tamaños
        /// </summary>
        private async Task<List<ThumbnailDto>> GenerateThumbnailsAsync(IFormFile originalFile, string originalPath)
        {
            var thumbnails = new List<ThumbnailDto>();

            try
            {
                using var inputStream = originalFile.OpenReadStream();
                using var image = await Image.LoadAsync(inputStream);

                foreach (var size in _settings.Storage.ThumbnailSizes)
                {
                    try
                    {
                        using var thumbnail = image.CloneAs<Rgba32>();
                        thumbnail.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(size, size),
                            Mode = ResizeMode.Max, // Mantener proporción
                            Sampler = KnownResamplers.Lanczos3
                        }));

                        using var outputStream = new MemoryStream();
                        var encoder = new JpegEncoder { Quality = _settings.Storage.ImageQuality };
                        await thumbnail.SaveAsync(outputStream, encoder);

                        var thumbnailPath = GenerateThumbnailPath(originalPath, size);
                        
                        // Subir thumbnail
                        var uploadResult = await _supabaseClient.Storage
                            .From(_settings.Storage.BucketName)
                            .Upload(outputStream.ToArray(), thumbnailPath, new Supabase.Storage.FileOptions
                            {
                                CacheControl = "3600",
                                Upsert = false
                            });

                        if (uploadResult != null)
                        {
                            var thumbnailUrl = _supabaseClient.Storage
                                .From(_settings.Storage.BucketName)
                                .GetPublicUrl(thumbnailPath);

                            thumbnails.Add(new ThumbnailDto
                            {
                                Url = thumbnailUrl,
                                Path = thumbnailPath,
                                Width = thumbnail.Width,
                                Height = thumbnail.Height,
                                Size = outputStream.Length
                            });

                            _logger.LogDebug("📸 Thumbnail {Size}x{Size} creado: {Path}", size, size, thumbnailPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Error generando thumbnail de tamaño {Size} para: {Path}", size, originalPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error crítico generando thumbnails para: {Path}", originalPath);
            }

            return thumbnails;
        }

        /// <summary>
        /// Genera la ruta para un thumbnail
        /// </summary>
        private string GenerateThumbnailPath(string originalPath, int size)
        {
            var directory = Path.GetDirectoryName(originalPath)?.Replace("\\", "/");
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath);
            var extension = Path.GetExtension(originalPath);

            return $"{directory}/thumbnails/{fileNameWithoutExtension}_{size}x{size}{extension}";
        }

        /// <summary>
        /// Elimina los thumbnails de una imagen
        /// </summary>
        private async Task DeleteThumbnailsAsync(string originalPath)
        {
            try
            {
                var thumbnailPaths = _settings.Storage.ThumbnailSizes
                    .Select(size => GenerateThumbnailPath(originalPath, size))
                    .ToList();

                if (thumbnailPaths.Any())
                {
                    await _supabaseClient.Storage
                        .From(_settings.Storage.BucketName)
                        .Remove(thumbnailPaths);

                    _logger.LogDebug("🗑️ Thumbnails eliminados para: {Path}", originalPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error eliminando thumbnails para: {Path}", originalPath);
            }
        }

        #endregion
     
    }
}