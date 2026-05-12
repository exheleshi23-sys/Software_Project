namespace PoliceFuncBackend.DTOs
{
    public class EvidenceCreateDto
    {
        public required string Evidence_Number { get; set; }
        public required int Case_ID { get; set; }
        public required string Evidence_Type { get; set; }
        public required string Description { get; set; }
        public required DateTime Collection_Date { get; set; }

        public int? Collected_By { get; set; }
    }
}