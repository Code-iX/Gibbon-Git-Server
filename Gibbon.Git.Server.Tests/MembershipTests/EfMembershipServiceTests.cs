using System;
using System.Linq;

using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Tests.TestHelper;

using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Tests.MembershipTests;

[TestClass]
public class EfMembershipServiceTests : DbTestBase<SqliteConnectionFactory>
{
    private IMembershipService _membershipService = null!;

    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<IMembershipService, MembershipService>();
    }

    protected override void UseServices(IServiceProvider serviceProvider)
    {
        _membershipService = serviceProvider.GetRequiredService<IMembershipService>();
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
        var admin = _membershipService.GetAllUsers().Single();
        Assert.AreEqual("admin", admin.Username);
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Verifies that the admin user has the correct default password.")]
    public void NewAdminUserHasCorrectPassword()
    {
        Assert.IsTrue(_membershipService.IsPasswordValid("admin", "admin"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Verifies that password validation is case-sensitive.")]
    public void PasswordsAreCaseSensitive()
    {
        Assert.IsFalse(_membershipService.IsPasswordValid("admin", "Admin"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Tests that GetUser returns null for a non-existent user.")]
    public void GetUserReturnsNullForNonExistentUser()
    {
        Assert.IsNull(_membershipService.GetUserModel("52734589237450892374509283745092834750928347502938475"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Tests that GetUser is case-insensitive.")]
    public void GetUserIsCaseInsensitive()
    {
        Assert.AreEqual("admin", _membershipService.GetUserModel("admin").Username);
        Assert.AreEqual("admin", _membershipService.GetUserModel("ADMIN").Username);
        Assert.AreEqual("admin", _membershipService.GetUserModel("Admin").Username);
    }

    [TestMethod]
    [TestCategory(TestCategories.Membership)]
    [Description("Tests that a new user can be successfully added.")]
    public void NewUserCanBeAdded()
    {
        CreateTestUser();
        Assert.AreEqual(2, _membershipService.GetAllUsers().Count);
        var newUser = _membershipService.GetUserModel("testuser");
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
        Assert.AreEqual(2, _membershipService.GetAllUsers().Count);
        _membershipService.DeleteUser(_membershipService.GetUserModel("testUser").Id);
        Assert.AreEqual(1, _membershipService.GetAllUsers().Count);
    }

    Guid CreateTestUser()
    {
        _membershipService.CreateUser("testUser", "hello", "John", "User", "test@user.com");
        return _membershipService.GetUserModel("testUser").Id;
    }
}
