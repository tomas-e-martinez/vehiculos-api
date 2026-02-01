using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vehiculos_api.Data;
using vehiculos_api.DTOs;
using vehiculos_api.Model;

namespace vehiculos_api.Controller
{
    [Route("maintenance-types")]
    [ApiController]
    public class MaintenanceTypesController : ControllerBase
    {
        private readonly VehicleContext _context;

        public MaintenanceTypesController(VehicleContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetMaintenanceTypes()
        {
            try
            {
                var maintenanceTypes = await _context.MaintenanceTypes
                    .Select(mt => new
                    {
                        mt.Id,
                        mt.Name,
                        mt.Description,
                        mt.DefaultKmInterval,
                        mt.DefaultMonthInterval,
                        VehicleTypes = mt.VehicleTypes.Select(vt => new
                        {
                            vt.Id,
                            vt.Name
                        })
                    })
                    .ToListAsync();

                return Ok(maintenanceTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener los tipos de mantenimiento.", detail = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateMaintenanceType([FromBody] CreateMaintenanceTypeDto dto)
        {
            try
            {
                var vehicleTypes = await _context.VehicleTypes
                    .Where(vt => dto.VehicleTypeIds.Contains(vt.Id))
                    .ToListAsync();

                if (vehicleTypes.Count != dto.VehicleTypeIds.Length)
                    return BadRequest(new { error = "Uno o más tipos de vehículo seleccionados son inválidos." });

                var newMaintenanceType = new MaintenanceType
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    DefaultKmInterval = dto.DefaultKmInterval,
                    DefaultMonthInterval = dto.DefaultMonthInterval,
                    VehicleTypes = vehicleTypes
                };

                _context.MaintenanceTypes.Add(newMaintenanceType);
                await _context.SaveChangesAsync();

                return StatusCode(201, new { message = "Tipo de mantenimiento creado correctamente.", id = newMaintenanceType.Id });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al crear el tipo de mantenimiento.", detail = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> SoftDeleteMaintenanceType(int id)
        {
            try
            {
                var maintenanceType = await _context.MaintenanceTypes.Where(mt => mt.Id == id).FirstOrDefaultAsync();

                if (maintenanceType == null || !maintenanceType.IsActive)
                    return NotFound("No se encontró el tipo de mantenimiento a dar de baja.");

                maintenanceType.IsActive = false;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Tipo de mantenimiento dado de baja correctamente. " });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al dar de baja el tipo de mantenimiento.", detail = ex.Message });
            }
        }
    }
}
