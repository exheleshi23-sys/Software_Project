using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficOfficerApi.Models;

[Table("traffic_fine")]
public class Fine
{
    [Key]
    [Column("Fine_ID")]
    public int Id { get; set; }

    [Column("Amount")]
    public int Amount { get; set; }

    [Column("IssueDate")]
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;

    [Column("DueDate")]
    public DateTime DueDate { get; set; }

    // This maps to 'FineStatus' in your DB. 
    // Logic in CitizenService should check for "Paid" or "Pending"
    [Column("FineStatus")]
    public string FineStatus { get; set; } = "Pending";

    // This links the fine to the Citizen.
    // Ensure your CitizenService uses 'UserId' to filter results.
    [Column("User_ID")]
    public int UserId { get; set; }

    [Column("Violation_ID")]
    public int ViolationId { get; set; }

    // We add this helper property so the CitizenService doesn't break.
    // It is "NotMapped", meaning it won't try to create a column in MariaDB.
    [NotMapped]
    public bool IsPaid => FineStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase);
}