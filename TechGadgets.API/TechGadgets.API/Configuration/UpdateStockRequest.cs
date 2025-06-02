using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Configuration
{
    public class UpdateStockRequest
    {
        [Required(ErrorMessage = "El nuevo stock es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0")]
        public int NuevoStock { get; set; }

        [StringLength(255, ErrorMessage = "El motivo no puede exceder 255 caracteres")]
        public string? Motivo { get; set; }
    }
}