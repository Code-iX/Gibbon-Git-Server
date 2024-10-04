using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Data.Entities;
using Gibbon.Git.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gibbon.Git.Server.Security;

public class MembershipService(ILogger<MembershipService> logger, GibbonGitServerContext context, IPasswordService passwordService)
    : IMembershipService
{
    private readonly ILogger<MembershipService> _logger = logger;
    private readonly GibbonGitServerContext _context = context;
    private readonly IPasswordService _passwordService = passwordService;

    public bool IsPasswordValid(string username, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(username, nameof(username));
        ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));

        _logger.LogDebug("Validating user {UserName}", username);

        username = username.ToLowerInvariant();
        var user = _context.Users.FirstOrDefault(i => i.Username == username);
        if (user == null)
        {
            _logger.LogWarning("Failed to find user {UserName}", username);
            return false;
        }

        var result = _passwordService.CompareHash(user.PasswordSalt, password, user.Password);
        _logger.LogDebug("User {UserName} validation result {Result}", username, result);
        return result;
    }

    public bool CreateUser(string username, string password, string givenName, string surname, string email)
    {
        ArgumentException.ThrowIfNullOrEmpty(username, nameof(username));
        ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));
        ArgumentException.ThrowIfNullOrEmpty(givenName, nameof(givenName));
        ArgumentException.ThrowIfNullOrEmpty(surname, nameof(surname));
        ArgumentException.ThrowIfNullOrEmpty(email, nameof(email));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username.ToLowerInvariant(),
            GivenName = givenName,
            Surname = surname,
            Email = email,
        };
        SetPassword(user, password);
        _context.Users.Add(user);
        try
        {
            _context.SaveChanges();
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public IList<UserModel> GetAllUsers()
    {
        return _context.Users.Include("Roles").ToList().Select(item => new UserModel
        {
            Id = item.Id,
            Username = item.Username,
            GivenName = item.GivenName,
            Surname = item.Surname,
            Email = item.Email,
        }).ToList();
    }

    public int UserCount()
    {
        return _context.Users.Count();
    }

    private static UserModel GetUserModel(User user) => user == null ? null : new UserModel
    {
        Id = user.Id,
        Username = user.Username,
        GivenName = user.GivenName,
        Surname = user.Surname,
        Email = user.Email,
    };

    public UserModel GetUserModel(Guid id)
    {
        var user = _context.Users.FirstOrDefault(i => i.Id == id);
        return GetUserModel(user);
    }

    public UserModel GetUserModel(string username)
    {
        //username = username.ToLowerInvariant();
        var user = _context.Users.FirstOrDefault(i => i.Username == username);
        return GetUserModel(user);
    }

    public void DeleteUser(Guid id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            user.AdministratedRepositories.Clear();
            user.Roles.Clear();
            user.Repositories.Clear();
            user.Teams.Clear();
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }

    public string GenerateResetToken(string username)
    {
        return _passwordService.GenerateToken(username);
    }

    public void UpdateUser(Guid id, string username, string givenName, string surname, string email)
    {
        var user = _context.Users.FirstOrDefault(i => i.Id == id);
        if (user == null)
            return;

        var lowerUsername = username?.ToLowerInvariant();
        user.Username = lowerUsername ?? user.Username;
        user.GivenName = givenName ?? user.GivenName;
        user.Surname = surname ?? user.Surname;
        user.Email = email ?? user.Email;
        _context.SaveChanges();
    }

    public void UpdatePassword(Guid id, string newPassword)
    {
        var user = _context.Users.FirstOrDefault(i => i.Id == id);
        if (user == null)
        {
            throw new InvalidOperationException("UserId not valid");
        }

        SetPassword(user, newPassword);
        _context.SaveChanges();
    }

    private void SetPassword(User user, string password)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));

        user.PasswordSalt = _passwordService.GenerateSalt();
        user.Password = _passwordService.GenerateHash(user.PasswordSalt, password);
    }
}
