using System;

namespace CrimeMapApi.Models
{
    public class CrimeRecord
    {
        public int Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
