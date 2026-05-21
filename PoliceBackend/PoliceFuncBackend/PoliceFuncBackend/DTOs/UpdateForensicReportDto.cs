namespace PoliceFuncBackend.DTOs
{
    public class UpdateForensicReportDto
    {
        public string Investigation_Status { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Evidence_Analysis { get; set; } = string.Empty;

        public string Suspect_Assessment { get; set; } = string.Empty;

        public string Investigative_Conclusions { get; set; } = string.Empty;

        public string Evidence { get; set; } = string.Empty;
    }
}