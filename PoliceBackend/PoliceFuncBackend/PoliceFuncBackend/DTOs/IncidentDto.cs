namespace PoliceFuncBackend.DTOs
{
    public class IncidentDto
    {
        public required int Report_ID { get; set; }

        public required string Type { get; set; }

        public required string Location { get; set; }

        public required string Description { get; set; }

        public required string Date { get; set; }

        public required string Time { get; set; }

        public string? Arrest_Record { get; set; }

        public required int Officer_ID { get; set; }

        public int? Case_ID { get; set; }
    }
}