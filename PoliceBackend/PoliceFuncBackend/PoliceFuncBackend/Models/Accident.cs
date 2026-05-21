namespace PoliceFuncBackend.Models
{
    public class Accident
    {
        public int Accident_ID { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime AccidentDate { get; set; }

        public string AccidentTime { get; set; } = string.Empty;

        public int Severity { get; set; }

        public string Status { get; set; } = "Open";

        public string? USER_ID { get; set; }
    }
}