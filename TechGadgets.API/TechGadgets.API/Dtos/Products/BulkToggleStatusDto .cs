using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class BulkToggleStatusDto : BulkOperationDto
    {
        [Required]
        public bool Activo { get; set; }
    }
}