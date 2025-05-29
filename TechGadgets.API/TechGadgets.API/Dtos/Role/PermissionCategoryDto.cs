using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Role
{
    public class PermissionCategoryDto
    {
        public string Modulo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public List<PermissionDto> Permisos { get; set; } = new();
    }
}