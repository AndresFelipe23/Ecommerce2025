using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class BulkImagenOperationDto
    {
        [Required(ErrorMessage = "Los IDs de las imágenes son requeridos")]
        [MinLength(1, ErrorMessage = "Debe proporcionar al menos un ID")]
        public int[] ImagenIds { get; set; } = Array.Empty<int>();
    }
}