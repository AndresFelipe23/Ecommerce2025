using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechGadgets.API.Dtos.Role;

namespace TechGadgets.API.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(int id);
        Task<RoleDto?> GetRoleByNameAsync(string name);
        Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
        Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleDto dto);
        Task<bool> DeleteRoleAsync(int id);
        Task<bool> RoleExistsAsync(string name, int? excludeId = null);

        // Gestión de Permisos
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
        Task<IEnumerable<PermissionCategoryDto>> GetPermissionsByCategoryAsync();
        Task<IEnumerable<string>> GetRolePermissionsAsync(int roleId);
        Task<bool> AssignPermissionsToRoleAsync(int roleId, List<string> permissionCodes);

        // Gestión de Usuarios y Roles
        Task<IEnumerable<UserRoleDto>> GetUsersWithRolesAsync();
        Task<UserRoleDto?> GetUserRolesAsync(int userId);
        Task<bool> AssignRolesToUserAsync(AssignRoleDto dto);
        Task<bool> RemoveRoleFromUserAsync(RemoveRoleDto dto);
        Task<bool> UserHasPermissionAsync(int userId, string permissionCode);
        Task<bool> UserHasRoleAsync(int userId, string roleName);
    }
}