using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Categories
{
    public class MoveCategoryDto
    {
        [Required]
        public int CategoryId { get; set; }
        
        public int? NuevoPadreId { get; set; } // null para mover a raíz
        
        [Range(0, 999)]
        public int NuevoOrden { get; set; } = 0;
    }
}