using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using vehiculos_api.Data;
using vehiculos_api.DTOs;
using vehiculos_api.Model;
using vehiculos_api.Service;

namespace vehiculos_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly VehicleContext _context;
        private readonly UsersService _usersService;

        public UsersController(VehicleContext context, UsersService usersService)
        {
            _context = context;
            _usersService = usersService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult> SignUp([FromBody] SignUpDto dto)
        {
            User newUser = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email
            };

            newUser.PasswordHash = _usersService.HashPassword(newUser, dto.Password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var response = new
            {
                message = "Usuario creado con éxito.",
                user = newUser
            };

            return StatusCode(201, response);

        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

            if(user == null)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos. " });
            }

            var success = _usersService.VerifyPassword(user, user.PasswordHash, dto.Password);

            if (success) return Ok(new { message = "Sesión iniciada con éxito." });

            return Unauthorized(new { message = "Usuario o contraseña incorrectos. " });
        }
    }

}
