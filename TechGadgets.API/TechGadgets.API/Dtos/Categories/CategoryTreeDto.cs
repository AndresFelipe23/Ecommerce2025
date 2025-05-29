using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Categories
{
    public class CategoryTreeDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Imagen { get; set; }
        public string? Icono { get; set; }
        public string Slug { get; set; } = string.Empty;
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public int TotalProductos { get; set; }
        public List<CategoryTreeDto> Hijos { get; set; } = new();
        public int Nivel { get; set; }
    }
}