namespace PoliceFuncBackend.DTOs
{
    public class UpdateUserDto
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Email { get; set; }
        public required string Phone_Number { get; set; }
        public required string Address { get; set; }
    }
}