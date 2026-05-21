namespace PoliceFuncBackend.DTOs
{
    public class CreateSuspectDto
    {
        public string Evidence { get; set; } = string.Empty;

        public int Investigation_ID { get; set; }
    }
}