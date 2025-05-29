using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class CreateProductWithImagesRequest
    {
        public string SKU { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? DescripcionCorta { get; set; }
        public string? DescripcionLarga { get; set; }
        public decimal Precio { get; set; }
        public decimal? PrecioComparacion { get; set; }
        public decimal? Costo { get; set; }
        public int CategoriaId { get; set; }
        public int MarcaId { get; set; }
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
        public bool RequiereEnvio { get; set; } = true;
        public bool PermiteReseñas { get; set; } = true;
        public string? Garantia { get; set; }
        public int Orden { get; set; }
        public int StockInicial { get; set; }
        
        // ✅ ARCHIVOS DE IMAGEN
        public IFormFileCollection? ImageFiles { get; set; }
        
        // ✅ URLs EXTERNAS
        public List<string>? ExternalImageUrls { get; set; }
    }
}