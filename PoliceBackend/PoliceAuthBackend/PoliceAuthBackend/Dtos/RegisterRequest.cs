namespace PoliceAuthBackend.Dtos
{
    public class RegisterRequest
    {
        public String User_ID { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public string Surname { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string Password { get; set; }
            = string.Empty;

        public string Phone_Number { get; set; }
            = string.Empty;

        public string Address { get; set; }
            = string.Empty;

        public DateTime Birth_Date { get; set; }

        public string? ProfilePhoto { get; set; }
    }
}