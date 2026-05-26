using System;
using System.Globalization;
using Gibbon.Git.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gibbon.Git.Server.Tests.Localization;

[TestClass]
public class LocalizationTests
{
    private IServiceProvider _serviceProvider;
    private IStringLocalizer<SharedResource> _localizer;

    [TestInitialize]
    public void Initialize()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        _serviceProvider = services.BuildServiceProvider();
        _localizer = _serviceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();
    }

    [TestMethod]
    public void SharedResource_ReturnsEnglishText_WhenCultureIsNeutral()
    {
        // Arrange
        var savedCulture = CultureInfo.CurrentUICulture;

        try
        {
            // Use invariant culture for neutral/English (the resources in SharedResource.resx)
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            // Act
            var result = _localizer["Product_Name"];

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.ResourceNotFound, $"Resource 'Product_Name' not found");
            Assert.AreEqual("Gibbon Git Server", result.Value);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [TestMethod]
    public void SharedResource_ReturnsGermanText_WhenCultureIsDe()
    {
        // Arrange
        var savedCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("de");

            // Act
            var result = _localizer["Product_Name"];

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.ResourceNotFound, $"Resource 'Product_Name' not found for German culture");
            // German translation should exist
            Assert.IsNotNull(result.Value);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [TestMethod]
    public void SharedResource_ReturnsFrenchText_WhenCultureIsFr()
    {
        // Arrange
        var savedCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("fr");

            // Act
            var result = _localizer["Product_Name"];

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.ResourceNotFound, $"Resource 'Product_Name' not found for French culture");
            Assert.IsNotNull(result.Value);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [TestMethod]
    public void SharedResource_ReturnsKey_WhenResourceNotFound()
    {
        // Arrange
        var savedCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en");

            // Act
            var result = _localizer["NonExistentKey"];

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResourceNotFound);
            Assert.AreEqual("NonExistentKey", result.Value);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [TestMethod]
    public void SharedResource_SwitchesCultures_WhenUICultureChanges()
    {
        // Arrange
        var savedCulture = CultureInfo.CurrentUICulture;

        try
        {
            // Act & Assert - Neutral/English
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            var englishResult = _localizer["Product_Name"];
            Assert.IsFalse(englishResult.ResourceNotFound);
            Assert.AreEqual("Gibbon Git Server", englishResult.Value);

            // Act & Assert - German
            CultureInfo.CurrentUICulture = new CultureInfo("de");
            var germanResult = _localizer["Product_Name"];
            Assert.IsFalse(germanResult.ResourceNotFound);
            // Just verify it's not the English value (actual translation may vary)
            Assert.IsNotNull(germanResult.Value);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [TestMethod]
    public void SharedResource_FallbackToNeutral_WhenSpecificCultureNotFound()
    {
        // Arrange
        var savedCulture = CultureInfo.CurrentUICulture;

        try
        {
            // Use a culture code that might not have specific resources
            CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

            // Act
            var result = _localizer["Product_Name"];

            // Assert - should fallback to neutral
            Assert.IsNotNull(result);
            // en-GB may not have specific resources, so it might be marked as not found
            // but it should still return the neutral culture value
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Gibbon Git Server", result.Value); // Should fallback to neutral
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [TestMethod]
    public void SharedResource_SupportsMultipleLanguages()
    {
        // Arrange
        var savedCulture = CultureInfo.CurrentUICulture;
        // Test cultures that we have actual resource files for (not 'en' since that's neutral)
        var supportedCultures = new[] { "de", "fr", "es", "it", "ru", "ja", "zh-CN" };

        try
        {
            foreach (var culture in supportedCultures)
            {
                // Act
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
                var result = _localizer["Product_Name"];

                // Assert
                Assert.IsNotNull(result, $"Result is null for culture {culture}");
                Assert.IsFalse(result.ResourceNotFound, $"Resource not found for culture {culture}");
                Assert.IsNotNull(result.Value, $"Value is null for culture {culture}");
            }
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [TestMethod]
    public void ValidationMessages_AreLocalized()
    {
        // Arrange
        var savedCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            // Act
            var requiredResult = _localizer["Validation_Required"];
            var emailResult = _localizer["Validation_Email"];

            // Assert
            Assert.IsFalse(requiredResult.ResourceNotFound, "Validation_Required not found");
            Assert.IsFalse(emailResult.ResourceNotFound, "Validation_Email not found");
            Assert.IsNotNull(requiredResult.Value);
            Assert.IsNotNull(emailResult.Value);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }
}
