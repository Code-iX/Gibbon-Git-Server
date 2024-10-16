using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Integration.Tests.Controller;

[TestClass]
internal class HomeControllerTests : IntegrationTestBase
{
    [TestMethod]
    public async Task Index_UnauthenticatedUser_ShouldRedirectToLogin()
    {
        using var client = Application.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Index");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsNotNull(response.Headers.Location);
        Assert.IsTrue(response.Headers.Location.ToString().Contains("/Login"), "Expected redirect to login page for unauthenticated users.");
    }

    [TestMethod]
    public async Task Index_AuthenticatedUser_ShouldRedirectToRepositoryIndex()
    {
        using var client = await CreateClientAsync(true, false);

        var response = await client.GetAsync("/Index");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsNotNull(response.Headers.Location);
        Assert.AreEqual("/Repositories", response.Headers.Location.ToString());
    }

    [TestMethod]
    public async Task Error_ShouldReturn_View()
    {
        using var client = Application.CreateClient();

        var response = await client.GetAsync("/Error/404");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(Regex.IsMatch(content, "<h1>\\s*An Error Occurred\\s*</h1>"));
    }

    [TestMethod]
    public async Task Error_404_ShouldReturnNotFoundPage()
    {
        using var client = Application.CreateClient();

        var response = await client.GetAsync("/NonExistentRoute");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("404 Page Not Found"));
    }

    [TestMethod]
    public async Task Login_ShouldReturn_View_WithReturnUrl()
    {
        using var client = Application.CreateClient();
        var returnUrl = "/Repository/Index";

        var response = await client.GetAsync($"/Login?returnUrl={returnUrl}");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.IsTrue(content.Contains(returnUrl));
    }

    [TestMethod]
    public async Task Login_InvalidModelState_ShouldReturnView()
    {
        // Arrange
        const string requestUri = "/Login";
        using var client = Application.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var postData = new Dictionary<string, string>(await GetAntiForgeryToken(client, requestUri))
        {
            { "Username", "" },
            { "Password", "password123" }
        };

        var response = await client.PostAsync(requestUri, new FormUrlEncodedContent(postData));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        Assert.IsTrue(content.Contains("Username is mandatory."));
    }

    [TestMethod]
    public async Task Login_InvalidCredentials_ShouldReturnViewWithError()
    {
        // Arrange
        const string requestUri = "/Login";
        using var client = Application.CreateClient();
        var postData = new Dictionary<string, string>(await GetAntiForgeryToken(client, requestUri))
        {
            { "Username", "invalidUser" },
            { "Password", "wrongPassword" }
        };

        // Act
        var response = await client.PostAsync(requestUri, new FormUrlEncodedContent(postData));
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.IsTrue(content.Contains("The username or password provided is incorrect."));
    }
    [TestMethod]
    public async Task Logout_ShouldSignOutAndRedirectToHome()
    {
        // Arrange
        using var client = await CreateClientAsync(true, false);
        // Act
        var response = await client.GetAsync("/Logout");

        // Assert
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsNotNull(response.Headers.Location);
        Assert.AreEqual("/", response.Headers.Location.ToString());
    }

    [TestMethod]
    public async Task ForgotPassword_ValidUser_ShouldSendResetLink()
    {
        using var client = Application.CreateClient();
        const string username = "admin";
        var postData = new Dictionary<string, string>(await GetAntiForgeryToken(client, "/ForgotPassword"))
        {
            { "Username", username }
        };

        var response = await client.PostAsync("/ForgotPassword", new FormUrlEncodedContent(postData));

        Application.MailService.Received(1).SendForgotPasswordEmail(Arg.Is<UserModel>(u => u.Username == username), Arg.Any<string>());
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task ResetPassword_EmptyToken_ShouldNotSuccess()
    {
        using var client = Application.CreateClient();
        var response = await client.GetAsync("/ResetPassword");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task ResetPassword_InvalidToken_ShouldNotSuccess()
    {
        using var client = Application.CreateClient();
        const string resetToken = "InvalidToken";

        var response = await client.GetAsync($"/ResetPassword?digest={resetToken}");
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ResetPassword_ValidToken_ShouldAllowPasswordReset()
    {
        using var client = Application.CreateClient();
        const string username = "admin";
        string resetToken = null!;

        Application.MailService.SendForgotPasswordEmail(Arg.Any<UserModel>(), Arg.Do<string>(x => resetToken = x));

        var postData = new Dictionary<string, string>(await GetAntiForgeryToken(client, "/ForgotPassword"))
        {
            { "Username", username }
        };

        await client.PostAsync("/ForgotPassword", new FormUrlEncodedContent(postData));

        postData = new Dictionary<string, string>(await GetAntiForgeryToken(client, resetToken))
        {
            { "Username", "validUser" },
            { "Password", "newValidPassword" },
            { "Digest", resetToken }
        };

        var response = await client.PostAsync("/ResetPassword", new FormUrlEncodedContent(postData));
        response.EnsureSuccessStatusCode();
    }
}
