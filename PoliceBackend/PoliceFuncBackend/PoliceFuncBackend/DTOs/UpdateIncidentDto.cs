namespace PoliceFuncBackend.DTOs
{
    public class UpdateIncidentDto
    {
        public required string Type { get; set; }

        public required string Location { get; set; }

        public required string Description { get; set; }

        public required string Date { get; set; }

        public required string Time { get; set; }

        public string? Arrest_Record { get; set; }
    }
}