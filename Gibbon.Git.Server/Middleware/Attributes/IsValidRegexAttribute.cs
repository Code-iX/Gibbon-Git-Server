using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Gibbon.Git.Server.Middleware.Attributes;

public class IsValidRegexAttribute : ValidationAttribute
{
    public IsValidRegexAttribute() : base("Validation_Invalid_Regex")
    {
    }

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
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
        }

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }
}
