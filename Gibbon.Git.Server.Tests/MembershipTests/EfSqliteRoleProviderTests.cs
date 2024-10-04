using System;
using System.Linq;

using Gibbon.Git.Server.Data.Entities;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Tests.TestHelper;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Gibbon.Git.Server.Tests.MembershipTests;

[TestClass]
public class EfSqliteRoleProviderTests : DbTestBase<SqliteConnectionFactory>
{
    private IRoleProvider _roleProvider = null!;

    protected override void ConfigureServices(ServiceCollection services)
    {
        var membershipService = Substitute.For<IMembershipService>();
        services.AddSingleton(membershipService);
        services.AddSingleton<IRoleProvider, RoleProvider>();
    }

    protected override void UseServices(IServiceProvider serviceProvider)
    {
        _roleProvider = serviceProvider.GetRequiredService<IRoleProvider>();
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("Verify that the new role provider initially has just the Admin role.")]
    public void TestNewProviderHasJustAdminRole()
    {
        Assert.AreEqual("Administrator", _roleProvider.GetAllRoles().Single());
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("Verify that the Admin role initially has one member.")]
    public void TestAdminRoleHasOneMember()
    {
        var roles = _roleProvider.GetRolesForUser(GetAdminId());
        CollectionAssert.AreEqual(new[] { "Administrator" }, roles);
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("When adding a Role to a non-existent user, we'll get an exception.")]
    public void TestAddingNonExistentUserToRoleIsSilentlyIgnored()
    {
        Assert.ThrowsException<InvalidOperationException>(() => _roleProvider.AddRolesToUser(Guid.NewGuid(), ["Administrator"]));
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("Verify that adding a real user to a role is successful.")]
    public void TestAddingRealUserIsSuccessful()
    {
        var userId = AddUserFred();
        _roleProvider.AddRolesToUser(userId, ["Administrator"]);
        var roles = _roleProvider.GetRolesForUser(userId);
        CollectionAssert.AreEqual(new[] { "Administrator" }, roles);
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("Verify that a new role can be successfully created.")]
    public void TestCreatingRole()
    {
        _roleProvider.CreateRole("Programmer");
        CollectionAssert.AreEqual(new[] { "Administrator", "Programmer" }, _roleProvider.GetAllRoles().OrderBy(role => role).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("Verify that a user can be removed from a specific role.")]
    public void RemovingAUserFromARole()
    {
        _roleProvider.CreateRole("Programmer");
        var userId = AddUserFred();
        _roleProvider.AddRolesToUser(userId, new[] { "Administrator", "Programmer" });

        _roleProvider.RemoveRolesFromUser(userId, new[] { "Administrator" });

        CollectionAssert.AreEqual(new[] { "Programmer" }, _roleProvider.GetRolesForUser(userId));
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("Verify that adding a user to multiple roles is successful.")]
    [Ignore]
    public void TestAddingUserToMultipleRoles()
    {
        _roleProvider.CreateRole("Programmer");
        var fredId = AddUserFred();
        _roleProvider.AddRolesToUser(fredId, new[] { "Programmer", "Administrator" });
        CollectionAssert.AreEqual(new[] { "Administrator", "Programmer" }, _roleProvider.GetRolesForUser(fredId).OrderBy(role => role).ToArray());
        //CollectionAssert.AreEqual(new[] { GetAdminId(), fredId }.OrderBy(u => u).ToArray(), _roleProvider.GetUsersInRole("Administrator").OrderBy(name => name).ToArray());
        //CollectionAssert.AreEqual(new[] { fredId }, _roleProvider.GetUsersInRole("Programmer"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("Ensure that attempting to delete a populated role throws an exception if not allowed.")]
    public void TestRoleCannotBeDeletedWhilePopulatedIfForbidden()
    {
        var userId = AddUserFred();
        _roleProvider.CreateRole("Programmer");
        _roleProvider.AddRolesToUser(userId, new[] { "Programmer" });
        Assert.ThrowsException<InvalidOperationException>(() => _roleProvider.DeleteRole("Programmer"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("Verify that a role can be deleted if no members are present.")]
    public void RoleCanBeDeletedIfNoMembersPresent()
    {
        _roleProvider.CreateRole("Programmer");
        _roleProvider.DeleteRole("Programmer");
        Assert.AreEqual(1, _roleProvider.GetAllRoles().Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("Verify that the system correctly detects if a user is in a role.")]
    public void UserInRoleDetectedCorrectly()
    {
        Assert.IsTrue(_roleProvider.IsUserInRole(GetAdminId(), "Administrator"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Roles)]
    [Description("Verify that adding an existing role to a user does not throw an error.")]
    public void AddingExistingRoleToUserDoesNotThrowError()
    {
        var adminId = GetAdminId();
        _roleProvider.AddRolesToUser(adminId, new[] { "Administrator" });

        // Ensure no exception is thrown and role is still only assigned once
        var roles = _roleProvider.GetRolesForUser(adminId);
        CollectionAssert.AreEqual(new[] { "Administrator" }, roles);
    }

    private Guid AddUserFred()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "fred",
            Password = "letmein",
            GivenName = "Fred",
            Surname = "FredBlogs",
            Email = "fred@aol.com"
        };
        DbContext.Users.Add(user);
        DbContext.SaveChanges();
        return user.Id;
    }

    private Guid GetAdminId()
    {
        return DbContext.Users.Single(u => u.Username == "admin").Id;
    }
}
