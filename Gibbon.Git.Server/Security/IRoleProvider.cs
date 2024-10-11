namespace Gibbon.Git.Server.Security;

public interface IRoleProvider
{
    string[] GetAllRoles();
    void CreateRole(string roleName);
    bool DeleteRole(string roleName);
    bool IsUserInRole(int userId, string roleName);
    string[] GetRolesForUser(int userId);
    void AddRolesToUser(int userId, string[] roleNames);
    void RemoveRolesFromUser(int userId, string[] roleNames);
}
