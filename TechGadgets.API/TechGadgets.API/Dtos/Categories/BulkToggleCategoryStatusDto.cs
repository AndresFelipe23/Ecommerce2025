using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Categories
{
    public class BulkToggleCategoryStatusDto : BulkCategoryOperationDto
    {
        [Required]
        public bool Active { get; set; }
    }
}