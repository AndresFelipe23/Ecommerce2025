using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Configuration
{
    public class MultipleImageUploadResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ImageDataDto> SuccessfulUploads { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int TotalFiles { get; set; }
        public int SuccessfulCount { get; set; }
        public int ErrorCount { get; set; }
    }
}