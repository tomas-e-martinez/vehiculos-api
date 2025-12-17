using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                Email = dto.Email,
                RoleId = dto.RoleId
            };

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict(new { message = "El email ingresado ya se encuentra en uso." });

            newUser.PasswordHash = _usersService.HashPassword(newUser, dto.Password);

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var response = new
            {
                message = "Usuario creado con éxito."
            };

            return StatusCode(201, response);

        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if(user == null)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos. " });
            }

            var success = _usersService.VerifyPassword(user, user.PasswordHash, dto.Password);

            if (success)
            {
                var token = _usersService.GenerateJwt(user);
                return Ok(new { message = "Sesión iniciada con éxito.", token });
            }

            return Unauthorized(new { message = "Usuario o contraseña incorrectos. " });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    Role = u.Role.Name
                })
                .ToListAsync();

            return Ok(users);
        }

    }

}
