using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Dtos.Auth;
using TechGadgets.API.Models.Entities;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly TechGadgetsDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(TechGadgetsDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress)
        {
            try
            {
                // Buscar usuario por email
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.UsuEmail.ToLower() == request.Email.ToLower());

                if (user == null || user.UsuActivo != true)
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Credenciales inválidas" 
                    };
                }

                // Verificar si el usuario está bloqueado
                if (user.UsuBloqueadoHasta.HasValue && user.UsuBloqueadoHasta > DateTime.UtcNow)
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Usuario bloqueado temporalmente" 
                    };
                }

                // Verificar contraseña
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.UsuPassword))
                {
                    // Incrementar intentos fallidos
                    user.UsuIntentosFallidos++;
                    
                    // Bloquear usuario después de 5 intentos
                    if (user.UsuIntentosFallidos >= 5)
                    {
                        user.UsuBloqueadoHasta = DateTime.UtcNow.AddMinutes(30);
                    }

                    await _context.SaveChangesAsync();

                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Credenciales inválidas" 
                    };
                }

                // Resetear intentos fallidos en login exitoso
                user.UsuIntentosFallidos = 0;
                user.UsuBloqueadoHasta = null;
                user.UsuUltimoAcceso = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Obtener roles y permisos
                var rolesPermisos = await GetUserRolesAndPermissionsAsync(user.UsuId);

                // Generar tokens
                var jwtToken = _tokenService.GenerateJwtToken(user, rolesPermisos.Roles, rolesPermisos.Permissions);
                var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);

                // Guardar refresh token
                await _tokenService.SaveRefreshTokenAsync(user.UsuId, refreshToken);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    Token = jwtToken,
                    RefreshToken = refreshToken.Token,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(60),
                    User = new UserInfoDto
                    {
                        Id = user.UsuId,
                        Email = user.UsuEmail,
                        NombreCompleto = $"{user.UsuNombre} {user.UsuApellido}",
                        Roles = rolesPermisos.Roles,
                        Permisos = rolesPermisos.Permissions
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto 
                { 
                    Success = false, 
                    Message = "Error interno del servidor" 
                };
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                // Verificar si el email ya existe
                if (await EmailExistsAsync(request.Email))
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "El email ya está registrado" 
                    };
                }

                // Crear nuevo usuario
                var user = new Usuario
                {
                    UsuEmail = request.Email.ToLower(),
                    UsuPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    UsuNombre = request.Nombre,
                    UsuApellido = request.Apellido,
                    UsuTelefono = request.Telefono,
                    UsuFechaNacimiento = request.FechaNacimiento,
                    UsuGenero = request.Genero,
                    UsuActivo = true,
                    UsuEmailVerificado = false, // Implementar verificación por email después
                    UsuFechaCreacion = DateTime.UtcNow,
                    UsuFechaModificacion = DateTime.UtcNow
                };

                _context.Usuarios.Add(user);
                await _context.SaveChangesAsync();

                // Asignar rol de Cliente por defecto
                var clienteRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RolNombre == "Cliente");

                if (clienteRole != null)
                {
                    var userRole = new UsuariosRole
                    {
                        UsrUsuarioId = user.UsuId,
                        UsrRolId = clienteRole.RolId,
                        UsrFechaAsignacion = DateTime.UtcNow,
                        UsrActivo = true
                    };

                    _context.UsuariosRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Usuario registrado exitosamente",
                    User = new UserInfoDto
                    {
                        Id = user.UsuId,
                        Email = user.UsuEmail,
                        NombreCompleto = $"{user.UsuNombre} {user.UsuApellido}",
                        Roles = new List<string> { "Cliente" },
                        Permisos = new List<string>()
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto 
                { 
                    Success = false, 
                    Message = "Error al registrar usuario" 
                };
            }
        }

        public Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress)
        {
            // Implementar lógica de refresh token
            // Por ahora retornamos un error básico
            return Task.FromResult(new AuthResponseDto 
            { 
                Success = false, 
                Message = "Refresh token no implementado aún" 
            });
        }

        public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            await _tokenService.RevokeRefreshTokenAsync(token, ipAddress);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            var user = await _context.Usuarios.FindAsync(userId);
            if (user == null) return false;

            // Verificar contraseña actual
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.UsuPassword))
                return false;

            // Actualizar contraseña
            user.UsuPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UsuFechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.UsuEmail.ToLower() == email.ToLower());
        }

        public async Task<UserInfoDto?> GetUserInfoAsync(int userId)
        {
            var user = await _context.Usuarios.FindAsync(userId);
            if (user == null) return null;

            var rolesPermisos = await GetUserRolesAndPermissionsAsync(userId);

            return new UserInfoDto
            {
                Id = user.UsuId,
                Email = user.UsuEmail,
                NombreCompleto = $"{user.UsuNombre} {user.UsuApellido}",
                Roles = rolesPermisos.Roles,
                Permisos = rolesPermisos.Permissions
            };
        }

        private async Task<(List<string> Roles, List<string> Permissions)> GetUserRolesAndPermissionsAsync(int userId)
        {
            var roles = await _context.UsuariosRoles
                .Where(ur => ur.UsrUsuarioId == userId && ur.UsrActivo == true)
                .Join(_context.Roles, ur => ur.UsrRolId, r => r.RolId, (ur, r) => r.RolNombre)
                .ToListAsync();

            var permissions = await _context.UsuariosRoles
                .Where(ur => ur.UsrUsuarioId == userId && ur.UsrActivo == true)
                .Join(_context.Roles, ur => ur.UsrRolId, r => r.RolId, (ur, r) => r.RolId)
                .Join(_context.RolesPermisos, r => r, rp => rp.RpeRolId, (r, rp) => rp.RpePermisoCodigo)
                .Join(_context.Permisos, rp => rp, p => p.PerCodigo, (rp, p) => p.PerCodigo)
                .Distinct()
                .ToListAsync();

            return (roles, permissions);
        }
    }
}