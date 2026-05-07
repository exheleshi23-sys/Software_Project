using Microsoft.AspNetCore.Mvc;
using PoliceAuthBackend.Dtos;
using PoliceAuthBackend.Services;

namespace PoliceAuthBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;
        private readonly EmailService _email;

        public AuthController(
            AuthService auth,
            EmailService email)
        {
            _auth = auth;
            _email = email;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest req)
        {
            var user =
                _auth.ValidateUser(
                    req.Name,
                    req.Password
                );

            if (user == null)
                return Unauthorized("Invalid credentials");

            string code = _auth.GenerateOtp();

            _auth.SaveOtp(user.Id, code);


            try
            {
                _email.SendOtp(user.Email, code);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "Email failed",
                    error = ex.ToString()
                });
            }
            return Ok(new
            {
                message = "OTP sent",
                userId = user.Id
            });
        }

        [HttpPost("verify")]
        public IActionResult Verify(VerifyOtpRequest req)
        {
            bool valid =
                _auth.VerifyOtp(
                    req.User_Id,
                    req.Code
                );

            if (!valid)
                return Unauthorized("Invalid OTP");

            return Ok("Login successful");
        }
    }
}