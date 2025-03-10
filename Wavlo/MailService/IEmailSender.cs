namespace Wavlo.MailService
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailMessage message);
    }
}
