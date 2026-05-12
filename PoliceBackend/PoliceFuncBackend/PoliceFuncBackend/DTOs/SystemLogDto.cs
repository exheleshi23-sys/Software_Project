namespace PoliceFuncBackend.DTOs
{
    public class SystemLogDto
    {
        public int Log_ID { get; set; }

        public int User_ID { get; set; }

        public required string Action { get; set; }

        public required string Table_Affected { get; set; }

        public DateTime Created_At { get; set; }
    }
}