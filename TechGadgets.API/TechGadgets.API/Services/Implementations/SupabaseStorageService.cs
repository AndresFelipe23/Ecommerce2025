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
        /// Sube una imagen individual a Supabase Storage
        /// </summary>
        public async Task<ImageUploadResponseDto> UploadImageAsync(IFormFile file, string? altText = null, string? folder = null)
        {
            try
            {
                // ✅ VALIDACIONES BÁSICAS
                var validation = ValidateFile(file);
                if (!validation.IsValid)
                {
                    return new ImageUploadResponseDto
                    {
                        Success = false,
                        Error = validation.ErrorMessage
                    };
                }

                // ✅ GENERAR NOMBRE ÚNICO
                var fileName = GenerateUniqueFileName(file.FileName);
                var filePath = string.IsNullOrEmpty(folder) 
                    ? $"products/{fileName}" 
                    : $"products/{folder}/{fileName}";

                // ✅ PROCESAR IMAGEN (COMPRESIÓN)
                var processedImageData = await ProcessImageAsync(file);

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
                    return new ImageUploadResponseDto
                    {
                        Success = false,
                        Error = "Error al subir imagen a Supabase"
                    };
                }

                // ✅ OBTENER URL PÚBLICA
                var publicUrl = _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .GetPublicUrl(filePath);

                // ✅ GENERAR THUMBNAILS SI ESTÁ HABILITADO
                List<ThumbnailDto>? thumbnails = null;
                if (_settings.Storage.GenerateThumbnails)
                {
                    thumbnails = await GenerateThumbnailsAsync(file, filePath);
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
                        Size = file.Length,
                        ContentType = file.ContentType,
                        AltText = altText,
                        Thumbnails = thumbnails
                    }
                };

                _logger.LogInformation("Imagen subida exitosamente: {FileName} -> {Path}", file.FileName, filePath);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen: {FileName}", file.FileName);
                return new ImageUploadResponseDto
                {
                    Success = false,
                    Error = $"Error interno: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Sube múltiples imágenes
        /// </summary>
        public async Task<MultipleImageUploadResponseDto> UploadMultipleImagesAsync(IFormFileCollection files, string? folder = null)
        {
            var result = new MultipleImageUploadResponseDto
            {
                TotalFiles = files.Count
            };

            var uploadTasks = files.Select(async file =>
            {
                var uploadResult = await UploadImageAsync(file, null, folder);
                
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
            });

            await Task.WhenAll(uploadTasks);

            result.Success = result.SuccessfulCount > 0;
            result.Message = $"{result.SuccessfulCount} de {result.TotalFiles} imágenes subidas exitosamente";

            return result;
        }

        /// <summary>
        /// Elimina una imagen de Supabase Storage
        /// </summary>
        public async Task<bool> DeleteImageAsync(string path)
        {
            try
            {
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

                    _logger.LogInformation("Imagen eliminada: {Path}", path);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen: {Path}", path);
                return false;
            }
        }

        /// <summary>
        /// Elimina múltiples imágenes
        /// </summary>
        public async Task<bool> DeleteMultipleImagesAsync(List<string> paths)
        {
            try
            {
                var deleteResult = await _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .Remove(paths);

                // Eliminar thumbnails
                if (_settings.Storage.GenerateThumbnails)
                {
                    foreach (var path in paths)
                    {
                        await DeleteThumbnailsAsync(path);
                    }
                }

                _logger.LogInformation("Imágenes eliminadas: {Count}", paths.Count);
                return deleteResult?.Count == paths.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar múltiples imágenes");
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
                return _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .GetPublicUrl(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener URL pública: {Path}", path);
                return string.Empty;
            }
        }

        /// <summary>
        /// Lista imágenes en una carpeta
        /// </summary>
        public async Task<List<string>> ListImagesAsync(string? folder = null)
        {
            try
            {
                var path = string.IsNullOrEmpty(folder) ? "products" : $"products/{folder}";
                
                var files = await _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .List(path);

                return files?.Select(f => f.Name).ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar imágenes en folder: {Folder}", folder);
                return new List<string>();
            }
        }

        /// <summary>
        /// Verifica si una imagen existe
        /// </summary>
        public async Task<bool> ImageExistsAsync(string path)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                var fileName = Path.GetFileName(path);
                
                var files = await _supabaseClient.Storage
                    .From(_settings.Storage.BucketName)
                    .List(directory ?? string.Empty);

                return files?.Any(f => f.Name == fileName) ?? false;
            }
            catch
            {
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
                return (false, "No se recibió ningún archivo");

            if (file.Length > _settings.Storage.MaxFileSize)
                return (false, $"El archivo es demasiado grande. Máximo {_settings.Storage.MaxFileSize / 1024 / 1024}MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_settings.Storage.AllowedExtensions.Contains(extension))
                return (false, $"Tipo de archivo no permitido. Permitidos: {string.Join(", ", _settings.Storage.AllowedExtensions)}");

            if (!file.ContentType.StartsWith("image/"))
                return (false, "El archivo debe ser una imagen");

            return (true, string.Empty);
        }

        /// <summary>
        /// Genera un nombre único para el archivo
        /// </summary>
        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var guid = Guid.NewGuid().ToString("N")[..8];
            
            return $"{timestamp}-{guid}{extension}";
        }

        /// <summary>
        /// Procesa la imagen (compresión, redimensionamiento)
        /// </summary>
        private async Task<byte[]> ProcessImageAsync(IFormFile file)
        {
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
            }

            // Comprimir y guardar
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
                    using var thumbnail = image.CloneAs<Rgba32>();
                    thumbnail.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(size, size),
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
                            Width = size,
                            Height = size
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error generando thumbnails para: {Path}", originalPath);
            }

            return thumbnails;
        }

        /// <summary>
        /// Genera la ruta para un thumbnail
        /// </summary>
        private string GenerateThumbnailPath(string originalPath, int size)
        {
            var directory = Path.GetDirectoryName(originalPath);
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error eliminando thumbnails para: {Path}", originalPath);
            }
        }

        #endregion
     
    }
}