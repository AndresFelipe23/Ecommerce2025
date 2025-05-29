using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Products
{
    public class UpdateProductWithImagesRequest : CreateProductWithImagesRequest
    {
        public bool Activo { get; set; } = true;
    
        // ✅ NUEVOS ARCHIVOS A AGREGAR
        public IFormFileCollection? NewImageFiles { get; set; }
    }
}