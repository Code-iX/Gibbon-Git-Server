using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Services;

public interface IMailService
{
    bool SendForgotPasswordEmail(UserModel user, string passwordResetUrl);
}
