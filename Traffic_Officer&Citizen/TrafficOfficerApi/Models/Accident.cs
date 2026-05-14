using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficOfficerApi.Models;

[Table("traffic_accident")]
public class Accident
{
    [Key]
    [Column("Accident_ID")]
    public int Id { get; set; }

    [Column("Location")]
    public string Location { get; set; } = string.Empty;

    [Column("Description")]
    public string Description { get; set; } = string.Empty;

    [Column("AccidentDate")]
    public DateTime AccidentDate { get; set; } = DateTime.UtcNow;

    [Column("AccidentTime")]
    public string AccidentTime { get; set; } = string.Empty;

    [Column("Severity")]
    public int Severity { get; set; }

    [Column("Status")]
    public string Status { get; set; } = "Open";

    [Column("User_ID")]
    public int UserId { get; set; }
}