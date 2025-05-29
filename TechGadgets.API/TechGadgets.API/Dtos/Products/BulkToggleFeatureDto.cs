using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class BulkToggleFeatureDto
    {
        [Required]
        public string Feature { get; set; } = string.Empty; // "destacado", "nuevo", "oferta"
        
        [Required]
        public bool Value { get; set; }
    }
}