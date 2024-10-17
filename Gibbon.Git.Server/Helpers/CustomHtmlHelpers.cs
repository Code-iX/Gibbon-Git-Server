using Markdig;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gibbon.Git.Server.Helpers;

public static class CustomHtmlHelpers
{
    public static IHtmlContent MarkdownToHtml(this IHtmlHelper helper, string markdownText)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .Build();

        var html = Markdown.ToHtml(markdownText, pipeline);
        return new HtmlString(html);
    }
}
