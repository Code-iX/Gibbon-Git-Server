using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Gibbon.Git.Server.Middleware.Attributes;

/// <summary>
/// Attribute for check uploaded file extension. Class based on ms source
/// <see cref="http://referencesource.microsoft.com/#System.ComponentModel.DataAnnotations/Resources/DataAnnotationsResources.Designer.cs,53c08675134a01ce"/>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class FileUploadExtensionsAttribute : DataTypeAttribute
{
    private string _extensions;

    public FileUploadExtensionsAttribute()
        : base(DataType.Upload)
    {
        ErrorMessage = new FileExtensionsAttribute() { Extensions = Extensions }.ErrorMessage;
    }

    public string Extensions
    {
        // Default file extensions match those from jquery validate.
        get => string.IsNullOrWhiteSpace(_extensions) ? "png,jpg,jpeg,gif" : _extensions;
        set => _extensions = value;
    }

    private string ExtensionsFormatted => string.Join(", ", ExtensionsParsed);

    private string ExtensionsNormalized => Extensions.Replace(" ", "").Replace(".", "").ToLowerInvariant();

    private IEnumerable<string> ExtensionsParsed => ExtensionsNormalized.Split(',').Select(e => "." + e);

    public override string FormatErrorMessage(string name) => string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, ExtensionsFormatted);

    public override bool IsValid(object value)
    {
        if (value == null)
        {
            return true;
        }

        if (value is IFormFile valueAsString)
        {
            return ValidateExtension(valueAsString.FileName);
        }

        return false;
    }

    private bool ValidateExtension(string fileName)
    {
        try
        {
            return ExtensionsParsed.Contains(Path.GetExtension(fileName).ToLowerInvariant());
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}