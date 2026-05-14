using TrafficOfficerApi.Models;

namespace TrafficOfficerApi.DTOs;

public class AccidentDto
{
    public int Id { get; set; }
    public DateTime DateReported { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string InvolvedPlates { get; set; } = string.Empty;
}

public class CreateAccidentDto
{
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AccidentSeverity Severity { get; set; }
    public string InvolvedPlates { get; set; } = string.Empty;
}

public class UpdateAccidentDto
{
    public string Description { get; set; } = string.Empty;
    public AccidentSeverity Severity { get; set; }
}
