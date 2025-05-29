using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Log
{
    public class LogDto
    {
        public long Id { get; set; }
        public string Nivel { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string? Excepcion { get; set; }
        public int? UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
        public string? DireccionIP { get; set; }
        public string? UserAgent { get; set; }
        public string? Url { get; set; }
        public DateTime Fecha { get; set; }
    }
}