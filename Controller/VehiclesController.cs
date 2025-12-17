using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Security.Claims;
using vehiculos_api.Data;
using vehiculos_api.DTOs;
using vehiculos_api.Model;

namespace vehiculos_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly VehicleContext _context;
        public VehiclesController(VehicleContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetUserVehicles()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var vehicles = await _context.Vehicles
                .Where(v => v.UserId == userId)
                .Select( v => new
                {
                    v.Id,
                    v.Brand,
                    v.Model,
                    v.Year,
                    v.Kilometers,
                    VehicleType = v.VehicleType.Name,
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateVehicle([FromBody] CreateVehicleDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();


            try
            {
                var vehicle = new Vehicle
                {
                    Brand = dto.Brand,
                    Model = dto.Model,
                    Year = dto.Year,
                    Kilometers = dto.Kilometers,
                    VehicleTypeId = dto.VehicleTypeId,
                    UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                var maintenanceTypes = await _context.MaintenanceTypes
                    .Where(mt => mt.VehicleTypes.Any(v => v.Id == dto.VehicleTypeId))
                    .ToListAsync();

                if(maintenanceTypes.Count > 0)
                {
                    foreach (var maintenanceType in maintenanceTypes)
                    {
                        var maintenanceTask = new MaintenanceTask
                        {
                            VehicleId = vehicle.Id,
                            MaintenanceTypeId = maintenanceType.Id,
                            KmTarget = null,
                            DateTarget = null,
                            IsCompleted = false,
                            CompletedAt = null,
                            CompletedKm = null
                        };
                        _context.Add(maintenanceTask);
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return StatusCode(201, new {
                    message = "Vehículo creado con éxito.",
                    vehicleId = vehicle.Id
                });

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = "Error al crear vehículo.", detail = ex.Message });
            }

        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto dto)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    return NotFound(new { error = "Vehículo no encontrado." });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (vehicle.UserId != userId)
                {
                    return StatusCode(403, new { error = "El vehículo no pertenece al usuario autenticado." });
                }

                _context.Entry(vehicle).CurrentValues.SetValues(dto);

                var result = await _context.SaveChangesAsync();


                return StatusCode(200, new { message = "Vehículo modificado con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al modificar vehículo.", detail = ex.Message });
            }
        }
    }
}
