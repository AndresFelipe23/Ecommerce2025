using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "El SKU es requerido")]
        [StringLength(50, ErrorMessage = "El SKU no puede exceder 50 caracteres")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del producto es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción corta no puede exceder 500 caracteres")]
        public string? DescripcionCorta { get; set; }

        public string? DescripcionLarga { get; set; }

        [StringLength(200, ErrorMessage = "El slug no puede exceder 200 caracteres")]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
        public decimal Precio { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "El precio de comparación debe estar entre 0.01 y 999,999.99")]
        public decimal? PrecioComparacion { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "El costo debe estar entre 0.01 y 999,999.99")]
        public decimal? Costo { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "La marca es requerida")]
        public int MarcaId { get; set; }

        [StringLength(20, ErrorMessage = "El tipo no puede exceder 20 caracteres")]
        public string? Tipo { get; set; } = "simple";

        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string? Estado { get; set; } = "disponible";

        public bool Destacado { get; set; } = false;
        public bool Nuevo { get; set; } = true;
        public bool EnOferta { get; set; } = false;

        [Range(0.01, 1000, ErrorMessage = "El peso debe estar entre 0.01 y 1000 kg")]
        public decimal? Peso { get; set; }

        [StringLength(50, ErrorMessage = "Las dimensiones no pueden exceder 50 caracteres")]
        public string? Dimensiones { get; set; }

        [StringLength(200, ErrorMessage = "El meta título no puede exceder 200 caracteres")]
        public string? MetaTitulo { get; set; }

        [StringLength(500, ErrorMessage = "La meta descripción no puede exceder 500 caracteres")]
        public string? MetaDescripcion { get; set; }

        [StringLength(500, ErrorMessage = "Las palabras claves no pueden exceder 500 caracteres")]
        public string? PalabrasClaves { get; set; }

        public bool RequiereEnvio { get; set; } = true;
        public bool PermiteReseñas { get; set; } = true;

        [StringLength(100, ErrorMessage = "La garantía no puede exceder 100 caracteres")]
        public string? Garantia { get; set; }

        [Range(0, 999, ErrorMessage = "El orden debe estar entre 0 y 999")]
        public int Orden { get; set; } = 0;

        // Imágenes
        public List<CreateProductImageDto> Imagenes { get; set; } = new();

        // Stock inicial
        [Range(0, int.MaxValue, ErrorMessage = "El stock inicial no puede ser negativo")]
        public int StockInicial { get; set; } = 0;

        // Validación personalizada
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PrecioComparacion.HasValue && PrecioComparacion >= Precio)
            {
                yield return new ValidationResult(
                    "El precio de comparación debe ser menor al precio regular",
                    new[] { nameof(PrecioComparacion) });
            }

            if (Costo.HasValue && Costo >= Precio)
            {
                yield return new ValidationResult(
                    "El costo debe ser menor al precio de venta",
                    new[] { nameof(Costo) });
            }

            if (Imagenes.Count(i => i.EsPrincipal) > 1)
            {
                yield return new ValidationResult(
                    "Solo puede haber una imagen principal",
                    new[] { nameof(Imagenes) });
            }
        }
    }
}