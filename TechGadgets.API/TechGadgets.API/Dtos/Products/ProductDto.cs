using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? DescripcionCorta { get; set; }
        public string? DescripcionLarga { get; set; }
        public string Slug { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal? PrecioComparacion { get; set; }
        public decimal? Costo { get; set; }
        public string? Tipo { get; set; }
        public string? Estado { get; set; }
        public bool Destacado { get; set; }
        public bool Nuevo { get; set; }
        public bool EnOferta { get; set; }
        public decimal? Peso { get; set; }
        public string? Dimensiones { get; set; }
        public string? MetaTitulo { get; set; }
        public string? MetaDescripcion { get; set; }
        public string? PalabrasClaves { get; set; }
        public bool RequiereEnvio { get; set; }
        public bool PermiteReseñas { get; set; }
        public string? Garantia { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Relaciones
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public string CategoriaRuta { get; set; } = string.Empty;
        public int MarcaId { get; set; }
        public string MarcaNombre { get; set; } = string.Empty;
        public string? MarcaLogo { get; set; }

        // Imágenes
        public List<ProductImageDto> Imagenes { get; set; } = new();
        public string? ImagenPrincipal { get; set; }

        // Inventario
        public int StockActual { get; set; }
        public int? StockReservado { get; set; }
        public int StockDisponible => StockActual - (StockReservado ?? 0);

        // Estados calculados
        public decimal PrecioFinal => EnOferta && PrecioComparacion.HasValue ? PrecioComparacion.Value : Precio;
        public decimal? PorcentajeDescuento => EnOferta && PrecioComparacion.HasValue && PrecioComparacion < Precio ? 
            Math.Round(((Precio - PrecioComparacion.Value) / Precio) * 100, 2) : null;
        public string EstadoStock => StockDisponible <= 0 ? "Sin Stock" : StockDisponible <= 5 ? "Bajo Stock" : "Disponible";
    }
}