using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Log
{
    public class LogFilterDto
    {
        public string? Nivel { get; set; }
        public string? Mensaje { get; set; }
        public int? UsuarioId { get; set; }
        public string? DireccionIP { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Url { get; set; }
        
        // Paginación
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        
        // Ordenamiento
        public string? SortBy { get; set; } = "fecha";
        public bool SortDescending { get; set; } = true;
    }
}