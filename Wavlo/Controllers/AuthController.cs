using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Wavlo.Data;
using Wavlo.DTOs;
using Wavlo.MailService;
using Wavlo.Models;
using Wavlo.OTPValidation;
using Wavlo.Services;

namespace Wavlo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _user;
        private readonly IEmailSender _emailSender;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IAuthService _authService;
        private readonly IFileService _fileService;
        public AuthController(UserManager<User> user, IEmailSender emailSender
        , IOptions<JwtSettings> jwtSettings, IAuthService authService, IFileService fileService)
        {
            _user = user;
            _emailSender = emailSender;
            _jwtSettings = jwtSettings;
            _authService = authService;
            _fileService = fileService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            var user = await _authService.LoginAsync(login);

            if (user.IsSucceeded)
            {
                return Ok(user);
            }

            return BadRequest(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _authService.RegisterAsync(registerDto);

            if (res.IsSucceeded)
                return Ok(res);

            return BadRequest(res);
        }

        [HttpPost("forget-Password")]
        public async Task<IActionResult> GenerateOTP([FromBody] GenerateOtpRequest otpRequest)
        {
            var user = await _user.FindByEmailAsync(otpRequest.Email);
            if (user == null)
            {
                return NotFound("User Not Found !");
            }

            var code = new Random().Next(100000, 999999).ToString();
            user.VerificationCode = code;
            user.ExpirationCode = DateTime.UtcNow.AddMinutes(10);

            await _user.UpdateAsync(user);

            var enumerable = new List<string> { otpRequest.Email };
            var message = new EmailMessage(enumerable, "Otp For Reset Password", HtmlTemplate.GetVerificationCodeEmailTemplate(code));

            _emailSender.SendEmailAsync(message);

            return Ok(code);
        }
        [HttpPost("validate-otp")]
        public async Task<IActionResult> ValidateOtp([FromBody] ValidateOtpRequest request)
        {
            var user = await _user.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound("User Not Found !");
            }

            if (user.VerificationCode == request.Otp && user.ExpirationCode > DateTime.UtcNow)
            {
                var token = await _user.GeneratePasswordResetTokenAsync(user);
                user.VerificationCode = null;
                user.ExpirationCode = null;
                await _user.UpdateAsync(user);
                return Ok(token);
            }

            return BadRequest("Invalid OR Expired OTP.");


        }
        [HttpPost("reset-Password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            var res = await _authService.ResetPasswordAsync(resetPassword);

            if (res.IsSucceeded)
                return Ok();

            return BadRequest(new { res.Message });

        }


            //[HttpPost("authenticate")]
            //public IActionResult Authenticate([FromBody] LoginRequestModel login)
            //{
            //    var user = _context.Users.SingleOrDefault(u => u.Username == login.Username);
            //    if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            //        return Unauthorized();

            //    var token = GenerateJwtToken(user.Id);
            //    return Ok(new { token });
            //}



            //    //var token = GenerateJwtToken(user.Id , user.Username);

            //   // return Ok(new { token });
            //} 
    }
}
