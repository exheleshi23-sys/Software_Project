namespace PoliceAuthBackend.Dtos
{
    public class VerifyOtpRequest
    {
        public String User_Id { get; set; }

        public String Code { get; set; }
    }
}
