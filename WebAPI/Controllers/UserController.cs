using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Authorize(Roles = "Administrator")]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly JwtTokenService _jwtTokenService;

        public UserController(UserService service, JwtTokenService jwtTokenService)
        {
            _userService = service;
            _jwtTokenService = jwtTokenService;
        }
     
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            if (users == null || !users.Any())
            {
                return NotFound("No Hero Loadouts Registered!");
            }
            return Ok(users);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserCredentials credentials)
        {
            if (credentials.UserName.Length > 10 ||!Regex.IsMatch(credentials.UserName, "^[A-Za-z0-9]+$"))
            {
                return BadRequest("Username can only contain letters and numbers and be a maximum of 10 characters with no spaces.");
            }
            if (credentials.Password.Length < 15 || credentials.Password.Length > 30 || !Regex.IsMatch(credentials.Password, "^[A-Za-z0-9!#$]+$"))
            {
                return BadRequest("Password must be a minimum of 15 characters with a maximum of 30 and only contain letters, numbers, no spaces, and/or special characters including !,#, and $.");
            }
            var user = await _userService.GetUserByUsernameAsync(credentials.UserName);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            if (!BCrypt.Net.BCrypt.Verify(credentials.Password, user.Password))
            {
                return Unauthorized("Invalid username or password.");
            }
            var token = _jwtTokenService.GenerateJwtToken(user.UserName, user.Role);
            return Ok(new { Token = token });
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterUser request)
        {
            if ( request.Username.Length > 10 ||!Regex.IsMatch(request.Username, "^[A-Za-z0-9]+$"))
            {
                return BadRequest("Username can only contain letters and numbers and be a maximum of 10 characters with no spaces.");
            }
            if (request.Password.Length < 15 || request.Password.Length > 30 ||!Regex.IsMatch(request.Password, "^[A-Za-z0-9!#$]+$"))
            {
                return BadRequest("Password must be a minimum of 15 characters with a maximum of 30 and only contain letters, numbers, no spaces, and/or special characters including !,#, and $.");
            }
            var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return BadRequest("Username already exists.");
            }
            var password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var newUser = new User
            {
                UserName = request.Username,
                Password = password,
                Role = "user" 
            };
            await _userService.CreateUserAsync(newUser);
            return Ok("User registered successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var userDeleted = await _userService.DeleteUserByIdAsync(id);
            if (!userDeleted)
            {
                return NotFound("User not found.");
            }
            return Ok("User deleted successfully.");
        }

        [HttpPost("registerAdministrator")]
        public async Task<ActionResult> RegisterAdministrator([FromBody] RegisterUser request)
        {
            if (request.Username.Length > 10 || !Regex.IsMatch(request.Username, "^[A-Za-z0-9]+$"))
            {
                return BadRequest("Username can only contain letters and numbers and be a maximum of 10 characters with no spaces.");
            }
            if (request.Password.Length < 15 || request.Password.Length > 30 || !Regex.IsMatch(request.Password, "^[A-Za-z0-9!#$]+$"))
            {
                return BadRequest("Password must be a minimum of 15 characters with a maximum of 30 and only contain letters, numbers, no spaces, and/or special characters including !,#, and $.");
            }
            var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return BadRequest("Username already exists.");
            }
            var password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var newUser = new User
            {
                UserName = request.Username,
                Password = password,
                Role = "Administrator" 
            };
            await _userService.CreateUserAsync(newUser);
            return Ok("User registered successfully.");
        }
    }
}
