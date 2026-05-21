namespace PoliceFuncBackend.DTOs
{
    public class UserDto
    {
        public String User_ID { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Phone_Number { get; set; }
        public required string Address { get; set; }
        public DateTime Birth_Date { get; set; }
        public int Role_ID { get; set; }
        public int Department_ID { get; set; }
    }
}