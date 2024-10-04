using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Security;

public class RoleProvider(GibbonGitServerContext context) : IRoleProvider
{
    private readonly GibbonGitServerContext _context = context;

    public string[] GetAllRoles()
    {
        return _context.Roles.Select(i => i.Name).ToArray();
    }

    public void CreateRole(string roleName)
    {
        _context.Roles.Add(new Role
        {
            Name = roleName,
        });
        _context.SaveChanges();
    }

    public void AddRolesToUser(Guid userId, string[] roleNames)
    {
        var roles = _context.Roles.Where(i => roleNames.Contains(i.Name)).ToList();
        var user = _context.Users.Include(x => x.Roles).SingleOrDefault(i => i.Id == userId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        foreach (var role in roles)
        {
            if (user.Roles.All(r => r.Id != role.Id))
            {
                user.Roles.Add(role);
            }
        }

        _context.SaveChanges();
    }

    public bool DeleteRole(string roleName)
    {
        var role = _context.Roles
            .Include(role => role.Users)
            .FirstOrDefault(i => i.Name == roleName);
       
        if (role == null)
        {
            return false;
        }

        if (role.Users.Count > 0)
        {
            throw new InvalidOperationException("Can't delete role with members.");
        }        

        _context.Roles.Remove(role);
        _context.SaveChanges();
        return true;

    }

    public string[] GetRolesForUser(Guid userId)
    {
        var roles = _context.Roles
            .Where(role => role.Users.Any(us => us.Id == userId))
            .Select(role => role.Name)
            .ToArray();
        return roles;
    }

    public bool IsUserInRole(Guid userId, string roleName)
    {
        return _context.Roles.Any(role => role.Name == roleName && role.Users.Any(us => us.Id == userId));
    }

    public void RemoveRolesFromUser(Guid userId, string[] roleNames)
    {
        var roles = _context.Roles.Where(i => roleNames.Contains(i.Name)).ToList();
        var user = _context.Users.SingleOrDefault(i => i.Id == userId);

        if (user == null)
        {
            // Test says, that we should ignore this case silently
            return;
        }

        foreach (var role in roles)
        {
            user.Roles.Remove(role);
        }
        _context.SaveChanges();
    }
}
