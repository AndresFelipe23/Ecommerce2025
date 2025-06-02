using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class UpdateImageOrderDto
    {
        [Required(ErrorMessage = "El ID de la imagen es requerido")]
    public int ImageId { get; set; }
    
    [Required(ErrorMessage = "El nuevo orden es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser mayor a 0")]
    public int NewOrder { get; set; }
    }
}