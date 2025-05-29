using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class UpdateProductoImagenDto
    {
        public int? Id { get; set; }
    
        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;
        
        public string? AltText { get; set; }
        
        public bool EsPrincipal { get; set; }
        
        public int Orden { get; set; }
        
        public bool Eliminar { get; set; }

        public bool Activo { get; set; } = true;
    }
}