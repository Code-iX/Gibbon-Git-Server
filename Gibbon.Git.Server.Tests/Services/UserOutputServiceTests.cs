using Gibbon.Git.Server.Services;

namespace Gibbon.Git.Server.Tests.Services;

[TestClass]
public class UserOutputServiceTests
{
    private IUserOutputService _userOutputService = null!;

    [TestInitialize]
    public void Init()
    {
        _userOutputService = new UserOutputService();
    }

    [DataTestMethod]
    [DataRow(0, "0 B")]
    [DataRow(2, "2 B")]
    [DataRow(512, "512 B")]
    [DataRow(100, "100 B")]
    [DataRow(2000, "1,95 kB")]
    [DataRow(1024, "1 kB")]
    [DataRow(1500, "1,46 kB")]
    [DataRow(2000000, "1,91 MB")]
    [DataRow(1048576, "1 MB")]
    [DataRow(2000000000, "1,86 GB")]
    [DataRow(1073741824, "1 GB")]
    [DataRow(2000000000000, "1,82 TB")]
    [DataRow(2000000000000000, "1819 TB")]
    [DataRow(-2000000, "1,91 MB")]
    [DataRow(999, "999 B")]
    [DataRow(10240, "10 kB")]
    [DataRow(1023999, "1000 kB")]
    [DataRow(1099511627776, "1 TB")]
    public void TestGetFileSizeString(long size, string expected)
    {
        Assert.AreEqual(expected, _userOutputService.GetFileSizeString(size));
    }
}
