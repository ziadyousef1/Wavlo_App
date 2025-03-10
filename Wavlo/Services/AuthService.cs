using Wavlo.DTOs;

namespace Wavlo.Services
{
    public class AuthService : IAuthService
    {
        public Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
        {
            throw new NotImplementedException();
        }

        public Task<ResetPasswordResultDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            throw new NotImplementedException();
        }
    }
}
