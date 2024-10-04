using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Gibbon.Git.Server.Middleware.Attributes;

public class IsValidRegexAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null) // Null-Werte sind zulässig, keine Regex-Validierung nötig
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
                // Gültige Fehlermeldung ohne detaillierte Exception-Nachricht
                return new ValidationResult(Resources.Validation_Invalid_Regex);
            }
        }

        // Falls value kein string ist, ist es ungültig
        return new ValidationResult(Resources.Validation_Invalid_Regex);
    }
}
