using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoliceFuncBackend.Models
{
    [Table("case_notes")]
    public class CaseNote
    {
        [Key]
        [Column("Note_ID")]
        public int NoteId { get; set; }

        [Column("Case_ID")]
        public int CaseId { get; set; }

        [Column("Note_Text")]
        public string NoteText { get; set; } = string.Empty;

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }
    }
}