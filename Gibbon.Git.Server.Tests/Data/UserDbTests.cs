using System;
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
            Id = Guid.NewGuid(),
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
        // Arrange: User ohne Required-Felder
        var user = new User
        {
            Id = Guid.NewGuid(),
            // Keine GivenName, Surname, Username, Password, Email gesetzt
        };

        // Act & Assert: Erwartet Exception, da Required-Felder fehlen
        Context.Users.Add(user);
        Assert.ThrowsException<DbUpdateException>(() => Context.SaveChanges());
    }

    [TestMethod]
    public void UsernameMustBeUnique()
    {
        // Arrange: Zwei Benutzer mit dem gleichen Username
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            GivenName = "Test1",
            Surname = "User1",
            Username = "duplicateUser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "user1@example.com"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            GivenName = "Test2",
            Surname = "User2",
            Username = "duplicateUser", // Gleicher Username
            Password = "password",
            PasswordSalt = "salt",
            Email = "user2@example.com"
        };

        // Act
        Context.Users.Add(user1);
        Context.SaveChanges();

        // Assert: Der zweite Benutzer mit gleichem Username darf nicht hinzugefügt werden.
        Context.Users.Add(user2);
        Assert.ThrowsException<DbUpdateException>(() => Context.SaveChanges());
    }

    [TestMethod]
    public void CanUpdateUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            GivenName = "OldName",
            Surname = "OldSurname",
            Username = "testuser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "test@example.com"
        };

        Context.Users.Add(user);
        Context.SaveChanges();

        // Act: Aktualisiere den Benutzer
        user.GivenName = "NewName";
        user.Surname = "NewSurname";
        Context.Users.Update(user);
        Context.SaveChanges();

        // Assert: Überprüfe, ob der Benutzer aktualisiert wurde
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
            Id = Guid.NewGuid(),
            GivenName = "Test",
            Surname = "User",
            Username = "deleteuser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "delete@example.com"
        };

        Context.Users.Add(user);
        Context.SaveChanges();

        // Act: Lösche den Benutzer
        Context.Users.Remove(user);
        Context.SaveChanges();

        // Assert: Überprüfe, ob der Benutzer gelöscht wurde
        var deletedUser = Context.Users.FirstOrDefault(u => u.Username == "deleteuser");
        Assert.IsNull(deletedUser);
    }

    [TestMethod]
    public void HasData_SeedUserExists()
    {
        // Arrange: Id des Seed-Users
        var seedUserId = Guid.Parse("3eb9995e-99e3-425a-b978-1409bdd61fb6");

        // Act: Hole den Seed-Benutzer
        var seedUser = Context.Users.FirstOrDefault(u => u.Id == seedUserId);

        // Assert: Seed-Benutzer existiert und hat die korrekten Daten
        Assert.IsNotNull(seedUser);
        Assert.AreEqual("admin", seedUser.GivenName);
        Assert.AreEqual("admin", seedUser.Username);
        Assert.AreEqual("2dpBKPc2rPqPa03udauh6LUo4uNHFSNQZaH4P1BIkNizmUmuir/61Vgkr5MaXlr+bVWnefxQD1H1ciMEtEr/hQ==", seedUser.Password);
    }

    [TestMethod]
    public void CanAssignUserToTeam()
    {
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", GivenName = "Test", Surname = "User", Email = "test@example.com", Password = "password", PasswordSalt = "salt" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Dev Team" };
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
            Id = Guid.NewGuid(),
            GivenName = "Complex",
            Surname = "User",
            Username = "complexuser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "complex@example.com"
        };

        var role1 = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        var role2 = new Role { Id = Guid.NewGuid(), Name = "Developer" };
        var team1 = new Team { Id = Guid.NewGuid(), Name = "Dev Team" };
        var team2 = new Team { Id = Guid.NewGuid(), Name = "Ops Team" };

        user.Roles.Add(role1);
        user.Roles.Add(role2);
        user.Teams.Add(team1);
        user.Teams.Add(team2);

        // Act
        Context.Users.Add(user);
        Context.SaveChanges();

        // Assert: Überprüfe, ob die Benutzer-Rollen- und Benutzer-Team-Beziehungen korrekt gespeichert wurden
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
            Id = Guid.NewGuid(),
            GivenName = "Transaction",
            Surname = "User1",
            Username = "transactionuser1",
            Password = "password1",
            PasswordSalt = "salt1",
            Email = "transaction1@example.com"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            GivenName = "Transaction",
            Surname = "User2",
            Username = "transactionuser2",
            Password = "password2",
            PasswordSalt = "salt2",
            Email = "transaction2@example.com"
        };

        // Act: Beide Benutzer hinzufügen
        Context.Users.Add(user1);
        Context.SaveChanges();

        // Füge den zweiten Benutzer mit einem Fehler hinzu (z.B. fehlendes Required-Feld)
        user2.Username = null; // Dieser Fehler wird einen DbUpdateException auslösen

        try
        {
            Context.Users.Add(user2);
            Context.SaveChanges(); // Erwartet Exception
        }
        catch (DbUpdateException)
        {
            // Rollback, wenn ein Fehler auftritt
            transaction.Rollback();
        }

        // Assert: Überprüfe, ob der erste Benutzer ebenfalls nicht mehr vorhanden ist (Rollback)
        var savedUser1 = Context.Users.FirstOrDefault(u => u.Email == "transaction1@example.com");
        Assert.IsNull(savedUser1);
    }
    [TestMethod]
    public void CompleteUserLifecycleTest()
    {
        // 1. User erstellen und speichern
        var user = new User
        {
            Id = Guid.NewGuid(),
            GivenName = "Lifecycle",
            Surname = "Test",
            Username = "lifecycleuser",
            Password = "password",
            PasswordSalt = "salt",
            Email = "lifecycle@example.com"
        };

        Context.Users.Add(user);
        Context.SaveChanges();

        // 2. Überprüfe, ob der User gespeichert wurde
        var savedUser = Context.Users.FirstOrDefault(u => u.Username == "lifecycleuser");
        Assert.IsNotNull(savedUser);

        // 3. User aktualisieren (z.B. Name und Passwort ändern)
        savedUser.GivenName = "Updated";
        savedUser.Password = "newpassword";
        Context.Users.Update(savedUser);
        Context.SaveChanges();

        // 4. Überprüfen, ob die Aktualisierungen erfolgreich waren
        var updatedUser = Context.Users.FirstOrDefault(u => u.Username == "lifecycleuser");
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual("Updated", updatedUser.GivenName);
        Assert.AreEqual("newpassword", updatedUser.Password);

        // 5. User löschen
        Context.Users.Remove(updatedUser);
        Context.SaveChanges();

        // 6. Überprüfen, ob der User erfolgreich gelöscht wurde
        var deletedUser = Context.Users.FirstOrDefault(u => u.Username == "lifecycleuser");
        Assert.IsNull(deletedUser);
    }

}
