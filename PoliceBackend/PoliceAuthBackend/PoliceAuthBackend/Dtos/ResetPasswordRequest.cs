namespace PoliceAuthBackend.Dtos
{
    public class ResetPasswordRequest
    {
        public string EmailOrUsername { get; set; }

        public string Code { get; set; }

        public string NewPassword { get; set; }
    }
}