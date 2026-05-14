using System.Net;
using System.Net.Mail;

namespace PoliceAuthBackend.Services
{
    public class EmailService
    {
        private readonly string _email =
            "ahazizi2005@gmail.com";

        private readonly string _password =
            "qmdwmgrdbrwocupc";

        public void SendOtp(string email, string code)
        {
            try
            {
                SmtpClient client =
                    new SmtpClient("smtp.gmail.com", 587);

                client.Credentials =
                    new NetworkCredential(
                        _email,
                        _password
                    );

                client.EnableSsl = true;

                MailMessage msg = new MailMessage();

                msg.From = new MailAddress(_email);

                msg.To.Add(email);

                msg.Subject = "Your OTP Code";

                msg.Body =
                    $"Your verification code is {code}";

                client.Send(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                throw;
            }
        }
    }
}