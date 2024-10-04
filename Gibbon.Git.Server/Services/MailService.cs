using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Gibbon.Git.Server.Services;

public class MailService : IMailService
{
    private readonly ILogger<MailService> _logger;
    private readonly MailSettings _mailSettings;

    public MailService(ILogger<MailService> logger, IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
        _logger = logger;
    }

    public bool SendForgotPasswordEmail(UserModel user, string passwordResetUrl)
    {
        var result = true;
        try
        {
            var message = new MimeMessage();
            var emailFrom = new MailboxAddress(_mailSettings.Name, _mailSettings.Username);
            MailboxAddress emailTo = new MailboxAddress(user.Username, user.Email);
            message.From.Add(emailFrom);
            message.To.Add(emailTo);
            message.Subject = $"{Resources.Product_Name} - {Resources.Email_PasswordReset_Title}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"{Resources.Email_PasswordReset_Body}<a href='{passwordResetUrl}'>{passwordResetUrl}</a>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Connect(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls); 
            client.Authenticate(_mailSettings.Username, _mailSettings.Password);

            client.Send(message);
            client.Disconnect(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Caught exception sending password reset email");
            result = false;
        }
        return result;
    }
}