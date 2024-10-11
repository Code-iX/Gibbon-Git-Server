using System.ComponentModel.DataAnnotations;
using Gibbon.Git.Server.Helpers;

namespace Gibbon.Git.Server.Models;

public class ServerOverviewModel
{
    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_DotNetVersion")]
    public string DotNetVersion { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_IsDemoActive")]
    public bool IsDemoActive { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_ApplicationPath")]
    public string ApplicationPath { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_DataPath")]
    public string DataPath { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_RepositoryPath")]
    public string RepositoryPath { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_GitPath")]
    public string GitPath { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_GitHomePath")]
    public string GitHomePath { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_ApplicationVersion")]
    public string ApplicationVersion { get; set; } = VersionHelper.GetAssemblyVersion();
}
