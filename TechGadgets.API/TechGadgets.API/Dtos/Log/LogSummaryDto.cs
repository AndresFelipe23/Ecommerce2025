using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Log
{
    public class LogSummaryDto
    {
        public long Id { get; set; }
        public string Nivel { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string? UsuarioNombre { get; set; }
        public DateTime Fecha { get; set; }
    }
}