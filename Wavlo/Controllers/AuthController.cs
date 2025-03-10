using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Wavlo.Data;
using Wavlo.Models;

namespace Wavlo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController: ControllerBase
    {
        private readonly ChatDbContext _context;
        private readonly JwtSettings _jwtSettings;
        public AuthController(ChatDbContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
            
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            //if (_context.Users.Any(u => u.Username == user.Username))
            //    return BadRequest(new { message = "Username already exists!" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User registered successfully!" });
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
        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginRequestModel login)
        //{
        //   // var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == login.Username);

        //    //if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
        //    //{
        //    //    return Unauthorized(new { message = "Invalid username or password" });
        //    //}


        //    //var token = GenerateJwtToken(user.Id , user.Username);

        //   // return Ok(new { token });
        //} 

        



    }
}
