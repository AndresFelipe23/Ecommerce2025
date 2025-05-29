using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class PriceRangeDto
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public decimal? PrecioOferta { get; set; }

        [Required(ErrorMessage = "El stock es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Range(0, 1000, ErrorMessage = "El stock mínimo debe estar entre 0 y 1000")]
        public int StockMinimo { get; set; } = 5;

        [Range(0.01, 1000, ErrorMessage = "El peso debe estar entre 0.01 y 1000 kg")]
        public decimal? Peso { get; set; }

        [StringLength(100, ErrorMessage = "Las dimensiones no pueden exceder 100 caracteres")]
        public string? Dimensiones { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "La marca es requerida")]
        public int MarcaId { get; set; }

        public bool Activo { get; set; } = true;
        public bool Destacado { get; set; } = false;

        // Imágenes - se manejan por separado para mejor control
        public List<UpdateProductImageDto> Imagenes { get; set; } = new();
    }
}