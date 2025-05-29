using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Log
{
    public class DeleteOldLogsRequest
    {
        [Required]
        [Range(7, 365)]
        public int DiasAConservar { get; set; } = 30;
    }
}