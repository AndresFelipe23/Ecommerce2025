using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Log
{
    public class LogSecurityEventRequest
    {
        [Required]
        public string TipoEvento { get; set; } = string.Empty;
        
        [Required]
        public string Descripcion { get; set; } = string.Empty;
        
        public int? UsuarioId { get; set; }
    }
}