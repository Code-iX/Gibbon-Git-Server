﻿using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Data.Update
{
    public static class UpdateScriptRepository
    {
        /// <summary>
        /// Creates the list of scripts that should be executed on app start. Ordering matters!
        /// </summary>
        public static IEnumerable<IUpdateScript> GetScriptsBySqlProviderName(string sqlProvider)
        {
            switch (sqlProvider)
            {
                case "SQLiteConnection":
                    return new List<IUpdateScript>
                    {
                        new Sqlite.InitialCreateScript(),
                        new UsernamesToLower(),
                        new Sqlite.AddAuditPushUser(),
                        new Sqlite.AddGroup(),
                        new Sqlite.AddRepositoryLogo(),
                        new Sqlite.AddGuidColumn(),
                        new Sqlite.AddRepoPushColumn(),
                        new Sqlite.AddRepoLinksColumn(),
                        new Sqlite.InsertDefaultData()
                    };
                case "SqlConnection":
                    return new List<IUpdateScript>
                    {
                        new SqlServer.InitialCreateScript(),
                        new UsernamesToLower(),
                        new SqlServer.AddAuditPushUser(),
                        new SqlServer.AddGroup(),
                        new SqlServer.AddRepositoryLogo(),
                        new SqlServer.AddGuidColumn(),
                        new SqlServer.AddRepoPushColumn(),
                        new SqlServer.AddRepoLinksColumn(),
                        new SqlServer.InsertDefaultData()
                    };
                default:
                    throw new NotImplementedException($"The provider '{sqlProvider}' is not supported yet");
            }
        }
    }
}
