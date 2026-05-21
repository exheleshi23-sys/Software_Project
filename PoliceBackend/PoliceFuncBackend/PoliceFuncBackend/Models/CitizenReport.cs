namespace PoliceFuncBackend.Models
{
    public class CitizenReport
    {
        public int Id { get; set; }

        public string CitizenId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime ReportedAt { get; set; }

        public string Status { get; set; } = "Pending";
    }
}