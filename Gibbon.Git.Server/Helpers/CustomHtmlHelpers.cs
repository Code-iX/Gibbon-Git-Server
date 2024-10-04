using CommonMark;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gibbon.Git.Server.Helpers;

public static class CustomHtmlHelpers
{
    public static IHtmlContent MarkdownToHtml(this IHtmlHelper helper, string markdownText)
    {
        return new HtmlString(CommonMarkConverter.Convert(markdownText));
    }
}