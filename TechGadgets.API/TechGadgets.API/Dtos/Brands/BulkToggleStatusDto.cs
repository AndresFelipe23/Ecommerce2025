using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Brands
{
    public class BulkToggleBrandStatusDto
    {
        [Required]
        public List<int> BrandIds { get; set; } = new();
        
        [Required]
        public bool Active { get; set; }
    }
}