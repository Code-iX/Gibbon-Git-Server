using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Security;

public interface IUserService
{
    bool CreateUser(string username, string password, string givenName, string surname, string email);
    List<UserModel> GetAllUsers();
    UserModel GetUserModel(int id);
    UserModel GetUserModel(string username);
    void DeleteUser(int id);
    string GenerateResetToken(string username);
    bool IsPasswordValid(string username, string password);
    void UpdateUser(int id, string givenName, string surname, string email);
    void UpdatePassword(int id, string newPassword);
}
