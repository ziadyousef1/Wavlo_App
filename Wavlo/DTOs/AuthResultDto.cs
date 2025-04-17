namespace Wavlo.DTOs
{
    public class AuthResultDto
    {
        public string Message { get; set; }
        public bool IsSucceeded { get; set; }

        public string Email { get; set; }

        public string Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
