using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoliceFuncBackend.Models
{
    [Table("evidence")]
    public class Evidence
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("evidence_number")]
        public string EvidenceNumber { get; set; } = string.Empty;

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("evidence_type")]
        public string? EvidenceType { get; set; }

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("collection_date")]
        public DateTime CollectionDate { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("collected_by")]
        public int? CollectedBy { get; set; }

        [Column("analyzed_by")]
        public int? AnalyzedBy { get; set; }

        [Column("chain_of_custody")]
        public string? ChainOfCustody { get; set; }
    }
}