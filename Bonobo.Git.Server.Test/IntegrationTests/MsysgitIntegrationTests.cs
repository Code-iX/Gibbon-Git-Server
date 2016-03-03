﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SpecsFor.Mvc;
using OpenQA.Selenium;

using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using System.Text;
using System.Threading;
using Bonobo.Git.Server.Test.IntegrationTests;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using OpenQA.Selenium.Support.UI;

namespace Bonobo.Git.Server.Test.Integration.ClAndWeb
{
    using ITH = IntegrationTestHelpers;
    /// <summary>
    /// This is a regression test for msysgit clients. It can be run against installed version of Bonobo Git Server.
    /// </summary>
    [TestClass]
    public class MsysgitIntegrationTests
    {
        private const string RepositoryName = "Integration";
        private static string WorkingDirectory = @"..\..\..\Tests\IntegrationTests";
        private static string GitPath = @"..\..\..\Gits\{0}\bin\git.exe";
        private static string RepositoryDirectory = Path.Combine(WorkingDirectory, RepositoryName);
        private readonly static string ServerRepositoryPath = Path.Combine(@"..\..\..\Bonobo.Git.Server\App_Data\Repositories", RepositoryName);
        private readonly static string ServerRepositoryBackupPath = Path.Combine(@"..\..\..\Tests\", RepositoryName, "Backup");
        private readonly static string[] GitVersions = { "1.7.4", "1.7.6", "1.7.7.1", "1.7.8", "1.7.9", "1.8.0", "1.8.1.2", "1.8.3", "1.9.5", "2.6.1" };
        private readonly static string Credentials = "admin:admin@";
        private readonly static string RepositoryUrlTemplate = "http://{0}localhost:20000/{2}{1}";
        private readonly static string RepositoryUrlWithoutCredentials = String.Format(RepositoryUrlTemplate, String.Empty, String.Empty, RepositoryName);
        private readonly static string RepositoryUrlWithCredentials = String.Format(RepositoryUrlTemplate, Credentials, ".git", RepositoryName);
        private readonly static string Url = string.Format(RepositoryUrlTemplate, string.Empty, string.Empty, string.Empty);
        private readonly static string BareUrl = Url.TrimEnd('/');

        private static List<Tuple<string, MsysgitResources>> installedgits = new List<Tuple<string, MsysgitResources>>();
        // Horrible hack while we talk about the best way to avoid this
        private static MsysgitResources _currentResources;

        private static MvcWebApp app;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            // Make sure relative paths are frozen in case the app's CurrentDir changes
            WorkingDirectory = Path.GetFullPath(WorkingDirectory);
            GitPath = Path.GetFullPath(GitPath);
            RepositoryDirectory = Path.Combine(WorkingDirectory, RepositoryName);
            
            bool any_git_installed = false;
            List<string> not_found = new List<string>();

            foreach (var version in GitVersions)
            {
                var git = String.Format(GitPath, version);
                if (File.Exists(git))
                {
                    var resource = new MsysgitResources(version);
                    installedgits.Add(Tuple.Create(git, resource));
                    any_git_installed = true;
                }
                else
                {
                    not_found.Add(git);
                }
            }

            if (!any_git_installed)
            {
                Assert.Fail(string.Format("Please ensure that you have at least one git installation in '{0}'.", string.Join("', '", not_found.Select(n => Path.GetFullPath(n)))));
            }

            Directory.CreateDirectory(WorkingDirectory);
            if (AnyCredentialHelperExists(installedgits.Last().Item1))
            {
                /* At the moment there is no reliable way of overriding credential.helper on a global basis.
                 * See the other comments for all the other bugs found so far.
                 * Having a credential helper set makes it impossible to check in non authorized login
                 * after a login with username and password has been done. */
                Assert.Fail("Cannot have any credential.helpers configured for integration tests.");
            }

            app = new MvcWebApp();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            app.Browser.Close();
        }

        [TestInitialize]
        public void Initialize()
        {
            DeleteDirectory(WorkingDirectory);
            IntegrationTestHelpers.Login(app);
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void RunGitTests()
        {

            ForAllGits((git, resource) =>
                {
                    Guid repo_id = IntegrationTestHelpers.CreateRepositoryOnWebInterface(app, RepositoryName);
                    CloneEmptyRepositoryWithCredentials(git, resource);
                    CreateIdentity(git);
                    CreateAndPushFiles(git, resource);
                    PushTag(git, resource);
                    PushBranch(git, resource);

                    DeleteDirectory(RepositoryDirectory);
                    CloneRepository(git, resource);

                    DeleteDirectory(RepositoryDirectory);
                    Directory.CreateDirectory(RepositoryDirectory);
                    InitAndPullRepository(git, resource);
                    PullTag(git, resource);
                    PullBranch(git, resource);
                    IntegrationTestHelpers.DeleteRepository(app, repo_id);
                });
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void AnonRepoClone()
        {
            /* This test can fail if you have any credential.helper setup on your system.
             * This is because it is a multi-value configuration which cannot (yet) be
             * unset on the command line or any other way. I have reported it and
             * maybe a new git version will be able to do it.
             * See http://article.gmane.org/gmane.comp.version-control.git/287538
             * and patch status http://article.gmane.org/gmane.comp.version-control.git/287565
             */
            ForAllGits((git, resource) =>
            {
                Guid repo_id = IntegrationTestHelpers.CreateRepositoryOnWebInterface(app, RepositoryName);
                AllowAnonRepoClone(repo_id, false);
                CloneRepoAnon(git, resource, false);
                AllowAnonRepoClone(repo_id, true);
                CloneRepoAnon(git, resource, true);
                IntegrationTestHelpers.DeleteRepository(app, repo_id);
            });
        }

        private static bool AnyCredentialHelperExists(string git)
        {
            IEnumerable<string> urls = new List<string>
            {
                string.Format(RepositoryUrlTemplate, string.Empty, string.Empty, string.Empty),
                string.Format(RepositoryUrlTemplate, Credentials, string.Empty, string.Empty),
            };

            foreach (var url in urls)
            {
                var exists = RunGit(git, string.Format("config --get-urlmatch credential.helper {0}", url), WorkingDirectory);
                /* Credential.helper is a multi-valued configuration setting. This means it you cannot rely on the value returned by any of the calls
                 * as it might return an empty value, which is a valid(! altough pointless) value where there is a second configured value
                 * in the same config file. But you can rely on the exit code. 0 means it found this key. */
                /* There seems to be a bug in git config --get-urlmatch where it will retun 0 eventhough there was no match.
                 * So we need to check the value. The good part is that if it is not set anywhere we get an empty string. If
                 * it is set somewhere the string contains at least "\r\n".
                 * See the bug report here http://article.gmane.org/gmane.comp.version-control.git/287740 */
                if (exists.ExitCode == 0 && exists.StdOut != "")
                {
                    Console.Write(string.Format("Stdout: {0}", exists.StdOut));
                    Console.Write(string.Format("Stderr: {0}", exists.StdErr));
                    Debug.Write(string.Format("Stdout: {0}", exists.StdOut));
                    Debug.Write(string.Format("Stderr: {0}", exists.StdErr));
                    return true;
                }
            }

            return false;
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void NoDeadlockOnLargeOutput()
        {
            var gitres = installedgits.Last();
            var git = gitres.Item1;
            var resource = gitres.Item2;
            Directory.CreateDirectory(WorkingDirectory);

            try{
                var repo_id = IntegrationTestHelpers.CreateRepositoryOnWebInterface(app, RepositoryName);
                CloneEmptyRepositoryWithCredentials(git, resource);
                CreateIdentity(git);
                CreateAndAddTestFiles(git, 2000);
                IntegrationTestHelpers.DeleteRepository(app, repo_id);
            }
            finally
            {
                DeleteDirectory(WorkingDirectory);
            }
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void AnonPush()
        {

            ForAllGits((git, resource) =>
            {
                var repo_id = IntegrationTestHelpers.CreateRepositoryOnWebInterface(app, RepositoryName);
                AllowAnonRepoClone(repo_id, true);
                CloneRepoAnon(git, resource, true);
                CreateIdentity(git);
                CreateRandomFile(Path.Combine(RepositoryDirectory, "file.txt"), 0);
                RunGitOnRepo(git, "add .");
                RunGitOnRepo(git, "commit -m\"Aw yeah!\"");

                SetAnonPush(false);
                PushFiles(git, resource, false);

                SetAnonPush(true);
                PushFiles(git, resource, true);

                IntegrationTestHelpers.DeleteRepository(app, repo_id);
            });
        }

        /// <summary>
        /// This does an authorized push to a repo that allows anon clone
        /// At the time of writing, this demonstrates an issue which breaks authorised push if the repo allows anon pull
        /// </summary>
        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void NamedPushToAnonRepo()
        {
            ForAllGits((git, resource) =>
            {
                Guid repo_id = ITH.CreateRepositoryOnWebInterface(app, RepositoryName);

                // Clone the repo
                AllowAnonRepoClone(repo_id, true);
                CloneEmptyRepositoryWithCredentials(git, resource);

                CreateIdentity(git);
                // I want to do a push *with* a username
                CreateAndPushFiles(git, resource);

                ITH.DeleteRepository(app, repo_id);
            });
        }
        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void PushToCreateIsNotNormallyAllowed()
        {
            ForAllGits((git, resource) =>
            {
                // Create a repo locally
                Directory.CreateDirectory(RepositoryDirectory);
                InitRepository(git);
                Environment.CurrentDirectory = RepositoryDirectory;
                CreateIdentity(git);
                CreateAndAddFiles(git);

                RunGitOnRepo(git, "push origin master").ErrorMustMatch(MsysgitResources.Definition.RepositoryNotFoundError, RepositoryUrlWithCredentials);
            });
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void PushToCreateIsAllowedIfOptionIsSet()
        {
            ForAllGits((git, resource) =>
            {
                // Create a repo locally
                Directory.CreateDirectory(RepositoryDirectory);
                InitRepository(git);
                Environment.CurrentDirectory = RepositoryDirectory;
                CreateIdentity(git);
                CreateAndAddFiles(git);

                // Enable the push-to-create option
                SetGlobalSetting(x => x.AllowPushToCreate, true);

    //            Thread.Sleep(12000000);

                RunGitOnRepo(git, "push origin master").ExpectSuccess();
            });
        }



        /// <summary>
        /// Helper to run a test for every installed Git instance
        /// </summary>
        /// <param name="action"></param>
        private void ForAllGits(Action<string, MsysgitResources> action)
        {
            foreach (var gitres in installedgits)
            {
                Directory.CreateDirectory(WorkingDirectory);
                try
                {
                    var git = gitres.Item1;
                    var resource = gitres.Item2;
                    _currentResources = resource;
                    action(git, resource);
                }
                finally
                {
                    // Make sure we're not in the working directory when we try to delete it
                    Environment.CurrentDirectory = Path.Combine(WorkingDirectory, "..");
                    DeleteDirectory(WorkingDirectory);
                }
            }
        }

        [TestMethod, TestCategory(TestCategories.ClAndWebIntegrationTest)]
        public void NavigateReposWithDropdown()
        {
            
        }

        private void PushFiles(string git, MsysgitResources resource, bool success)
        {
            var res = RunGitOnRepo(git, "push origin master");
            if (success)
            {
                Assert.AreEqual(string.Format(resource[MsysgitResources.Definition.PushFilesSuccessError], RepositoryUrlWithoutCredentials + ".git"), res.StdErr);
            }
            else
            {
                Assert.AreEqual(string.Format(resource[MsysgitResources.Definition.PushFilesFailError], BareUrl), res.StdErr);
            }
        } 

        private void SetCheckbox<T>(FluentField<T, bool> field, bool select) where T : class
        {
            if (select)
            {
                if (!field.Field.Selected)
                {
                    field.Click();
                }
            }
            else
            {
                if(field.Field.Selected)
                {
                    field.Click();
                }
            }
        }

        private void SetAnonPush(bool allowAnonymousPush)
        {
            SetGlobalSetting(f => f.AllowAnonymousPush, allowAnonymousPush);
        }

        private void CreateAndAddTestFiles(string git, int count)
        {
            foreach (var i in 0.To(count - 1))
            {
                CreateRandomFile(Path.Combine(RepositoryDirectory, "file" + i.ToString()), 0);
            }
            RunGitOnRepo(git, "add .");
            RunGitOnRepo(git, "commit -m \"Commit me!\"");
        }

        private void CloneRepoAnon(string git, MsysgitResources resource, bool success)
        {
            var result = RunGit(git, string.Format("clone {0}.git", RepositoryUrlWithoutCredentials), WorkingDirectory);
            if (success)
            {
                Assert.AreEqual(resource[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.StdOut);
                Assert.AreEqual(resource[MsysgitResources.Definition.CloneEmptyRepositoryError], result.StdErr);
            }
            else
            {
                Assert.AreEqual(resource[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.StdOut);
                Assert.AreEqual(string.Format(resource[MsysgitResources.Definition.CloneRepositoryFailRequiresAuthError], BareUrl), result.StdErr);
            }

        }

        private void AllowAnonRepoClone(Guid repo, bool allow)
        {
            app.NavigateTo<RepositoryController>(c => c.Edit(repo));
            var form = app.FindFormFor<RepositoryDetailModel>();
            var repo_clone = form.Field(f => f.AllowAnonymous);
            SetCheckbox(repo_clone, allow);
            form.Submit();
        }

        private void SetGlobalSetting(Expression<Func<GlobalSettingsModel, bool>> optionExpression, bool value)
        {
            app.NavigateTo<SettingsController>(c => c.Index());
            var form = app.FindFormFor<GlobalSettingsModel>();
            SetCheckbox(form.Field(optionExpression), value);
            form.Submit();
        }

        private void CreateIdentity(string git)
        {
            RunGitOnRepo(git, "config user.name \"McFlono McFloonyloo\"").ExpectSuccess();
            RunGitOnRepo(git, "config user.email \"DontBotherMe@home.never\"").ExpectSuccess();
        }

        private void PullBranch(string git, MsysgitResources resource)
        {
            var result = RunGitOnRepo(git, "pull origin TestBranch");

            Assert.AreEqual("Already up-to-date.\r\n", result.StdOut);
            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PullBranchError], RepositoryUrlWithoutCredentials), result.StdErr);
        }

        private void PullTag(string git, MsysgitResources resource)
        {
            var result = RunGitOnRepo(git, "fetch");

            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PullTagError], RepositoryUrlWithoutCredentials), result.StdErr);
        }

        private void InitAndPullRepository(string git, MsysgitResources resource)
        {

            RunGitOnRepo(git, "init");
            RunGitOnRepo(git, String.Format("remote add origin {0}", RepositoryUrlWithCredentials));
            var result = RunGitOnRepo(git, "pull origin master");

            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PullRepositoryError], RepositoryUrlWithoutCredentials), result.StdErr);
        }


        private void InitRepository(string git)
        {
            RunGitOnRepo(git, "init").ExpectSuccess();
            RunGitOnRepo(git, String.Format("remote add origin {0}", RepositoryUrlWithCredentials)).ExpectSuccess();
        }
		
        private void InitAndPushRepository(string git, MsysgitResources resource)
        {
            RunGitOnRepo(git, "init");
            RunGitOnRepo(git, String.Format("remote add origin {0}", RepositoryUrlWithCredentials));
            var result = RunGitOnRepo(git, "push origin master");

            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PullRepositoryError], RepositoryUrlWithoutCredentials), result.StdErr);
        }

        private void CloneRepository(string git, MsysgitResources resource)
        {
            var result = RunGit(git, String.Format(String.Format("clone {0}", RepositoryUrlWithCredentials), RepositoryName), WorkingDirectory);

            Assert.AreEqual(resource[MsysgitResources.Definition.CloneRepositoryOutput], result.StdOut);
            Assert.AreEqual(resource[MsysgitResources.Definition.CloneRepositoryError], result.StdErr);
        }

        private void PushBranch(string git, MsysgitResources resource)
        {
            RunGitOnRepo(git, "checkout -b \"TestBranch\"");
            var result = RunGitOnRepo(git, "push origin TestBranch");

            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PushBranchError], RepositoryUrlWithCredentials), result.StdErr);
        }

        private void PushTag(string git, MsysgitResources resource)
        {
            RunGitOnRepo(git, "tag -a v1.4 -m \"my version 1.4\"");
            var result = RunGitOnRepo(git, "push --tags origin");
            
            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PushTagError], RepositoryUrlWithCredentials), result.StdErr);
        }

        private void CreateAndPushFiles(string git, MsysgitResources resource)
        {
            CreateAndAddFiles(git);
            var result = RunGitOnRepo(git, "push origin master");

            Assert.AreEqual(String.Format(resource[MsysgitResources.Definition.PushFilesSuccessError], RepositoryUrlWithCredentials), result.StdErr);
        }

        private void CreateAndAddFiles(string git)
        {
            CreateRandomFile(Path.Combine(RepositoryDirectory, "1.dat"), 10);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "2.dat"), 1);
            Directory.CreateDirectory(Path.Combine(RepositoryDirectory, "SubDirectory"));
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "3.dat"), 20);
            CreateRandomFile(Path.Combine(RepositoryDirectory, "Subdirectory", "4.dat"), 15);

            RunGitOnRepo(git, "add .").ExpectSuccess();
            RunGitOnRepo(git, "commit -m \"Test Files Added\"").ExpectSuccess();
        }

        private void CloneEmptyRepositoryWithCredentials(string git, MsysgitResources resource)
        {
            var result = RunGit(git, String.Format(String.Format("clone {0}", RepositoryUrlWithCredentials), RepositoryName), WorkingDirectory);
            
            Assert.AreEqual(resource[MsysgitResources.Definition.CloneEmptyRepositoryOutput], result.StdOut);
            Assert.AreEqual(resource[MsysgitResources.Definition.CloneEmptyRepositoryError], result.StdErr);
        }


        private GitResult RunGitOnRepo(string git, string arguments, int timeout = 30000 /* milliseconds */)
        {
            return RunGit(git, arguments, RepositoryDirectory, timeout);
        }

        private static GitResult RunGit(string git, string arguments, string workingDirectory, int timeout = 30000 /* milliseconds */)
        {
            // When a git version supports overwriting multi-value config files values this should be uncommented to make
            // all tests runnable on systems that have a credential.helper configured.
            // arguments = "-c credential.helper=\"store --file=bonobo.randomstring.credentials.txt\" " + arguments;
            Console.WriteLine("About to run '{0}' with args '{1}' in '{2}'", git, arguments, workingDirectory);
            Debug.WriteLine("About to run '{0}' with args '{1}' in '{2}'", git, arguments, workingDirectory);

            // http://stackoverflow.com/a/7608823/551045 and http://stackoverflow.com/a/22956924/551045
            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = git;
                    process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        var strout = output.ToString();
                        var strerr = error.ToString();

                        Console.WriteLine("Stdout: {0}", output);
                        Console.WriteLine("Stderr: {0}", error);

                        return new GitResult {StdErr = strerr, StdOut = strout, ExitCode = process.ExitCode, Resources = _currentResources };
                    }
                    else
                    {
                        Assert.Fail(string.Format("Runing command '{0} {1}' timed out! Timeout {2} seconds.", git, arguments, timeout));
                        return new GitResult() { StdErr = null, StdOut = null, ExitCode = -1 };
                    }
                }
            }
        }

        private void CreateRandomFile(string fileName, int sizeInMb)
        {
            const int blockSize = 1024 * 8;
            const int blocksPerMb = (1024 * 1024) / blockSize;
            byte[] data = new byte[blockSize];
            Random rng = new Random();
            using (FileStream stream = File.OpenWrite(fileName))
            {
                for (int i = 0; i < sizeInMb * blocksPerMb; i++)
                {
                    rng.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }
        }

        private void DeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            // We have to tolerate intermittent errors during directory deletion, because
            // other parts of Windows sometimes hold locks on files briefly
            // Multiple tries normally fixes it
            for (int attempt = 10; attempt >= 0; attempt--)
            {
                try
                {
                    var directory = new DirectoryInfo(directoryPath) {Attributes = FileAttributes.Normal};
                    foreach (var item in directory.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        item.Attributes = FileAttributes.Normal;
                    }
                    directory.Delete(true);
                    return;
                }
                catch
                {
                    if (attempt == 0)
                    {
                        throw;
                    }
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
