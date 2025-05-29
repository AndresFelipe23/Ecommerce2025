using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechGadgets.API.Dtos.Auth;

namespace TechGadgets.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress);
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress);
        Task<bool> RevokeTokenAsync(string token, string ipAddress);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
        Task<bool> EmailExistsAsync(string email);
        Task<UserInfoDto?> GetUserInfoAsync(int userId);
    }
}