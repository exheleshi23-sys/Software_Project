namespace PoliceFuncBackend.DTOs
{
    public class UpdateForensicReportDto
    {
        public string InvestigationStatus { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string EvidenceAnalysis { get; set; } = string.Empty;
        public string SuspectAssessment { get; set; } = string.Empty;
        public string InvestigativeConclusions { get; set; } = string.Empty;
        public string Evidence { get; set; } = string.Empty;
    }
}