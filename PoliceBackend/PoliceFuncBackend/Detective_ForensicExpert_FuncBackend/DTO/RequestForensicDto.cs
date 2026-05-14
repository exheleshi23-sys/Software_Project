namespace PoliceFuncBackend.DTOs
{
    public class RequestForensicDto
    {
        public int CaseId { get; set; }
        public int EvidenceId { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}