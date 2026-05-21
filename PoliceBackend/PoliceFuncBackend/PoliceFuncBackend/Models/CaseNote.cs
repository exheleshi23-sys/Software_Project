namespace PoliceFuncBackend.Models
{
    public class CaseNote
    {
        public int Note_ID { get; set; }

        public int Case_ID { get; set; }

        public string Note_Text { get; set; } = string.Empty;

        public DateTime Created_At { get; set; }
    }
}