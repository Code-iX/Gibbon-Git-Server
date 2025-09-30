using System;
using System.Linq;
using System.Threading.Tasks;
using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Gibbon.Git.Server.Tests.Services;

[TestClass]
[TestCategory("UserSettingsService")]
public class UserSettingsServiceTests
{
    private GibbonGitServerContext _context = null!;
    private IMemoryCache _memoryCache = null!;
    private UserSettingsService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<SqliteGibbonContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new SqliteGibbonContext(options);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new UserSettingsService(_memoryCache, _context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _memoryCache.Dispose();
    }

    [TestMethod]
    [Description("Test that default user settings have null DefaultRepositoryView (to inherit server default)")]
    public void GetDefaultSettings_ReturnsNullDefaultRepositoryView()
    {
        // Act
        var settings = _service.GetDefaultSettings();

        // Assert
        Assert.IsNull(settings.DefaultRepositoryView);
        Assert.IsNull(settings.PreferredLanguage);
    }

    [TestMethod]
    [Description("Test that GetSettings returns default settings when user has no settings")]
    public async Task GetSettings_NoUserSettings_ReturnsDefaults()
    {
        // Arrange
        const int userId = 1;

        // Act
        var settings = await _service.GetSettings(userId);

        // Assert
        Assert.IsNotNull(settings);
        Assert.IsNull(settings.DefaultRepositoryView);
    }

    [TestMethod]
    [Description("Test that SaveSettings persists DefaultRepositoryView to database")]
    public async Task SaveSettings_SavesDefaultRepositoryView()
    {
        // Arrange
        const int userId = 1;
        var settings = new UserSettings
        {
            PreferredLanguage = null,
            DefaultRepositoryView = RepositoryDefaultView.Tree
        };

        // Act
        await _service.SaveSettings(userId, settings);

        // Assert
        var savedEntity = await _context.UserSettings.FirstOrDefaultAsync(u => u.UserId == userId);
        Assert.IsNotNull(savedEntity);
        Assert.AreEqual(RepositoryDefaultView.Tree, savedEntity.DefaultRepositoryView);
    }

    [TestMethod]
    [Description("Test that SaveSettings can save null DefaultRepositoryView")]
    public async Task SaveSettings_CanSaveNullDefaultRepositoryView()
    {
        // Arrange
        const int userId = 1;
        var settings = new UserSettings
        {
            PreferredLanguage = "en",
            DefaultRepositoryView = null
        };

        // Act
        await _service.SaveSettings(userId, settings);

        // Assert
        var savedEntity = await _context.UserSettings.FirstOrDefaultAsync(u => u.UserId == userId);
        Assert.IsNotNull(savedEntity);
        Assert.IsNull(savedEntity.DefaultRepositoryView);
    }

    [TestMethod]
    [Description("Test that GetSettings retrieves DefaultRepositoryView from database")]
    public async Task GetSettings_RetrievesDefaultRepositoryView()
    {
        // Arrange
        const int userId = 1;
        var entity = new UserSettingsEntity
        {
            UserId = userId,
            PreferredLanguage = "en",
            DefaultRepositoryView = RepositoryDefaultView.Commits
        };
        _context.UserSettings.Add(entity);
        await _context.SaveChangesAsync();

        // Clear cache to force database read
        _memoryCache.Remove($"UserSettings_{userId}");

        // Act
        var settings = await _service.GetSettings(userId);

        // Assert
        Assert.AreEqual(RepositoryDefaultView.Commits, settings.DefaultRepositoryView);
    }

    [TestMethod]
    [Description("Test that SaveSettings updates existing user settings")]
    public async Task SaveSettings_UpdatesExistingSettings()
    {
        // Arrange
        const int userId = 1;
        var initialSettings = new UserSettings
        {
            PreferredLanguage = "en",
            DefaultRepositoryView = RepositoryDefaultView.Detail
        };
        await _service.SaveSettings(userId, initialSettings);

        var updatedSettings = new UserSettings
        {
            PreferredLanguage = "en",
            DefaultRepositoryView = RepositoryDefaultView.Tags
        };

        // Act
        await _service.SaveSettings(userId, updatedSettings);

        // Assert
        var entities = await _context.UserSettings.Where(u => u.UserId == userId).ToListAsync();
        Assert.AreEqual(1, entities.Count, "Should only have one settings record per user");
        Assert.AreEqual(RepositoryDefaultView.Tags, entities[0].DefaultRepositoryView);
    }

    [TestMethod]
    [Description("Test that different users have independent settings")]
    public async Task SaveSettings_DifferentUsersHaveIndependentSettings()
    {
        // Arrange
        const int userId1 = 1;
        const int userId2 = 2;
        
        var settings1 = new UserSettings
        {
            PreferredLanguage = "en",
            DefaultRepositoryView = RepositoryDefaultView.Tree
        };
        
        var settings2 = new UserSettings
        {
            PreferredLanguage = "de",
            DefaultRepositoryView = RepositoryDefaultView.Commits
        };

        // Act
        await _service.SaveSettings(userId1, settings1);
        await _service.SaveSettings(userId2, settings2);

        // Assert
        var retrievedSettings1 = await _service.GetSettings(userId1);
        var retrievedSettings2 = await _service.GetSettings(userId2);

        Assert.AreEqual(RepositoryDefaultView.Tree, retrievedSettings1.DefaultRepositoryView);
        Assert.AreEqual(RepositoryDefaultView.Commits, retrievedSettings2.DefaultRepositoryView);
    }

    [TestMethod]
    [Description("Test that user settings are cached after retrieval")]
    public async Task GetSettings_CachesUserSettings()
    {
        // Arrange
        const int userId = 1;
        var entity = new UserSettingsEntity
        {
            UserId = userId,
            PreferredLanguage = "en",
            DefaultRepositoryView = RepositoryDefaultView.Tree
        };
        _context.UserSettings.Add(entity);
        await _context.SaveChangesAsync();

        // Act - First call should read from DB
        var settings1 = await _service.GetSettings(userId);
        
        // Modify DB directly
        entity.DefaultRepositoryView = RepositoryDefaultView.Commits;
        await _context.SaveChangesAsync();
        
        // Second call should return cached value
        var settings2 = await _service.GetSettings(userId);

        // Assert
        Assert.AreEqual(RepositoryDefaultView.Tree, settings1.DefaultRepositoryView);
        Assert.AreEqual(RepositoryDefaultView.Tree, settings2.DefaultRepositoryView, "Should return cached value");
    }

    [TestMethod]
    [Description("Test that user can change from custom view to server default (null)")]
    public async Task SaveSettings_CanChangeToServerDefault()
    {
        // Arrange
        const int userId = 1;
        var customSettings = new UserSettings
        {
            PreferredLanguage = "en",
            DefaultRepositoryView = RepositoryDefaultView.Tree
        };
        await _service.SaveSettings(userId, customSettings);

        var defaultSettings = new UserSettings
        {
            PreferredLanguage = "en",
            DefaultRepositoryView = null // Use server default
        };

        // Act
        await _service.SaveSettings(userId, defaultSettings);

        // Assert
        var retrievedSettings = await _service.GetSettings(userId);
        Assert.IsNull(retrievedSettings.DefaultRepositoryView);
    }
}
