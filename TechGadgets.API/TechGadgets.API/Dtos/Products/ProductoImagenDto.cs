using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductoImagenDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int? VarianteId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool EsPrincipal { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        
        // Información relacionada
        public string? ProductoNombre { get; set; }
        public string? VarianteNombre { get; set; }
    }
}