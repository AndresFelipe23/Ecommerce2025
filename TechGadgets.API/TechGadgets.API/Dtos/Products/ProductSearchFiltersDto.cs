using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductSearchFiltersDto
    {
        public List<CategoryFilterOption> Categorias { get; set; } = new();
        public List<BrandFilterOption> Marcas { get; set; } = new();
        public List<TypeFilterOption> Tipos { get; set; } = new();
        public List<StatusFilterOption> Estados { get; set; } = new();
        public PriceRangeDto RangoPrecios { get; set; } = new();
    }
}