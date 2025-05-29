using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductTypeStatsDto
    {
        public string Tipo { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}