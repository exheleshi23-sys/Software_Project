namespace PoliceFuncBackend.Models
{
    public class Case
    {
        public int Case_ID { get; set; }

        public string Case_Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime OpenDate { get; set; }

        public DateTime CloseDate { get; set; }

        public string? Priority { get; set; }
    }
}