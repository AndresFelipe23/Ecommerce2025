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
    [SwaggerTag("Gestión de roles y permisos del sistema")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Obtiene todos los roles del sistema
        /// </summary>
        [HttpGet]
        [RequirePermission("roles.listar")]
        [SwaggerOperation(Summary = "Listar todos los roles", Description = "Obtiene todos los roles con sus permisos asociados")]
        [SwaggerResponse(200, "Lista de roles obtenida exitosamente", typeof(IEnumerable<RoleDto>))]
        [SwaggerResponse(403, "No tiene permisos para ver los roles")]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(new { success = true, data = roles });
        }

        /// <summary>
        /// Obtiene un rol específico por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("roles.ver")]
        [SwaggerOperation(Summary = "Obtener rol por ID", Description = "Obtiene los detalles de un rol específico")]
        [SwaggerResponse(200, "Rol encontrado", typeof(RoleDto))]
        [SwaggerResponse(404, "Rol no encontrado")]
        [SwaggerResponse(403, "No tiene permisos para ver este rol")]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { success = false, message = "Rol no encontrado" });
            }

            return Ok(new { success = true, data = role });
        }

        /// <summary>
        /// Crea un nuevo rol
        /// </summary>
        [HttpPost]
        [RequirePermission("roles.crear")]
        [SwaggerOperation(Summary = "Crear nuevo rol", Description = "Crea un nuevo rol con sus permisos")]
        [SwaggerResponse(201, "Rol creado exitosamente", typeof(RoleDto))]
        [SwaggerResponse(400, "Datos inválidos o rol ya existe")]
        [SwaggerResponse(403, "No tiene permisos para crear roles")]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto dto)
        {
            // Verificar si el rol ya existe
            if (await _roleService.RoleExistsAsync(dto.Nombre))
            {
                return BadRequest(new { success = false, message = "Ya existe un rol con ese nombre" });
            }

            var role = await _roleService.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, 
                new { success = true, message = "Rol creado exitosamente", data = role });
        }

        /// <summary>
        /// Actualiza un rol existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("roles.editar")]
        [SwaggerOperation(Summary = "Actualizar rol", Description = "Actualiza un rol existente y sus permisos")]
        [SwaggerResponse(200, "Rol actualizado exitosamente", typeof(RoleDto))]
        [SwaggerResponse(404, "Rol no encontrado")]
        [SwaggerResponse(400, "Datos inválidos")]
        [SwaggerResponse(403, "No tiene permisos para editar roles")]
        public async Task<ActionResult<RoleDto>> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            // Verificar si el nombre ya existe en otro rol
            if (await _roleService.RoleExistsAsync(dto.Nombre, id))
            {
                return BadRequest(new { success = false, message = "Ya existe otro rol con ese nombre" });
            }

            var role = await _roleService.UpdateRoleAsync(id, dto);
            if (role == null)
            {
                return NotFound(new { success = false, message = "Rol no encontrado" });
            }

            return Ok(new { success = true, message = "Rol actualizado exitosamente", data = role });
        }

        /// <summary>
        /// Elimina un rol
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("roles.eliminar")]
        [SwaggerOperation(Summary = "Eliminar rol", Description = "Elimina un rol del sistema")]
        [SwaggerResponse(200, "Rol eliminado exitosamente")]
        [SwaggerResponse(404, "Rol no encontrado")]
        [SwaggerResponse(400, "No se puede eliminar el rol porque tiene usuarios asignados")]
        [SwaggerResponse(403, "No tiene permisos para eliminar roles")]
        public async Task<ActionResult> DeleteRole(int id)
        {
            var success = await _roleService.DeleteRoleAsync(id);
            if (!success)
            {
                return NotFound(new { success = false, message = "Rol no encontrado" });
            }

            return Ok(new { success = true, message = "Rol eliminado exitosamente" });
        }

        /// <summary>
        /// Obtiene todos los permisos disponibles
        /// </summary>
        [HttpGet("permissions")]
        [RequirePermission("permisos.listar")]
        [SwaggerOperation(Summary = "Listar permisos", Description = "Obtiene todos los permisos disponibles agrupados por categoría")]
        [SwaggerResponse(200, "Lista de permisos obtenida exitosamente", typeof(IEnumerable<PermissionCategoryDto>))]
        [SwaggerResponse(403, "No tiene permisos para ver los permisos")]
        public async Task<ActionResult<IEnumerable<PermissionCategoryDto>>> GetPermissions()
        {
            var permissions = await _roleService.GetPermissionsByCategoryAsync();
            return Ok(new { success = true, data = permissions });
        }

        /// <summary>
        /// Obtiene los permisos de un rol específico
        /// </summary>
        [HttpGet("{id}/permissions")]
        [RequirePermission("roles.ver")]
        [SwaggerOperation(Summary = "Obtener permisos de un rol", Description = "Obtiene los permisos asignados a un rol específico")]
        [SwaggerResponse(200, "Permisos del rol obtenidos exitosamente", typeof(IEnumerable<string>))]
        [SwaggerResponse(404, "Rol no encontrado")]
        [SwaggerResponse(403, "No tiene permisos para ver los permisos del rol")]
        public async Task<ActionResult<IEnumerable<string>>> GetRolePermissions(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { success = false, message = "Rol no encontrado" });
            }

            var permissions = await _roleService.GetRolePermissionsAsync(id);
            return Ok(new { success = true, data = permissions });
        }
    }
}