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
                    v.VehicleType.Name,
                    v.MaintenanceTasks
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
    }
}
