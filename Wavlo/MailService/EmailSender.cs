using MimeKit;
using MimeKit.Text;
using System.Net.Mail;

namespace Wavlo.MailService
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfigration _emailConfiguration;
        public EmailSender(EmailConfigration emailConfigration)
        {
            _emailConfiguration = emailConfigration;
        }
        public async Task SendEmailAsync(EmailMessage message)
        {
            var emailMessage = CreateEmailMessage(message);
            await SendAsync(emailMessage);
        }

       private MimeMessage CreateEmailMessage(EmailMessage message)
       {
            var emailMessage = new MimeMessage();
            var verification = new Random().Next(500, 90000);

            emailMessage.From.Add(new MailboxAddress(_emailConfiguration.DisplayName, _emailConfiguration.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = message.Content
            };

            return emailMessage;
       }


        private async Task SendAsync(MimeMessage emailMessage)
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfiguration.UserName, _emailConfiguration.Password);
                    await client.SendAsync(emailMessage);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }


        }
    }
}
