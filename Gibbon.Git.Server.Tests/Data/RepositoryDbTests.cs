﻿using System;
using System.Collections.Generic;
using System.Linq;

using Gibbon.Git.Server.Data.Entities;
using Gibbon.Git.Server.Tests.TestHelper;

using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Tests.Data;

[TestClass]
[TestCategory(TestCategories.Database)]
public class SqliteRepositoryDbTests : GibbonDbContextTestsBase<SqliteConnectionFactory>
{
    [TestMethod]
    public void CanAddRepository()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "TestRepo" };

        // Act
        Context.Repositories.Add(repository);
        Context.SaveChanges();

        // Assert
        var savedRepo = Context.Repositories.FirstOrDefault(r => r.Name == "TestRepo");
        Assert.IsNotNull(savedRepo);
        Assert.AreEqual("TestRepo", savedRepo.Name);
    }

    [TestMethod]
    public void RepositoryNameMustBeUnique()
    {
        // Arrange
        var repo1 = new Repository { Id = Guid.NewGuid(), Name = "UniqueRepo" };
        var repo2 = new Repository { Id = Guid.NewGuid(), Name = "UniqueRepo" }; // Gleicher Name

        // Act
        Context.Repositories.Add(repo1);
        Context.SaveChanges();

        // Assert
        Context.Repositories.Add(repo2);
        Assert.ThrowsException<DbUpdateException>(() => Context.SaveChanges());
    }

    [TestMethod]
    public void CanAddUserWithRole()
    {
        // Arrange
        var role = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            GivenName = "Test",
            Surname = "User",
            Email = "example@example.org",
            Password = "TestPassword",
            PasswordSalt = "TestSalt",
            Roles = [role]
        };

        // Act
        Context.Users.Add(user);
        Context.SaveChanges();

        // Assert
        var savedUser = Context.Users.Include(u => u.Roles).FirstOrDefault(u => u.Username == "TestUser");
        Assert.IsNotNull(savedUser);
        Assert.IsTrue(savedUser.Roles.Any(r => r.Name == "Admin"));
    }

    [TestMethod]
    public void CanDeleteRepository()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "TestRepo" };
        Context.Repositories.Add(repository);
        Context.SaveChanges();

        // Act
        Context.Repositories.Remove(repository);
        Context.SaveChanges();

        // Assert
        var deletedRepo = Context.Repositories.FirstOrDefault(r => r.Name == "TestRepo");
        Assert.IsNull(deletedRepo);
    }

    [TestMethod]
    public void CanAddTeamWithRepository()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "TestRepo" };
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Dev Team",
            Repositories = [repository]
        };

        // Act
        Context.Teams.Add(team);
        Context.SaveChanges();

        // Assert
        var savedTeam = Context.Teams.Include(t => t.Repositories).FirstOrDefault(t => t.Name == "Dev Team");
        Assert.IsNotNull(savedTeam);
        Assert.IsTrue(savedTeam.Repositories.Any(r => r.Name == "TestRepo"));
    }

    [TestMethod]
    public void CanAssignAdministratorToRepository()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "AdminRepo" };
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Username = "AdminUser",
            GivenName = "Admin",
            Surname = "User",
            Email = "admin@example.org",
            Password = "AdminPassword",
            PasswordSalt = "AdminSalt"
        };

        // Act
        repository.Administrators.Add(admin);
        Context.Repositories.Add(repository);
        Context.SaveChanges();

        // Assert
        var savedRepo = Context.Repositories.Include(r => r.Administrators).FirstOrDefault(r => r.Name == "AdminRepo");
        Assert.IsNotNull(savedRepo);
        Assert.IsTrue(savedRepo.Administrators.Any(a => a.Username == "AdminUser"));
    }

    [TestMethod]
    public void CanAddMultipleUsersToRepository()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "TestRepo" };
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Username = "User1",
            GivenName = "Test1",
            Surname = "User1",
            Email = "user1@example.org",
            Password = "Password1",
            PasswordSalt = "Salt1"
        };
        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Username = "User2",
            GivenName = "Test2",
            Surname = "User2",
            Email = "user2@example.org",
            Password = "Password2",
            PasswordSalt = "Salt2"
        };

        // Act
        repository.Users.Add(user1);
        repository.Users.Add(user2);
        Context.Repositories.Add(repository);
        Context.SaveChanges();

        // Assert
        var savedRepo = Context.Repositories.Include(r => r.Users).FirstOrDefault(r => r.Name == "TestRepo");
        Assert.IsNotNull(savedRepo);
        Assert.IsTrue(savedRepo.Users.Any(u => u.Username == "User1"));
        Assert.IsTrue(savedRepo.Users.Any(u => u.Username == "User2"));
    }
    [TestMethod]
    public void TransactionRollbackOnRepositoryFailure()
    {
        // Arrange
        using var transaction = Context.Database.BeginTransaction();

        var repository = new Repository { Id = Guid.NewGuid(), Name = "RollbackRepo" };
        var team = new Team { Id = Guid.NewGuid(), Name = "RollbackTeam" };

        Context.Repositories.Add(repository);
        Context.SaveChanges();

        // Act: Versuch, ein Team hinzuzufügen, aber absichtlich einen Fehler einbauen
        try
        {
            team.Repositories.Add(repository);
            Context.Teams.Add(team);

            // Dieser Fehler führt zum Abbruch, da ein ungültiges Feld gesetzt wird (z.B. kein Name)
            repository.Name = null; // Invalidiert das Repository

            Context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            // Rollback bei einem Fehler
            transaction.Rollback();
        }

        // Assert: Überprüfe, ob die Änderungen wirklich zurückgerollt wurden
        var savedRepo = Context.Repositories.FirstOrDefault(r => r.Name == "RollbackRepo");
        Assert.IsNull(savedRepo); // Repository sollte nicht gespeichert sein
    }
    [TestMethod]
    public void DeleteRepositoryRemovesOnlyRelationsNotUsersOrTeams()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "RelationTestRepo" };
        var admin = new User { Id = Guid.NewGuid(), Username = "AdminUser", GivenName = "Admin", Surname = "User", Email = "admin@example.com", Password = "password", PasswordSalt = "salt" };
        var team = new Team { Id = Guid.NewGuid(), Name = "RelationTestTeam" };

        repository.Administrators.Add(admin);
        repository.Teams.Add(team);

        Context.Repositories.Add(repository);
        Context.SaveChanges();

        // Act: Lösche das Repository
        Context.Repositories.Remove(repository);
        Context.SaveChanges();

        // Assert: Überprüfe, ob das Repository gelöscht wurde
        var deletedRepo = Context.Repositories.FirstOrDefault(r => r.Name == "RelationTestRepo");
        Assert.IsNull(deletedRepo); // Repository sollte gelöscht sein

        // Überprüfe, ob der Administrator und das Team NICHT gelöscht wurden
        var existingAdmin = Context.Users.FirstOrDefault(u => u.Username == "AdminUser");
        var existingTeam = Context.Teams.FirstOrDefault(t => t.Name == "RelationTestTeam");
        Assert.IsNotNull(existingAdmin); // Admin sollte nicht gelöscht sein
        Assert.IsNotNull(existingTeam);  // Team sollte nicht gelöscht sein

        // Überprüfe, ob die Beziehungen entfernt wurden
        var remainingAdminRelation = Context.Set<Dictionary<string, object>>("UserRepository_Administrator")
            .FirstOrDefault(relation => relation["User_Id"].Equals(admin.Id));
        var remainingTeamRelation = Context.Set<Dictionary<string, object>>("TeamRepository_Permission")
            .FirstOrDefault(relation => relation["Team_Id"].Equals(team.Id));

        Assert.IsNull(remainingAdminRelation);
        Assert.IsNull(remainingTeamRelation);
    }
    [TestMethod]
    public void DeleteUserOrTeamDoesNotDeleteRepository()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "TestRepo" };
        var user = new User { Id = Guid.NewGuid(), Username = "TestUser", GivenName = "Test", Surname = "User", Email = "testuser@example.com", Password = "password", PasswordSalt = "salt" };
        var team = new Team { Id = Guid.NewGuid(), Name = "TestTeam" };

        repository.Users.Add(user);
        repository.Teams.Add(team);

        Context.Repositories.Add(repository);
        Context.Users.Add(user);
        Context.Teams.Add(team);
        Context.SaveChanges();

        // Act: Lösche den Benutzer und das Team
        Context.Users.Remove(user);
        Context.Teams.Remove(team);
        Context.SaveChanges();

        // Assert: Das Repository sollte noch vorhanden sein
        var remainingRepo = Context.Repositories.FirstOrDefault(r => r.Name == "TestRepo");
        Assert.IsNotNull(remainingRepo);

        // Überprüfe, dass die Beziehungen zu den gelöschten Usern/Teams entfernt wurden
        var remainingUserRelation = Context.Set<Dictionary<string, object>>("UserRepository_Permission")
            .FirstOrDefault(relation => relation["User_Id"].Equals(user.Id));
        var remainingTeamRelation = Context.Set<Dictionary<string, object>>("TeamRepository_Permission")
            .FirstOrDefault(relation => relation["Team_Id"].Equals(team.Id));

        Assert.IsNull(remainingUserRelation); // Beziehung zwischen User und Repository sollte gelöscht sein
        Assert.IsNull(remainingTeamRelation); // Beziehung zwischen Team und Repository sollte gelöscht sein
    }

    [TestMethod]
    public void RepositoryDataIntegrityWithUsersTeamsAndAdmins()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "DataIntegrityRepo" };
        var user = new User { Id = Guid.NewGuid(), Username = "DataUser", GivenName = "User", Surname = "Test", Email = "user@example.com", Password = "password", PasswordSalt = "salt" };
        var team = new Team { Id = Guid.NewGuid(), Name = "DataTeam" };
        var admin = new User { Id = Guid.NewGuid(), Username = "AdminUser", GivenName = "Admin", Surname = "Test", Email = "admin@example.com", Password = "password", PasswordSalt = "salt" };

        repository.Users.Add(user);
        repository.Teams.Add(team);
        repository.Administrators.Add(admin);

        Context.Repositories.Add(repository);
        Context.SaveChanges();

        // Act: Lade das Repository mit den verknüpften Entitäten
        var savedRepo = Context.Repositories
            .Include(r => r.Users)
            .Include(r => r.Teams)
            .Include(r => r.Administrators)
            .FirstOrDefault(r => r.Name == "DataIntegrityRepo");

        // Assert: Überprüfe, ob alle Beziehungen korrekt sind
        Assert.IsNotNull(savedRepo);
        Assert.AreEqual(1, savedRepo.Users.Count);
        Assert.AreEqual(1, savedRepo.Teams.Count);
        Assert.AreEqual(1, savedRepo.Administrators.Count);
        Assert.IsTrue(savedRepo.Users.Any(u => u.Username == "DataUser"));
        Assert.IsTrue(savedRepo.Teams.Any(t => t.Name == "DataTeam"));
        Assert.IsTrue(savedRepo.Administrators.Any(a => a.Username == "AdminUser"));
    }
    [TestMethod]
    public void CompleteRepositoryLifecycleTest()
    {
        // Arrange 
        var repository = new Repository { Id = Guid.NewGuid(), Name = "LifecycleRepo" };
        var admin = new User { Id = Guid.NewGuid(), Username = "AdminLifecycle", GivenName = "Admin", Surname = "Test", Email = "admin@example.com", Password = "password", PasswordSalt = "salt" };
        var user = new User { Id = Guid.NewGuid(), Username = "UserLifecycle", GivenName = "User", Surname = "Test", Email = "user@example.com", Password = "password", PasswordSalt = "salt" };
        var team = new Team { Id = Guid.NewGuid(), Name = "LifecycleTeam" };

        // Act: Create
        repository.Administrators.Add(admin);
        repository.Users.Add(user);
        repository.Teams.Add(team);

        Context.Repositories.Add(repository);
        Context.SaveChanges();

        // Assert
        var savedRepo = Context.Repositories.Include(r => r.Users).Include(r => r.Teams).Include(r => r.Administrators).FirstOrDefault(r => r.Name == "LifecycleRepo");
        Assert.IsNotNull(savedRepo);
        Assert.IsTrue(savedRepo.Users.Any(u => u.Username == "UserLifecycle"));
        Assert.IsTrue(savedRepo.Administrators.Any(a => a.Username == "AdminLifecycle"));
        Assert.IsTrue(savedRepo.Teams.Any(t => t.Name == "LifecycleTeam"));

        // Act: Update
        savedRepo.Name = "UpdatedRepo";
        Context.Repositories.Update(savedRepo);
        Context.SaveChanges();

        // Assert
        var updatedRepo = Context.Repositories.FirstOrDefault(r => r.Name == "UpdatedRepo");
        Assert.IsNotNull(updatedRepo);

        // Act: Delete
        Context.Repositories.Remove(updatedRepo);
        Context.SaveChanges();

        // Assert
        var deletedRepo = Context.Repositories.FirstOrDefault(r => r.Name == "UpdatedRepo");
        Assert.IsNull(deletedRepo);
    }
    [TestMethod]
    public void RepositoryDeleteFailsRollbackUsersAndTeams()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "FailRepo" };
        var user = new User { Id = Guid.NewGuid(), Username = "FailUser", GivenName = "User", Surname = "Fail", Email = "fail@example.com", Password = "password", PasswordSalt = "salt" };
        var team = new Team { Id = Guid.NewGuid(), Name = "FailTeam" };

        repository.Users.Add(user);
        repository.Teams.Add(team);

        Context.Repositories.Add(repository);
        Context.SaveChanges();

        using var transaction = Context.Database.BeginTransaction();
        // Act
        repository.Users.Clear();
        repository.Teams.Clear();

        Context.Repositories.Remove(repository);
        Context.SaveChanges();

        transaction.Rollback();

        // Assert
        var remainingUser = Context.Users.FirstOrDefault(u => u.Username == "FailUser");
        var remainingTeam = Context.Teams.FirstOrDefault(t => t.Name == "FailTeam");
        Assert.IsNotNull(remainingUser);
        Assert.IsNotNull(remainingTeam);
    }
    [TestMethod]
    public void CanAddAndRemoveUserFromRepository()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "TestRepo" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            GivenName = "Test",
            Surname = "User",
            Email = "testuser@example.com",
            Password = "password",
            PasswordSalt = "salt"
        };

        Context.Repositories.Add(repository);
        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
        repository.Users.Add(user);
        Context.SaveChanges();

        // Assert
        var savedRepo = Context.Repositories.Include(r => r.Users).FirstOrDefault(r => r.Name == "TestRepo");
        Assert.IsNotNull(savedRepo);
        Assert.IsTrue(savedRepo.Users.Any(u => u.Username == "TestUser"));

        // Act
        savedRepo.Users.Remove(user);
        Context.SaveChanges();

        // Assert
        var updatedRepo = Context.Repositories.Include(r => r.Users).FirstOrDefault(r => r.Name == "TestRepo");
        Assert.IsNotNull(updatedRepo);
        Assert.IsFalse(updatedRepo.Users.Any(u => u.Username == "TestUser"));
    }
    [TestMethod]
    public void CanAddTwoTeamsAndRemoveFirstFromRepository()
    {
        // Arrange
        var repository = new Repository { Id = Guid.NewGuid(), Name = "TestRepo" };
        var team1 = new Team { Id = Guid.NewGuid(), Name = "FirstTeam" };
        var team2 = new Team { Id = Guid.NewGuid(), Name = "SecondTeam" };

        Context.Repositories.Add(repository);
        Context.Teams.Add(team1);
        Context.Teams.Add(team2);
        Context.SaveChanges();

        // Act
        repository.Teams.Add(team1);
        Context.SaveChanges();

        // Act
        repository.Teams.Add(team2);
        Context.SaveChanges();

        // Assert
        var savedRepo = Context.Repositories.Include(r => r.Teams).FirstOrDefault(r => r.Name == "TestRepo");
        Assert.IsNotNull(savedRepo);
        Assert.IsTrue(savedRepo.Teams.Any(t => t.Name == "FirstTeam"));
        Assert.IsTrue(savedRepo.Teams.Any(t => t.Name == "SecondTeam"));

        // Act
        savedRepo.Teams.Remove(team1);
        Context.SaveChanges();

        // Assert
        var updatedRepo = Context.Repositories.Include(r => r.Teams).FirstOrDefault(r => r.Name == "TestRepo");
        Assert.IsNotNull(updatedRepo);
        Assert.IsFalse(updatedRepo.Teams.Any(t => t.Name == "FirstTeam"));
        Assert.IsTrue(updatedRepo.Teams.Any(t => t.Name == "SecondTeam"));
    }
}
