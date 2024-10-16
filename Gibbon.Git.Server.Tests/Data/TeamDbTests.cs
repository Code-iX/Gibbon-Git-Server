using System.Linq;
using Gibbon.Git.Server.Data.Entities;
using Gibbon.Git.Server.Tests.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Tests.Data;

[TestClass]
public class SqliteTeamDbTests : TeamDbTests<SqliteConnectionFactory>;

public abstract class TeamDbTests<TConnectionFactory> : GibbonDbContextTestsBase<TConnectionFactory>
    where TConnectionFactory : IDbConnectionFactory, new()
{
    [TestMethod]
    public void CanCreateTeam()
    {
        // Arrange
        var team = new Team
        {
            
            Name = "Development Team",
            Description = "Team for development tasks"
        };

        // Act
        Context.Teams.Add(team);
        Context.SaveChanges();

        // Assert
        var savedTeam = Context.Teams.FirstOrDefault(t => t.Name == "Development Team");
        Assert.IsNotNull(savedTeam);
        Assert.AreEqual("Development Team", savedTeam.Name);
        Assert.AreEqual("Team for development tasks", savedTeam.Description);
    }

    [TestMethod]
    public void TeamNameMustBeUnique()
    {
        // Arrange
        var team1 = new Team
        {
            
            Name = "DuplicateTeam",
            Description = "First team with duplicate name"
        };

        var team2 = new Team
        {
            
            Name = "DuplicateTeam",
            Description = "Second team with duplicate name"
        };

        // Act
        Context.Teams.Add(team1);
        Context.SaveChanges();

        Context.Teams.Add(team2);
        // Assert
        Assert.ThrowsException<DbUpdateException>(() => Context.SaveChanges());
    }

    [TestMethod]
    public void CanUpdateTeam()
    {
        // Arrange
        var team = new Team
        {
            
            Name = "OldTeamName",
            Description = "Old team description"
        };

        Context.Teams.Add(team);
        Context.SaveChanges();

        // Act
        team.Name = "NewTeamName";
        team.Description = "Updated team description";
        Context.Teams.Update(team);
        Context.SaveChanges();

        // Assert
        var updatedTeam = Context.Teams.FirstOrDefault(t => t.Name == "NewTeamName");
        Assert.IsNotNull(updatedTeam);
        Assert.AreEqual("Updated team description", updatedTeam.Description);
    }

    [TestMethod]
    public void CanDeleteTeam()
    {
        // Arrange
        var team = new Team
        {
            
            Name = "TeamToDelete",
            Description = "This team will be deleted"
        };

        Context.Teams.Add(team);
        Context.SaveChanges();

        // Act
        Context.Teams.Remove(team);
        Context.SaveChanges();

        // Assert
        var deletedTeam = Context.Teams.FirstOrDefault(t => t.Name == "TeamToDelete");
        Assert.IsNull(deletedTeam);
    }

    [TestMethod]
    public void CanAssignUserToTeam()
    {
        // Arrange
        var team = new Team
        {
            
            Name = "TestTeam",
            Description = "A team to assign users"
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

        Context.Teams.Add(team);
        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
        user.Teams.Add(team);
        Context.Users.Update(user);
        Context.SaveChanges();

        // Assert
        var assignedUser = Context.Users.Include(u => u.Teams).FirstOrDefault(u => u.Username == "testuser");
        Assert.IsNotNull(assignedUser);
        Assert.IsTrue(assignedUser.Teams.Any(t => t.Name == "TestTeam"));
    }
}
