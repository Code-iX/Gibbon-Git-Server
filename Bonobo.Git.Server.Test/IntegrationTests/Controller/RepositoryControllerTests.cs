﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using SpecsFor.Mvc;

using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Bonobo.Git.Server.Test.IntegrationTests;

namespace Bonobo.Git.Server.Test.IntegrationTests.Controller
{
    using ITH = IntegrationTestHelpers;
    using OpenQA.Selenium.Support.UI; 

    [TestClass]
    public class RepositoryControllerTests
    {
        private static MvcWebApp app;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            app = new MvcWebApp();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            app.Browser.Close();
        }

        [TestInitialize]
        public void InitTests()
        {
            ITH.Login(app);
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void EnsureCheckboxesStayCheckOnCreateError()
        {
            ITH.CreateUsers(app, 1);
            app.NavigateTo<RepositoryController>(c => c.Create());
            var form = app.FindFormFor<RepositoryDetailModel>();
            var chkboxes = form.WebApp.Browser.FindElementsByCssSelector("form.pure-form>fieldset>div.pure-control-group.checkboxlist>input");
            foreach (var chk in chkboxes)
            {
                if (!chk.Selected)
                {
                    chk.Click();
                }
            }
            form.Submit();


            form = app.FindFormFor<RepositoryDetailModel>();
            chkboxes = form.WebApp.Browser.FindElementsByCssSelector("form.pure-form>fieldset>div.pure-control-group.checkboxlist>input");
            foreach (var chk in chkboxes)
            {
                Assert.AreEqual(true, chk.Selected, "A message box was unselected eventhough we selected all!");
            }
            
        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void CreateDuplicateRepoNameDifferentCaseNotAllowed()
        {
            var reponame = "A_Nice_Repo";
            var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame.ToUpper())
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();

            ITH.DeleteRepository(app, id1);

        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void CreateDuplicateRepoNameNotAllowed()
        {
            var reponame = "A_Nice_Repo";
            var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame);

            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();

            ITH.DeleteRepository(app, id1);

        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowed()
        {
            var reponame = "A_Nice_Repo";
            var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface(app, "other_name");

            app.NavigateTo<RepositoryController>(c => c.Edit(id2));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame)
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();

            ITH.DeleteRepository(app, id1);
            ITH.DeleteRepository(app, id2);

        }

        [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
        public void RenameRepoToExistingRepoNameNotAllowedDifferentCase()
        {
            var reponame = "A_Nice_Repo";
            var id1 = ITH.CreateRepositoryOnWebInterface(app, reponame);
            var id2 = ITH.CreateRepositoryOnWebInterface(app, "other_name");

            app.NavigateTo<RepositoryController>(c => c.Edit(id2));
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(reponame.ToUpper())
                .Submit();

            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).ShouldBeInvalid();

            ITH.DeleteRepository(app, id1);
            ITH.DeleteRepository(app, id2);

        }

/*
        [TestMethod]
        public void RepositoryCanBeSavedBySysAdminWithoutHavingAnyRepoAdmins()
        {
            var repoId = ITH.CreateRepositoryOnWebInterface(app, "Repo");

            app.NavigateTo<RepositoryController>(c => c.Edit(repoId));
            // TODO - need to find all the admin checkboxes here and turn them off
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.PostedSelectedAdministrators).SetValueTo("0")
                .Submit();

        }
*/

    }
}

