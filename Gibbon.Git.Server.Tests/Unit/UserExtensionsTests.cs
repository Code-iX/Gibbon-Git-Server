using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Gibbon.Git.Server.Extensions;

namespace Gibbon.Git.Server.Tests.Unit;

[TestClass]
public class UserExtensionsTests
{
    const string domainslashusername = @"domain.alsodomain\username";
    const string usernameatdomain = "username@domain.alsodomain";

    [TestMethod]
    public void GetDomainFromDomainSlashUsername()
    {
        Assert.AreEqual("domain.alsodomain", domainslashusername.GetDomain());
    }

    [TestMethod]
    public void StripDomainFromDomainSlashUsername()
    {
        Assert.AreEqual("username", domainslashusername.StripDomain());
    }

    [TestMethod]
    public void GetDomainFromUsernameAtDomain()
    {
        Assert.AreEqual("domain.alsodomain", usernameatdomain.GetDomain());
    }

    [TestMethod]
    public void StripDomainFromUsernameAtDomain()
    {
        Assert.AreEqual("username", usernameatdomain.StripDomain());
    }

    [TestMethod]
    public void GetGuidFromNameIdentityClaimWhenGuidStringEncoded() 
    {
        var testGuid = 1;
        var user = MakeUserWithClaims(new Claim(ClaimTypes.NameIdentifier, testGuid.ToString()));
        Assert.AreEqual(testGuid, user.Id());
    }

    [TestMethod]
    public void GuidIsEmptyForUserWithNoNameIdentifier()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        Assert.AreEqual(0, user.Id());
    }

    [TestMethod]
    public void GuidIsEmptyForUserWithUnparsableNameIdentifier()
    {
        var user = MakeUserWithClaims(new Claim(ClaimTypes.NameIdentifier, "NotAGuid"));
        Assert.AreEqual(0, user.Id());
    }

    [TestMethod]
    public void UsernameIsInNameClaim()
    {
        var user = MakeUserWithClaims(new Claim(ClaimTypes.Name, "JoeBloggs"));
        Assert.AreEqual("JoeBloggs", user.Username());
    }

    [TestMethod]
    public void UsernameFallsbackToUpn()
    {
        var user = MakeUserWithClaims(new Claim(ClaimTypes.Upn, "JoeBloggs@local"));
        Assert.AreEqual("JoeBloggs@local", user.Username());
    }

    [TestMethod]
    public void UsernameFallsbackToUpnOnlyIfNameIsMissing()
    {
        var user = MakeUserWithClaims(new Claim(ClaimTypes.Upn, "JoeBloggs@local"), new Claim(ClaimTypes.Name, "JoeBloggs"));
        Assert.AreEqual("JoeBloggs", user.Username());
    }

    [TestMethod]
    public void EscapeStringlistAsInFaq()
    {
        Assert.AreEqual(@"Editors\\ Architects,Programmers\,Testers", new List<string>{@"Editors\ Architects", "Programmers,Testers"}.StringlistToEscapedStringForEnvVar());
    }
    
    [TestMethod]
    public void EscapeStringlistReturnsEmptyStringforEmptyLists()
    {
        Assert.AreEqual("", new List<string> { "" }.StringlistToEscapedStringForEnvVar());
        Assert.AreEqual("", new List<string>().StringlistToEscapedStringForEnvVar());
        Assert.AreEqual("", Enumerable.Empty<string>().StringlistToEscapedStringForEnvVar());
    }

    [TestMethod]
    public void EscapeStringlistWithCustomSeparatorMultiChar()
    {
        Assert.AreEqual(@"Editors\\ Architects<>Programmers\<>Testers", new List<string>{@"Editors\ Architects", "Programmers<>Testers"}.StringlistToEscapedStringForEnvVar("<>"));
    }

    [TestMethod]
    public void EscapeStringlistWithCustomSeparatorSingleChar()
    {
        Assert.AreEqual(@"Editors\\ Architects|Programmers\|Testers", new List<string>{@"Editors\ Architects", "Programmers|Testers"}.StringlistToEscapedStringForEnvVar("|"));
    }

    private static ClaimsPrincipal MakeUserWithClaims(params Claim[] claims)
    {
        var id = new ClaimsIdentity();
        foreach (var claim in claims)
        {
            id.AddClaim(claim);
        }
        return new ClaimsPrincipal(id);
    }
}
