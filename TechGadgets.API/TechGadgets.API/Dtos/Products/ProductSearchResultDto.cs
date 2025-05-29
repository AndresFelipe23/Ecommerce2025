using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductSearchResultDto
    {
        public List<ProductSummaryDto> Productos { get; set; } = new();
        public ProductSearchFiltersDto FiltrosDisponibles { get; set; } = new();
        public int TotalResultados { get; set; }
        public int Pagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}