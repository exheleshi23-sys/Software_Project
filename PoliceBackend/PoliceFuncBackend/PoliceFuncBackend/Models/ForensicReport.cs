namespace PoliceFuncBackend.Models
{
    public class ForensicReport
    {
        public int Investigation_ID { get; set; }

        public string Investigation_Status { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Evidence_analysis { get; set; } = string.Empty;

        public string Suspect_assessment { get; set; } = string.Empty;

        public string Investigative_Conclusions { get; set; } = string.Empty;

        public string Evidence { get; set; } = string.Empty;

        public int Report_ID { get; set; }

        public string? Detective_ID { get; set; }
    }
}