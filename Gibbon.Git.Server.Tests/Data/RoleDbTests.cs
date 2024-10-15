using System.Linq;

using Gibbon.Git.Server.Data.Entities;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Tests.TestHelper;

using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Tests.Data;

[TestClass]
public class SqliteRoleDbTests : RoleDbTests<SqliteConnectionFactory>;

public abstract class RoleDbTests<TConnectionFactory> : GibbonDbContextTestsBase<TConnectionFactory>
    where TConnectionFactory : IDbConnectionFactory, new()
{
    [TestMethod]
    public void CanCreateRole()
    {
        // Arrange
        var role = new Role
        {
            
            Name = "TestRole",
            Description = "A test role"
        };

        // Act
        Context.Roles.Add(role);
        Context.SaveChanges();

        // Assert
        var savedRole = Context.Roles.FirstOrDefault(r => r.Name == "TestRole");
        Assert.IsNotNull(savedRole);
        Assert.AreEqual("TestRole", savedRole.Name);
        Assert.AreEqual("A test role", savedRole.Description);
    }

    [TestMethod]
    public void RoleNameMustBeUnique()
    {
        // Arrange
        var role1 = new Role
        {
            
            Name = "DuplicateRole",
            Description = "First role with duplicate name"
        };

        var role2 = new Role
        {
            
            Name = "DuplicateRole",
            Description = "Second role with duplicate name"
        };

        // Act
        Context.Roles.Add(role1);
        Context.SaveChanges();

        Context.Roles.Add(role2);

        // Assert
        Assert.ThrowsException<DbUpdateException>(() => Context.SaveChanges());
    }

    [TestMethod]
    public void CanUpdateRole()
    {
        // Arrange
        var role = new Role
        {
            
            Name = "OldRoleName",
            Description = "Old description"
        };

        Context.Roles.Add(role);
        Context.SaveChanges();

        // Act
        role.Name = "NewRoleName";
        role.Description = "Updated description";
        Context.Roles.Update(role);
        Context.SaveChanges();

        // Assert
        var updatedRole = Context.Roles.FirstOrDefault(r => r.Name == "NewRoleName");
        Assert.IsNotNull(updatedRole);
        Assert.AreEqual("Updated description", updatedRole.Description);
    }

    [TestMethod]
    public void CanDeleteRole()
    {
        // Arrange
        var role = new Role
        {
            
            Name = "RoleToDelete",
            Description = "This role will be deleted"
        };

        Context.Roles.Add(role);
        Context.SaveChanges();

        // Act
        Context.Roles.Remove(role);
        Context.SaveChanges();

        // Assert
        var deletedRole = Context.Roles.FirstOrDefault(r => r.Name == "RoleToDelete");
        Assert.IsNull(deletedRole);
    }

    [TestMethod]
    public void HasData_SeedRoleExists()
    {
        // Arrange
        var seedRoleId = 1;

        // Act
        var seedRole = Context.Roles.FirstOrDefault(r => r.Id == seedRoleId);

        // Assert
        Assert.IsNotNull(seedRole);
        Assert.AreEqual(Roles.Admin, seedRole.Name);
        Assert.AreEqual("System administrator", seedRole.Description);
    }

    [TestMethod]
    public void CanAssignUserToRole()
    {
        // Arrange
        var role = new Role
        {
            
            Name = "NewRole",
            Description = "A role to assign"
        };

        var user = new User
        {
            
            GivenName = "Test",
            Surname = "User",
            Username = "testuser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "test@example.com"
        };

        Context.Roles.Add(role);
        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
        user.Roles.Add(role);
        Context.Users.Update(user);
        Context.SaveChanges();

        // Assert
        var assignedUser = Context.Users.Include(u => u.Roles).FirstOrDefault(u => u.Username == "testuser");
        Assert.IsNotNull(assignedUser);
        Assert.IsTrue(assignedUser.Roles.Any(r => r.Name == "NewRole"));
    }
}
