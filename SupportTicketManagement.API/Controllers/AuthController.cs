using Microsoft.AspNetCore.Mvc;
using SupportTicketManagement.API.DTOs.Auth;
using SupportTicketManagement.API.Services;

namespace SupportTicketManagement.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Tags("Auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Login — returns JWT token</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var result = await _authService.LoginAsync(dto.Email, dto.Password);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(result);
        }
    }
}
