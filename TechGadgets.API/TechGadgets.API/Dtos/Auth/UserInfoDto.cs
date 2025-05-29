using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechGadgets.API.Dtos.Auth
{
    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permisos { get; set; } = new();
    }
}