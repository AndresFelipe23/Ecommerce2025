using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductStatusStatsDto
    {
        public string Estado { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}