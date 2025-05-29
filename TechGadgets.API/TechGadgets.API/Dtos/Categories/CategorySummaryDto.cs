using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Categories
{
    public class CategorySummaryDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Icono { get; set; }
        public string Slug { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string RutaCompleta { get; set; } = string.Empty;
    }
}