namespace vehiculos_api.Model
{
    public class MaintenanceTask
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public int MaintenanceTypeId { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public int? KmTarget { get; set; }
        public DateTime? DateTarget { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CompletedKm { get; set; }
    }
}
