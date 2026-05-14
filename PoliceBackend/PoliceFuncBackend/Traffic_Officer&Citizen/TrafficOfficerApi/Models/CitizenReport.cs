namespace TrafficOfficerApi.Models;

public class CitizenReport
{
    public int Id { get; set; }
    public string CitizenId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Reviewed, Resolved
}