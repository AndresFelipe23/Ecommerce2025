using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Categories
{
    public class CategoryProductCountDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int TotalProductos { get; set; }
        public int ProductosDirectos { get; set; }
        public int ProductosDeSubcategorias { get; set; }
    }
}