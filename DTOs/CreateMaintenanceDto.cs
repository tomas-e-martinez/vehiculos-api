using System.ComponentModel.DataAnnotations;

namespace vehiculos_api.DTOs
{
    public class CreateMaintenanceDto
    {
        [Required]
        public int? MaintenanceTypeId { get; set; }
        public int? KmTarget { get; set; }
        public DateTime? DateTarget { get; set; }
    }
}
