using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductoImagenFilterDto
    {
        public int? ProductoId { get; set; }
        public int? VarianteId { get; set; }
        public bool? EsPrincipal { get; set; }
        public bool? Activo { get; set; }
        public string? Url { get; set; }
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
        public string? OrderBy { get; set; } = "Orden";
        public bool? OrderDesc { get; set; } = false;
    }
}