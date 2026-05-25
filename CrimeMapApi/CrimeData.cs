using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace CrimeMapApi.Models
{
  [Keyless]
    public class CrimeData
    {
        public string Location { get; set; } = string.Empty;
        public int Crime_Count { get; set; }
        public double StreetLighting { get; set; }
        public double CommunityPatrols { get; set; }
    }
}
