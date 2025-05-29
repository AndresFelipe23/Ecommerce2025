using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductSummaryDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal? PrecioOferta { get; set; }
        public string? ImagenPrincipal { get; set; }
        public bool Activo { get; set; }
        public bool Destacado { get; set; }
        public int Stock { get; set; }
        public string MarcaNombre { get; set; } = string.Empty;
        public string CategoriaNombre { get; set; } = string.Empty;
        public decimal PrecioFinal => PrecioOferta ?? Precio;
        public bool EnOferta => PrecioOferta.HasValue && PrecioOferta < Precio;
    }
}