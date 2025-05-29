using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Role
{
    public class RemoveRoleDto
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El ID del rol es requerido")]
        public int RoleId { get; set; }

        public string? Motivo { get; set; } 
    }
}