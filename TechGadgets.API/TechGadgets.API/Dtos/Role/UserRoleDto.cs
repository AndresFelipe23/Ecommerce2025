using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Role
{
    public class UserRoleDto
    {
        public int UsuarioId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public List<RoleDto> Roles { get; set; } = new();
        public List<string> Permisos { get; set; } = new();
        public bool Activo { get; set; }
    }
}