using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class UpdateOrdenImagenDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(0, 99)]
        public int Orden { get; set; }
    }
}