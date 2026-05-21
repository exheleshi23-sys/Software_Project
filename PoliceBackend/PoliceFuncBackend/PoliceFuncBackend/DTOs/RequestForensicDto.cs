namespace PoliceFuncBackend.DTOs
{
    public class RequestForensicDto
    {
        public int Case_ID { get; set; }

        public int Evidence_ID { get; set; }

        public string Request_Type { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}