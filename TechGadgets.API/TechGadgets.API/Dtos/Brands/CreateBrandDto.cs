using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Brands
{
    public class CreateBrandDto
    {
        [Required(ErrorMessage = "El nombre de la marca es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [Url(ErrorMessage = "El logo debe ser una URL válida")]
        [StringLength(255, ErrorMessage = "La URL del logo no puede exceder 255 caracteres")]
        public string? Logo { get; set; }

        [Url(ErrorMessage = "El sitio web debe ser una URL válida")]
        [StringLength(255, ErrorMessage = "La URL del sitio web no puede exceder 255 caracteres")]
        public string? SitioWeb { get; set; }
    }
}