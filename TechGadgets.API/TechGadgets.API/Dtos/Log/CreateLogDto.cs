using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Log
{
    public class CreateLogDto
    {
        [Required]
        [StringLength(20)]
        public string Nivel { get; set; } = string.Empty;

        [Required]
        public string Mensaje { get; set; } = string.Empty;

        public string? Excepcion { get; set; }

        public int? UsuarioId { get; set; }

        [StringLength(45)]
        public string? DireccionIP { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(500)]
        public string? Url { get; set; }

        public DateTime? Fecha { get; set; }
    }
}