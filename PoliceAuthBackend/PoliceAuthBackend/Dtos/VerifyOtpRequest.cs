namespace PoliceAuthBackend.Dtos
{
    public class VerifyOtpRequest
    {
        public int User_Id { get; set; }

        public String Code { get; set; }
    }
}
