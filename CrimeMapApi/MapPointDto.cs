namespace CrimeMapApi.Models
{
    public class MapPointDto
    {
        public string? Location { get; set; }
        public int TotalCrimes { get; set; }
        public string? RiskLevel { get; set; }
    }
}
