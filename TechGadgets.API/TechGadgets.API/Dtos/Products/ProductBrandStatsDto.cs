using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductBrandStatsDto
    {
        public int MarcaId { get; set; }
        public string MarcaNombre { get; set; } = string.Empty;
        public int TotalProductos { get; set; }
        public int ProductosActivos { get; set; }
    }
}