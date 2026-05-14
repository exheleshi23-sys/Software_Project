namespace PoliceAuthBackend.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int Role_ID { get; set; }

        public int Department_ID { get; set; }
    }
}