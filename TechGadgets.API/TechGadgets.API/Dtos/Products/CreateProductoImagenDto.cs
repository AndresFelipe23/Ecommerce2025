using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class CreateProductoImagenDto
    {
        [Required]
        public int ProductoId { get; set; }

        public int? VarianteId { get; set; }
        
        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;
        
        public string? AltText { get; set; }
        
        public bool EsPrincipal { get; set; }
        
        public int Orden { get; set; }

        public bool Activo { get; set; } = true;
    }
}