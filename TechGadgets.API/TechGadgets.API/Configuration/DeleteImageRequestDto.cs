using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Configuration
{
    public class DeleteImageRequestDto
    {
        public string Path { get; set; } = string.Empty;
        public bool DeleteThumbnails { get; set; } = true;
    }
}