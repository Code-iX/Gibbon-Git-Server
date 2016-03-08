﻿using System;
using System.Linq;
using SpecsFor.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Test.Integration.Web
{
    using ITH = Bonobo.Git.Server.Test.IntegrationTests.Helpers.IntegrationTestHelpers;
    public class AccountControllerSpecs
    {
        [TestClass]
        public class AccountControllerTests : IntegrationTestBase
        {

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void UserDetailRequiresLogin()
            {
                app.NavigateTo<HomeController>(c => c.LogOff());
                app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Home/Index"));

                app.NavigateTo<AccountController>(c => c.Detail(new Guid("7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f")));

                app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Account/Detail/7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f"));
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void LoginWithoutCredentialFailsWithInvalidMessages()
            {
                app.NavigateTo<HomeController>(c => c.LogOn("/"));
                app.FindFormFor<LogOnModel>()
                    .Field(f => f.Username).SetValueTo("")
                    .Field(f => f.Password).SetValueTo("")
                    .Submit();

                app.FindFormFor<LogOnModel>()
                    .Field(f => f.Username).ShouldBeInvalid();
                app.FindFormFor<LogOnModel>()
                    .Field(f => f.Password).ShouldBeInvalid();
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void UsernameEnsureDuplicationDetectionAsYouTypeWorksOnCreation()
            {
                var id1 = ITH.CreateUsers().Single();
                app.NavigateTo<AccountController>(c => c.Create());
                var form = app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Username).SetValueTo(id1.Username)
                    .Field(f => f.Name).Click(); // Set focus

                var input = app.Browser.FindElementByCssSelector("input#Username");
                Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void UsernameEnsureDuplicationDetectionAsYouTypeWorksOnEdit()
            {
                var ids = ITH.CreateUsers(2).ToList();
                var id1 = ids[0];
                var id2 = ids[1];
                app.NavigateTo<AccountController>(c => c.Edit(id2.Id));
                var form = app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Username).SetValueTo(id1.Username)
                    .Field(f => f.Name).Click(); // Set focus


                var input = app.Browser.FindElementByCssSelector("input#Username");
                Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void UsernameEnsureDuplicationDetectionStillAllowsEditOtherProperties()
            {
                var ids = ITH.CreateUsers().ToList();
                var id1 = ids[0];
                app.NavigateTo<AccountController>(c => c.Edit(id1.Id));
                app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Name).SetValueTo("somename")
                    .Submit();

                app.NavigateTo<AccountController>(c => c.Edit(id1.Id)); // force refresh
                app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Name).ValueShouldEqual("somename");
            }
        }
    }
}
