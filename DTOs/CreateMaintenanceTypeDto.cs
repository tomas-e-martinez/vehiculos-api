using System.ComponentModel.DataAnnotations;

namespace vehiculos_api.DTOs
{
    public class CreateMaintenanceTypeDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public int DefaultKmInterval { get; set; }
        public int DefaultMonthInterval { get; set; }
        [Required]
        public int[] VehicleTypeIds { get; set; }
    }
}
