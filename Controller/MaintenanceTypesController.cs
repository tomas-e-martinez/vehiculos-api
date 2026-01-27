using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vehiculos_api.Data;

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
    }
}
