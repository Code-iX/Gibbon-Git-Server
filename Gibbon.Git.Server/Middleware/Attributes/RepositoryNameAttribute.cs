using System.ComponentModel.DataAnnotations;
using Gibbon.Git.Server.Helpers;

namespace Gibbon.Git.Server.Middleware.Attributes;

public class RepositoryNameAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is not string fileName || string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        return StringHelper.NameIsValid(fileName);
    }
}
