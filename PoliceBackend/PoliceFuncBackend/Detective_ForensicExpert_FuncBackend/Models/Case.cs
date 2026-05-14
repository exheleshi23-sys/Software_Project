using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoliceFuncBackend.Models
{
    [Table("cases")]
    public class Case
    {
        [Key]
        [Column("Case_ID")]
        public int CaseId { get; set; }

        [Column("Case_Type")]
        public string CaseType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime OpenDate { get; set; }

        public DateTime CloseDate { get; set; }

        public string? Priority { get; set; }
    }
}