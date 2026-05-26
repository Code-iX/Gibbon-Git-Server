using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class ServerDatabaseModel
{
    [Display(Name = "ServerDatabaseModel_DatabasePath")]
    public string DatabasePath { get; set; }

    [Display(Name = "ServerDatabaseModel_DatabaseSize")]
    public long DatabaseSize { get; set; }    
}
