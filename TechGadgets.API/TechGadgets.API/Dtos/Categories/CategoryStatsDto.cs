using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Categories
{
    public class CategoryStatsDto
    {
        public int TotalCategorias { get; set; }
        public int CategoriasActivas { get; set; }
        public int CategoriasInactivas { get; set; }
        public int CategoriasRaiz { get; set; }
        public int CategoriasConHijos { get; set; }
        public int CategoriasConProductos { get; set; }
        public int CategoriasSinProductos { get; set; }
        public int NivelesMaximos { get; set; }
        public DateTime? UltimaCategoriaCreada { get; set; }
        public List<CategoryProductCountDto> TopCategoriasPorProductos { get; set; } = new();
    }
}