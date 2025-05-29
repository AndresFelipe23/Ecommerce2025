using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Brands
{
    public class BrandProductCountDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int TotalProductos { get; set; }
    }
}