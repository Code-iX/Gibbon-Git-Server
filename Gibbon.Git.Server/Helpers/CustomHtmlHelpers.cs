using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gibbon.Git.Server.Helpers;

public static class CustomHtmlHelpers
{
    public static IHtmlContent MarkdownToHtml(this IHtmlHelper helper, IUrlHelper urlHelper, string markdownText)
    {
        if (string.IsNullOrEmpty(markdownText))
        {
            return HtmlString.Empty;
        }

        var values = new RouteValueDictionary(helper.ViewContext.RouteData.Values);

        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
                                              
        var document = Markdown.Parse(markdownText, pipeline);

        foreach (var link in document.Descendants<LinkInline>())
        {
            if (!link.IsImage)
                continue;

            var originalUrl = link.Url?.TrimStart('.', '/');

            if (string.IsNullOrWhiteSpace(originalUrl))
                continue;

            if (Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
                continue;

            values["path"] = originalUrl;

            link.Url = urlHelper.Action("Raw", "Repositories", values);
        }

        var html = document.ToHtml(pipeline);

        return new HtmlString(html);
    }
}
