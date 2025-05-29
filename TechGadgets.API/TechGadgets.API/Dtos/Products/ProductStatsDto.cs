using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechGadgets.API.Dtos.Brands;
using TechGadgets.API.Dtos.Categories;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductStatsDto
    {
        public int TotalProductos { get; set; }
        public int ProductosActivos { get; set; }
        public int ProductosInactivos { get; set; }
        public int ProductosDestacados { get; set; }
        public int ProductosEnOferta { get; set; }
        public int ProductosBajoStock { get; set; }
        public int ProductosSinStock { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public decimal PrecioPromedio { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public DateTime? UltimoProductoCreado { get; set; }
        public List<ProductSalesStatsDto> TopProductosVendidos { get; set; } = new();
        public List<CategoryProductCountDto> ProductosPorCategoria { get; set; } = new();
        public List<BrandProductCountDto> ProductosPorMarca { get; set; } = new();
    }
}
