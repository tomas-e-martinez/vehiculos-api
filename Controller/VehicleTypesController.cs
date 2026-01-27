using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vehiculos_api.Data;

namespace vehiculos_api.Controller
{
    [Route("vehicle-types")]
    [ApiController]
    public class VehicleTypesController : ControllerBase
    {
        private readonly VehicleContext _context;

        public VehicleTypesController(VehicleContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetVehicleTypes()
        {
            try
            {
                var vehicleTypes = await _context.VehicleTypes
                    .Select(vt => new
                    {
                        vt.Id,
                        vt.Name
                    })
                    .ToListAsync();

                return Ok(vehicleTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener los tipos de vehículo.", detail = ex.Message });
            }
        }

        [HttpGet("{id}/maintenance-types")]
        public async Task<ActionResult> GetVehicleTypeMaintenanceTypes(int id)
        {
            try
            {
                var maintenanceTypes = await _context.MaintenanceTypes
                    .Where(mt => mt.VehicleTypes.Any(vt => vt.Id == id))
                    .Select(mt => new
                    {
                        mt.Id,
                        mt.Name,
                        mt.DefaultKmInterval,
                        mt.DefaultMonthInterval
                    })
                    .ToListAsync();

                return Ok(maintenanceTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener los tipos de mantenimiento.", detail = ex.Message });
            }
        }
    }
}
