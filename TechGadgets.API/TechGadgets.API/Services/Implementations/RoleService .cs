// Services/Implementations/RoleService.cs
using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Dtos.Role;
using TechGadgets.API.Models.Entities;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly TechGadgetsDbContext _context;

        public RoleService(TechGadgetsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _context.Roles
                .Include(r => r.RolesPermisos)
                .ThenInclude(rp => rp.RpePermisoCodigoNavigation)
                .ToListAsync();

            var roleDtos = new List<RoleDto>();

            foreach (var role in roles)
            {
                var userCount = await _context.UsuariosRoles
                    .Where(ur => ur.UsrRolId == role.RolId && ur.UsrActivo == true)
                    .CountAsync();

                roleDtos.Add(new RoleDto
                {
                    Id = role.RolId,
                    Nombre = role.RolNombre,
                    Descripcion = role.RolDescripcion,
                    Activo = role.RolActivo ?? true,
                    FechaCreacion = role.RolFechaCreacion ?? DateTime.UtcNow,
                    TotalUsuarios = userCount,
                    Permisos = role.RolesPermisos.Select(rp => new PermissionDto
                    {
                        Codigo = rp.RpePermisoCodigo,
                        Nombre = rp.RpePermisoCodigoNavigation?.PerNombre ?? "",
                        Descripcion = rp.RpePermisoCodigoNavigation?.PerDescripcion,
                        Modulo = rp.RpePermisoCodigoNavigation?.PerModulo ?? "",
                        Activo = rp.RpePermisoCodigoNavigation?.PerActivo ?? true
                    }).ToList()
                });
            }

            return roleDtos;
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            var role = await _context.Roles
                .Include(r => r.RolesPermisos)
                .ThenInclude(rp => rp.RpePermisoCodigoNavigation)
                .FirstOrDefaultAsync(r => r.RolId == id);

            if (role == null) return null;

            var userCount = await _context.UsuariosRoles
                .Where(ur => ur.UsrRolId == role.RolId && ur.UsrActivo == true)
                .CountAsync();

            return new RoleDto
            {
                Id = role.RolId,
                Nombre = role.RolNombre,
                Descripcion = role.RolDescripcion,
                Activo = role.RolActivo ?? true,
                FechaCreacion = role.RolFechaCreacion ?? DateTime.UtcNow,
                TotalUsuarios = userCount,
                Permisos = role.RolesPermisos.Select(rp => new PermissionDto
                {
                    Codigo = rp.RpePermisoCodigo,
                    Nombre = rp.RpePermisoCodigoNavigation?.PerNombre ?? "",
                    Descripcion = rp.RpePermisoCodigoNavigation?.PerDescripcion,
                    Modulo = rp.RpePermisoCodigoNavigation?.PerModulo ?? "",
                    Activo = rp.RpePermisoCodigoNavigation?.PerActivo ?? true
                }).ToList()
            };
        }

        public async Task<RoleDto?> GetRoleByNameAsync(string name)
        {
            var role = await _context.Roles
                .Include(r => r.RolesPermisos)
                .ThenInclude(rp => rp.RpePermisoCodigoNavigation)
                .FirstOrDefaultAsync(r => r.RolNombre.ToLower() == name.ToLower());

            if (role == null) return null;

            return await GetRoleByIdAsync(role.RolId);
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            var role = new Role
            {
                RolNombre = dto.Nombre,
                RolDescripcion = dto.Descripcion,
                RolActivo = true,
                RolFechaCreacion = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Asignar permisos si se proporcionaron
            if (dto.PermisosCodigos.Any())
            {
                await AssignPermissionsToRoleAsync(role.RolId, dto.PermisosCodigos);
            }

            return await GetRoleByIdAsync(role.RolId) ?? throw new InvalidOperationException("Error al crear el rol");
        }

        public async Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleDto dto)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return null;

            role.RolNombre = dto.Nombre;
            role.RolDescripcion = dto.Descripcion;
            role.RolActivo = dto.Activo;

            // Actualizar permisos
            await AssignPermissionsToRoleAsync(id, dto.PermisosCodigos);

            await _context.SaveChangesAsync();
            return await GetRoleByIdAsync(id);
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return false;

            // Verificar si hay usuarios con este rol
            var hasUsers = await _context.UsuariosRoles
                .AnyAsync(ur => ur.UsrRolId == id && ur.UsrActivo == true);

            if (hasUsers)
            {
                // Desactivar en lugar de eliminar
                role.RolActivo = false;
            }
            else
            {
                // Eliminar permisos asociados
                var rolePermissions = await _context.RolesPermisos
                    .Where(rp => rp.RpeRolId == id)
                    .ToListAsync();
                _context.RolesPermisos.RemoveRange(rolePermissions);

                // Eliminar rol
                _context.Roles.Remove(role);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RoleExistsAsync(string name, int? excludeId = null)
        {
            return await _context.Roles
                .AnyAsync(r => r.RolNombre.ToLower() == name.ToLower() && 
                          (excludeId == null || r.RolId != excludeId));
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _context.Permisos
                .Where(p => p.PerActivo == true)
                .OrderBy(p => p.PerModulo)
                .ThenBy(p => p.PerNombre)
                .ToListAsync();

            return permissions.Select(p => new PermissionDto
            {
                Codigo = p.PerCodigo,
                Nombre = p.PerNombre,
                Descripcion = p.PerDescripcion,
                Modulo = p.PerModulo ?? "general",
                Activo = p.PerActivo ?? true
            });
        }

        public async Task<IEnumerable<PermissionCategoryDto>> GetPermissionsByCategoryAsync()
        {
            var permissions = await GetAllPermissionsAsync();
            
            return permissions
                .GroupBy(p => p.Modulo)
                .Select(g => new PermissionCategoryDto
                {
                    Modulo = g.Key,
                    Descripcion = GetModuleDescription(g.Key),
                    Permisos = g.ToList()
                })
                .OrderBy(c => c.Modulo);
        }

        private static string GetModuleDescription(string modulo)
        {
            return (modulo?.ToLower()) switch
            {
                "usuarios" => "Gestión de usuarios y perfiles",
                "productos" => "Gestión de productos y catálogo",
                "pedidos" => "Gestión de pedidos y ventas",
                "inventario" => "Control de inventario y stock",
                "reportes" => "Acceso a reportes y estadísticas",
                "configuracion" => "Configuración del sistema",
                "roles" => "Gestión de roles y permisos",
                "general" => "Permisos generales del sistema",
                _ => "Otros permisos"
            };
        }

        public async Task<IEnumerable<string>> GetRolePermissionsAsync(int roleId)
        {
            return await _context.RolesPermisos
                .Where(rp => rp.RpeRolId == roleId)
                .Select(rp => rp.RpePermisoCodigo)
                .ToListAsync();
        }

        public async Task<bool> AssignPermissionsToRoleAsync(int roleId, List<string> permissionCodes)
        {
            // Eliminar permisos existentes
            var existingPermissions = await _context.RolesPermisos
                .Where(rp => rp.RpeRolId == roleId)
                .ToListAsync();
            _context.RolesPermisos.RemoveRange(existingPermissions);

            // Agregar nuevos permisos
            foreach (var code in permissionCodes)
            {
                // Verificar que el permiso existe
                var permissionExists = await _context.Permisos
                    .AnyAsync(p => p.PerCodigo == code && p.PerActivo == true);

                if (permissionExists)
                {
                    _context.RolesPermisos.Add(new RolesPermiso
                    {
                        RpeRolId = roleId,
                        RpePermisoCodigo = code,
                        RpeFechaAsignacion = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserRoleDto>> GetUsersWithRolesAsync()
        {
            var usersWithRoles = await _context.Usuarios
                .Include(u => u.UsuariosRoles.Where(ur => ur.UsrActivo == true))
                .ThenInclude(ur => ur.UsrRol)
                .ThenInclude(r => r.RolesPermisos)
                .Where(u => u.UsuActivo == true)
                .ToListAsync();

            var result = new List<UserRoleDto>();

            foreach (var user in usersWithRoles)
            {
                var userRoles = user.UsuariosRoles.Select(ur => new RoleDto
                {
                    Id = ur.UsrRol.RolId,
                    Nombre = ur.UsrRol.RolNombre,
                    Descripcion = ur.UsrRol.RolDescripcion,
                    Activo = ur.UsrRol.RolActivo ?? true,
                    FechaCreacion = ur.UsrRol.RolFechaCreacion ?? DateTime.UtcNow
                }).ToList();

                var userPermissions = user.UsuariosRoles
                    .SelectMany(ur => ur.UsrRol.RolesPermisos.Select(rp => rp.RpePermisoCodigo))
                    .Distinct()
                    .ToList();

                result.Add(new UserRoleDto
                {
                    UsuarioId = user.UsuId,
                    Email = user.UsuEmail,
                    NombreCompleto = $"{user.UsuNombre} {user.UsuApellido}",
                    Roles = userRoles,
                    Permisos = userPermissions,
                    Activo = user.UsuActivo ?? true
                });
            }

            return result;
        }

        public async Task<UserRoleDto?> GetUserRolesAsync(int userId)
        {
            var user = await _context.Usuarios
                .Include(u => u.UsuariosRoles.Where(ur => ur.UsrActivo == true))
                .ThenInclude(ur => ur.UsrRol)
                .ThenInclude(r => r.RolesPermisos)
                .FirstOrDefaultAsync(u => u.UsuId == userId);

            if (user == null) return null;

            var userRoles = user.UsuariosRoles.Select(ur => new RoleDto
            {
                Id = ur.UsrRol.RolId,
                Nombre = ur.UsrRol.RolNombre,
                Descripcion = ur.UsrRol.RolDescripcion,
                Activo = ur.UsrRol.RolActivo ?? true,
                FechaCreacion = ur.UsrRol.RolFechaCreacion ?? DateTime.UtcNow
            }).ToList();

            var userPermissions = user.UsuariosRoles
                .SelectMany(ur => ur.UsrRol.RolesPermisos.Select(rp => rp.RpePermisoCodigo))
                .Distinct()
                .ToList();

            return new UserRoleDto
            {
                UsuarioId = user.UsuId,
                Email = user.UsuEmail,
                NombreCompleto = $"{user.UsuNombre} {user.UsuApellido}",
                Roles = userRoles,
                Permisos = userPermissions,
                Activo = user.UsuActivo ?? true
            };
        }

        public async Task<bool> AssignRolesToUserAsync(AssignRoleDto dto)
        {
            // Desactivar roles existentes
            var existingRoles = await _context.UsuariosRoles
                .Where(ur => ur.UsrUsuarioId == dto.UsuarioId)
                .ToListAsync();

            foreach (var role in existingRoles)
            {
                role.UsrActivo = false;
            }

            // Asignar nuevos roles
            foreach (var roleId in dto.RoleIds)
            {
                // Verificar si ya existe la relación
                var existingRelation = existingRoles
                    .FirstOrDefault(ur => ur.UsrRolId == roleId);

                if (existingRelation != null)
                {
                    // Reactivar relación existente
                    existingRelation.UsrActivo = true;
                }
                else
                {
                    // Crear nueva relación
                    _context.UsuariosRoles.Add(new UsuariosRole
                    {
                        UsrUsuarioId = dto.UsuarioId,
                        UsrRolId = roleId,
                        UsrFechaAsignacion = DateTime.UtcNow,
                        UsrActivo = true
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(RemoveRoleDto dto)
        {
            var userRole = await _context.UsuariosRoles
                .FirstOrDefaultAsync(ur => ur.UsrUsuarioId == dto.UsuarioId && 
                                         ur.UsrRolId == dto.RoleId && 
                                         ur.UsrActivo == true);

            if (userRole == null) return false;

            userRole.UsrActivo = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserHasPermissionAsync(int userId, string permissionCode)
        {
            return await _context.UsuariosRoles
                .Where(ur => ur.UsrUsuarioId == userId && ur.UsrActivo == true)
                .Join(_context.RolesPermisos, ur => ur.UsrRolId, rp => rp.RpeRolId, (ur, rp) => rp)
                .Where(rp => rp.RpePermisoCodigo == permissionCode)
                .AnyAsync();
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleName)
        {
            return await _context.UsuariosRoles
                .Where(ur => ur.UsrUsuarioId == userId && ur.UsrActivo == true)
                .Join(_context.Roles, ur => ur.UsrRolId, r => r.RolId, (ur, r) => r)
                .Where(r => r.RolNombre.ToLower() == roleName.ToLower() && r.RolActivo == true)
                .AnyAsync();
        }
    }
}