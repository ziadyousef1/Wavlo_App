using Azure.Core;
using Microsoft.AspNetCore.Authentication;
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
        private readonly ITokenService _tokenService;
        private readonly ChatDbContext _context;
        public AuthController(UserManager<User> user, IEmailSender emailSender
        , IOptions<JwtSettings> jwtSettings, IAuthService authService, IFileService fileService, ITokenService tokenService, ChatDbContext context)
        {
            _user = user;
            _emailSender = emailSender;
            _jwtSettings = jwtSettings;
            _authService = authService;
            _fileService = fileService;
            _tokenService = tokenService;
            _context = context;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            var result = await _authService.LoginAsync(login);

            if (!result.IsSucceeded)
                return BadRequest(result);


            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(30),
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("refreshToken", result.RefreshToken!, cookieOptions);


            return Ok(new
            {
                token = result.Token,
                refreshToken = result.RefreshToken
            });
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

            var token = await _user.GeneratePasswordResetTokenAsync(user);

            var message = new EmailMessage(
                new List<string> { otpRequest.Email },
                "OTP For Reset Password",
                HtmlTemplate.GetVerificationCodeEmailTemplate(code));

            await _emailSender.SendEmailAsync(message);

            return Ok("OTP Sent Successfully :)");
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
                return Ok("OTP Verified Successfully !");
            }

            return BadRequest("Invalid OR Expired OTP.");


        }
        [HttpPost("reset-Password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            var res = await _authService.ResetPasswordAsync(resetPassword);

            if (res.IsSucceeded)
                return Ok();

            return BadRequest(new
            {
                message = res.Message,
                errors = res.Errors
            });

        }
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAndDeleteAccount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User not authenticated.");

            var userId = userIdClaim.Value;

            var user = await _user.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");


            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Refresh token is missing.");

            var storedToken = await _tokenService.GetRefreshToken(refreshToken);
            if (storedToken == null || storedToken.IsRevoked || storedToken.IsExpired)
                return Unauthorized("Invalid or expired refresh token.");


            await _tokenService.RevokeRefreshToken(storedToken.Token);


            Response.Cookies.Delete("refreshToken");


            var chatUsers = _context.ChatUsers.Where(c => c.UserId == user.Id);
            _context.ChatUsers.RemoveRange(chatUsers);
            await _context.SaveChangesAsync();


            var result = await _user.DeleteAsync(user);
            if (!result.Succeeded)
                return StatusCode(500, "Failed to delete the account.");

            return Ok("Account deleted and logged out successfully.");
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Refresh token is missing.");

            var storedToken = await _tokenService.GetRefreshToken(refreshToken);
            if (storedToken == null || storedToken.IsRevoked || storedToken.IsExpired)
                return Unauthorized("Invalid or expired refresh token.");

            var user = storedToken.User;
            if (user == null)
                return Unauthorized("User not found.");


            var newRefreshToken = await _tokenService.RotateRefreshToken(storedToken);
            var newAccessToken = await _tokenService.GenerateJwtToken(user);


            Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = newRefreshToken.ExpiresOn
            });


            return Ok(new
            {
                token = newAccessToken,
                refreshToken = newRefreshToken.Token
            });
        }

    }
}
