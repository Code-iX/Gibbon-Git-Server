using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Security;

public interface IMembershipService
{
    bool CreateUser(string username, string password, string givenName, string surname, string email);
    IList<UserModel> GetAllUsers();
    UserModel GetUserModel(Guid id);
    UserModel GetUserModel(string username);
    void DeleteUser(Guid id);
    string GenerateResetToken(string username);
    bool IsPasswordValid(string username, string password);
    void UpdateUser(Guid id, string givenName, string surname, string email);
    void UpdatePassword(Guid id, string newPassword);
}
