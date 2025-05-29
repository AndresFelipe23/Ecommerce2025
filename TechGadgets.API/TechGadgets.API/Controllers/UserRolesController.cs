using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TechGadgets.API.Attributes;
using TechGadgets.API.Dtos.Role;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SwaggerTag("Gestión de roles de usuarios")]
    public class UserRolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public UserRolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Obtiene todos los usuarios con sus roles asignados
        /// </summary>
        [HttpGet]
        [RequirePermission("usuarios.listar", "roles.ver")]
        [SwaggerOperation(Summary = "Listar usuarios con roles", Description = "Obtiene todos los usuarios con sus roles y permisos")]
        [SwaggerResponse(200, "Lista de usuarios con roles obtenida exitosamente", typeof(IEnumerable<UserRoleDto>))]
        [SwaggerResponse(403, "No tiene permisos para ver los usuarios")]
        public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUsersWithRoles()
        {
            var users = await _roleService.GetUsersWithRolesAsync();
            return Ok(new { success = true, data = users });
        }

        /// <summary>
        /// Obtiene los roles de un usuario específico
        /// </summary>
        [HttpGet("{userId}")]
        [RequirePermission("usuarios.ver", "roles.ver")]
        [SwaggerOperation(Summary = "Obtener roles de usuario", Description = "Obtiene los roles y permisos de un usuario específico")]
        [SwaggerResponse(200, "Roles del usuario obtenidos exitosamente", typeof(UserRoleDto))]
        [SwaggerResponse(404, "Usuario no encontrado")]
        [SwaggerResponse(403, "No tiene permisos para ver los roles del usuario")]
        public async Task<ActionResult<UserRoleDto>> GetUserRoles(int userId)
        {
            var userRoles = await _roleService.GetUserRolesAsync(userId);
            if (userRoles == null)
            {
                return NotFound(new { success = false, message = "Usuario no encontrado" });
            }

            return Ok(new { success = true, data = userRoles });
        }

        /// <summary>
        /// Asigna roles a un usuario
        /// </summary>
        [HttpPost("assign")]
        [RequirePermission("roles.asignar")]
        [SwaggerOperation(Summary = "Asignar roles", Description = "Asigna uno o más roles a un usuario")]
        [SwaggerResponse(200, "Roles asignados exitosamente")]
        [SwaggerResponse(400, "Datos inválidos")]
        [SwaggerResponse(404, "Usuario no encontrado")]
        [SwaggerResponse(403, "No tiene permisos para asignar roles")]
        public async Task<ActionResult> AssignRoles([FromBody] AssignRoleDto dto)
        {
            var success = await _roleService.AssignRolesToUserAsync(dto);
            if (!success)
            {
                return BadRequest(new { success = false, message = "Error al asignar roles" });
            }

            return Ok(new { success = true, message = "Roles asignados exitosamente" });
        }

        /// <summary>
        /// Remueve un rol de un usuario
        /// </summary>
        [HttpPost("remove")]
        [RequirePermission("roles.remover")]
        [SwaggerOperation(Summary = "Remover rol", Description = "Remueve un rol específico de un usuario")]
        [SwaggerResponse(200, "Rol removido exitosamente")]
        [SwaggerResponse(400, "Datos inválidos")]
        [SwaggerResponse(404, "Usuario o rol no encontrado")]
        [SwaggerResponse(403, "No tiene permisos para remover roles")]
        public async Task<ActionResult> RemoveRole([FromBody] RemoveRoleDto dto)
        {
            var success = await _roleService.RemoveRoleFromUserAsync(dto);
            if (!success)
            {
                return NotFound(new { success = false, message = "Usuario o rol no encontrado" });
            }

            return Ok(new { success = true, message = "Rol removido exitosamente" });
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico
        /// </summary>
        [HttpGet("{userId}/has-permission/{permission}")]
        [RequirePermission("usuarios.ver")]
        [SwaggerOperation(Summary = "Verificar permiso", Description = "Verifica si un usuario tiene un permiso específico")]
        [SwaggerResponse(200, "Verificación completada", typeof(bool))]
        [SwaggerResponse(404, "Usuario no encontrado")]
        [SwaggerResponse(403, "No tiene permisos para verificar permisos")]
        public async Task<ActionResult<bool>> HasPermission(int userId, string permission)
        {
            var hasPermission = await _roleService.UserHasPermissionAsync(userId, permission);
            return Ok(new { success = true, hasPermission = hasPermission });
        }

        /// <summary>
        /// Verifica si un usuario tiene un rol específico
        /// </summary>
        [HttpGet("{userId}/has-role/{roleName}")]
        [RequirePermission("usuarios.ver")]
        [SwaggerOperation(Summary = "Verificar rol", Description = "Verifica si un usuario tiene un rol específico")]
        [SwaggerResponse(200, "Verificación completada", typeof(bool))]
        [SwaggerResponse(404, "Usuario no encontrado")]
        [SwaggerResponse(403, "No tiene permisos para verificar roles")]
        public async Task<ActionResult<bool>> HasRole(int userId, string roleName)
        {
            var hasRole = await _roleService.UserHasRoleAsync(userId, roleName);
            return Ok(new { success = true, hasRole = hasRole });
        }
    }
}

