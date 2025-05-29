using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Categories
{
    public class BulkCategoryOperationDto
    {
        [Required]
        public List<int> CategoryIds { get; set; } = new();
    }
}