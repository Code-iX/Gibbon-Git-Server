namespace Gibbon.Git.Server.Security;

public interface IRoleProvider
{
    string[] GetAllRoles();
    void CreateRole(string roleName);
    bool DeleteRole(string roleName);
    bool IsUserInRole(Guid userId, string roleName);
    string[] GetRolesForUser(Guid userId);
    void AddRolesToUser(Guid userId, string[] roleNames);
    void RemoveRolesFromUser(Guid userId, string[] roleNames);
}
