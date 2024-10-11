namespace Gibbon.Git.Server.Integration.Tests.Controller;

[TestClass]
internal class ServerControllerTests : IntegrationTestBase
{
    [TestMethod]
    public async Task EnsureSelectedLanguageIsSaved()
    {
        const string language = "de";
        var client = await CreateClientAsync(true);

        var response = await client.GetAsync("/Server/Settings");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.IsFalse(content.Contains($"selected=\"selected\" value=\"{language}\""));

        var postData = new Dictionary<string, string>(await GetAntiForgeryToken(client, "/Server/Settings"))
        {
            { "DefaultLanguage", language }
        };
        response = await client.PostAsync("/Server/Settings", new FormUrlEncodedContent(postData));
        response.EnsureSuccessStatusCode();

        response = await client.GetAsync("/Server/Settings");
        content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains($"selected=\"selected\" value=\"{language}\""));
    }

    [TestMethod]
    [Ignore]
    public async Task InvalidLinkifyRegexAsYouTypeInSettings()
    {
        var client = await CreateClientAsync(true);

        // Schritt 1: Lade die Settings-Seite
        var response = await client.GetAsync("/Server/Settings");
        response.EnsureSuccessStatusCode();

        // Schritt 2: Ungültigen Regex eingeben und absenden
        var brokenRegex = @"\";
        var postData = new Dictionary<string, string>(await GetAntiForgeryToken(client, "/Server/Settings"))
        {
            { "LinksRegex", brokenRegex }
        };
        response = await client.PostAsync("/Server/Settings", new FormUrlEncodedContent(postData));
        var content = await response.Content.ReadAsStringAsync();

        // Logge den gesamten Inhalt für Debugging-Zwecke
        Console.WriteLine(content);

        // Schritt 3: Überprüfen, dass der Fehler für den ungültigen Regex angezeigt wird
        Assert.IsTrue(content.Contains("Invalid regex pattern"));
        Assert.IsTrue(content.Contains("input-validation-error"));
    }

    [TestMethod]
    public async Task ResetSettings_ShouldResetToDefault()
    {
        var client = await CreateClientAsync(true);

        // Schritt 1: Setze Einstellungen zurück
        var postData = new Dictionary<string, string>(await GetAntiForgeryToken(client, "/Server/Settings"))
        {
            { "DefaultLanguage", "de" },
            { "SiteTitle", "Test" },
            { "SiteLogoUrl", "https://example.com/logo.png" },
            { "SiteCssUrl", "https://example.com/style.css" },
            { "LinksRegex", ".*" },
            { "LinksUrl", "https://example.com" }
        };
        var response = await client.PostAsync("/Server/Settings", new FormUrlEncodedContent(postData));
        response.EnsureSuccessStatusCode();
        response = await client.GetAsync("/Server/Settings");
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("Test"));
        Assert.IsTrue(content.Contains("https://example.com/logo.png"));
        Assert.IsTrue(content.Contains("https://example.com/style.css"));
        Assert.IsTrue(content.Contains(".*"));
        Assert.IsTrue(content.Contains("https://example.com"));

        postData = new Dictionary<string, string>(await GetAntiForgeryToken(client, "/Server/Settings"));

        response = await client.PostAsync("/Server/ResetSettings", new FormUrlEncodedContent(postData));
        response.EnsureSuccessStatusCode();
        response = await client.GetAsync("/Server/Settings");
        content = await response.Content.ReadAsStringAsync();
        Assert.IsFalse(content.Contains("Test"));
        Assert.IsFalse(content.Contains("https://example.com/logo.png"));
        Assert.IsFalse(content.Contains("https://example.com/style.css"));
        Assert.IsFalse(content.Contains(".*"));
        Assert.IsFalse(content.Contains("https://example.com"));
    }

    [TestMethod]
    public async Task DownloadGit_ShouldRedirectToSettings()
    {
        var client = await CreateClientAsync(true, false);

        // Schritt 1: Führe den Download aus
        var response = await client.GetAsync("/Server/DownloadGit");

        await Application.GitDownloadService.Received().EnsureDownloadedAsync();
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsNotNull(response.Headers.Location);
        Assert.AreEqual("/Server/Settings", response.Headers.Location.ToString());
    }

    [TestMethod]
    public async Task UnauthorizedUser_ShouldBeRedirectedToLogin()
    {
        var client = await CreateClientAsync(false, false);

        // Schritt 1: Zugriff ohne Authentifizierung
        var response = await client.GetAsync("/Server/Settings");

        // Schritt 2: Überprüfen, dass der Benutzer zur Login-Seite weitergeleitet wird
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsNotNull(response.Headers.Location);
        Assert.IsTrue(response.Headers.Location.ToString().Contains("/Home/Login"));
    }
}
