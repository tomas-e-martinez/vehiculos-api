namespace vehiculos_api.Model
{
    public class VehicleKmsDate
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public DateTime Date { get; set; }
        public int Kilometers { get; set; }
    }
}
