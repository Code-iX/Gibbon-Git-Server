using System;
using System.Linq;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Tests.TestHelper;
using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Tests.UserTests;

[TestClass]
public class EfUserServiceTests : DbTestBase<SqliteConnectionFactory>
{
    private IUserService _userService = null!;

    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<IUserService, UserService>();
    }

    protected override void UseServices(IServiceProvider serviceProvider)
    {
        _userService = serviceProvider.GetRequiredService<IUserService>();
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Integration test to verify if updates can be re-run without issues on an already updated database.")]
    public void UpdatesCanBeRunOnAlreadyUpdatedDatabase()
    {
        // Run all the updates again - this should be completely harmless
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Verifies that a new database contains only the default admin user.")]
    public void NewDatabaseContainsJustAdminUser()
    {
        var admin = _userService.GetAllUsers().Single();
        Assert.AreEqual("admin", admin.Username);
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Verifies that the admin user has the correct default password.")]
    public void NewAdminUserHasCorrectPassword()
    {
        Assert.IsTrue(_userService.IsPasswordValid("admin", "admin"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Verifies that password validation is case-sensitive.")]
    public void PasswordsAreCaseSensitive()
    {
        Assert.IsFalse(_userService.IsPasswordValid("admin", "Admin"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Tests that GetUser returns null for a non-existent user.")]
    public void GetUserReturnsNullForNonExistentUser()
    {
        Assert.IsNull(_userService.GetUserModel("52734589237450892374509283745092834750928347502938475"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Tests that GetUser is case-insensitive.")]
    public void GetUserIsCaseInsensitive()
    {
        Assert.AreEqual("admin", _userService.GetUserModel("admin").Username);
        Assert.AreEqual("admin", _userService.GetUserModel("ADMIN").Username);
        Assert.AreEqual("admin", _userService.GetUserModel("Admin").Username);
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Tests that a new user can be successfully added.")]
    public void NewUserCanBeAdded()
    {
        CreateTestUser();
        Assert.AreEqual(2, _userService.GetAllUsers().Count);
        var newUser = _userService.GetUserModel("testuser");
        Assert.AreEqual("John", newUser.GivenName);
        Assert.AreEqual("User", newUser.Surname);
        Assert.AreEqual("test@user.com", newUser.Email);
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Tests that a new user can be deleted.")]
    public void NewUserCanBeDeleted()
    {
        CreateTestUser();
        Assert.AreEqual(2, _userService.GetAllUsers().Count);
        _userService.DeleteUser(_userService.GetUserModel("testUser").Id);
        Assert.AreEqual(1, _userService.GetAllUsers().Count);
    }

    int CreateTestUser()
    {
        _userService.CreateUser("testUser", "hello", "John", "User", "test@user.com");
        return _userService.GetUserModel("testUser").Id;
    }
}
