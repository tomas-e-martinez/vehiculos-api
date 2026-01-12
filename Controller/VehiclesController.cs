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
    [Route("[controller]")]
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
                .Where(v => v.UserId == userId && v.IsActive)
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
        [HttpGet("{id}")]
        public async Task<ActionResult> GetVehicle(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == id && v.IsActive)
                    .Select(v => new
                    {
                        v.Id,
                        v.Brand,
                        v.Model,
                        v.Year,
                        v.Kilometers,
                        VehicleType = v.VehicleType.Name,
                        v.UserId
                    })
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return NotFound(new { error = "Vehículo no encontrado." });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (vehicle.UserId != userId)
                {
                    return StatusCode(403, new { error = "El vehículo no pertenece al usuario autenticado." });
                }

                return Ok(new
                {
                    vehicle.Id,
                    vehicle.Brand,
                    vehicle.Model,
                    vehicle.Year,
                    vehicle.Kilometers,
                    vehicle.VehicleType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener el vehículo.", detail = ex.Message });
            }
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

                _context.VehicleKmsDates.Add(new VehicleKmsDate
                {
                    VehicleId = vehicle.Id,
                    Date = DateTime.UtcNow,
                    Kilometers = vehicle.Kilometers
                });

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
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == id && v.IsActive);

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

        [Authorize]
        [HttpGet("{id}/maintenance")]
        public async Task<ActionResult> GetVehicleMaintenance(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == id && v.IsActive)
                    .Select(v => new
                    {
                        MaintenanceTasks = v.MaintenanceTasks.Select(mt => new
                        {
                            mt.Id,
                            maintenanceType = mt.MaintenanceType.Name,
                            mt.KmTarget,
                            mt.DateTarget,
                            mt.IsCompleted,
                            mt.CompletedAt,
                            mt.CompletedKm
                        })
                    })
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                    return NotFound("No se encontró el vehículo.");

                return Ok(vehicle.MaintenanceTasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener el mantenimiento del vehículo.", detail = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}/kilometers")]
        public async Task<ActionResult> GetVehicleKmLogs(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == id && v.IsActive)
                    .Select( v => new
                    {
                        v.UserId,
                        KmLogs = v.VehicleKmsDates
                            .OrderByDescending(k => k.Date)
                            .Select(k => new
                            {
                                k.Id,
                                k.Date,
                                k.Kilometers
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                    return NotFound(new { message = "Vehículo no encontrado." });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (vehicle.UserId != userId)
                {
                    return StatusCode(403, new { error = "El vehículo no pertenece al usuario autenticado." });
                }

                return Ok(vehicle.KmLogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener los logs de kilometraje del vehículo.", detail = ex.Message });
            }
        }

        [Authorize]
        [HttpPatch("{id}/kilometers")]
        public async Task<ActionResult> UpdateKilometers(int id, [FromBody] UpdateKmsDto dto)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == id && v.IsActive);

                if (vehicle == null)
                    return NotFound(new { message = "Vehículo no encontrado." });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (vehicle.UserId != userId)
                {
                    return StatusCode(403, new { error = "El vehículo no pertenece al usuario autenticado." });
                }

                if (dto.Kilometers <= vehicle.Kilometers)
                    return Conflict(new { message = "No puede restar kilómetros al vehículo. " });

                vehicle.Kilometers = dto.Kilometers;

                VehicleKmsDate kmsLog = new VehicleKmsDate
                {
                    VehicleId = vehicle.Id,
                    Date = DateTime.UtcNow,
                    Kilometers = vehicle.Kilometers
                };

                _context.VehicleKmsDates.Add(kmsLog);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Kilometraje del vehículo actualizado correctamente."});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al actualizar los kilómetros del vehículo.", detail = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeactivateVehicle(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);

                if (vehicle == null)
                    return NotFound(new { message = "Vehículo no encontrado." });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (vehicle.UserId != userId)
                    return StatusCode(403, new { error = "El vehículo no pertenece al usuario autenticado." });

                if (!vehicle.IsActive)
                    return BadRequest(new { message = "El vehículo ya está desactivado." });

                vehicle.IsActive = false;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Vehículo desactivado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al desactivar el vehículo.", detail = ex.Message });
            }
        }
    }
}
