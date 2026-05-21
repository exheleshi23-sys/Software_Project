namespace PoliceFuncBackend.Models
{
    public class Evidence
    {
        public int id { get; set; }

        public string evidence_number { get; set; } = string.Empty;

        public int case_id { get; set; }

        public string? evidence_type { get; set; }

        public string description { get; set; } = string.Empty;

        public DateTime collection_date { get; set; }

        public string? status { get; set; }

        public string? collected_by { get; set; }

        public string? analyzed_by { get; set; }

        public string? chain_of_custody { get; set; }
    }
}