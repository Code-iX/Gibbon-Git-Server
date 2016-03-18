﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    public class GitResult
    {
        public string StdErr { get; set; }
        public string StdOut { get; set; }
        public int ExitCode { get; set; }
        public MsysgitResources Resources { get; set; }

        public bool Succeeded
        {
            get { return ExitCode == 0; }
        }

        public bool AccessDenied
        {
            get { return !Succeeded && StdErr.Contains(Resources[MsysgitResources.Definition.AuthenticationFailedError]); }
        }

        public GitResult ExpectSuccess()
        {
            if (!Succeeded)
            {
                Assert.Fail("Git operation failed with exit code {0}, stderr {1}", ExitCode, StdErr);
            }
            return this;
        }

        public GitResult ErrorMustMatch(MsysgitResources.Definition resource, params object[] args)
        {
            var expected = string.Format(Resources[resource], args).Trim();
            var actual = StdErr.Trim();
            Assert.AreEqual(expected, actual, "Git operation StdErr mismatch");
            return this;
        }
    }
}