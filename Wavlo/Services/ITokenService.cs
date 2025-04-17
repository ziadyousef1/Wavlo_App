using Wavlo.Models;

namespace Wavlo.Services
{
    public interface ITokenService
    {
        public Task<string> GenerateJwtToken(User user);
        string GenerateRefreshToken();
        Task SaveRefreshToken(User user, string refreshToken);
        Task<RefreshToken?> GetRefreshToken(string token);
        Task<RefreshToken> RotateRefreshToken(RefreshToken oldToken);
        Task RevokeRefreshToken(string token);
    }
}
