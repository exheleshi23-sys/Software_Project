namespace PoliceFuncBackend.Models
{
    public class Vehicle
    {
        public int Vehicle_ID { get; set; }

        public int PlateNumber { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int RegistrationNumber { get; set; }

        public string RegistratiionStatus { get; set; } = string.Empty;

        public string? USER_ID { get; set; }
    }
}