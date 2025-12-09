namespace vehiculos_api.DTOs
{
    public class CreateVehicleDto
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Kilometers { get; set; }
        public int VehicleTypeId { get; set; }
    }
}
