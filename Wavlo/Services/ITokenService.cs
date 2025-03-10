using Wavlo.Models;

namespace Wavlo.Services
{
    public interface ITokenService
    {
        public Task<string> GenerateJwtToken(User user);
    }
}
