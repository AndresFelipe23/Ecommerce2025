using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Brands
{
    public class BrandStatsDto
    {
        public int TotalMarcas { get; set; }
        public int MarcasActivas { get; set; }
        public int MarcasInactivas { get; set; }
        public int MarcasConProductos { get; set; }
        public int MarcasSinProductos { get; set; }
        public DateTime? UltimaMarcaCreada { get; set; }
        public List<BrandProductCountDto> TopMarcasPorProductos { get; set; } = new();
    }
}