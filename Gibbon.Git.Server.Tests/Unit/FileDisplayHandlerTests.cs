using System;
using System.Text;

using Gibbon.Git.Server.Helpers;

namespace Gibbon.Git.Server.Tests.Unit;

[TestClass]
public class FileDisplayHandlerTests
{
    [DataTestMethod]
    [DataRow("test.jpg", true)]
    [DataRow("test.png", true)]
    [DataRow("test.txt", false)]
    [DataRow(null, false)]
    [DataRow("", false)]
    public void IsImage_ShouldReturnCorrectResult(string fileName, bool expected)
    {
        var result = FileDisplayHandler.IsImage(fileName);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow("test.cs", "csharp")]
    [DataRow("test.html", "html")]
    [DataRow("test.unknown", "nohighlight")]
    [DataRow("test", "nohighlight")]
    public void GetBrush_ShouldReturnCorrectBrush(string fileName, string expectedBrush)
    {
        var result = FileDisplayHandler.GetBrush(fileName);
        Assert.AreEqual(expectedBrush, result);
    }

    [TestMethod]
    public void GetText_ShouldReturnCorrectText()
    {
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        var result = FileDisplayHandler.GetText(data, Encoding.UTF8);
        Assert.AreEqual("Hello, World!", result);
    }

    [DataTestMethod]
    [DataRow("Hello, Wörld!", "utf-8", true)]
    [DataRow("Hello, World! 😊", "utf-8", false)] // TODO - why?
    [DataRow("Hello, World!", "us-ascii", true)]
    [DataRow("", "utf-8", false)]
    public void TryGetEncoding_ShouldDetectEncoding(string input, string expectedEncoding, bool expectedSuccess)
    {
        byte[] data = Encoding.UTF8.GetBytes(input);
        bool success = FileDisplayHandler.TryGetEncoding(data, out var encoding);

        Console.WriteLine($"Detected Encoding: {encoding?.WebName ?? "None"}");

        Assert.AreEqual(expectedSuccess, success, "Success expectation mismatch.");
        if (expectedSuccess)
        {
            Assert.AreEqual(expectedEncoding, encoding.WebName, $"Expected encoding: {expectedEncoding}");
        }
    }

    [TestMethod]
    public void TryGetEncoding_ShouldReturnFalseOnUnknownCharset()
    {
        var data = new byte[] { 0xFF, 0xFF, 0xFF };
        var success = FileDisplayHandler.TryGetEncoding(data, out var encoding);
        Assert.IsFalse(success);
        Assert.AreEqual(Encoding.Default, encoding);
    }
}
