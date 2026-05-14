using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoliceFuncBackend.Models
{
    [Table("investigation_report")]
    public class ForensicReport
    {
        [Key]
        [Column("Investigation_ID")]
        public int InvestigationId { get; set; }

        [Column("Investigation_Status")]
        public string InvestigationStatus { get; set; } = string.Empty;

        [Column("Summary")]
        public string Summary { get; set; } = string.Empty;

        [Column("Evidence_analysis")]
        public string EvidenceAnalysis { get; set; } = string.Empty;

        [Column("Suspect_assessment")]
        public string SuspectAssessment { get; set; } = string.Empty;

        [Column("Investigative_Conclusions")]
        public string InvestigativeConclusions { get; set; } = string.Empty;

        [Column("Evidence")]
        public string Evidence { get; set; } = string.Empty;

        [Column("Report_ID")]
        public int ReportId { get; set; }

        [Column("Detective_ID")]
        public int DetectiveId { get; set; }
    }
}