using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class ServerDatabaseModel
{
    [Display(ResourceType = typeof(Resources), Name = "ServerDatabaseModel_DatabasePath")]
    public string DatabasePath { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerDatabaseModel_DatabaseSize")]
    public long DatabaseSize { get; set; }    
}
