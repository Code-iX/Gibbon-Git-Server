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
}
