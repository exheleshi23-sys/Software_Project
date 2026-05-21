using Microsoft.AspNetCore.Http;

namespace PoliceFuncBackend.DTOs
{
    public class FineDto
    {
        public int Fine_ID { get; set; }

        public int Amount { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime DueDate { get; set; }

        public string FineStatus { get; set; } = string.Empty;

        public string? USER_ID { get; set; }

        public int Violation_ID { get; set; }
    }

    public class CreateFineDto
    {
        public int Amount { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime DueDate { get; set; }

        public string FineStatus { get; set; } = "unpaid";

        public int Violation_ID { get; set; }

        public IFormFile? Photo { get; set; }
    }
}