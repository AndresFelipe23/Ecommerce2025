using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Configuration
{
    public class ImageUploadResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public ImageDataDto? Data { get; set; }
        public string? Error { get; set; }
    }
}