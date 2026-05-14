namespace TrafficOfficerApi.DTOs;

public class VehicleLookupDto
{
    public string LicensePlate { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string RegistrationStatus { get; set; } = string.Empty;
}
