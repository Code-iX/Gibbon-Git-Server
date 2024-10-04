using System;

using Gibbon.Git.Server.Security;

namespace Gibbon.Git.Server.Tests.Unit;

[TestClass]
public class PasswordServiceTest
{
    private const string DefaultAdminPassword = "admin";
    private PasswordService _passwordService = null!;

    [TestInitialize]
    public void Setup()
    {
        _passwordService = new PasswordService();
    }

    [TestMethod]
    public void SaltGenerationShouldCreateUniqueSalts()
    {
        var salt1 = _passwordService.GenerateSalt();
        var salt2 = _passwordService.GenerateSalt();

        Assert.AreNotEqual(salt1, salt2, "Generated salts should be unique.");
    }

    [TestMethod]
    public void PasswordHashingShouldBeConsistent()
    {
        var salt = _passwordService.GenerateSalt();
        var hash1 = _passwordService.GenerateHash(salt, DefaultAdminPassword);
        var hash2 = _passwordService.GenerateHash(salt, DefaultAdminPassword);

        Assert.AreEqual(hash1, hash2, "Hashing the same password with the same salt should return the same hash.");
    }

    [TestMethod]
    public void PasswordHashingShouldBeDifferentForDifferentSalts()
    {
        var salt1 = _passwordService.GenerateSalt();
        var salt2 = _passwordService.GenerateSalt();
        var hash1 = _passwordService.GenerateHash(salt1, DefaultAdminPassword);
        var hash2 = _passwordService.GenerateHash(salt2, DefaultAdminPassword);

        Assert.AreNotEqual(hash1, hash2, "Hashing the same password with different salts should return different hashes.");
    }

    [TestMethod]
    public void CorrectPasswordShouldValidate()
    {
        var salt = _passwordService.GenerateSalt();
        var hash = _passwordService.GenerateHash(salt, DefaultAdminPassword);

        var isCorrect = _passwordService.CompareHash(salt, DefaultAdminPassword, hash);
        Assert.IsTrue(isCorrect, "The correct password should validate successfully.");
    }

    [TestMethod]
    public void IncorrectPasswordShouldNotValidate()
    {
        var salt = _passwordService.GenerateSalt();
        var hash = _passwordService.GenerateHash(salt, DefaultAdminPassword);

        var isCorrect = _passwordService.CompareHash(salt, "wrongpassword", hash);
        Assert.IsFalse(isCorrect, "An incorrect password should not validate.");
    }

    [TestMethod]
    public void ConsistentPasswordHashesWithSameSalt()
    {
        var salt = _passwordService.GenerateSalt();
        var hash1 = _passwordService.GenerateHash(salt, DefaultAdminPassword);
        var hash2 = _passwordService.GenerateHash(salt, DefaultAdminPassword);

        Assert.AreEqual(hash1, hash2, "The same password should always produce the same hash with the same salt.");
    }

    [TestMethod]
    public void DifferentPasswordsShouldProduceDifferentHashes()
    {
        var salt = _passwordService.GenerateSalt();
        var hash1 = _passwordService.GenerateHash(salt, DefaultAdminPassword);
        var hash2 = _passwordService.GenerateHash(salt, "differentpassword");

        Assert.AreNotEqual(hash1, hash2, "Different passwords should produce different hashes, even with the same salt.");
    }

    [TestMethod]
    public void SaltedHashesShouldNotBeEqualToPlaintextPassword()
    {
        var salt = _passwordService.GenerateSalt();
        var hash = _passwordService.GenerateHash(salt, DefaultAdminPassword);

        Assert.AreNotEqual(DefaultAdminPassword, hash, "The salted hash should not be equal to the plaintext password.");
    }

    [TestMethod]
    public void GenerateToken_ShouldReturnNonEmptyString()
    {
        string input = "testuser";

        var result = _passwordService.GenerateToken(input);

        Assert.IsFalse(string.IsNullOrEmpty(result));
    }

    [TestMethod]
    public void GenerateToken_ShouldGenerateUniqueTokensForDifferentInputs()
    {
        string input1 = "testuser1";
        string input2 = "testuser2";

        var token1 = _passwordService.GenerateToken(input1);
        var token2 = _passwordService.GenerateToken(input2);

        Assert.AreNotEqual(token1, token2);
    }

    [TestMethod]
    public void GenerateToken_ShouldGenerateDifferentTokensEachTimeForSameInput()
    {
        string input = "testuser";

        var token1 = _passwordService.GenerateToken(input);
        var token2 = _passwordService.GenerateToken(input);

        Assert.AreNotEqual(token1, token2);
    }

    [TestMethod]
    public void GenerateToken_ShouldHaveExpectedLength()
    {
        string input = "testuser";

        var result = _passwordService.GenerateToken(input);

        var tokenBytes = Convert.FromBase64String(result);

        Assert.AreEqual(1 + 128 / 8 + 256 / 8, tokenBytes.Length);
    }
}
