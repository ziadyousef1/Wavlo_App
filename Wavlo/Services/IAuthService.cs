using Wavlo.DTOs;

namespace Wavlo.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
        Task<ResetPasswordResultDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        Task<bool> LogoutAsync(string userId, string refreshToken);
    }
}
