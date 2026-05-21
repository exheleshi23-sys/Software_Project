namespace PoliceFuncBackend.DTOs
{
    public class AccidentDto
    {
        public int Accident_ID { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime AccidentDate { get; set; }

        public string AccidentTime { get; set; } = string.Empty;

        public int Severity { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? USER_ID { get; set; }
    }

    public class CreateAccidentDto
    {
        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime AccidentDate { get; set; }

        public string AccidentTime { get; set; } = string.Empty;

        public int Severity { get; set; }

        public string Status { get; set; } = "open";

        public string? USER_ID { get; set; }
    }

    public class UpdateAccidentDto
    {
        public string Description { get; set; } = string.Empty;

        public int Severity { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}