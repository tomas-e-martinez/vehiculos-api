namespace vehiculos_api.Model
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int Kilometers { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int VehicleTypeId { get; set; }
        public VehicleType VehicleType { get; set; }
        public ICollection<MaintenanceTask> MaintenanceTasks { get; set; } = [];
    }
}
