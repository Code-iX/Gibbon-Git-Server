using System;
using System.Collections.Generic;
using System.Linq;

using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Data.Entities;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Tests.TestHelper;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Gibbon.Git.Server.Tests.UserTests;

[TestClass]
public class EfSqliteTeamRepositoryTests : DbTestBase<SqliteConnectionFactory>
{
    private ITeamService _repo = null!;
    private IUserService _userService = null!;

    protected override void ConfigureServices(ServiceCollection services)
    {
        _userService = services.AddSubstitute<IUserService>();
        services.AddSubstitute<IRepositoryService>();
        services.AddSingleton<ITeamService, TeamService>();
    }

    protected override void UseServices(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<ITeamService>();
        _userService = serviceProvider.GetRequiredService<IUserService>();
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that deleting a missing team is silently ignored.")]
    public void DeletingMissingTeamIsSilentlyIgnored()
    {
        var team1 = new TeamModel { Name = "Team1", Description = "Test Team" };
        CreateTeam(team1);

        _repo.Delete(17);

        Assert.AreEqual("Team1", _repo.GetAllTeams().Single().Name);
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that a team can be deleted successfully.")]
    public void TeamCanBeDeleted()
    {
        var team1 = new TeamModel { Name = "Team1", Description = "Test Team" };
        CreateTeam(team1);
        var team2 = new TeamModel { Name = "Team2", Description = "Test Team" };
        CreateTeam(team2);

        _repo.Delete(team1.Id);

        Assert.AreEqual("Team2", _repo.GetAllTeams().Single().Name);
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that a team can be updated to include a user.")]
    public void TeamCanBeUpdatedToIncludeAUser()
    {
        var team1 = new TeamModel { Name = "Team1", Description = "Test Team" };
        CreateTeam(team1);

        var newUser = AddUserFred();

        _repo.UpdateUserTeams(newUser.Id, new List<string> { "Team1" });

        Assert.AreEqual(1, _repo.GetTeamsForUser(newUser.Id).Count);
        CollectionAssert.AreEqual(new[] { newUser.Id }, _repo.GetTeam(team1.Id).Members.Select(member => member.Id).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that a team can not be updated to change its name.")]
    public void TeamCanNotBeUpdatedToChangeName()
    {
        const string teamName = "Team1";
        var teamModel = new TeamModel { Name = teamName, Description = "Test Team" };
        CreateTeam(teamModel);

        teamModel.Name = "SonOfTeam1";
        _repo.Update(teamModel);

        Assert.AreEqual(teamName, _repo.GetAllTeams().Single().Name);
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that multiple teams cannot have the same name.")]
    public void TestMultipleTeamsCannotHaveSameTeamName()
    {
        var createResult1 = CreateTeam(new TeamModel { Name = "Team1" });
        var createResult2 = CreateTeam(new TeamModel { Name = "Team1" });

        Assert.IsTrue(createResult1);
        Assert.IsFalse(createResult2);
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that adding duplicate members is silently ignored.")]
    public void DuplicateMemberIsSilentlyIgnored()
    {
        var newMember = AddUserFred();
        var createResult = CreateTeam(new TeamModel
        {
            Name = "Team1",
            Members = [newMember, newMember]
        });

        Assert.IsTrue(createResult);
        Assert.AreEqual(1, _repo.GetAllTeams().Single().Members.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that a new repository is created successfully.")]
    public void TestRepositoryIsCreated()
    {
        Assert.IsNotNull(_repo);
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that a new repository is empty initially.")]
    public void TestNewRepositoryIsEmpty()
    {
        Assert.AreEqual(0, _repo.GetAllTeams().Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that a new team can be added with no members.")]
    public void TestNewTeamCanBeAddedWithNoMembers()
    {
        var createResult = CreateTeam(new TeamModel { Name = "Team1", Description = "Test Team" });

        Assert.IsTrue(createResult);
        var addedTeam = _repo.GetAllTeams().Single();
        Assert.AreEqual("Team1", addedTeam.Name);
        Assert.AreEqual("Test Team", addedTeam.Description);
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that a new team can be retrieved by its ID.")]
    public void TestNewTeamCanBeRetrievedById()
    {
        CreateTeam(new TeamModel { Name = "Team1", Description = "Test Team" });
        var addedTeamId = _repo.GetAllTeams().Single().Id;
        var addedTeam = _repo.GetTeam(addedTeamId);
        Assert.AreEqual("Team1", addedTeam.Name);
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that checking for unique team by name is case-insensitive.")]
    public void TestIsTeamNameUniqueIsCaseInsensitive()
    {
        var createResult = CreateTeam(new TeamModel { Name = "Team1" });
        Assert.IsTrue(createResult);
        Assert.IsFalse(_repo.IsTeamNameUnique("Team1"));
        Assert.IsFalse(_repo.IsTeamNameUnique("team1"));
        Assert.IsFalse(_repo.IsTeamNameUnique("TEAM1"));
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that team can be renamed to itself.")]
    public void TestGetTeamByNameIsCaseInsensitive()
    {
        TeamModel teamModel = new TeamModel { Name = "Team1" };
        var createResult = CreateTeam(teamModel);
        Assert.IsTrue(createResult);
        Assert.IsTrue(_repo.IsTeamNameUnique("Team1", teamModel.Id));
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that multiple teams can have different names.")]
    public void TestMultipleTeamsCanHaveDifferentTeamNames()
    {
        var createResult1 = CreateTeam(new TeamModel { Name = "Team1" });
        var createResult2 = CreateTeam(new TeamModel { Name = "Team2" });

        Assert.IsTrue(createResult1);
        Assert.IsTrue(createResult2);
        Assert.AreEqual(2, _repo.GetAllTeams().Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that a new team can be added with a member.")]
    public void TestNewTeamCanBeAddedWithAMember()
    {
        var newMember = AddUserFred();
        var createResult = CreateTeam(new TeamModel
        {
            Name = "Team1",
            Members = [newMember]
        });

        Assert.IsTrue(createResult);
        var addedTeam = _repo.GetAllTeams().Single();
        Assert.AreEqual("Team1", addedTeam.Name);
        CollectionAssert.AreEqual(new[] { "fred" }, addedTeam.Members.Select(user => user.Username).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.TeamRepository)]
    [Description("Verify that a new user is not a team member initially.")]
    public void NewUserIsNotATeamMember()
    {
        var newMember = AddUserFred();
        Assert.AreEqual(0, _repo.GetTeamsForUser(newMember.Id).Count);
    }

    private UserModel AddUserFred()
    {
        var entity = new User
        {
            Id = 2,
            Username = "fred",
            GivenName = "Fred",
            Surname = "FredBlogs",
            Email = "fred@aol",
            Password = "password",
            PasswordSalt = "salt",
        };
        DbContext.Users.Add(entity);
        DbContext.SaveChanges();
        var user = entity.ToModel();
        _userService.GetUserModel("fred").Returns(user);
        return user;
    }

    protected bool CreateTeam(TeamModel team)
    {
        return _repo.Create(team);
    }
}
