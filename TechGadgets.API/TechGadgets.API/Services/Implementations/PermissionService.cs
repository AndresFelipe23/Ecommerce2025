// Services/Implementations/PermissionService.cs
using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly TechGadgetsDbContext _context;

        public PermissionService(TechGadgetsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permission)
        {
            return await _context.UsuariosRoles
                .Where(ur => ur.UsrUsuarioId == userId && ur.UsrActivo == true)
                .Join(_context.RolesPermisos, ur => ur.UsrRolId, rp => rp.RpeRolId, (ur, rp) => rp)
                .Where(rp => rp.RpePermisoCodigo == permission)
                .AnyAsync();
        }

        public async Task<bool> HasRoleAsync(int userId, string role)
        {
            return await _context.UsuariosRoles
                .Where(ur => ur.UsrUsuarioId == userId && ur.UsrActivo == true)
                .Join(_context.Roles, ur => ur.UsrRolId, r => r.RolId, (ur, r) => r)
                .Where(r => r.RolNombre.ToLower() == role.ToLower() && r.RolActivo == true)
                .AnyAsync();
        }

        public async Task<bool> HasAnyPermissionAsync(int userId, params string[] permissions)
        {
            return await _context.UsuariosRoles
                .Where(ur => ur.UsrUsuarioId == userId && ur.UsrActivo == true)
                .Join(_context.RolesPermisos, ur => ur.UsrRolId, rp => rp.RpeRolId, (ur, rp) => rp)
                .Where(rp => permissions.Contains(rp.RpePermisoCodigo))
                .AnyAsync();
        }

        public async Task<bool> HasAllPermissionsAsync(int userId, params string[] permissions)
        {
            var userPermissions = await GetUserPermissionsAsync(userId);
            return permissions.All(p => userPermissions.Contains(p));
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            return await _context.UsuariosRoles
                .Where(ur => ur.UsrUsuarioId == userId && ur.UsrActivo == true)
                .Join(_context.RolesPermisos, ur => ur.UsrRolId, rp => rp.RpeRolId, (ur, rp) => rp)
                .Select(rp => rp.RpePermisoCodigo)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            return await _context.UsuariosRoles
                .Where(ur => ur.UsrUsuarioId == userId && ur.UsrActivo == true)
                .Join(_context.Roles, ur => ur.UsrRolId, r => r.RolId, (ur, r) => r)
                .Where(r => r.RolActivo == true)
                .Select(r => r.RolNombre)
                .ToListAsync();
        }
    }
}