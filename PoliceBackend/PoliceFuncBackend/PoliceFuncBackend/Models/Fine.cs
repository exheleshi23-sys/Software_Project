namespace PoliceFuncBackend.Models
{
    public class Fine
    {
        public int Fine_ID { get; set; }

        public int Amount { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime DueDate { get; set; }

        public string FineStatus { get; set; } = "Pending";

        public string? USER_ID { get; set; }

        public int Violation_ID { get; set; }

        public bool IsPaid =>
            FineStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase);
    }
}