using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Categories
{
    public class CategoryFilterDto
    {
        public string? Nombre { get; set; }
        public int? CategoriaPadreId { get; set; }
        public bool? Activo { get; set; }
        public bool? SoloRaiz { get; set; } // Solo categorías principales (sin padre)
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "Orden";
        public bool SortDescending { get; set; } = false;
        public bool IncluirSubcategorias { get; set; } = false;
    }
}