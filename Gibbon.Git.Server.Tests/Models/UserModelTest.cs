using Gibbon.Git.Server.Data.Entities;
using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Tests.Models;

[TestClass]
[TestCategory(TestCategories.UnitTest)]
public sealed class UserModelTest
{
    [TestMethod]
    public void DisplayNameFormation()
    {
        Assert.AreEqual("Smith, John", new UserModel { GivenName = "John", Surname = "Smith" }.DisplayName);
        Assert.AreEqual("John", new UserModel { GivenName = "John", Surname = null }.DisplayName);
        Assert.AreEqual("John", new UserModel { GivenName = "John", Surname = "" }.DisplayName);
        Assert.AreEqual("Smith", new UserModel { GivenName = null, Surname = "Smith" }.DisplayName);
        Assert.AreEqual("Smith", new UserModel { GivenName = "", Surname = "Smith" }.DisplayName);
        Assert.AreEqual("JohnSmith", new UserModel { Username = "JohnSmith" }.DisplayName);
    }

    [TestMethod]
    public void SortNameFormation()
    {
        Assert.AreEqual("SmithJohn", new UserModel { GivenName = "John", Surname = "Smith" }.SortName);
        Assert.AreEqual("John", new UserModel { GivenName = "John", Surname = null }.SortName);
        Assert.AreEqual("John", new UserModel { GivenName = "John", Surname = "" }.SortName);
        Assert.AreEqual("Smith", new UserModel { GivenName = null, Surname = "Smith" }.SortName);
        Assert.AreEqual("Smith", new UserModel { GivenName = "", Surname = "Smith" }.SortName);
        Assert.AreEqual("JohnSmith", new UserModel { Username = "JohnSmith" }.SortName);
    }

    [TestMethod]
    public void DisplayNameFormation_FirstLast()
    {
        var user = new UserModel { GivenName = "John", Surname = "Smith" };
        Assert.AreEqual("John Smith", user.GetDisplayName(NameFormat.FirstLast));
        
        var userOnlyGiven = new UserModel { GivenName = "John", Surname = null };
        Assert.AreEqual("John", userOnlyGiven.GetDisplayName(NameFormat.FirstLast));
        
        var userOnlySurname = new UserModel { GivenName = null, Surname = "Smith" };
        Assert.AreEqual("Smith", userOnlySurname.GetDisplayName(NameFormat.FirstLast));
        
        var userNoNames = new UserModel { Username = "JohnSmith" };
        Assert.AreEqual("JohnSmith", userNoNames.GetDisplayName(NameFormat.FirstLast));
    }

    [TestMethod]
    public void DisplayNameFormation_LastCommaFirst()
    {
        var user = new UserModel { GivenName = "John", Surname = "Smith" };
        Assert.AreEqual("Smith, John", user.GetDisplayName(NameFormat.LastCommaFirst));
        
        var userOnlyGiven = new UserModel { GivenName = "John", Surname = null };
        Assert.AreEqual("John", userOnlyGiven.GetDisplayName(NameFormat.LastCommaFirst));
        
        var userOnlySurname = new UserModel { GivenName = null, Surname = "Smith" };
        Assert.AreEqual("Smith", userOnlySurname.GetDisplayName(NameFormat.LastCommaFirst));
        
        var userNoNames = new UserModel { Username = "JohnSmith" };
        Assert.AreEqual("JohnSmith", userNoNames.GetDisplayName(NameFormat.LastCommaFirst));
    }

    [TestMethod]
    public void DisplayNameFormation_LastFirst()
    {
        var user = new UserModel { GivenName = "John", Surname = "Smith" };
        Assert.AreEqual("Smith John", user.GetDisplayName(NameFormat.LastFirst));
        
        var userOnlyGiven = new UserModel { GivenName = "John", Surname = null };
        Assert.AreEqual("John", userOnlyGiven.GetDisplayName(NameFormat.LastFirst));
        
        var userOnlySurname = new UserModel { GivenName = null, Surname = "Smith" };
        Assert.AreEqual("Smith", userOnlySurname.GetDisplayName(NameFormat.LastFirst));
        
        var userNoNames = new UserModel { Username = "JohnSmith" };
        Assert.AreEqual("JohnSmith", userNoNames.GetDisplayName(NameFormat.LastFirst));
    }

    [TestMethod]
    public void SortNameFormation_FirstLast()
    {
        var user = new UserModel { GivenName = "John", Surname = "Smith" };
        Assert.AreEqual("JohnSmith", user.GetSortName(NameFormat.FirstLast));
        
        var userOnlyGiven = new UserModel { GivenName = "John", Surname = null };
        Assert.AreEqual("John", userOnlyGiven.GetSortName(NameFormat.FirstLast));
        
        var userOnlySurname = new UserModel { GivenName = null, Surname = "Smith" };
        Assert.AreEqual("Smith", userOnlySurname.GetSortName(NameFormat.FirstLast));
        
        var userNoNames = new UserModel { Username = "JohnSmith" };
        Assert.AreEqual("JohnSmith", userNoNames.GetSortName(NameFormat.FirstLast));
    }

    [TestMethod]
    public void SortNameFormation_LastCommaFirst()
    {
        var user = new UserModel { GivenName = "John", Surname = "Smith" };
        Assert.AreEqual("SmithJohn", user.GetSortName(NameFormat.LastCommaFirst));
        
        var userOnlyGiven = new UserModel { GivenName = "John", Surname = null };
        Assert.AreEqual("John", userOnlyGiven.GetSortName(NameFormat.LastCommaFirst));
        
        var userOnlySurname = new UserModel { GivenName = null, Surname = "Smith" };
        Assert.AreEqual("Smith", userOnlySurname.GetSortName(NameFormat.LastCommaFirst));
        
        var userNoNames = new UserModel { Username = "JohnSmith" };
        Assert.AreEqual("JohnSmith", userNoNames.GetSortName(NameFormat.LastCommaFirst));
    }

    [TestMethod]
    public void SortNameFormation_LastFirst()
    {
        var user = new UserModel { GivenName = "John", Surname = "Smith" };
        Assert.AreEqual("SmithJohn", user.GetSortName(NameFormat.LastFirst));
        
        var userOnlyGiven = new UserModel { GivenName = "John", Surname = null };
        Assert.AreEqual("John", userOnlyGiven.GetSortName(NameFormat.LastFirst));
        
        var userOnlySurname = new UserModel { GivenName = null, Surname = "Smith" };
        Assert.AreEqual("Smith", userOnlySurname.GetSortName(NameFormat.LastFirst));
        
        var userNoNames = new UserModel { Username = "JohnSmith" };
        Assert.AreEqual("JohnSmith", userNoNames.GetSortName(NameFormat.LastFirst));
    }
}
