using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class CategoryFilterOption
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}