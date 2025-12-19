using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using vehiculos_api.Data;
using vehiculos_api.DTOs;
using vehiculos_api.Model;

namespace vehiculos_api.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private readonly VehicleContext _context;

        public MaintenanceController(VehicleContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPatch("{maintenanceId}/complete")]
        public async Task<ActionResult> CompleteMaintenanceTask(int maintenanceId, [FromBody] CompleteMaintenanceDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var maintenanceTask = await _context.MaintenanceTasks
                    .Include(mt => mt.MaintenanceType)
                    .Include(mt => mt.Vehicle)
                    .FirstOrDefaultAsync(mt => mt.Id == maintenanceId);

                if (maintenanceTask == null)
                    return NotFound(new { message = "No se encontró la tarea de mantenimiento." });

                if (maintenanceTask.Vehicle.UserId != userId)
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "El vehículo indicado no es de su propiedad." });

                if (maintenanceTask.IsCompleted)
                    return BadRequest(new { message = "La tarea ya fue completada." });

                //MARCAR TAREA DE MANTENIMIENTO COMO COMPLETADA
                maintenanceTask.CompletedKm = dto.Kilometers;
                maintenanceTask.CompletedAt = dto.CompletedAt ?? DateTime.UtcNow;
                maintenanceTask.IsCompleted = true;

                //CREAR SIGUIENTE MANTENIMIENTO
                var maintenanceType = maintenanceTask.MaintenanceType;
                var newMaintenance = new MaintenanceTask
                {
                    VehicleId = maintenanceTask.VehicleId,
                    MaintenanceTypeId = maintenanceTask.MaintenanceTypeId,
                    KmTarget = maintenanceTask.CompletedKm + maintenanceType.DefaultKmInterval,
                    DateTarget = maintenanceTask.CompletedAt.Value.AddMonths(maintenanceType.DefaultMonthInterval),
                    IsCompleted = false,
                    CompletedAt = null,
                    CompletedKm = null
                };
                _context.MaintenanceTasks.Add(newMaintenance);

                //ACTUALIZAR KILOMETRAJE DEL VEHÍCULO SI CORRESPONDE
                var vehicle = maintenanceTask.Vehicle;
                if (dto.Kilometers > vehicle.Kilometers)
                {
                    vehicle.Kilometers = dto.Kilometers;
                    
                    var vehicleKmDate = new VehicleKmsDate
                    {
                        Date = maintenanceTask.CompletedAt.Value,
                        Kilometers = vehicle.Kilometers,
                        VehicleId = vehicle.Id
                    };
                    _context.VehicleKmsDates.Add(vehicleKmDate);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Mantenimiento completado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al completar la tarea de mantenimiento.", detail = ex.Message  });
            }
        }
    }
}
