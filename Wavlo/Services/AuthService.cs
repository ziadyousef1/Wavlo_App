using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Wavlo.DTOs;
using Wavlo.MailService;
using Wavlo.Models;

namespace Wavlo.Services
{
    public class AuthService : IAuthService
    {

        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _user;
        private readonly IEmailSender _emailSender;
        public readonly JwtSettings _jwtSettings;
        private readonly IFileService _fileService;

        public AuthService(ITokenService tokenService, UserManager<User> user, IEmailSender emailSender
        , IOptions<JwtSettings> jwtSettings , IFileService fileService)
        {
            _tokenService = tokenService;
            _user = user;
            _emailSender = emailSender;
            _fileService = fileService;
        }
        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            var authRes = new AuthResultDto();
            var user = await _user.FindByEmailAsync(loginDto.Email);
            if (user is null || !await _user.CheckPasswordAsync(user, loginDto.Password))
            {
                authRes.Message = "Invalid Email or Password";
                return authRes;
            }
            authRes.Token = await _tokenService.GenerateJwtToken(user);
            authRes.IsSucceeded = true;
            return authRes;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
        {
            var authResult = new AuthResultDto();

            try
            {
                if (await _user.FindByEmailAsync(registerDto.Email) != null)
                {
                    authResult.Message = "Email already exists";
                    return authResult;
                }

                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    UserName = registerDto.Email
                };

                //if (registerDto.ProfileImage != null)
                //{
                //   var imageUrl = await _fileService.UploadFileAsync(registerDto.ProfileImage);
                //    user.UserImages.Add(new UserImage { ImageUrl = imageUrl });
                //}

                var result = await _user.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    authResult.Message = "User creation failed: " + string.Join("\n", result.Errors.Select(e => e.Description));
                    return authResult;
                }
                if (registerDto.ProfileImage != null)
                {
                    var imageUrl = await _fileService.UploadFileAsync(registerDto.ProfileImage);
                    user.UserImages.Add(new UserImage { ImageUrl = imageUrl });
                    await _user.UpdateAsync(user);
                }

                var code = new Random().Next(100000, 999999).ToString();
                user.VerificationCode = code;
                user.ExpirationCode = DateTime.UtcNow.AddMinutes(10);
                await _user.UpdateAsync(user);

                
                var verificationCodeEmail = HtmlTemplate.GetVerificationCodeEmailTemplate(code);
                var emailMessage = new EmailMessage(new[] { user.Email }, "Verification Code", verificationCodeEmail);
                _emailSender.SendEmailAsync(emailMessage);

                authResult.IsSucceeded = true;
                authResult.Message = "User registered successfully. Verification code sent to email.";
                return authResult;
            }
            catch (Exception ex)
            {
                authResult.Message = $"An error occurred while registering the user: {ex.InnerException?.Message ?? ex.Message}";
                return authResult;
            }
        }

        public async Task<ResetPasswordResultDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var resultDto = new ResetPasswordResultDto();

            var user = await _user.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                resultDto.Message = "User not found.";
                return resultDto;
            }

            var oldPassword = user.PasswordHash;
            var result = await _user.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.confirmPassword);
            var newPassword = user.PasswordHash;

            if (result.Succeeded)
            {
                resultDto.IsSucceeded = true;
                return resultDto;
            }

            resultDto.Message = "Password reset failed.";

            return resultDto;
        }
        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _user.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            
            var result = await _user.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}
