using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Wavlo.Data;
using Wavlo.Models;

namespace Wavlo.Services
{
    public class TokenService : ITokenService
    {
        private readonly ChatDbContext _context;
       private readonly UserManager<User> _userManager;
       private readonly JwtSettings _jwtSettings;
        public TokenService(IOptions<JwtSettings> jwtSettings , UserManager<User> userManager,ChatDbContext context)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _context = context;
        }
        public async Task<string> GenerateJwtToken(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), 
                new Claim(ClaimTypes.Email, user.Email ?? "") 
            };

            claims.AddRange(userClaims);
            claims.AddRange(roleClaims);


            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.DurationInDays),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key))
                , SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<RefreshToken?> GetRefreshToken(string token)
        {
            var refreshToken = await _context.RefreshTokens
             .Include(rt => rt.User)
             .FirstOrDefaultAsync(rt => rt.Token == token);

            return refreshToken;
        }

        public async Task RevokeRefreshToken(string token)
        {
            var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null || refreshToken.IsRevoked) return;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken> RotateRefreshToken(RefreshToken oldToken)
        {
            oldToken.IsRevoked = true;
            oldToken.RevokedOn = DateTime.UtcNow;

            var newToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(_jwtSettings.DurationInDays),
                IsRevoked = false,
                UserId = oldToken.UserId 
            };

            _context.RefreshTokens.Add(newToken); 
            await _context.SaveChangesAsync(); 

            return newToken;
        }

        public async Task SaveRefreshToken(User user, string refreshToken)
        {
            var token = new RefreshToken
            {
                Token = refreshToken,
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(_jwtSettings.DurationInDays),
                IsRevoked = false,
                UserId = user.Id 
            };

            user.RefreshTokens ??= new List<RefreshToken>();
            user.RefreshTokens.Add(token);

            await _context.SaveChangesAsync();
        }
    }
}
