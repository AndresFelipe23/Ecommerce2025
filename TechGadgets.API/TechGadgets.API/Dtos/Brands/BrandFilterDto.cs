using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Brands
{
    public class BrandFilterDto
    {
        public string? Nombre { get; set; }
        public bool? Activo { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "Nombre";
        public bool SortDescending { get; set; } = false;
    }
}