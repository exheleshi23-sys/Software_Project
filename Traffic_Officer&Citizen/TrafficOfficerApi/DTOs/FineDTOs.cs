namespace TrafficOfficerApi.DTOs;

public class FineDto
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string OffenseType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DateIssued { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}

public class CreateFineDto
{
    public string LicensePlate { get; set; } = string.Empty;
    public string OffenseType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public IFormFile? Photo { get; set; } // For uploading the image
}
