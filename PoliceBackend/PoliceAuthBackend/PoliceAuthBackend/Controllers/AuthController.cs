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

        private readonly IConfiguration _config;

        public AuthController(
            AuthService auth,
            EmailService email,
            IConfiguration config)
        {
            _auth = auth;

            _email = email;

            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register(
            RegisterRequest req)
        {
            bool success =
                _auth.RegisterUser( req);

            if (!success)
            {
                return BadRequest(
                    "User ID or Email already exists"
                );
            }

            return Ok(
                "User registered successfully"
            );
        }

        [HttpPost("login")]
        public IActionResult Login(
            LoginRequest req)
        {
            var user =
                _auth.ValidateUser(
                    req.UserId,
                    req.Password
                );

            if (user == null)
            {
                return Unauthorized(
                    "Invalid credentials"
                );
            }

            if (user.Status.ToLower() == "suspended")
            {
                return Unauthorized(
                    "Account suspended"
                );
            }

            string code =
                _auth.GenerateOtp();

            _auth.SaveOtp(
                user.User_ID,
                code
            );

            try
            {
                _email.SendOtp(
                    user.Email,
                    code
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "Email failed",
                    Error = ex.ToString()
                });
            }

            return Ok(new
            {
                message = "OTP sent",
                userId = user.User_ID
            });
        }

        [HttpPost("verify")]
        public IActionResult Verify(
            VerifyOtpRequest req)
        {
            bool valid =
                _auth.VerifyOtp(
                    req.User_Id,
                    req.Code
                );

            if (!valid)
            {
                return Unauthorized(
                    "Invalid OTP"
                );
            }

            var user =
                _auth.GetUserById(
                    req.User_Id
                );

            if (user == null)
            {
                return BadRequest(
                    "User not found"
                );
            }

            string token =
                _auth.GenerateJwtToken(
                    user,
                    _config
                );

            return Ok(new
            {
                message = "Login successful",

                token = token,

                user = new
                {
                    id = user.User_ID,

                    name = user.Name,

                    surname = user.Surname,

                    email = user.Email,

                    role =
                        user.Role_ID,

                    department =
                        user.Department_ID
                }
            });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(
            ForgotPasswordRequest req)
        {
            var user =
                _auth.GetUserByEmailOrUsername(
                    req.EmailOrUsername
                );

            if (user == null)
            {
                return BadRequest(
                    "User not found"
                );
            }

            string code =
                _auth.GenerateOtp();

            _auth.SaveOtp(
                user.User_ID,
                code
            );

            try
            {
                _email.SendOtp(
                    user.Email,
                    code
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "Email failed",
                    Error = ex.ToString()
                });
            }

            return Ok(
                "Reset code sent"
            );
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword(
            ResetPasswordRequest req)
        {
            var user =
                _auth.GetUserByEmailOrUsername(
                    req.EmailOrUsername
                );

            if (user == null)
            {
                return BadRequest(
                    "User not found"
                );
            }

            bool valid =
                _auth.VerifyOtp(
                    user.User_ID,
                    req.Code
                );

            if (!valid)
            {
                return Unauthorized(
                    "Invalid code"
                );
            }

            _auth.UpdatePassword(
                user.User_ID,
                req.NewPassword
            );

            return Ok(
                "Password updated"
            );
        }
    }
}