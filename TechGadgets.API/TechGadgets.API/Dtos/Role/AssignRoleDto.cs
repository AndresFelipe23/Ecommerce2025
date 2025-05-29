using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Role
{
    public class AssignRoleDto
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Al menos un rol es requerido")]
        public List<int> RoleIds { get; set; } = new();

        public string? Motivo { get; set; }
    }
}