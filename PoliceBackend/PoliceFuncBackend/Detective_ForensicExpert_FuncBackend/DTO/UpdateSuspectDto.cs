namespace PoliceFuncBackend.DTOs
{
    public class UpdateSuspectDto
    {
        public string Evidence { get; set; } = string.Empty;
        public int InvestigationId { get; set; }
    }
}