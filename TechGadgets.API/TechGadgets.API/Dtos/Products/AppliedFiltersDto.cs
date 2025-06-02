using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class AppliedFiltersDto
    {
        public string? Estado { get; set; }
        public string? Categoria { get; set; }
        public string? Marca { get; set; }
        public string? RangoPrecios { get; set; }
        public string? TerminoBusqueda { get; set; }
        public string Ordenamiento { get; set; } = "Nombre";
        public bool OrdenDescendente { get; set; }
    }
}