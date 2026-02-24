using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketManagement.API.DTOs.Users;
using SupportTicketManagement.API.Services;

namespace SupportTicketManagement.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "MANAGER")]
    [Tags("Users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>Create a new user — MANAGER only</summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(dto);
                return StatusCode(201, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>List all users — MANAGER only</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserResponseDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
    }
}
