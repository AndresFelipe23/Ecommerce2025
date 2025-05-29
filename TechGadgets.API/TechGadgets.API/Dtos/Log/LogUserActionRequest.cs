using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Log
{
    public class LogUserActionRequest
    {
        [Required]
        public string Accion { get; set; } = string.Empty;
        
        [Required]
        public int UsuarioId { get; set; }
        
        public string? Detalles { get; set; }
    }
}