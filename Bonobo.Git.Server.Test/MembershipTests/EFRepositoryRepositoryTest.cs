﻿using System;
using System.Linq;
using System.Text;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    [TestClass]
    public class EFSqliteRepositoryRepositoryTest : EFRepositoryRepositoryTest
    {
        SqliteTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqliteTestConnection();
            _repo = EFRepositoryRepository.FromCreator(() => _connection.GetContext());
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        protected override BonoboGitServerContext MakeContext()
        {
            return _connection.GetContext();
        }
    }

    [TestClass]
    public class EfSqlServerRepositoryRepositoryTest : EFRepositoryRepositoryTest
    {
        SqlServerTestConnection _connection;

        [TestInitialize]
        public void Initialize()
        {
            _connection = new SqlServerTestConnection();
            _repo = EFRepositoryRepository.FromCreator(() => _connection.GetContext());
            new AutomaticUpdater().RunWithContext(_connection.GetContext());
        }

        protected override BonoboGitServerContext MakeContext()
        {
            return _connection.GetContext();
        }
    }

    public abstract class EFRepositoryRepositoryTest
    {
        protected IRepositoryRepository _repo;
        protected abstract BonoboGitServerContext MakeContext();

        [TestMethod]
        public void NewRepoIsEmpty()
        {
            Assert.AreEqual(0, _repo.GetAllRepositories().Count);
        }

        [TestMethod]
        public void RespositoryWithNoUsersCanBeAdded()
        {
            var newRepo = MakeRepo("Repo1");

            _repo.Create(newRepo);

            Assert.AreEqual("Repo1", _repo.GetAllRepositories().Single().Name);
        }

        [TestMethod]
        public void DuplicateRepoAddReturnsFalse()
        {
            Assert.IsTrue(_repo.Create(MakeRepo("Repo1")));
            Assert.IsFalse(_repo.Create(MakeRepo("Repo1")));
        }

        [TestMethod]
        public void RespositoryWithUsersCanBeAdded()
        {
            var newRepo = MakeRepo("Repo1");
            newRepo.Users = new [] { AddUserFred() };

            _repo.Create(newRepo);

            Assert.AreEqual("Fred Blogs", _repo.GetAllRepositories().Single().Users.Single().DisplayName);
        }

        [TestMethod]
        public void RespositoryWithAdministratorCanBeAdded()
        {
            var newRepo = MakeRepo("Repo1");
            newRepo.Administrators = new[] { AddUserFred() };

            _repo.Create(newRepo);

            Assert.AreEqual("Fred Blogs", _repo.GetAllRepositories().Single().Administrators.Single().DisplayName);
        }

        [TestMethod]
        public void RespositoriesAdministeredAreFound()
        {
            var administator = AddUserFred();

            var newRepo1 = MakeRepo("Repo1");
            newRepo1.Administrators = new[] { administator };
            _repo.Create(newRepo1);

            var newRepo2 = MakeRepo("Repo2");
            _repo.Create(newRepo2);

            // Only one repo is administered by our user
            Assert.AreEqual("Repo1", _repo.GetAdministratedRepositories(administator.Id).Single().Name);
        }

        [TestMethod]
        public void NewRepoCanBeRetrievedById()
        {
            var newRepo1 = MakeRepo("Repo1");
            _repo.Create(newRepo1);

            Assert.AreEqual("Repo1", _repo.GetRepository(newRepo1.Id).Name);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NonExistentRepoIdThrowsException()
        {
            var newRepo1 = MakeRepo("Repo1");
            _repo.Create(newRepo1);

            _repo.GetRepository(Guid.NewGuid());
        }

        [TestMethod]
        public void NonExistentRepoNameReturnsNull()
        {
            var newRepo1 = MakeRepo("Repo1");
            _repo.Create(newRepo1);

            Assert.IsNull(_repo.GetRepository("Repo2"));
        }

        [TestMethod]
        public void NewRepoCanBeRetrievedByName()
        {
            var newRepo1 = MakeRepo("Repo1");
            _repo.Create(newRepo1);

            Assert.AreEqual("Repo1", _repo.GetRepository("Repo1").Name);
        }

        [TestMethod]
        public void NewRepoCanBeDeleted()
        {
            _repo.Create(MakeRepo("Repo1"));
            _repo.Create(MakeRepo("Repo2"));

            _repo.Delete(_repo.GetRepository("Repo1").Id);

            Assert.AreEqual("Repo2", _repo.GetAllRepositories().Single().Name);
        }

        [TestMethod]
        public void DeletingMissingRepoIsSilentlyIgnored()
        {
            _repo.Create(MakeRepo("Repo1"));

            _repo.Delete(Guid.NewGuid());

            Assert.AreEqual("Repo1", _repo.GetAllRepositories().Single().Name);
        }

        [TestMethod]
        public void RepoSimplePropertiesAreSavedOnUpdate()
        {
            var repo = MakeRepo("Repo1");
            _repo.Create(repo);

            repo.Name = "SonOfRepo";
            repo.Group = "RepoGroup";
            repo.AnonymousAccess = true;
            repo.AuditPushUser = true;
            repo.Description = "New desc";

            _repo.Update(repo);

            var readBackRepo = _repo.GetRepository("SonOfRepo");
            Assert.AreEqual("SonOfRepo", readBackRepo.Name);
            Assert.AreEqual(repo.Group, readBackRepo.Group);
            Assert.AreEqual(repo.AnonymousAccess, readBackRepo.AnonymousAccess);
            Assert.AreEqual(repo.AuditPushUser, readBackRepo.AuditPushUser);
            Assert.AreEqual(repo.Description, readBackRepo.Description);
        }

        [TestMethod]
        public void RepoLogoCanBeAddedWithUpdate()
        {
            var repo = MakeRepo("Repo1");
            _repo.Create(repo);

            var logoBytes = Encoding.UTF8.GetBytes("Hello");
            repo.Logo = logoBytes;

            _repo.Update(repo);

            var readBackRepo = _repo.GetRepository("Repo1");
            CollectionAssert.AreEqual(logoBytes, readBackRepo.Logo);
        }

        [TestMethod]
        public void RepoLogoCanBeRemovedWithUpdate()
        {
            var repo = MakeRepo("Repo1");
            _repo.Create(repo);

            repo.Logo = Encoding.UTF8.GetBytes("Hello");
            _repo.Update(repo);
            repo.RemoveLogo = true;
            _repo.Update(repo);

            Assert.IsNull(_repo.GetRepository("Repo1").Logo);
        }

        [TestMethod]
        public void NewRepositoryIsPermittedToNobody()
        {
            _repo.Create(MakeRepo("Repo1"));

            Assert.IsFalse(_repo.GetPermittedRepositories(null, null).Any());
        }

        [TestMethod]
        public void AnonymousRepoIsPermittedToEverybody()
        {
            var repo = MakeRepo("Repo1");
            repo.AnonymousAccess = true;
            _repo.Create(repo);

            Assert.AreEqual("Repo1", _repo.GetPermittedRepositories(null, null).Single().Name);
        }

        [TestMethod]
        public void RepositoryIsPermittedToUser()
        {
            var repoWithUser = MakeRepo("Repo1");
            var user = AddUserFred();
            repoWithUser.Users = new[] { user };
            _repo.Create(repoWithUser);
            var repoWithoutUser = MakeRepo("Repo2");
            _repo.Create(repoWithoutUser);

            Assert.AreEqual("Repo1", _repo.GetPermittedRepositories(user.Id, null).Single().Name);
        }

        [TestMethod]
        public void NewRepositoryNotPermittedToUnknownUser()
        {
            var repoWithUser = MakeRepo("Repo1");
            var user = AddUserFred();
            repoWithUser.Users = new[] { user };
            _repo.Create(repoWithUser);

            Assert.IsFalse(_repo.GetPermittedRepositories(Guid.NewGuid(), null).Any());
        }

        [TestMethod]
        public void RepositoryIsPermittedToAdministrator()
        {
            var repoWithAdmin = MakeRepo("Repo1");
            var user = AddUserFred();
            repoWithAdmin.Administrators = new[] { user };
            _repo.Create(repoWithAdmin);
            var repoWithoutUser = MakeRepo("Repo2");
            _repo.Create(repoWithoutUser);

            Assert.AreEqual("Repo1", _repo.GetPermittedRepositories(user.Id, null).Single().Name);
        }

        [TestMethod]
        public void RepositoryIsPermittedToTeam()
        {
            var team = AddTeam();
            var repoWithTeam = MakeRepo("Repo1");
            repoWithTeam.Teams = new []{ team };
            _repo.Create(repoWithTeam);
            var repoWithoutTeam = MakeRepo("Repo2");
            _repo.Create(repoWithoutTeam);

            Assert.AreEqual("Repo1", _repo.GetPermittedRepositories(null, new[] { team.Id }).Single().Name);
        }

        [TestMethod]
        public void RepositoryIsNotPermittedIfTeamIsWrong()
        {
            var team = AddTeam();
            var repoWithTeam = MakeRepo("Repo1");
            repoWithTeam.Teams = new[] { team };
            _repo.Create(repoWithTeam);
            var repoWithoutTeam = MakeRepo("Repo2");
            _repo.Create(repoWithoutTeam);

            Assert.AreEqual(0, _repo.GetPermittedRepositories(null, new[] { Guid.NewGuid() }).Count);
        }


        UserModel AddUserFred()
        {
            EFMembershipService memberService = new EFMembershipService(MakeContext);
            memberService.CreateUser("fred", "letmein", "Fred", "Blogs", "fred@aol", null);
            return memberService.GetUserModel("fred");
        }

        TeamModel AddTeam()
        {
            EFTeamRepository teams = EFTeamRepository.FromCreator(MakeContext);
            var newTeam = new TeamModel { Name="Team1" };
            teams.Create(newTeam);
            return newTeam;
        }

        private static RepositoryModel MakeRepo(string name)
        {
            var newRepo = new RepositoryModel();
            newRepo.Name = name;
            newRepo.Users = new UserModel[0];
            newRepo.Administrators = new UserModel[0];
            newRepo.Teams = new TeamModel[0];
            return newRepo;
        }

    }
}