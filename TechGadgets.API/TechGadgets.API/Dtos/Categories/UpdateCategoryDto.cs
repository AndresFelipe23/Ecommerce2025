using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Categories
{
    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "El nombre de la categoría es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        public int? CategoriaPadreId { get; set; }

        [Url(ErrorMessage = "La imagen debe ser una URL válida")]
        [StringLength(255, ErrorMessage = "La URL de la imagen no puede exceder 255 caracteres")]
        public string? Imagen { get; set; }

        [StringLength(50, ErrorMessage = "El ícono no puede exceder 50 caracteres")]
        public string? Icono { get; set; }

        [StringLength(100, ErrorMessage = "El slug no puede exceder 100 caracteres")]
        public string? Slug { get; set; }

        [Range(0, 999, ErrorMessage = "El orden debe estar entre 0 y 999")]
        public int Orden { get; set; } = 0;

        public bool Activo { get; set; } = true;
    }
}