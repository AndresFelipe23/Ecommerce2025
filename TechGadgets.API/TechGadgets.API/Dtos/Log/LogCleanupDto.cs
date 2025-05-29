using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Log
{
    public class LogCleanupDto
    {
        [Required]
        public DateTime FechaLimite { get; set; }
        
        public List<string>? NivelesAEliminar { get; set; }
        
        public bool EliminarTodos { get; set; } = false;
    }
}