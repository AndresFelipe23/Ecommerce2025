using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductFilterDto
    {
        public string? Busqueda { get; set; }
        public int? CategoriaId { get; set; }
        public int? MarcaId { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public bool? EnOferta { get; set; }
        public bool? Destacado { get; set; }
        public bool? Activo { get; set; }
        public bool? BajoStock { get; set; }
        public bool? SinStock { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? SKU { get; set; }
        
        // Paginación y ordenamiento
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string? SortBy { get; set; } = "Nombre";
        public bool SortDescending { get; set; } = false;
        
        // Opciones de inclusión
        public bool IncluirImagenes { get; set; } = true;
        public bool IncluirInactivos { get; set; } = false;
    }
}