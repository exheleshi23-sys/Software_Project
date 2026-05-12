namespace PoliceFuncBackend.DTOs
{
    public class DepartmentDto
    {
        public int Department_ID { get; set; }
        public required string Department_Name { get; set; }
        public required string Description { get; set; }
    }
}