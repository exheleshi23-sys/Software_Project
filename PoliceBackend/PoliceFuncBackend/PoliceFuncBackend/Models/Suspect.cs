namespace PoliceFuncBackend.Models
{
    public class Suspect
    {
        public int Suspect_ID { get; set; }

        public string Evidence { get; set; } = string.Empty;

        public int Investigation_ID { get; set; }
    }
}