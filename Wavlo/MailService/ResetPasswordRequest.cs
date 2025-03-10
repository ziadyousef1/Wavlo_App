namespace Wavlo.MailService
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }

        public string Token { get; set; }

        public string newPassword { get; set; }
    }
}
