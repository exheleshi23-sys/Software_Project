using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoliceFuncBackend.Models
{
    [Table("suspect_list")]
    public class Suspect
    {
        [Key]
        [Column("Suspect_ID")]
        public int SuspectId { get; set; }

        public string Evidence { get; set; } = string.Empty;

        [Column("Investigation_ID")]
        public int InvestigationId { get; set; }
    }
}