namespace PoliceAuthBackend.Models
{
    public class LoginOtp
    {
        public int Id { get; set; }

        public int User_ID { get; set; }

        public string Code { get; set; }

        public DateTime Expiry {  get; set; }

        public bool IsUsed { get; set; }


    }

}
