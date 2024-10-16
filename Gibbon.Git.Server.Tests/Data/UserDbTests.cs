using System.Linq;
using Gibbon.Git.Server.Data.Entities;
using Gibbon.Git.Server.Tests.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Tests.Data;

[TestClass]
public class SqliteUserDbTests : UserDbTests<SqliteConnectionFactory>;

public abstract class UserDbTests<TConnectionFactory> : GibbonDbContextTestsBase<SqliteConnectionFactory>
    where TConnectionFactory : IDbConnectionFactory, new()
{
    [TestMethod]
    public void CanCreateUser()
    {
        // Arrange
        var user = new User
        {
            
            GivenName = "Test",
            Surname = "User",
            Username = "testuser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "test@example.com"
        };

        // Act
        Context.Users.Add(user);
        Context.SaveChanges();

        // Assert
        var savedUser = Context.Users.FirstOrDefault(u => u.Username == "testuser");
        Assert.IsNotNull(savedUser);
        Assert.AreEqual("Test", savedUser.GivenName);
        Assert.AreEqual("User", savedUser.Surname);
        Assert.AreEqual("testuser", savedUser.Username);
        Assert.AreEqual("password", savedUser.Password);
        Assert.AreEqual("salt", savedUser.PasswordSalt);
        Assert.AreEqual("test@example.com", savedUser.Email);
    }

    [TestMethod]
    public void CannotCreateUserWithoutRequiredFields()
    {
        // Arrange
        var user = new User
        {            
            // empty
        };

        // Act & Assert
        Context.Users.Add(user);
        Assert.ThrowsException<DbUpdateException>(() => Context.SaveChanges());
    }

    [TestMethod]
    public void UsernameMustBeUnique()
    {
        // Arrange
        var user1 = new User
        {
            
            GivenName = "Test1",
            Surname = "User1",
            Username = "duplicateUser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "user1@example.com"
        };

        var user2 = new User
        {
            
            GivenName = "Test2",
            Surname = "User2",
            Username = "duplicateUser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "user2@example.com"
        };

        // Act
        Context.Users.Add(user1);
        Context.SaveChanges();

        // Assert
        Context.Users.Add(user2);
        Assert.ThrowsException<DbUpdateException>(() => Context.SaveChanges());
    }

    [TestMethod]
    public void CanUpdateUser()
    {
        // Arrange
        var user = new User
        {
            
            GivenName = "OldName",
            Surname = "OldSurname",
            Username = "testuser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "test@example.com"
        };

        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
        user.GivenName = "NewName";
        user.Surname = "NewSurname";
        Context.Users.Update(user);
        Context.SaveChanges();

        // Assert
        var updatedUser = Context.Users.FirstOrDefault(u => u.Username == "testuser");
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual("NewName", updatedUser.GivenName);
        Assert.AreEqual("NewSurname", updatedUser.Surname);
    }

    [TestMethod]
    public void CanDeleteUser()
    {
        // Arrange
        var user = new User
        {
            
            GivenName = "Test",
            Surname = "User",
            Username = "deleteuser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "delete@example.com"
        };

        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
        Context.Users.Remove(user);
        Context.SaveChanges();

        // Assert
        var deletedUser = Context.Users.FirstOrDefault(u => u.Username == "deleteuser");
        Assert.IsNull(deletedUser);
    }

    [TestMethod]
    public void HasData_SeedUserExists()
    {
        // Arrange
        var seedUserId = 1;

        // Act
        var seedUser = Context.Users.FirstOrDefault(u => u.Id == seedUserId);

        // Assert
        Assert.IsNotNull(seedUser);
        Assert.AreEqual("admin", seedUser.GivenName);
        Assert.AreEqual("admin", seedUser.Username);
        Assert.AreEqual("2dpBKPc2rPqPa03udauh6LUo4uNHFSNQZaH4P1BIkNizmUmuir/61Vgkr5MaXlr+bVWnefxQD1H1ciMEtEr/hQ==", seedUser.Password);
    }

    [TestMethod]
    public void CanAssignUserToTeam()
    {
        var user = new User {  Username = "testuser", GivenName = "Test", Surname = "User", Email = "test@example.com", Password = "password", PasswordSalt = "salt" };
        var team = new Team {  Name = "Dev Team" };
        user.Teams.Add(team);

        Context.Users.Add(user);
        Context.SaveChanges();

        var savedUser = Context.Users.Include(u => u.Teams).FirstOrDefault(u => u.Username == "testuser");
        Assert.IsNotNull(savedUser);
        Assert.IsTrue(savedUser.Teams.Any(t => t.Name == "Dev Team"));
    }
    [TestMethod]
    public void CanAssignMultipleRolesAndTeamsToUser()
    {
        // Arrange
        var user = new User
        {
            
            GivenName = "Complex",
            Surname = "User",
            Username = "complexuser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "complex@example.com"
        };

        var role1 = new Role {  Name = "Admin" };
        var role2 = new Role {  Name = "Developer" };
        var team1 = new Team {  Name = "Dev Team" };
        var team2 = new Team {  Name = "Ops Team" };

        user.Roles.Add(role1);
        user.Roles.Add(role2);
        user.Teams.Add(team1);
        user.Teams.Add(team2);

        // Act
        Context.Users.Add(user);
        Context.SaveChanges();

        // Assert
        var savedUser = Context.Users
            .Include(u => u.Roles)
            .Include(u => u.Teams)
            .FirstOrDefault(u => u.Username == "complexuser");

        Assert.IsNotNull(savedUser);
        Assert.AreEqual(2, savedUser.Roles.Count);
        Assert.AreEqual(2, savedUser.Teams.Count);
        Assert.IsTrue(savedUser.Roles.Any(r => r.Name == "Admin"));
        Assert.IsTrue(savedUser.Roles.Any(r => r.Name == "Developer"));
        Assert.IsTrue(savedUser.Teams.Any(t => t.Name == "Dev Team"));
        Assert.IsTrue(savedUser.Teams.Any(t => t.Name == "Ops Team"));
    }
    [TestMethod]
    public void TransactionRollbackOnFailure()
    {
        // Arrange
        using var transaction = Context.Database.BeginTransaction();

        var user1 = new User
        {
            
            GivenName = "Transaction",
            Surname = "User1",
            Username = "transactionuser1",
            Password = "password1",
            PasswordSalt = "salt1",
            Email = "transaction1@example.com"
        };

        var user2 = new User
        {
            
            GivenName = "Transaction",
            Surname = "User2",
            Username = "transactionuser2",
            Password = "password2",
            PasswordSalt = "salt2",
            Email = "transaction2@example.com"
        };

        // Act
        Context.Users.Add(user1);
        Context.SaveChanges();

        user2.Username = null;

        try
        {
            Context.Users.Add(user2);
            Context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            transaction.Rollback();
        }

        // Assert
        var savedUser1 = Context.Users.FirstOrDefault(u => u.Email == "transaction1@example.com");
        Assert.IsNull(savedUser1);
    }
    [TestMethod]
    public void CompleteUserLifecycleTest()
    {
        var user = new User
        {
            
            GivenName = "Lifecycle",
            Surname = "Test",
            Username = "lifecycleuser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "lifecycle@example.com"
        };

        Context.Users.Add(user);
        Context.SaveChanges();

        var savedUser = Context.Users.FirstOrDefault(u => u.Username == "lifecycleuser");
        Assert.IsNotNull(savedUser);

        savedUser.GivenName = "Updated";
        savedUser.Password = "newpassword";
        Context.Users.Update(savedUser);
        Context.SaveChanges();

        var updatedUser = Context.Users.FirstOrDefault(u => u.Username == "lifecycleuser");
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual("Updated", updatedUser.GivenName);
        Assert.AreEqual("newpassword", updatedUser.Password);

        Context.Users.Remove(updatedUser);
        Context.SaveChanges();

        var deletedUser = Context.Users.FirstOrDefault(u => u.Username == "lifecycleuser");
        Assert.IsNull(deletedUser);
    }

}
