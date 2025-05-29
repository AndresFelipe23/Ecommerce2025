using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class SetImagenPrincipalDto
    {
        [Required(ErrorMessage = "El ID del producto es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser mayor que 0")]
        public int ProductoId { get; set; }
        
        [Required(ErrorMessage = "El ID de la imagen es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la imagen debe ser mayor que 0")]
        public int ImagenId { get; set; }
    }
}