using System.ComponentModel.DataAnnotations;
using Gibbon.Git.Server.Helpers;

namespace Gibbon.Git.Server.Models;

public class ServerOverviewModel
{
    [Display(Name = "ServerOverviewModel_DotNetVersion")]
    public string DotNetVersion { get; set; }

    [Display(Name = "ServerOverviewModel_IsDemoActive")]
    public bool IsDemoActive { get; set; }

    [Display(Name = "ServerOverviewModel_ApplicationPath")]
    public string ApplicationPath { get; set; }

    [Display(Name = "ServerOverviewModel_DataPath")]
    public string DataPath { get; set; }

    [Display(Name = "ServerOverviewModel_RepositoryPath")]
    public string RepositoryPath { get; set; }

    [Display(Name = "ServerOverviewModel_GitPath")]
    public string GitPath { get; set; }

    [Display(Name = "ServerOverviewModel_GitHomePath")]
    public string GitHomePath { get; set; }

    [Display(Name = "ServerOverviewModel_ApplicationVersion")]
    public string ApplicationVersion { get; set; } = VersionHelper.GetAssemblyVersion();
}
