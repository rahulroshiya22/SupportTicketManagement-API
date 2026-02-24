using Microsoft.AspNetCore.Mvc;
using SupportTicketManagement.API.DTOs.Auth;
using SupportTicketManagement.API.Services;

namespace SupportTicketManagement.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(dto.Email, dto.Password);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(result);
        }
    }
}
