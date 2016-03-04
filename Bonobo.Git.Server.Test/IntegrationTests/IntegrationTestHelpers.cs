﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpecsFor.Mvc;

using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace Bonobo.Git.Server
{
    public static class UserExtensions
    {

        // http://stackoverflow.com/questions/915745/thoughts-on-foreach-with-enumerable-range-vs-traditional-for-loop
        public static IEnumerable<int> To(this int from, int to)
        {
            if (from < to)
            {
                while (from <= to)
                {
                    yield return from++;
                }
            }
            else
            {
                while (from >= to)
                {
                    yield return from--;
                }
            }
        }

        public static IEnumerable<T> Step<T>(this IEnumerable<T> source, int step)
        {
            if (step == 0)
            {
                throw new ArgumentOutOfRangeException("step", "Param cannot be zero.");
            }

            return source.Where((x, i) => (i % step) == 0);
        }
    }
}


namespace Bonobo.Git.Server.Test.IntegrationTests.Helpers
{
    static class IntegrationTestHelpers
    {
        public static Guid CreateRepositoryOnWebInterface(MvcWebApp app, string name)
        {
            app.NavigateTo<RepositoryController>(c => c.Create());
            app.FindFormFor<RepositoryDetailModel>()
                .Field(f => f.Name).SetValueTo(name)
                .Submit();

            // ensure it appears on the listing
            app.NavigateTo<RepositoryController>(c => c.Index(null, null));

            bool has_name = false;
            var repo_links = app.Browser.FindElementsByCssSelector("table.repositories a.RepositoryName");
            IWebElement element = null;
            foreach (var item in repo_links)
            {
                if (item.Text == name)
                {
                    element = item;
                    has_name = true;
                }
            }
            Assert.AreEqual(true, has_name, string.Format("Repository {0} not found in Index after creation!", name));
            return new Guid(element.GetAttribute("id").Substring(5));
        }


        public static IEnumerable<Guid> CreateUsers(MvcWebApp app, int count = 1, int start = 0)
        {
            var guids = new List<Guid>();
            foreach (int i in start.To(start + count - 1))
            {
                var index = i.ToString();
                app.NavigateTo<AccountController>(c => c.Create());
                app.FindFormFor<UserCreateModel>()
                    .Field(f => f.Username).SetValueTo("TestUser" + index)
                    .Field(f => f.Name).SetValueTo("Uname" + index)
                    .Field(f => f.Surname).SetValueTo("Surname" + index)
                    .Field(f => f.Email).SetValueTo("mail" + index + "@domain.com")
                    .Field(f => f.Password).SetValueTo("aaa")
                    .Field(f => f.ConfirmPassword).SetValueTo("aaa")
                    .Submit();
                var item = app.Browser.FindElementByXPath("//div[@class='summary-success']/p");
                string id = item.GetAttribute("id");
                guids.Add(new Guid(id));
            }
            return guids;
        }

        public static void DeleteRepository(MvcWebApp app, Guid guid)
        {
            app.NavigateTo<RepositoryController>(c => c.Delete(guid));
            app.FindFormFor<RepositoryDetailModel>().Submit();

            // make sure it no longer is listed
            bool has_repo = false;
            var repo_links = app.Browser.FindElementsByCssSelector("table.repositories a.RepositoryName");
            foreach (var item in repo_links)
            {
                if (item.GetAttribute("id") == "repo_" + guid.ToString())
                {
                    has_repo = true;
                }
            }
            Assert.AreEqual(false, has_repo, string.Format("Repository {0} still in Index after deleting!", guid));
        }

    }
}
