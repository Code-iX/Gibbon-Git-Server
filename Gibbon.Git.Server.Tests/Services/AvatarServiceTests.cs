using System;
using System.Linq;
using System.Threading.Tasks;

using Gibbon.Git.Server.Services;

namespace Gibbon.Git.Server.Tests.Services;

[TestClass]
[TestCategory(AvatarService)]
public class AvatarServiceTests
{
    private const string AvatarService = "AvatarServiceTests";
    private AvatarService _avatarService = null!;

    [TestInitialize]
    public void Setup()
    {
        _avatarService = new AvatarService();
    }

    [TestMethod]
    [DataRow("  beau@Automattic.com  ")]
    [DataRow("beau@automattic.com")]
    [DataRow("BEAU@AUTOMATTIC.COM")]
    [DataRow("  BEAU@AUTOMATTIC.COM  ")]
    [Description("Tests that the email normalization process (trimming and case-insensitivity) produces the correct SHA256 hash for the email.")]
    public void TestEmailNormalization(string email)
    {
        // The email is from the Gravatar documentation: https://docs.gravatar.com/getting-started//
        // Arrange
        const string expectedHash = "27205e5c51cb03f862138b22bcb5dc20f94a342e744ff6df1b8dc8af3c865109";
        _avatarService.Size = 100;

        // Act
        var avatarUrl = _avatarService.GetAvatar(email);

        // Assert
        Assert.IsNotNull(avatarUrl);
        Assert.IsTrue(avatarUrl.Contains(expectedHash), $"Expected hash: {expectedHash} not found in URL");
        Assert.IsTrue(avatarUrl.EndsWith("?s=100"), "Expected size parameter to be 100");
    }

    [TestMethod]
    [Description("Tests that the avatar URLs change based on the size property.")]
    public void TestAvatarSizeVariation()
    {
        // Arrange
        var email = "beau@automattic.com";

        // Act
        _avatarService.Size = 100;
        var avatarUrl1 = _avatarService.GetAvatar(email);

        _avatarService.Size = 150;
        var avatarUrl2 = _avatarService.GetAvatar(email);

        // Assert
        Assert.IsTrue(avatarUrl1.EndsWith("?s=100"), "Expected size parameter to be 100");
        Assert.IsTrue(avatarUrl2.EndsWith("?s=150"), "Expected size parameter to be 150");
        Assert.AreNotEqual(avatarUrl1, avatarUrl2, "URLs for different sizes should be different");
    }

    [TestMethod]
    [Description("Tests that the avatar URLs are cached and reused when the same email and size are requested.")]
    public void TestAvatarCaching()
    {
        // Arrange
        var email = "beau@automattic.com";

        // Act
        _avatarService.Size = 75;
        var avatarUrl1 = _avatarService.GetAvatar(email);

        var cachedAvatarUrl = _avatarService.GetAvatar(email);

        _avatarService.Size = 150;
        var avatarUrl2 = _avatarService.GetAvatar(email);

        _avatarService.Size = 75;
        var cachedAvatarUrlAgain = _avatarService.GetAvatar(email);

        // Assert
        Assert.AreEqual(avatarUrl1, cachedAvatarUrl, "Cached URL should be returned for the same size and email");
        Assert.AreNotEqual(avatarUrl1, avatarUrl2, "Different size should result in different URL");
        Assert.AreEqual(avatarUrl1, cachedAvatarUrlAgain, "Cached URL should be returned again for size 75");
    }

    [TestMethod]
    [Description("Concurrent calls to GetAvatar must not throw or return inconsistent results.")]
    public void GetAvatar_ConcurrentAccess_DoesNotThrow()
    {
        const int threadCount = 50;
        var emails = Enumerable.Range(0, 10).Select(i => $"user{i}@example.com").ToArray();
        var results = new string[threadCount];
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        Parallel.For(0, threadCount, i =>
        {
            try
            {
                var svc = new AvatarService();
                results[i] = svc.GetAvatar(emails[i % emails.Length]);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.AreEqual(0, exceptions.Count, $"Unexpected exceptions: {string.Join(", ", exceptions.Select(e => e.Message))}");
        Assert.IsTrue(results.All(r => r != null), "All results should be non-null");
    }

    [TestMethod]
    [Description("Concurrent calls for the same email must return the same URL every time.")]
    public void GetAvatar_ConcurrentSameEmail_ReturnsConsistentUrl()
    {
        const string email = "concurrent@test.com";
        const int threadCount = 100;
        var results = new string[threadCount];

        Parallel.For(0, threadCount, i =>
        {
            results[i] = _avatarService.GetAvatar(email);
        });

        var expected = results[0];
        Assert.IsTrue(results.All(r => r == expected), "All concurrent calls must return the same URL");
    }
}
