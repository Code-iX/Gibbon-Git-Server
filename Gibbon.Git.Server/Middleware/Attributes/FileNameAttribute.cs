using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Middleware.Attributes;

public class FileNameAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return true;
        }

        if (value is string fileName)
        {
            return fileName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
        }

        return false;
    }
}
