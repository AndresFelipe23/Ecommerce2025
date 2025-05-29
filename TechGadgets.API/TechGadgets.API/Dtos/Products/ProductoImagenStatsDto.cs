using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductoImagenStatsDto
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int TotalImagenes { get; set; }
        public int ImagenesActivas { get; set; }
        public int ImagenesInactivas { get; set; }
        public bool TieneImagenPrincipal { get; set; }
        public ProductoImagenDto? ImagenPrincipal { get; set; }
        public IEnumerable<ProductoImagenDto> ImagenesVariantes { get; set; } = new List<ProductoImagenDto>();
    }
}