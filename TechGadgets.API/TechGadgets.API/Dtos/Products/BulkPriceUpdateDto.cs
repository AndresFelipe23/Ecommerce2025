using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class BulkPriceUpdateDto
    {
        [Required]
        public List<int> ProductIds { get; set; } = new();

        public decimal? NuevoPrecio { get; set; }
        public decimal? NuevoPrecioComparacion { get; set; }
        public decimal? PorcentajeIncremento { get; set; }
        public decimal? PorcentajeDescuento { get; set; }

        public string TipoOperacion { get; set; } = "precio"; // "precio", "comparacion", "incremento", "descuento"
    }
}