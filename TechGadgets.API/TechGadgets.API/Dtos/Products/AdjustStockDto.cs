using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class AdjustStockDto
    {
        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(-9999, 9999, ErrorMessage = "La cantidad debe estar entre -9999 y 9999")]
        public int Cantidad { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "El motivo no puede exceder 200 caracteres")]
        public string Motivo { get; set; } = string.Empty;

        public string? Observaciones { get; set; }
    }
}