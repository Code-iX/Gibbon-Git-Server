using System;
using System.Threading.Tasks;
using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Gibbon.Git.Server.Tests.Services;

[TestClass]
[TestCategory("ServerSettingsService")]
public class ServerSettingsServiceTests
{
    private GibbonGitServerContext _context = null!;
    private IMemoryCache _memoryCache = null!;
    private ServerSettingsService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<SqliteGibbonContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new SqliteGibbonContext(options);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new ServerSettingsService(_memoryCache, _context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _memoryCache.Dispose();
    }

    [TestMethod]
    [Description("Test that default settings include Detail as the default repository view")]
    public void GetDefaultSettings_ReturnsDetailAsDefaultView()
    {
        // Act
        var settings = _service.GetDefaultSettings();

        // Assert
        Assert.AreEqual(RepositoryDefaultView.Detail, settings.DefaultRepositoryView);
    }

    [TestMethod]
    [Description("Test that GetSettings returns default settings when no settings exist in database")]
    public void GetSettings_NoSettingsInDb_ReturnsDefaults()
    {
        // Act
        var settings = _service.GetSettings();

        // Assert
        Assert.IsNotNull(settings);
        Assert.AreEqual(RepositoryDefaultView.Detail, settings.DefaultRepositoryView);
        Assert.AreEqual(true, settings.AllowUserRepositoryCreation);
        Assert.AreEqual("en", settings.DefaultLanguage);
    }

    [TestMethod]
    [Description("Test that SaveSettings persists DefaultRepositoryView to database")]
    public async Task SaveSettings_SavesDefaultRepositoryView()
    {
        // Arrange
        var settings = new ServerSettings
        {
            DefaultLanguage = "en",
            AllowUserRepositoryCreation = true,
            DefaultRepositoryView = RepositoryDefaultView.Tree
        };

        // Act
        await _service.SaveSettings(settings);

        // Assert
        var savedEntity = await _context.ServerSettings.FirstOrDefaultAsync();
        Assert.IsNotNull(savedEntity);
        Assert.AreEqual(RepositoryDefaultView.Tree, savedEntity.DefaultRepositoryView);
    }

    [TestMethod]
    [Description("Test that GetSettings retrieves DefaultRepositoryView from database")]
    public async Task GetSettings_RetrievesDefaultRepositoryView()
    {
        // Arrange
        var entity = new ServerSettingsEntity
        {
            DefaultLanguage = "en",
            AllowUserRepositoryCreation = true,
            DefaultRepositoryView = RepositoryDefaultView.Commits
        };
        _context.ServerSettings.Add(entity);
        await _context.SaveChangesAsync();

        // Clear cache to force database read
        _memoryCache.Remove("ServerSettings");

        // Act
        var settings = _service.GetSettings();

        // Assert
        Assert.AreEqual(RepositoryDefaultView.Commits, settings.DefaultRepositoryView);
    }

    [TestMethod]
    [Description("Test that SaveSettings updates existing settings")]
    public async Task SaveSettings_UpdatesExistingSettings()
    {
        // Arrange
        var initialSettings = new ServerSettings
        {
            DefaultLanguage = "en",
            AllowUserRepositoryCreation = true,
            DefaultRepositoryView = RepositoryDefaultView.Detail
        };
        await _service.SaveSettings(initialSettings);

        var updatedSettings = new ServerSettings
        {
            DefaultLanguage = "en",
            AllowUserRepositoryCreation = true,
            DefaultRepositoryView = RepositoryDefaultView.Tags
        };

        // Act
        await _service.SaveSettings(updatedSettings);

        // Assert
        var entities = await _context.ServerSettings.ToListAsync();
        Assert.AreEqual(1, entities.Count, "Should only have one settings record");
        Assert.AreEqual(RepositoryDefaultView.Tags, entities[0].DefaultRepositoryView);
    }

    [TestMethod]
    [Description("Test that settings are cached after save")]
    public async Task SaveSettings_UpdatesCache()
    {
        // Arrange
        var entity = new ServerSettingsEntity
        {
            DefaultLanguage = "en",
            AllowUserRepositoryCreation = true,
            DefaultRepositoryView = RepositoryDefaultView.Tree
        };
        _context.ServerSettings.Add(entity);
        await _context.SaveChangesAsync();

        // Act - Save settings should update cache
        var settings = new ServerSettings
        {
            DefaultLanguage = "en",
            AllowUserRepositoryCreation = true,
            DefaultRepositoryView = RepositoryDefaultView.Commits
        };
        await _service.SaveSettings(settings);
        
        // Try to retrieve from cache
        var cached = _memoryCache.Get<ServerSettings>("ServerSettings");

        // Assert
        Assert.IsNotNull(cached, "Settings should be cached");
        Assert.AreEqual(RepositoryDefaultView.Commits, cached.DefaultRepositoryView);
    }
}
