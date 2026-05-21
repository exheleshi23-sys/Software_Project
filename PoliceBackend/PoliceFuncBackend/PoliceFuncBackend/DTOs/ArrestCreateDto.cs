namespace PoliceFuncBackend.DTOs
{
    public class ArrestCreateDto
    {
        public required string Arrest_Number { get; set; }

        public required String Suspect_ID { get; set; }
        public required int Case_ID { get; set; }
        public required String Arresting_Officer_ID { get; set; }

        public required DateTime Arrest_Date { get; set; }

        public required string Charges { get; set; }

        public string? Status { get; set; }
    }
}
