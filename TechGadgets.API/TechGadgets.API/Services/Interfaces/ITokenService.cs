using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TechGadgets.API.Models;
using TechGadgets.API.Models.Entities;

namespace TechGadgets.API.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(Usuario user, List<string> roles, List<string> permissions);
        RefreshToken GenerateRefreshToken(string ipAddress);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
        Task SaveRefreshTokenAsync(int userId, RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress);
    }
}