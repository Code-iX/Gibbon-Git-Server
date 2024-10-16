namespace Gibbon.Git.Server.Integration.Tests.Helper;

public static class IntegrationHelper
{

    internal static async Task<Dictionary<string, string>> GetAntiForgeryToken(HttpClient client, string requestUri)
    {
        const string antiforgery = "_AntiForgery";
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var tokenMatch = Regex.Match(content, $"""<input name="{antiforgery}" type="hidden" value="([^"]+)" />""");
        return tokenMatch.Success ? new Dictionary<string, string> { { antiforgery, tokenMatch.Groups[1].Value } } : throw new InvalidOperationException("Anti-forgery token not found");
    }
}
