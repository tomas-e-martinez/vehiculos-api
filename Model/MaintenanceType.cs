namespace vehiculos_api.Model
{
    public class MaintenanceType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DefaultKmInterval { get; set; }
        public int DefaultMonthInterval { get; set; }
        public ICollection<VehicleType> VehicleTypes { get; set; } = [];
    }
}
