using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechGadgets.API.Configuration;

namespace TechGadgets.API.Services.Interfaces
{
    public interface ISupabaseStorageService
    {
        Task<ImageUploadResponseDto> UploadImageAsync(IFormFile file, string? altText = null, string? folder = null);
        Task<MultipleImageUploadResponseDto> UploadMultipleImagesAsync(IFormFileCollection files, string? folder = null);
        Task<bool> DeleteImageAsync(string path);
        Task<bool> DeleteMultipleImagesAsync(List<string> paths);
        Task<string> GetPublicUrlAsync(string path);
        Task<List<string>> ListImagesAsync(string? folder = null);
        Task<bool> ImageExistsAsync(string path);
    }
}