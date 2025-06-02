using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Configuration
{
    public class UpdateProductWithImagesRequest
    {
        [Required(ErrorMessage = "El SKU es requerido")]
        [StringLength(50, ErrorMessage = "El SKU no puede exceder 50 caracteres")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? DescripcionCorta { get; set; }

        public string? DescripcionLarga { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        public decimal? PrecioComparacion { get; set; }
        public decimal? Costo { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "La marca es requerida")]
        public int MarcaId { get; set; }

        [StringLength(20)]
        public string Tipo { get; set; } = "simple";

        [StringLength(20)]
        public string Estado { get; set; } = "borrador";

        public bool Destacado { get; set; }
        public bool Nuevo { get; set; }
        public bool EnOferta { get; set; }
        public decimal? Peso { get; set; }

        [StringLength(50)]
        public string? Dimensiones { get; set; }

        [StringLength(200)]
        public string? MetaTitulo { get; set; }

        [StringLength(500)]
        public string? MetaDescripcion { get; set; }

        [StringLength(500)]
        public string? PalabrasClaves { get; set; }

        public bool RequiereEnvio { get; set; } = true;
        public bool PermiteReseñas { get; set; } = true;

        [StringLength(100)]
        public string? Garantia { get; set; }

        public int Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;

        // Nuevos archivos de imagen para subir
        public IFormFileCollection? NewImageFiles { get; set; }

        // URLs externas adicionales
        public List<string>? NewExternalImageUrls { get; set; }
    }
}