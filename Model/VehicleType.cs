namespace vehiculos_api.Model
{
    public class VehicleType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<MaintenanceType> MaintenanceTypes { get; set; } = [];
    }
}
