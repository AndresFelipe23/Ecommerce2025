using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class UpdateProductImageDto
    {
        public int? Id { get; set; } // null para nuevas imágenes

        [Required(ErrorMessage = "La URL de la imagen es requerida")]
        [Url(ErrorMessage = "Debe ser una URL válida")]
        public string Url { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "El texto alternativo no puede exceder 200 caracteres")]
        public string? AltText { get; set; }

        public bool EsPrincipal { get; set; } = false;

        [Range(0, 99, ErrorMessage = "El orden debe estar entre 0 y 99")]
        public int Orden { get; set; } = 0;

        public bool Eliminar { get; set; } = false; // Para marcar imágenes a eliminar
    }
}