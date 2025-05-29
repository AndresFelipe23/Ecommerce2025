using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Configuration
{
    public class SupabaseStorageSettings
    {
        public string BucketName { get; set; } = "product-images";
        public long MaxFileSize { get; set; } = 5242880; // 5MB
        public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp" };
        public bool CompressImages { get; set; } = true;
        public bool GenerateThumbnails { get; set; } = true;
        public int[] ThumbnailSizes { get; set; } = { 150, 300, 600 };
        public int ImageQuality { get; set; } = 85;
        public int MaxImageWidth { get; set; } = 1920;
        public int MaxImageHeight { get; set; } = 1920;
    }
}