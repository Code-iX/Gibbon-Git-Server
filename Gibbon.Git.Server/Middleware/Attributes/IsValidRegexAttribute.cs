using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Gibbon.Git.Server.Middleware.Attributes;

public class IsValidRegexAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is string regexPattern)
        {
            try
            {
                _ = new Regex(regexPattern);
                return ValidationResult.Success;
            }
            catch (ArgumentException)
            {
                return new ValidationResult(Resources.Validation_Invalid_Regex);
            }
        }

        return new ValidationResult(Resources.Validation_Invalid_Regex);
    }
}
