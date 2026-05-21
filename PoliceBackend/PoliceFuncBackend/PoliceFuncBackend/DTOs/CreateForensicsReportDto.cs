namespace PoliceFuncBackend.DTOs
{
    public class CreateForensicReportDto
    {
        public int Investigation_ID { get; set; }

        public string Investigation_Status { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Evidence_Analysis { get; set; } = string.Empty;

        public string Suspect_Assessment { get; set; } = string.Empty;

        public string Investigative_Conclusions { get; set; } = string.Empty;

        public string Evidence { get; set; } = string.Empty;

        public int Report_ID { get; set; }

        public string Detective_ID { get; set; } = string.Empty;
    }
}