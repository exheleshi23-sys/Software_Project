namespace TrafficOfficerApi.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;    // Example: Toyota
    public string Model { get; set; } = string.Empty;   // Example: Corolla
    public string Color { get; set; } = string.Empty;
    public int Year { get; set; }
}