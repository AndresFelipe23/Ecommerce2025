using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using TechGadgets.API.Dtos.Auth;
using TechGadgets.API.Services.Interfaces;

namespace TechGadgets.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Gestión de autenticación y autorización del sistema")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = GetIpAddress();
            var result = await _authService.LoginAsync(request, ipAddress);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = GetIpAddress();
            var result = await _authService.RefreshTokenAsync(request, ipAddress);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
        {
            var ipAddress = GetIpAddress();
            await _authService.RevokeTokenAsync(refreshToken, ipAddress);
            return Ok(new { message = "Token revocado exitosamente" });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var result = await _authService.ChangePasswordAsync(userId, request);

            if (!result)
                return BadRequest(new { message = "Error al cambiar contraseña" });

            return Ok(new { message = "Contraseña cambiada exitosamente" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var userInfo = await _authService.GetUserInfoAsync(userId);

            if (userInfo == null)
                return NotFound();

            return Ok(userInfo);
        }

        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            var exists = await _authService.EmailExistsAsync(email);
            return Ok(new { exists });
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];

            if (Request.Headers.ContainsKey("X-Real-IP"))
                return Request.Headers["X-Real-IP"];

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        }
    }
}