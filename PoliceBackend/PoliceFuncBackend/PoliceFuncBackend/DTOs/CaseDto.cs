namespace PoliceFuncBackend.DTOs
{
    public class CaseDto
    {
        public required int Case_ID { get; set; }

        public required string Case_Type { get; set; }

        public required string Title { get; set; }

        public required string Description { get; set; }

        public required string Status { get; set; }

        public required string Priority { get; set; }
    }
}