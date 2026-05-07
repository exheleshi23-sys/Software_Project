using System.Net;
using System.Net.Mail;

namespace PoliceAuthBackend.Services
{
    public class EmailService
    {
        public void SendOtp(string email, string code)
        {
            try
            {
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);

                client.Credentials = new NetworkCredential(
                    "ahazizi2005@gmail.com",
                    "qmdwmgrdbrwocupc"
                    );

                client.EnableSsl = true;

                MailMessage msg = new MailMessage();

                msg.From = new MailAddress("ahazizi2005@gmail.com");

                msg.To.Add(email);

                msg.Subject = "Your Login OTP";

                msg.Body = $"Your login code is {code}";

                client.Send(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

